using System;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.WardrobeTab;

public class WardrobeGagCompartment
{
    private readonly GagStorageSelector _selector;
    private readonly GagStorageDetails  _details;

    public WardrobeGagCompartment(GagStorageSelector selector, GagStorageDetails details) {
        _selector = selector;
        _details  = details;
    }

    public void DrawContent()
    {
        _selector.Draw(GetSetSelectorSize());
        ImGui.SameLine();
        _details.Draw();
    }

    public float GetSetSelectorSize()
        => 160f * ImGuiHelpers.GlobalScale;
}
