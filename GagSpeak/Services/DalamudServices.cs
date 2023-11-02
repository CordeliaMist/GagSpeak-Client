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
		services.AddSingleton(DalamudPluginInterface.UiBuilder);
		services.AddSingleton(BuddyList);
		services.AddSingleton(PluginLog);
		services.AddSingleton(ChatGui);
		services.AddSingleton(ClientState);
		services.AddSingleton(CommandManager);
		services.AddSingleton(Condition);
		services.AddSingleton(DutyStage);
		services.AddSingleton(FateTable);
		services.AddSingleton(FlyTextGui);
		services.AddSingleton(Framework);
		services.AddSingleton(GameGui);
		services.AddSingleton(GameNetwork);
		services.AddSingleton(JobGauges);
		services.AddSingleton(KeyState);
		services.AddSingleton(LibcFunction);
		services.AddSingleton(ObjectTable);
		services.AddSingleton(PartyFinderGui);
		services.AddSingleton(PartyList);
		services.AddSingleton(SigScanner);
		services.AddSingleton(TargetManager);
		services.AddSingleton(ToastGui);
		services.AddSingleton(this);
	}
		
	#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	[PluginService] public IGameInteropProvider GameInteropProvider { get; private set; } = null!; // Maybe add a =null!; at end?
	[PluginService] public DalamudPluginInterface DalamudPluginInterface { get; private set; } = null!; // for interfacing w/ plugin.
	[PluginService] public IBuddyList BuddyList { get; private set; } = null!; // Have no idea what this does yet.
	[PluginService] public IChatGui ChatGui { get; private set; } = null!; // For interfacing with the chat
	[PluginService] public IClientState ClientState { get; private set; } = null!; // For interfacing with the client state
	[PluginService] public ICommandManager CommandManager { get; private set; } = null!; // For interfacing with commands
	[PluginService] public ICondition Condition { get; private set; } = null!; // For interfacing with conditions
	[PluginService] public IDutyState DutyStage { get; private set; } = null!; // To know if we are currently in duty (may not be needed)
	[PluginService] public IFateTable FateTable { get; private set; } = null!; // Very much likely not needed
	[PluginService] public IFlyTextGui FlyTextGui { get; private set; } = null!; // For fly by text (may not be needed)
	[PluginService] public IFramework Framework { get; private set; } = null!; // For interfacing with the framework [Dalamud Plugin Service type]
	[PluginService] public IGameGui GameGui { get; private set; } = null!; // For interfacing with the game GUI
	[PluginService] public IGameNetwork GameNetwork { get; private set; } = null!; // for interfacing with the gameNetwork status.
	[PluginService] public IJobGauges JobGauges { get; private set; } = null!; // for interfacing with job guages (unsure why needed)
	[PluginService] public IKeyState KeyState { get; private set; } = null!; // for getting the keystate (unsure what this mean atm)
	[PluginService] public ILibcFunction LibcFunction { get; private set; } = null!; // For interfacing with the libc function
	[PluginService] public IObjectTable ObjectTable { get; private set; } = null!; // For interfacing with the object table
	[PluginService] public IPartyFinderGui PartyFinderGui { get; private set; } = null!; // For interfacing with the party finder (may remove)
	[PluginService] public IPartyList PartyList { get; private set; } = null!; // For interfacing with the party list to know if someone is in party
	[PluginService] public IPluginLog PluginLog { get; private set; } = null!;
	[PluginService] public ISigScanner SigScanner { get; private set; } = null!; // Have no idea what this does
	[PluginService] public ITargetManager TargetManager { get; private set; } = null!; // For interfacing with the target manager (may not need)
	[PluginService] public IToastGui ToastGui { get; private set; } = null!;
	#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}