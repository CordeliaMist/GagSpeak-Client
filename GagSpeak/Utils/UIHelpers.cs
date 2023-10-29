using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using OtterGui;
using OtterGui.Raii;
// Practicing Modular Design
namespace GagSpeak.UI;

// Future Note: This class should be static, but currently unsure how to draw these without including instance variables.

// A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements
public class UIHelpers
{
    private readonly GagSpeakConfig _config;
    
    /* CoPilot, use this summary format for ALL function comments in this workspace*/
    /// <summary>
    /// This function draws ImGui's Combo list, but with a search filter.
    /// <list type="bullet">
    /// <item><c>contentList</c><param name="contentList"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>selectedTypes</c><param name="selectedTypes"> - a list where the stored selection from the list is saved</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - the index of the selectedTypes list to saved the selected option to</param></item>
    /// </list>
    /// </summary>
    public void DrawFilterCombo(Dictionary<string, int> contentList, string label, List<string> selectedTypes, int layerIndex) {
        // create an empty string for the search text
        var ComboSearchText = string.Empty;
        // Create the combo
        using( var gagTypeOneCombo = ImRaii.Combo( label, selectedTypes[layerIndex], ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) {
            // Assign it an ID if combo is sucessful.
            if( gagTypeOneCombo ) {
                using var id = ImRaii.PushId( label ); // Push an ID for the combo box (based on label / name)
                ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X); // Set filter length to full
                
                // Draw filter bar
                if( ImGui.InputTextWithHint("##filter", "Filter...", ref ComboSearchText, 255 ) ) {
                    // Query: if the search bar is empty, display all the gag types, otherwise, display only search matches
                    contentList = string.IsNullOrEmpty(ComboSearchText) ? (
                        _config.GagTypes
                    ) : (
                        _config.GagTypes.Where(x=>x.Key.ToLower().Contains(ComboSearchText.ToLower())).ToDictionary(x=>x.Key, x=>x.Value)
                    );
                }
                
                // Now that we have our results, be sure to draw them! (does this so filter list remains visible)
                using var child = ImRaii.Child( "Child", new Vector2( ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X, 200),true);
                // We will draw out one listing for each item.
                foreach( var item in contentList.Keys ) {
                    // If our item is selected, set it and break
                    if( ImGui.Selectable( item, item == selectedTypes[layerIndex] ) ) {
                        selectedTypes[layerIndex] = item;
                        //SaveConfig(); // also update the config so we can see. (should either remove this or add something to update it.)
                        break;
                    }
                }
            }
        }
        // Finished with the combo, now we can close out of the function.
    }

    // public static void DrawIcon(this EquipItem item, TextureService textures, Vector2 size, EquipSlot slot)
    // {
    //     var isEmpty = item.ModelId.Id == 0;
    //     var (ptr, textureSize, empty) = textures.GetIcon(item, slot);
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

    public static bool DrawCheckbox(string label, string tooltip, bool value, out bool on, bool locked)
    {
        using var disabled = ImRaii.Disabled(locked);
        var       ret      = ImGuiUtil.Checkbox(label, string.Empty, value, v => value = v);
        ImGuiUtil.HoverTooltip(tooltip);
        on = value;
        return ret;
    }
    // Out of function here!
}