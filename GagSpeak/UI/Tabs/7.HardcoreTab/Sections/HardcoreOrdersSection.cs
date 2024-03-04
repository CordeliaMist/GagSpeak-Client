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
    private readonly HardcoreManager _hcManager;
    private readonly GsActionManager _actionManager;
    private readonly CharacterHandler _charHandler;
    private readonly FontService _fontService;
    private readonly IClientState _client;
    public HC_Orders(HardcoreManager hardcoreManager, GsActionManager actionManager,
    CharacterHandler charHandler, FontService fontService, IClientState client) {
        _hcManager = hardcoreManager;
        _actionManager = actionManager;
        _charHandler = charHandler;
        _fontService = fontService;
        _client = client;
    }
    public void Draw() {
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
        UIHelpers.CheckboxNoConfig("", $"Enable the forced follow from {name}, over allow forced follow",
        _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._forcedFollow,
        v => _hcManager.SetForcedFollow(_charHandler.activeListIdx, v));
        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
        try {
            ImGui.TextWrapped($"Follow {name} when they say \"{yourName}, follow me.\" in any channel.\n"+
            $"Movement Input is blocked until your character stops moving for a set amount of time.");
        } finally { ImGui.PopStyleColor(); }

        ImGui.Separator();
        // draw out sit option
        UIHelpers.CheckboxNoConfig($"{name} can order you to sit.",
        $"You will be forcibily sat down on your nees when {name} says to you \"{yourName}, sit.\" in any channel.\n"+
        $"Your movement is restricted until they say \"you may stand now {yourName}.\"",
        _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._allowForcedSit,
        v => _hcManager.SetAllowForcedSit(_charHandler.activeListIdx, v));
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("", $"Enable the forced sit from {name}, over allow forced sit",
        _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._forcedSit,
        v => _hcManager.SetForcedSit(_charHandler.activeListIdx, v));
        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
        try {
            ImGui.TextWrapped($"When {name} says to you \"{yourName}, sit.\" in any channel. You will be forcibly sat down. "+
            $"Movement is restricted until they say \"you may stand now {yourName}.\"");
        } finally { ImGui.PopStyleColor(); }

        ImGui.Separator();
    }
}