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
    public void DrawOverviewPerms() {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        ImGui.PushFont(_fontService.UidFont);
        ImGui.Text($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s General Settings");
        ImGui.PopFont();

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTier();
        // store the hovered var for tooltips
        var hovered  = ImGui.IsItemHovered();

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
            hovered  |= ImGui.IsItemHovered();
            if(hovered)
                ImGui.SetTooltip("Dictates if this player has used their safeword / it is active.");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._safewordUsed
                                     ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");

            
            
            // draw out the extended lock times
            ImGuiUtil.DrawFrameColumn($"Extended Lock Times:");
            hovered  |= ImGui.IsItemHovered();
            if(hovered)
                ImGui.SetTooltip("Determines if you are allowed to lock them for any duration over 12 hours.\n(subject to change in future updates)");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._grantExtendedLockTimes
                                     ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleExtendedLockTimesButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier2))) {
                // This will only allow us to send this if the defined dynamic tier is 2 or higher
                TogglePlayerExtendedLockTimes();
                _interactOrPermButtonEvent.Invoke();
            }
            
            
            // draw out the Direct Chat Garbler Active option
            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler:");
            hovered  |= ImGui.IsItemHovered();
            if(hovered)
                ImGui.SetTooltip("View if this player's live chat garbler is currently active or not.");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerActive
                                     ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleDirectChatGarblerActiveButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier2))) {
                // This will only allow us to send this if the defined dynamic tier is 4 or higher
                TogglePlayerLiveChatGarbler();
                _interactOrPermButtonEvent.Invoke();
            }

            // draw out the Direct Chat Garbler Locked option
            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler Lock:");
            hovered  |= ImGui.IsItemHovered();
            if(hovered)
                ImGui.SetTooltip("View if this player's live chat garbler is currently locked or not.");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._directChatGarblerLocked
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("3");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleDirectChatGarblerLockedButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier3))) {
                // This will only allow us to send this if the defined dynamic tier is 3 or higher
                TogglePlayerLiveChatGarblerLock();
                _interactOrPermButtonEvent.Invoke();
            }

            // Enable Wardrobe option
            ImGuiUtil.DrawFrameColumn($"Wardrobe Enabled:");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableWardrobe
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");

            // Enable Wardrobe option
            ImGuiUtil.DrawFrameColumn($"Puppeteer Active:");
            
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirTriggerPhrase != ""
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");

            // Enable Toybox option
            ImGuiUtil.DrawFrameColumn($"Toybox Enabled:");

            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableToybox
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToyboxComponentEnabledButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier4))) {
                TogglePlayersEnableToyboxOption();
                _interactOrPermButtonEvent.Invoke();
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
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
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
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
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
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
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