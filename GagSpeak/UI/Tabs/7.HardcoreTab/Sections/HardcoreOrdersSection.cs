using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using GagSpeak.Hardcore.Actions;
using GagSpeak.Services;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_Orders
{
    private readonly GagSpeakConfig _config;
    private readonly HardcoreManager _hcManager;
    private readonly CharacterHandler _charHandler;
    private readonly FontService _fontService;
    private readonly IClientState _client;
    public HC_Orders(HardcoreManager hardcoreManager, CharacterHandler charHandler,
    FontService fontService, IClientState client, GagSpeakConfig config) {
        _config = config;
        _hcManager = hardcoreManager;
        _charHandler = charHandler;
        _fontService = fontService;
        _client = client;
    }
    public void Draw() {
        if (!_config.hardcoreMode) { ImGui.BeginDisabled(); }
        try {
            using var child = ImRaii.Child("##HC_OrdersChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
            if (!child)
                return;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
            var name = $"{_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}";
            var yourName = "";
            if(_client.LocalPlayer == null) { yourName = "You"; }
            else { yourName = $"{_client.LocalPlayer.Name.ToString().Split(' ')[0]}"; }
            // show header
            ImGui.PushFont(_fontService.UidFont);
            try{
                ImGuiUtil.Center($"Order Permissions for {name}");
                if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"This determines what you are allowing {name} to be able to order you to do.\n"+
                "You are NOT controlling what you can do to them");
                }
            } finally { ImGui.PopFont(); }

            ImGui.Separator();
            // draw out the options
            UIHelpers.CheckboxNoConfig($"{name} can order you to follow them.",
            $"Automatically follow {name} when they say to you \"{yourName}, follow me.\" in any channel.",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._allowForcedFollow,
            v => _hcManager.SetAllowForcedFollow(_charHandler.activeListIdx, v));
            ImGui.SameLine();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.RightAlign((_hcManager._perPlayerConfigs[_charHandler.activeListIdx]._forcedFollow ? FontAwesomeIcon.UserCheck : FontAwesomeIcon.UserTimes).ToIconString());
            }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"Shows if the current option is currently active for your player, or inactive");
            }
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            try {
                ImGui.TextWrapped($"Follow {name} when they say \"{yourName}, follow me.\" in any channel.\n"+
                $"Movement Input is blocked until your character stops moving for a set amount of time.");
            } finally { ImGui.PopStyleColor(); }

            ImGui.Separator();
            // draw out sit option
            UIHelpers.CheckboxNoConfig($"{name} can order you to sit.",
            $"You will be forcibily execute /sit when {name} says to you \"{yourName}, sit.\", or /groundsit when {name} says to you \"{yourName}, on your knees.\" in any channel\n"+
            $"Your movement is restricted until they say \"you may stand now {yourName}.\"",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._allowForcedSit,
            v => _hcManager.SetAllowForcedSit(_charHandler.activeListIdx, v));
            ImGui.SameLine();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.RightAlign((_hcManager._perPlayerConfigs[_charHandler.activeListIdx]._forcedSit ? FontAwesomeIcon.UserCheck : FontAwesomeIcon.UserTimes).ToIconString());
            }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"Shows if the current option is currently active for your player, or inactive");
            }
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            try {
                ImGui.TextWrapped($"When {name} says to you \"{yourName}, sit.\" in any channel. You will be forcibly sat down. "+
                $"Movement is restricted until they say \"you may stand now {yourName}.\"");
            } finally { ImGui.PopStyleColor(); }

            ImGui.Separator();
        } finally {
            if (!_config.hardcoreMode) { ImGui.EndDisabled(); }
        }
    }
}