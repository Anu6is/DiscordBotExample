Imports System.Collections.Concurrent
Imports System.IO
Imports Discord
Imports Discord.Audio

Public Class AudioService
    Private ReadOnly ConnectedChannels As ConcurrentDictionary(Of ULong, IAudioClient) = New ConcurrentDictionary(Of ULong, IAudioClient)()

    Public Async Function JoinAudio(guild As IGuild, target As IVoiceChannel) As Task
        Dim client As IAudioClient = Nothing

        If ConnectedChannels.TryGetValue(guild.Id, client) Then Return 'Already connected

        Dim audioClient = Await target.ConnectAsync()

        If ConnectedChannels.TryAdd(guild.Id, audioClient) Then
            Console.WriteLine($"Connected to voice in {guild.Name}")
        End If
    End Function

    Public Async Function LeaveAudio(ByVal guild As IGuild) As Task
        Dim client As IAudioClient = Nothing

        If ConnectedChannels.TryRemove(guild.Id, client) Then
            Await client.StopAsync()
            Console.WriteLine($"Disconnected from voice in {guild.Name}")
        End If
    End Function

    Public Async Function SendAudioAsync(ByVal guild As IGuild, ByVal channel As IMessageChannel, ByVal path As String) As Task
        'NOTE: You need a full path to the file if the value of 'path' is only a filename.

        If Not File.Exists(path) Then path = $".\{path}"
        If Not File.Exists(path) Then Await channel.SendMessageAsync("File does not exist.") : Return

        Dim client As IAudioClient = Nothing

        If ConnectedChannels.TryGetValue(guild.Id, client) Then
            Console.WriteLine($"Starting playback of {path} in {guild.Name}")
            Using ffmpeg = CreateProcess(path)
                Using stream = client.CreatePCMStream(AudioApplication.Music)
                    Try
                        Await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream)
                    Catch e As Exception
                        Console.WriteLine(e.ToString)
                    End Try

                    Await stream.FlushAsync()
                End Using
            End Using
        End If
        End Function

    Private Function CreateProcess(ByVal path As String) As Process
        Return Process.Start(New ProcessStartInfo With {
            .FileName = "ffmpeg.exe",
            .Arguments = $"-hide_banner -loglevel panic -i ""{path}"" -ac 2 -f s16le -ar 48000 pipe:1",
            .UseShellExecute = False,
            .RedirectStandardOutput = True
        })
    End Function
End Class
