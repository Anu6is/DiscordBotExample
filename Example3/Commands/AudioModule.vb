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
    Public Async Function JoinCmd() As Task
        Dim voiceState = TryCast(Context.User, IVoiceState)

        If voiceState Is Nothing Then Await ReplyAsync("You must be connected to a voice channel!") : Return

        Await _service.JoinAudio(Context.Guild, voiceState.VoiceChannel)
    End Function

    <Command("leave", RunMode:=RunMode.Async)>
    Public Async Function LeaveCmd() As Task
        Await _service.LeaveAudio(Context.Guild)
    End Function

    <Command("play", RunMode:=RunMode.Async)>
    Public Async Function PlayCmd(<Remainder> song As String) As Task
        Await _service.SendAudioAsync(Context.Guild, Context.Channel, song)
    End Function
End Class
