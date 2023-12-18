using System;
using System.Collections.Generic;
using System.Linq; // For enabling lists
using System.IO;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Configuration;
using OtterGui.Classes;
using OtterGui.Widgets;
using GagSpeak.Chat;
using GagSpeak.Data;
using GagSpeak.UI;
using GagSpeak.Services;
using GagSpeak.Events;
using GagSpeak.Translator;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

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
    public          bool                                ExperimentalGarbler { get; set; } = false;               // Is experimental garbler enabled?
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
    // stuff for the garbler
    public          string                              language { get; set; } = "English";                     // The language dialect to use for the IPA conversion
    public          string                              languageDialect { get; set; } = "IPA_US";               // The language dialect to use for the IPA conversion
    public          List<PhoneticSymbol>                phoneticSymbolList;                                     // List of the phonetic symbols for the currently selected language
    // variables involved with saving and updating the config
    private readonly SaveService            _saveService;                                                       // Save service for the GagSpeak plugin
    
    
    /// <summary> Gets or sets the colors used within our UI </summary>
    public Dictionary<ColorId, uint> Colors { get; private set; }
        = Enum.GetValues<ColorId>().ToDictionary(c => c, c => c.Data().DefaultColor);

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
        // set default values for the phonetic listings for the default language
        if (this.phoneticSymbolList == null || !this.phoneticSymbolList.Any()) {
            GagSpeak.Log.Debug($"[Config]: PhoneticRestrictions is null, creating new list");
            this.phoneticSymbolList = Data.PhonemMasterLists.MasterListEN_US;}
    }

    /// <summary> Saves the config to our save service and updates the garble level to its new value. </summary>
    public void Save() {
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
}