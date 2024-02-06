using System;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.ToyboxTab;

public class ToyboxOverviewSubtab
{
    private readonly ToyboxSelector _selector;
    private readonly ToyboxOverviewPanel _overviewPanel;

    public ToyboxOverviewSubtab(ToyboxSelector selector, ToyboxOverviewPanel overviewPanel)
    {
        _selector = selector;
        _overviewPanel = overviewPanel;
    }
    public void DrawContent()
    {
        _selector.Draw(GetSetSelectorSize());
        ImGui.SameLine();
        _overviewPanel.Draw();
    }

    public float GetSetSelectorSize()
        => 140f * ImGuiHelpers.GlobalScale;
}
