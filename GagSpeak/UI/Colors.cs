using System.Collections.Generic;

namespace GagSpeak.UI;
/* General Purpose Note:
    - This file is primarily for visual styles. I aimed to adapt it in the use of my UI design, but quickly realized that
    outside of the header styles, I was too burnt out to apply using it everywhere else I have defined my colors.
    - As such, these primarily dictate the colors of the profile viewer and the header, but not much else at the moment sadly.
*/

// Enum to represent different color IDs
public enum ColorId {
    ProfileBG,
    ProfileHeaderBG,
    ProfileTableBG,
    SubmissiveButton,
    DominantButton,
    HeaderButtons,
    EnabledLock,
    DisabledLock,
    WhitelistBG,
    AddRemovePlayerButtons,
}

public static class Colors
{
    // Constant for selected red color
    public const uint SelectedRed = 0xFF2020D0;
    // Method to get the default color, name, and description for a given color ID
    public static (uint DefaultColor, string Name, string Description) Data(this ColorId color)
        => color switch
        {
            // @formatter:off
            ColorId.ProfileBG               => (0xFFFFFFFF, "Profile Background",               "The color for the background of the whole profile window."             ),
            ColorId.ProfileHeaderBG         => (0xFFC000C0, "Profile Viewer Header",            "The color for the header section of the profile window."               ),
            ColorId.ProfileTableBG          => (0xFF00C0C0, "Profile Viewer Table",             "The color for the table section of the profile viewer."                ),
            ColorId.SubmissiveButton        => (0xFF00C000, "Submissive Button",                "The color design of the submissive button in the general tab."         ),
            ColorId.DominantButton          => (0xFF18C018, "Dominant Button",                  "The color design of the dominant button in general tab."               ),
            ColorId.HeaderButtons           => (0xFFFFFFF0, "Header Buttons",                   "the color design of the buttons used in the header bars, if any"       ),
            ColorId.EnabledLock             => (0xFFFFF0C0, "Enabled Lock Buttons",             "The color that the buttons in the general tab will have when locked."  ),
            ColorId.DisabledLock            => (0xFFFFF0C0, "Disabled Lock Buttons",            "The color that the buttons in the general tab will have when unlocked."),
            ColorId.WhitelistBG             => (0xFFFFF0C0, "Whitelist Background",             "The color of the background that the whitelist viewer uses."           ),
            ColorId.AddRemovePlayerButtons  => (0xFFA0F0A0, "Add or Remove Player Buttons",     "the color that the add and remove player buttons will use."            ),
            _                               => (0x00000000, string.Empty,                         string.Empty                                                          ),
            // @formatter:on
        };

    private static IReadOnlyDictionary<ColorId, uint> _colors = new Dictionary<ColorId, uint>();

    /// <summary> Obtain the configured value for a color. </summary>
    public static uint Value(this ColorId color)
        => _colors.TryGetValue(color, out var value) ? value : color.Data().DefaultColor;

    /// <summary> Set the configurable colors dictionary to a value. </summary>
    public static void SetColors(GagSpeakConfig config)
        => _colors = config.Colors;
}
