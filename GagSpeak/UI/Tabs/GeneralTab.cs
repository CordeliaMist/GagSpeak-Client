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
using GagSpeak.UI;
using GagSpeak.Events;
using GagSpeak.Chat;

namespace GagSpeak.UI.Tabs.GeneralTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class GeneralTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;
    private string? _tempSafeword; // for initializing a temporary safeword for the text input field

    
    public GeneralTab(GagSpeakConfig config)
    {
        // Set the readonlys
        _config = config;
    }

    // store our current safeword
    public string _currentSafeword = string.Empty;

    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label
        => "General"u8;

    /// <summary>
    /// This Function draws the content for the window of the General Tab
    /// </summary>
    public void DrawContent() {
        // Definitely need to refine the ImGui code here, but this is a good start.
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        
        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("GeneralTabChild"))
        {
            DrawHeader();
            DrawGeneral();
        }
    }

    // Draw the header stuff
    private void DrawHeader()
        => WindowHeader.Draw("Gag Selections / Inspector", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    // Draw the actual general tab contents
    private void DrawGeneral() {

        // create a name variable for the safeword
        var safeword  = _tempSafeword ?? _config.Safeword;;
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        ImGui.SetNextItemWidth(330 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputText("Set Safeword##Safeword", ref safeword, 128, ImGuiInputTextFlags.None))
            _tempSafeword = safeword;

        if (ImGui.IsItemDeactivated()) {
            _config.Safeword = safeword;
            _tempSafeword = null;
        }
        
        ImGui.Separator();
        // Now let's draw our 3 gag appliers

        // This will draw the filter combo for our first gag applier
        ImGui.Text("Currently Selected Gag (Layer 1):");
        DrawFilterCombo(_config.GagTypes, _config.selectedGagTypes[0], _config.selectedGagTypes, 0);
        ImGui.NewLine();

        // This will draw the filter combo for our first gag applier
        ImGui.Text("Currently Selected Gag (Layer 2):");
        DrawFilterCombo(_config.GagTypes, _config.selectedGagTypes[1], _config.selectedGagTypes, 1);
        ImGui.NewLine();

        // This will draw the filter combo for our first gag applier
        ImGui.Text("Currently Selected Gag (Layer 3):");
        DrawFilterCombo(_config.GagTypes, _config.selectedGagTypes[2], _config.selectedGagTypes, 2);
        ImGui.NewLine();
    }

    /// <summary>
    /// This function draws ImGui's Combo list, but with a search filter.
    /// <list type="bullet">
    /// <item><c>contentList</c><param name="contentList"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>selectedTypes</c><param name="selectedTypes"> - a list where the stored selection from the list is saved</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - the index of the selectedTypes list to saved the selected option to</param></item>
    /// </list>
    /// </summary>
    public void DrawFilterCombo(Dictionary<string, int> contentList, string label, List<string> selectedTypes, int layerIndex) {
        // create an empty string for the search text
        var ComboSearchText = string.Empty;
        // Create the combo
        using( var gagTypeOneCombo = ImRaii.Combo( label, selectedTypes[layerIndex], ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) {
            // Assign it an ID if combo is sucessful.
            if( gagTypeOneCombo ) {
                using var id = ImRaii.PushId( label ); // Push an ID for the combo box (based on label / name)
                ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X); // Set filter length to full
                
                // Draw filter bar
                if( ImGui.InputTextWithHint("##filter", "Filter...", ref ComboSearchText, 255 ) ) {
                    // Query: if the search bar is empty, display all the gag types, otherwise, display only search matches
                    contentList = string.IsNullOrEmpty(ComboSearchText) ? (
                        _config.GagTypes
                    ) : (
                        _config.GagTypes.Where(x=>x.Key.ToLower().Contains(ComboSearchText.ToLower())).ToDictionary(x=>x.Key, x=>x.Value)
                    );
                }
                
                // Now that we have our results, be sure to draw them! (does this so filter list remains visible)
                using var child = ImRaii.Child( "Child", new Vector2( ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X, 200),true);
                // We will draw out one listing for each item.
                foreach( var item in contentList.Keys ) {
                    // If our item is selected, set it and break
                    if( ImGui.Selectable( item, item == selectedTypes[layerIndex] ) ) {
                        selectedTypes[layerIndex] = item;
                        //SaveConfig(); // also update the config so we can see. (should either remove this or add something to update it.)
                        break;
                    }
                }
            }
        }
        // Personal Note - We cant put this in UI Helpers because UI helpers must remain
        // a static class in order to not depend on being part of a service.
    }
}

#pragma warning restore IDE1006