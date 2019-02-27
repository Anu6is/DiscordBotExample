Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.Extensions.Configuration          'Required for ConfigurationBuilder
Imports Microsoft.Extensions.DependencyInjection    'Required for ServiceCollection

Module Program
    Private Client As DiscordSocketClient
    Private Commands As CommandService
    Private Config As IConfiguration
    Private Services As IServiceProvider

    Sub Main()
        'Call the Start function and wait until it completes (which should be never)
        Start().GetAwaiter().GetResult()
    End Sub

    Async Function Start() As Task
        Client = New DiscordSocketClient()
        Commands = New CommandService()
        Config = BuildConfig()

        'Set up your Dependency Injection (DI) Container
        'This is not a requirement and can be ignored if not using DI
        AddServices()

        'Subscribe to desired events
        AddHandlers()

        'Load command modules into the command service
        'If you are not using an IService provider, Nothing can be passed in place of Services
        Await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services)

        Await Client.LoginAsync(TokenType.Bot, Config("token")) 'A valid token must exist in config.json
        Await Client.StartAsync()

        'Infinite delay; Keeps the console open and the bot connected
        Await Task.Delay(Timeout.Infinite)
    End Function

    Private Sub AddServices()
        Dim collection As New ServiceCollection()

        'Basic services
        collection.AddSingleton(Client)
        collection.AddSingleton(Commands)

        'Additional services and dependencies that you may require
        collection.AddSingleton(Config)

        'All additional services should be added above before building
        Services = collection.BuildServiceProvider()
    End Sub

    Private Sub AddHandlers()
        AddHandler Client.Log, AddressOf Logger
        AddHandler Commands.Log, AddressOf Logger 'Commands.CommandExecuted can be used for more detailed command logging (success and errors)
        AddHandler Client.MessageReceived, AddressOf CommandHandler
    End Sub

    Private Async Function CommandHandler(ByVal message As SocketMessage) As Task
        Dim userMessage As SocketUserMessage = TryCast(message, SocketUserMessage)

        'Ignore non user messages 
        If userMessage Is Nothing OrElse userMessage.Author.IsBot Then Return

        Dim pos As Integer = 0

        'Create the command context
        Dim context As New SocketCommandContext(Client, userMessage)

        'This is where you set your command prefix. This can be hard coded or made configurable.
        'HasCharPrefix - checks to see if the message begins with a predefined Character
        'HasStringPrefix - checks to see if the message begins with a predefined String
        'HasMentionPrefix - checks to see if the message begins with a predefined user Mention
        If userMessage.HasStringPrefix("|>", pos) OrElse userMessage.HasMentionPrefix(Client.CurrentUser, pos) Then
            Dim result As IResult = Await Commands.ExecuteAsync(context, pos, Services)

            'Send a message if the command failed. Excludes sending said message for unknown commands
            If Not result.IsSuccess AndAlso Not result.ErrorReason = CommandError.UnknownCommand Then
                Await userMessage.Channel.SendMessageAsync(result.ErrorReason)
            End If

            'If you so desire, you can configure behaviour specific to the error type
            'Remove the lines above and uncomment the line below
            'If Not result.IsSuccess Then
            '    Select Case result.GetType
            '        Case GetType(PreconditionResult)
            '            'Do something
            '        Case GetType(PreconditionGroupResult)
            '            'Do something 
            '        Case GetType(ParseResult)
            '            'Do something
            '        Case GetType(SearchResult)
            '            'Do something
            '        Case GetType(TypeReaderResult)
            '            'Do something
            '        Case Else
            '            'Do something
            '    End Select
            'End If
        End If
    End Function

    Private Function Logger(ByVal message As LogMessage, Optional task As Task = Nothing) As Task
        'Very basic logging
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message} {message.Exception}")
        Return Task.CompletedTask
    End Function

    Private Function BuildConfig() As IConfiguration
        Return New ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory).AddJsonFile("config.json").Build
    End Function
End Module
