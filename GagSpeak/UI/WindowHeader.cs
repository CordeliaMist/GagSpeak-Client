using System.Numerics;
using System;
using System.Linq;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;

namespace GagSpeak.UI;

// This class draws the header of each of our tabs (not the tab list, but the line below it, listing the title)
public static class WindowHeader
{
    public struct Button {
        public static readonly Button Invisible = new() {
            Visible = false,
            Width   = 0,
        };

        public Action?         OnClick;
        public string          Description = string.Empty;
        public float           Width;
        public uint            BorderColor;
        public uint            TextColor;
        public FontAwesomeIcon Icon;
        public bool            Disabled;
        public bool            Visible;

        public Button() {
            Visible     = true;
            Width       = ImGui.GetFrameHeightWithSpacing();
            BorderColor = ColorId.HeaderButtons.Value();
            TextColor   = ColorId.HeaderButtons.Value();
            Disabled    = false;
        }

        /// <summary>
        /// This function draws the button.
        /// </summary>
        public readonly void Draw() {
            if (!Visible)
                return;
            using var color = ImRaii.PushColor(ImGuiCol.Border, BorderColor)
                .Push(ImGuiCol.Text, TextColor, TextColor != 0); //
            
            if (ImGuiUtil.DrawDisabledButton(Icon.ToIconString(), new Vector2(Width, ImGui.GetFrameHeight()), string.Empty, Disabled, true))
                OnClick?.Invoke();
            
            color.Pop();
            ImGuiUtil.HoverTooltip(Description);
        }
    }

    /// <summary>
    /// This function draws the header for the window. (the fancy label at the top of each tab's window you see "Whitelist Manager" ext.)
    /// <list type="bullet">
    /// <item><c>text</c><param name="text"> - The text to display in the header</param></item>
    /// <item><c>textColor</c><param name="textColor"> - The color of the text</param></item>
    /// <item><c>frameColor</c><param name="frameColor"> - The color of the frame</param></item>
    /// </list></summary>
    public static void Draw(string text, uint textColor, uint frameColor, int leftButtons, float baseWidth = 0f, params Button[] buttons)
    {
        // if no custom basewidth if given, just set it to the basewidthavailable
        if (baseWidth == 0f) {
            baseWidth = ImGui.GetContentRegionAvail().X;
        }

        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding,   0)
            .Push(ImGuiStyleVar.FrameBorderSize, ImGuiHelpers.GlobalScale);

        var leftButtonSize = 0f;
        foreach (var button in buttons.Take(leftButtons).Where(b => b.Visible))
        {
            button.Draw();
            ImGui.SameLine();
            leftButtonSize += button.Width;
        }

        var rightButtonSize = buttons.Length > leftButtons ? buttons.Skip(leftButtons).Where(b => b.Visible).Select(b => b.Width).Sum() : 0f;
        var midSize         = baseWidth - rightButtonSize - leftButtonSize;

        style.Pop();
        style.Push(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f + (rightButtonSize - leftButtonSize) / midSize, 0.5f));
        if (textColor != 0)
            ImGuiUtil.DrawTextButton(text, new Vector2(midSize, ImGui.GetFrameHeight()), frameColor, textColor);
        else
            ImGuiUtil.DrawTextButton(text, new Vector2(midSize, ImGui.GetFrameHeight()), frameColor);
        style.Pop();
        style.Push(ImGuiStyleVar.FrameBorderSize, ImGuiHelpers.GlobalScale);

        foreach (var button in buttons.Skip(leftButtons).Where(b => b.Visible))
        {
            ImGui.SameLine();
            button.Draw();
        }

        style.Pop();
    }
}
