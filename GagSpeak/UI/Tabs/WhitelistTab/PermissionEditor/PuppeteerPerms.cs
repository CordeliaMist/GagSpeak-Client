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

#region DrawPuppeteerPerms
    public void DrawPuppeteerPerms(int currentWhitelistItem) {
        // draw out the table for our permissions
        using (var tablePuppeteer = ImRaii.Table("RelationsManagerTable", 3, ImGuiTableFlags.RowBg)) {
            if (!tablePuppeteer) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Setting/Option",ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Status",        ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Falsemm").X);
            ImGui.TableSetupColumn("Toggle##",      ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglem").X);
            ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            // will get back to drawing out the trigger phrase field as this is a little bit confusing
            ImGuiUtil.DrawFrameColumn($"Allow Sit Requests:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._allowSitRequests ? "True" : "False");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleSitRequests", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayersSitRequestOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }

            ImGuiUtil.DrawFrameColumn($"Allow Motion Requests:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._allowMotionRequests ? "True" : "False");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleMotionRequests", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerMotionRequestsOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }

            ImGuiUtil.DrawFrameColumn($"Allow All Commands:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._allowAllCommands ? "True" : "False");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleAllCommands", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerAllCommandsOption(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }
        }
    }
#endregion DrawPuppeteerPerms
#region ButtonHelpers
    public void TogglePlayersSitRequestOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Allow Sit Requests Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._allowSitRequests = !_config.whitelist[currentWhitelistItem]._allowSitRequests;
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleOnlySitRequestOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerMotionRequestsOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Allow Motion Requests Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._allowMotionRequests = !_config.whitelist[currentWhitelistItem]._allowMotionRequests;
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleOnlyMotionRequestOption(playerPayload, targetPlayer));
    }
    public void TogglePlayerAllCommandsOption(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Allow All Commands Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._allowAllCommands = !_config.whitelist[currentWhitelistItem]._allowAllCommands;
        _chatManager.SendRealMessage(_messageEncoder.EncodePuppeteerToggleAllCommandsOption(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}