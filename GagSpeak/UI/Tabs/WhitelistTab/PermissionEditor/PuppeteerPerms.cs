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

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPlayerPermissions {

#region DrawPuppeteerPerms
    public void DrawPuppeteerPerms(ref bool _viewMode) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        ImGui.PushFont(_fontService.UidFont);
        var name = _viewMode ? $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s" : "Your";
        ImGui.Text($"{name} Puppeteer Settings");
        ImGui.PopFont();

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTier();
        // store the hovered var for tooltips
        var hovered  = ImGui.IsItemHovered();

        ImGui.Text($"Trigger phase: \"");
        ImGui.SameLine();
        var triggerPhrase = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerPhrase : _characterHandler.playerChar._triggerPhraseForPuppeteer[_characterHandler.activeListIdx];
        if(triggerPhrase == "") {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Empty / Invalid Trigger Phrase");
        } else {
            ImGui.TextColored(new Vector4(0, 1, 0, 1), triggerPhrase);
        }
        ImGui.SameLine();
        ImGui.Text("\"");

        ImGui.Text("Start/End Chars:");
        if(_viewMode) {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerStartChar);
            ImGui.SameLine(); ImGui.Text(" & "); ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerEndChar);
        } else {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), _characterHandler.playerChar._StartCharForPuppeteerTrigger[_characterHandler.activeListIdx]);
            ImGui.SameLine(); ImGui.Text(" & "); ImGui.SameLine();
            ImGui.TextColored(new Vector4(1, 0, 0, 1), _characterHandler.playerChar._EndCharForPuppeteerTrigger[_characterHandler.activeListIdx]);
        }

        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("PuppeteerManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            var text = _viewMode ? "Granted Permission Level" : $"Permissions for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}";
            ImGui.TableSetupColumn($"{text}",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req. Tier").X);
            ImGui.AlignTextToFramePadding();
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();


            // Lock Gag Storage on Gag Lock option
            ImGuiUtil.DrawFrameColumn($"Allows Sit Commands:");

            ImGui.TableNextColumn();
            var allowSits = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsSitRequests
                                      : _characterHandler.playerChar._allowSitRequests[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((allowSits ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleSitCommandsPermissionButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier1))) {
                // how to treat what happens when we press the button
                if(_viewMode) {
                    TogglePlayersSitRequestOption();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    // its us pressing it, so just toggle the state
                    _characterHandler.ToggleAllowSitRequests(_characterHandler.activeListIdx);
                }
            }



            // Enable Restraint Sets option
            ImGuiUtil.DrawFrameColumn($"Allows Motion Commands:");
            ImGui.TableNextColumn();
            var allowMotions = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsMotionRequests
                                        : _characterHandler.playerChar._allowMotionRequests[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((allowMotions ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleMotionCommandsPermissionButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier2))) {
                // how to treat what happens when we press the button
                if(_viewMode) {
                    TogglePlayerMotionRequestsOption();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    // its us pressing it, so just toggle the state
                    _characterHandler.ToggleAllowMotionRequests(_characterHandler.activeListIdx);
                }
            }


            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Allow All Commands:");
            ImGui.TableNextColumn();
            var allowAllCommands = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsAllCommands
                                            : _characterHandler.playerChar._allowAllCommands[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((allowAllCommands ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleAllCommandsPermissionButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier4))) {
                // how to treat what happens when we press the button
                if(_viewMode) {
                    TogglePlayerAllCommandsOption();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    // its us pressing it, so just toggle the state
                    _characterHandler.ToggleAllowAllCommands(_characterHandler.activeListIdx);
                }
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
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Allow Sit Requests Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsSitRequests = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsSitRequests;
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleOnlySitRequestOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerMotionRequestsOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Allow Motion Requests Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsMotionRequests = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsMotionRequests;
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleOnlyMotionRequestOption(playerPayload, targetPlayer));
    }
    public void TogglePlayerAllCommandsOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Allow All Commands Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsAllCommands = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsAllCommands;
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleAllCommandsOption(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}