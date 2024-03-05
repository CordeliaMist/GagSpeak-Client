using System;
using System.IO;
using System.Runtime.InteropServices;
using Dalamud.Plugin;
using ImGuiNET;

namespace GagSpeak.Services;
/// <summary> Manages any extra font for the UI. The only font currently is meant to be one that can display UTF8 for phonems. </summary>
public class FontService : IDisposable
{
    private             DalamudPluginInterface  _pluginInterface;                   // used to get the plugin interface
    public              ImFontPtr               UidFont { get; private set; }       // the font used for the UID
    public              bool                    UidFontBuilt { get; private set; }  // whether the font was built successfully
    
    public FontService(DalamudPluginInterface pluginInterface) {
        #pragma warning disable CS0618 // I dont really care about if it is absolete at the moment
        _pluginInterface = pluginInterface;
        _pluginInterface.UiBuilder.BuildFonts += BuildFont;  // subscribe to the build fonts event
        _pluginInterface.UiBuilder.RebuildFonts();           // rebuild the fonts
    }
    private unsafe void BuildFont() {
        var fontFile = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, "DoulosSIL-Regular.ttf");
        UidFontBuilt = false;
        // check if the font exists
        if (File.Exists(fontFile)) {
            try {
                /// <summary> The glyph ranges to load. Read bottom of file for the actual ranges (these were co-pilot generated and not entirely accurate) </summary>
                ushort[] glyphRanges = new ushort[]
                {
                    0x0020, 0x007E,  // Basic Latin
                    0x00A0, 0x00FF,  // Latin-1 Supplement
                    0x0100, 0x017F,  // Latin Extended-A
                    0x0180, 0x024F,  // Latin Extended-B
                    0x0250, 0x02AF,  // IPA Extensions
                    0x02B0, 0x02FF,  // Spacing Modifier Letters
                    0x0300, 0x036F,  // Combining Diacritical Marks
                    0x0370, 0x03FF,  // Greek and Coptic
                    0x0400, 0x04FF,  // Cyrillic
                    0x0500, 0x052F,  // Cyrillic Supplement
                    0x1AB0, 0x1AFF,  // Combining Diacritical Marks Extended
                    0x1D00, 0x1D7F,  // Phonetic Extensions
                    0x1D80, 0x1DBF,  // Phonetic Extensions Supplement
                    0x1DC0, 0x1DFF,  // Combining Diacritical Marks Supplement
                    0x1E00, 0x1EFF,  // Latin Extended Additional
                    0x2000, 0x206F,  // General Punctuation
                    0x2070, 0x209F,  // Superscripts and Subscripts
                    0x20A0, 0x20CF,  // Currency Symbols
                    0x20D0, 0x20FF,  // Combining Diacritical Marks for Symbols
                    0x2100, 0x214F,  // Letterlike Symbols
                    0x2150, 0x218F,  // Number Forms
                    0x2190, 0x21FF,  // Arrows
                    0x2200, 0x22FF,  // Mathematical Operators
                    0x2300, 0x23FF,  // Miscellaneous Technical
                    0x2400, 0x243F,  // Control Pictures
                    0x2440, 0x245F,  // Optical Character Recognition
                    0x2460, 0x24FF,  // Enclosed Alphanumerics
                    0x2500, 0x257F,  // Box Drawing
                    0x2580, 0x259F,  // Block Elements
                    0x25A0, 0x25FF,  // Geometric Shapes
                    0x2600, 0x26FF,  // Miscellaneous Symbols
                    0x2700, 0x27BF,  // Dingbats
                    0x27C0, 0x27EF,  // Miscellaneous Mathematical Symbols-A
                    0x27F0, 0x27FF,  // Supplemental Arrows-A
                };
                // pin the glyph ranges
                GCHandle handle = GCHandle.Alloc(glyphRanges, GCHandleType.Pinned);
                // attempt to load them in
                UidFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontFile, 30f, null, handle.AddrOfPinnedObject());
                UidFontBuilt = true;
                GSLogger.LogType.Debug($"[Font] Constructed. DoulosSIL-Regular.ttf");
            }
            catch (Exception ex) {
                GSLogger.LogType.Warning($"[Font] Failed to load Font :: {ex.Message}");
            }
        } else {
            GSLogger.LogType.Debug($"[Font] Error");
        }
    }
    /// <summary> Dispose of the font service </summary>
    public void Dispose() {
        GSLogger.LogType.Debug($"[Font] Disposing FontService");
        _pluginInterface.UiBuilder.BuildFonts -= BuildFont;
        #pragma warning restore CS0618 // I dont really care about if it is absolete at the moment
    }
}
/*  ======= Unicode block	Charis SIL support ==========
        C0 Controls and Basic Latin*	U+0020..U+007E
        C1 Controls and Latin-1 Supplement*	U+00A0..U+00FF
        Latin Extended-A	U+0100..U+0148, U+014A..U+017F
        Latin Extended-B*	U+0180..U+024F
        IPA Extensions*	U+0250..U+02AF
        Spacing Modifier Letters*	U+02B0..U+02FF
        Combining Diacritical Marks	U+0300..U+033F, U+0346..U+036F
        Greek and Coptic	U+0387, U+0393..U+0394, U+0398, U+039E, U+03A0, U+03A8..U+03A9, U+03B1..U+03B4, U+03B8, U+03BB..U+03BC, U+03C0..U+03C1, U+03C3, U+03C6..U+03C7, U+03C9, U+03D1, U+03F4
        Cyrillic	U+0400..U+045F, U+0462..U+0463, U+0472..U+0475, U+048A..U+04FF
        Cyrillic Supplement*	U+0500..U+052F
        Combining Diacritical Marks Extended	U+1AB0..U+1ABA. U+1ABF..U+1AC0, U+1AC6..U+1ACE
        Phonetic Extensions*	U+1D00..U+1D7F
        Phonetic Extensions Supplement*	U+1D80..U+1DBF
        Combining Diacritical Marks Supplement	U+1DC2, U+1DC4..U+1DCD, U+1DDA, U+1DDC, U+1DF5, U+1DFC..U+1DFF
        Latin Extended Additional*	U+1E00..U+1EFF
        General Punctuation	U+2000..U+2042, U+2044, U+204A, U+2053, U+2057, U+2060..U+2063, U+206A..U+206F
        Superscripts and Subscripts*	U+2070..U+2071, U+2074..U+208E, U+2090..U+209C
        Currency Symbols*	U+20A0..U+20C0
        Combining Diacritical Marks for Symbols	U+20E5, U+20EC..U+20EF
        Letterlike Symbols	U+210C, U+2113, U+2116..U+2117, U+211F, U+2122..U+2123, U+2126, U+212D, U+2135, U+214F
        Number Forms*	U+2150..U+218B
        Arrows	U+2190..U+219B, U+21A8, U+21B6..U+21B7, U+21Ba..U+21BB, U+21D0..U+21D5
        Mathematical Operators	U+2202..U+2206, U+220F, U+2211..U+2213, U+2215, U+2219..U+221A, U+221E, U+2221, U+2225..U+2228, U+222B, U+2234..U+2235, U+223C, U+2248, U+225F, U+2260..U+2262, U+2264..U+2265, U+226E..U+226F, U+U+2282..U+2287
        Miscellaneous Technical	U+2308..U+230B, U+2318, U+231C..U+231F, U+2329..U+232A, U+239B..U+23AD
        Control Pictures	U+2423
        Geometric Shapes	U+25C9, U+25CA..U+25CC
        Miscellaneous Symbols	U+2610..U+2612, U+2639..U+263A, U+2640, U+2642, U+266D, U+266F
        Dingbats	U+2713, U+2717, U+274D
        Misc. Math. Symbols-A	U+27C2, U+27E6..U+27E9
        Misc. Math. Symbols-B	U+2980
        Supplemental Math. Operators	U+2AFD
        Latin Extended-C*	U+2C60..U+2C7F
        Coptic	U+2C88
        Supplemental Punctuation	U+2E00..U+2E0D, U+2E13, U+2E14, U+2E17, U+2E22..U+2E25, U+2E3A..U+2E3B, U+2E3E
        Modifier Tone Letters*	U+A700..U+A71F
        Latin Extended-D	U+A720..U+A799, U+A7A0..U+A7AF, U+A7B0..U+A7BF, U+A7C4..U+A7CA, U+A7F2..U+A7F4, U+A7F7..U+A7FF
        Kayah Li	U+A92E
        Latin Extended-E	U+AB30, U+AB53, U+AB5C, U+AB5E, U+AB64..U+AB6B
        PUA: Specials	U+F130..U+F133
        PUA: Modifier letters (e.g. superscripts)	U+F1A1, U+F1A3..U+F1A4, U+F1AB, U+F1AE, U+F1B4..U+F1B5, U+F1BC, U+F1CD..U+F1CE, U+F1F1..U+F1F9
        PUA: Latin	U+F20D, U+F234..U+F235, U+F258..U+F259, U+F267..U+F269, U+F26C..U+F26D
        PUA: Cyrillic	U+F326..U+F327
        Alphabetic Presentation Forms	U+FB00..U+FB04
        Variation Selectors*	U+FE00..U+FE0F
        Combining Half Marks	U+FE20..U+FE23
        Arabic Presentation Forms-B	U+FEFF (zero-width no-break space)
        Specials*	U+FFF9..U+FFFD
        Latin Extended-F*	U+10780..U+10785, U+10787..U+107B0, U+107B2..U+107BA
        Mathematical Alphanumeric Symbols	U+1D40C, U+1D504..U+1D505, U+1D50A, U+1D50E..U+1D510, U+1D513..U+1D514, U+1D516..U+1D517, U+1D519
        Latin Extended-G*	U+1DF00..U+1DF1E, U+1DF25..U+1DF2A
        Cyrillic Extended-D	U+1E030..U+1E049, U+1E04B..U+1E06B, U+1E06D, U+1E06F
        Enclosed Alphanumeric Supplement	U+1F12F
        Emoticons	U+1F610
    ================ https://software.sil.org/doulos/charset/ ================
*/