using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using OtterGui;
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
using GagSpeak.UI.GagListings;
using Dalamud.Interface.Utility.Raii;

namespace GagSpeak.UI.Tabs.GeneralTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class GeneralTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;

    private readonly GagListingsDrawer _gagListingsDrawer;
    private string? _tempSafeword; // for initializing a temporary safeword for the text input field
    // style variables
    private bool _isLocked;
    
    public GeneralTab(GagListingsDrawer gagListingsDrawer, GagSpeakConfig config)
    {
        _isLocked = false;
        // Set the readonlys
        _config = config;
        _gagListingsDrawer = gagListingsDrawer;

        // draw out our gagpadlock filter combo listings


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
        using (var child2 = ImRaii.Child("GeneralTabChild")) {
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
        _gagListingsDrawer.PrepareGagListDrawing(); // prepare our listings

        // draw our listings
        _gagListingsDrawer.DrawGagAndLockListing(00, _config.selectedGagTypes[0], _config.selectedGagPadlocks[0], 0, "Gag Slot 1", _isLocked);
        ImGui.Separator();
        _gagListingsDrawer.DrawGagAndLockListing(01, _config.selectedGagTypes[1], _config.selectedGagPadlocks[1], 1, "Gag Slot 2", _isLocked);
        ImGui.Separator();
        _gagListingsDrawer.DrawGagAndLockListing(02, _config.selectedGagTypes[2], _config.selectedGagPadlocks[2], 2, "Gag Slot 3", _isLocked);
    
        // let users know information about the plugin
        ImGui.NewLine();
        ImGui.Separator();
        ImGui.Text("GagSpeak Pre-Release v0.1");
        ImGui.NewLine();
        // let the user then know that we have yet to polish the UI, but that all commands and interfaces should be accessable.
        ImGui.Text("This is a pre-release version of GagSpeak. The UI is not yet polished,");
        ImGui.Text("but all commands and interfaces should be accessable.");
        ImGui.NewLine();
        ImGui.Text("Feature Plan: Whitelist User Profile actions, proper sub/dom mode & more.");
        ImGui.Text("Should probably figure out why window flickers every time you left-click.");

        ImGui.Dummy(new Vector2(0, 10));
    }
}

#pragma warning restore IDE1006