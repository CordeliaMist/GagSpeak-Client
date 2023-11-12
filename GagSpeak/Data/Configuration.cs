using System;
using Dalamud.Configuration;
using Dalamud.Game.Text; // Interacting with game chat, XIVChatType, ext.
using System.Collections.Generic;
using System.Linq; // For enabling lists
using System.IO;
using Newtonsoft.Json;
using Dalamud.Interface.Internal.Notifications;
using GagSpeak.Data;
using OtterGui.Classes;

using GagSpeak.UI;
using GagSpeak.Services;

using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
// Sets up the configuration controls for the GagSpeak Plugin
namespace GagSpeak;

public enum GagPadlocks {
    None, // No gag
    MetalPadlock, // Metal Padlock, can be picked
    CombinationPadlock, // Combination Padlock, must enter 4 digit combo to unlock
    PasswordPadlock, // Password Padlock, must enter password to unlock
    FiveMinutesPadlock, // 5 minute padlock, must wait 5 minutes to unlock
    TimerPasswordPadlock, // Timer Password Padlock, must enter password to unlock, but only after a certain amount of time
    MistressPadlock, // Mistress Padlock, must ask mistress to unlock
    MistressTimerPadlock, // Mistress Timer Padlock, must ask mistress to unlock, but only after a certain amount of time
}

public class GagSpeakConfig : IPluginConfiguration, ISavable
{
    public int Version { get; set; } = 0; // Version of the plugin
    public bool FreshInstall { get; set; } = true; // Is user on a fresh install?
    public bool Enabled { get; set; } = true; // Is plugin enabled?
    public bool InDomMode { get; set; } = false; // Is plugin in dom mode?
    public bool DirectChatGarbler { get; set; } = false; // Is direct chat garbler enabled?
    public string Safeword { get; set; } = "safeword"; // What is the safeword?
    public bool friendsOnly { get; set; } = false; // is friend only enabled?
    public bool partyOnly { get; set; } = false; // Is party only enabled?
    public bool whitelistOnly { get; set; } = false; // Is whitelist only enabled?
    public bool DebugMode { get; set; } = false; // Is debug mode enabled?
    public int GarbleLevel { get; set; } = 0; // Current Garble Level (0-20)
    public List<string> selectedGagTypes { get; set; } // What gag types are selected?
    public List<GagPadlocks> selectedGagPadlocks { get; set; } // which padlocks are equipped currently?
    public List<string> selectedGagPadlocksPassword { get; set; } // password lock on padlocks, if any
    public List<string> selectedGagPadlocksAssigner { get; set; } // name of who assigned the padlocks to the user
    public List<ChatChannel.ChatChannels> Channels { get; set; } // Which channels are currently enabled / allowed?
    //public ChatChannel.ChatChannels CurrentChannel { get; set; } // What is the current channel?
    public int ProcessTranslationInterval { get; set; } = 300000; // current process intervals for the history
    public int TranslationHistoryMax { get; set; } = 30; // Gets or sets max number of translations stored in history
    public XivChatType[] _allowedChannels = { // Dont think we need this but may be wrong
        XivChatType.Say, XivChatType.Shout, XivChatType.TellOutgoing, XivChatType.TellIncoming, XivChatType.Party,
        XivChatType.Alliance, XivChatType.Ls1, XivChatType.Ls2, XivChatType.Ls3, XivChatType.Ls4, XivChatType.Ls5,
        XivChatType.Ls6, XivChatType.Ls7, XivChatType.Ls8, XivChatType.FreeCompany, XivChatType.NoviceNetwork,
        XivChatType.CustomEmote, XivChatType.StandardEmote, XivChatType.Yell, XivChatType.CrossParty, XivChatType.CrossLinkShell1,
        XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3, XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5,
        XivChatType.CrossLinkShell6, XivChatType.CrossLinkShell7, XivChatType.CrossLinkShell8
    };
    // Config Options brought over for UI purposes / Implementation purposes
    public MainWindow.TabType SelectedTab          { get; set; } = MainWindow.TabType.General; // Default to the general tab
    public bool ShowDesignQuickBar               { get; set; } = false; // Show the design quickbar?
    public bool LockDesignQuickBar               { get; set; } = false; // Lock the design quickbar?
    public bool ShowQuickBarInTabs               { get; set; } = true;  // Show the quickbar in the tabs?
    public bool LockMainWindow                   { get; set; } = false; // Lock the main window?

    private List<WhitelistCharData> whitelist = new List<WhitelistCharData>(); // appears to be baseline for whitelist
    public List<WhitelistCharData> Whitelist { get => whitelist; set => whitelist = value; } // Note sure why, document later

    // There was stuiff about a colorId dictionary here, if you ever need to include it later, you know where to put it so it fits into the hierarchy
    private readonly SaveService _saveService;

    public GagSpeakConfig(SaveService saveService, ConfigMigrationService migrator)
    {
        _saveService = saveService;
        Load(migrator);

        // Make sure we aren't getting any duplicates
        if (this.selectedGagTypes == null || !this.selectedGagTypes.Any() || this.selectedGagTypes.Count > 3) {
            this.selectedGagTypes = new List<string> { "None", "None", "None" };}
        // Set default values for selectedGagPadlocks
        if (this.selectedGagPadlocks == null || !this.selectedGagPadlocks.Any() || this.selectedGagPadlocks.Count > 3) {
            this.selectedGagPadlocks = new List<GagPadlocks> { GagPadlocks.None, GagPadlocks.None, GagPadlocks.None };}
        // set default values for selected channels/
        if (this.Channels == null || !this.Channels.Any()) {
            this.Channels = new List<ChatChannel.ChatChannels>(){ChatChannel.ChatChannels.Say};}
        // set default values for selectedGagPadlocksPassword
        if (this.selectedGagPadlocksPassword == null || !this.selectedGagPadlocksPassword.Any() || this.selectedGagPadlocksPassword.Count > 3) {
            this.selectedGagPadlocksPassword = new List<string> { "", "", "" };}
        // set default values for selectedGagPadlocksAssigner
        if (this.selectedGagPadlocksAssigner == null || !this.selectedGagPadlocksAssigner.Any() || this.selectedGagPadlocksAssigner.Count > 3) {
            this.selectedGagPadlocksAssigner = new List<string> { "", "", "" };}
    }

    public void Save() {
        // update garble scrore
        this.GarbleLevel = this.selectedGagTypes.Sum(gagType => this.GagTypes[gagType]);

        // initialize save service
        _saveService.DelaySave(this);
    }

    public void Load(ConfigMigrationService migrator) {
        static void HandleDeserializationError(object? sender, ErrorEventArgs errorArgs) {
            GagSpeak.Log.Error( $"Error parsing Configuration at {errorArgs.ErrorContext.Path}, using default or migrating:\n{errorArgs.ErrorContext.Error}");
            errorArgs.ErrorContext.Handled = true;
        }
        if (!File.Exists(_saveService.FileNames.ConfigFile))
            return;

        if (File.Exists(_saveService.FileNames.ConfigFile))
            try {
                var text = File.ReadAllText(_saveService.FileNames.ConfigFile);
                JsonConvert.PopulateObject(text, this, new JsonSerializerSettings {
                    Error = HandleDeserializationError,
                });
            }
            catch (Exception ex) {
                GagSpeak.Messager.NotificationMessage(ex,
                    "Error reading Configuration, reverting to default.\nYou may be able to restore your configuration using the rolling backups in the XIVLauncher/backups/GagSpeak directory.",
                    "Error reading Configuration", NotificationType.Error);
            }
        migrator.Migrate(this);
    }
    public string ToFilename(FilenameService fileNames)
        => fileNames.ConfigFile;

    public void Save(StreamWriter writer) {
        using var jWriter    = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        var       serializer = new JsonSerializer { Formatting         = Formatting.Indented };
        serializer.Serialize(jWriter, this);
    }

    public static class Constants {
        public const int CurrentVersion = 4;
    }

    // create an dictionary for all the gag types and their strengths
    public Dictionary<string, int> GagTypes {get; set; } = new() {
        { "None", 0},
        { "Ball Gag", 3 },
        { "Ball Gag Mask", 4 },
        { "Bamboo Gag", 4 },
        { "Bit Gag", 3 },
        { "Bone Gag", 2 },
        { "Cage Muzzle", 0},
        { "Chloroform Cloth", 1 },
        { "Chopstick Gag", 2 },
        { "Cloth Gag", 1 },
        { "Cloth Stuffing", 2 },
        { "Crop", 1 },
        { "Cup Holder Gag", 3 },
        { "Custom Latex Hood", 4 },
        { "Deepthroat Penis Gag", 6 },
        { "Dental Gag", 2 },
        { "Dildo Gag", 5 },
        { "Duct Tape", 4 },
        { "Duster Gag", 3 },
        { "Exposed Dog Muzzle", 4 },
        { "Funnel Gag", 5 },
        { "Futuristic Ball Gag", 5 },
        { "Futuristic Harness Panel Gag", 6 },
        { "Futuristic Panel Gag", 4 },
        { "Gas Mask", 3 },
        { "Harness Ball Gag", 5 },
        { "Harness Ball Gag XL", 6 },
        { "Harness Panel Gag", 3 },
        { "Hook Gag Mask", 3 },
        { "Inflatable Hood", 5 },
        { "Large Dildo", 4 },
        { "Latex Ball Muzzle Gag", 5 },
        { "Latex Posture Collar Gag", 4 },
        { "Leather Corset Collar Gag", 4 },
        { "Leather Hood", 4 },
        { "Lip Gag", 2 },
        { "Medical Mask", 1},
        { "Muzzle Gag", 4 },
        { "Panty Stuffing", 2 },
        { "Plastic Wrap", 2 },
        { "Plug Gag", 5 },
        { "Pony Hood", 4 },
        { "Prison Lockdown Gag", 4 },
        { "Pump Gag lv.1", 2 },
        { "Pump Gag lv.2", 3 },
        { "Pump Gag lv.3", 5 },
        { "Pump Gag lv.4", 7 },
        { "Ribbons", 2 },
        { "Ring Gag", 3 },
        { "Rope Gag", 2 },
        { "Rubber Carrot Gag", 5 },
        { "Scarf", 1 },
        { "Sensory Deprivation Hood", 6 },
        { "Slime", 4 },
        { "Sock Stuffing", 2 },
        { "Spider Gag", 3 },
        { "Steel Muzzle Gag", 4 },
        { "Stitched Muzzle Gag", 3 },
        { "Tentacle", 5 },
        { "Web Gag", 2 },
        { "Wiffle Gag", 2 },
        { "XL Bone Gag", 4 },
    };
}
#pragma warning restore IDE1006
