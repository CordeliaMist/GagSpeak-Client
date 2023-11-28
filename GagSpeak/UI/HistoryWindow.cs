﻿using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using GagSpeak.Services;
using GagSpeak.UI.GagListings;

namespace GagSpeak.UI;
/// <summary> This class is used to handle the history window. </summary>
public class HistoryWindow : Window //, IDisposable
{
    private readonly HistoryService     _historyService;
    private readonly GagSpeakConfig     _config;
    private readonly GagListingsDrawer  _gagListingsDrawer;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryWindow"/> class.
    /// <list type="bullet">
    /// <item><c>pluginInt</c><param name="pluginInt"> - The DalamudPluginInterface.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>historyService</c><param name="historyService"> - The history service.</param></item>
    /// <item><c>gagListingsDrawer</c><param name="gagListingsDrawer"> - The gag listings drawer.</param></item>
    /// </list> </summary>
    public HistoryWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, HistoryService historyService,
    GagListingsDrawer gagListingsDrawer) : base(GetLabel()) {
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;
        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(300, 400),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };
        _historyService = historyService;
        _config = config;
        _gagListingsDrawer = gagListingsDrawer;
    }

    /// <summary> This function is used to draw the history window. </summary>
    public override void Draw() {
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
                ImGui.TextWrapped(translation.Input);
                ImGui.NextColumn();
                // Then the output
                ImGui.TextWrapped(translation.Output);
                ImGui.NextColumn();
                // Place a seperator between each message for good measure.
                ImGui.Separator();
            }
            // Window should be finished drawing now
        } else {
            // If the translation.count is 0, display a message to let the user know nothing is translated yet.
            ImGui.Text("Nothing has been translated yet.");
        }
        ImGui.Columns(1);
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakHistory###GagSpeakHistory";    
}