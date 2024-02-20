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
    private readonly    HC_OrdersControl          _movementControl; // for getting the movement control
    private readonly    HC_Humiliation              _humiliation; // for getting the humiliation
    private             HardcoreSubTab              _activeTab;
    public HardcoreMainPanel(HC_RestraintSetProperties restraintSetProperties,
    HC_OrdersControl movementControl, HC_Humiliation humiliation) {
        _restraintSetProperties = restraintSetProperties;
        _movementControl = movementControl;
        _humiliation = humiliation;

        _activeTab = HardcoreSubTab.RestraintSetProperties;        
    }

    public void Draw() {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using (var group = ImRaii.Group()) {
            DrawShelfSelection();
            if(_activeTab == HardcoreSubTab.RestraintSetProperties) {
                _restraintSetProperties.Draw();
            }
            else if(_activeTab == HardcoreSubTab.MovementControl) {
                _movementControl.Draw();
            }
            else if(_activeTab == HardcoreSubTab.Humiliation) {
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
        _activeTab == HardcoreSubTab.RestraintSetProperties))
        {
            _activeTab = HardcoreSubTab.RestraintSetProperties;
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Movement Control", buttonSize,
        "Configure movement control settings!",
        _activeTab == HardcoreSubTab.MovementControl)) 
        {
            _activeTab = HardcoreSubTab.MovementControl;
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Humiliation", buttonSize,
        "Configure humiliation settings!",
        _activeTab == HardcoreSubTab.Humiliation)) 
        {
            _activeTab = HardcoreSubTab.Humiliation;
        }
    }
}