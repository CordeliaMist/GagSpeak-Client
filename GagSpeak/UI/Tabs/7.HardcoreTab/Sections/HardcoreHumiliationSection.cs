using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using ImGuiNET;
using OtterGui;
using Dalamud.Plugin.Services;
using GagSpeak.Hardcore.Actions;
using JetBrains.Annotations;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.Utility;
using System.Runtime.InteropServices;
using GagSpeak.Services;
namespace GagSpeak.UI.Tabs.HardcoreTab;
public class HC_ControlHumiliation
{
    private readonly HardcoreManager _hcManager;
    private readonly GsActionManager _actionManager;
    private readonly CharacterHandler _charHandler;
    private readonly FontService _fontService;
    private readonly IClientState _client;
    public HC_ControlHumiliation(HardcoreManager hardcoreManager, GsActionManager actionManager,
    CharacterHandler charHandler, FontService fontService, IClientState client) {
        _hcManager = hardcoreManager;
        _actionManager = actionManager;
        _charHandler = charHandler;
        _fontService = fontService;
        _client = client;
    }
    public void Draw() {
        using var child = ImRaii.Child("##HC_HumiliationChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        var name = $"{_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}";
        var yourName = "";
        if(_client.LocalPlayer == null) { yourName = "You"; }
        else { yourName = $"{_client.LocalPlayer.Name.ToString().Split(' ')[0]}"; }
        // show header
        ImGui.PushFont(_fontService.UidFont);
        try{
            ImGuiUtil.Center($"Control & Humilation Permissions for {name}");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
            $"This determines what you are allowing {name} to be able to control you to do.\n"+
            "You are NOT controlling what you can do to them");
            }
        } finally { ImGui.PopFont(); }

        ImGui.Separator();
        // draw out the options
        UIHelpers.CheckboxNoConfig($"{name} can blindfold you.",
        $"Whenever {name} wants, they can blindfold you. Either can toggle off",
        _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._allowBlindfold,
        v => _hcManager.SetAllowBlindfold(_charHandler.activeListIdx, v)
        );
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig($"", $"{name} can force you to follow.", 
        _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._blindfolded,
        v => _hcManager.SetBlindfolded(_charHandler.activeListIdx, v)
        );
        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
        try {
            ImGui.TextWrapped($"Whenever {name} wants, they can blindfold you. Either can toggle off");
        } finally { ImGui.PopStyleColor(); }

        ImGui.Separator();    
    }
}