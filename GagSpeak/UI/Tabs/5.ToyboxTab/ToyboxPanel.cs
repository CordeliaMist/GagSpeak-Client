using System.Numerics;
using GagSpeak.Services;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public enum ToyboxSubTab {
    Setup,
    Patterns,
    Triggers
}

public class ToyboxPanel
{
    private readonly PatternPlayback            _patternPlayback; // for getting the pattern playback
    private readonly PermsAndInfoSubtab         _permsInfoSubtab; // for getting the setup and info subtab
    private readonly PatternSubtab              _patternSubtab; // for getting the pattern subtab
    private readonly TriggersSubtab             _triggerSubtab; // for getting the trigger subtab
    private          ToyboxSubTab               _toyboxActiveTab;
    private readonly GagSpeakConfig             _config;
    public ToyboxPanel(FontService fontService, PatternPlayback patternPlayback,
    TriggersSubtab triggerSubtab, ToyboxPatternTable patternTable, GagSpeakConfig config,
    PermsAndInfoSubtab PermsAndInfoSubtab, PatternSubtab patternSubtab) {
        _config = config;
        _patternPlayback = patternPlayback;
        _permsInfoSubtab = PermsAndInfoSubtab;
        _patternSubtab = patternSubtab;
        _triggerSubtab = triggerSubtab;
        _toyboxActiveTab = ToyboxSubTab.Setup;
    }

    public void Draw() {
        if(_toyboxActiveTab != _config.ToyboxActiveTab) {
            _toyboxActiveTab = _config.ToyboxActiveTab;
        }
        // subtab selection
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using (var group = ImRaii.Group()) {
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            DrawShelfSelection();
            if(_toyboxActiveTab == ToyboxSubTab.Setup) {
                _permsInfoSubtab.Draw();
            }
            else if(_toyboxActiveTab == ToyboxSubTab.Patterns) {
                _patternSubtab.Draw();
            }
            else if(_toyboxActiveTab == ToyboxSubTab.Triggers) {
                _triggerSubtab.Draw();
            }
            _patternPlayback.Draw(); // draw playback at bottom
        }
    }

    private void DrawShelfSelection() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).
                        Push(ImGuiStyleVar.FrameRounding, 0);
        // button size
        var initialWidth = ImGui.GetContentRegionAvail().X;
        var width1 = initialWidth *.45f;
        var width2 = initialWidth *.275f;
        var width3 = initialWidth *.275f;
        var buttonSize = new Vector2((ImGui.GetContentRegionAvail().X-ImGui.GetFrameHeight()) / 3, ImGui.GetFrameHeight());
        // tab selection
        if (ImGuiUtil.DrawDisabledButton("Toy Overview & Settings", new Vector2(width1, ImGui.GetFrameHeight()),
        "Configure permissions for this player, and setup the connection for your toy!",
        _toyboxActiveTab == ToyboxSubTab.Setup))
        {
            _config.SetToyboxActiveTab(ToyboxSubTab.Setup);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Patterns", new Vector2(width2, ImGui.GetFrameHeight()),
        "Configure movement control settings!",
        _toyboxActiveTab == ToyboxSubTab.Patterns)) 
        {
            _config.SetToyboxActiveTab(ToyboxSubTab.Patterns);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Triggers", new Vector2(width3, ImGui.GetFrameHeight()),
        "Configure Restriction settings!",
        _toyboxActiveTab == ToyboxSubTab.Triggers)) 
        {
            _config.SetToyboxActiveTab(ToyboxSubTab.Triggers);
        }
    }
}
