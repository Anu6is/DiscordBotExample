Imports Discord
Imports Discord.Commands

Public Class BasicModule
    Inherits ModuleBase(Of SocketCommandContext)

    'Translated from ~> https://gist.github.com/Quahu/13367ed9252eab24c69aea693481e5aa
    <Command("ping")>
    <[Alias]("latency")>
    <Summary("Shows the websocket connection's latency and time it takes to send a message.")>
    Public Async Function PingAsync() As Task
        'Start a new stopwatch to measure the time it takes for us to send a message
        Dim sw = Stopwatch.StartNew()

        'Send the message and store it for later modification
        Dim message = Await ReplyAsync($"**Websocket latency**: {Context.Client.Latency}ms{vbNewLine}**Response**: ...")

        'Pause the stopwatch
        sw.Stop()

        'Modify the message we sent earlier to display measured time
        Await message.ModifyAsync(Sub(msg) msg.Content = $"**Websocket latency**: {Context.Client.Latency}ms{vbNewLine}**Response**: {sw.Elapsed.TotalMilliseconds}ms")
    End Function

    'Translated from ~> https://gist.github.com/Quahu/7ee7550cd54ef8329af4a733f9052fe4
    <Command("purge")>
    <[Alias]("clean")>
    <Summary("Downloads and removes X messages from the current channel.")>
    <RequireUserPermission(ChannelPermission.ManageMessages)>
    <RequireBotPermission(ChannelPermission.ManageMessages)>
    Public Async Function PurgeAsync(ByVal amount As Integer) As Task
        'Check if the amount provided by the user is positive.
        If amount <= 0 Then Await ReplyAsync("The amount of messages to remove must be positive.") : Return

        'Download X messages starting from Context.Message, which means
        'that it won't delete the message used to invoke this command.
        Dim messages = Await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync()

        'Ensure that the messages aren't older than 14 days,
        'because trying to bulk delete messages older than that will result in a bad request.
        Dim filteredMessages = messages.Where(Function(msg) (DateTimeOffset.UtcNow - msg.Timestamp).TotalDays <= 14)

        'Get the total amount of messages.
        Dim count = filteredMessages.Count()

        'Check if there are any messages to delete.
        If count = 0 Then
            Await ReplyAsync("Nothing to delete.")
        Else
            Await DirectCast(Context.Channel, ITextChannel).DeleteMessagesAsync(filteredMessages)
            Await ReplyAsync($"Done. Removed {count} {If(count > 1, "messages", "message")}.")
        End If
    End Function

End Class
