using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class TriggersSubtab
{
    // any other included classes here
    private readonly IClientState _client;
    private readonly CharacterHandler _charHandler;
    private readonly FontService _fontService;
    public TriggersSubtab(CharacterHandler charHandler, FontService fontService,
    IClientState client) {
        _charHandler = charHandler;
        _fontService = fontService;
        _client = client;
        // other initializations here
    }

    public void Draw() {
        // any other multiple components to draw here
        DrawTriggersUI();
    }

    private void DrawTriggersUI() {
        using var child = ImRaii.Child("##ToyboxTriggersChild", new Vector2(ImGui.GetContentRegionAvail().X, -ImGuiHelpers.GlobalScale*81), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // pop the zero spacing style var for everything inside
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        var name = $"{AltCharHelpers.FetchCurrentName().Split(' ')[0]}";
        var yourName = "";
        if(_client.LocalPlayer == null) { yourName = "You"; }
        else { yourName = $"{_client.LocalPlayer.Name.ToString().Split(' ')[0]}"; }
        // show header
        ImGui.PushFont(_fontService.UidFont);
        try{
            ImGuiUtil.Center($"Triggers Setup for {name}");
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip($"Any Triggers set down to the list below can be applied by {name} if you allow it.");
            }
        } finally {
            ImGui.PopFont();
        }
        
        ImGui.Separator();

        // draw out the UI content for the main triggers draw window
        ImGuiUtil.Center($"Will be a bit before this is present");



        // at the very bottom, pop the style
        ImGui.PopStyleVar();
    }
}