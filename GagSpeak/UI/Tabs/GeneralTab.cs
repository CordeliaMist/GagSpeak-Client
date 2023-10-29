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

public class GeneralTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;
    private readonly UiBuilder _uiBuilder;
    private readonly UIHelpers _uiHelpers; // for drawing filter combo's
    private string? _tempSafeword; // for initializing a temporary safeword for the text input field

    
    public GeneralTab(GagSpeakConfig config, UiBuilder uiBuilder)
    {
        // Set the readonlys
        _config = config;
        _uiBuilder = uiBuilder;
    }

    // store our current safeword
    public string _currentSafeword = string.Empty;

    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label
        => "ConfigSettings"u8;

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
        => WindowHeader.Draw("Settings & Options", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    // Draw the actual general tab contents
    private void DrawGeneral() {

        // create a name variable for the safeword
        var safeword  = _tempSafeword ?? _config.Safeword;;
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
        _uiHelpers.DrawFilterCombo(_config.GagTypes, _config.selectedGagTypes[0], _config.selectedGagTypes, 0);
        ImGui.NewLine();

        // This will draw the filter combo for our first gag applier
        ImGui.Text("Currently Selected Gag (Layer 2):");
        _uiHelpers.DrawFilterCombo(_config.GagTypes, _config.selectedGagTypes[1], _config.selectedGagTypes, 1);
        ImGui.NewLine();

        // This will draw the filter combo for our first gag applier
        ImGui.Text("Currently Selected Gag (Layer 3):");
        _uiHelpers.DrawFilterCombo(_config.GagTypes, _config.selectedGagTypes[1], _config.selectedGagTypes, 2);
        ImGui.NewLine();



        ImGui.PopStyleColor(3);
        ImGui.End();
    }
}