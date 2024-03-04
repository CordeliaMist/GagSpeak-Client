using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using OtterGui;

namespace GagSpeak.UI.Tabs.WardrobeTab;
public enum WardrobeSubTab {
    GagStorage,
    RestraintSetCompartment,
}

public class WardrobeTab : ITab
{
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    WardrobeGagCompartment          _GagCompartment;              // for getting the gag shelf
    private readonly    WardrobeRestraintCompartment    _RestraintCompartment;        // for getting the restraint shelf
    private             WardrobeSubTab                  _subTab;                // for getting the sub tab

    public WardrobeTab(GagSpeakConfig config, WardrobeGagCompartment GagCompartment, WardrobeRestraintCompartment RestraintCompartment) {
        _config = config;
        _GagCompartment = GagCompartment;
        _RestraintCompartment = RestraintCompartment;
        _subTab = _config.WardrobeActiveTab;
    }

    public ReadOnlySpan<byte> Label => "Wardrobe"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        if(_subTab != _config.WardrobeActiveTab) {
            _subTab = _config.WardrobeActiveTab;
        }
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        DrawShelfSelection();
        if(_subTab == WardrobeSubTab.GagStorage) {
            _GagCompartment.DrawContent();
        }
        else {
            _RestraintCompartment.DrawContent();
        }
    }

    /// <summary> Draws out the compartments (better name than shelves?) of our kink wardrobe </summary>
    private void DrawShelfSelection() {
        // make our buttons look like selection tabs
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / 2, ImGui.GetFrameHeight());
        // draw out the buttons for the compartments of our kink wardrobe
        if (ImGuiUtil.DrawDisabledButton("Gag Storage Compartment", buttonSize, "Shows all of your stored gag's and lets you configure unique settings for each!",
        _subTab == WardrobeSubTab.GagStorage))
        {
            _config.SetWardrobeActiveTab(WardrobeSubTab.GagStorage);
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Restraint Outfits Compartment", buttonSize, "Configure Lockable Restraint sets that can act as an overlay for your glamour!",
        _subTab == WardrobeSubTab.RestraintSetCompartment))
        {
            _config.SetWardrobeActiveTab(WardrobeSubTab.RestraintSetCompartment);
        }
        // end the style
        style.Pop();
    }
}
