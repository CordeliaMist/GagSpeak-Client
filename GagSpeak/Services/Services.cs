using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

/// <Summary>
/// The purpose of this file is to list all of the services used within the plugin.
/// This way, we know what we include and what we need to interact / work with
/// 
/// WARNING:
/// Many of these were pulled from chatbubbles and may never end up being used. If they are not, reove them from this list!
/// 
/// </Summary>
namespace GagSpeak {
    public class Services {
        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // May need to chance some of these to public static
        [PluginService] static internal IGameInteropProvider GameInteropProvider { get; private set; } // Maybe add a =null!; at end?
        [PluginService] static internal DalamudPluginInterface PluginInterface { get; private set; } // for interfacing w/ plugin.
        [PluginService] static internal IBuddyList BuddyList { get; private set; } // Have no idea what this does yet.
        [PluginService] static internal IPluginLog PluginLog { get; private set; } // For logging information about the plugin
        [PluginService] static internal IChatGui ChatGui { get; private set; } // For interfacing with the chat
        [PluginService] static internal IClientState ClientState { get; private set; } // For interfacing with the client state
        [PluginService] static internal ICommandManager CommandManager { get; private set; } // For interfacing with commands
        [PluginService] static internal ICondition Condition { get; private set; } // For interfacing with conditions
        [PluginService] static internal IDutyState DutyStage { get; private set; } // To know if we are currently in duty (may not be needed)
		[PluginService] static internal IFateTable fateTable { get; private set; } // Very much likely not needed
		[PluginService] static internal IFlyTextGui flyTextGui { get; private set; } // For fly by text (may not be needed)
		[PluginService] static internal IFramework framework { get; private set; } // For interfacing with the framework [Dalamud Plugin Service type]
		[PluginService] static internal IGameGui gameGui { get; private set; } // For interfacing with the game GUI
		[PluginService] static internal IGameNetwork gameNetwork { get; private set; } // for interfacing with the gameNetwork status.
		[PluginService] static internal IJobGauges jobGauges { get; private set; } // for interfacing with job guages (unsure why needed)
		[PluginService] static internal IKeyState keyState { get; private set; } // for getting the keystate (unsure what this mean atm)
		[PluginService] static internal ILibcFunction libcFunction { get; private set; } // For interfacing with the libc function
		[PluginService] static internal IObjectTable objectTable { get; private set; } // For interfacing with the object table
		[PluginService] static internal IPartyFinderGui partyFinderGui { get; private set; } // For interfacing with the party finder (may remove)
		[PluginService] static internal IPartyList partyList { get; private set; } // For interfacing with the party list to know if someone is in party
		[PluginService] static internal ISigScanner sigScannerD { get; private set; } // Have no idea what this does
		[PluginService] static internal ITargetManager targetManager { get; private set; } // For interfacing with the target manager (may not need)
		[PluginService] static internal IToastGui toastGui { get; private set; }

		#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}