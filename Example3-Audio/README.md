## Example 3 - Audio

This example follows the setup of Example 2 and adds BASIC audio functions  
- Connect to a voice channel via the `Join` command
- Play a local audio file using the `Play` command
- Record user voice data with the `Record` command
### Things to note:
- The Bot Token is loaded via a text file. This is one option to avoid hard-coding your token
- Dependency Injection is used, including logging and the command handler as example services.
- Audio service adds basic audio functions. Requires `ffmpeg.exe` to play the sample audio provided.
- The AudioModule includes commands `join`, `leave` and `play`. 
- `libsodium.dll` and `opus.dll` are required for connecting to the audio channel 
