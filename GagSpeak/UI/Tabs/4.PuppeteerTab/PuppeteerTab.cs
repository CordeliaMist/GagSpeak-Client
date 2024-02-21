using System;
using System.Collections.Generic;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using GagSpeak.Services;
using GagSpeak.UI.Tabs.WhitelistTab;
using ImGuiNET;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.PuppeteerTab;

/// <summary> This class is used to handle the _characterHandler.whitelistChars tab. </summary>
public class PuppeteerTab : ITab
{
    private readonly    PuppeteerSelector           _selector;
    private readonly    PuppeteerPanel              _panel;


    public PuppeteerTab(PuppeteerSelector selector, PuppeteerPanel panel) {
        _selector = selector;
        _panel = panel;
    }

    public ReadOnlySpan<byte> Label
        => "Puppeteer"u8;

    public void DrawContent()
    {
        _selector.Draw(GetSelectorWidth());
        ImGui.SameLine();
        _panel.Draw();
    }

    public float GetSelectorWidth()
        => 140f * ImGuiHelpers.GlobalScale;

    public float GetPanelHeight()
        => 175f * ImGuiHelpers.GlobalScale;
}
