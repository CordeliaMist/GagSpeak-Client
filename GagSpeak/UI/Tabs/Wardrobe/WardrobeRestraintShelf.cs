using System;
using System.Numerics;
using System.IO;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using Dalamud.Plugin;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using System.Collections.Generic;
using System.Linq;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;
using GagSpeak.Data;
using GagSpeak.Interop;
using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.ComboListings;
using System.Runtime.CompilerServices;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Dalamud.Interface.Utility;
using Penumbra.GameData.DataContainers;
using Penumbra.GameData.Data;
using Dalamud.Interface.Internal.Windows.StyleEditor;
using OtterGui;

namespace GagSpeak.Wardrobe;

/// <summary> Stores the UI for the restraints shelf of the kink wardrobe. </summary>
public class WardrobeRestraintCompartment
{
    private const float DefaultWidth = 280;
    private readonly    UiBuilder                       _uiBuilder;             // for loading images
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    FontService                     _fontService;           // for getting the font service
    private readonly    IDataManager                    _gameData;              // for getting the game data
    private readonly    TextureService                  _textures;              // for getting the textures
    private readonly    TimerService                    _timerService;          // for getting the timer service
    private readonly    ISavable                        _saveService;           // for getting the save service
    private readonly    FilenameService                 _filenameService;       // for getting the filename service
    private readonly    RestraintSetManager             _restraintSetManager;   // for getting the restraint set manager
    // variables
    private readonly    Vector2                         _iconSize;              // size of icons that can display
    private readonly    float                           _comboLength;           // length of combo boxes
    private readonly    int                             _listLength;            // length of list boxes
    private             int                             _restraintSetSelected;  // index of the selected restraint set
    // for the combo's
    private readonly    GameItemCombo[]                 _gameItemCombo;         // for getting the item combo
    private readonly    StainColorCombo                 _stainCombo;            // for getting the stain combo
    private readonly    DictStain                       _stainData;             // for getting the stain data
    private readonly    ItemData                        _itemData;              // for getting the item data
    private             string                          _inputTimer;            // for getting the input timer


    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public WardrobeRestraintCompartment(GagSpeakConfig config, FontService fontService, IDataManager gameData, TextureService textures,
    ItemData itemData, DictStain stainData, DalamudPluginInterface pluginInterface, UiBuilder uiBuilder, TimerService timerService,
    ISavable saveService, FilenameService filenameService, RestraintSetManager restraintSetManager) {
        _config = config;
        _fontService = fontService;
        _uiBuilder = uiBuilder;
        _gameData = gameData;
        _textures = textures;
        _itemData = itemData;
        _stainData = stainData;
        _config = config;
        _timerService = timerService;
        _saveService = saveService;
        _filenameService = filenameService;
        _restraintSetManager = restraintSetManager;

        _iconSize = new Vector2(48, 48);
        _comboLength = (ImGuiHelpers.GlobalScale * 350);
        _listLength = 5;
        _restraintSetSelected = 0;
        _inputTimer = "";

        // create a new gameItemCombo for each equipment piece type, then store them into the array.
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData, GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(DefaultWidth * ImGuiHelpers.GlobalScale, _stainData);

        // Subscribe to timer events
        _timerService.RemainingTimeChanged += OnRemainingTimeChanged;
    }

    public ReadOnlySpan<byte> Label => "WardrobeRestraintCompartment"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("WardrobeRestraintCompartmentChild");
        if (!child)
            return;

        DrawWardrobeRestraintCompartmentUI();
    }

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawWardrobeRestraintCompartmentUI() {
        using var child = ImRaii.Child("##WardrobeRestraintSetCompartment", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
        using (var table = ImRaii.Table("RestraintSetCompartmentTable", 2)) {
            if (!table) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("RestraintSetList", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 170);
            ImGui.TableSetupColumn("RestraintSetInfo", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 284); // test
            // begin by drawing the list
            ImGui.TableNextRow(); ImGui.TableNextColumn();

            // before we draw the list box, check to see if the list count is zero. if it is, add a new one with blank parameters
            if (_restraintSetManager._restraintSets.Count == 0) {
                _restraintSetManager.AddNewRestraintSet();
            }
            string[] restraintSetNames = _restraintSetManager._restraintSets.Select(set => set._name?? "ERROR").ToArray();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionMax().X-5);
            // Incase locked, disabled the options
            if(_restraintSetManager._restraintSets[_restraintSetSelected]._locked)
                ImGui.BeginDisabled();
            // draw out the list box
            ImGui.SetCursorPosY(ImGui.GetCursorPosY()+5);
            ImGui.ListBox("##RestraintSetListbox", ref _restraintSetSelected, restraintSetNames, restraintSetNames.Length, 5);
            // draw buttons
            style.Pop();
            var buttonwidth = ImGui.GetContentRegionAvail().X / 2;
            if (ImGui.Button("Add Set", new Vector2(buttonwidth, 25 * ImGuiHelpers.GlobalScale))) {
                _restraintSetManager.AddNewRestraintSet();
            }
            ImGui.SameLine();
            // remove button
            if (ImGui.Button("Remove Set", new Vector2(buttonwidth, 25.0f * ImGuiHelpers.GlobalScale))) {
                _restraintSetManager.DeleteRestraintSet(_restraintSetSelected);
            }
            // re-enable the options
            if(_restraintSetManager._restraintSets[_restraintSetSelected]._locked)
                ImGui.EndDisabled();

            // now we need to draw information about that restraint set and some button options
            ImGui.TableNextColumn();

            if(_restraintSetManager._restraintSets[_restraintSetSelected]._locked)
                ImGui.EndDisabled();
            // draw out the options
            var optionsTableWidth = ImGui.GetContentRegionAvail().X-5;
            using (var table2 = ImRaii.Table("RestraintSetOptions", 1, ImGuiTableFlags.RowBg, new Vector2(optionsTableWidth, 90))) {
                if (!table2)
                    return;
                ImGui.TableSetupColumn("Name & Description", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                // restraint set name
                ImGui.PushFont(_fontService.UidFont);
                string newName = _restraintSetManager._restraintSets[_restraintSetSelected]._name;
                UIHelpers.EditableTextFieldWithPopup("RestraintSetName", ref newName, 26,
                "Rename your Restraint Set:", "Enter a new restraint set name here");
                if (newName != _restraintSetManager._restraintSets[_restraintSetSelected]._name) {
                    _restraintSetManager.ChangeRestraintSetName(_restraintSetSelected, newName);
                }
                ImGui.PopFont();
                // descirption
                string newDescription = _restraintSetManager._restraintSets[_restraintSetSelected]._description;
                UIHelpers.EditableTextFieldWithPopup("RestraintSetDescription", ref newDescription, 128,
                "Write out a description for the set:", "Sets a description field for the restraint set, purely cosmetic feature");
                if (newDescription != _restraintSetManager._restraintSets[_restraintSetSelected]._description) {
                    _restraintSetManager.ChangeRestraintSetDescription(_restraintSetSelected, newDescription);
                }
            }
            // Incase locked, disabled the options
            using (var RestratintSetButtonOptions = ImRaii.Table("RestratintSetButtonOptions", 2, ImGuiTableFlags.RowBg)) {
                if (!RestratintSetButtonOptions) return;
                // Create the headers for the table
                ImGui.TableSetupColumn("RestraintSetOption", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Set is Inactivem").X);
                ImGui.TableSetupColumn("RestraintSetApplier", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextRow(); ImGui.TableNextColumn();

                if(_restraintSetManager._restraintSets[_restraintSetSelected]._locked)
                    ImGui.BeginDisabled();
                // draw out the options
                string lambdaText = _restraintSetManager._restraintSets[_restraintSetSelected]._enabled ? "ACTIVE" : "INACTIVE";
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Set is {lambdaText}");
                ImGui.TableNextColumn();
                if (ImGui.Button("Toggle State", new Vector2(0, 22.0f * ImGuiHelpers.GlobalScale))) {
                    _restraintSetManager.ToggleRestraintSetEnabled(_restraintSetSelected);
                }
                // now draw lock button
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Lock Restraints");
                ImGui.TableNextColumn();
                string result = _inputTimer; // get the input timer storage
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X/3);
                if (ImGui.InputTextWithHint("##RestraintSetTimer", "Ex: 0h2m7s", ref result, 12, ImGuiInputTextFlags.None)) {
                    _inputTimer = result;
                }
                ImGui.SameLine();
                // in the same line, place a button that enables the lock for the spesified time
                if (ImGui.Button("Lock", new Vector2(0, 22.0f * ImGuiHelpers.GlobalScale))) {
                    // parse the input timer
                    int currentIndex = _restraintSetSelected; // Capture the current index by value
                    _restraintSetManager.ChangeRestraintSetNewLockEndTime(currentIndex, UIHelpers.GetEndTime(_inputTimer));
                    _restraintSetManager.ChangeRestraintSetLocked(currentIndex, true);
                    _timerService.StartTimer($"RestraintSet {_restraintSetManager._restraintSets[currentIndex]._name}",
                        _inputTimer, 1000, () => { _restraintSetManager._restraintSets[currentIndex]._locked = false; });
                }
                // re-enable the options
                if(_restraintSetManager._restraintSets[_restraintSetSelected]._locked)
                    ImGui.EndDisabled();
            }
        } // end of table

        // next line, draw out the setup
        ImGui.Separator();
        //draw out two column table
        using (var table2 = ImRaii.Table("RestraintEquipSelection", 2, ImGuiTableFlags.RowBg)) {
            if (!table2) return;

            // Create the headers for the table
            var width = ImGui.GetContentRegionAvail().X/2;
            ImGui.TableSetupColumn("EquipmentSlots", ImGuiTableColumnFlags.WidthFixed, width);
            ImGui.TableSetupColumn("AccessorySlots", ImGuiTableColumnFlags.WidthStretch);
        
            // draw out the equipment slots
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            foreach(var slot in EquipSlotExtensions.EquipmentSlots) {
                var curentDrawData = _restraintSetManager._restraintSets[_restraintSetSelected]._drawData[slot];
                curentDrawData._gameItem.DrawIcon(_textures, _iconSize, curentDrawData._slot);  ImGui.SameLine();
                UIHelpers.DrawEquip(curentDrawData, _comboLength, _gameItemCombo, _stainCombo, _stainData,
                _config, _restraintSetManager, _filenameService);
            }
            ImGui.TableNextColumn();
            // draw out the accessory slots
            foreach(var slot in EquipSlotExtensions.AccessorySlots) {
                var curentDrawData = _restraintSetManager._restraintSets[_restraintSetSelected]._drawData[slot];
                curentDrawData._gameItem.DrawIcon(_textures, _iconSize, curentDrawData._slot);  ImGui.SameLine();
                UIHelpers.DrawEquip(curentDrawData, _comboLength, _gameItemCombo, _stainCombo, _stainData,
                _config, _restraintSetManager, _filenameService);
            }
        } // end of table
    }

    // For getting timer text updates
    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime) {
        // only display our restraints timer
        foreach(var restraintset in _restraintSetManager._restraintSets) {
            if(timerName == $"RestraintSet {restraintset._name}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Days} Days, "+
                $"{remainingTime.Hours} Hours, {remainingTime.Minutes} Minutes, {remainingTime.Seconds} Seconds";
            }
        }
    }
}
