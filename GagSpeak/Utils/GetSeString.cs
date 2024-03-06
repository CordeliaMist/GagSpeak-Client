using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using System;
using System.Linq;

namespace GagSpeak.Utility;
public static class GS_GetSeString
{
    internal static unsafe SeString GetSeString(byte* textPtr)
        => GetSeString((IntPtr)textPtr);

    internal static SeString GetSeString(IntPtr textPtr)
    {
        if (textPtr == IntPtr.Zero) {
            GSLogger.LogType.Error("GetSeString: textPtr is null");
            return null;
        }

        try {
            return MemoryHelper.ReadSeStringNullTerminated(textPtr);
        }
        catch (Exception ex) {
            GSLogger.LogType.Error($"Error in GetSeString: {ex.Message}");
            return null;
        }
    }

    internal static unsafe string GetSeStringText(byte* textPtr)
        => GetSeStringText(GetSeString(textPtr));

    internal static string GetSeStringText(IntPtr textPtr)
        => GetSeStringText(GetSeString(textPtr));

    internal static string GetSeStringText(SeString seString)
    {
        var pieces = seString.Payloads.OfType<TextPayload>().Select(t => t.Text);
        var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
        return text;
    }
}
