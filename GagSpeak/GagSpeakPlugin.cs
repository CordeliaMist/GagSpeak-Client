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

// The main namespace for the plugin.
namespace GagSpeak
{
    // The main plugin class, IDalamudPlugin is required.
    // All variables in here have underscores because similar names are used elsewhere
    // Might make public unsafe partial class
    public unsafe partial class GagSpeak : IGagSpeakPlugin, IDalamudPlugin
    {
        public string Name => "Gag Speak"; // Define plugin name

        private string _safeword; // Define the safeword for the plugin
        private bool _friendsOnly; // Declare if only people on friends list can say your trigger word
        private bool _partyOnly; // Declare if only people in your party can say your trigger word
        private bool _whitelistOnly; // Declare if only people on your whitelist can say your trigger word
        private bool _config; // called in the public void GagSpeakConfig function
        private bool _debug; // for toggling the debug window in the config

        private readonly List<XivChatType> _channels; // List to hold different channels
        private readonly List<string> _selectedGagTypes; // List to hold the different gag types
        private readonly List<GagSpeakConfig.GagPadlocks> _selectedGagPadlocks; // List to hold the different gag padlocks


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

        // Get services & managers needed not already provided in Services.cs (maybe add these to them later see if it works)
        public HistoryService HistoryService { get; private set; } = null!;
        public GagSpeakConfig Configuration { get; set; } = null!;



        // Make the main GagSpeak plugin class, in the Namespace!
        // Paramater: pluginInt - The plugin interface, used for interacting with Dalamud & the game
        public GagSpeak(DalamudPluginInterface pluginInt)
        {    
            // Create a new instance of the plugin interface. See Services.cs for details
            pluginInt.Create<Services>();

            // establish plugin configuation from current config, or generate a new one if none exists.
            this.Configuration = Services.PluginInterface.GetPluginConfig() as PluginConfig?? new PluginConfig();
            
            // set our local plugin variables to the variables stored in our config!           
            _safeword = this.Configuration.Safeword;
            _friendsOnly = this.Configuration.friendsOnly;
            _partyOnly = this.Configuration.partyOnly;
            _whitelistOnly = this.Configuration.whitelistOnly;
            _selectedGagTypes = this.Configuration.selectedGagTypes;
            _selectedGagPadlocks = this.Configuration.selectedGagPadlocks;
            _channels = this.Configuration.Channels;


            // Process the main handlers from Services [THE CORE PART OF THE PLUGIN FUNCTIONALITY]

            // Services.framework.Update += OnceUponAFrame; <-- LOOK INTO LATER

            // For handling onchat messages
            Services.ChatGui.ChatMessage += Chat_OnChatMessage; // From OnChat Handling
            
            // for UI building
            Services.PluginInterface.UiBuilder.Draw += GagSpeakConfigUI;
            Services.PluginInterface.UiBuilder.OpenConfigUi += GagSpeakConfig;
            
            // command handle for opening config (May not need this but also not sure)
            Services.CommandManager.AddHandler("/gagspeak", new CommandInfo(Command) {
                HelpMessage = "Opens the GagSpeak config window."
            });
        }
        
        // Function: SaveConfig
        // Purpose: To save the stored variables from the config whenever config is closed.
        public void SaveConfig() {
            this.Configuration.Channels = _channels;
            this.Configuration.friendsOnly = _friendsOnly;
            this.Configuration.partyOnly = _partyOnly;
            this.Configuration.whitelistOnly = _whitelistOnly;
            this.Configuration.selectedGagTypes = _selectedGagTypes;
            this.Configuration.selectedGagPadlocks = _selectedGagPadlocks;
            Services.PluginInterface.SavePluginConfig((IPluginConfiguration)this.Configuration);
        }

        // Dispose function to dispose of the plugin when it is closed
        void IDisposable.Dispose() {
            Services.ChatGui.ChatMessage -= Chat_OnChatMessage; // remove the chat handler
            Services.PluginInterface.UiBuilder.Draw -= GagSpeakConfigUI; // remove the config UI
            Services.PluginInterface.UiBuilder.OpenConfigUi -= GagSpeakConfig; // remove the config information
            Services.CommandManager.RemoveHandler("/gagspeak"); // remove the handler created for /gagspeak command
            /// Below is possibly a better system for handling multiple windows, look into more later.
        }

        // Simple function for the config command 
        private void GagSpeakConfig() => _config = true;

        // The main handler that decides what happens based on the commands called.
        private void Command(string command, string args)
        {
            // This system may need to be pulled off to its own file in order to shorten the main plugin.cs
            if (command == "gagspeak") {
                // Our command is gagspeak
                if (args == "config") GagSpeakConfig(); // If the arguements are config, open the config window
                if (args == "safeword") {
                    // If the arguements are safeword, assign the string afterward as the safeword automatically
                }
                if (args == "showlist") {
                    // secondary arguement would be "padlocks" or "gags". Will display all options for each respective list.
                }
            } else if (command == "gag") {
                // Our command is gag, so handle which gag is applied (maybe control other user with this)

                // List out all gags here and trigger the applicable one on.

            } else if (command == "gaglock") {
                // Our command is gaglock, which will apply a spesified padlock type to the spesified gag.

                // Arguements would be the gag layer, and the type of padlock to use

            } else if (command == "ungag") {
                // Our command is ungag, so handle which gag is removed

                // List out all gags here and trigger the applicable one off

                // Alternatively, they can write all to take all off
                if (args == "all") {
                    // Turn all gags off
                }
            } else {
                _config = !_config;
            }

            /// EXAMPLE COMMANDS FROM EACH SECTION ///
            // /gagspeak config
            // /gagspeak safeword "Samurai"
            // /gagspeak showlist padlocks
            // /gagspeak showlist gags
            // /gag 1 ballgag
            // /gag 2 harness ballgag | Hretha@Crystal
            // /gaglock 1 MistressPadlock | Hretha@Crystal
            // /gaglock 2 CombinationPadlock
            // /ungag 2
            // /ungag all
        }

        // Function for determining if someone is a friend or not, (useful for && statements with _friendsonly)
        private bool IsFriend(string nameInput) {
            // Check if it is possible for the client to grab the local player name, if so by default set to true.
            if (nameInput == Services.ClientState.LocalPlayer?.Name.TextValue) return true;

            // after, scan through each object in the object table
            foreach (var t in Services.objectTable) {
                // If the object is a player character, conmtinue on..
                if (!(t is PlayerCharacter pc)) continue;
                // If the player characters name matches the list of names from local players 
                if (pc.Name.TextValue == nameInput) {
                    // See if they have a status of being a friend, if so return true, otherwise return false.
                    return pc.StatusFlags.HasFlag(StatusFlags.Friend);
                }
            }
            return false;
        }

        // Similar function to IsFreind, except looks for if it is a party member. (useful for && statements with _partyonly)
        private bool IsPartyMember(string nameInput) {
            if (nameInput == Services.ClientState.LocalPlayer?.Name.TextValue) return true;

            foreach (var t in Services.objectTable) {
                if (!(t is PlayerCharacter pc)) continue;
                if (pc.Name.TextValue == nameInput) {
                    return pc.StatusFlags.HasFlag(StatusFlags.PartyMember);
                }
            }
            return false;
        }

        // Class to store character data to interact with later
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
}
