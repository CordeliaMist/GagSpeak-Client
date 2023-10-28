using System;
using ImGuiNET;
using OtterGui.Widgets;
using Dalamud.Game.ClientState.Objects.Enums;
using System.Numerics;
using System.Text.RegularExpressions;

// practicing modular design
namespace GagSpeak.UI.Tabs.WhitelistTab;

public class WhitelistTab : ITab
{
    // When going back through this, be sure to try and reference anything possible to include from the glamourer convention, since it is more modular.
    private readonly GagSpeakConfig _config; // snag the conmfig from the main plugin for obvious reasons
    private int _currentWhitelistItem; // store a value so we know which item is selected in whitelist

    // Set label for whitelist tab
    public ReadOnlySpan<byte> Label
        => "Whitelist"u8;

    // Helper function to clean senders name off the list of clientstate objects
    public static string CleanSenderName(string senderName) {
        string[] senderStrings = SplitCamelCase(RemoveSpecialSymbols(senderName)).Split(" ");
        string playerSender = senderStrings.Length == 1 ? senderStrings[0] : senderStrings.Length == 2 ?
            (senderStrings[0] + " " + senderStrings[1]) :
            (senderStrings[0] + " " + senderStrings[2]);
        return playerSender;
    }

    // Helper functions for parsing payloads and clientstruct information
    public static string SplitCamelCase(string input) {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
    public static string RemoveSpecialSymbols(string value) {
        Regex rgx = new Regex(@"[^a-zA-Z:/._\ -]");
        return rgx.Replace(value, "");
    }



    // Draw the content for the window of the Whitelist Tab
    public void DrawContent() {
        // Let us begin by creating an array of strings that store the whitelist of appended character names
        string[] whitelist = _config.Whitelist.ToArray(); // Take whitelist list<string> from config and put into array of str.

        // If the whitelist is empty, then we should set the whitelist to "None"
        if (whitelist.Length == 0) {
            whitelist = ["None"];
        }

        // Set the next item width to the max content region in X direction
        ImGui.SetNextItemWidth(ImGui.GetContentRegionMax().X);

        // Create a listbox for the whitelist FORMAT:
        // [string label] - the label ID for the listbox
        // [ref int current_item] - what is displayed at the top of the list
        // [string[] items] - What content goes into the list
        // [int items_count] - How many items are there?
        // [int height_in_items] - How many items from that list to display at once.
        ImGui.ListBox("##whitelist", ref _currentWhitelistItem, whitelist, whitelist.Length, 10);

        // Create a bool for if the player is targetted (more detail on this later after experimentation)
        bool playerTargetted = Services.ClientState.LocalPlayer != null && Services.ClientState.LocalPlayer.TargetObject != null;
        // Create a bool for if the player is close enough to the targetted player (more detail on this later after experimentation)
        bool playerCloseEnough = playerTargetted && Vector3.Distance( Services.ClientState.LocalPlayer.Position, Services.ClientState.LocalPlayer.TargetObject.Position) < 1;
        
        // Message to display based on target proximity
        string targetedPlayerText = "Add Targetted Player"; // Displays if no target
        if (!playerTargetted) {
            targetedPlayerText += " (No Target)"; // If not tagetting a player, display "No Target"
            ImGui.BeginDisabled(); // Disable the button since no target to add
        } else if (playerTargetted && !playerCloseEnough) {
            targetedPlayerText += " (Too Far)"; // If target is too far, display "Too Far"
            ImGui.BeginDisabled(); // Disable the button since target is too far
        }

        // Create a button for adding the targetted player to the whitelist, assuming they are within proxy.
        if (ImGui.Button(targetedPlayerText)) {
            // The Null warnings are hinting that there is a possibility there could be none
            // However, the checks above that return to the bools safeguard this for us.
            if (Services.ClientState.LocalPlayer.TargetObject.ObjectKind == ObjectKind.Player) {
                // IF the object type is another player
                string senderName = CleanSenderName(Services.ClientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                // And now, if the player is not already in our whitelist, we will add them. Otherwise just do nothing.
                if (!_config.Whitelist.Contains(senderName)) {
                    _config.Whitelist.Add(senderName);
                }
                // Save(); <-- Replace this with a config update down the line!
            }
        }

        // If the player is not targetted or not close enough, end the disabled button
        if (!playerTargetted || !playerCloseEnough) {
            ImGui.EndDisabled();
        }
        ImGui.SameLine();
        // Also give people the option to remove someone from the whitelist.
        if (ImGui.Button("Remove Selected Player")) {
            _config.Whitelist.Remove(_config.Whitelist[_currentWhitelistItem]);
            // Save(); change to config update later
        }

        // Display text
        ImGui.TextWrapped("Add users to your whitelist to either use gag commands on them / recieve any from them.");
        ImGui.Dummy(new Vector2(0, 10));
    }

}