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

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class WardrobeTab : ITab
{
    private const float DefaultWidth = 280;
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    UiBuilder                       _uiBuilder;             // for loading images
    private readonly    GlamourerInterop                _Interop;      // for getting the glamourer interop
    private readonly    IClientState                    _clientState;           // for getting the client state
    private readonly    IDataManager                    _gameData;              // for getting the game data
    private readonly    TextureService                  _textures;              // for getting the textures
    private readonly    FontService                     _fonts;                 // for getting the fonts
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
    public WardrobeTab(GagSpeakConfig config, UiBuilder uiBuilder, DalamudPluginInterface pluginInterface,
    GlamourerInterop glamourerInterop, IClientState clientState, TextureService textures,
    DictStain stainData, IDataManager gameData, ItemData itemData, FontService fonts) {
        _config = config;
        _uiBuilder = uiBuilder;
        _Interop = glamourerInterop;
        _clientState = clientState;
        _textures = textures;
        _gameData = gameData;
        _stainData = stainData;
        _itemData = itemData;
        _fonts = fonts;
        // set the gaglisting names
        _gagNames = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().Select(gag => gag.GetGagAlias()).ToArray();
        _gagNameSelected = 0;

        // create a new gameItemCombo for each equipment piece type, then store them into the array.
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData, GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(DefaultWidth * ImGuiHelpers.GlobalScale, _stainData);
    }

    public ReadOnlySpan<byte> Label => "Wardrobe"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;
        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("WardrobeTabChild")) {
            DrawHeader();
            DrawWardrobeManager();
        }
    }

    /// <summary> this function initializaes most of the dropdowns and icon sizes </summary>
    public void Prepare() {
        // set the icon size
        _iconSize    = new Vector2(60, 60);
        _comboLength = (ImGuiHelpers.GlobalScale * 290);
    }

    /// <summary> This Function draws the header for the ConfigSettings Tab </summary>
    private void DrawHeader()
        => WindowHeader.Draw("Wardrobe Manager", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawWardrobeManager() {
        // P R E P A R E
        Prepare();
        // configure the child and its styles
        using var child = ImRaii.Child("##WardrobePanel", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);
        //var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        //ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

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
                    _config.Save(); // maybe remove if too agressive
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
                _config.gagEquipData[selectedGag]._gameItem.DrawIcon(_textures, _iconSize, _config.gagEquipData[selectedGag]._slot);

                // now draw out the customization header and dropdown.
                ImGui.TableNextColumn();
                // draw out the title
                ImGui.PushFont(_fonts.UidFont);
                ImGui.Text($"Personalized Gag Drawer");
                ImGui.PopFont();
                
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X/2);
                // display the wardrobe slot for this gag
                if(ImGui.Combo("Equipment Slot##WardrobeEquipSlot", ref _config.gagEquipData[selectedGag]._activeSlotListIdx,
                EquipSlotExtensions.EqdpSlots.Select(slot => slot.ToName()).ToArray(), EquipSlotExtensions.EqdpSlots.Count)) {
                    // Update the selected slot when the combo box selection changes
                    _config.gagEquipData[selectedGag]._slot = EquipSlotExtensions.EqdpSlots[_config.gagEquipData[selectedGag]._activeSlotListIdx];
                }
                // end the table
            }
            // down below, have a listing for the equipment drawer
            DrawEquip(_config.gagEquipData[selectedGag]);
            style.Pop();

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
            ImGui.Text($"Locked: {_config.gagEquipData[selectedGag]._locked}");
            ImGui.Text($"ActiveSlotListIdx: {_config.gagEquipData[selectedGag]._activeSlotListIdx}");
            ImGui.Text($"Slot: {_config.gagEquipData[selectedGag]._slot}");
            ImGui.Text($"GameItem: {_config.gagEquipData[selectedGag]._gameItem}");
            ImGui.Text($"GameStain: {_config.gagEquipData[selectedGag]._gameStain}");

            if(ImGui.Button("Flood the config", new Vector2(ImGui.GetContentRegionAvail().X, 25))) {
                foreach (var gagType in _config.gagEquipData.Keys)
                {
                    var equipData = _config.gagEquipData[gagType];
                    GagSpeak.Log.Debug($"Gag Name: {gagType}");
                    GagSpeak.Log.Debug($"IsEnabled: {equipData._isEnabled}");
                    GagSpeak.Log.Debug($"Locked: {equipData._locked}");
                    GagSpeak.Log.Debug($"ActiveSlotListIdx: {equipData._activeSlotListIdx}");
                    GagSpeak.Log.Debug($"Slot: {equipData._slot}");
                    GagSpeak.Log.Debug($"GameItem: {equipData._gameItem}");
                    GagSpeak.Log.Debug($"GameStain: {equipData._gameStain}");
                }
            }
        } // end of table


        //     // first get the customization
        //     try {
        //         // create a new character customization
        //         CharacterCustomization characterCustomization = null; // initialize to null at first
        //         // get the character customization from the client state
        //         string customizationValue = _Interop._GetAllCustomizationFromCharacter.InvokeFunc(_clientState.LocalPlayer);
        //         // convert from the base64string
        //         var bytes = System.Convert.FromBase64String(customizationValue);
        //         // grab the version
        //         var version = bytes[0];
        //         version = bytes.DecompressToString(out var decompressed); // get the decompressed
        //         characterCustomization = JsonConvert.DeserializeObject<CharacterCustomization>(decompressed); // get the deserialization
        //         // attempt to set the item
        //         ImGui.InputText("##CustomizationCode", ref customizationValue, 2000);
        //     }
        //     catch (Exception e) {
        //         GagSpeak.Log.Error($"[WardrobeTab] Error: {e}");
        //     }
        // } // end of table
    }

    public void DrawEquip(EquipDrawData equipDrawData) {
        using var id      = ImRaii.PushId((int)equipDrawData._slot);
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var left  = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        _comboLength = ImGui.GetContentRegionAvail().X; // update length
        using var group = ImRaii.Group();
        DrawItem(equipDrawData, out var label, right, left);
        DrawStain(equipDrawData);
    }

    // for drawing out the item dropdown combo
    private void DrawItem(in EquipDrawData data, out string label,bool clear, bool open) {
        // begin making the item combo.
        var combo = _gameItemCombo[data._slot.ToIndex()];
        label = combo.Label;
        if (!data._locked && open) {
            UIHelpers.OpenCombo($"##{combo.Label}");
            GagSpeak.Log.Debug($"{combo.Label}");
        }

        using var disabled = ImRaii.Disabled(data._locked);
        var change = combo.Draw(data._gameItem.Name, data._gameItem.ItemId, _comboLength, _comboLength);
        if (change && !data._gameItem.Equals(combo.CurrentSelection)) {
            data.SetGameItem(combo.CurrentSelection);
            _config.Save();
        }
        // if (clear || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
        //     data.ResetGameItem();
        //     GagSpeak.Log.Debug($"[WardrobeTab] Right Click processed, item reverted to none!");
        // }
    }

    // for drawing out the stain combo dropdown.
    private void DrawStain(in EquipDrawData data) {
        var       found    = _stainData.TryGetValue(data._gameStain, out var stain);
        using var disabled = ImRaii.Disabled(data._locked);

        if (_stainCombo.Draw($"##stain{data._slot}", stain.RgbaColor, stain.Name, found, stain.Gloss, _comboLength)) {
            if (_stainData.TryGetValue(_stainCombo.CurrentSelection.Key, out stain)) {
                data.SetGameStain(stain.RowIndex);
                GagSpeak.Log.Debug($"[WardrobeTab] Stain Changed: {stain.RowIndex}");
            }
            else if (_stainCombo.CurrentSelection.Key == Stain.None.RowIndex) {
                //data.StainSetter(Stain.None.RowIndex);
                GagSpeak.Log.Debug($"[WardrobeTab] Stain Changed: None");
            }
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            data.ResetGameStain();
            GagSpeak.Log.Debug($"[WardrobeTab] Right Click processed, stain reverted to none!");
        }
    }
}
