using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using GagSpeak.Events;
using System.Diagnostics;
using Dalamud.Interface;

namespace GagSpeak.UI.Tabs.HardcoreTab;
// what type of hardcore restrictions are we looking at modifying?
public enum HardcoreSubTab {
    RestraintSetProperties,
    MovementControl,
    Humiliation,
}

public class HardcoreMainPanel
{
    private readonly    HC_RestraintSetProperties   _restraintSetProperties; // for getting the restraint set properties
    private readonly    HC_OrdersControl            _movementControl; // for getting the movement control
    private readonly    HC_Humiliation              _humiliation; // for getting the humiliation
    private             HardcoreSubTab              _hcActiveTab;
    private             GagSpeakConfig              _config;
    public HardcoreMainPanel(HC_RestraintSetProperties restraintSetProperties,
    HC_OrdersControl movementControl, HC_Humiliation humiliation, GagSpeakConfig config) {
        _restraintSetProperties = restraintSetProperties;
        _movementControl = movementControl;
        _humiliation = humiliation;
        _config = config;

        _hcActiveTab = HardcoreSubTab.RestraintSetProperties;        
    }

    public void Draw() {
        if(_hcActiveTab != _config.ActiveTab) {
            _hcActiveTab = _config.ActiveTab;
        }
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using (var group = ImRaii.Group()) {
            DrawShelfSelection();
            if(_hcActiveTab == HardcoreSubTab.RestraintSetProperties) {
                _restraintSetProperties.Draw();
            }
            else if(_hcActiveTab == HardcoreSubTab.MovementControl) {
                _movementControl.Draw();
            }
            else if(_hcActiveTab == HardcoreSubTab.Humiliation) {
                _humiliation.Draw();
            }
        }
    }

    private void DrawShelfSelection() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).
                        Push(ImGuiStyleVar.FrameRounding, 0);
        // button size
        var buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / 3, ImGui.GetFrameHeight());
        // tab selection
        if (ImGuiUtil.DrawDisabledButton("Restraint Properties", buttonSize,
        "Configure Lockable Restraint sets that can act as an overlay for your glamour!",
        _hcActiveTab == HardcoreSubTab.RestraintSetProperties))
        {
            _config.SetActiveHcTab(HardcoreSubTab.RestraintSetProperties);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Orders / Control", buttonSize,
        "Configure movement control settings!",
        _hcActiveTab == HardcoreSubTab.MovementControl)) 
        {
            _config.SetActiveHcTab(HardcoreSubTab.MovementControl);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Humiliation", buttonSize,
        "Configure humiliation settings!",
        _hcActiveTab == HardcoreSubTab.Humiliation)) 
        {
            _config.SetActiveHcTab(HardcoreSubTab.Humiliation);
        }
    }
}