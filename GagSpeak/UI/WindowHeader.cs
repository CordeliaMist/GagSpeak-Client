using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;

// practicing modular design
namespace GagSpeak.UI.Tabs;

// This class draws the header of each of our tabs (not the tab list, but the line below it, listing the title)
public static class WindowHeader
{
    /// <summary>
    /// This function draws the header for the window.
    /// <list type="bullet">
    /// <item><c>text</c><param name="text"> - The text to display in the header</param></item>
    /// <item><c>textColor</c><param name="textColor"> - The color of the text</param></item>
    /// <item><c>frameColor</c><param name="frameColor"> - The color of the frame</param></item>
    /// </list></summary>
    public static void Draw(string text, uint textColor, uint frameColor)
    {
        // Push our style variables for the item spacing, frame rounding, and border size (May be redundant since the styls is popped)
        // Can experiment with this later.
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding,   0)
            .Push(ImGuiStyleVar.FrameBorderSize, ImGuiHelpers.GlobalScale);

        // Calculate our mid size
        var midSize = ImGui.GetContentRegionAvail().X - ImGuiHelpers.GlobalScale;

        style.Pop();
        // This line seems redundant too.
        style.Push(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
        
        // If our text has a color, draw it with the color, otherwise, draw it without.
        if (textColor != 0) {
            ImGuiUtil.DrawTextButton(text, new Vector2(midSize, ImGui.GetFrameHeight()), frameColor, textColor);
            GagSpeak.Log.Debug("Drawing Header");
        } else
            ImGuiUtil.DrawTextButton(text, new Vector2(midSize, ImGui.GetFrameHeight()), frameColor);
        
        // Pop that off our styles, then push the one for the borders
        style.Pop();
        style.Push(ImGuiStyleVar.FrameBorderSize, ImGuiHelpers.GlobalScale);
    }
}
