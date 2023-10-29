using System;
using System.ComponentModel;
using Dalamud.Configuration;
using Dalamud.Game.Text; // Interacting with game chat, XIVChatType, ext.
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Threading.Channels;
using System.Linq; // For enabling lists
using System.IO;
using Newtonsoft.Json;
using Dalamud.Plugin;

// practicing modular design
using GagSpeak.UI;
using GagSpeak.Services;
using GagSpeak.Events;


#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
// Sets up the configuration controls for the GagSpeak Plugin
namespace GagSpeak;

// create an enumeration for all the gag types
// A majority of these likely wont be implemented, but its nice to have.
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
    public string Safeword { get; set; } = "safeword"; // What is the safeword?
    public bool friendsOnly { get; set; } = false; // is friend only enabled?
    public bool partyOnly { get; set; } = false; // Is party only enabled?
    public bool whitelistOnly { get; set; } = false; // Is whitelist only enabled?
    public bool DebugMode { get; set; } = false; // Is debug mode enabled?
    public int GarbleLevel { get; set; } = 0; // Current Garble Level (0-20)
    public int ProcessTranslationInterval { get; set; } = 300000; // current process intervals for the history
    public int TranslationHistoryMax { get; set; } = 30; // Gets or sets max number of translations stored in history
    public List<string> selectedGagTypes { get; set; } // What gag types are selected?
    public List<GagPadlocks> selectedGagPadlocks { get; set; } // which padlocks are equipped currently?
    public List<XivChatType> Channels { get; set; } // Which channels are currently enabled?

    // public List<XivChatType> _order = new() {
    //     XivChatType.None, XivChatType.None, XivChatType.None, XivChatType.None, 
    //     XivChatType.Say, XivChatType.Shout, XivChatType.TellOutgoing, XivChatType.TellIncoming,
    //     XivChatType.Party, XivChatType.Alliance, XivChatType.Ls1, XivChatType.Ls2,
    //     XivChatType.Ls3, XivChatType.Ls4, XivChatType.Ls5, XivChatType.Ls6,
    //     XivChatType.Ls7, XivChatType.Ls8, XivChatType.FreeCompany, XivChatType.NoviceNetwork,
    //     XivChatType.CustomEmote, XivChatType.StandardEmote, XivChatType.Yell, XivChatType.CrossParty,
    //     XivChatType.PvPTeam, XivChatType.CrossLinkShell1, XivChatType.Echo, XivChatType.None,
    //     XivChatType.None, XivChatType.None, XivChatType.None, XivChatType.None,
    //     XivChatType.None, XivChatType.None, XivChatType.CrossLinkShell2, XivChatType.CrossLinkShell3,
    //     XivChatType.CrossLinkShell4, XivChatType.CrossLinkShell5, XivChatType.CrossLinkShell6, XivChatType.CrossLinkShell7,
    //     XivChatType.CrossLinkShell8
    // }; May not need for now!!!!! Keeping for good reference for the CHANNEL_IS_ACTIVE below
    public bool[] ChannelsIsActive = {
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
    }; // Which channels are currently enabled?

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


    // Configuration options for the whitelist tab
    private List<string> whitelist = new List<string>(); // appears to be baseline for whitelist
    public List<string> Whitelist { get => whitelist; set => whitelist = value; } // Note sure why, document later

    private readonly SaveService _saveService;

    public GagSpeakConfig(SaveService saveService)
    {
        _saveService = saveService;
        //Load(migrator);
        if (this.selectedGagTypes == null || !this.selectedGagTypes.Any() || this.selectedGagTypes.Count > 3) {
            this.selectedGagTypes = new List<string> { "None", "None", "None" };
        }
        // Set default values for selectedGagPadlocks
        if (this.selectedGagPadlocks == null || !this.selectedGagPadlocks.Any())
        {
            this.selectedGagPadlocks = new List<GagPadlocks> { GagPadlocks.None, GagPadlocks.None, GagPadlocks.None };
        }
        // set default values for selected channels/
        if (this.Channels == null || !this.Channels.Any()) {
            this.Channels = new List<XivChatType>(){XivChatType.Say};
        }
    }

    public void Save()
        => _saveService.DelaySave(this);

    public string ToFilename(FilenameService fileNames)
        => fileNames.ConfigFile;

    public void Save(StreamWriter writer)
    {
        using var jWriter    = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        var       serializer = new JsonSerializer { Formatting         = Formatting.Indented };
        serializer.Serialize(jWriter, this);
    }
    // create an dictionary for all the gag types and their strengths
    public Dictionary<string, int> GagTypes {get; set; } = new() {
        { "None", 0},
        { "Ball Gag", 5 },
        { "Ball Gag Mask", 5 },
        { "Bamboo Gag", 4 },
        { "Bit Gag", 2 },
        { "Bone Gag", 2 },
        { "Chloroform Cloth", 1 },
        { "Chopstick Gag", 4 },
        { "Cloth Gag", 1 },
        { "Cloth Stuffing", 2 },
        { "Crop", 2 },
        { "Cup Holder Gag", 3 },
        { "Custom Latex Hood", 4 },
        { "Deepthroat Penis Gag", 6 },
        { "Dental Gag", 2 },
        { "Dildo Gag", 5 },
        { "Dog Hood", 4 },
        { "Duct Tape", 4 },
        { "Duster Gag", 3 },
        { "Exposed Dog Muzzle", 4 },
        { "Funnel Gag", 5 },
        { "Fur Scarf", 2 },
        { "Futuristic Ball Gag", 6 },
        { "Futuristic Harness Panel Gag", 7 },
        { "Futuristic Panel Gag", 5 },
        { "Gas Mask", 3 },
        { "Harness Ball Gag", 5 },
        { "Harness Ball Gag XL", 6 },
        { "Harness OTN Plug Gag", 8 },
        { "Harness Pacifier", 2 },
        { "Harness Panel Gag", 3 },
        { "Hook Gag Mask", 3 },
        { "Inflatable Hood", 5 },
        { "Large Dildo", 4 },
        { "Latex Ball Muzzle Gag", 5 },
        { "Latex Posture Collar Gag", 4 },
        { "Latex Respirator", 1 },
        { "Leather Corset Collar Gag", 4 },
        { "Leather Hood", 4 },
        { "Muzzle Gag", 4 },
        { "Panty Stuffing", 2 },
        { "Plastic Wrap", 2 },
        { "Plug Gag", 5 },
        { "Polished Steel Hood", 6 },
        { "Pony Hood", 4 },
        { "Prison Lockdown Gag", 4 },
        { "Pump Gag lv1", 2 },
        { "Pump Gag lv2", 3 },
        { "Pump Gag lv3", 5 },
        { "Pump Gag lv4", 7 },
        { "Ribbons", 2 },
        { "Ring Gag", 3 },
        { "Rope Gag", 2 },
        { "Rubber Carrot Gag", 5 },
        { "Scarf", 1 },
        { "Sensory Deprivation Hood", 6 },
        { "Silicon Bit Gag", 4 },
        { "Slime", 6 },
        { "Smooth Latex Mask", 5 },
        { "Sock Stuffing", 2 },
        { "Spider Gag", 3 },
        { "Steel Muzzle Gag", 4 },
        { "Stitched Muzzle Gag", 3 },
        { "Stitches", 6 },
        { "Tentacle", 5 },
        { "Web Gag", 2 },
        { "XL Bone Gag", 4 },
    };
}
#pragma warning restore IDE1006