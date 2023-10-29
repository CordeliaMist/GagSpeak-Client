using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

// Remember to remove any which have 0 references.
namespace GagSpeak.Services;

public class DalamudServices
{
	// Default constructor
	public DalamudServices(DalamudPluginInterface pluginInt) {
		// Set the services to the pluginInt
		pluginInt.Inject(this);
	}

	// Add services to collection. This stores all our services in one single collection we can use everything from. Organization!
	public void AddServices(IServiceCollection services) 
	{
		// Adds a singleton service of the type specified in serviceType with an implementation of the type specified in
		services.AddSingleton(GameInteropProvider);
		services.AddSingleton(DalamudPluginInterface);
		services.AddSingleton(BuddyList);
		services.AddSingleton(PluginLog);
		services.AddSingleton(ChatGui);
		services.AddSingleton(ClientState);
		services.AddSingleton(CommandManager);
		services.AddSingleton(Condition);
		services.AddSingleton(DutyStage);
		services.AddSingleton(fateTable);
		services.AddSingleton(flyTextGui);
		services.AddSingleton(framework);
		services.AddSingleton(gameGui);
		services.AddSingleton(gameNetwork);
		services.AddSingleton(jobGauges);
		services.AddSingleton(keyState);
		services.AddSingleton(libcFunction);
		services.AddSingleton(objectTable);
		services.AddSingleton(partyFinderGui);
		services.AddSingleton(partyList);
		services.AddSingleton(sigScannerD);
		services.AddSingleton(targetManager);
		services.AddSingleton(toastGui);
		services.AddSingleton(this);
	}
		
	#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	[PluginService] public IGameInteropProvider GameInteropProvider { get; private set; } = null!; // Maybe add a =null!; at end?
	[PluginService] public DalamudPluginInterface DalamudPluginInterface { get; private set; } = null!; // for interfacing w/ plugin.
	[PluginService] public IBuddyList BuddyList { get; private set; } = null!; // Have no idea what this does yet.
	[PluginService] public IPluginLog PluginLog { get; private set; } = null!; // For logging information about the plugin
	[PluginService] public IChatGui ChatGui { get; private set; } = null!; // For interfacing with the chat
	[PluginService] public IClientState ClientState { get; private set; } = null!; // For interfacing with the client state
	[PluginService] public ICommandManager CommandManager { get; private set; } = null!; // For interfacing with commands
	[PluginService] public ICondition Condition { get; private set; } = null!; // For interfacing with conditions
	[PluginService] public IDutyState DutyStage { get; private set; } = null!; // To know if we are currently in duty (may not be needed)
	[PluginService] public IFateTable fateTable { get; private set; } = null!; // Very much likely not needed
	[PluginService] public IFlyTextGui flyTextGui { get; private set; } = null!; // For fly by text (may not be needed)
	[PluginService] public IFramework framework { get; private set; } = null!; // For interfacing with the framework [Dalamud Plugin Service type]
	[PluginService] public IGameGui gameGui { get; private set; } = null!; // For interfacing with the game GUI
	[PluginService] public IGameNetwork gameNetwork { get; private set; } = null!; // for interfacing with the gameNetwork status.
	[PluginService] public IJobGauges jobGauges { get; private set; } = null!; // for interfacing with job guages (unsure why needed)
	[PluginService] public IKeyState keyState { get; private set; } = null!; // for getting the keystate (unsure what this mean atm)
	[PluginService] public ILibcFunction libcFunction { get; private set; } = null!; // For interfacing with the libc function
	[PluginService] public IObjectTable objectTable { get; private set; } = null!; // For interfacing with the object table
	[PluginService] public IPartyFinderGui partyFinderGui { get; private set; } = null!; // For interfacing with the party finder (may remove)
	[PluginService] public IPartyList partyList { get; private set; } = null!; // For interfacing with the party list to know if someone is in party
	[PluginService] public ISigScanner sigScannerD { get; private set; } = null!; // Have no idea what this does
	[PluginService] public ITargetManager targetManager { get; private set; } = null!; // For interfacing with the target manager (may not need)
	[PluginService] public IToastGui toastGui { get; private set; } = null!;
	#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}