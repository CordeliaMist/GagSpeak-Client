﻿using Dalamud.Game.Text;
using Dalamud.Plugin;
using ImGuiNET;
using Dalamud.Interface.Colors;
using System.Linq;
using System;
using System.Diagnostics;
using Num = System.Numerics;


// Also taken from sillychat

namespace GagSpeak
{
    /// History window for the GagSpeak plugin.
    public unsafe partial class GagSpeak : IDalamudPlugin
    {
        private void HistoryWindowUI()
        {
            // Attempt to draw the window
            try
            {
                // If the history service is already processing, back out
                if (HistoryService.IsProcessing) return;
                // Otherwise, set tralsnations to the list of translations from the history service
                var translations = HistoryService.Translations.ToList();
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
            }
            // If the window could not be drawn, just ignore it
            catch
            {
                // ignored
            }
        }
    }
}