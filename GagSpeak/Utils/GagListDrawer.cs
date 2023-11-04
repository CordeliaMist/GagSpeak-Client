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

// Practicing Modular Design
namespace GagSpeak.UI.Helpers;
public class GagListDrawer
{
    private const float DefaultWidth = 280; // set the default width
    private readonly GagSpeakConfig _config;
    private readonly GagItemCombo[] _itemCombo; // create an array of item combos

    private float _requiredComboWidthUnscaled;
    private float _requiredComboWidth;
    
    // I believe this dictates the data for the stain list, swap out for padlock list probably
    
    public GagListDrawer(GagSpeakConfig config) // Constructor
    {
        _config = config;
        //create combo's
        _itemCombo = new GagItemCombo[] {
            new GagItemCombo(_config.GagTypes, "LayerOneGagType", GagSpeak.Log),
            new GagItemCombo(_config.GagTypes, "LayerTwoGagType", GagSpeak.Log),
            new GagItemCombo(_config.GagTypes, "LayerThreeGagType", GagSpeak.Log)
        };
    }
 
    private Vector2 _iconSize;
    private float _comboLength;

    public void Prepare() {
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

    // draw list
    public void DrawGaglist(string slot, string currentArmor, out string replacedArmor,
                            GagPadlocks cPadlocktype, out string rPadlocktype, bool locked) {
        // push our styles
        using var    id = ImRaii.PushId(slot); // push the ID
        var     spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y }; // push spacing
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing); // push style

        // draw our icon thingy
        //currentArmor.DrawGagIcon(_textures, _iconSize, slot);
        // set our right and left
        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right); // if the item is clicked with the right mouse button
        var left = ImGui.IsItemClicked(ImGuiMouseButton.Left); // if the item is clicked with the left mouse button
        
        ImGui.SameLine();
        // create a group for the 2 dropdowns and icon
        using var group = ImRaii.Group();
        if (DrawGagItemCombo(slot, currentArmor, out replacedArmor, out var labelArmor, locked, right, left)) {
            GagSpeak.Log.Debug($"GagSpeak: {slot} changed to {labelArmor}");
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(labelArmor); // draw the label text

        if (DrawGagItemCombo(slot, cPadlocktype.ToString(), out rPadlocktype, out var labelPadlock, locked, right, left)) {
            GagSpeak.Log.Debug($"GagSpeak: {slot} changed to {rPadlocktype}");
        }  
    }

    // draw the gag item combo
    // clear = clear selection, open = open dropdown
    private bool DrawGagItemCombo(string slot, string currentArmor, out string replacedArmor, out string label,
                                 bool locked, bool clear, bool open) {
        // if slot = "LayerOneGagType", make layer =1, do this up to 3
        var layer = slot switch {
            "LayerOneGagType" => 0,
            "LayerTwoGagType" => 1,
            "LayerThreeGagType" => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
        };
        var combo = _itemCombo[layer]; // get the combo
        label = combo.Label; // set the label
        replacedArmor = currentArmor; // get our current selection
        // if our selection is not locked and we clicked on the dropdown, open it
        if (!locked && open) {
            UIHelpers.OpenCombo($"##{combo.Label}");
        }
        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        var change = combo.Draw(replacedArmor, _comboLength, _requiredComboWidth);

        // if we right click on it, clear the selection
        if (!locked && replacedArmor != null) {
            if (clear || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                change = true;
                replacedArmor  = _config.GagTypes.Keys.First();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return change;
    }



    // public static void DrawGagIcon(this EquipItem item, TextureService textures, Vector2 size, int layer)
    // {
    //     var isEmpty = item.ModelId.Id == 0;
    //     var (ptr, textureSize, empty) = textures.GetIcon(item, layer);
    //     if (empty)
    //     {
    //         var (bgColor, tint) = isEmpty
    //             ? (ImGui.GetColorU32(ImGuiCol.FrameBg), new Vector4(0.1f,       0.1f, 0.1f, 0.5f))
    //             : (ImGui.GetColorU32(ImGuiCol.FrameBgActive), new Vector4(0.3f, 0.3f, 0.3f, 0.8f));
    //         var pos = ImGui.GetCursorScreenPos();
    //         ImGui.GetWindowDrawList().AddRectFilled(pos, pos + size, bgColor, 5 * ImGuiHelpers.GlobalScale);
    //         if (ptr != nint.Zero)
    //             ImGui.Image(ptr, size, Vector2.Zero, Vector2.One, tint);
    //         else
    //             ImGui.Dummy(size);
    //     }
    //     else
    //     {
    //         ImGuiUtil.HoverIcon(ptr, textureSize, size);
    //     }
    // }
}