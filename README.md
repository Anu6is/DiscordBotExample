# Discord.Net (Visual Basic) Example Bot
[![NuGet](https://img.shields.io/nuget/vpre/Discord.Net.svg?maxAge=2592000?style=plastic)](https://www.nuget.org/packages/Discord.Net)  
Visual Basic Example for creating a Discord Bot using [Discord.Net](https://github.com/RogueException/Discord.Net) (beta 2.0.0)

This example guides you through getting your bot online and creating a few simple commands.  
**IMPORTANT!**: Your bot token needs to be placed in config.json in order for the bot to connect successfully.  

## Getting Started 
### Documentation  
Before you start coding a project, I would suggest reviewing the [Guides](https://discord.foxbot.me/latest/guides/getting_started/installing.html) and [Documentation](https://discord.foxbot.me/latest/api/index.html) provided.  
### Initial Visual Studio Project
Once you've created your new project (preferably a .Net Core Console Application) install the following  
Nuget Packages:  
- [Discord.Net](https://www.nuget.org/packages/Discord.Net/) 
- [Microsoft.Extensions.Configuration](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/)  

From there, simply follow the [Program.vb](https://github.com/Anu6is/DiscordBotExample/blob/master/DiscordBot/Program.vb) template, to get your bot connected. Additionally, you can take a look at the [SimpleExample](https://github.com/Anu6is/DiscordBotExample/blob/master/DiscordBot/Modules/SimpleExample.vb) Module, to get started with commands.
