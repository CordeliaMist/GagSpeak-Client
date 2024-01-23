using Dalamud.Game;				// Provides service classes for interacting with the game
using Dalamud.IoC; 				// Provides service classes for dependency injection
using Dalamud.Plugin; 			// Provides interfaces and service classes for creating Dalamud plugins
using Dalamud.Plugin.Services; 	// Provides service classes for plugin services
using Microsoft.Extensions.DependencyInjection; // Provides the dependency injection

// Because we need this to be part of our GagSpeak Namespace, but it is part of the services group, we name our namespace this:
namespace GagSpeak.Services;

/// <summary> Provides services for the Dalamud plugin. </summary>
public class DalamudServices {
	/// <summary> A more simplified version of the AddServices method, thanks to the new service manager </summary>
	public static void AddServices(ServiceManager services, DalamudPluginInterface pi)
	{
        services.AddExistingService(pi);
        services.AddExistingService(pi.UiBuilder);
		// now add the dalamud services
		services.AddDalamudService<IChatGui>(pi);				// For interfacing with the chat
		services.AddDalamudService<IClientState>(pi);			// For interfacing with the client state, getting player info, etc.
		services.AddDalamudService<ICommandManager>(pi);		// For interfacing with commands
		services.AddDalamudService<IDataManager>(pi);			// for parsing object data
		services.AddDalamudService<IFramework>(pi);				// For interfacing with the dalamud framework (scheduler, timings, etc.)
		services.AddDalamudService<IGameInteropProvider>(pi);	// helps with detouring the chat input for our plugin
		services.AddDalamudService<IKeyState>(pi);				// for the file system selector to use to get our state
		services.AddDalamudService<IObjectTable>(pi);			// For interfacing with the object table
		services.AddDalamudService<IPartyFinderGui>(pi);		// For interfacing with the party finder (may remove)
		services.AddDalamudService<IPluginLog>(pi);				// For interfacing with the plugin logger
		services.AddDalamudService<ISigScanner>(pi);			// For getting signatures to do stuff in danger files.
		services.AddDalamudService<ITextureProvider>(pi);		// For interfacing with the texture provider
	}
}