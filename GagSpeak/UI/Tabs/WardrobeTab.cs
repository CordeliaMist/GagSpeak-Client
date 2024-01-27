using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;

using OtterGui;

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class WardrobeTab : ITab
{
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    WardrobeGagCompartment                _GagCompartment;              // for getting the gag shelf
    private readonly    WardrobeRestraintCompartment          _RestraintCompartment;        // for getting the restraint shelf

    // for toggling the restraint shelf tab
    private bool ViewingRestraintCompartment {
        get => _config.viewingRestraintCompartment;
        set {
            _config.viewingRestraintCompartment = value;
            _config.Save();
        }
    } 

    public WardrobeTab(GagSpeakConfig config, WardrobeGagCompartment GagCompartment, WardrobeRestraintCompartment RestraintCompartment) {
        _config         = config;
        _GagCompartment       = GagCompartment;
        _RestraintCompartment = RestraintCompartment;
        _config.viewingRestraintCompartment = false;
    }

    public ReadOnlySpan<byte> Label => "Wardrobe"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        DrawShelfSelection();
        if(ViewingRestraintCompartment) {
            _RestraintCompartment.DrawContent();
        }
        else {
            _GagCompartment.DrawContent();
        }
    }

    /// <summary> Draws out the compartments (better name than shelves?) of our kink wardrobe </summary>
    private void DrawShelfSelection() {
        // make our buttons look like selection tabs
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / 2, ImGui.GetFrameHeight());
        // draw out the buttons for the compartments of our kink wardrobe
        if (ImGuiUtil.DrawDisabledButton("Gag Storage Compartment", buttonSize, "Shows all of your stored gag's and lets you configure unique settings for each!", !ViewingRestraintCompartment))
            ViewingRestraintCompartment = false;
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Restraint Outfits Compartment", buttonSize, "Configure Lockable Restraint sets that can act as an overlay for your glamour!", ViewingRestraintCompartment))
            ViewingRestraintCompartment = true;
    }
}
