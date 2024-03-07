using System;
using ImGuiNET;
using OtterGui.Raii;
using System.Linq;
using GagSpeak.Events;
using GagSpeak.Gagsandlocks;
using GagSpeak.CharacterData;

namespace GagSpeak.UI.Equipment;

/// <summary> This class is used to handle the gag lock filter combo box. </summary>
public sealed class GagLockFilterCombo
{
    private GagSpeakConfig  _config;              // the config for the plugin
    private string          _comboSearchText;     // the search text for the combo box
    
    /// <summary> 
    /// Initializes a new instance of the <see cref="GagLockFilterCombo"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    public GagLockFilterCombo(GagSpeakConfig config) {
        _comboSearchText = string.Empty;
        _config = config;
    } 


    /// <summary>
    /// This function draws the players dropdown combo for the padlocks.
    /// <list type="bullet">
    /// <item><c>ID</c><param name="ID"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>layerindex</c><param name="layerIndex"> - a list where the stored selection from the list is saved</param></item>
    /// </list> </summary>
    public void Draw(int ID, CharacterHandler characterHandler, int layerIndex, float width) {
        try
        {
            ImGui.SetNextItemWidth(width);
            using( var gagLockCombo = ImRaii.Combo($"##{ID}_Enum",  _config.padlockIdentifier[layerIndex]._padlockType.ToString(), 
            ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) {
                if( gagLockCombo ) { // Assign it an ID if combo is sucessful.
                    // add the popup state
                    using var id = ImRaii.PushId($"##{ID}_Enum"); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(width); // Set filter length to full

                    foreach (var item in Enum.GetValues(typeof(Padlocks)).Cast<Padlocks>()) {
                        if (ImGui.Selectable(item.ToString(), characterHandler.playerChar._selectedGagPadlocks[layerIndex] == item)) {
                            _config.padlockIdentifier[layerIndex]._padlockType = item; // update the padlock identifier label
                            _config.Save();
                            _comboSearchText = string.Empty;
                            ImGui.CloseCurrentPopup();
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e) {
            GSLogger.LogType.Debug(e.ToString());
        }
    }


    /// <summary>
    /// This function draws the whitelisted characters dropdown combo for the padlocks.
    /// <list type="bullet">
    /// <item><c>ID</c><param name="ID"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>layerindex</c><param name="layerIndex"> - a list where the stored selection from the list is saved</param></item>
    /// </list> </summary>
    public void Draw(int ID, ref string label, CharacterHandler characterHandler, int whitelistIdx, int layerIndex, float width) {
        try
        {
            // set the next item width to the width we want
            ImGui.SetNextItemWidth(width);
            // draw the combo box
            using( var gagLockCombo = ImRaii.Combo($"##{ID}_Enum",  label.ToString(), ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) {
                // Assign it an ID if combo is sucessful.
                if( gagLockCombo ) {
                    // add the popup state
                    using var id = ImRaii.PushId($"##{ID}_Enum"); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(width); // Set filter length to full
                    // draw the combo box
                    foreach (var item in Enum.GetValues(typeof(Padlocks)).Cast<Padlocks>()) {
                        // if the item is selected, set the label to the item and save the config
                        if (ImGui.Selectable(item.ToString(), characterHandler.whitelistChars[whitelistIdx]._selectedGagPadlocks[layerIndex] == item)) {
                            label = item.ToString(); // update label
                            _comboSearchText = string.Empty;
                            ImGui.CloseCurrentPopup();
                            return;
                        }
                    }
                }
            }
        } catch (Exception e) {
            GSLogger.LogType.Debug(e.ToString());
        }
    }
}