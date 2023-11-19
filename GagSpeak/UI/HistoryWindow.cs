﻿
using Dalamud.Plugin;
using ImGuiNET;
using Dalamud.Interface.Colors;
using System.Linq;
using Dalamud.Interface.Windowing;
using System.Numerics;
using GagSpeak.Services;

using System;
using GagSpeak.Data;
using GagSpeak.UI.GagListings;
using GagSpeak.UI.Helpers;
using Dalamud.Interface.Utility.Table;

namespace GagSpeak.UI;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class HistoryWindow : Window //, IDisposable
{
    // Private readonly variables for help in making the history window
    private readonly HistoryService _historyService;
    private readonly GagSpeakConfig _config;
    private readonly GagListingsDrawer _gagListingsDrawer;

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
        ImGui.Columns(1);

        // draw out the debug window so we can keep track
        DrawDebug();

    }

    private void DrawDebug() {
        ImGui.Text("DEBUG INFORMATION:");
        try
        {
            ImGui.Text($"Fresh Install?: {_config.FreshInstall} || Is Enabled?: {_config.Enabled} || In Dom Mode?: {_config.InDomMode}");
            ImGui.Text($"Debug Mode?: {_config.DebugMode} || In DirectChatGarbler Mode?: {_config.DirectChatGarbler}");
            ImGui.Text($"Safeword: {_config.Safeword}");
            ImGui.Text($"Friends Only?: {_config.friendsOnly} || Party Only?: {_config.partyOnly} || Whitelist Only?: {_config.whitelistOnly}");
            ImGui.Text($"Garble Level: {_config.GarbleLevel}");
            ImGui.Text($"Process Translation Interval: {_config.ProcessTranslationInterval} || Max Translation History: {_config.TranslationHistoryMax}");
            ImGui.Text($"Total Gag List Count: {_config.GagTypes.Count}");
            ImGui.Text("Selected GagTypes: ||"); ImGui.SameLine(); foreach (var gagType in _config.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
            ImGui.Text("Selected GagPadlocks: ||"); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in _config.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} ||");};
            ImGui.Text("Selected GagPadlocks Passwords: ||"); ImGui.SameLine(); foreach (var gagPadlockPassword in _config.selectedGagPadlocksPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} ||"); };
            ImGui.Text("Selected GagPadlock Timers: ||"); ImGui.SameLine(); foreach (var gagPadlockTimer in _config.selectedGagPadLockTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} ||"); };
            ImGui.Text("Selected GagPadlocks Assigners: ||"); ImGui.SameLine(); foreach (var gagPadlockAssigner in _config.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} ||"); };
            ImGui.Text($"Translatable Chat Types:");
            foreach (var chanel in _config.Channels) { ImGui.SameLine(); ImGui.Text(chanel.ToString()); };
            ImGui.Text($"Current ChatBox Channel: {ChatChannel.GetChatChannel()} || Requesting Info: {_config.SendInfoName} || Accepting?: {_config.acceptingInfoRequests}");
            ImGui.Text("Whitelist:"); ImGui.Indent();
            foreach (var whitelistPlayerData in _config.Whitelist) {
                ImGui.Text(whitelistPlayerData.name);
                ImGui.Indent();
                ImGui.Text($"Relationship to this Player: {whitelistPlayerData.relationshipStatus}");
                ImGui.Text($"Commitment Duration: {whitelistPlayerData.GetCommitmentDuration()}");
                ImGui.Text($"Locked Live Chat Garbler: {whitelistPlayerData.lockedLiveChatGarbler}");
                ImGui.Text($"Selected GagTypes: || "); ImGui.SameLine(); foreach (var gagType in whitelistPlayerData.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
                ImGui.Text($"Selected GagPadlocks: || "); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in whitelistPlayerData.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} || ");};
                ImGui.Text($"Selected GagPadlocks Passwords: || "); ImGui.SameLine(); foreach (var gagPadlockPassword in whitelistPlayerData.selectedGagPadlocksPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} || "); };
                ImGui.Text($"Selected GagPadlocks Assigners: || "); ImGui.SameLine(); foreach (var gagPadlockAssigner in whitelistPlayerData.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} || "); };
                ImGui.Unindent();
            }
            ImGui.Unindent();
            ImGui.NewLine();
            ImGui.Text("Padlock Identifiers Variables:");
            // output debug messages to display the gaglistingdrawers boolean list for _islocked, _adjustDisp. For each padlock identifer, diplay all of its public varaibles
            ImGui.Text($"Listing Drawer _isLocked: ||"); ImGui.SameLine(); foreach(var index in _config._isLocked) { ImGui.SameLine(); ImGui.Text($"{index} ||"); };
            ImGui.Text($"Listing Drawer _adjustDisp: ||"); ImGui.SameLine(); foreach(var index in _gagListingsDrawer._adjustDisp) { ImGui.SameLine(); ImGui.Text($"{index} ||"); };
            var width = ImGui.GetContentRegionAvail().X / 3;
            foreach(var index in _config._padlockIdentifier) {
                ImGui.Columns(3,"DebugColumns", true);
                ImGui.SetColumnWidth(0,width); ImGui.SetColumnWidth(1,width); ImGui.SetColumnWidth(2,width);
                ImGui.Text($"Input Password: {index._inputPassword}"); ImGui.NextColumn();
                ImGui.Text($"Input Combination: {index._inputCombination}"); ImGui.NextColumn();
                ImGui.Text($"Input Timer: {index._inputTimer}");ImGui.NextColumn();
                ImGui.Text($"Stored Password: {index._storedPassword}");ImGui.NextColumn();
                ImGui.Text($"Stored Combination: {index._storedCombination}");ImGui.NextColumn();
                ImGui.Text($"Stored Timer: {index._storedTimer}");ImGui.NextColumn();
                ImGui.Text($"Padlock Type: {index._padlockType}");ImGui.NextColumn();
                ImGui.Columns(1);
                ImGui.NewLine();
            }
             
        }
        catch (Exception e)
        {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching config in debug: {e}");
            ImGui.NewLine();
        }
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakHistory###GagSpeakHistory";    
}

#pragma warning restore IDE1006