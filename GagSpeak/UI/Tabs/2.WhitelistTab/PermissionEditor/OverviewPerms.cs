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
public partial class WhitelistPanel {
    public void DrawOverviewPerms(ref bool _interactions, string prefix, string suffix) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _tempWhitelistChar.GetDynamicTierClient();
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
            if(ImGui.IsItemHovered()) { var tt = tooltips["usedSafewordTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var usedSafewordIcon = _activePanelTab==WhitelistPanelTab.TheirSettings
                                 ? _tempWhitelistChar._safewordUsed : _characterHandler.playerChar._safewordUsed;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((usedSafewordIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ViewOnly");

            // for hardcore mode check
            ImGuiUtil.DrawFrameColumn($"In Hardcore Mode:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["hardcoreModeTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_tempWhitelistChar._inHardcoreMode ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ViewOnly");

            // draw out the extended lock times
            ImGuiUtil.DrawFrameColumn($"Extended Lock Times:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ExtendedLockTimesTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var extendedLockTimesIcon = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._grantExtendedLockTimes
                                                  : _characterHandler.playerChar._uniquePlayerPerms[_tempWhitelistIdx]._grantExtendedLockTimes;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((extendedLockTimesIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleExtendedLockTimesButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier2))) {
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    TogglePlayerExtendedLockTimes();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    _characterHandler.ToggleExtendedLockTimes();
                }
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["ToggleButtonTT"](); ImGui.SetTooltip($"{tt}"); }
            // draw out the Direct Chat Garbler Active option
            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["LiveChatGarblerTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var directChatGarblerActiveIcon = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._directChatGarblerActive
                                                        : _characterHandler.playerChar._directChatGarblerActive;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((directChatGarblerActiveIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleDirectChatGarblerActiveButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier2))) {
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    TogglePlayerLiveChatGarbler();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    _characterHandler.ToggleDirectChatGarbler();
                }
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["ToggleButtonTT"](); ImGui.SetTooltip($"{tt}"); }
            // draw out the Direct Chat Garbler Locked option
            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler Lock:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["LiveChatGarblerLockTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var directChatGarblerLockedIcon = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._directChatGarblerLocked
                                                        : _characterHandler.playerChar._directChatGarblerLocked;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((directChatGarblerLockedIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("3");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleDirectChatGarblerLockedButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                string.Empty, _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier3))) {
                    // This will only allow us to send this if the defined dynamic tier is 3 or higher
                    TogglePlayerLiveChatGarblerLock();
                    _interactOrPermButtonEvent.Invoke(5);
                }
                if(ImGui.IsItemHovered()) { var tt = tooltips["ToggleButtonTT"](); ImGui.SetTooltip($"{tt}"); }
            } else {
                ImGuiUtil.Center("ViewOnly");
            }
            // Enable Wardrobe option
            ImGuiUtil.DrawFrameColumn($"Wardrobe Enabled:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["WardrobeEnabledTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var wardrobeIcon = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._enableWardrobe
                                         : _characterHandler.playerChar._enableWardrobe;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((wardrobeIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ViewOnly");
            // Enable puppeteer option
            ImGuiUtil.DrawFrameColumn($"Puppeteer Active:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["PuppeteerEnabledTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var puppeteerIcon = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._allowPuppeteer
                                         : _characterHandler.playerChar._allowPuppeteer;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((puppeteerIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ViewOnly");

            // Enable Toybox option
            ImGuiUtil.DrawFrameColumn($"Toybox Enabled:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ToyboxEnabledTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            var toyboxIcon = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._enableToybox
                                       : _characterHandler.playerChar._enableToybox;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyboxIcon ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToyboxComponentEnabledButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                string.Empty, _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier4))) {
                    TogglePlayersEnableToyboxOption();
                    _interactOrPermButtonEvent.Invoke(5);
                }
                if(ImGui.IsItemHovered()) { var tt = tooltips["ToggleButtonTT"](); ImGui.SetTooltip($"{tt}"); }
            } else {
                ImGuiUtil.Center("ViewOnly");
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
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Extended Lock Times Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistGrantExtendedLockTimes(_tempWhitelistIdx, !_tempWhitelistChar._grantExtendedLockTimes);
        _chatManager.SendRealMessage(_messageEncoder.OrderToggleExtendedLockTimes(playerPayload, targetPlayer));
    }

    public void TogglePlayerLiveChatGarbler() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Live Chat Garbler Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistDirectChatGarblerActive(_tempWhitelistIdx, !_tempWhitelistChar._directChatGarblerActive);
        _chatManager.SendRealMessage(_messageEncoder.GagOrderToggleLiveChatGarbler(playerPayload, targetPlayer));
    }

    public void TogglePlayerLiveChatGarblerLock() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Live Chat Garbler Lock Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistDirectChatGarblerLocked(_tempWhitelistIdx, !_tempWhitelistChar._directChatGarblerLocked);
        _chatManager.SendRealMessage(_messageEncoder.GagOrderToggleLiveChatGarblerLock(playerPayload, targetPlayer));
    }

#endregion ButtonHelpers
}