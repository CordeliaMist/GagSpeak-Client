﻿using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using GagSpeak.ChatMessages;
using GagSpeak.Services;
using GagSpeak.Utility;
using GagSpeak.Garbler.Translator;
using GagSpeak.Events;
using GagSpeak.Gagsandlocks;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.CharacterData;
using GagSpeak.Wardrobe;
using GagSpeak.Hardcore;
using System.Linq;
using GagSpeak.ToyboxandPuppeteer;
using System.Collections.Generic;

namespace GagSpeak.UI;
// probably can remove this later, atm it is literally just used for the debug window
public enum EquipmentSlotNameByEnum {
    MainHand,
    OffHand,
    Head,
    Body,
    Hands,
    Belt,
    Legs,
    Feet,
    Ears,
    Neck,
    Wrists,
    RFinger,
    BothHand,
    LFinger,
}
/// <summary> This class is used to show the debug menu in its own window. </summary>
public class DebugWindow : Window //, IDisposable
{
    private          GagSpeakConfig         _config;                        // for retrieving the config data to display to the window
    private readonly CharacterHandler       _characterHandler;
    private readonly HardcoreManager        _hcManager;               // for knowing the information in the hardcore manager
    private readonly RestraintSetManager    _restraintSetManager;           // for knowing the information in the currently equipped restraints
    private readonly IpaParserEN_FR_JP_SP   _translatorLanguage;            // creates an instance of the EnglishToIPA class
    private readonly GagGarbleManager       _gagManager;                    // for knowing what gags are equipped
    private readonly GagListingsDrawer      _gagListingsDrawer;             // for knowing the information in the currently equipped gags
    private readonly FontService            _fontService;                   // for displaying the IPA symbols on the bottom chart
    private readonly GagService             _gagService;                    // for displaying the number of registered gags
    private readonly PatternHandler         _patternHandler;
    private readonly GagSpeakGlamourEvent   _gagSpeakGlamourEvent;          // for knowing if the glamour event is executing
    private          string?                _tempTestMessage;               // stores the input password for the test translation system
    private          string?                _tempPatternStorage;            // temporarily stores the pattern of the string.
    private          string?                _outputString = "";             // converted pattern output
    private          string[]?              _splitInput;
    private          List<byte>             convertedPatternData; // temp pattern data
    private          string?                _translatedMessage = "";        // stores the translated message for the test translation system
    private          string?                _translatedMessageSpaced ="";   // stores the translated message for the test translation system
    private          string?                _translatedMessageOutput ="";   // stores the translated message for the test translation system

    public DebugWindow(DalamudPluginInterface pluginInt, FontService fontService, GagService gagService,
    RestraintSetManager restraintSetManager, IpaParserEN_FR_JP_SP translatorLanguage, GagSpeakConfig config,
    CharacterHandler characterHandler, GagSpeakGlamourEvent gagSpeakGlamourEvent, GagGarbleManager GagGarbleManager,
    GagListingsDrawer gagListingsDrawer, HardcoreManager hardcoreManager, PatternHandler patternHandler) : base(GetLabel()) {
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;
        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(300, 400),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };
        _config = config;
        _characterHandler = characterHandler;
        _fontService = fontService;
        _gagService = gagService;
        _gagManager = GagGarbleManager;
        _gagListingsDrawer = gagListingsDrawer;
        _translatorLanguage = translatorLanguage;
        _restraintSetManager = restraintSetManager;
        _gagSpeakGlamourEvent = gagSpeakGlamourEvent;
        _hcManager = hardcoreManager;
        _patternHandler = patternHandler;

        convertedPatternData = new List<byte>();
    }


    public override void Draw() {
        ImGui.Text($"IsGagSpeakGlamourEventExecuting: {_gagSpeakGlamourEvent.IsGagSpeakGlamourEventExecuting}");
        ImGui.Text($"restraint set active index: {_restraintSetManager._selectedIdx}");
        ImGui.Text($"whitelist active index: {_characterHandler.activeListIdx}");
        // add a button to reset faulty request info's
        ImGui.Separator();
        ImGui.Text($"Send Info Name: {_config.sendInfoName}");
        ImGui.Text($"Accepting Info Requests: {_config.acceptingInfoRequests}");
        ImGui.Text($"Processing Info Request: {_config.processingInfoRequest}");
        if (ImGui.Button("Reset Faulty Request Info")) {
            _config.SetSendInfoName("");
            _config.SetAcceptInfoRequests(true);
            _config.SetprocessingInfoRequest(false);
        }
        DrawPlayerCharInfo();
        DrawWhitelistCharactersAndLocks();
        DrawRestraintSetOverview();
        DrawAdvancedGarblerInspector();
        DrawPhoneticDebugInformation();
        DrawLovensePatternConverter();
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakDebug###GagSpeakDebug";    

    /// <summary> Draws the debug information. Needs a serious Massive overhaul </summary>
    public void DrawPlayerCharInfo() {
        if(!ImGui.CollapsingHeader("PLAYER INFORMATION")) { return; }
        // General plugin information
        ImGui.Text($"Fresh Install?: {_config.FreshInstall}");
        ImGui.Text($"Safeword: {_characterHandler.playerChar._safeword}");
        ImGui.Text($"Has Safeword Been Used?: {_characterHandler.playerChar._safewordUsed}");
        ImGui.Text($"Selected Language: {_config.language}");
        ImGui.Text($"Selected Dialect: {_config.languageDialect}");
        ImGui.Separator();
        ImGui.Text($"Allow Commands from Friends?: {_characterHandler.playerChar._doCmdsFromFriends}");
        ImGui.Text($"Allow Commands from Party Members?: {_characterHandler.playerChar._doCmdsFromParty}");
        ImGui.Text($"Direct Chat Garbler Active: {_characterHandler.playerChar._directChatGarblerActive}");
        ImGui.Text($"Direct Chat Garbler Locked: {_characterHandler.playerChar._directChatGarblerLocked}");
        ImGui.Text($"Live Garbler Warning on Zone Change: {_characterHandler.playerChar._liveGarblerWarnOnZoneChange}");
        ImGui.Text($"Translatable Chat Types:");
        foreach (var chanel in _config.ChannelsGagSpeak) { ImGui.SameLine(); ImGui.Text($"{chanel.ToString()}, "); };
        ImGui.Text($"Current ChatBox Channel: {ChatChannel.GetChatChannel()}");
        ImGui.Text($"Player Current Requesting Info: {_config.sendInfoName}");
        ImGui.Text($"Ready To Accept sending player information?: {_config.acceptingInfoRequests}");
        ImGui.Text($"Processing Info Request?: {_config.processingInfoRequest}");
        ImGui.Separator();
        ImGui.Text($"Enable Wardrobe?: {_characterHandler.playerChar._enableWardrobe}");
        ImGui.Text($"Enable Item Auto-Equip?: {_characterHandler.playerChar._allowItemAutoEquip}");
        ImGui.Text($"Allow Restraint Locking?: {_characterHandler.playerChar._allowRestraintSetAutoEquip}");
        ImGui.Text($"Lock Gag Storage on Gag Lock: {_characterHandler.playerChar._lockGagStorageOnGagLock}");
        ImGui.Text($"Enable Restraint Sets: {_characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._enableRestraintSets}");
        ImGui.Text($"Restraint Set Locking: {_characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._restraintSetLocking}");
        ImGui.Separator();
        ImGui.Text($"Enable Toybox: {_characterHandler.playerChar._enableToybox}");
        ImGui.Text($"Allow Intensity Locking: {_characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowIntensityControl}");
        ImGui.Text($"Intensity Level: {_characterHandler.playerChar._intensityLevel}");
        ImGui.Text($"Allow Toybox Locking: {_characterHandler.playerChar._lockToyboxUI}");
        ImGui.Separator();
        ImGui.Text($"Total Gag List Count: {_gagService._gagTypes.Count}");
        ImGui.Text("Selected GagTypes: ||"); ImGui.SameLine(); foreach (var gagType in _characterHandler.playerChar._selectedGagTypes) { ImGui.SameLine(); ImGui.Text($"{gagType} ||"); };
        ImGui.Text("Selected Padlocks: ||"); ImGui.SameLine(); foreach (Padlocks gagPadlock in _characterHandler.playerChar._selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} ||");};
        ImGui.Text("Selected Padlocks Passwords: ||"); ImGui.SameLine(); foreach (var gagPadlockPassword in _characterHandler.playerChar._selectedGagPadlockPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} ||"); };
        ImGui.Text("Selected GagPadlock Timers: ||"); ImGui.SameLine(); foreach (var gagPadlockTimer in _characterHandler.playerChar._selectedGagPadlockTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} ||"); };
        ImGui.Text("Selected Padlocks Assigners: ||"); ImGui.SameLine(); foreach (var gagPadlockAssigner in _characterHandler.playerChar._selectedGagPadlockAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} ||"); };
        ImGui.Separator();
        var triggerlist = _characterHandler.playerChar._triggerAliases[_characterHandler.activeListIdx];
        ImGui.Text($"Trigger Aliases: || "); ImGui.SameLine(); foreach (var alias in triggerlist._aliasTriggers) { ImGui.Text(alias._inputCommand); };
        ImGui.Separator();
        
    }

    public void DrawRestraintSetOverview() {
        if(!ImGui.CollapsingHeader("Restraint Set Information")) { return; }

        foreach (var restraintSet in _restraintSetManager._restraintSets)
        {
            ImGui.Text($"Restraint Set:");
            ImGui.Text($"Name: {restraintSet._name}");
            ImGui.Text($"Description: {restraintSet._description}");
            ImGui.Text($"Enabled: {restraintSet._enabled}");
            ImGui.Text($"Locked: {restraintSet._locked}");
            ImGui.Text($"Locked By: {restraintSet._wasLockedBy}");
            ImGui.Text($"Locked Timer: {UIHelpers.FormatTimeSpan(restraintSet._lockedTimer - DateTimeOffset.Now)}");

            ImGui.Text("Draw Data:");
            ImGui.Indent();
            foreach (var pair in restraintSet._drawData)
            {
                var equipDrawData = pair.Value;
                ImGui.Text($"Equip Slot: {pair.Key}");
                ImGui.Text($"Is Enabled: {equipDrawData._isEnabled}");
                ImGui.Text($"Equipped By: {equipDrawData._wasEquippedBy}");
                ImGui.Text($"Locked: {equipDrawData._locked}");
                ImGui.Text($"Active Slot List Index: {equipDrawData._activeSlotListIdx}");
                ImGui.Text($"Slot: {equipDrawData._slot}");
                ImGui.Text($"Game Item: {equipDrawData._gameItem}");
                ImGui.Text($"Game Stain: {equipDrawData._gameStain}");
                ImGui.Text($"-----------------------------------------");
            }
            ImGui.Unindent();
            ImGui.Separator();
        }
    }

    public void DrawWhitelistCharactersAndLocks() {
        if(!ImGui.CollapsingHeader("Whitelist & Locks Info")) { return; }
        // Whitelist uder information
        ImGui.Text("Whitelist:"); ImGui.Indent();
        foreach (var whitelistPlayerData in _characterHandler.whitelistChars) {
            ImGui.Text(whitelistPlayerData._charNAW[whitelistPlayerData._charNAWIdxToProcess]._name);
            ImGui.Indent();
            ImGui.Text($"Relationship to this Player: {whitelistPlayerData._yourStatusToThem}");
            ImGui.Text($"Relationship to You: {whitelistPlayerData._theirStatusToYou}");
            ImGui.Text($"Commitment Duration: {whitelistPlayerData.GetCommitmentDuration()}");
            ImGui.Text($"Locked Live Chat Garbler: {whitelistPlayerData._directChatGarblerLocked}");
            ImGui.Text($"Pending Relationship Request From You: {whitelistPlayerData._pendingRelationRequestFromYou}");
            ImGui.Text($"Pending Relationship Request: {whitelistPlayerData._pendingRelationRequestFromPlayer}");
            ImGui.Text($"Selected GagTypes: || "); ImGui.SameLine(); foreach (var gagType in whitelistPlayerData._selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
            ImGui.Text($"Selected Padlocks: || "); ImGui.SameLine(); foreach (Padlocks gagPadlock in whitelistPlayerData._selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} || ");};
            ImGui.Text($"Selected Padlocks Passwords: || "); ImGui.SameLine(); foreach (var gagPadlockPassword in whitelistPlayerData._selectedGagPadlockPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} || "); };
            ImGui.Text($"Selected Padlocks Timers: || "); ImGui.SameLine(); foreach (var gagPadlockTimer in whitelistPlayerData._selectedGagPadlockTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} || "); };
            ImGui.Text($"Selected Padlocks Assigners: || "); ImGui.SameLine(); foreach (var gagPadlockAssigner in whitelistPlayerData._selectedGagPadlockAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} || "); };
            ImGui.Unindent();
        }
        ImGui.Unindent();
        ImGui.Separator();

        // Padlock identifier Information
        ImGui.Text("Padlock Identifiers Variables:");
        // output debug messages to display the gaglistingdrawers boolean list for _islocked, _adjustDisp. For each padlock identifer, diplay all of its public varaibles
        ImGui.Text($"Listing Drawer isLocked: ||"); ImGui.SameLine(); foreach(var index in _config.isLocked) { ImGui.SameLine(); ImGui.Text($"{index} ||"); };
        ImGui.Text($"Listing Drawer _adjustDisp: ||"); ImGui.SameLine(); foreach(var index in _gagListingsDrawer._adjustDisp) { ImGui.SameLine(); ImGui.Text($"{index} ||"); };
        var width = ImGui.GetContentRegionAvail().X / 3;
        foreach(var index in _config.padlockIdentifier) {
            ImGui.Columns(3,"DebugColumns", true);
            ImGui.SetColumnWidth(0,width); ImGui.SetColumnWidth(1,width); ImGui.SetColumnWidth(2,width);
            ImGui.Text($"Input Password: {index._inputPassword}"); ImGui.NextColumn();
            ImGui.Text($"Input Combination: {index._inputCombination}"); ImGui.NextColumn();
            ImGui.Text($"Input Timer: {index._inputTimer}");ImGui.NextColumn();
            ImGui.Text($"Stored Password: {index._storedPassword}");ImGui.NextColumn();
            ImGui.Text($"Stored Combination: {index._storedCombination}");ImGui.NextColumn();
            ImGui.Text($"Stored Timer: {index._storedTimer}");ImGui.NextColumn();
            ImGui.Text($"Padlock Type: {index._padlockType}");ImGui.NextColumn();
            ImGui.Text($"Padlock Assigner: {index._mistressAssignerName}");ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.NewLine();
        } // This extra one is just the whitelist padlock stuff
        ImGui.Columns(3,"DebugColumns", true);
        ImGui.SetColumnWidth(0,width); ImGui.SetColumnWidth(1,width); ImGui.SetColumnWidth(2,width);
        ImGui.Text($"Input Password: {_config.whitelistPadlockIdentifier._inputPassword}"); ImGui.NextColumn();
        ImGui.Text($"Input Combination: {_config.whitelistPadlockIdentifier._inputCombination}"); ImGui.NextColumn();
        ImGui.Text($"Input Timer: {_config.whitelistPadlockIdentifier._inputTimer}");ImGui.NextColumn();
        ImGui.Text($"Stored Password: {_config.whitelistPadlockIdentifier._storedPassword}");ImGui.NextColumn();
        ImGui.Text($"Stored Combination: {_config.whitelistPadlockIdentifier._storedCombination}");ImGui.NextColumn();
        ImGui.Text($"Stored Timer: {_config.whitelistPadlockIdentifier._storedTimer}");ImGui.NextColumn();
        ImGui.Text($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");ImGui.NextColumn();
        ImGui.Text($"Padlock Assigner: {_config.whitelistPadlockIdentifier._mistressAssignerName}");ImGui.NextColumn();
        ImGui.Columns(1);
    }


    /// <summary> Draws the advanced garbler inspector. </summary>
    public void DrawAdvancedGarblerInspector() {
        // create a collapsing header for this.
        if(!ImGui.CollapsingHeader("Advanced Garbler Debug Testing")) { return; }
        // create a input text field here, that stores the result into a string. On the same line, have a button that says garble message. It should display the garbled message in text on the next l
        var testMessage  = _tempTestMessage ?? ""; // temp storage to hold until we de-select the text input
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X/2);
        if (ImGui.InputText("##GarblerTesterField", ref testMessage, 400, ImGuiInputTextFlags.None))
            _tempTestMessage = testMessage;

        ImGui.SameLine();
        if (ImGui.Button("Garble Message")) {
            // Use the EnglishToIPA instance to translate the message
            try {
                _translatedMessage       = _translatorLanguage.ToIPAStringDisplay(testMessage);
                _translatedMessageSpaced = _translatorLanguage.ToIPAStringSpacedDisplay(testMessage);
                _translatedMessageOutput = _gagManager.ProcessMessage(testMessage);
            } catch (Exception ex) {
                GSLogger.LogType.Debug($"An error occurred while attempting to parse phonetics: {ex.Message}");
            }
        }
        // DISPLAYS THE ORIGINAL MESSAGE STRING
        ImGui.Text($"Original Message: {testMessage}");
        // DISPLAYS THE IPA PARSED DEFINED MESSAGE DISPLAY
        ImGui.Text("Decoded Message: "); ImGui.SameLine();
        UIHelpers.FontText($"{_translatedMessage}", _fontService.UidFont);
        // DISPLAYS THE DECODED MESSAGE SPACED
        ImGui.Text("Decoded Message: "); ImGui.SameLine();
        UIHelpers.FontText($"{_translatedMessageSpaced}", _fontService.UidFont);   
        // DISPLAYS THE OUTPUT STRING 
        ImGui.Text("Output Message: "); ImGui.SameLine();
        UIHelpers.FontText($"{_translatedMessageOutput}", _fontService.UidFont);
        // DISPLAYS THE UNIQUE SYMBOLS FOR CURRENT LANGUAGE DIALECT
        string uniqueSymbolsString = _translatorLanguage.uniqueSymbolsString;
        ImGui.PushFont(_fontService.UidFont);
        ImGui.Text($"Unique Symbols for {_config.language} with dialect {_config.languageDialect}: ");
        ImGui.InputText("##UniqueSymbolsField", ref uniqueSymbolsString, 128, ImGuiInputTextFlags.ReadOnly);
        ImGui.PopFont();
    }

    public void DrawPhoneticDebugInformation() {
        if(!ImGui.CollapsingHeader("Phonetic Debug Information")) { return; }
        var width = ImGui.GetContentRegionAvail().X / 3;
        ImGui.Text("Gag Manager Information:");
        // define the columns and the gag names
        ImGui.Columns(3, "GagColumns", true);
        ImGui.SetColumnWidth(0, width); ImGui.SetColumnWidth(1, width); ImGui.SetColumnWidth(2, width);
        ImGui.Text($"Gag Name: {_gagManager._activeGags[0]._gagName}"); ImGui.NextColumn();
        ImGui.Text($"Gag Name: {_gagManager._activeGags[1]._gagName}"); ImGui.NextColumn();
        ImGui.Text($"Gag Name: {_gagManager._activeGags[2]._gagName}"); ImGui.NextColumn();
        try {
        ImGui.PushFont(_fontService.UidFont);
        foreach (var gag in _gagManager._activeGags) {
            // Create a table for the relations manager
            using (var table = ImRaii.Table($"InfoTable_{gag._gagName}", 3, ImGuiTableFlags.RowBg)) {
                if (!table) { return; }
                // Create the headers for the table
                ImGui.TableSetupColumn("Symbol", ImGuiTableColumnFlags.WidthFixed, width/4);
                ImGui.TableSetupColumn("Strength", ImGuiTableColumnFlags.WidthFixed, width/3);
                ImGui.TableSetupColumn("Sound", ImGuiTableColumnFlags.WidthFixed, width/4);
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.Text("Symbol"); ImGui.TableNextColumn();
                ImGui.Text("Strength"); ImGui.TableNextColumn();
                ImGui.Text("Sound"); ImGui.TableNextColumn();
                foreach (var phoneme in gag._muffleStrOnPhoneme){
                    ImGui.Text($"{phoneme.Key}"); ImGui.TableNextColumn();
                    ImGui.Text($"{phoneme.Value}"); ImGui.TableNextColumn();
                    ImGui.Text($"{gag._ipaSymbolSound[phoneme.Key]}"); ImGui.TableNextColumn();
                }
            } // table ends here
            ImGui.NextColumn();
        }
        ImGui.Columns(1);
        ImGui.PopFont();
        } catch (Exception e) {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching config in debug: {e}");
            ImGui.NewLine();
            GSLogger.LogType.Error($"Error while fetching config in debug: {e}");
        }
    }

    public string convertedPatternDuration = "";
    public void DrawLovensePatternConverter() {
        if(!ImGui.CollapsingHeader("Lovense Pattern Converted INFORMATION")) { return; }
        // input for the text to convert
        var lovenseFormattedPattern  = _tempPatternStorage ?? ""; // temp storage to hold until we de-select the text input
        ImGui.Text("Input Lovense Downloaded Pattern Data Here:");
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint("##LovensePatternConverter", "Input Lovense Pattern Here", ref lovenseFormattedPattern, 500000, ImGuiInputTextFlags.None))
        {
            _tempPatternStorage = lovenseFormattedPattern;
        }
        if(ImGui.IsItemDeactivatedAfterEdit()) {
            _tempPatternStorage = lovenseFormattedPattern;
            _splitInput = Array.Empty<string>(); // clear it then make the new one
            _splitInput = lovenseFormattedPattern.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }
        ImGui.Spacing();
        ImGui.Text($"Element Count: {_splitInput?.Length ?? 0}");
        ImGui.Spacing();
        // if we press the convert button, convert it
        if(ImGui.Button("Convert Pattern")) {
            _splitInput = lovenseFormattedPattern.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            GSLogger.LogType.Information($"Converting Lovense Pattern to GagSpeak Pattern");
            // clear the lists
            convertedPatternData.Clear();
            for (int i = 0; i < _splitInput.Length; i++)
            {
                // get the current number and the one after
                byte number = byte.Parse(_splitInput[i]);
                byte nextNumber = i < _splitInput.Length - 1 ? byte.Parse(_splitInput[i+1]) : number;
                // append the data * 5 with easing
                for(int j = 0; j < 5; j++)
                {
                    byte intropolatedNumber = (byte)(number*5 + (nextNumber*5 - number*5) * j / 5);
                    convertedPatternData.Add(intropolatedNumber);
                }
            }
            GSLogger.LogType.Information($"Converted Lovense Pattern to GagSpeak Pattern, pairing with commas now.\nArray has {convertedPatternData.Count} elements.");
            // set the output string
            _outputString = string.Join(",", convertedPatternData);
            GSLogger.LogType.Information($"Converted Lovense Pattern to GagSpeak Pattern");

            int totalMilliseconds = convertedPatternData.Count * 20;
            TimeSpan duration = TimeSpan.FromMilliseconds(totalMilliseconds);
            convertedPatternDuration = string.Format("{0}h{1:D2}m{2:D2}s", duration.Hours, duration.Minutes, duration.Seconds);
        }

        // display the output string
        ImGui.InputText("##PatternOutput", ref _outputString, 500000, ImGuiInputTextFlags.None);


        if(ImGui.Button("Create New Pattern From Data")) {
            PatternData newPattern = new PatternData();
            newPattern._name = "Converted Lovense Pattern";
            newPattern._description = "Converted Lovense Pattern From GagSpeak Debug";
            // get duration and add it
            int totalMilliseconds = convertedPatternData.Count * 20;
            TimeSpan duration = TimeSpan.FromMilliseconds(totalMilliseconds);
            newPattern._duration = string.Format("{0}h{1:D2}m{2:D2}s", duration.Hours, duration.Minutes, duration.Seconds);
            // add pattern
            List<byte> newPatternData = new List<byte>(convertedPatternData);
            newPattern._patternData = newPatternData;
            _patternHandler.AddNewPattern(newPattern); // Add the new pattern to the list
            GSLogger.LogType.Information($"Added new pattern to the list");
        }
        ImGui.Spacing();
        ImGui.Text($"Converted Element Count: {convertedPatternData.Count}");
        ImGui.Spacing();
        ImGui.Text($"Converted Pattern Duration: {convertedPatternDuration}");
    }
}

