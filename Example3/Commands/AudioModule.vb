Imports Discord
Imports Discord.Commands

Public Class AudioModule
    Inherits ModuleBase(Of SocketCommandContext)

    Private ReadOnly _service As AudioService

    Public Sub New(ByVal service As AudioService)
        _service = service
    End Sub


    ''' <summary>
    '''     The command Run Mode MUST be set to RunMode.Async, otherwise, 
    '''     being connected to a voice channel will block the gateway.
    ''' </summary>
    <Command("join", RunMode:=RunMode.Async)>
    <Summary("Attempts to have the bot join the voice channel the user is connected to")>
    Public Async Function JoinAsync() As Task
        Dim voiceState = TryCast(Context.User, IVoiceState)

        If voiceState.VoiceChannel Is Nothing Then Await ReplyAsync("You must be connected to a voice channel!") : Return

        Await _service.JoinAudioAsync(Context.Guild, voiceState.VoiceChannel)
    End Function

    <Command("leave", RunMode:=RunMode.Async)>
    <Summary("Disconnects the bot from the voice channel")>
    Public Async Function LeaveAsync() As Task
        Await _service.LeaveAudioAsync(Context.Guild)
    End Function

    <Command("play", RunMode:=RunMode.Async)>
    <Summary("Plays a local audio file")>
    Public Async Function PlayAsync(<Remainder> filepath As String) As Task
        Await _service.SendAudioAsync(Context.Guild, Context.Channel, filepath)
    End Function

    <Command("record", RunMode:=RunMode.Async)>
    <Summary("Records the audio stream of each user in the voice channel")>
    Public Async Function RecordAsync() As Task
        Dim voiceState = TryCast(Context.User, IVoiceState)

        If voiceState.VoiceChannel Is Nothing Then Await ReplyAsync("You must be connected to a voice channel!") : Return

        Await _service.RecordAudioAsync(Context.Guild, Context.Channel, Context.User)
    End Function
End Class
