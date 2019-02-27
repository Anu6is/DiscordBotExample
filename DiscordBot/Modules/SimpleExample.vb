Imports Discord.Commands
Imports Discord.WebSocket

Namespace Modules
    Public Class SimpleExample
        Inherits ModuleBase(Of SocketCommandContext)
        'In order to trigger a command, the user must type the prefix (|>) expected by the command handler, followed by the command name or alias. 

        <Command("hi")> 'To execute this command the user needs to enter |>hi
        <[Alias]("hello")> 'alternately, they can use |>hello
        <Summary("Replies to the user that typed the command")>
        Public Function SayAsync() As Task
            Return ReplyAsync($"Hello {Context.User.Mention}")
        End Function

        '|>say text for the bot to repeat
        'To accept user input with the command, the method signature must include parameters to capture the user input
        <Command("say")>
        <Summary("Responds with whatever the user enters after the command trigger")>
        <Remarks("The Remainder attribute is useful for accepting multi-word inputs. Without it, a sentence would have to be wrapped in quotes.")>
        Public Function EchoAsync(<Remainder> ByVal text As String) As Task
            Return ReplyAsync(text)
        End Function

        '|>userinfo @Anu6is
        <Command("userinfo")>
        <[Alias]("whois")>
        <Summary("Displays basic user info")>
        <Remarks("Discord.Net provides various TypeReaders that you can utilize to assist with parsing commands. Example: SocketGuildUser")>
        Public Function UserInfo(Optional ByVal user As SocketGuildUser = Nothing) As Task
            'Context provides access to a host of properties made available from the DiscordSocketClient as the SocketUserMessage
            If user Is Nothing Then user = Context.User 'The user that typed the command

            'An alternate way to send a message. Sending a message simply requires an implementation of ITextChannel
            Return Context.Channel.SendMessageAsync($"{user.Username}#{user.Discriminator} joined **{Context.Guild.Name}** on *{user.JoinedAt.Value.ToString("f")}*")
        End Function
    End Class
End Namespace

