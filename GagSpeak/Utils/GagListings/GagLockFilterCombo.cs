using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using OtterGui;
using OtterGui.Raii;
﻿using Dalamud.Game.Text;
using Dalamud.Plugin;
using System.Diagnostics;
using Num = System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using OtterGui.Widgets;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.Events;
using GagSpeak.Chat;

namespace GagSpeak.UI.GagListings;

public sealed class GagLockFilterCombo
{
    private GagSpeakConfig _config;
    private string _comboSearchText;
    private GagPadlocks _selectedGagPadlocks;
    private string _displayLabel; // the "current item"

    public GagLockFilterCombo(GagPadlocks displayLabel, GagSpeakConfig config) {
        _comboSearchText = string.Empty;
        _selectedGagPadlocks = displayLabel;
        _config = config;
    } 

    /// <summary>
    /// This function draws ImGui's Combo list, but with a search filter. (and for GagPadlocks Enum)
    /// <list type="bullet">
    /// <item><c>ID</c><param name="ID"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>layerindex</c><param name="layerIndex"> - a list where the stored selection from the list is saved</param></item>
    /// </list>
    /// </summary>
    public void Draw(int ID, GagPadlocks label, int layerIndex) {
        try
        {
            using( var gagLockCombo = ImRaii.Combo($"##{ID}_Enum",  _config.selectedGagPadlocks[layerIndex].ToString(), 
                                      ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) 
            {
                //ImGui.SetKeyboardFocusHere(); // focus our text into the filter thingy
                if( gagLockCombo ) { // Assign it an ID if combo is sucessful.
                    // add the popup state
                    using var id = ImRaii.PushId($"##{ID}_Enum"); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X); // Set filter length to full
                    // if( ImGui.InputTextWithHint("##filter", "Filter...", ref _comboSearchText, 255 ) ) { // Draw filter bar
                    // // Filter logic for GagPadlocks enum based on _comboSearchText
                    //     var filteredValues = Enum.GetValues(typeof(GagPadlocks))
                    //         .Cast<GagPadlocks>()
                    //         .Where(x => x.ToString().ToLower().Contains(_comboSearchText.ToLower()));

                    //     foreach (var item in filteredValues) {
                    //         if (ImGui.Selectable(item.ToString(), _config.selectedGagPadlocks[layerIndex] == item)) {
                    //             _config.selectedGagPadlocks[layerIndex] = item;
                    //             GagSpeak.Log.Debug($"GagSpeak: GagPadlock changed to {item}");

                    //             _comboSearchText = string.Empty;
                    //             ImGui.CloseCurrentPopup();
                    //             return;
                    //         }
                    //     }
                    // }

                    foreach (var item in Enum.GetValues(typeof(GagPadlocks)).Cast<GagPadlocks>()) {
                        if (ImGui.Selectable(item.ToString(), _config.selectedGagPadlocks[layerIndex] == item)) {
                            _config.selectedGagPadlocks[layerIndex] = item;
                            GagSpeak.Log.Debug($"GagSpeak: GagPadlock changed to {item}");

                            _comboSearchText = string.Empty;
                            ImGui.CloseCurrentPopup();
                            _config.Save();
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            GagSpeak.Log.Debug(e.ToString());
        }
    }
}