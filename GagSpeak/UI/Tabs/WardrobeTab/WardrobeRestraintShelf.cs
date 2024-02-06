using System;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.Wardrobe;
using ImGuiNET;
using OtterGui;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.WardrobeTab;

public class WardrobeRestraintCompartment
{
    private readonly RestraintSetSelector _selector;
    private readonly RestraintSetEditor  _editor;
    private readonly RestraintSetManager _restraintSetManager;

    public WardrobeRestraintCompartment(RestraintSetSelector selector,
    RestraintSetEditor editor, RestraintSetManager restraintSetManager) {
        _selector = selector;
        _editor  = editor;
        _restraintSetManager = restraintSetManager;
    }

    public void DrawContent()
    {
        // make content disabled
        if(_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._locked) { ImGui.BeginDisabled(); }
        // draw the selector for the set
        _selector.Draw(GetSetSelectorWidth(), GetInfoSectionHeight());
        // draw the editor for that set
        _editor.Draw();
        // remove the disabled state
        if(_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._locked) { ImGui.EndDisabled(); }
    }

    public float GetSetSelectorWidth()
        => 200f * ImGuiHelpers.GlobalScale;

    public float GetInfoSectionHeight()
        => 145f * ImGuiHelpers.GlobalScale;
}
