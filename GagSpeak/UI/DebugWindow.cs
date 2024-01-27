﻿using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui;
using GagSpeak.Services;
using GagSpeak.UI.ComboListings;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;
using GagSpeak.Garbler.Translator;
using GagSpeak.Events;
using Penumbra.GameData.Enums;
using System.Linq;
using Penumbra.GameData.Structs;
using Penumbra.GameData.Data;

namespace GagSpeak.UI;
/// <summary> This class is used to show the debug menu in its own window. </summary>
public class DebugWindow : Window //, IDisposable
{
    private          string?                _tempTestMessage;               // stores the input password for the test translation system
    private          string?                _translatedMessage = "";        // stores the translated message for the test translation system
    private          string?                _translatedMessageSpaced ="";   // stores the translated message for the test translation system
    private          string?                _translatedMessageOutput ="";   // stores the translated message for the test translation system
    private readonly FontService            _fontService;                   // for displaying the IPA symbols on the bottom chart
    private readonly GagService             _gagService;                    // for displaying the number of registered gags
    private readonly GagSpeakConfig         _config;                        // for retrieving the config data to display to the window
    private readonly ItemAutoEquipEvent     _itemAutoEquipEvent;            // for knowing when a gag is equipped
    private readonly IpaParserEN_FR_JP_SP   _translatorLanguage;            // creates an instance of the EnglishToIPA class
    private readonly GagManager             _gagManager;                    // for knowing what gags are equipped
    private readonly GagListingsDrawer      _gagListingsDrawer;             // for knowing the information in the currently equipped gags
    private readonly ItemData               _itemData;                      // for knowing the item data

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryWindow"/> class.
    /// </summary>
    public DebugWindow(DalamudPluginInterface pluginInt, FontService fontService, GagService gagService, IpaParserEN_FR_JP_SP translatorLanguage,
    GagSpeakConfig config, GagManager gagManager, GagListingsDrawer gagListingsDrawer, ItemAutoEquipEvent itemAutoEquipEvent, ItemData itemData) : base(GetLabel()) {
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;
        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(300, 400),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };
        _config = config;
        _itemAutoEquipEvent = itemAutoEquipEvent;
        _fontService = fontService;
        _gagService = gagService;
        _gagManager = gagManager;
        _gagListingsDrawer = gagListingsDrawer;
        _translatorLanguage = translatorLanguage;
        _itemData = itemData;
    }

    /// <summary> This function is used to draw the history window. </summary>
    public override void Draw() {
        DrawAdvancedGarblerInspector();
        DrawDebugInformationBasic();
        DrawDebugInformationWhitelistAndLocks();
        DrawPhoneticDebugInformation();
        DrawCachedCharacterInformation();
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakDebug###GagSpeakDebug";    


    /// <summary>
    /// Draws the advanced garbler inspector.
    /// </summary>
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
                GagSpeak.Log.Debug($"An error occurred while attempting to parse phonetics: {ex.Message}");
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

    /// <summary>
    /// Draws the debug information.
    /// </summary>
    public void DrawDebugInformationBasic() {
        if(!ImGui.CollapsingHeader("DEBUG INFORMATION")) { return; }
        // General plugin information
        ImGui.Text($"Fresh Install?: {_config.FreshInstall}");
        ImGui.Text($"In Dom Mode?: {_config.InDomMode}");
        ImGui.Text($"Safeword: {_config.Safeword}");
        ImGui.Text($"Has Safeword Been Used?: {_config.SafewordUsed}");
        ImGui.Text($"Process Translation Interval: {_config.ProcessTranslationInterval}");
        ImGui.Text($"Max Translation History: {_config.TranslationHistoryMax}");
        ImGui.Separator();
        // configuration tab options & details
        ImGui.Text($"Friends Only?: {_config.friendsOnly}");
        ImGui.Text($"Party Only?: {_config.partyOnly}");
        ImGui.Text($"Whitelist Only?: {_config.whitelistOnly}");
        ImGui.Text($"In DirectChatGarbler Mode?: {_config.DirectChatGarbler}");
        ImGui.Text($"Selected Language: {_config.language}");
        ImGui.Text($"Selected Dialect: {_config.languageDialect}");
        ImGui.Text($"Translatable Chat Types:");
        foreach (var chanel in _config.Channels) { ImGui.SameLine(); ImGui.Text($"{chanel.ToString()}, "); };
        ImGui.Text($"Current ChatBox Channel: {ChatChannel.GetChatChannel()}");
        ImGui.Text($"Player Current Requesting Info: {_config.sendInfoName}");
        ImGui.Text($"Ready To Accept sending player information?: {_config.acceptingInfoRequests}");
        ImGui.Separator();
        // wardrobe details
        ImGui.Text($"Enable Wardrobe?: {_config.enableWardrobe}");
        ImGui.Text($"Enable Item Auto-Equip?: {_config.allowItemAutoEquip}");
        ImGui.Text($"Allow Restraint Locking?: {_config.allowRestraintLocking}");
        ImGui.Text($"Surrender Restraint Control?: {_config.surrenderRestraintControl}");
        // Gag details
        ImGui.Separator();
        ImGui.Text($"Total Gag List Count: {_gagService._gagTypes.Count}");
        ImGui.Text("Selected GagTypes: ||"); ImGui.SameLine(); foreach (var gagType in _config.selectedGagTypes) { ImGui.SameLine(); ImGui.Text($"{gagType} ||"); };
        ImGui.Text("Selected GagPadlocks: ||"); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in _config.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} ||");};
        ImGui.Text("Selected GagPadlocks Passwords: ||"); ImGui.SameLine(); foreach (var gagPadlockPassword in _config.selectedGagPadlocksPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} ||"); };
        ImGui.Text("Selected GagPadlock Timers: ||"); ImGui.SameLine(); foreach (var gagPadlockTimer in _config.selectedGagPadLockTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} ||"); };
        ImGui.Text("Selected GagPadlocks Assigners: ||"); ImGui.SameLine(); foreach (var gagPadlockAssigner in _config.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} ||"); };
    }

    public void DrawDebugInformationWhitelistAndLocks() {
        if(!ImGui.CollapsingHeader("Whitelist & Locks Info")) { return; }
        // Whitelist uder information
        ImGui.Text("Whitelist:"); ImGui.Indent();
        foreach (var whitelistPlayerData in _config.Whitelist) {
            ImGui.Text(whitelistPlayerData.name);
            ImGui.Indent();
            ImGui.Text($"Relationship to this Player: {whitelistPlayerData.relationshipStatus}");
            ImGui.Text($"Relationship to You: {whitelistPlayerData.relationshipStatusToYou}");
            ImGui.Text($"Commitment Duration: {whitelistPlayerData.GetCommitmentDuration()}");
            ImGui.Text($"Locked Live Chat Garbler: {whitelistPlayerData.lockedLiveChatGarbler}");
            ImGui.Text($"Pending Relationship Request From You: {whitelistPlayerData.PendingRelationRequestFromYou}");
            ImGui.Text($"Pending Relationship Request: {whitelistPlayerData.PendingRelationRequestFromPlayer}");
            ImGui.Text($"Selected GagTypes: || "); ImGui.SameLine(); foreach (var gagType in whitelistPlayerData.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
            ImGui.Text($"Selected GagPadlocks: || "); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in whitelistPlayerData.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} || ");};
            ImGui.Text($"Selected GagPadlocks Passwords: || "); ImGui.SameLine(); foreach (var gagPadlockPassword in whitelistPlayerData.selectedGagPadlocksPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} || "); };
            ImGui.Text($"Selected GagPadlocks Timers: || "); ImGui.SameLine(); foreach (var gagPadlockTimer in whitelistPlayerData.selectedGagPadlocksTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} || "); };
            ImGui.Text($"Selected GagPadlocks Assigners: || "); ImGui.SameLine(); foreach (var gagPadlockAssigner in whitelistPlayerData.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} || "); };
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
            GagSpeak.Log.Error($"Error while fetching config in debug: {e}");
        }
    }

    /// <summary>
    /// Draws the cached character information.
    /// </summary>
    public void DrawCachedCharacterInformation() {
        if(!ImGui.CollapsingHeader("Cached Character Information")) { return; }
        // draw all the design data we need to know
        using var table = ImRaii.Table("##equip", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table) { return; }
        ImGui.TableNextRow();
        // temp creation for equipment class
        var properties = typeof(Equipment).GetProperties().Where(p => p.Name != "Hat" && p.Name != "Visor" && p.Name != "Weapon").ToArray();
        // display equipment slots:
        var equipSlots = EquipSlotExtensions.EqdpSlots.Prepend(EquipSlot.OffHand).Prepend(EquipSlot.MainHand);
        for (int i = 0; i < properties.Length; i++) {
            try{
                // get our property
                var property = properties[i];
                // get the value of the property
                dynamic? slot = property.GetValue(_config.cachedCharacterData.Equipment);
                // get rest of the objects values
                EquipSlot equipslot = equipSlots.ElementAt(i);
                CustomItemId itemId = slot?.ItemId;
                int stain = slot?.Stain;
                bool apply = slot?.Apply;
                bool applyStain = slot?.ApplyStain;
                EquipItem temp = Resolve(equipslot, itemId);
                // draw them outttt
                ImGuiUtil.DrawTableColumn(((EquipmentSlotNameByEnum)i).ToString()); // the slot name
                ImGuiUtil.DrawTableColumn(temp.Name); // the item name
                ImGuiUtil.DrawTableColumn(itemId.ToString());
                ImGuiUtil.DrawTableColumn(stain.ToString());
                ImGuiUtil.DrawTableColumn(apply ? "Apply" : "Keep");
                ImGuiUtil.DrawTableColumn(applyStain ? "Apply" : "Keep");

            } catch (Exception e) {
                GagSpeak.Log.Error($"Error while slot property in character print in debug: {e}");
            }
        }
        ImGuiUtil.DrawTableColumn("Hat Visible");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Equipment.Hat.Show}");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Equipment.Hat.Apply}");
        ImGui.TableNextRow();
        ImGuiUtil.DrawTableColumn("Visor Toggled");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Equipment.Visor.IsToggled}");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Equipment.Visor.Apply}");
        ImGui.TableNextRow();
        ImGuiUtil.DrawTableColumn("Weapon Visible");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Equipment.Weapon.Show}");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Equipment.Weapon.Apply}");
        ImGui.TableNextRow();
        // customization
        ImGuiUtil.DrawTableColumn("Model ID");
        ImGuiUtil.DrawTableColumn(_config.cachedCharacterData.Customize.ModelId.ToString());
        ImGui.TableNextRow();

        foreach (var index in Enum.GetValues<CustomizeIndex>())
        {
            if (index.ToString() == "ModelId") continue; // Skip ModelId

            var property = typeof(Customize).GetProperty(index.ToString());
            dynamic? value = property?.GetValue(_config.cachedCharacterData.Customize);
            int valueInt = value?.Value;
            bool apply = value?.Apply;

            ImGuiUtil.DrawTableColumn(index.ToString());
            ImGuiUtil.DrawTableColumn(valueInt.ToString());
            ImGuiUtil.DrawTableColumn(apply ? "Apply" : "Keep");
            ImGui.TableNextRow();
        }

        ImGuiUtil.DrawTableColumn("Is Wet");
        ImGuiUtil.DrawTableColumn($"{_config.cachedCharacterData.Customize.Wetness.Value}");
        ImGui.TableNextRow();
        // finished debugging
    }

    public EquipItem Resolve(EquipSlot slot, CustomItemId itemId)
    {
        slot = slot.ToSlot();
        if (itemId == ItemIdVars.NothingId(slot))
            return ItemIdVars.NothingItem(slot);
        if (itemId == ItemIdVars.SmallclothesId(slot))
            return ItemIdVars.SmallClothesItem(slot);
        if (!itemId.IsItem || !_itemData.TryGetValue(itemId.Item, slot, out var item))
            return EquipItem.FromId(itemId);

        if (item.Type.ToSlot() != slot)
            return new EquipItem(string.Intern($"Invalid #{itemId}"), itemId, item.IconId, item.PrimaryId, item.SecondaryId, item.Variant, 0, 0,
                0,
                0);
        return item;
    }


}

