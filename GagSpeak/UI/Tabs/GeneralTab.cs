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

namespace GagSpeak.UI.Tabs.GeneralTab;

public class GeneralTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;

    public ReadOnlySpan<byte> Label
        => "General"u8;


    // ADDITIONAL HELPER FUNCTIONS GO HERE:


    // Draw the content for the window of the General Tab
    public void DrawContent() {
        // Definitely need to refine the ImGui code here, but this is a good start.

        // First, declare a space for people to type in their safeword
        ImGui.InputText("Safeword", ref _safeword, 128);
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("This Safeword let's you override gags lock restrictions, but wont be able to gag again for awhile if you do.");
        }

        // Below this, put a horizontal line.
        ImGui.NewLine();
        ImGui.Separator();

        // Draw a combo selection Filter for the GagType Selection
        UIHelpers.DrawFilterCombo(Configuration.GagTypes, Configuration.selectedGagTypes[0], Configuration.selectedGagTypes, 0);



        // Below this, put a horizontal line.
        ImGui.NewLine();
        ImGui.Separator();

        // in this section, display the checkboxes for all of the different chat types, 
        // allowing the user to select only the chats they want their garbles messages to process through.

        var i = 0;
        ImGui.Text("Enabled channels:");
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Which chat channels to show bubbles for.");
        }

        ImGui.Columns(4);
        foreach (var e in (XivChatType[]) Enum.GetValues(typeof(XivChatType))) {
            // If the chat type is a valid chat type
            if (_yesno[i]) {
                // See if it is already enabled by default
                var enabled = _channels.Contains(e);
                // If a checkbox exists (it always will)...
                if (ImGui.Checkbox($"{e}", ref enabled)) {
                    // See If checkbox is clicked, If not, add to list of enabled channels, otherwise, remove it.
                    if (enabled) _channels.Add(e);
                    else _channels.Remove(e);
                }
                ImGui.NextColumn();
            }
            i++;
        }

        // Set the columns back to 1 now and space over to next section
        ImGui.Columns(1);
        ImGui.NewLine();
        ImGui.Separator();
        ImGui.NewLine();

        // Below this, have a button to save and close the config. Next to it, have a button to link to my Ko-Fi
        // If the save & close button is clicked
        if (ImGui.Button("Save and Close Config")) {
            // Save the config, and toggle _config, forcing it to close
            SaveConfig();
            _config = false;
        }

        // In that same line...
        ImGui.SameLine();
        // Configure the style for our next button
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        ImGui.Text(" ");
        ImGui.SameLine();

        // And now have that button be for the Ko-Fi Link
        if (ImGui.Button("Tip Cordy for her hard work!")) {
            ImGui.SetTooltip( "Only if you want to though!");
            Process.Start(new ProcessStartInfo {FileName = "https://ko-fi.com/cordeliamist", UseShellExecute = true});
        }

        ImGui.PopStyleColor(3);
        ImGui.End();
    }

        // Structure Outline:
}