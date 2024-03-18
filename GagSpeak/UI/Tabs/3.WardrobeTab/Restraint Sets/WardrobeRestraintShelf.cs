using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.Wardrobe;
using ImGuiNET;
using OtterGui;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.WardrobeTab;

public class WardrobeRestraintCompartment
{
    private readonly RestraintSetSelector   _selector;
    private readonly RestraintSetOverview   _overview;
    private readonly RestraintSetEditor     _editor;
    private readonly RestraintSetManager    _restraintSetManager;

    public WardrobeRestraintCompartment(RestraintSetSelector selector,
    RestraintSetEditor editor, RestraintSetManager restraintSetManager,
    RestraintSetOverview overview) {
        _selector = selector;
        _editor  = editor;
        _restraintSetManager = restraintSetManager;
        _overview = overview;
    }

    public void DrawContent()
    {
        using var child = ImRaii.Child("##RestraintSetShelf", new Vector2(ImGui.GetContentRegionAvail().X, -1), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        if (!child)
            return;

        // make content disabled (the temp index is to fix a bug which occurs when the index is changed within this loop and the end condition is different)
        var tempIndexLockBool = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._locked;
        if(tempIndexLockBool) { ImGui.BeginDisabled(); }
        try{
        // draw the selector for the set
        _selector.Draw(GetSetSelectorWidth());
        ImGui.SameLine();
        _overview.Draw();
        // draw the editor for that set
        _editor.Draw();
        // remove the disabled state
        } finally {
            if(tempIndexLockBool) { ImGui.EndDisabled(); }
        }
    }

    public float GetSetSelectorWidth()
        => 175f * ImGuiHelpers.GlobalScale;
}
