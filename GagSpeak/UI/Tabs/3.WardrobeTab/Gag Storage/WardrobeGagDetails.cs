using System;
using System.Numerics;
using System.IO;
using ImGuiNET;
using OtterGui.Raii;
using Dalamud.Plugin;
using System.Linq;
using Penumbra.GameData.Enums;
using GagSpeak.Services;
using GagSpeak.Utility;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Utility;
using Penumbra.GameData.DataContainers;
using Penumbra.GameData.Data;
using GagSpeak.Wardrobe;
using GagSpeak.Gagsandlocks;
using GagSpeak.UI.Equipment;

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class GagStorageDetails
{
    private const float _comboWidth = 315;
    private readonly    IDataManager                    _gameData;              // for getting the game data
    private readonly    TextureService                  _textures;              // for getting the textures
    private readonly    FontService                     _fonts;                 // for getting the fonts
    private readonly    GagStorageManager               _gagStorageManager;     // for getting the gag storage manager
    private             Vector2                         _iconSize;              // for setting the icon size
    private             float                           _comboLength;           // for setting the combo length
    private readonly    GameItemCombo[]                 _gameItemCombo;         // for getting the item combo
    private readonly    StainColorCombo                 _stainCombo;            // for getting the stain combo
    private readonly    DictStain                       _stainData;             // for getting the stain data
    private readonly    ItemData                        _itemData;              // for getting the item data

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public GagStorageDetails(DalamudPluginInterface pluginInterface, TextureService textures,
    DictStain stainData, IDataManager gameData, ItemData itemData, FontService fonts,
    GagStorageManager gagStorageManager) {
        _textures = textures;
        _gameData = gameData;
        _stainData = stainData;
        _itemData = itemData;
        _fonts = fonts;
        _gagStorageManager = gagStorageManager;

        // create a new gameItemCombo for each equipment piece type, then store them into the array.
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData, GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(_comboWidth-20, _stainData);
    }

    public void Draw() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("WardrobeGagCompartmentChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child)
            return;
        // draw the compartment details
        DrawWardrobeGagCompartmentUI();
    }

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawWardrobeGagCompartmentUI() {
        _iconSize    = ImGuiHelpers.ScaledVector2(60);
        _comboLength = _comboWidth * ImGuiHelpers.GlobalScale;
        // create a secondary table in this for prettiness
        using (var table2 = ImRaii.Table("GagDrawerCustomizerHeader", 2)) {
            if (!table2) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("LargeActiveItemIcon", ImGuiTableColumnFlags.WidthFixed, 60 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("Header Text", ImGuiTableColumnFlags.WidthStretch);
            // draw then icon
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            _gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._gameItem.DrawIcon(_textures, _iconSize, 
            _gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._slot);
            // now draw out the customization header and dropdown.
            ImGui.TableNextColumn();
            // draw out the title
            ImGui.PushFont(_fonts.UidFont);
            ImGui.Text($"Personalized Gag Drawer");
            ImGui.PopFont();
            
            ImGui.SetNextItemWidth(_comboLength * 1/3);
            // display the wardrobe slot for this gag
            if(ImGui.Combo("Equipment Slot##WardrobeEquipSlot", ref _gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._activeSlotListIdx,
            EquipSlotExtensions.EqdpSlots.Select(slot => slot.ToName()).ToArray(), EquipSlotExtensions.EqdpSlots.Count)) {
                // Update the selected slot when the combo box selection changes
                _gagStorageManager.ChangeGagDrawDataSlot(_gagStorageManager._selectedGag, 
                EquipSlotExtensions.EqdpSlots[_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._activeSlotListIdx]);
                _gagStorageManager.ResetGagDrawDataGameItem(_gagStorageManager._selectedGag);
            }
        }

        DrawEquip(_gagStorageManager._selectedGag, _gameItemCombo, _stainCombo, _stainData, _comboLength);

        // If true, draw enable auto-equip as green and disabled, and bottom button as default color and pressable.
        if(_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._isEnabled) {
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x0080FF40); // Slightly more Green
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x0080FF40); // Slightly more Green
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x0080FF40); // Slightly more Green
            ImGui.BeginDisabled(); 
            ImGui.Button("Enable Item Auto-Equip", new Vector2(_comboLength, 50));
            ImGui.EndDisabled();
            ImGui.PopStyleColor(3);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0xFE / 255f, 0x73 / 255f, 0xBE / 255f, 0xEF / 255f)); // #FE73BEEF
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0xFF / 255f, 0x61 / 255f, 0xD9 / 255f, 0xEF / 255f)); // #FF61D9EF
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0xFF / 255f, 0xAF / 255f, 0xEC / 255f, 0xEF / 255f)); // #FFAFECEF
            // and draw the interactable button
            if(ImGui.Button("Disable Item Auto-Equip", new Vector2(_comboLength, 50))) {
                // do something when the button is clicked
                _gagStorageManager.ChangeGagDrawDataIsEnabled(_gagStorageManager._selectedGag, false);
                GSLogger.LogType.Debug($"[WardrobeTab] Disable Gag Drawer Button Clicked!");
            }
            ImGui.PopStyleColor(3);
        }
        else {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0xFE / 255f, 0x73 / 255f, 0xBE / 255f, 0xEF / 255f)); // #FE73BEEF
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0xFF / 255f, 0x61 / 255f, 0xD9 / 255f, 0xEF / 255f)); // #FF61D9EF
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0xFF / 255f, 0xAF / 255f, 0xEC / 255f, 0xEF / 255f)); // #FFAFECEF
            if(ImGui.Button("Enable Item Auto-Equip",  new Vector2(_comboLength, 50))) {
                _gagStorageManager.ChangeGagDrawDataIsEnabled(_gagStorageManager._selectedGag, true);
                GSLogger.LogType.Debug($"[WardrobeTab] Enable Gag Drawer Button Clicked!");
            }
            ImGui.PopStyleColor(3);
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x0080FF40); // Slightly more Green
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x0080FF40); // Slightly more Green
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x0080FF40); // Slightly more Green
            ImGui.BeginDisabled();
            ImGui.Button("Disable Item Auto-Equip", new Vector2(_comboLength, 50));
            ImGui.EndDisabled();
            ImGui.PopStyleColor(3);
        }

        // draw debug metrics
        ImGui.NewLine();
        ImGui.Text($"Gag Name: {_gagStorageManager._selectedGag.GetGagAlias()}");
        ImGui.Text($"IsEnabled: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._isEnabled}");
        ImGui.Text($"WasEquippedBy: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._wasEquippedBy}");
        ImGui.Text($"Locked: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._locked}");
        ImGui.Text($"ActiveSlotListIdx: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._activeSlotListIdx}");
        ImGui.Text($"Slot: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._slot}");
        ImGui.Text($"GameItem: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._gameItem}");
        ImGui.Text($"GameStain: {_gagStorageManager._gagEquipData[_gagStorageManager._selectedGag]._gameStain}");
    }


    /// <summary> Draws the equipment combo for the icon, item combo, and stain combo to the wardrobe tab.
    /// <list type="bullet">
    /// <item><c>EquipDrawData</c><paramref name="equipDrawData"> The equip data to draw.</paramref></item>
    /// </list> </summary>
    public void DrawEquip(GagList.GagType gagType, GameItemCombo[] _gameItemCombo, 
    StainColorCombo _stainCombo, DictStain _stainData, float _comboLength) {
        using var id      = ImRaii.PushId((int)_gagStorageManager._gagEquipData[gagType]._slot);
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var left  = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        using var group = ImRaii.Group();
        DrawItem(gagType, out var label, right, left, _comboLength, _gameItemCombo);
        DrawStain(gagType, _comboLength, _stainCombo, _stainData);
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
            GSLogger.LogType.Debug($"{combo.Label}");
        }
        // draw the combo
        using var disabled = ImRaii.Disabled(_gagStorageManager._gagEquipData[gagType]._locked);
        var change = combo.Draw(_gagStorageManager._gagEquipData[gagType]._gameItem.Name, _gagStorageManager._gagEquipData[gagType]._gameItem.ItemId, width, width);
        // conditionals to detect for changes in the combo's
        if (change && !_gagStorageManager._gagEquipData[gagType]._gameItem.Equals(combo.CurrentSelection)) {
            _gagStorageManager.ChangeGagDrawDataGameItem(gagType, combo.CurrentSelection);
        }
        if (clear || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            _gagStorageManager.ResetGagDrawDataGameItem(gagType);

            GSLogger.LogType.Debug($"[WardrobeTab] Right Click processed, item reverted to none!");
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
                GSLogger.LogType.Debug($"[WardrobeTab] Stain Changed: {stain.RowIndex}");
            }
            else if (_stainCombo.CurrentSelection.Key == Penumbra.GameData.Structs.Stain.None.RowIndex) {
                //data.StainSetter(Stain.None.RowIndex);
                GSLogger.LogType.Debug($"[WardrobeTab] Stain Changed: None");
            }
        }
        // conditionals to detect for changes in the combo's via reset
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            _gagStorageManager.ResetGagDrawDataGameStain(gagType);
            GSLogger.LogType.Debug($"[WardrobeTab] Right Click processed, stain reverted to none!");
        }
    }
}
