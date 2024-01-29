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
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class WardrobeGagCompartment
{
    private const float DefaultWidth = 280;
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    GlamourerInterop                _Interop;      // for getting the glamourer interop
    private readonly    IClientState                    _clientState;           // for getting the client state
    private readonly    IDataManager                    _gameData;              // for getting the game data
    private readonly    TextureService                  _textures;              // for getting the textures
    private readonly    FontService                     _fonts;                 // for getting the fonts
    private readonly    FilenameService                 _filenameService;       // for getting the filename service
    private readonly    GagStorageManager               _gagStorageManager;     // for getting the gag storage manager
    public              string                          _filename;
    private             Vector2                         _iconSize;              // for setting the icon size
    private             float                           _comboLength;           // for setting the combo length
    // stuff for gag selection and equip 
    private readonly    string[]                        _gagNames;              // for getting the gag names
    private             int                             _gagNameSelected;       // for getting the selected gag
    public              GagList.GagType                 selectedGag;            // for getting the gag type
    private readonly    GameItemCombo[]                 _gameItemCombo;         // for getting the item combo
    private readonly    StainColorCombo                 _stainCombo;            // for getting the stain combo
    private readonly    DictStain                       _stainData;             // for getting the stain data
    private readonly    ItemData                        _itemData;              // for getting the item data

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public WardrobeGagCompartment(GagSpeakConfig config, DalamudPluginInterface pluginInterface, GlamourerInterop glamourerInterop,
    IClientState clientState, TextureService textures, DictStain stainData, IDataManager gameData, ItemData itemData,
    FontService fonts, FilenameService filenameService, GagStorageManager gagStorageManager) {
        _config = config;
        _Interop = glamourerInterop;
        _clientState = clientState;
        _textures = textures;
        _gameData = gameData;
        _stainData = stainData;
        _itemData = itemData;
        _fonts = fonts;
        _filenameService = filenameService;
        _gagStorageManager = gagStorageManager;

        // set the gaglisting names
        _gagNames = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().Select(gag => gag.GetGagAlias()).ToArray();
        _gagNameSelected = 0;
        _filename = _gagStorageManager.ToFilename(_filenameService);

        // create a new gameItemCombo for each equipment piece type, then store them into the array.
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData, GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(DefaultWidth * ImGuiHelpers.GlobalScale, _stainData);
    }

    public ReadOnlySpan<byte> Label => "WardrobeGagCompartment"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("WardrobeGagCompartmentChild");
        if (!child)
            return;
        DrawWardrobeGagCompartmentUI();
    }

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawWardrobeGagCompartmentUI() {
        // P R E P A R E
        _iconSize    = new Vector2(60, 60);
        _comboLength = (ImGuiHelpers.GlobalScale * 290);
        // configure the child and its styles
        using var child = ImRaii.Child("##WardrobeGagCompartment", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);

        // create a 2 column table layout
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
        using (var table = ImRaii.Table("PersonalKinkWardrobe", 2)) {
            if (!table) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("Gag Drawer", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 170);
            ImGui.TableSetupColumn("Active Gag Options", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 289); // test
            
            // begin by drawing the list
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // maybe some fancy text here while we have our cool font loaded anyways
            int tempSelectTracker = _gagNameSelected;
            // Create the listbox for the gag Drawer
            ImGui.SetNextItemWidth(ImGui.GetContentRegionMax().X-5);
            ImGui.ListBox("##Gag Drawer Listbox", ref _gagNameSelected, _gagNames, _gagNames.Length, 20);
            // Update WardrobeGagTypeActive when the the _gagNameSelected changes
            try {
                if(tempSelectTracker != _gagNameSelected) {
                    // this long ass thing is just a way around a possible null reference. Its just getting the selected Gag.
                    selectedGag = _gagNameSelected >= 0 && _gagNameSelected < Enum.GetValues(typeof(GagList.GagType)).Length 
                        ? (GagList.GagType)Enum.GetValues(typeof(GagList.GagType)).GetValue(_gagNameSelected)! 
                        : GagList.GagType.BallGag;
                }
            }
            catch (Exception e) {
                // print an error if we get any.
                GagSpeak.Log.Error($"[WardrobeTab] Error: {e}");
            }

            // go over to the next column, where we will have our collapsable headers (potentially)
            ImGui.TableNextColumn();

            // create a secondary table in this for prettiness
            using (var table2 = ImRaii.Table("GagDrawerCustomizerHeader", 2)) {
                if (!table2) { return; } // make sure our table was made
                // Create the headers for the table
                ImGui.TableSetupColumn("LargeActiveItemIcon", ImGuiTableColumnFlags.WidthFixed, 60);
                ImGui.TableSetupColumn("Header Text", ImGuiTableColumnFlags.WidthFixed, 230);
                // draw then icon
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                _gagStorageManager._gagEquipData[selectedGag]._gameItem.DrawIcon(_textures, _iconSize, _gagStorageManager._gagEquipData[selectedGag]._slot);
                // now draw out the customization header and dropdown.
                ImGui.TableNextColumn();
                // draw out the title
                ImGui.PushFont(_fonts.UidFont);
                ImGui.Text($"Personalized Gag Drawer");
                ImGui.PopFont();
                
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X/2);
                // display the wardrobe slot for this gag
                if(ImGui.Combo("Equipment Slot##WardrobeEquipSlot", ref _gagStorageManager._gagEquipData[selectedGag]._activeSlotListIdx,
                EquipSlotExtensions.EqdpSlots.Select(slot => slot.ToName()).ToArray(), EquipSlotExtensions.EqdpSlots.Count)) {
                    // Update the selected slot when the combo box selection changes
                    _gagStorageManager.ChangeGagDrawDataSlot(selectedGag, EquipSlotExtensions.EqdpSlots[_gagStorageManager._gagEquipData[selectedGag]._activeSlotListIdx]);
                    _gagStorageManager.ResetGagDrawDataGameItem(selectedGag);
                }
                // end the table
            }
            // down below, have a listing for the equipment drawer
            _comboLength = ImGui.GetContentRegionAvail().X;
            DrawEquip(selectedGag, _comboLength, _gameItemCombo, _stainCombo, _stainData);
            style.Pop();

            // If true, draw enable auto-equip as green and disabled, and bottom button as default color and pressable.
            if(_gagStorageManager._gagEquipData[selectedGag]._isEnabled) {
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
                    _gagStorageManager.ChangeGagDrawDataIsEnabled(selectedGag, false);
                    GagSpeak.Log.Debug($"[WardrobeTab] Disable Gag Drawer Button Clicked!");
                }
                ImGui.PopStyleColor(3);
            }
            else {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0xFE / 255f, 0x73 / 255f, 0xBE / 255f, 0xEF / 255f)); // #FE73BEEF
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0xFF / 255f, 0x61 / 255f, 0xD9 / 255f, 0xEF / 255f)); // #FF61D9EF
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0xFF / 255f, 0xAF / 255f, 0xEC / 255f, 0xEF / 255f)); // #FFAFECEF
                if(ImGui.Button("Enable Item Auto-Equip", new Vector2(ImGui.GetContentRegionAvail().X, 50))) {
                    _gagStorageManager.ChangeGagDrawDataIsEnabled(selectedGag, true);
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
            ImGui.Text($"IsEnabled: {_gagStorageManager._gagEquipData[selectedGag]._isEnabled}");
            ImGui.Text($"WasEquippedBy: {_gagStorageManager._gagEquipData[selectedGag]._wasEquippedBy}");
            ImGui.Text($"Locked: {_gagStorageManager._gagEquipData[selectedGag]._locked}");
            ImGui.Text($"ActiveSlotListIdx: {_gagStorageManager._gagEquipData[selectedGag]._activeSlotListIdx}");
            ImGui.Text($"Slot: {_gagStorageManager._gagEquipData[selectedGag]._slot}");
            ImGui.Text($"GameItem: {_gagStorageManager._gagEquipData[selectedGag]._gameItem}");
            ImGui.Text($"GameStain: {_gagStorageManager._gagEquipData[selectedGag]._gameStain}");
        } // end of table
    }


    /// <summary> Draws the equipment combo for the icon, item combo, and stain combo to the wardrobe tab.
    /// <list type="bullet">
    /// <item><c>EquipDrawData</c><paramref name="equipDrawData"> The equip data to draw.</paramref></item>
    /// </list> </summary>
    public void DrawEquip(GagList.GagType gagType, float width, GameItemCombo[] _gameItemCombo, 
    StainColorCombo _stainCombo, DictStain _stainData) {
        using var id      = ImRaii.PushId((int)_gagStorageManager._gagEquipData[gagType]._slot);
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var left  = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        width = ImGui.GetContentRegionAvail().X; // update length
        using var group = ImRaii.Group();
        DrawItem(gagType, out var label, right, left, width, _gameItemCombo);
        DrawStain(gagType, width, _stainCombo, _stainData);
    }

    /// <summary> Draws the item combo dropdown for our equipDrawData.
    /// <list type="bullet">
    /// <item><c>EquipDrawData</c><paramref name="data"> The equip data to draw.</paramref></item>
    /// <item><c>string</c><paramref name="label"> The label for the combo.</paramref></item>
    /// <item><c>bool</c><paramref name="clear"> Whether or not to clear the item.</paramref></item>
    /// <item><c>bool</c><paramref name="open"> Whether or not to open the combo.</paramref></item>
    /// </list> </summary>
    private void DrawItem(GagList.GagType gagType, out string label,bool clear, bool open, float width, 
    GameItemCombo[] _gameItemCombo) {
        // draw the item combo.
        var combo = _gameItemCombo[_gagStorageManager._gagEquipData[gagType]._slot.ToIndex()];
        label = combo.Label;
        if (!_gagStorageManager._gagEquipData[gagType]._locked && open) {
            UIHelpers.OpenCombo($"##GagShelfItem{gagType}{combo.Label}");
            GagSpeak.Log.Debug($"{combo.Label}");
        }
        // draw the combo
        using var disabled = ImRaii.Disabled(_gagStorageManager._gagEquipData[gagType]._locked);
        var change = combo.Draw(_gagStorageManager._gagEquipData[gagType]._gameItem.Name, _gagStorageManager._gagEquipData[gagType]._gameItem.ItemId, width, width);
        // conditionals to detect for changes in the combo's
        if (change && !_gagStorageManager._gagEquipData[gagType]._gameItem.Equals(combo.CurrentSelection)) {
            _gagStorageManager.ChangeGagDrawDataGameItem(gagType, combo.CurrentSelection);
            // save the correct config
            using (StreamWriter writer = new StreamWriter(_filename)) { _gagStorageManager.Save(writer); }
        }
        if (clear || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            _gagStorageManager.ResetGagDrawDataGameItem(gagType);
            // save the correct config
            using (StreamWriter writer = new StreamWriter(_filename)) { _gagStorageManager.Save(writer); }
            GagSpeak.Log.Debug($"[WardrobeTab] Right Click processed, item reverted to none!");
        }
    }

    /// <summary> Draws the stain combo dropdown for our equipDrawData.
    /// <list type="bullet">
    /// <item><c>EquipDrawData</c><paramref name="data"> The equip data to draw.</paramref></item>
    /// </list> </summary>
    private void DrawStain(GagList.GagType gagType, float width, StainColorCombo _stainCombo, DictStain _stainData) {
        // fetch the correct stain from the stain data
        var       found    = _stainData.TryGetValue(_gagStorageManager._gagEquipData[gagType]._gameStain, out var stain);
        using var disabled = ImRaii.Disabled(_gagStorageManager._gagEquipData[gagType]._locked);
        // draw the stain combo
        if (_stainCombo.Draw($"##GagShelfStain{gagType}{_gagStorageManager._gagEquipData[gagType]._slot}", stain.RgbaColor, stain.Name, found, stain.Gloss, width)) {
            if (_stainData.TryGetValue(_stainCombo.CurrentSelection.Key, out stain)) {
                _gagStorageManager.ChangeGagDrawDataGameStain(gagType, stain.RowIndex);
                GagSpeak.Log.Debug($"[WardrobeTab] Stain Changed: {stain.RowIndex}");
                using (StreamWriter writer = new StreamWriter(_filename)) { _gagStorageManager.Save(writer); }
            }
            else if (_stainCombo.CurrentSelection.Key == Penumbra.GameData.Structs.Stain.None.RowIndex) {
                //data.StainSetter(Stain.None.RowIndex);
                GagSpeak.Log.Debug($"[WardrobeTab] Stain Changed: None");
            }
        }
        // conditionals to detect for changes in the combo's via reset
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            _gagStorageManager.ResetGagDrawDataGameStain(gagType);
            using (StreamWriter writer = new StreamWriter(_filename)) { _gagStorageManager.Save(writer); }
            GagSpeak.Log.Debug($"[WardrobeTab] Right Click processed, stain reverted to none!");
        }
    }
}
