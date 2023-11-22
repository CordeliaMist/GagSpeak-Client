using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using Lumina.Misc;
using System.Text.RegularExpressions;
using System;

// Practicing Modular Design
namespace GagSpeak.UI.Helpers;

public static class UIHelpers // A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements
{
    public static bool DrawCheckbox(string label, string tooltip, bool value, out bool on, bool locked)
    {
        using var disabled = ImRaii.Disabled(locked);
        var       ret      = ImGuiUtil.Checkbox(label, string.Empty, value, v => value = v);
        ImGuiUtil.HoverTooltip(tooltip);
        on = value;
        return ret;
    }

    public static void OpenCombo(string comboLabel)
    {
        var windowId = ImGui.GetID(comboLabel);
        var popupId  = ~Crc32.Get("##ComboPopup", windowId);
        ImGui.OpenPopup(popupId); // was originally popup ID
    }

    // Helper function to clean senders name off the list of clientstate objects
    public static string CleanSenderName(string senderName) {
        string[] senderStrings = SplitCamelCase(RemoveSpecialSymbols(senderName)).Split(" ");
        string playerSender = senderStrings.Length == 1 ? senderStrings[0] : senderStrings.Length == 2 ?
            (senderStrings[0] + " " + senderStrings[1]) :
            (senderStrings[0] + " " + senderStrings[2]);
        return playerSender;
    }

    // Helper functions for parsing payloads and clientstruct information
    public static string SplitCamelCase(string input) {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
    public static string RemoveSpecialSymbols(string value) {
        Regex rgx = new Regex(@"[^a-zA-Z:/._\ -]");
        return rgx.Replace(value, "");
    }

    public static string FormatTimeSpan(TimeSpan timeSpan) {
        if (timeSpan <= TimeSpan.Zero) {
            return "0d0h0m0s";        
        }
        return $"{timeSpan.Days % 30}d{timeSpan.Hours}h{timeSpan.Minutes}m{timeSpan.Seconds}s";
    }

    public static DateTimeOffset GetEndTime(string input) {
        // Match days, hours, minutes, and seconds in the input string
        var match = Regex.Match(input, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");

        if (match.Success) { 
            // Parse days, hours, minutes, and seconds
            int.TryParse(match.Groups[1].Value, out int days);
            int.TryParse(match.Groups[2].Value, out int hours);
            int.TryParse(match.Groups[3].Value, out int minutes);
            int.TryParse(match.Groups[4].Value, out int seconds);
            // Create a TimeSpan from the parsed values
            TimeSpan duration = new TimeSpan(days, hours, minutes, seconds);
            // Add the duration to the current DateTime to get a DateTimeOffset
            return DateTimeOffset.Now.Add(duration);
        }

        // If the input string is not in the correct format, throw an exception
        throw new FormatException($"[MsgResultLogic]: Invalid duration format: {input}");
    }
}