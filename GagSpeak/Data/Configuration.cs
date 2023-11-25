using System;
using Dalamud.Configuration;
using System.Collections.Generic;
using System.Linq; // For enabling lists
using System.IO;
using Newtonsoft.Json;
using Dalamud.Interface.Internal.Notifications;
using GagSpeak.Data;
using OtterGui.Classes;
using GagSpeak.UI;
using GagSpeak.Services;
using GagSpeak.Events;
using OtterGui.Widgets;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

#pragma warning disable IDE1006 

namespace GagSpeak;

/// <summary> The configuration for the GagSpeak plugin. </summary>
public class GagSpeakConfig : IPluginConfiguration, ISavable
{   
    // Plugin information
    public          ChangeLogDisplayType                ChangeLogDisplayType { get; set; } = ChangeLogDisplayType.New;
    public          int                                 LastSeenVersion { get; set; } = GagSpeakChangelog.LastChangelogVersion; // look more into ottergui to figure out how to implement this later.
    public          int                                 Version { get; set; } = 0;                              // Version of the plugin
    public          bool                                FreshInstall { get; set; } = true;                      // Is user on a fresh install?
    public          bool                                Enabled { get; set; } = true;                           // Is plugin enabled?
    // Personal information 
    public          bool                                SafewordUsed { get; set; } = false;                     // Has the safeword been used?
    public          bool                                InDomMode { get; set; } = false;                        // Is plugin in dom mode?
    public          bool                                DirectChatGarbler { get; set; } = false;                // Is direct chat garbler enabled?
    public          bool                                LockDirectChatGarbler { get; set; } = false;            // Is live chat garbler enabled?
    public          string                              Safeword { get; set; } = "safeword";                    // What is the safeword?
    public          bool                                friendsOnly { get; set; } = false;                      // is friend only enabled?
    public          bool                                partyOnly { get; set; } = false;                        // Is party only enabled?
    public          bool                                whitelistOnly { get; set; } = false;                    // Is whitelist only enabled?
    public          bool                                DebugMode { get; set; } = false;                        // Is debug mode enabled?
    public          int                                 GarbleLevel { get; set; } = 0;                          // Current Garble Level (0-20)
    public          ObservableList<string>              selectedGagTypes { get; set; }                          // What gag types are selected?
    public          ObservableList<GagPadlocks>         selectedGagPadlocks { get; set; }                       // which padlocks are equipped currently?
    public          List<string>                        selectedGagPadlocksPassword { get; set; }               // password lock on padlocks, if any
    public          List<DateTimeOffset>                selectedGagPadLockTimer { get; set; }                   // stores time when the padlock will be unlocked.
    public          List<string>                        selectedGagPadlocksAssigner { get; set; }               // name of who assigned the padlocks
    // additonal information below
    public          List<ChatChannel.ChatChannels>      Channels { get; set; }                                  // Which channels are currently enabled / allowed?
    public          int                                 ProcessTranslationInterval { get; set; } = 300000;      // current process intervals for the history
    public          int                                 TranslationHistoryMax { get; set; } = 30;               // Gets or sets max number of translations stored in history
    public          MainWindow.TabType                  SelectedTab { get; set; } = MainWindow.TabType.General; // Default to the general tab
    private         List<WhitelistCharData>             whitelist = new List<WhitelistCharData>();              // appears to be baseline for whitelist
    public          List<WhitelistCharData>             Whitelist { get=>whitelist; set=>whitelist = value; }   // Note sure why, document later
    public          string                              SendInfoName = "";                                      // Name of the person you are sending info to
    public          bool                                acceptingInfoRequests = true;                           // Are you accepting info requests? (for cooldowns)//
    public          Dictionary<string,DateTimeOffset>   TimerData { get; set; }                                 // stores the timer data for the plugin
    // stuff for the gaglistingDrawer
    public          List<bool>                          _isLocked { get; set; }                                 // determines if it is locked
    public          List<string>                        displaytext { get; set; }                               // stores the display text for each gaglisting 
    public          List<PadlockIdentifier>             _padlockIdentifier { get; set; }                        // stores the padlock identifier for each gaglisting
    public          PadlockIdentifier                   _whitelistPadlockIdentifier {get; set; }                // stores the padlock identifier for the whitelist

    /// <summary> Gets or sets the colors used within our UI </summary>
    public Dictionary<ColorId, uint> Colors { get; private set; }
        = Enum.GetValues<ColorId>().ToDictionary(c => c, c => c.Data().DefaultColor);

    private readonly SaveService            _saveService;                                          // Save service for the GagSpeak plugin

    /// <summary>
    /// Initializes a new instance of the <see cref="GagSpeakConfig"/> class, and initializes any empty lists and dictionaries and other variables so we get a clean fresh startup!
    /// <list type="bullet">
    /// <item><c>saveService</c><param name="saveService"> - The save service.</param></item>
    /// <item><c>migrator</c><param name="migrator"> - The config migrator.</param></item>
    /// </list> </summary>
    public GagSpeakConfig(SaveService saveService, ConfigMigrationService migrator)
    {
        _saveService = saveService;
        Load(migrator);

        // Make sure we aren't getting any duplicates
        if (this.selectedGagTypes == null || !this.selectedGagTypes.Any() || this.selectedGagTypes.Count > 3) {
            this.selectedGagTypes = new ObservableList<string> { "None", "None", "None" };}
        // Set default values for selectedGagPadlocks
        if (this.selectedGagPadlocks == null || !this.selectedGagPadlocks.Any() || this.selectedGagPadlocks.Count > 3) {
            this.selectedGagPadlocks = new ObservableList<GagPadlocks> { GagPadlocks.None, GagPadlocks.None, GagPadlocks.None };}
        // set default values for selected channels/
        if (this.Channels == null || !this.Channels.Any()) {
            this.Channels = new List<ChatChannel.ChatChannels>(){ChatChannel.ChatChannels.Say};}
        // set default values for selectedGagPadlocksPassword
        if (this.selectedGagPadlocksPassword == null || !this.selectedGagPadlocksPassword.Any() || this.selectedGagPadlocksPassword.Count > 3) {
            this.selectedGagPadlocksPassword = new List<string> { "", "", "" };}
        // set default values for selectedGagPadlocksAssigner
        if (this.selectedGagPadlocksAssigner == null || !this.selectedGagPadlocksAssigner.Any() || this.selectedGagPadlocksAssigner.Count > 3) {
            this.selectedGagPadlocksAssigner = new List<string> { "", "", "" };}
        // set default values for selectedGagPadLockTimer
        if (this.selectedGagPadLockTimer == null || !this.selectedGagPadLockTimer.Any() || this.selectedGagPadLockTimer.Count > 3) {
            this.selectedGagPadLockTimer = new List<DateTimeOffset> { DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now };}
        // set default values for _isLocked
        if (this._isLocked == null || !this._isLocked.Any() || this._isLocked.Count > 3) {
            GagSpeak.Log.Debug($"[Config]: _isLocked is null, creating new list");
            this._isLocked = new List<bool> { false, false, false };} //
        // set default values for displaytext
        if (this.displaytext == null || !this.displaytext.Any() || this.displaytext.Count > 3) {
            GagSpeak.Log.Debug($"[Config]: displaytext is null, creating new list");
            this.displaytext = new List<string> { "", "", "" };}
        // set default values for _padlockIdentifier
        if (this._whitelistPadlockIdentifier == null) {
            GagSpeak.Log.Debug($"[Config]: _whitelistPadlockIdentifier is null, creating new list");
            this._whitelistPadlockIdentifier = new PadlockIdentifier();}
        // set default for the padlock identifier listings
        if (this._padlockIdentifier == null || !this._padlockIdentifier.Any() || this._padlockIdentifier.Count > 3) {
            GagSpeak.Log.Debug($"[Config]: _padlockIdentifier is null, creating new list");
            this._padlockIdentifier = new List<PadlockIdentifier> { new PadlockIdentifier(), new PadlockIdentifier(), new PadlockIdentifier() };}
        // set default values for the timer data
        if (this.TimerData == null || !this.TimerData.Any()) {
            GagSpeak.Log.Debug($"[Config]: TimerData is null, creating new list");
            this.TimerData = new Dictionary<string, DateTimeOffset>();}
    }

    /// <summary> Saves the config to our save service and updates the garble level to its new value. </summary>
    public void Save() {
        // update garble scrore
        this.GarbleLevel = this.selectedGagTypes.Sum(gagType => this.GagTypes[gagType]);
        // initialize save service
        _saveService.DelaySave(this);
    }

    /// <summary> 
    /// Loads the config from our save service and migrates it if necessary.
    /// <list type="bullet">
    /// <item><c>migrator</c><param name="migrator"> - The config migrator.</param></item>
    /// </list> </summary>
    /// <returns>The migrated config.</returns>
    public void Load(ConfigMigrationService migrator) {
        // Handle deserialization errors
        static void HandleDeserializationError(object? sender, ErrorEventArgs errorArgs) {
            GagSpeak.Log.Error( $"[Config]: Error parsing Configuration at {errorArgs.ErrorContext.Path}, using default or migrating:\n{errorArgs.ErrorContext.Error}");
            errorArgs.ErrorContext.Handled = true;
        }
        // If the config file does not exist, return
        if (!File.Exists(_saveService.FileNames.ConfigFile))
            return;
        // Otherwise, load the config
        if (File.Exists(_saveService.FileNames.ConfigFile))
            // try to load the config
            try {
                var text = File.ReadAllText(_saveService.FileNames.ConfigFile);
                JsonConvert.PopulateObject(text, this, new JsonSerializerSettings {
                    Error = HandleDeserializationError,
                });
            }
            catch (Exception ex) {
                // If there is an error, log it and revert to default
                GagSpeak.Messager.NotificationMessage(ex,
                    "Error reading Configuration, reverting to default.\nYou may be able to restore your configuration using the rolling backups in the XIVLauncher/backups/GagSpeak directory.",
                    "Error reading Configuration", NotificationType.Error);
            }
        // Migrate the config
        migrator.Migrate(this);
    }

    /// <summary> 
    /// Gets the filename for the config file.
    /// <list type="bullet">
    /// <item><c>fileNames</c><param name="fileNames"> - The file names service.</param></item>
    /// </list> </summary>
    public string ToFilename(FilenameService fileNames)
        => fileNames.ConfigFile;

    /// <summary>
    /// Save the config to a file.
    /// <list type="bullet">
    /// <item><c>writer</c><param name="writer"> - The writer to write to.</param></item>
    /// </list> </summary>
    public void Save(StreamWriter writer) {
        using var jWriter    = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        var       serializer = new JsonSerializer { Formatting         = Formatting.Indented };
        serializer.Serialize(jWriter, this);
    }

    /// <summary> a very small class that gets the current version of the config save file. </summary>
    public static class Constants {
        public const int CurrentVersion = 4;
    }
    // embedded dictionary of gag types, not put into a seperate data file because i dont want to deal with doing that honestly.
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

// Enum for the gag padlocks
public enum GagPadlocks {
None,                   // No gag
MetalPadlock,           // Metal Padlock, can be picked
CombinationPadlock,     // Combination Padlock, must enter 4 digit combo to unlock
PasswordPadlock,        // Password Padlock, must enter password to unlock
FiveMinutesPadlock,     // 5 minute padlock, must wait 5 minutes to unlock
TimerPasswordPadlock,   // Timer Password Padlock, must enter password to unlock, but only after a certain amount of time
MistressPadlock,        // Mistress Padlock, must ask mistress to unlock
MistressTimerPadlock,   // Mistress Timer Padlock, must ask mistress to unlock, but only after a certain amount of time
};

#pragma warning restore IDE1006
