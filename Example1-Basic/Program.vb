Imports System.IO
Imports System.Reflection
Imports System.Threading
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.Extensions.Configuration          'Required for ConfigurationBuilder

Namespace Example_1

    Module Program
        Private _client As DiscordSocketClient
        Private _commands As CommandService

        Sub Main()
            'Call the Start function and wait until it completes (which should be never)
            Start().GetAwaiter().GetResult()
        End Sub

        Async Function Start() As Task
            Dim config = BuildConfig()

            _client = New DiscordSocketClient()
            _commands = New CommandService()

            'Subscribe to desired events
            AddEventHandlers()

            'Load command modules into the command service. Any Class that inherits from ModuleBase is loaded
            'If you are not using an IServiceProvider, 'Nothing' can be passed in place of Services
            Await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), Nothing) 'See Example 2 for using Dependancy Injection

            Await _client.LoginAsync(TokenType.Bot, config("token")) 'A valid token must exist in config.json
            Await _client.StartAsync()

            'Infinite delay; Keeps the console open and the bot connected
            Await Task.Delay(Timeout.Infinite)
        End Function

        Private Sub AddEventHandlers()
            AddHandler _client.Log, AddressOf Logger
            AddHandler _commands.Log, AddressOf Logger 'Commands.CommandExecuted can be used for more detailed command logging (success and errors)
            AddHandler _client.MessageReceived, AddressOf CommandHandler
        End Sub

        Private Async Function CommandHandler(ByVal message As SocketMessage) As Task
            Dim userMessage As SocketUserMessage = TryCast(message, SocketUserMessage)

            'Ignore non user messages
            If userMessage Is Nothing OrElse userMessage.Author.IsBot Then Return

            'Create the command context
            Dim context As New SocketCommandContext(_client, userMessage)

            Dim pos As Integer = 0 'This is used to find the starting position of the command (all text after the command prefix)

            'This is where you set your command prefix.
            'This can be hard coded Or made configurable.
            'HasCharPrefix - checks to see if the message begins with a predefined Character
            'HasStringPrefix - checks to see if the message begins with a predefined String
            'HasMentionPrefix - checks to see if the message begins with a predefined user Mention
            If userMessage.HasStringPrefix("|>", pos) OrElse userMessage.HasMentionPrefix(_client.CurrentUser, pos) Then
                Dim result As IResult = Await _commands.ExecuteAsync(context, pos, Nothing)

                'Send a message if the command failed. Excludes sending said message for unknown commands
                If Not result.IsSuccess AndAlso Not result.Error = CommandError.UnknownCommand Then
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
End Namespace