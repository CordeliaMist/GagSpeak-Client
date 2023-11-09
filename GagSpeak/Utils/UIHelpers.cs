using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using OtterGui;
using OtterGui.Raii;
using Lumina.Misc;


// Practicing Modular Design
namespace GagSpeak.UI.Helpers;

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


// using System;
// using System.Drawing;
// using System.Reflection;
// namespace SamplePlugin
// {
//     public class ImageLoader
//     {
//         public Image LoadEmbeddedImage()
//         {
//             string resourceName = "SamplePlugin.goat.png"; // Update the resource path with your project's namespace
//             // Load the image from embedded resources
//             using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
//             {
//                 if (stream == null)
//                 {
//                     throw new Exception("Resource not found in assembly.");
//                 }
//                 return Image.FromStream(stream);
//             }
//         }
//     }
// }


// ORIGINAL, STILL WORKING ONE:
    public static bool DrawCheckbox(string label, string tooltip, bool value, out bool on, bool locked)
    {
        using var disabled = ImRaii.Disabled(locked);
        var       ret      = ImGuiUtil.Checkbox(label, string.Empty, value, v => value = v);
        ImGuiUtil.HoverTooltip(tooltip);
        on = value;
        return ret;
    }
    // Out of function here!

    public static void OpenCombo(string comboLabel)
    {
        var windowId = ImGui.GetID(comboLabel);
        var popupId  = ~Crc32.Get("##ComboPopup", windowId);
        ImGui.OpenPopup(popupId); // was originally popup ID
    }
}