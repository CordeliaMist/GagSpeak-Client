using System.Reflection;  // Without this, the Version string will not be able to fetch from the assembly class
using Dalamud.Plugin;     // Used for the IDalamudPlugin interface
using Microsoft.Extensions.DependencyInjection; // Used in the creation of our service collection, and so must be included when we call to get our services
using OtterGui.Classes;   // For the messageService, which is used to display the changelog
using OtterGui.Log;       // for our plugin logger
using GagSpeak.UI;        // REQUIRED for our plugins GagSpeakWindowManager requiredservices to be fetched
using GagSpeak.Services;  // REQUIRED for our plugins CommandManager requiredservices to be fetched
using GagSpeak.Chat;      // REQUIRED for our plugins ChatManager requiredservices to be fetched

// The main namespace for the plugin, aka the same name of our plugin, the highest level
namespace GagSpeak;

// i just use this across all my files because i dont like the yellow errors that occur due to my shitty _variable naming convention
#pragma warning disable IDE1006
public class GagSpeak : IDalamudPlugin
{
  /// <summary> Gets the name of the plugin. </summary>
  public string Name => "GagSpeak"; // Define plugin name

  /// <summary> gets the version of our current plugin from the .csproj file </summary>
  public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty; // I have no idea how this line works, look into it further later.
  
  /// <summary> initialize the logger for our plugin, which will display debug information to our /xllog in game </summary>
  public static readonly Logger Log = new(); // initialize the logger for our plugin

  /// <summary> the messager service for this plugin. A part of the otterGui services, and is used to display the changelog </summary>
  public static MessageService Messager { get; private set; } = null!; // initialize the messager service, part of otterGui services.
  
  /// <summary> the service provider for the plugin </summary>
  private readonly ServiceProvider _services; 

  /// <summary>
  /// Initializes a new instance of the <see cref="GagSpeak"/> class.
  /// <list type="bullet">
  /// <item><c>pluginInt</c><param name="pluginInt"> - The Dalamud plugin interface.</param></item>
  /// </list> </summary>
  public GagSpeak(DalamudPluginInterface pluginInt)
  {
      // Initialize the services in the large Service collection. (see ServiceHandler.cs)
      // if at any point this process fails, we should immidiately dispose of the services, throw an exception, and exit the plugin.
      try
      {
          _services = ServiceHandler.CreateProvider(pluginInt, Log); // Initialize the services in the large Service collection. (see ServiceHandler.cs)
          Messager = _services.GetRequiredService<MessageService>(); // Initialize messager service here

          _services.GetRequiredService<GagSpeakWindowManager>(); // Initialize the UI
          _services.GetRequiredService<CommandManager>(); // Initialize the command manager
          _services.GetRequiredService<ChatManager>(); // Initialize the OnChatMessage
          _services.GetRequiredService<ChatInputProcessor>(); // Initialize the chat message detour
          Log.Information($"GagSpeak version{Version} loaded successfully."); // Log the version to the /xllog menu
      }
      catch
      {
          Dispose();
          throw;
      }
  }

  /// <summary> Disposes the plugin and its services, will call upon the service collection so all services use their dispose function. </summary>
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