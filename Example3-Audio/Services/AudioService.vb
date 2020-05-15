Imports System.Collections.Concurrent
Imports System.IO
Imports System.Threading
Imports Discord
Imports Discord.Audio
Imports Discord.WebSocket

''' <summary>
'''     This is a basic implementation of a bot sending and receiving audio.
''' </summary>
Public Class AudioService
    Private ReadOnly ConnectedChannels As New ConcurrentDictionary(Of ULong, IAudioClient)
    Private ReadOnly RecordingUsers As New ConcurrentDictionary(Of ULong, AudioInStream)
    Private ReadOnly Client As DiscordSocketClient

    Public Sub New(client As DiscordSocketClient)
        Me.Client = client
        AddHandler client.UserVoiceStateUpdated, AddressOf VoiceUserDisconnectedAsync
    End Sub

    ''' <summary>
    '''     <see cref="IVoiceChannel.ConnectAsync()"/> returns the <see cref="IAudioClient"/>
    '''     used for sending audio
    ''' </summary>
    Public Async Function JoinAudioAsync(guild As IGuild, voiceChannel As IVoiceChannel) As Task
        Dim client As IAudioClient = Nothing

        If ConnectedChannels.TryGetValue(guild.Id, client) Then Return 'Already connected

        Dim audioClient = Await voiceChannel.ConnectAsync()

        If ConnectedChannels.TryAdd(guild.Id, audioClient) Then
            Console.WriteLine($"Connected to voice in {guild.Name}")
        End If
    End Function

    ''' <summary>
    '''     To disconnect from a voice channel, call <see cref="IAudioClient.StopAsync()"/>
    '''     Before disconnecting, stop any voice recordings by simply removing the user from <see cref="RecordingUsers"/>
    ''' </summary>
    Public Async Function LeaveAudioAsync(guild As SocketGuild) As Task
        Dim client As IAudioClient = Nothing

        For Each user In guild.CurrentUser.VoiceChannel.Users
            If RecordingUsers.ContainsKey(user.Id) Then RecordingUsers.TryRemove(user.Id, Nothing)
        Next

        If ConnectedChannels.TryRemove(guild.Id, client) Then
            Await client.StopAsync()
            Console.WriteLine($"Disconnected from voice in {guild.Name}")
        End If
    End Function

    Public Async Function SendAudioAsync(guild As IGuild, channel As IMessageChannel, path As String) As Task
        'NOTE: You need a full path to the file if the value of 'path' is only a filename.

        If Not File.Exists(path) Then path = $".\{path}"
        If Not File.Exists(path) Then Await channel.SendMessageAsync("File does not exist.") : Return

        Dim client As IAudioClient = Nothing

        If ConnectedChannels.TryGetValue(guild.Id, client) Then
            Console.WriteLine($"Starting playback of {path} in {guild.Name}")
            Using ffmpeg = CreateOutputProcess(path)
                Using stream = client.CreatePCMStream(AudioApplication.Music)
                    Try
                        Await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream)
                    Catch e As Exception
                        Console.WriteLine(e.ToString)
                    End Try
                    Await stream.FlushAsync()
                End Using
            End Using
            Console.WriteLine($"{path} ended in {guild.Name}")
        End If
    End Function

    Public Async Function RecordAudioAsync(guild As SocketGuild, channel As IMessageChannel, user As SocketGuildUser) As Task
        Dim client As IAudioClient = Nothing

        If RecordingUsers.TryGetValue(user.Id, client) Then Return 'Already recording
        If Not ConnectedChannels.TryGetValue(guild.Id, client) Then Await channel.SendMessageAsync("Run the `join` command") : Return

        'Discord requires that audio be sent to the channel before you can start receiving voice
        'Send short audio notification to establish listening rights
        'This could be done immediately after joining the channel
        Await SendAudioAsync(guild, channel, "./recording_audio.mp3")

        For Each voiceUser In user.VoiceChannel.Users
            If guild.CurrentUser.Id = voiceUser.Id Then Continue For
            Dim tsk = ListenToStreamAsync(voiceUser.Id, voiceUser.AudioStream) 'listen to current users of the channel
        Next

        'StreamCreated is triggered when a user joins a voice channel
        AddHandler client.StreamCreated, AddressOf ListenToStreamAsync 'listen to users joining the channel
        AddHandler client.StreamDestroyed, AddressOf StreamDestroyedAsync
    End Function

    Private Function ListenToStreamAsync(id As ULong, inStream As AudioInStream) As Task
        RecordingUsers.TryAdd(id, inStream)
        Return CaptureAudioAsync(id, inStream)
    End Function

    Private Function StreamDestroyedAsync(id As ULong) As Task
        RecordingUsers.TryRemove(id, Nothing)
        Return Task.CompletedTask
    End Function

    ''' <summary>
    '''     Stop recording a user when they leave the voice channel
    ''' </summary>
    Private Function VoiceUserDisconnectedAsync(user As SocketUser, before As SocketVoiceState, after As SocketVoiceState) As Task
        If RecordingUsers.ContainsKey(user.Id) AndAlso after.VoiceChannel Is Nothing Then
            RecordingUsers.TryRemove(user.Id, Nothing)
        End If
        Return Task.CompletedTask
    End Function

    Public Async Function CaptureAudioAsync(id As ULong, inStream As AudioInStream) As Task
        Dim cts = New CancellationTokenSource()
        Dim path = Client.GetUser(id).ToString & DateTimeOffset.UtcNow.ToUnixTimeMilliseconds

        Console.WriteLine($"Start recording {Client.GetUser(id)}")
        Using ffmpeg = CreateInputProces(path)
            Using stream = ffmpeg.StandardInput.BaseStream
                Try
                    While RecordingUsers.ContainsKey(id) 'If the key (user id) is removed, recording is stopped
                        If inStream.AvailableFrames > 0 Then
                            Dim rtpFrame As RTPFrame = Nothing
                            Dim readFrame = inStream.TryReadFrame(cts.Token, rtpFrame)

                            Await stream.WriteAsync(rtpFrame.Payload, 0, rtpFrame.Payload.Length)
                            Await stream.FlushAsync()
                        End If
                    End While
                Catch ex As Exception
                    Console.WriteLine(ex.ToString)
                End Try
                cts.Dispose()
                Await inStream.DisposeAsync()
                Console.WriteLine($"Stop recording {Client.GetUser(id)}")
            End Using
        End Using
    End Function

    ''' <summary>
    '''     FFMPEG process for playing an audio file
    ''' </summary>
    ''' <param name="path">The filepath for the audio file to be played</param>
    Private Function CreateOutputProcess(path As String) As Process
        Return Process.Start(New ProcessStartInfo With {
            .FileName = "ffmpeg.exe",
            .Arguments = $"-hide_banner -loglevel panic -i ""{path}"" -ac 2 -f s16le -ar 48000 pipe:1",
            .UseShellExecute = False,
            .RedirectStandardOutput = True
        })
    End Function

    ''' <summary>
    '''     FFMPEG process for recording an audio file
    ''' </summary>
    ''' <param name="path">The filepath for storing the recording</param>
    Private Function CreateInputProces(path As String) As Process
        Return Process.Start(New ProcessStartInfo With {
            .FileName = "ffmpeg.exe",
            .Arguments = $"-hide_banner -loglevel panic -ac 2 -f s16le -ar 48000 -i pipe:0 -ar 44100 .\{path}.wav",
            .UseShellExecute = False,
            .RedirectStandardInput = True
        })
    End Function
End Class
