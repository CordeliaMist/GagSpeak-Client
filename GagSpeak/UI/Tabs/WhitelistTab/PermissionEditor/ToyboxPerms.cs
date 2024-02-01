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
    public int _vibratorIntensity = 1;
    public string _vibePatternName = "";
#region DrawWardrobePerms
    public void DrawToyboxPerms(int currentWhitelistItem) {
        // draw out the table for our permissions
        using (var tableToybox = ImRaii.Table("RelationsManagerTable", 3, ImGuiTableFlags.RowBg)) {
            if (!tableToybox) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Setting/Option",ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Status",        ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Inactives").X);
            ImGui.TableSetupColumn("Toggle##",      ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Toggles").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();


            ImGuiUtil.DrawFrameColumn($"Toybox Enabled:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._enableToybox ? "True" : "False");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleToybox", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayersEnableToyboxOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Connected Toy State:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._toyActiveState ? "Active" : "Inactive");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleToyActiveState", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerToggleToyState(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGui.TableNextColumn();
            int intensityResult = _vibratorIntensity;
            if(ImGui.SliderInt("##ToyIntensity", ref intensityResult, 0, 10)) {
                _vibratorIntensity = intensityResult;
            }
            ImGui.TableNextColumn();
            ImGui.Text("Toy Intensity");
            if(ImGui.Button("Update##UpdateToyIntensity", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                UpdatePlayerToyIntensity(currentWhitelistItem, _vibratorIntensity);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Execute Pattern:");
            ImGui.TableNextColumn();
            string patternResult = _vibePatternName;
            if (ImGui.InputTextWithHint("##ToyPatternName", "Pattern Name", ref patternResult, 50)) {
                _vibePatternName = patternResult;
            }
            ImGui.TableNextColumn();
            if(ImGui.Button("Execute##ExecuteToyPattern", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                ExecutePlayerToyPattern(currentWhitelistItem, _vibePatternName);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Their Toybox State:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._allowToyboxLocking ? "Locked" : "Unlocked");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleToyboxLock", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerToyboxLockOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }
        }
    }
#endregion DrawWardrobePerms
#region ButtonHelpers
    public void TogglePlayersEnableToyboxOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Enable Toybox Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._enableToybox = !_config.whitelist[currentWhitelistItem]._enableToybox;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerToggleToyState(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Toy State!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._toyActiveState = !_config.whitelist[currentWhitelistItem]._toyActiveState;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleActiveToyboxOption(playerPayload, targetPlayer));
    }

    public void UpdatePlayerToyIntensity(int currentWhitelistItem, int newIntensityLevel) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Toy Intensity to {newIntensityLevel}!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxUpdateActiveToyIntensity(playerPayload, targetPlayer, newIntensityLevel));
    }

    public void ExecutePlayerToyPattern(int currentWhitelistItem, string patternName) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Executing  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Toy Pattern [{patternName}]!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxExecuteStoredToyPattern(playerPayload, targetPlayer, patternName));
    }

    public void TogglePlayerToyboxLockOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Toybox Lock Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._allowToyboxLocking = !_config.whitelist[currentWhitelistItem]._allowToyboxLocking;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleLockToyboxUI(playerPayload, targetPlayer));
    }


#endregion ButtonHelpers
}