using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using OtterGui;
using GagSpeak.CharacterData;

namespace GagSpeak.UI.Tabs.ToyboxTab;
/// <summary> This class is used to handle the Toybox Tab. </summary>
public class ToyboxTab : ITab
{
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    CharacterHandler                _characterHandler;      // for getting the character handler
    private readonly    ToyboxOverviewSubtab            _overviewSubtab;        // for getting the overview subtab
    private readonly    ToyboxWorkshopSubtab            _workshopSubtab;        // for getting the workshop subtab
    private Vector4 lovenseDragButtonBGAlt = new Vector4(0.1f, 0.1f, 0.1f, 0.930f);
    private bool ToyboxLeftSubTabActive {
        get => _config.ToyboxLeftSubTabActive;
        set
        {
            _config.ToyboxLeftSubTabActive = value;
            _config.Save();
        }
    } 

    public ToyboxTab(GagSpeakConfig config, ToyboxOverviewSubtab overviewSubtab, 
    ToyboxWorkshopSubtab workshopSubtab, CharacterHandler characterHandler) {
        _config         = config;
        _characterHandler = characterHandler;
        _overviewSubtab = overviewSubtab;
        _workshopSubtab = workshopSubtab;
    }

    public ReadOnlySpan<byte> Label => "Toybox"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the Toybox Tab </summary>
    public void DrawContent() {
        if(_characterHandler.playerChar._lockToyboxUI) { ImGui.BeginDisabled(); }
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        DrawShelfSelection();
        if(ToyboxLeftSubTabActive) {
            using var c = ImRaii.PushColor(ImGuiCol.ChildBg, lovenseDragButtonBGAlt);
            _workshopSubtab.Draw();
            c.Dispose();
            c.Pop();
        }
        else {
            _overviewSubtab.DrawContent();
        }
        if(_characterHandler.playerChar._lockToyboxUI) { ImGui.EndDisabled(); }
    }

    /// <summary> Draws out the subtabs for the toybox tab </summary>
    private void DrawShelfSelection() {
        // make our buttons look like selection tabs
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / 2, ImGui.GetFrameHeight());
        // draw out the buttons subtabs of our toybox
        if (ImGuiUtil.DrawDisabledButton("Toybox Overview", buttonSize, "Shows your toybox's connection status, and settings for each whitelisted player", !ToyboxLeftSubTabActive))
            ToyboxLeftSubTabActive = false;
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("The Workshop", buttonSize, "A savable pattern creator, for all your fun needs~", ToyboxLeftSubTabActive))
            ToyboxLeftSubTabActive = true;
        style.Pop();
    }
}
