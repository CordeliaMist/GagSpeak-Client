using Dalamud.IoC;
using System.IO;
using Dalamud.Interface.Windowing; // for interfacing with the UI windows
using Dalamud.Plugin.Services; // For the plugin services
using Dalamud.Configuration; // For the plugin configuration, unsure if needed yet.
using Dalamud.Game.Text; // Interacting with game chat, XIVChatType, ext.
using Dalamud.Game.Command; // To grab ingame commands so we can use some for our plugin
using Dalamud.Plugin; // Required include for the plugin to work
using System; // For the IDisposable

using Dalamud.Game.Text.SeStringHandling; // For parsing the way SE strings & payload handling
using System.Collections.Generic; // For enabling lists

using ImGuiNET;
using OtterGui;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Balloon = FFXIVClientStructs.FFXIV.Client.Game.Balloon;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework;
using Num = System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using System.Runtime.CompilerServices;
using System.Diagnostics.Tracing;
using System.Linq;


// Good practice for modular design
using System.Reflection;
using GagSpeak.UI;
using GagSpeak.Services;
using GagSpeak.Chat;
using Microsoft.Extensions.DependencyInjection;
using OtterGui.Classes;
using OtterGui.Log;
using Lumina.Excel.GeneratedSheets;

// In an ideal world, once fully compartmentalizard, main should be very small.

// The main namespace for the plugin.
namespace GagSpeak;


public class GagSpeak : IDalamudPlugin
{
    // Main initializations here.
    public string Name => "GagSpeak"; // Define plugin name

    // I have no idea how this line works, look into it further later.
    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
    public static readonly Logger Log = new(); // initialize the logger for our plugin
    private readonly ServiceProvider _services; // initialize our services.
    private readonly List<XivChatType> _channels = new(); // List to hold different channels [SHOULD REMOVE THIS!!!!]

        // Holds the order of the XIVChatType channels in _order
        private readonly List<XivChatType> _order = new()
        {
            XivChatType.None, XivChatType.None, XivChatType.None, XivChatType.None, 
            XivChatType.Say, XivChatType.Shout, XivChatType.TellOutgoing, XivChatType.TellIncoming,
            XivChatType.Party, XivChatType.Alliance, XivChatType.Ls1, XivChatType.Ls2,
            XivChatType.Ls3, XivChatType.Ls4, XivChatType.Ls5, XivChatType.Ls6,
            XivChatType.Ls7, XivChatType.Ls8, XivChatType.FreeCompany, XivChatType.NoviceNetwork,
            XivChatType.CustomEmote, XivChatType.StandardEmote, XivChatType.Yell, XivChatType.CrossParty,
            XivChatType.PvPTeam, XivChatType.CrossLinkShell1, XivChatType.Echo, XivChatType.None,
            XivChatType.None, XivChatType.None, XivChatType.None, XivChatType.None,
            XivChatType.None, XivChatType.None, XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3,
            XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6, XivChatType.CrossLinkShell7,
            XivChatType.CrossLinkShell8
        };

        // Holds the yes/no for each XIVChatType channel so we know which is enabled or not
        private readonly bool[] _yesno =
        {
            false, false, false, false, // None, None, None, None
            true, true, true, true,     // Say, Shout, TellOutgoing, TellIncoming
            true, true, true, true,     // Party, Alliance, Ls1, Ls2
            true, true, true, true,     // Ls3, Ls4, Ls5, Ls6
            true, true, true, true,     // Ls7, Ls8, FreeCompany, NoviceNetwork
            true, true, true, true,     // CustomEmote, StandardEmote, Yell, CrossParty
            true, true, true, false,    // PvPTeam, CrossLinkShell1, Echo, None
            false, false, false, false, // None, None, None, None
            false, false, true, true,   // None, None, CWL2, CWL3
            true, true, true, true,     // CWL4, CWL5, CWL6, CWL7
            true                        // CWL8
        };

        // Create an array of all xivchattypes excluding all channels with a .None, leaving us with all allowed channels
        private readonly XivChatType[] _allowedChannels =
            {
            XivChatType.Say, XivChatType.Shout, XivChatType.TellOutgoing, XivChatType.TellIncoming, XivChatType.Party,
            XivChatType.Alliance, XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3, XivChatType.Ls4, XivChatType.Ls5,
            XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8, XivChatType.FreeCompany, XivChatType.NoviceNetwork,
            XivChatType.CustomEmote, XivChatType.StandardEmote, XivChatType.Yell, XivChatType.CrossParty, XivChatType.CrossLinkShell1,
            XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3, XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5,
            XivChatType.CrossLinkShell6, XivChatType.CrossLinkShell7, XivChatType.CrossLinkShell8
        };

        ////* -- MAIN FUNCTION FOR PLUGIN OPENING -- *///
        public GagSpeak(DalamudPluginInterface pluginInt)
        {    
            try
            {
                // Initialize the services in the large Service collection. (see ServiceHandler.cs)
                _services = ServiceHandler.CreateProvider(pluginInt, Log);

                // Initialize messager service once one is made here if needed

                // Initialize the UI
                _services.GetRequiredService<GagSpeakWindowManager>();

                // Initialize the command manager
                _services.GetRequiredService<CommandManager>();

                // Initialize the OnChatMessage handler
                _services.GetRequiredService<OnChatManager>();

                // Initialize the garbler service (may retain inside of GagSpeakWindowManager or OnChatMessage handler)
                Log.Information($"GagSpeak version{Version} loaded successfully.");
            }
            catch
            {
                // Note sure how to throw an error here since the services are not yet initialized but yeah
                // Error($"Error while fetching config: {e}");
                // if we couldnt suceed, just yeet it.
                Dispose();
                throw;
            }
        }

            // For handling onchat messages
            //Services.ChatGui.ChatMessage += Chat_OnChatMessage; // From OnChat Handling [Catagorized as an Event?]
            
            // for UI building
            // This should replace the two commented lines below
            
            // Services.PluginInterface.UiBuilder.Draw += _mainWindow.Draw;
            // Services.PluginInterface.UiBuilder.OpenConfigUi += GagSpeakConfig; // for opening Main Window
            
            // command handle for opening config (May not need this but also not sure)

        ////* -- MAIN FUNCTION FOR PLUGIN CLOSING -- *///
        public void Dispose()
            => _services?.Dispose(); // Dispose of all services. (call all of their dispose functions)
            //Services.ChatGui.ChatMessage -= Chat_OnChatMessage; // remove the chat handler
            //Services.PluginInterface.UiBuilder.Draw -= this.BuildUI; // remove the config UI
            //Services.PluginInterface.UiBuilder.OpenConfigUi -= GagSpeakConfig; // remove the config information
            //Services.CommandManager.RemoveHandler("/gagspeak"); // remove the handler created for /gagspeak command
        


        // Function: SaveConfig
        // Purpose: To save the stored variables from the config whenever config is closed.
        // public void SaveConfig() {
        //     this.Configuration.friendsOnly = _friendsOnly;
        //     this.Configuration.partyOnly = _partyOnly;
        //     this.Configuration.whitelistOnly = _whitelistOnly;
        //     this.Configuration.Channels = _channels;
        //     Services.PluginInterface.SavePluginConfig(this.Configuration);

        //     // Empty out any lists to prevent addative leaking
        // }


        // Simple function for the config command, DISABLED FOR NEW STRUCTURE TEST 
        //private void GagSpeakConfig() => _config = true;

        // Initializes any empty lists from config with default values


        // USE THIS FUNCTION FOR STORING DATA ABOUT MESSAGE DETECTION AND STRING BUILDING
        // private bool IsFriend(string nameInput) {
        //     // Check if it is possible for the client to grab the local player name, if so by default set to true.
        //     if (nameInput == Services.ClientState.LocalPlayer?.Name.TextValue) return true;

        //     // after, scan through each object in the object table
        //     foreach (var t in Services.objectTable) {
        //         // If the object is a player character, conmtinue on..
        //         if (!(t is PlayerCharacter pc)) continue;
        //         // If the player characters name matches the list of names from local players 
        //         if (pc.Name.TextValue == nameInput) {
        //             // See if they have a status of being a friend, if so return true, otherwise return false.
        //             return pc.StatusFlags.HasFlag(StatusFlags.Friend);
        //         }
        //     }
        //     return false;
        // }

        // // Similar function to IsFreind, except looks for if it is a party member. (useful for && statements with _partyonly)
        // private bool IsPartyMember(string nameInput) {
        //     if (nameInput == Services.ClientState.LocalPlayer?.Name.TextValue) return true;

        //     foreach (var t in Services.objectTable) {
        //         if (!(t is PlayerCharacter pc)) continue;
        //         if (pc.Name.TextValue == nameInput) {
        //             return pc.StatusFlags.HasFlag(StatusFlags.PartyMember);
        //         }
        //     }
        //     return false;
        // }

    // May not even need this class
    private class CharacterData {
        public SeString? Message;
        public XivChatType Type;
        public uint ActorId;
        public DateTime MessageDateTime;
        public string? Name;
        public bool NewMessage { get; set; }
        public bool KillMe { get; set; } = false; 
    }
}
