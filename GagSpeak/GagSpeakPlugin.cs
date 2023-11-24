using System.Reflection;
using Dalamud.Plugin; // Required include for the plugin to work
using Microsoft.Extensions.DependencyInjection;
using OtterGui.Classes;
using OtterGui.Log;
using GagSpeak.UI;
using GagSpeak.Services;
using GagSpeak.Chat;

namespace GagSpeak; // The main namespace for the plugin.

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class GagSpeak : IDalamudPlugin
{
    public string Name => "GagSpeak"; // Define plugin name
    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty; // I have no idea how this line works, look into it further later.
    public static readonly Logger Log = new(); // initialize the logger for our plugin
    public static MessageService Messager { get; private set; } = null!; // initialize the messager service, part of otterGui services.
    private readonly ServiceProvider _services; // initialize our services.

    ////* -- MAIN FUNCTION FOR PLUGIN OPENING -- *///
    public GagSpeak(DalamudPluginInterface pluginInt)
    {
        try
        {
            _services = ServiceHandler.CreateProvider(pluginInt, Log); // Initialize the services in the large Service collection. (see ServiceHandler.cs)
            Messager = _services.GetRequiredService<MessageService>(); // Initialize messager service here

            // _sigscanner can now be passed into services that require it.
            _services.GetRequiredService<GagSpeakWindowManager>(); // Initialize the UI
            _services.GetRequiredService<CommandManager>(); // Initialize the command manager
            _services.GetRequiredService<ChatManager>(); // Initialize the OnChatMessage handler
            _services.GetRequiredService<ChatInputProcessor>(); // Initialize the OnChatMessage handler
            Log.Information($"GagSpeak version{Version} loaded successfully."); // Log the version
        }
        catch
        {
            Dispose();
            throw;
        }
    }
    ////* -- MAIN FUNCTION FOR PLUGIN CLOSING -- *///
    public void Dispose()
        => _services?.Dispose(); // Dispose of all services. (call all of their dispose functions)
}
#pragma warning restore IDE1006

/*
                                 _,..----.._
                               _/  . -.     \_
                              /     \  \      \_
                             /    _  \ _`-.  -. \
                            /  _-'_.  /    \   \ \
                           /  /  /     \-_  \_ /  .
                          /  /  /     / \_\_  '   `.
                         (   `._'    /    \ \_     |
                          \         /      ; |    -.
                          /  /     /       \ |._   |
                         (  / ._.-'         )/ |  ||
                          \`|  ---.  .---. //  ' .'|
                          . \  `-' )  `''  '  /  ' |
                         /  | (   /          // /  '
                         `. |\ \  ._.       // /  /____
                           \|| |\ ____     // '/  /    `-
                            '| \ \`..'  / / .-'  /       \
                             | |  \_  _/ / '( ) |         \
                ___..__      | /    `'  /  `./  \          \
             _-'       `-.   |      /   \   /  / \          .
           _/             `- |  // /   .-  /  /   \         `
          /   _.-           `'.   .-' /     _// /| \_
         /   /        _    )   `./    \ .--'-' / /\_ \       \
        /   /      .-' `-./      |     `-'__.-' /  \\|
       /    |   -\ |      - ._   \  _          '    /'
       |    /  / | |       \  )   -' .-.            \         :
       |   / . | | |   .--.|  /  /  /o |             \        `
       |  / /  | : |   .--.| .  /   \_/               \        \
       / / (   | \ |  `._O'| ! .                       \        .
      // .  `  |  \ \      |.' |                       .        |
      /|  -._  |   \|   )  |   `              /       . \       `
       |     \ |           '  ) \            /        '  .       .
     _/     -._ \  .----. /  /   \._     _.-'        .   \       \
  .-'_-'-,     \ \  `--' /  (     . `---'            '    \       \
 |.-'  _/       \ \     / .-.\  \\ \                /     \        \
 \\   /          ) )-._.'/    `.  \|               |       \  _     )
  \|  /|     _.-'//     /       `-.|               |        -'      |
      |\ \  /    / _.-'/           -.              |        |       |
      |   `-.    \'  .'              \             \        '       '
      \\    `.   |  /                 `.            \      .        '
      /      -  _/                      `.           `.    |        '
      \   _.'  /                          -.          |    |       ,
     / -.     /           _.-               `.        |    |       '
    /    -   _.              `\               -.      `.   |      /
    \ -.   .'                  `._              \      |   !     ,
     |  ._/                       -.             `. .-=\  .'
     |   /._            |           `.             \-'  |.'     /
     |  /,o ;                        |-            _`.--'       ;
     \ .|`.'            |            | `-_      _.'_.          /
     -' |               '            |    `.   (_ .           /
    /   \              /             |      `-_ _' _         /`.
   /     \           .'              |      /(_' _'         .' !
  .       `._     _.'                |     / ( -'_.-'     _.'  |
  (       |  `---'                    \-._'   (._ _.- _.-'      .
  `.      |  \                         \      |: `---'  |       !
    \     |   \                         \     ||        |        .
     `.__.|    \                         \`-._/`.       |        !
          |                               \   \ |       |         .
           \                               \_  \|       |         |
            \                            .-' `. `.      |         `
*/