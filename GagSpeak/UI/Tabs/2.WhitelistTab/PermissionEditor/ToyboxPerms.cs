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
using System;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {
    public int _vibratorIntensity = 1;
    public string _vibePatternName = "";
    public int _activeStoredPatternListIdx = 0;

#region DrawWardrobePerms
    public void DrawToyboxPerms(ref bool _interactions, string prefix, string suffix) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        // toy state:
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
        ImGui.PushFont(_fontService.UidFont);
        try {
            ImGui.Text($"{prefix}{suffix} Toy State: ");
        } finally {
            ImGui.PopFont();
        }
        ImGui.SameLine();
        if(!_tempWhitelistChar._allowChangingToyState) { ImGui.BeginDisabled(); }
        try{
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 6*ImGuiHelpers.GlobalScale);
            var text2 = _tempWhitelistChar._isToyActive ? "On" : "Off";
            if(ImGuiUtil.DrawDisabledButton($"{text2}##ToggleToyActive", new Vector2(ImGuiHelpers.GlobalScale*50, 22*ImGuiHelpers.GlobalScale),
            "Toggle the current state of the toy", _activePanelTab==WhitelistPanelTab.TheirSettings && !_tempWhitelistChar._allowChangingToyState)) {
                TogglePlayersIsToyActiveOption();
                _interactOrPermButtonEvent.Invoke(5);
            }
        } finally {
            if(!_tempWhitelistChar._allowChangingToyState) { ImGui.EndDisabled(); }
        }
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3*ImGuiHelpers.GlobalScale);
        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _tempWhitelistChar.GetDynamicTierClient();
        
        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("ToyboxManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            ImGui.TableSetupColumn($"Setting",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req.Tier").X);
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemo").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Locked Toybox UI?");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ToyboxStateTT"](); ImGui.SetTooltip(tt); }
            ImGui.TableNextColumn();
            var toyboxUILock = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._lockToyboxUI : _characterHandler.playerChar._lockToyboxUI;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyboxUILock ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip(tt); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip(tt); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##UpdateWhitelistPlayersToyIntensityButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            tooltips["ToggleButtonTT"](), _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier3))) {
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    // the whitelisted players lock
                    TogglePlayerToyboxLockOption();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    // toggle your lock
                    _characterHandler.ToggleToyboxUILocking();
                }
            }
            if(_activePanelTab==WhitelistPanelTab.YourSettings && _characterHandler.playerChar._lockToyboxUI) { ImGui.BeginDisabled(); }
            try{
                // Lock Gag Storage on Gag Lock option
                ImGuiUtil.DrawFrameColumn($"Allow Change Toy State:");
                if(ImGui.IsItemHovered()) { var tt = tooltips["AllowChangeToyStateTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                var toyStatePerm = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._allowChangingToyState 
                                            : _characterHandler.playerChar._uniquePlayerPerms[_tempWhitelistIdx]._allowChangingToyState;
                using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                    ImGuiUtil.Center((toyStatePerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
                }
                if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                ImGuiUtil.Center("1");
                if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyActiveState", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                tooltips["ToggleButtonTT"](), _activePanelTab==WhitelistPanelTab.TheirSettings && !(dynamicTier >= DynamicTier.Tier1))) {
                    if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                        // toggle the whitelisted players permission to allow changing toy state
                        TogglePlayerToggleChangeToyState();
                        _interactOrPermButtonEvent.Invoke(5);
                    } else {
                        // toggles if this person can change your toy state
                        _characterHandler.ToggleChangeToyState(_tempWhitelistIdx);
                    }
                }
                // Can Control Intensity
                ImGuiUtil.DrawFrameColumn($"Can Control Intensity:");
                if(ImGui.IsItemHovered()) { var tt = tooltips["CanControlIntensityTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                var toyIntensityPerm = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._allowsIntensityControl 
                                                : _characterHandler.playerChar._uniquePlayerPerms[_tempWhitelistIdx]._allowIntensityControl;
                using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                    ImGuiUtil.Center((toyIntensityPerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
                }
                if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                ImGuiUtil.Center("0");
                if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    ImGuiUtil.Center("ReadOnly");
                } else {
                    if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyIntensityControl", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    tooltips["ToggleButtonTT"](), false)) {
                        // toggles if this person can change your toy state
                        _characterHandler.ToggleAllowIntensityControl(_tempWhitelistIdx);
                    }
                }
                // Enable Restraint Sets option
                ImGuiUtil.DrawFrameColumn($"Can Execute Patterns:");
                if(ImGui.IsItemHovered()) { var tt = tooltips["CanExecutePatternsTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                var patternExecuttionPerm = _activePanelTab==WhitelistPanelTab.TheirSettings ? _tempWhitelistChar._allowsUsingPatterns 
                                                : _characterHandler.playerChar._uniquePlayerPerms[_tempWhitelistIdx]._allowUsingPatterns;
                using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                    ImGuiUtil.Center((patternExecuttionPerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
                }
                if(ImGui.IsItemHovered()) { var tt = tooltips["CurrentStateTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                ImGuiUtil.Center("0");
                if(ImGui.IsItemHovered()) { var tt = tooltips["ReqTierTT"](); ImGui.SetTooltip(tt); }
                ImGui.TableNextColumn();
                if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                    ImGuiUtil.Center("ReadOnly");
                } else {
                    if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleAllowingPatternExecution", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    tooltips["ToggleButtonTT"](), false)) {
                        // toggles if this person can change your toy state
                        _characterHandler.ToggleAllowPatternExecution(_tempWhitelistIdx);
                    }
                }
            } finally {
                if(_activePanelTab==WhitelistPanelTab.YourSettings && _characterHandler.playerChar._lockToyboxUI) { ImGui.EndDisabled(); }
            }
        }
        // disable just incase our UI is locked
        if(_activePanelTab==WhitelistPanelTab.YourSettings && _characterHandler.playerChar._lockToyboxUI) { ImGui.BeginDisabled(); }
        // draw out the table for our permissions
        try{
            if(_activePanelTab==WhitelistPanelTab.TheirSettings) {
                using (var toyboxDisplayList = ImRaii.Table("ToyboxManagerDisplayTable", 3, ImGuiTableFlags.RowBg)) {
                    if (!toyboxDisplayList) return;
                    // Create the headers for the table
                    ImGui.TableSetupColumn("Setting",  ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Stored Patternmm").X);
                    ImGui.TableSetupColumn("Adjuster",     ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemo").X);
                    ImGui.TableNextRow();

                    ImGuiUtil.DrawFrameColumn("Intensity Level: ");
                    if(ImGui.IsItemHovered()) { var tt = tooltips["IntensityMeterTT"](); ImGui.SetTooltip(tt); }
                    ImGui.TableNextColumn();
                    int intensityResult = _vibratorIntensity;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    // default to a range of 10, but otherwise, display the toy's active step size
                    var maxSliderVal = _tempWhitelistChar._activeToystepSize==0 ? 20 : _tempWhitelistChar._activeToystepSize;
                    if(ImGui.SliderInt("##ToyIntensity", ref intensityResult, 0, maxSliderVal)) {
                        _vibratorIntensity = intensityResult;
                    }
                    if(ImGui.IsItemHovered()) { var tt = tooltips["IntensityMeterTT"](); ImGui.SetTooltip(tt); }
                    ImGui.TableNextColumn();
                    if(ImGuiUtil.DrawDisabledButton("Update##UpdateToyIntensity", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    tooltips["ToggleButtonTT"](), !(dynamicTier >= DynamicTier.Tier2))) {
                        UpdateWhitelistPlayersToyIntensity(_vibratorIntensity);
                        _interactOrPermButtonEvent.Invoke(2);
                    }
                    // pattern executtion section
                    ImGuiUtil.DrawFrameColumn("Pattern Name: ");
                    if(ImGui.IsItemHovered()) { var tt = tooltips["PatternTT"](); ImGui.SetTooltip(tt); }
                    ImGui.TableNextColumn();
                    string patternResult = _vibePatternName;
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.AlignTextToFramePadding();
                    if (ImGui.InputTextWithHint("##ToyPatternName", "Pattern Name", ref patternResult, 50)) {
                        _vibePatternName = patternResult;
                    }
                    if(ImGui.IsItemHovered()) { var tt = tooltips["PatternTT"](); ImGui.SetTooltip(tt); }
                    // then go over and draw the execute button
                    ImGui.TableNextColumn();
                    if(ImGuiUtil.DrawDisabledButton("Execute##ExecuteToyPattern", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                    tooltips["ToggleButtonTT"](), !(_tempWhitelistChar._allowsUsingPatterns == true))) {
                        ExecutePlayerToyPattern(_vibePatternName);
                        _interactOrPermButtonEvent.Invoke(5);
                    }
                    // stored pattern list
                    ImGuiUtil.DrawFrameColumn("Stored Patterns: ");
                    if(ImGui.IsItemHovered()) { var tt = tooltips["PatternListTT"](); ImGui.SetTooltip(tt); }
                    ImGui.TableNextColumn();
                    // Create a combo box with the stored restraint data (had to convert to array because am dumb)
                    string[] patternData = _tempWhitelistChar._storedPatternNames.ToArray();
                    int currentPatternIndex = _activeStoredPatternListIdx==0 ? 0 : _activeStoredPatternListIdx; // This should be the current selected index
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.Combo("##storedPatternData", ref currentPatternIndex, patternData, patternData.Length)) {
                        // If an item is selected from the dropdown, update the restraint set name field
                        _vibePatternName = patternData[currentPatternIndex];
                        // update the index to display
                        _activeStoredPatternListIdx = currentPatternIndex;
                    }
                    if(ImGui.IsItemHovered()) { var tt = tooltips["PatternListTT"](); ImGui.SetTooltip(tt); }
                    // end the disabled state
                    if(!_tempWhitelistChar._enableWardrobe || _activePanelTab==WhitelistPanelTab.TheirSettings==false) {
                        ImGui.EndDisabled();
                    }
                }
            }
        } catch (Exception e) {
            GSLogger.LogType.Debug($"Error drawing Toybox Permissions {e.Message}");
        } finally {
            if(_activePanelTab==WhitelistPanelTab.YourSettings && _characterHandler.playerChar._lockToyboxUI) { ImGui.EndDisabled(); }
        }
        // pop the style
        ImGui.PopStyleVar();
    }
#endregion DrawWardrobePerms
#region ButtonHelpers
    public void TogglePlayersEnableToyboxOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Enable Toybox Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistEnableToybox(_tempWhitelistIdx, !_tempWhitelistChar._enableToybox);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerToggleChangeToyState() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Toy State!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowChangingToyState(_tempWhitelistIdx, !_tempWhitelistChar._allowChangingToyState);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleActiveToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayersIsToyActiveOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Toy Active Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistToyIsActive(_tempWhitelistIdx, !_tempWhitelistChar._isToyActive);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleToyOnOff(playerPayload, targetPlayer));
    }

    public void UpdateWhitelistPlayersToyIntensity(int newIntensityLevel) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating  "+ 
            $"{_tempWhitelistChar._name}'s Toy Intensity to {newIntensityLevel}!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistIntensityLevel(_tempWhitelistIdx, (byte)newIntensityLevel);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxUpdateActiveToyIntensity(playerPayload, targetPlayer, newIntensityLevel));
    }

    public void ExecutePlayerToyPattern(string patternName) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Executing  "+ 
            $"{_tempWhitelistChar._name}'s Toy Pattern [{patternName}]!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxExecuteStoredToyPattern(playerPayload, targetPlayer, patternName));
    }

    public void TogglePlayerToyboxLockOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_tempWhitelistChar._name}'s Toybox Lock Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowToyboxLocking(_tempWhitelistIdx, !_tempWhitelistChar._lockToyboxUI);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleLockToyboxUI(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}