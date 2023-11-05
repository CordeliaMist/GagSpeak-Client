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

namespace GagSpeak.UI.Tabs.ConfigSettingsTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class ConfigSettingsTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;
    private readonly UiBuilder _uiBuilder;

    public ConfigSettingsTab(GagSpeakConfig config, UiBuilder uiBuilder)
    {
        // Set the readonlys
        _config = config;
        _uiBuilder = uiBuilder;
    }

    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label
        => "ConfigSettings"u8;

    /// <summary>
    /// This Function draws the content for the window of the ConfigSettings Tab
    /// </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;


        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("ConfigSettingsChild"))
        {
            DrawHeader();
            DrawConfigSettings();
        }
    }

    private void DrawHeader()
        => WindowHeader.Draw("Configuration & Settings", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    // Draw the actual config settings
    private void DrawConfigSettings() {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##ConfigSettingsPanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // See "setpanel.cs" for other checkbox options that base off the above ^^
        ImGui.Text("Gag Configuration:");
        // Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        Checkbox("Only Friends", "Only processes process /gag (target) commands from others if they are on your friend list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.friendsOnly, v => _config.friendsOnly = v);

        // Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        Checkbox("Only Party Members", "Only processes /gag (target) commands from others if they are in your current party list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.partyOnly, v => _config.partyOnly = v);

        // Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        Checkbox("Only Whitelist", "Only processes /gag (target) commands from others if they are in your whitelist.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.whitelistOnly, v => _config.whitelistOnly = v);

        // Checkbox to display debug information
        Checkbox("Debug Display", "Displays information for plugin variables. For developer", _config.DebugMode, v => _config.DebugMode = v);
        // Checkbox will dictate if only players from their party are allowed to use /gag (target) commands on them.
        
        // Show Debug Menu when Debug logging is enabled
        if (_config.DebugMode) {
            DrawDebug();
        }

        ImGui.Separator();

        // Display Enabled channels: (GagSpeak Only works in these channels)
        var i = 0;
        ImGui.Text("Enabled channels: (GagSpeak Only works in these channels)");
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Which chat channels to show bubbles for.");
        }

        ImGui.Columns(4);
        foreach (var e in (XivChatType[]) Enum.GetValues(typeof(XivChatType))) {
            // If the chat type is a valid chat type
            if (_config.ChannelsIsActive[i]) {
                // See if it is already enabled by default
                var enabled = _config.Channels.Contains(e);
                // If a checkbox exists (it always will)...
                if (ImGui.Checkbox($"{e}", ref enabled)) {
                    // See If checkbox is clicked, If not, add to list of enabled channels, otherwise, remove it.
                    if (enabled) _config.Channels.Add(e);
                    else _config.Channels.Remove(e);
                }
                ImGui.NextColumn();
            }
            i++;
        }

        // Set the columns back to 1 now and space over to next section
        ImGui.Columns(1);
    }

    /// <summary>
    /// This function draws a checkbox with a label and tooltip, and saves the value to the config.
    /// <list type="bullet">
    /// <item><c>label</c><param name="label"> - The label to display outside the checkbox</param></item>
    /// <item><c>tooltip</c><param name="tooltip"> - The tooltip to display when hovering over the checkbox</param></item>
    /// <item><c>current</c><param name="current"> - The current value of the checkbox</param></item>
    /// <item><c>setter</c><param name="setter"> - The setter for the checkbox</param></item>
    /// </list>
    /// </summary>
    private void Checkbox(string label, string tooltip, bool current, Action<bool> setter) {
        using var id  = ImRaii.PushId(label);
        var       tmp = current;
        if (ImGui.Checkbox(string.Empty, ref tmp) && tmp != current) {
            setter(tmp);
            _config.Save();
        }

        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker(label, tooltip);
    }

    /// <summary>
    /// This just literally displays extra information for debugging variables in game to keep track of them.
    /// </summary>
    private void DrawDebug() {
        ImGui.Text("DEBUG INFORMATION:");
        try
        {
            ImGui.Text($"Version: {_config.Version}");
            ImGui.Text($"Fresh Install?: {_config.FreshInstall}");
            ImGui.Text($"Is Enabled?: {_config.Enabled}");
            ImGui.Text($"Debug Mode?: {_config.InDomMode}");
            ImGui.Text($"Safeword: {_config.Safeword}");
            ImGui.Text($"Friends Only?: {_config.friendsOnly}");
            ImGui.Text($"Party Only?: {_config.partyOnly}");
            ImGui.Text($"Whitelist Only?: {_config.whitelistOnly}");
            ImGui.Text($"Garble Level: {_config.GarbleLevel}");
            ImGui.Text($"Process Translation Interval: {_config.ProcessTranslationInterval}");
            ImGui.Text($"Max Translation History: {_config.TranslationHistoryMax}");
            ImGui.Text($"Total Gag List Count: {_config.GagTypes.Count}");
            ImGui.Text("Selected GagTypes:"); ImGui.SameLine(); ImGui.Text($"{_config.selectedGagTypes.Count}"); ImGui.SameLine();
            foreach (var gagType in _config.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
            ImGui.Text("Selected GagPadlocks:"); ImGui.SameLine(); ImGui.Text($"{_config.selectedGagPadlocks.Count}"); ImGui.SameLine();
            foreach (GagPadlocks gagPadlock in _config.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text(gagPadlock.ToString()); };

            ImGui.Text($"Translatable Chat Types:");
            foreach (var chanel in _config.Channels) { ImGui.SameLine(); ImGui.Text(chanel.ToString()); };
            ImGui.Text($"Current ChatBox Channel: {_config.CurrentChannel.ToString()}");
            ImGui.Text("Whitelist:"); ImGui.Indent();
            foreach (var item in _config.Whitelist) { ImGui.Text(item); }
            ImGui.Unindent();
            // print the width of the imgui screen and the height
            ImGui.Text($"ImGui Screen Width: {ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X}");
            ImGui.Text($"ImGui Screen Height: {ImGui.GetWindowContentRegionMax().Y - ImGui.GetWindowContentRegionMin().Y}");

        }
        catch (Exception e)
        {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching config in debug: {e}");
            ImGui.NewLine();
        }
    }
}

#pragma warning restore IDE1006