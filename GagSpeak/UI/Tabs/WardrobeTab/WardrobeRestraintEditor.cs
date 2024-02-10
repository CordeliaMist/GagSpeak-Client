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
using GagSpeak.UI.Equipment;
using GagSpeak.Wardrobe;
using OtterGui;
using Dalamud.Interface;

namespace GagSpeak.UI.Tabs.WardrobeTab;

/// <summary> Stores the UI for the restraints shelf of the kink wardrobe. </summary>
public class RestraintSetEditor
{
    private const float DefaultWidth = 177;
    private             Vector2                         _VisibilityIconSize;
    private readonly    IDataManager                    _gameData;              // for getting the game data
    private readonly    TextureService                  _textures;              // for getting the textures
    private readonly    RestraintSetManager             _restraintSetManager;   // for getting the restraint set manager
    private readonly    Vector2                         _iconSize;              // size of icons that can display
    private             float                           _comboLength;           // length of combo boxes
    private readonly    GameItemCombo[]                 _gameItemCombo;         // for getting the item combo
    private readonly    StainColorCombo                 _stainCombo;            // for getting the stain combo
    private readonly    DictStain                       _stainData;             // for getting the stain data
    private readonly    ItemData                        _itemData;              // for getting the item data 
    private             string[]                        _eyeIcon;

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public RestraintSetEditor(IDataManager gameData, TextureService textures, ItemData itemData, DictStain stainData,
    DalamudPluginInterface pluginInterface, RestraintSetManager restraintSetManager) {
        _gameData = gameData;
        _textures = textures;
        _itemData = itemData;
        _stainData = stainData;
        _restraintSetManager = restraintSetManager;
        _iconSize    = ImGuiHelpers.ScaledVector2(48);
        _eyeIcon = new string[EquipSlotExtensions.EqdpSlots.Count];
        // create a new gameItemCombo for each equipment piece type, then store them into the array.
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData, GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(DefaultWidth-20, _stainData);
    }

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    public void Draw() {
        _comboLength = DefaultWidth * ImGuiHelpers.GlobalScale;
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        using var child = ImRaii.Child("WardrobeRestraintCompartmentChild", new Vector2(0, -1));
        if (!child) {return;}
        //draw out two column table
        var xPos = ImGui.GetCursorPosX();
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPos, yPos + ImGui.GetStyle().ItemSpacing.Y * ImGuiHelpers.GlobalScale));
        using (var table2 = ImRaii.Table("RestraintEquipSelection", 2, ImGuiTableFlags.RowBg)) {
            if (!table2) return;
            // Create the headers for the table
            var width = ImGui.GetContentRegionAvail().X/2 - ImGui.GetStyle().ItemSpacing.X;
            // setup the columns
            ImGui.TableSetupColumn("EquipmentSlots", ImGuiTableColumnFlags.WidthFixed, width);
            ImGui.TableSetupColumn("AccessorySlots", ImGuiTableColumnFlags.WidthStretch);

            // draw out the equipment slots
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            int i = 0;
            foreach(var slot in EquipSlotExtensions.EquipmentSlots) {
                _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._drawData[slot]._gameItem.DrawIcon(_textures, _iconSize, slot);
                ImGui.SameLine();
                DrawEquip(_restraintSetManager._selectedIdx, slot, _comboLength, _gameItemCombo, _stainCombo, _stainData);
                ImGui.SameLine();
                _eyeIcon[i] = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._drawData[slot]._isEnabled 
                        ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
                // display either eyeslash or eye based on if it is enabled or not
                if(ImGuiUtil.DrawDisabledButton($"{_eyeIcon[i]}##{slot}VisibilityButton", new Vector2(ImGui.GetContentRegionAvail().X, 48),
                "Enable or Disable this piece from being visible. [[ Does nothing right now ]]", false, true)) {
                    // if we click this, we should toggle between states with a visable eye icon
                    _restraintSetManager.ToggleRestraintSetPieceEnabledState(_restraintSetManager._selectedIdx, slot);
                }
                i++;
            }
            // i am dumb and dont know how to place adjustable divider lengths
            ImGui.TableNextColumn();
            //draw out the accessory slots
            foreach(var slot in EquipSlotExtensions.AccessorySlots) {
                _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._drawData[slot]._gameItem.DrawIcon(_textures, _iconSize, slot);
                ImGui.SameLine();
                DrawEquip(_restraintSetManager._selectedIdx, slot, _comboLength, _gameItemCombo, _stainCombo, _stainData);
                ImGui.SameLine();
                _eyeIcon[i] = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._drawData[slot]._isEnabled 
                        ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
                // display either eyeslash or eye based on if it is enabled or not
                if(ImGuiUtil.DrawDisabledButton($"{_eyeIcon[i]}##{slot}VisibilityButton", new Vector2(ImGui.GetContentRegionAvail().X, 48),
                "Enable or Disable this piece from being visible. [[ Does nothing right now ]]", false, true)) {
                    // if we click this, we should toggle between states with a visable eye icon
                    _restraintSetManager.ToggleRestraintSetPieceEnabledState(_restraintSetManager._selectedIdx, slot);
                    _eyeIcon[i] = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._drawData[slot]._isEnabled 
                        ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
                }
                i++;
            }
        } // end of table
    }

    /// <summary> Draws the equipment combo for the icon, item combo, and stain combo to the wardrobe tab. </summary>
    public void DrawEquip(int SetIndex, EquipSlot slot, float width, GameItemCombo[] _gameItemCombo, 
    StainColorCombo _stainCombo, DictStain _stainData) {
        var uniqueId = $"{SetIndex}_{(int)slot}";
        using var id      = ImRaii.PushId(uniqueId);
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var left  = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        if (SetIndex >= 0 && SetIndex < _gameItemCombo.Length) {
            using var group = ImRaii.Group();
            DrawItem(SetIndex, slot, out var label, right, left, width, _gameItemCombo);
            DrawStain(SetIndex, slot, width, _stainCombo, _stainData);
        } else {
            // Handle the error, e.g., log a message or throw an exception
            Console.WriteLine($"Invalid SetIndex: {SetIndex}. Must be between 0 and {_gameItemCombo.Length - 1}.");
        }
    }

    /// <summary> Draws the item combo dropdown for our equipDrawData.
    /// <list type="bullet">
    /// <item><c>EquipDrawData</c><paramref name="data"> The equip data to draw.</paramref></item>
    /// <item><c>string</c><paramref name="label"> The label for the combo.</paramref></item>
    /// <item><c>bool</c><paramref name="clear"> Whether or not to clear the item.</paramref></item>
    /// <item><c>bool</c><paramref name="open"> Whether or not to open the combo.</paramref></item>
    /// </list> </summary>
    private void DrawItem(int SetIndex, EquipSlot slot, out string label,bool clear, bool open, float width, 
    GameItemCombo[] _gameItemCombo) {
        // draw the item combo
        var combo = _gameItemCombo[_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._activeSlotListIdx];
        label = combo.Label;
        if (!_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._locked && open) {
            UIHelpers.OpenCombo($"##RestraintShelf{_restraintSetManager._restraintSets[SetIndex]._name}{combo.Label}");
            GagSpeak.Log.Debug($"{combo.Label}");
        }
        // draw the combo
        using var disabled = ImRaii.Disabled(_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._locked);
        var change = combo.Draw(_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._gameItem.Name,
                                _restraintSetManager._restraintSets[SetIndex]._drawData[slot]._gameItem.ItemId,
                                width, width);
        // conditionals to detect for changes in the combo's
        if (change && !_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._gameItem.Equals(combo.CurrentSelection)) {
            _restraintSetManager.ChangeSetDrawDataGameItem(SetIndex, slot, combo.CurrentSelection);
        }
        if (clear || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            _restraintSetManager.ResetSetDrawDataGameItem(SetIndex, slot);
            GagSpeak.Log.Debug($"[WardrobeTab] Right Click processed, item reverted to none!");
        }
    }

    /// <summary> Draws the stain combo dropdown for our equipDrawData.
    /// <list type="bullet">
    /// <item><c>EquipDrawData</c><paramref name="data"> The equip data to draw.</paramref></item>
    /// </list> </summary>
    private void DrawStain(int SetIndex, EquipSlot slot, float width, StainColorCombo _stainCombo, DictStain _stainData) {
        // fetch the correct stain from the stain data
        var       found    = _stainData.TryGetValue(_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._gameStain, out var stain);
        using var disabled = ImRaii.Disabled(_restraintSetManager._restraintSets[SetIndex]._drawData[slot]._locked);
        // draw the stain combo
        if (_stainCombo.Draw($"##GagShelfStain{_restraintSetManager._restraintSets[SetIndex]._name}{slot}", stain.RgbaColor, stain.Name, found, stain.Gloss, width)) {
            if (_stainData.TryGetValue(_stainCombo.CurrentSelection.Key, out stain)) {
                _restraintSetManager.ChangeSetDrawDataGameStain(SetIndex, slot, stain.RowIndex);
                GagSpeak.Log.Debug($"[WardrobeTab] Stain Changed: {stain.RowIndex}");
            }
            else if (_stainCombo.CurrentSelection.Key == Penumbra.GameData.Structs.Stain.None.RowIndex) {
                //data.StainSetter(Stain.None.RowIndex);
                GagSpeak.Log.Debug($"[WardrobeTab] Stain Changed: None");
            }
        }
        // conditionals to detect for changes in the combo's via reset
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            _restraintSetManager.ResetSetDrawDataGameStain(SetIndex, slot);
            GagSpeak.Log.Debug($"[WardrobeTab] Right Click processed, stain reverted to none!");
        }
    }
}
