using System.Numerics;
using ImGuiNET;
using GagSpeak.Utility;
using OtterGui;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface;
using GagSpeak.CharacterData;
using Dalamud.Interface.Utility;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {

#region DrawPuppeteerPerms
    public void DrawPuppeteerPerms(ref bool _interactions, string prefix, string suffix) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTierClient();
        // temp name storage
        string tempPlayerName = AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess);

        // store the hovered var for tooltips
        ImGui.Text($"{prefix}{suffix} Trigger Phrase: \"");
        if(ImGui.IsItemHovered()) { var tt = tooltips["TriggerPhraseTT"](); ImGui.SetTooltip($"{tt}"); }
        ImGui.SameLine();
        var triggerPhrase = _activePanelTab==WhitelistPanelTab.TheirSettings ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerPhrase 
                                    : _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._triggerPhraseForPuppeteer;
        if(triggerPhrase == "") {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Invalid Trigger Phrase");
            if(ImGui.IsItemHovered()) { var tt = tooltips["TriggerPhraseTT"](); ImGui.SetTooltip($"{tt}"); }
        } else {
            ImGui.TextColored(new Vector4(0, 1, 0, 1), triggerPhrase);
            if(ImGui.IsItemHovered()) { var tt = tooltips["TriggerPhraseTT"](); ImGui.SetTooltip($"{tt}"); }
        }
        ImGui.SameLine(); ImGui.Text("\"");

        ImGui.Text("Start Bracket Char: ");
        if(ImGui.IsItemHovered()) { var tt = tooltips["StartCharTT"](); ImGui.SetTooltip($"{tt}"); }
        if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0, 1, 0, 1), _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerStartChar);
        } else {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0, 1, 0, 1), _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._StartCharForPuppeteerTrigger);
        }
        ImGui.Text("End Bracket Char: ");
        if(ImGui.IsItemHovered()) { var tt = tooltips["EndCharTT"](); ImGui.SetTooltip($"{tt}"); }
        if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0, 1, 0, 1), _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerEndChar);
        } else {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0, 1, 0, 1), _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._EndCharForPuppeteerTrigger);
        }
        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("PuppeteerManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            var text2 = _activePanelTab==WhitelistPanelTab.TheirSettings ? "Granted Permission Level" : $"Permissions for {tempPlayerName.Split(' ')[0]}";
            ImGui.TableSetupColumn($"{text2}",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req. Tier").X);
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();

            // Lock Gag Storage on Gag Lock option
            ImGuiUtil.DrawFrameColumn($"Allows Sit Commands:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["AllowSitPermTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var allowSits = _activePanelTab==WhitelistPanelTab.TheirSettings ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsSitRequests
                                      : _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowSitRequests;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((allowSits ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleSitCommandsPermissionButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            tooltips["ToggleButtonTT"](), _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier1))) {
                // how to treat what happens when we press the button
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    TogglePlayersSitRequestOption();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    // its us pressing it, so just toggle the state
                    _characterHandler.ToggleAllowSitRequests(_characterHandler.activeListIdx);
                }
            }
            // Enable Restraint Sets option
            ImGuiUtil.DrawFrameColumn($"Allows Motion Commands:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["AllowMotionPermTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var allowMotions = _activePanelTab==WhitelistPanelTab.TheirSettings ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsMotionRequests
                                        : _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowMotionRequests;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((allowMotions ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleMotionCommandsPermissionButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            tooltips["ToggleButtonTT"](), _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier2))) {
                // how to treat what happens when we press the button
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    TogglePlayerMotionRequestsOption();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    // its us pressing it, so just toggle the state
                    _characterHandler.ToggleAllowMotionRequests(_characterHandler.activeListIdx);
                }
            }
            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Allow All Commands:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["AllowAllCommandsPermTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var allowAllCommands = _activePanelTab==WhitelistPanelTab.TheirSettings ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsAllCommands
                                            : _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowAllCommands;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((allowAllCommands ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleAllCommandsPermissionButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            tooltips["ToggleButtonTT"](), _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier4))) {
                // how to treat what happens when we press the button
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    TogglePlayerAllCommandsOption();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    // its us pressing it, so just toggle the state
                    _characterHandler.ToggleAllowAllCommands(_characterHandler.activeListIdx);
                }
            }
        }
        // draw out the table for our permissions
        using (var ImportedAliasTable = ImRaii.Table("ImportedAliasTable", 2, ImGuiTableFlags.RowBg)) {
            if (!ImportedAliasTable) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Stored Aliases", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Stored Aliasesmmmmm").X);
            ImGui.TableSetupColumn("Output",        ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();

            // draw out the alias list
            foreach (var alias in _characterHandler.whitelistChars[_characterHandler.activeListIdx]._storedAliases) {
                ImGui.TableNextColumn();
                ImGui.Text(alias.Key);
                if(ImGui.IsItemHovered()) { var tt = tooltips["AliasInputTT"](); ImGui.SetTooltip($"{tt}"); }
                ImGui.TableNextColumn();
                ImGui.Text(alias.Value);
                if(ImGui.IsItemHovered()) { var tt = tooltips["AliasOutputTT"](); ImGui.SetTooltip($"{tt}"); }
            }
        }
        // pop the style
        ImGui.PopStyleVar();
    }
#endregion DrawPuppeteerPerms
#region ButtonHelpers
    public void TogglePlayersSitRequestOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }

        string targetPlayer = AltCharHelpers.FetchNameWorldFormatByWhitelistIdxForNAWIdxToProcess(_characterHandler.activeListIdx);
        string targetPlayerName = AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess);

        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{targetPlayerName}'s Allow Sit Requests Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowSitRequests(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsSitRequests);
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleOnlySitRequestOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerMotionRequestsOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }

        string targetPlayer = AltCharHelpers.FetchNameWorldFormatByWhitelistIdxForNAWIdxToProcess(_characterHandler.activeListIdx);
        string targetPlayerName = AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess);

        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{targetPlayerName}'s Allow Motion Requests Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowMotionRequests(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsMotionRequests);
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleOnlyMotionRequestOption(playerPayload, targetPlayer));
    }
    public void TogglePlayerAllCommandsOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }

        string targetPlayer = AltCharHelpers.FetchNameWorldFormatByWhitelistIdxForNAWIdxToProcess(_characterHandler.activeListIdx);
        string targetPlayerName = AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess);

        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{targetPlayerName}'s Allow All Commands Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowAllCommands(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsAllCommands);
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleAllCommandsOption(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}