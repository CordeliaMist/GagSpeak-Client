using System.Collections.Generic;
using System.Numerics;

namespace GagSpeak.UI;

// Enum to represent different color IDs
public enum ColorId {
    LushPinkLine,
    LushPinkButton,
    LovenseScrollingBG,
    LovenseDragButtonBG,
    LovenseDragButtonBGAlt,
    ButtonDrag,
    SideButton,
    SideButtonBG,
    WhiteMostlyOpaque,

    // button,
    // header buttons
    // donation buttons
    // window BG
    // header BG
    // list BG
    // line color

}

public static class Colors
{
    // Constant for selected red color
    public const uint SelectedRed = 0xFF2020D0;

    // Method to get the default color, name, and description for a given color ID
    public static (Vector4 DefaultColor, string Name, string Description) Data(this ColorId color)
        => color switch
        {
            ColorId.LushPinkLine            => (new Vector4(.806f, .102f, .407f, 1),        "Lush Pink Line",               "Description for Lush Pink Line"),
            ColorId.LushPinkButton          => (new Vector4(1, .051f, .462f, 1),            "Lush Pink Button",             "Description for Lush Pink Button"),
            ColorId.LovenseScrollingBG      => (new Vector4(0.042f, 0.042f, 0.042f, 0.930f),"Lovense Scrolling BG",         "Description for Lovense Scrolling BG"),
            ColorId.LovenseDragButtonBG     => (new Vector4(0.110f, 0.110f, 0.110f, 0.930f),"Lovense Drag Button BG",       "Description for Lovense Drag Button BG"),
            ColorId.LovenseDragButtonBGAlt  => (new Vector4(0.1f, 0.1f, 0.1f, 0.930f),      "Lovense Drag Button BG Alt",   "Description for Lovense Drag Button BG Alt"),
            ColorId.ButtonDrag              => (new Vector4(0.097f, 0.097f, 0.097f, 0.930f),"Button Drag",                  "Description for Button Drag"),
            ColorId.SideButton              => (new Vector4(0.451f, 0.451f, 0.451f, 1),     "Side Button",                  "Description for Side Button"),
            ColorId.SideButtonBG            => (new Vector4(0.451f, 0.451f, 0.451f, .25f),  "Side Button BG",               "Description for Side Button BG"),
            ColorId.WhiteMostlyOpaque       => (new Vector4(1f, 1f, 1f, .941f),             "Normal header button default", "Description for Light Grey Button"),
            _                               => (new Vector4(0, 0, 0, 0), string.Empty, string.Empty),
        };

    private static IReadOnlyDictionary<ColorId, Vector4> _colors = new Dictionary<ColorId, Vector4>();

    /// <summary> Obtain the configured value for a color. </summary>
    public static Vector4 Value(this ColorId color)
        => _colors.TryGetValue(color, out var value) ? value : color.Data().DefaultColor;

    /// <summary> Set the configurable colors dictionary to a value. </summary>
    public static void SetColors(Dictionary<ColorId, Vector4> config)
        => _colors = config;
}