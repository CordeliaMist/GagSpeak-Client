using System.Numerics;
using ImGuiNET;
using GagSpeak.Utility;
using OtterGui;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using Dalamud.Interface.Utility.Raii;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPermissionEditor {
    public string _restraintSetToEnable = "";
    public string _restraintSetLockDuration = "";
    public string _resrtaintSetToUnlock = "";

#region DrawWardrobePerms
    public void DrawWardrobePerms(int currentWhitelistItem) {
        // draw out the table for our permissions
        using (var tableWardrobe = ImRaii.Table("RelationsManagerTable", 3, ImGuiTableFlags.RowBg)) {
            if (!tableWardrobe) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Setting/Option",ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Status",        ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("restraintNameWhoa").X);
            ImGui.TableSetupColumn("Toggle##",      ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglem").X);
            ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            // Draw out the line that displays the players defined relationship towards you
            ImGuiUtil.DrawFrameColumn($"GagStorage on GagLock:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._grantExtendedLockTimes ? "Locked" : "Not Locked");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleGagStorage", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayersGagStorageUIState(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Enable Restraints:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._enableRestraintSets ? "True" : "False");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleEnableRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerEnableRestraintSetOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Allow RestraintSet Locking:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._restraintSetLocking ? "True" : "False");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleRestraintSetLocking", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerRestraintSetLockingOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Enable Restraint Set:");
            ImGui.TableNextColumn();
            string restraintSetResult = _restraintSetToEnable; // get the input timer storage
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("##RestraintSetName", "RestraintSet Name..", ref restraintSetResult, 36, ImGuiInputTextFlags.None)) {
                _restraintSetToEnable = restraintSetResult;
            }
            ImGui.TableNextColumn();
            if(ImGui.Button("Enable##EnableRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                EnableRestraintSetByName(currentWhitelistItem, _restraintSetToEnable);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Lock Restraint Set:");
            ImGui.TableNextColumn();
            string restraintSetLockResult = _restraintSetLockDuration; // get the input timer storage
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("##RestraintSetLockDuration", "Ex: 0h2m7s", ref restraintSetLockResult, 12, ImGuiInputTextFlags.None)) {
                _restraintSetLockDuration = restraintSetLockResult;
            }
            ImGui.TableNextColumn();
            if(ImGui.Button("Lock##LockRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                LockRestraintSetToPlayer(currentWhitelistItem, _restraintSetToEnable, _restraintSetLockDuration);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Unlock Restraint Set:");
            ImGui.TableNextColumn();
            string restraintSetUnlockResult = _resrtaintSetToUnlock; // get the input timer storage
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint("##RestraintSetToUnlock", "RestraintSet Name..", ref restraintSetUnlockResult, 36, ImGuiInputTextFlags.None)) {
                _resrtaintSetToUnlock = restraintSetUnlockResult;
            }
            ImGui.TableNextColumn();
            if(ImGui.Button("Unlock##UnlockRestraintSet", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                UnlockRestraintSetToPlayer(currentWhitelistItem, _resrtaintSetToUnlock);
                _interactOrPermButtonEvent.Invoke();
            }
        }
    }
#endregion DrawWardrobePerms
#region ButtonHelpers
    public void TogglePlayersGagStorageUIState(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+
            $"{_config.whitelist[currentWhitelistItem]._name}'s GagStorageUILock state!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._lockGagStorageUiOnGagLock = !_config.whitelist[currentWhitelistItem]._lockGagStorageUiOnGagLock;
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeGagStorageUiLockToggle(playerPayload, targetPlayer));
    }

    public void TogglePlayerEnableRestraintSetOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+
            $"{_config.whitelist[currentWhitelistItem]._name}'s Enable RestraintSets Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._enableRestraintSets = !_config.whitelist[currentWhitelistItem]._enableRestraintSets;
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeEnableRestraintSetsOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerRestraintSetLockingOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+
            $"{_config.whitelist[currentWhitelistItem]._name}'s allow RestraintSet Locking Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._restraintSetLocking = !_config.whitelist[currentWhitelistItem]._restraintSetLocking;
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeEnableRestraintSetLockingOption(playerPayload, targetPlayer));
    }

    public void EnableRestraintSetByName(int currentWhitelistItem, string restraintSetNameToApply) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Now attempting to enable "+
            $"{_config.whitelist[currentWhitelistItem]._name}'s {restraintSetNameToApply} restraint set.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeEnableRestraintSet(playerPayload, targetPlayer, restraintSetNameToApply));
    }

    public void LockRestraintSetToPlayer(int currentWhitelistItem, string restraintSetNameToApply, string timeToLock) {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload); // THIS IS THE SENDER
        if (currentWhitelistItem < 0 || currentWhitelistItem >= _config.whitelist.Count) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Now attempting to lock "+
            $"{_config.whitelist[currentWhitelistItem]._name}'s {restraintSetNameToApply} restraint set for "+
            $"{timeToLock}. Keep in mind if they have this option disabled, it will not apply.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeRestraintSetLock(playerPayload, targetPlayer, restraintSetNameToApply, timeToLock));
    }

    public void UnlockRestraintSetToPlayer(int currentWhitelistItem, string restraintSetNameToApply) {  
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Attempting to unlock "+
            $"{_config.whitelist[currentWhitelistItem]._name}'s [{restraintSetNameToApply}] restraint set. "+
            "Keep in mind if you are not the one who assigned it and they did not do it to themselves, it will not unlock.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeWardrobeRestraintSetUnlock(playerPayload, targetPlayer, restraintSetNameToApply));
    }
#endregion ButtonHelpers
}