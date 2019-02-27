Imports Discord
Imports Discord.Commands

Namespace Modules
    <RequireOwner> 'This precondition attribute ensures that any commands in this class, can only be executed by the bot owner
    Public Class OwnerCommands
        Inherits ModuleBase(Of SocketCommandContext)

        <Command("shutdown")>
        <Summary("Shutdown...")>
        <[Alias]("logout")>
        Public Async Function LogOff(Optional ByVal code As Integer = 0) As Task
            Await Context.Client.SetStatusAsync(UserStatus.Invisible)
            Environment.Exit(code)
        End Function
    End Class
End Namespace