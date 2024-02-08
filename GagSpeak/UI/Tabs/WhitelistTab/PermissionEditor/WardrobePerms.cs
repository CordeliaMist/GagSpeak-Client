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
    public string _restraintSetToEnable = "";
    public string _restraintSetLockDuration = "";
    public string _resrtaintSetToUnlock = "";

#region DrawWardrobePerms
    public void DrawWardrobePerms(ref bool _viewMode) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        ImGui.PushFont(_fontService.UidFont);
        var name = _viewMode ? $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s" : "Your";
        ImGui.Text($"{name} Wardrobe Settings");
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
            ImGui.AlignTextToFramePadding();
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();


            // Lock Gag Storage on Gag Lock option
            ImGuiUtil.DrawFrameColumn($"Lock Gag Storage on Gag Lock:");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockGagStorageOnGagLock
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleLockGagStorageOnGagLockButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier1))) {
                TogglePlayersGagStorageUIState();
                _interactOrPermButtonEvent.Invoke();
            }



            // Enable Restraint Sets option
            ImGuiUtil.DrawFrameColumn($"Enable Restraint Sets:");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont))
            {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableRestraintSets
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleEnableRestraintSetsButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier2))) {
                TogglePlayerEnableRestraintSetOption();
                _interactOrPermButtonEvent.Invoke();
            }



            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Restraint Set Locking:");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._restraintSetLocking
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleRestraintSetLockingButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier1))) {
                TogglePlayerRestraintSetLockingOption();
                _interactOrPermButtonEvent.Invoke();
            }
        }
            // shift it up to align with the other table
            var xPos = ImGui.GetCursorPosX();
            var yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPos(new Vector2(xPos, yPos - ImGui.GetStyle().ItemSpacing.Y));
        // table for force enabling / setting / locking
        using (var tableWardrobe = ImRaii.Table("RelationsManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableWardrobe) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("WardrobeOption",ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("RestraintInput",        ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("restraintNameWhoawhoa").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req. Tier").X);
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);

            // None of the editable states should be enabled if the wardrobe is not enabled
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableWardrobe)
            { 
                ImGui.BeginDisabled();
            }

            // Force Enable a Restraint Set option, if the permission is enabled
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableRestraintSets) { ImGui.BeginDisabled(); }
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"Enable Set:");
            ImGui.TableNextColumn();
            string restraintSetResult = _restraintSetToEnable; // get the input timer storage
            ImGui.AlignTextToFramePadding();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("##RestraintSetName", "Restraint Set Name..",
            ref restraintSetResult, 36, ImGuiInputTextFlags.None)) {
                _restraintSetToEnable = restraintSetResult;
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("2");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Enable##EnableRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier2))) {
                EnableRestraintSetByName(_restraintSetToEnable);
                _interactOrPermButtonEvent.Invoke();
            }
            // end the disabled state
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableRestraintSets) { ImGui.EndDisabled(); }


            // Lock Restraint Set option, if the permission is enabled
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._restraintSetLocking) { ImGui.BeginDisabled(); }
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"Lock Set:");
            ImGui.TableNextColumn();
            string restraintSetLockResult = _restraintSetLockDuration; // get the input timer storage
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("##RestraintSetLockDuration", "Ex: 0h2m7s",
            ref restraintSetLockResult, 12, ImGuiInputTextFlags.None)) {
                _restraintSetLockDuration = restraintSetLockResult;
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Lock##LockRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, dynamicTier >= DynamicTier.Tier1)) {
                LockRestraintSetToPlayer(_restraintSetToEnable, _restraintSetLockDuration);
                _interactOrPermButtonEvent.Invoke();
            }
            // end the disabled state
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._restraintSetLocking) { ImGui.EndDisabled(); }


            // end the disabled state
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableWardrobe)
            {
                ImGui.EndDisabled();
            }

            
            // dont need any state to try and unlock
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"Unlock Set:");
            ImGui.TableNextColumn();
            string restraintSetUnlockResult = _resrtaintSetToUnlock; // get the input timer storage
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("##RestraintSetToUnlock", "RestraintSet Name..",
            ref restraintSetUnlockResult, 36, ImGuiInputTextFlags.None)) {
                _resrtaintSetToUnlock = restraintSetUnlockResult;
            }
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            if(ImGui.Button("Unlock##UnlockRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                UnlockRestraintSetToPlayer(_resrtaintSetToUnlock);
                _interactOrPermButtonEvent.Invoke();
            }
        }
        
        // pop the spacing
        ImGui.PopStyleVar();
    }
#endregion DrawWardrobePerms
#region ButtonHelpers
    public void TogglePlayersGagStorageUIState() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s GagStorageUILock state!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockGagStorageOnGagLock = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockGagStorageOnGagLock;
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeGagStorageUiLockToggle(playerPayload, targetPlayer));
    }

    public void TogglePlayerEnableRestraintSetOption() {
        // get the player payload
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Enable RestraintSets Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableRestraintSets = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableRestraintSets;
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeEnableRestraintSetsOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerRestraintSetLockingOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s allow RestraintSet Locking Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._restraintSetLocking = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._restraintSetLocking;
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeEnableRestraintSetLockingOption(playerPayload, targetPlayer));
    }

    public void EnableRestraintSetByName(string restraintSetNameToApply) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Now attempting to enable "+
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s {restraintSetNameToApply} restraint set.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeEnableRestraintSet(playerPayload, targetPlayer, restraintSetNameToApply));
    }

    public void LockRestraintSetToPlayer(string restraintSetNameToApply, string timeToLock) {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload); // THIS IS THE SENDER
        if (_characterHandler.activeListIdx < 0 || _characterHandler.activeListIdx >= _characterHandler.whitelistChars.Count) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Now attempting to lock "+
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s {restraintSetNameToApply} restraint set for "+
            $"{timeToLock}. Keep in mind if they have this option disabled, it will not apply.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeRestraintSetLock(playerPayload, targetPlayer, restraintSetNameToApply, timeToLock));
    }

    public void UnlockRestraintSetToPlayer(string restraintSetNameToApply) {  
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Attempting to unlock "+
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s [{restraintSetNameToApply}] restraint set. "+
            "Keep in mind if you are not the one who assigned it and they did not do it to themselves, it will not unlock.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeRestraintSetUnlock(playerPayload, targetPlayer, restraintSetNameToApply));
    }
#endregion ButtonHelpers
}