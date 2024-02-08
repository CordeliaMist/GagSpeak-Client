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

        ImGui.PushFont(_fontService.UidFont);
        ImGui.Text($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s Toybox Settings");
        ImGui.PopFont();

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTier();
        // store the hovered var for tooltips
        var hovered  = ImGui.IsItemHovered();
        
        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("ToyboxManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Setting",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req. Tier").X);
            ImGui.AlignTextToFramePadding();
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemm").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Locked Toybox UI?");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsAllCommands
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##UpdatePlayerToyIntensityButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier3))) {
                TogglePlayerToyboxLockOption();
                _interactOrPermButtonEvent.Invoke();
            }
            // Lock Gag Storage on Gag Lock option
            ImGuiUtil.DrawFrameColumn($"Connected Toy Active:");

            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsChangingToyState
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyActiveState", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier1))) {
                TogglePlayerToggleToyState();
                _interactOrPermButtonEvent.Invoke();
            }
            // Enable Restraint Sets option
            ImGuiUtil.DrawFrameColumn($"Can Control Intensity:");
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsIntensityControl
                                    ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            ImGuiUtil.Center("ReadOnly");
        }
        // seperate the table and sliders
        ImGui.Separator();
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
            if(ImGui.SliderInt("##ToyIntensity", ref intensityResult, 0, 10)) {
                _vibratorIntensity = intensityResult;
            }

            ImGui.TableNextColumn();
            var yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(yPos + 20*ImGuiHelpers.GlobalScale);
            if(ImGuiUtil.DrawDisabledButton("Update##UpdateToyIntensity", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(dynamicTier >= DynamicTier.Tier2))) {
                UpdatePlayerToyIntensity(_vibratorIntensity);
                _interactOrPermButtonEvent.Invoke();
            }


            // execute patterns
            ImGuiUtil.DrawFrameColumn($"Execute Pattern:");
            // then draw the input box
            string patternResult = _vibePatternName;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.AlignTextToFramePadding();
            if (ImGui.InputTextWithHint("##ToyPatternName", "Pattern Name", ref patternResult, 50)) {
                _vibePatternName = patternResult;
            }
            // then go over and draw the execute button
            ImGui.TableNextColumn();
            yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(yPos + 26*ImGuiHelpers.GlobalScale);
            if(ImGuiUtil.DrawDisabledButton("Execute##ExecuteToyPattern", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsUsingPatterns == true))) {
                ExecutePlayerToyPattern(_vibePatternName);
                _interactOrPermButtonEvent.Invoke();
            }
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
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Enable Toybox Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableToybox = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableToybox;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerToggleToyState() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy State!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsChangingToyState = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsChangingToyState;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleActiveToyboxOption(playerPayload, targetPlayer));
    }

    public void UpdatePlayerToyIntensity(int newIntensityLevel) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Intensity to {newIntensityLevel}!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxUpdateActiveToyIntensity(playerPayload, targetPlayer, newIntensityLevel));
    }

    public void ExecutePlayerToyPattern(string patternName) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
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
        if (_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toybox Lock Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowToyboxLocking = !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowToyboxLocking;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleLockToyboxUI(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}