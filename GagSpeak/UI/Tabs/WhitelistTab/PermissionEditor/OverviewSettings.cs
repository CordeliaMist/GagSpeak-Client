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
    public void DrawOverviewPerms(ref bool _viewMode) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        ImGui.PushFont(_fontService.UidFont);
        var name = _viewMode ? $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s" : "Your";
        ImGui.Text($"{name} General Settings");
        ImGui.PopFont();

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTier();

        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("RelationsManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Setting",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req. Tier").X);
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();


            // safeword option
            ImGuiUtil.DrawFrameColumn($"Has Used Safeword:");
            ImGui.TableNextColumn();
            var usedSafewordIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._safewordUsed
                                             : _characterHandler.playerChar._safewordUsed;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((usedSafewordIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");

            // draw out the extended lock times
            ImGuiUtil.DrawFrameColumn($"Extended Lock Times:");
            ImGui.TableNextColumn();
            var extendedLockTimesIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._grantExtendedLockTimes
                                                  : _characterHandler.playerChar._grantExtendedLockTimes[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((extendedLockTimesIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleExtendedLockTimesButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier2))) {
                if(_viewMode) {
                    TogglePlayerExtendedLockTimes();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    _characterHandler.ToggleExtendedLockTimes();
                }
            }
            
            
            // draw out the Direct Chat Garbler Active option
            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler:");
            ImGui.TableNextColumn();
            var directChatGarblerActiveIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerActive
                                                        : _characterHandler.playerChar._directChatGarblerActive;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((directChatGarblerActiveIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleDirectChatGarblerActiveButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier2))) {
                if(_viewMode) {
                    TogglePlayerLiveChatGarbler();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    _characterHandler.ToggleDirectChatGarbler();
                }
            }

            // draw out the Direct Chat Garbler Locked option
            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler Lock:");
            ImGui.TableNextColumn();
            var directChatGarblerLockedIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerLocked
                                                        : _characterHandler.playerChar._directChatGarblerLocked;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((directChatGarblerLockedIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("3");
            ImGui.TableNextColumn();
            if(_viewMode) {
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleDirectChatGarblerLockedButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier3))) {
                // This will only allow us to send this if the defined dynamic tier is 3 or higher
                TogglePlayerLiveChatGarblerLock();
                _interactOrPermButtonEvent.Invoke();
            }
            } else {
                ImGuiUtil.Center("ReadOnly");
            }

            // Enable Wardrobe option
            ImGuiUtil.DrawFrameColumn($"Wardrobe Enabled:");
            ImGui.TableNextColumn();
            var wardrobeIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableWardrobe
                                         : _characterHandler.playerChar._enableWardrobe;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((wardrobeIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");

            // Enable Wardrobe option
            ImGuiUtil.DrawFrameColumn($"Puppeteer Active:");
            
            ImGui.TableNextColumn();
            var puppeteerIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowPuppeteer
                                         : _characterHandler.playerChar._allowPuppeteer;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((puppeteerIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");

            // Enable Toybox option
            ImGuiUtil.DrawFrameColumn($"Toybox Enabled:");

            ImGui.TableNextColumn();
            var toyboxIcon = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableToybox
                                       : _characterHandler.playerChar._enableToybox;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyboxIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(_viewMode) {
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToyboxComponentEnabledButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier4))) {
                    TogglePlayersEnableToyboxOption();
                    _interactOrPermButtonEvent.Invoke();
                }
            } else {
                ImGuiUtil.Center("ReadOnly");
            }
        }
        // pop the spacing
        ImGui.PopStyleVar();
    }

#region ButtonHelpers
    public void TogglePlayerExtendedLockTimes() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Extended Lock Times Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._grantExtendedLockTimes = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._grantExtendedLockTimes;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerLiveChatGarbler() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Live Chat Garbler Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerActive = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerActive;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerLiveChatGarblerLock() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Live Chat Garbler Lock Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerLocked = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerLocked;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

#endregion ButtonHelpers
}