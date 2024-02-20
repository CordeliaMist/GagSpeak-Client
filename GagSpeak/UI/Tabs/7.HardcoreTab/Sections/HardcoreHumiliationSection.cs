using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_Humiliation
{
    private readonly CharacterHandler _charHandler;
    public HC_Humiliation(CharacterHandler characterHandler) {
        _charHandler = characterHandler;
    }

    public void Draw() {
        using var child = ImRaii.Child("##HC_Humiliation", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child)
            return;

        ImGuiUtil.Center("whats this? Idk.");
    }
}