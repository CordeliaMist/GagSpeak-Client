using Dalamud.Game;				// Provides service classes for interacting with the game
using Dalamud.IoC; 				// Provides service classes for dependency injection
using Dalamud.Plugin; 			// Provides interfaces and service classes for creating Dalamud plugins
using Dalamud.Plugin.Services; 	// Provides service classes for plugin services
using Microsoft.Extensions.DependencyInjection; // Provides the dependency injection

// Because we need this to be part of our GagSpeak Namespace, but it is part of the services group, we name our namespace this:
namespace GagSpeak.Services;

/// <summary> Provides services for the Dalamud plugin. </summary>
public class DalamudServices {
	/// <summary>
	/// Initializes a new instance of the <see cref="DalamudServices"/> class.
	/// <list type="bullet">
	/// <item><c>pluginInt</c><param name="pluginInt"> - The Dalamud plugin interface.</param></item>
	/// </list> </summary>
	public DalamudServices(DalamudPluginInterface pluginInt) {
		// Set the services to the pluginInt
		pluginInt.Inject(this);
	}

	/// <summary>
	/// Adds services to the service collection.
	/// <list type="bullet">
	/// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
	/// </list> </summary>
	public void AddServices(IServiceCollection services) {
		// Adds a singleton service of the type specified in serviceType with an implementation of the type specified in
		services.AddSingleton(GameInteropProvider);
		services.AddSingleton(DalamudPluginInterface);
		services.AddSingleton(DalamudPluginInterface.UiBuilder);
		services.AddSingleton(PluginLog);
		services.AddSingleton(ChatGui);
		services.AddSingleton(ClientState);
		services.AddSingleton(CommandManager);
		services.AddSingleton(DataManager);
		services.AddSingleton(Framework);
		services.AddSingleton(KeyState);
		services.AddSingleton(ObjectTable);
		services.AddSingleton(PartyFinderGui);
		services.AddSingleton(SigScanner);
		services.AddSingleton(this);
	}
	[PluginService] public IGameInteropProvider GameInteropProvider { get; private set; } = null!; // helps with detouring the chat input for our plugin
	[PluginService] public DalamudPluginInterface DalamudPluginInterface { get; private set; } = null!; // for interfacing w/ plugin.
	[PluginService] public IChatGui ChatGui { get; private set; } = null!; // For interfacing with the chat
	[PluginService] public IClientState ClientState { get; private set; } = null!; // For interfacing with the client state, getting player info, etc.
	[PluginService] public ICommandManager CommandManager { get; private set; } = null!; // For interfacing with commands
	[PluginService] public IDataManager DataManager { get; set; } = null!; // for parsing object data
	[PluginService] public IFramework Framework { get; private set; } = null!; // For interfacing with the framework [Dalamud Plugin Service type]
	[PluginService] public IKeyState KeyState { get; private set; } = null!; // for the file system selector to use to get our state
	[PluginService] public IObjectTable ObjectTable { get; private set; } = null!; // For interfacing with the object table
	[PluginService] public IPartyFinderGui PartyFinderGui { get; private set; } = null!; // For interfacing with the party finder (may remove)
	[PluginService] public IPluginLog PluginLog { get; private set; } = null!; // For interfacing with the plugin logger
	[PluginService] public ISigScanner SigScanner { get; private set; } = null!; // For getting our signatures to perform the operations in our danger files.
}