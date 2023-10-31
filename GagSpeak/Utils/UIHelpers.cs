using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using OtterGui;
using OtterGui.Raii;

using GagSpeak.Chat;
using GagSpeak.Services;
using GagSpeak.Events;

// Practicing Modular Design
namespace GagSpeak.UI;
public static class UIHelpers // A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements
{
    
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