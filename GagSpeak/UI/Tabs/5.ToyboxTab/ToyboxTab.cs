using System;
using ImGuiNET;
using OtterGui.Widgets;
using Dalamud.Interface.Utility;

namespace GagSpeak.UI.Tabs.ToyboxTab;
/// <summary> This class is used to handle the Toybox Tab. </summary>
public class ToyboxTab : ITab
{
    private readonly    ToyboxSelector  _selector;
    private readonly    ToyboxPanel     _panel;

    public ToyboxTab(ToyboxSelector selector, ToyboxPanel panel) {
        _selector = selector;
        _panel = panel;
    }

    public void DrawContent()
    {
        _selector.Draw(GetSetSelectorSize());
        ImGui.SameLine();
        _panel.Draw();
    }

    public float GetSetSelectorSize() {
        return 140f * ImGuiHelpers.GlobalScale;
    }

    public ReadOnlySpan<byte> Label => "Toybox"u8; // apply the tab label


}
