﻿using Dalamud.Game.Text;
using Dalamud.Plugin;
using ImGuiNET;
using Dalamud.Interface.Colors;
using System.Linq;
using System;


using OtterGui.Custom;
using OtterGui.Widgets;

using System.Diagnostics;
using Num = System.Numerics;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface.Utility;

// Practicing Modular Design
using GagSpeak.Services;
using GagSpeak.Events;
using GagSpeak.UI.Tabs;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;
using Dalamud.IoC;
// Also taken from sillychat

namespace GagSpeak.UI;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class HistoryWindow : Window //, IDisposable
{
    // Private readonly variables for help in making the history window
    private readonly HistoryService _historyService;
    private readonly GagSpeakConfig _config;

    public HistoryWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, HistoryService historyService) : base(GetLabel()) {
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;

        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(700, 675),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };

        _historyService = historyService;
        _config = config;
    }

    public override void Draw() {
        try
        {
            // If the history service is already processing, back out
            if (_historyService.IsProcessing) return;
            // Otherwise, set tralsnations to the list of translations from the history service
            var translations = _historyService.Translations.ToList();
            // If the size of the list is more than 0, draw out the history to the window
            if (translations.Count > 0) {
                // Make the window 2 columns.
                ImGui.Columns(2);
                // Label column 1 as source, and column 2 as translation
                ImGui.TextColored(ImGuiColors.HealerGreen, "Source");
                ImGui.NextColumn();
                ImGui.TextColored(ImGuiColors.DPSRed, "Translation");
                ImGui.NextColumn();
                // Put in a seperator
                ImGui.Separator();
                // Now that labels are in place, we can display the source and translation of each message
                foreach (var translation in translations) {
                    // wrap tesxt into the space, and display the input
                    //ImGui.TextWrapped(translation.Input);
                    ImGui.NextColumn();
                    // Then the output
                    //ImGui.TextWrapped(translation.Output);
                    ImGui.NextColumn();
                    // Place a seperator between each message for good measure.
                    ImGui.Separator();
                }
                // Window should be finished drawing now
            } else {
                // If the translation.count is 0, display a message to let the user know nothing is translated yet.
                ImGui.Text("Nothing has been translated yet.");
            }
        }
        // If the window could not be drawn, just ignore it
        catch
        {
            // ignored
        }
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakHistory###GagSpeakHistory";    
}

#pragma warning restore IDE1006