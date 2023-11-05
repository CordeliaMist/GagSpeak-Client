using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using OtterGui;
using OtterGui.Raii;

using GagSpeak.Chat;
using GagSpeak.Services;
using GagSpeak.Events;
using OtterGui.Widgets;
using Lumina;
using Dalamud.Interface.Utility;
using System;
using OtterGui.Log;
using GagSpeak.UI.Helpers;
using System.Threading;

// Practicing Modular Design
namespace GagSpeak.UI.GagListings;
public class GagListingsDrawer
{
    private const float DefaultWidth = 280; // set the default width
    private readonly GagSpeakConfig _config;
    private GagTypeFilterCombo[] _gagTypeFilterCombo; // create an array of item combos
    private GagLockFilterCombo[] _gagLockFilterCombo; // create an array of item combos
    
    private float _requiredComboWidthUnscaled;
    private float _requiredComboWidth;
    
    // I believe this dictates the data for the stain list, swap out for padlock list probably
    
    public GagListingsDrawer(GagSpeakConfig config) // Constructor
    {
        _config = config;
        // draw out our gagtype filter combo listings
        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_config.GagTypes, _config.selectedGagTypes[0], _config),
            new GagTypeFilterCombo(_config.GagTypes, _config.selectedGagTypes[1], _config),
            new GagTypeFilterCombo(_config.GagTypes, _config.selectedGagTypes[2], _config)
        };
        // draw out our gagpadlock filter combo listings
        _gagLockFilterCombo = new GagLockFilterCombo[] {
            new GagLockFilterCombo(_config.selectedGagPadlocks[0], _config),
            new GagLockFilterCombo(_config.selectedGagPadlocks[1], _config),
            new GagLockFilterCombo(_config.selectedGagPadlocks[2], _config)
        };
    }

    private Vector2 _iconSize;
    private float _comboLength;

    // This function just prepares our styleformat for the drawing
    public void PrepareGagListDrawing() {
        // Draw out the content size of our icon
        _iconSize = new Vector2(2 * ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y);
        // Determine the size of our comboLength
        _comboLength = DefaultWidth * ImGuiHelpers.GlobalScale;
        // if the required combo with is unscaled
        if (_requiredComboWidthUnscaled == 0)
            _requiredComboWidthUnscaled = _config.GagTypes.Keys.Max(key => ImGui.CalcTextSize(key).X) / ImGuiHelpers.GlobalScale;
        // get the scaled combo width
        _requiredComboWidth = _requiredComboWidthUnscaled * ImGuiHelpers.GlobalScale;
    }


    // Draw the listings
    public void DrawGagAndLockListing(int ID, string GagLabel, GagPadlocks LockLabel,
                                      int layerIndex, string displayLabel, bool locked) {
        // push our styles
        using var    id = ImRaii.PushId($"{ID}_listing"); // push the ID
        var     spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y }; // push spacing
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing); // push style

        // draw our icon thingy
        //currentArmor.DrawGagIcon(_textures, _iconSize, slot);        
        ImGui.SameLine();
        // create a group for the 2 dropdowns and icon
        using var group = ImRaii.Group();
        if (DrawGagTypeItemCombo(ID, GagLabel, layerIndex, locked)) {
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(displayLabel); // draw the label text

        if (DrawGagLockItemCombo(ID, LockLabel, layerIndex, locked)) {
        }
    }

    // draw the gag item combo
    private bool DrawGagTypeItemCombo(int ID, string label, int layerIndex, bool locked) {
        // set our combo the the combofilterlist we want to see
        var combo = _gagTypeFilterCombo[layerIndex]; // get the combo

        // if we left click and it is unlocked, open it
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Type");

        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        combo.Draw(ID, label, layerIndex);

        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                _config.selectedGagTypes[layerIndex] = _config.GagTypes.Keys.First(); // update config
                label = _config.GagTypes.Keys.First(); // to the first option, none
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }

        return true;
    }

    private bool DrawGagLockItemCombo(int ID, GagPadlocks label, int layerIndex, bool locked) {
        // set our combo the the combofilterlist we want to see
        var combo = _gagLockFilterCombo[layerIndex]; // get the combo

        // if we left click and it is unlocked, open it
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Enum");

        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        combo.Draw(ID, label, layerIndex);

        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                _config.selectedGagPadlocks[layerIndex] = GagPadlocks.None; // update config
                label = GagPadlocks.None; // to the first option, none
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }

        return true;
    }
}