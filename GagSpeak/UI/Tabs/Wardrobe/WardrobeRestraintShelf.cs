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


    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public WardrobeRestraintCompartment(GagSpeakConfig config, FontService fontService, IDataManager gameData,
    TextureService textures, ItemData itemData, DictStain stainData, DalamudPluginInterface pluginInterface,
    UiBuilder uiBuilder, RestraintSetManager restraintSetManager) {
        _config = config;
        _fontService = fontService;
        _uiBuilder = uiBuilder;
        _gameData = gameData;
        _textures = textures;
        _itemData = itemData;
        _stainData = stainData;
        _config = config;
        _restraintSetManager = restraintSetManager;

        _iconSize = new Vector2(40, 40);
        _comboLength = (ImGuiHelpers.GlobalScale * 350);
        _listLength = 5;
        // create a new gameItemCombo for each equipment piece type, then store them into the array.
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData, GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(DefaultWidth * ImGuiHelpers.GlobalScale, _stainData);
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
        ImGui.PushFont(_fontService.UidFont);
        ImGuiUtil.Center("Wardrobe Restraint Compartment");
        ImGui.PopFont();

        using var child = ImRaii.Child("##WardrobeRestraintSetCompartment", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);

        // create a 2 column table layout
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
        using (var table = ImRaii.Table("PersonalKinkWardrobe", 2)) {
            if (!table) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("RestraintSetList", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 170);
            ImGui.TableSetupColumn("RestraintSetInfo", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 289); // test
            
            // begin by drawing the list
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // get the array of names from the restraintSetList
            var restraintSetNameArray = _restraintSetManager._restraintSets.Select(set => set.GetName()).ToArray();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionMax().X-5);
            ImGui.ListBox("##RestraintSetListbox", ref _restraintSetSelected, restraintSetNameArray, 5);

            // now we need to draw information about that restraint set and some button options
            ImGui.TableNextColumn();
            // draw out the title
            ImGui.PushFont(_fontService.UidFont);
            ImGui.Text($"{_restraintSetManager._restraintSets[_restraintSetSelected].GetName()}");
            ImGui.PopFont();
            if(ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                ImGui.OpenPopup("RenameSetPopup"); // Open the context menu
            }
            if(ImGui.BeginPopup("RenameSetPopup")) {
                var currentPath = _restraintSetManager._restraintSets[_restraintSetSelected].GetName();
                if (ImGui.IsWindowAppearing())
                    ImGui.SetKeyboardFocusHere(0);
                ImGui.TextUnformatted("Rename Search Path or Move:");
                if (ImGui.InputText("##Rename", ref currentPath, 48, ImGuiInputTextFlags.EnterReturnsTrue)) {
                    //_fsActions.Enqueue(() =>
                    //{
                    //    FileSystem.RenameAndMove(leaf, currentPath);
                    //    _filterDirty |= ExpandAncestors(leaf);
                    //});
                    ImGui.CloseCurrentPopup();
                }
                ImGuiUtil.HoverTooltip("Enter a new restraint set design name here");
            }

        }

        if(!ImGui.CollapsingHeader($"Restraint Customizations for set: {_restraintSetManager._restraintSets[_restraintSetSelected].GetName()}")) { return; }
        // draw out two column table
        using (var table2 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table2) return;

            // Create the headers for the table
            var width = ImGui.GetContentRegionAvail().X/2;
            ImGui.TableSetupColumn("EquipmentSlots", ImGuiTableColumnFlags.WidthFixed, width);
            ImGui.TableSetupColumn("AccessorySlots", ImGuiTableColumnFlags.WidthStretch);
        
            // draw out the equipment slots
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            foreach(var slot in EquipSlotExtensions.EquipmentSlots) {
                // Get the drawData location
                var curentDrawData = _restraintSetManager._restraintSets[_restraintSetSelected]._drawData[slot];
                // now get the icon out from it.
                curentDrawData._gameItem.DrawIcon(_textures, _iconSize, curentDrawData._slot);
                ImGui.SameLine();
                UIHelpers.DrawEquip(curentDrawData, _comboLength, _gameItemCombo, _stainCombo, _stainData, _config);
            }

            // If true, draw enable auto-equip as green and disabled, and bottom button as default color and pressable.
            if(_config.gagEquipData[selectedGag]._isEnabled) {
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x0080FF40); // Slightly more Green
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x0080FF40); // Slightly more Green
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x0080FF40); // Slightly more Green
                ImGui.BeginDisabled(); 
                ImGui.Button("Enable Item Auto-Equip", new Vector2(ImGui.GetContentRegionAvail().X, 50));
                ImGui.EndDisabled();
                ImGui.PopStyleColor(3);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0xFE / 255f, 0x73 / 255f, 0xBE / 255f, 0xEF / 255f)); // #FE73BEEF
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0xFF / 255f, 0x61 / 255f, 0xD9 / 255f, 0xEF / 255f)); // #FF61D9EF
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0xFF / 255f, 0xAF / 255f, 0xEC / 255f, 0xEF / 255f)); // #FFAFECEF
                // and draw the interactable button
                if(ImGui.Button("Disable Item Auto-Equip", new Vector2(ImGui.GetContentRegionAvail().X, 50))) {
                    // do something when the button is clicked
                    _config.gagEquipData[selectedGag]._isEnabled = false;
                    GagSpeak.Log.Debug($"[WardrobeTab] Disable Gag Drawer Button Clicked!");
                }
                ImGui.PopStyleColor(3);
            }
            else {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0xFE / 255f, 0x73 / 255f, 0xBE / 255f, 0xEF / 255f)); // #FE73BEEF
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0xFF / 255f, 0x61 / 255f, 0xD9 / 255f, 0xEF / 255f)); // #FF61D9EF
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0xFF / 255f, 0xAF / 255f, 0xEC / 255f, 0xEF / 255f)); // #FFAFECEF
                if(ImGui.Button("Enable Item Auto-Equip", new Vector2(ImGui.GetContentRegionAvail().X, 50))) {
                    _config.gagEquipData[selectedGag]._isEnabled = true;
                    GagSpeak.Log.Debug($"[WardrobeTab] Enable Gag Drawer Button Clicked!");
                }
                ImGui.PopStyleColor(3);
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x0080FF40); // Slightly more Green
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x0080FF40); // Slightly more Green
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x0080FF40); // Slightly more Green
                ImGui.BeginDisabled();
                ImGui.Button("Disable Item Auto-Equip", new Vector2(ImGui.GetContentRegionAvail().X, 50));
                ImGui.EndDisabled();
                ImGui.PopStyleColor(3);
            }

            // draw debug metrics
            ImGui.NewLine();
            ImGui.Text($"Gag Name: {selectedGag.GetGagAlias()}");
            ImGui.Text($"IsEnabled: {_config.gagEquipData[selectedGag]._isEnabled}");
            ImGui.Text($"WasEquippedBy: {_config.gagEquipData[selectedGag]._wasEquippedBy}");
            ImGui.Text($"Locked: {_config.gagEquipData[selectedGag]._locked}");
            ImGui.Text($"ActiveSlotListIdx: {_config.gagEquipData[selectedGag]._activeSlotListIdx}");
            ImGui.Text($"Slot: {_config.gagEquipData[selectedGag]._slot}");
            ImGui.Text($"GameItem: {_config.gagEquipData[selectedGag]._gameItem}");
            ImGui.Text($"GameStain: {_config.gagEquipData[selectedGag]._gameStain}");
        } // end of table
    }
    }
}
