﻿using System;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using GagSpeak.Services;
using GagSpeak.UI.GagListings;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;

namespace GagSpeak.UI;
/// <summary> This class is used to show the debug menu in its own window. </summary>
public class DebugWindow : Window //, IDisposable
{
    private readonly GagSpeakConfig     _config;
    private readonly GagService         _gagService;
    private readonly FontService        _fontService;
    private readonly GagManager         _gagManager;
    private readonly GagListingsDrawer  _gagListingsDrawer;

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryWindow"/> class.
    /// <list type="bullet">
    /// <item><c>pluginInt</c><param name="pluginInt"> - The DalamudPluginInterface.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// 
    /// </list> </summary>
    public DebugWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, GagService gagService,
    GagListingsDrawer gagListingsDrawer, GagManager gagManager, FontService fontService) : base(GetLabel()) {
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;
        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(300, 400),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };
        _config = config;
        _fontService = fontService;
        _gagService = gagService;
        _gagManager = gagManager;
        _gagListingsDrawer = gagListingsDrawer;
    }

    /// <summary> This function is used to draw the history window. </summary>
    public override void Draw() {
        ImGui.Text("DEBUG INFORMATION:");
        try
        {
            // General information
            ImGui.Text($"Fresh Install?: {_config.FreshInstall} || Is Enabled?: {_config.Enabled} || In Dom Mode?: {_config.InDomMode}");
            ImGui.Text($"Debug Mode?: {_config.DebugMode} || In DirectChatGarbler Mode?: {_config.DirectChatGarbler}");
            ImGui.Text($"Safeword: {_config.Safeword}");
            ImGui.Text($"Friends Only?: {_config.friendsOnly} || Party Only?: {_config.partyOnly} || Whitelist Only?: {_config.whitelistOnly}");
            ImGui.Text($"ExperimentalGarblerMode: {_config.ExperimentalGarbler}");
            ImGui.Text($"Process Translation Interval: {_config.ProcessTranslationInterval} || Max Translation History: {_config.TranslationHistoryMax}");
            ImGui.Text($"Total Gag List Count: {_gagService._gagTypes.Count}");
            ImGui.Text("Selected GagTypes: ||"); ImGui.SameLine(); foreach (var gagType in _config.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
            ImGui.Text("Selected GagPadlocks: ||"); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in _config.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} ||");};
            ImGui.Text("Selected GagPadlocks Passwords: ||"); ImGui.SameLine(); foreach (var gagPadlockPassword in _config.selectedGagPadlocksPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} ||"); };
            ImGui.Text("Selected GagPadlock Timers: ||"); ImGui.SameLine(); foreach (var gagPadlockTimer in _config.selectedGagPadLockTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} ||"); };
            ImGui.Text("Selected GagPadlocks Assigners: ||"); ImGui.SameLine(); foreach (var gagPadlockAssigner in _config.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} ||"); };
            ImGui.Text($"Translatable Chat Types:");
            foreach (var chanel in _config.Channels) { ImGui.SameLine(); ImGui.Text(chanel.ToString()); };
            ImGui.Text($"Current ChatBox Channel: {ChatChannel.GetChatChannel()} || Requesting Info: {_config.SendInfoName} || Accepting?: {_config.acceptingInfoRequests}");
            
            // Whitelist uder information
            ImGui.Text("Whitelist:"); ImGui.Indent();
            foreach (var whitelistPlayerData in _config.Whitelist) {
                ImGui.Text(whitelistPlayerData.name);
                ImGui.Indent();
                ImGui.Text($"Relationship to this Player: {whitelistPlayerData.relationshipStatus}");
                ImGui.Text($"Commitment Duration: {whitelistPlayerData.GetCommitmentDuration()}");
                ImGui.Text($"Locked Live Chat Garbler: {whitelistPlayerData.lockedLiveChatGarbler}");
                ImGui.Text($"Pending Relationship Request: {whitelistPlayerData.PendingRelationRequestFromPlayer}");
                ImGui.Text($"Pending Relationship Request From You: {whitelistPlayerData.PendingRelationRequestFromYou}");
                ImGui.Text($"Selected GagTypes: || "); ImGui.SameLine(); foreach (var gagType in whitelistPlayerData.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
                ImGui.Text($"Selected GagPadlocks: || "); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in whitelistPlayerData.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} || ");};
                ImGui.Text($"Selected GagPadlocks Timers: || "); ImGui.SameLine(); foreach (var gagPadlockTimer in whitelistPlayerData.selectedGagPadlocksTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} || "); };
                ImGui.Text($"Selected GagPadlocks Assigners: || "); ImGui.SameLine(); foreach (var gagPadlockAssigner in whitelistPlayerData.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} || "); };
                ImGui.Unindent();
            }
            ImGui.Unindent();
            ImGui.Separator();

            // Padlock identifier Information
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
                ImGui.Text($"Padlock Assigner: {index._mistressAssignerName}");ImGui.NextColumn();
                ImGui.Columns(1);
                ImGui.NewLine();
            } // This extra one is just the whitelist padlock stuff
            ImGui.Columns(3,"DebugColumns", true);
            ImGui.SetColumnWidth(0,width); ImGui.SetColumnWidth(1,width); ImGui.SetColumnWidth(2,width);
            ImGui.Text($"Input Password: {_config._whitelistPadlockIdentifier._inputPassword}"); ImGui.NextColumn();
            ImGui.Text($"Input Combination: {_config._whitelistPadlockIdentifier._inputCombination}"); ImGui.NextColumn();
            ImGui.Text($"Input Timer: {_config._whitelistPadlockIdentifier._inputTimer}");ImGui.NextColumn();
            ImGui.Text($"Stored Password: {_config._whitelistPadlockIdentifier._storedPassword}");ImGui.NextColumn();
            ImGui.Text($"Stored Combination: {_config._whitelistPadlockIdentifier._storedCombination}");ImGui.NextColumn();
            ImGui.Text($"Stored Timer: {_config._whitelistPadlockIdentifier._storedTimer}");ImGui.NextColumn();
            ImGui.Text($"Padlock Type: {_config._whitelistPadlockIdentifier._padlockType}");ImGui.NextColumn();
            ImGui.Text($"Padlock Assigner: {_config._whitelistPadlockIdentifier._mistressAssignerName}");ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.NewLine();
            // For the gag manager information
            ImGui.Separator();
            ImGui.Text("Gag Manager Information:");
            // define the columns and the gag names
            ImGui.Columns(3, "GagColumns", true);
            ImGui.SetColumnWidth(0, width); ImGui.SetColumnWidth(1, width); ImGui.SetColumnWidth(2, width);
            ImGui.Text($"Gag Name: {_gagManager._activeGags[0]._gagName}"); ImGui.NextColumn();
            ImGui.Text($"Gag Name: {_gagManager._activeGags[1]._gagName}"); ImGui.NextColumn();
            ImGui.Text($"Gag Name: {_gagManager._activeGags[2]._gagName}"); ImGui.NextColumn();
            try {
            ImGui.PushFont(_fontService.UidFont);
            foreach (var gag in _gagManager._activeGags) {
                // Create a table for the relations manager
                using (var table = ImRaii.Table($"InfoTable_{gag._gagName}", 3, ImGuiTableFlags.RowBg)) {
                    if (!table)
                        return;

                    // Create the headers for the table
                    ImGui.TableSetupColumn("Symbol", ImGuiTableColumnFlags.WidthFixed, width/4);
                    ImGui.TableSetupColumn("Strength", ImGuiTableColumnFlags.WidthFixed, width/3);
                    ImGui.TableSetupColumn("Sound", ImGuiTableColumnFlags.WidthFixed, width/4);
                
                    ImGui.TableNextRow(); ImGui.TableNextColumn();
                    ImGui.Text("Symbol"); ImGui.TableNextColumn();
                    ImGui.Text("Strength"); ImGui.TableNextColumn();
                    ImGui.Text("Sound"); ImGui.TableNextColumn();
                
                    foreach (var phoneme in gag._muffleStrOnPhoneme){
                        ImGui.Text($"{phoneme.Key}"); ImGui.TableNextColumn();
                        ImGui.Text($"{phoneme.Value}"); ImGui.TableNextColumn();
                        ImGui.Text($"{gag._ipaSymbolSound[phoneme.Key]}"); ImGui.TableNextColumn();
                    }
                } // table ends here
                ImGui.NextColumn();
            }
            ImGui.Columns(1);
            ImGui.PopFont();

            }
            catch (Exception e)
            {
                ImGui.NewLine();
                ImGui.Text($"Error while fetching config in debug: {e}");
                ImGui.NewLine();
                GagSpeak.Log.Error($"Error while fetching config in debug: {e}");
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
    private static string GetLabel() => "GagSpeakDebug###GagSpeakDebug";    
}