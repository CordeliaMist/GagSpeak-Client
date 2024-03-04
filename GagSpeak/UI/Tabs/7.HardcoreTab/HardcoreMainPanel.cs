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
    Orders,
    Restrictions,
    Debugger,
}

public class HardcoreMainPanel
{
    private readonly    HC_RestraintSetProperties   _restraintSetProperties; // for getting the restraint set properties
    private readonly    HC_Orders                   _Orders; // for getting the movement control
    private readonly    HC_ControlRestrictions       _Restrictions; // for getting the Restrictions
    private readonly    ActionDataSnagger           _actionDataSnagger; // for getting the action data
    private             HardcoreSubTab              _hcActiveTab;
    private             GagSpeakConfig              _config;
    public HardcoreMainPanel(HC_RestraintSetProperties restraintSetProperties,
    HC_Orders Orders, HC_ControlRestrictions Restrictions, ActionDataSnagger actionDataSnagger, 
    GagSpeakConfig config) {
        _restraintSetProperties = restraintSetProperties;
        _Orders = Orders;
        _Restrictions = Restrictions;
        _actionDataSnagger = actionDataSnagger;
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
            else if(_hcActiveTab == HardcoreSubTab.Orders) {
                _Orders.Draw();
            }
            else if(_hcActiveTab == HardcoreSubTab.Restrictions) {
                _Restrictions.Draw();
            }
            else if(_hcActiveTab == HardcoreSubTab.Debugger) {
                _actionDataSnagger.Draw();
            }
        }
    }

    private void DrawShelfSelection() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).
                        Push(ImGuiStyleVar.FrameRounding, 0);
        // button size
        var initialWidth = ImGui.GetContentRegionAvail().X-ImGui.GetFrameHeight();
        var width1 = initialWidth *.35f;
        var width2 = initialWidth *.3f;
        var width3 = initialWidth *.35f;
        var buttonSize = new Vector2((ImGui.GetContentRegionAvail().X-ImGui.GetFrameHeight()) / 3, ImGui.GetFrameHeight());
        // tab selection
        if (ImGuiUtil.DrawDisabledButton("Restraint Properties", new Vector2(width1, ImGui.GetFrameHeight()),
        "Configure Lockable Restraint sets that can act as an overlay for your glamour!",
        _hcActiveTab == HardcoreSubTab.RestraintSetProperties))
        {
            _config.SetActiveHcTab(HardcoreSubTab.RestraintSetProperties);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Orders", new Vector2(width2, ImGui.GetFrameHeight()),
        "Configure movement control settings!",
        _hcActiveTab == HardcoreSubTab.Orders)) 
        {
            _config.SetActiveHcTab(HardcoreSubTab.Orders);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Restrictions", new Vector2(width3, ImGui.GetFrameHeight()),
        "Configure Restriction settings!",
        _hcActiveTab == HardcoreSubTab.Restrictions)) 
        {
            _config.SetActiveHcTab(HardcoreSubTab.Restrictions);
        }
        ImGui.SameLine();
        if(ImGuiUtil.DrawDisabledButton("?", new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeight()),
        "Debugger", _hcActiveTab == HardcoreSubTab.Debugger)) {
            _config.SetActiveHcTab(HardcoreSubTab.Debugger);
        }
    }
}