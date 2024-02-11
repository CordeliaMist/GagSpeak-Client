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
public partial class WhitelistPlayerPermissions {
    public int _vibratorIntensity = 1;
    public string _vibePatternName = "";

#region DrawWardrobePerms
    public void DrawToyboxPerms(ref bool _viewMode) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        if(_characterHandler.playerChar._lockToyboxUI) { ImGui.BeginDisabled(); }

        ImGui.PushFont(_fontService.UidFont);
        var name = _viewMode ? $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s" : "Your";
        ImGui.Text($"{name} Toybox Settings");
        ImGui.PopFont();

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTierClient();
        // store the hovered var for tooltips
        var hovered  = ImGui.IsItemHovered();
        
        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("ToyboxManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            var text = _viewMode ? "Setting" : $"Permission Setting for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}";
            ImGui.TableSetupColumn($"{text}",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req. Tier").X);
            ImGui.AlignTextToFramePadding();
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Locked Toybox UI?");
            ImGui.TableNextColumn();
            var toyboxUILock = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockToyboxUI : _characterHandler.playerChar._lockToyboxUI;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyboxUILock ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##UpdatePlayerToyIntensityButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier3))) {
                if(_viewMode) {
                    // the whitelisted players lock
                    TogglePlayerToyboxLockOption();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    // toggle your lock
                    _characterHandler.ToggleToyboxUILocking();
                }
            }
            // Lock Gag Storage on Gag Lock option
            ImGuiUtil.DrawFrameColumn($"Allow Change Toy State:");

            ImGui.TableNextColumn();
            var toyStatePerm = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState 
                                         : _characterHandler.playerChar._allowChangingToyState[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyStatePerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyActiveState", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier1))) {
                if(_viewMode) {
                    // toggle the whitelisted players permission to allow changing toy state
                    TogglePlayerToggleChangeToyState();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    // toggles if this person can change your toy state
                    _characterHandler.ToggleChangeToyState(_characterHandler.activeListIdx);
                }
            }
            // turn the toy on / off
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState) { ImGui.BeginDisabled(); }
            ImGuiUtil.DrawFrameColumn($"Toy Active:");
            ImGui.TableNextColumn();
            var toyActivePerm = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._isToyActive 
                                            : _characterHandler.playerChar._isToyActive;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyActivePerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyActive", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState)) {
                if(_viewMode) {
                    // toggle the whitelisted players permission to allow changing toy state
                    TogglePlayersIsToyActiveOption();
                    _interactOrPermButtonEvent.Invoke();
                } else {
                    // toggles if this person can change your toy state
                    _characterHandler.ToggleToyState();
                }
            }
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState) { ImGui.EndDisabled(); }


            // Enable Restraint Sets option
            ImGuiUtil.DrawFrameColumn($"Can Control Intensity:");
            ImGui.TableNextColumn();
            var toyIntensityPerm = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsIntensityControl 
                                            : _characterHandler.playerChar._allowIntensityControl[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyIntensityPerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            if(_viewMode) {
                ImGuiUtil.Center("ReadOnly");
            } else {
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyIntensityControl", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                string.Empty, false)) {
                    // toggles if this person can change your toy state
                    _characterHandler.ToggleAllowIntensityControl(_characterHandler.activeListIdx);
                }
            }
        }
        // seperate the table and sliders
        ImGui.Separator();
        if(!_viewMode) { ImGui.BeginDisabled(); }
        // now make a table of 2 columns
        using (var toyboxPlaytimeTable = ImRaii.Table("ToyboxManagerPlaytimeTable", 2)) {
            if (!toyboxPlaytimeTable) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Input",     ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            // draw the intensity level
            ImGui.Text("Intensity Level");
            // then draw the slider
            int intensityResult = _vibratorIntensity;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            // default to a range of 10, but otherwise, display the toy's active step size
            var maxSliderVal = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._activeToystepSize==0 ? 10 : _characterHandler.whitelistChars[_characterHandler.activeListIdx]._activeToystepSize;
            if(ImGui.SliderInt("##ToyIntensity", ref intensityResult, 0, maxSliderVal)) {
                _vibratorIntensity = intensityResult;
            }

            ImGui.TableNextColumn();
            var yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(yPos + 20*ImGuiHelpers.GlobalScale);
            if(ImGuiUtil.DrawDisabledButton("Update##UpdateToyIntensity", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier2))) {
                UpdatePlayerToyIntensity(_vibratorIntensity);
                _interactOrPermButtonEvent.Invoke();
            }
            ImGui.TableNextColumn();
            // then draw the input box
            string patternResult = _vibePatternName;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.AlignTextToFramePadding();
            if (ImGui.InputTextWithHint("##ToyPatternName", "Pattern Name", ref patternResult, 50)) {
                _vibePatternName = patternResult;
            }
            // then go over and draw the execute button
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Execute##ExecuteToyPattern", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsUsingPatterns == true))) {
                ExecutePlayerToyPattern(_vibePatternName);
                _interactOrPermButtonEvent.Invoke();
            }
        }
        if(!_viewMode) { ImGui.EndDisabled(); }
        if(_characterHandler.playerChar._lockToyboxUI) { ImGui.EndDisabled(); }
        // pop the style
        ImGui.PopStyleVar();
    }
#endregion DrawWardrobePerms
#region ButtonHelpers
    public void TogglePlayersEnableToyboxOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Enable Toybox Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistEnableToybox(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableToybox);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerToggleChangeToyState() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy State!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowChangingToyState(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleActiveToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayersIsToyActiveOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Active Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistToyIsActive(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._isToyActive);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleToyOnOff(playerPayload, targetPlayer));
    }

    public void UpdatePlayerToyIntensity(int newIntensityLevel) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Intensity to {newIntensityLevel}!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistIntensityLevel(_characterHandler.activeListIdx, (byte)newIntensityLevel);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxUpdateActiveToyIntensity(playerPayload, targetPlayer, newIntensityLevel));
    }

    public void ExecutePlayerToyPattern(string patternName) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Executing  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Pattern [{patternName}]!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxExecuteStoredToyPattern(playerPayload, targetPlayer, patternName));
    }

    public void TogglePlayerToyboxLockOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toybox Lock Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowToyboxLocking(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockToyboxUI);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleLockToyboxUI(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}