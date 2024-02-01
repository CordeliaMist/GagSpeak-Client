using System.Numerics;
using ImGuiNET;
using GagSpeak.Utility;
using OtterGui;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using Dalamud.Interface.Utility.Raii;
using System.Runtime.CompilerServices;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPermissionEditor {
    public void DrawSettingOverridePerms(int currentWhitelistItem) {
        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("RelationsManagerTable", 3, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            ImGui.TableSetupColumn("Setting/Option",ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Status",        ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Not Alloweds").X);
            ImGui.TableSetupColumn("Toggle##",      ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Toggles").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();

            ImGuiUtil.DrawFrameColumn($"Extended Lock Times:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._grantExtendedLockTimes ? "Allowed" : "Not Allowed");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleExtendedLockTimes", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerExtendedLockTimes(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._directChatGarblerActive ? "Enabled" : "Disabled");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleLiveChatGarbler", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerLiveChatGarbler(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }


            ImGuiUtil.DrawFrameColumn($"Live Chat Garbler Lock:");
            ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._directChatGarblerLocked ? "Locked" : "Unlocked");
            ImGui.TableNextColumn();
            if(ImGui.Button("Toggle##ToggleLiveChatGarblerLock", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
                TogglePlayerLiveChatGarblerLock(currentWhitelistItem);
                _interactOrPermButtonEvent.Invoke();
            }
        }
    }

#region ButtonHelpers
    public void TogglePlayerExtendedLockTimes(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Extended Lock Times Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._grantExtendedLockTimes = !_config.whitelist[currentWhitelistItem]._grantExtendedLockTimes;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerLiveChatGarbler(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Live Chat Garbler Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._directChatGarblerActive = !_config.whitelist[currentWhitelistItem]._directChatGarblerActive;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerLiveChatGarblerLock(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (WhitelistHelpers.IsIndexWithinBounds(currentWhitelistItem, _config)) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_config.whitelist[currentWhitelistItem]._name}'s Live Chat Garbler Lock Option for your character!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _config.whitelist[currentWhitelistItem]._directChatGarblerLocked = !_config.whitelist[currentWhitelistItem]._directChatGarblerLocked;
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

#endregion ButtonHelpers
}