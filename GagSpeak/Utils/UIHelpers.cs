using System.Text.RegularExpressions;
using System;
using System.Numerics;
using ImGuiNET;
using Lumina.Misc;
using OtterGui;
using OtterGui.Raii;
namespace GagSpeak.UI.Helpers;

/// <summary> A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements </summary>
public static class UIHelpers
{
    /// <summary> Frame Height for square icon buttons. </summary>
    public static Vector2 IconButtonSize = new Vector2(ImGui.GetFrameHeight());

    /// <summary> 
    /// A helper function for drawing checkboxes
    /// <list type="Bullet">
    /// <item><c>label</c><param name="label"> - The label for the checkbox</param></item>
    /// <item><c>tooltip</c><param name="tooltip"> - The tooltip for the checkbox</param></item>
    /// <item><c>value</c><param name="value"> - The value of the checkbox</param></item>
    /// <item><c>on</c><param name="on"> - The value of the checkbox</param></item>
    /// <item><c>locked</c><param name="locked"> - Whether or not the checkbox is locked</param></item>
    /// </list> </summary>
    /// <returns> The value of the checkbox </returns>
    public static bool DrawCheckbox(string label, string tooltip, bool value, out bool on, bool locked){
        using var disabled = ImRaii.Disabled(locked);
        var       ret      = ImGuiUtil.Checkbox(label, string.Empty, value, v => value = v);
        ImGuiUtil.HoverTooltip(tooltip);
        on = value;
        return ret;
    }

    /// <summary>
    /// function for opening the correct combo dropdown
    /// <list type="Bullet">
    /// <item><c>comboLabel</c><param name="comboLabel"> - The label for the combo dropdown</param></item>
    /// </list> </summary>
    public static void OpenCombo(string comboLabel) {
        var windowId = ImGui.GetID(comboLabel);
        var popupId  = ~Crc32.Get("##ComboPopup", windowId);
        ImGui.OpenPopup(popupId); // was originally popup ID
    }

    /// <summary>
    /// function for cleaning the senders name
    /// <list type="Bullet">
    /// <item><c>senderName</c><param name="senderName"> - The name of the sender</param></item>
    /// </list> </summary>
    /// <returns> The cleaned sender name </returns>
    public static string CleanSenderName(string senderName) {
        string[] senderStrings = SplitCamelCase(RemoveSpecialSymbols(senderName)).Split(" ");
        string playerSender = senderStrings.Length == 1 ? senderStrings[0] : senderStrings.Length == 2 ?
            (senderStrings[0] + " " + senderStrings[1]) :
            (senderStrings[0] + " " + senderStrings[2]);
        return playerSender;
    }

    /// <summary>
    /// function for splitting camel case
    /// <list type="Bullet">
    /// <item><c>input</c><param name="input"> - The input string</param></item>
    /// </list> </summary>
    /// <returns> The split camel case string </returns>
    public static string SplitCamelCase(string input) {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    /// <summary>
    /// function for removing special symbols
    /// <list type="Bullet">
    /// <item><c>value</c><param name="value"> - The input string</param></item>
    /// </list> </summary>
    /// <returns> The string with special symbols removed </returns>
    public static string RemoveSpecialSymbols(string value) {
        Regex rgx = new Regex(@"[^a-zA-Z:/._\ '-]");
        //      [^...] matches any character not in the brackets.
        //      a-z matches any lowercase letter.
        //      A-Z matches any uppercase letter.
        //      :/._\ '- matches a colon, slash, period, underscore, space, or hyphen, or apostrophe.
        return rgx.Replace(value, "");
    }

    /// <summary>
    /// function for formatting a timespan duration into a regex string format
    /// <list type="Bullet">
    /// <item><c>timeSpan</c><param name="timeSpan"> - The timespan to format</param></item>
    /// </list> </summary>
    /// <returns> The formatted timespan in a string format</returns>
    public static string FormatTimeSpan(TimeSpan timeSpan) {
        if (timeSpan <= TimeSpan.Zero) {
            return "0d0h0m0s";        
        }
        return $"{timeSpan.Days % 30}d{timeSpan.Hours}h{timeSpan.Minutes}m{timeSpan.Seconds}s";
    }

    /// <summary>
    /// function for getting the end time of a duration
    /// <list type="Bullet">
    /// <item><c>input</c><param name="input"> - The input string</param></item>
    /// </list> </summary>
    /// <returns> The DateTimeOffset of when the regex string input will end </returns>
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