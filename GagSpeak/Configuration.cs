using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Configuration;
using OtterGui.Classes;
using OtterGui.Widgets;
using Penumbra.GameData.Enums;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using GagSpeak.Data;
using GagSpeak.UI;
using GagSpeak.Services;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
using GagSpeak.Garbler.PhonemeData;
using GagSpeak.CharacterData;

namespace GagSpeak;

/// <summary> The configuration for the GagSpeak plugin. </summary>
public class GagSpeakConfig : IPluginConfiguration, ISavable
{   
    // Plugin information
    public          ChangeLogDisplayType                        ChangeLogDisplayType { get; set; } = ChangeLogDisplayType.New;
    public          int                                         LastSeenVersion { get; set; } = GagSpeakChangelog.LastChangelogVersion; // look more into ottergui to figure out how to implement this later.
    public          int                                         Version { get; set; } = 0;                              // Version of the plugin
    public          bool                                        FreshInstall { get; set; } = true;                      // Is user on a fresh install?
    public          bool                                        Enabled { get; set; } = true;                           // Is plugin enabled?
    // additonal information below
    public          List<ChatChannel.ChatChannels>              ChannelsGagSpeak { get; set; }                          // Which channels are currently enabled / allowed?
    public          List<ChatChannel.ChatChannels>              ChannelsPuppeteer { get; set; }                         // Which channels are currently enabled / allowed?
    public          int                                         ProcessTranslationInterval { get; set; } = 300000;      // current process intervals for the history
    public          int                                         TranslationHistoryMax { get; set; } = 30;               // Gets or sets max number of translations stored in history
    public          MainWindow.TabType                          SelectedTab { get; set; } = MainWindow.TabType.General; // Default to the general tab
    public          bool                                        viewingRestraintCompartment { get; set; } = false;      // Is viewing the restraint shelf tab in wardrobe?
    public          string                                      sendInfoName = "";                                      // Name of the person you are sending info to
    public          bool                                        acceptingInfoRequests = true;                           // Are you accepting info requests? (for cooldowns)//
    public          Dictionary<string,DateTimeOffset>           timerData { get; set; }                                 // stores the timer data for the plugin
    // stuff for the gaglistingDrawer
    public          List<bool>                                  isLocked { get; set; }                                  // determines if the gaglisting should have its UI locked
    public          List<string>                                displaytext { get; set; }                               // stores the display time remaining for each locked gag in general tab
    public          List<PadlockIdentifier>                     padlockIdentifier { get; set; }                         // stores the padlock identifier for each gaglisting
    public          PadlockIdentifier                           whitelistPadlockIdentifier {get; set; }                 // stores the padlock identifier for the whitelist
    // stuff for the wardrobemanager
    public          GlamourerCharacterData                      cachedCharacterData { get; set; }
    public          bool                                        disableGlamChangeEvent { get; set; } = false;           // disables the glam change event
    public          bool                                        finishedDrawingGlamChange { get; set; } = false;        // disables the glamourer
    // stuff for the garbler
    public          string                                      language { get; set; } = "English";                     // The language dialect to use for the IPA conversion
    public          string                                      languageDialect { get; set; } = "IPA_US";               // The language dialect to use for the IPA conversion
    public          List<string>                                phoneticSymbolList;                                     // List of the phonetic symbols for the currently selected language
    // our main information for player character and whitelist characters
    public          PlayerCharacterInfo                         playerInfo { get; set; }                       // stores the player character info
    public          List<WhitelistedCharacterInfo>              whitelist { get; set; }                    // stores the whitelist character info
    
    [JsonIgnore]
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

        // set default values for selected channels/
        if (ChannelsGagSpeak == null || !ChannelsGagSpeak.Any()) {
            ChannelsGagSpeak = new List<ChatChannel.ChatChannels>(){ChatChannel.ChatChannels.Say};}
        // set default values for selected channels/
        if (ChannelsPuppeteer == null || !ChannelsPuppeteer.Any()) {
            ChannelsPuppeteer = new List<ChatChannel.ChatChannels>(){ChatChannel.ChatChannels.Say};}

        // set default values for isLocked
        if (this.isLocked == null || !this.isLocked.Any() || this.isLocked.Count > 3) {
            GagSpeak.Log.Debug($"[Config]: isLocked is null, creating new list");
            this.isLocked = new List<bool> { false, false, false };} //
        // set default values for displaytext
        if (this.displaytext == null || !this.displaytext.Any() || this.displaytext.Count > 3) {
            GagSpeak.Log.Debug($"[Config]: displaytext is null, creating new list");
            this.displaytext = new List<string> { "", "", "" };}
        // set default values for padlockIdentifier
        if (this.whitelistPadlockIdentifier == null) {
            GagSpeak.Log.Debug($"[Config]: whitelistPadlockIdentifier is null, creating new list");
            this.whitelistPadlockIdentifier = new PadlockIdentifier();}
        // set default for the padlock identifier listings
        if (this.padlockIdentifier == null || !this.padlockIdentifier.Any() || this.padlockIdentifier.Count > 3) {
            GagSpeak.Log.Debug($"[Config]: padlockIdentifier is null, creating new list");
            this.padlockIdentifier = new List<PadlockIdentifier> { new PadlockIdentifier(), new PadlockIdentifier(), new PadlockIdentifier() };}
        // set default values for the timer data
        if (this.timerData == null || !this.timerData.Any()) {
            GagSpeak.Log.Debug($"[Config]: timerData is null, creating new list");
            this.timerData = new Dictionary<string, DateTimeOffset>();}
        // set default values for the phonetic listings for the default language
        if (this.phoneticSymbolList == null || !this.phoneticSymbolList.Any()) {
            GagSpeak.Log.Debug($"[Config]: PhoneticRestrictions is null, creating new list");
            this.phoneticSymbolList = PhonemMasterLists.MasterListEN_US;}
        // & create new entry in dictionary for each one!
        // set default class for GlamourerCharacterData if none exists already. Will be removed later, used for debug purposes now.
        // if (this.cachedCharacterData == null) {
        //     GagSpeak.Log.Debug($"[Config]: cachedCharacterData is null, creating new list");
        this.cachedCharacterData = new GlamourerCharacterData();
        //}
            
        // finished!
        GagSpeak.Log.Debug("[Configuration File] Constructor Finished Initializing and setting default values, and previous data restored.");
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
                    // add our custom converter
                    Converters = { new EquipItemConverter() }
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

    /// <summary> Gets the filename for the config file. </summary>
    public string ToFilename(FilenameService fileNames)
        => fileNames.ConfigFile;

    /// <summary> Save the config to a file. </summary>
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