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

namespace GagSpeak.UI.Tabs.ConfigSettingsTab;

public class ConfigSettingsTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;
    // other stuff here

    public ReadOnlySpan<byte> Label
        => "General"u8;

    // Draw the content for the window of the General Tab
    public void DrawContent()
    {
        // Definitely need to refine the ImGui code here, but this is a good start.

        // First, declare a space for people to type in their safeword
        ImGui.InputText("Safeword", ref _config.Safeword, 128);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("This Safeword let's you override gags lock restrictions, but wont be able to gag again for awhile if you do.");
        }
        // Below this, put a horizontal line.
        ImGui.NewLine();
        ImGui.Separator();

        // In this line, include 3 checkboxes. One for FriendOnly, one for PartyOnly, one for WhitelistOnly
        ImGui.Checkbox("Only Friends", ref _friendsOnly);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Will not process /gag commands unless they are recieved from a player on your friend list.");
        }
        ImGui.SameLine(); // This just ensures it happens on the same line
        ImGui.Checkbox("Only Party Members", ref _partyOnly);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Will not process /gag commands unless they are recieved from a player in your party.");
        }
        ImGui.SameLine();
        ImGui.Checkbox("Only Whitelist", ref _whitelistOnly);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Will not process /gag commands unless they are recieved from a player in your plugins whitelist.");
        }

        // Below this is a debug option. When checked, display notable info.
        ImGui.Checkbox("Debug Logging", ref _debug);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip( "Enable logging for debug purposes.");
        }
        // Show Debug Menu when Debug logging is enabled
        if (_config.DebugMode) {
            ImGui.Text("DEBUG INFORMATION:");
            try
            {
                ImGui.Text($"Fresh Install?: {Configuration.FreshInstall}");
                ImGui.Text($"Is Enabled?: {Configuration.Enabled}");
                ImGui.Text($"Friends Only?: {Configuration.friendsOnly}");
                ImGui.Text($"Party Only?: {Configuration.partyOnly}");
                ImGui.Text($"Whitelist Only?: {Configuration.whitelistOnly}");
                ImGui.Text($"Garble Level: {Configuration.GarbleLevel}");
                ImGui.Text($"Process Translation Interval: {Configuration.ProcessTranslationInterval}");
                ImGui.Text($"Max Translation History: {Configuration.TranslationHistoryMax}");
                ImGui.Text($"Total Gag List Count: {Configuration.GagTypes.Count}");
                ImGui.Text("Selected GagTypes:"); ImGui.SameLine(); ImGui.Text($"{Configuration.selectedGagTypes.Count}"); ImGui.SameLine();
                foreach (var gagType in Configuration.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
                ImGui.Text("Selected GagPadlocks:"); ImGui.SameLine(); ImGui.Text($"{Configuration.selectedGagPadlocks.Count}"); ImGui.SameLine();
                foreach (GagPadlocks gagPadlock in Configuration.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text(gagPadlock.ToString()); };
    
                ImGui.Text($"Translatable Chat Types:");
                foreach (var chanel in Configuration.Channels) { ImGui.SameLine(); ImGui.Text(chanel.ToString()); };
                // Eventually, display the following:
                // Layer 1 Gag Type, Is Locked?, Lock Type (if Owner Locked, display owner name), Gag muffle Level,
                // Layer 2 Gag Type, Is Locked?, Lock Type (if Owner Locked, display owner name), Gag muffle Level,
                // Layer 3 Gag Type, Is Locked?, Lock Type (if Owner Locked, display owner name), Gag muffle Level,
                // Total Garble Level
                // Gag Capacity (should max at 3)
                // Safeword
                // Safeword Cooldown Timer
            }
            catch (Exception e)
            {
                ImGui.Text($"Error while fetching config in debug: {e}");
            }
        }
        // Below this, put a horizontal line.
        ImGui.End();
    }
}