using System.Text.RegularExpressions;
using System;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;  // Contains classes for handling special encoded (SeString) payloads in the Dalamud game
using ImGuiNET;
using Lumina.Misc;
using OtterGui;
using OtterGui.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Colors;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;
using GagSpeak.Services;
using Dalamud.Interface.Utility;
using GagSpeak.Data;
using GagSpeak.UI.ComboListings;
using Penumbra.GameData.DataContainers;
using Lumina.Excel.GeneratedSheets;
using System.IO;

namespace GagSpeak.Utility;

/// <summary> A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements </summary>
public static class UIHelpers
{
    /// <summary> Frame Height for square icon buttons. </summary>
    public static Vector2 IconButtonSize = new Vector2(ImGui.GetFrameHeight());

    /// <summary> Push a text under a certain font to the UI 
    /// <list type="Bullet">
    /// <item><c>text</c><param name="text"> - The text to push</param></item>
    /// <item><c>font</c><param name="font"> - The font to push the text under</param></item>
    /// <item><c>color</c><param name="color"> - The color of the text (optional)</param></item>
    /// </list> </summary>
    public static void FontText(string text, ImFontPtr font, Vector4? color = null) {
        using var pushedFont = ImRaii.PushFont(font);
        // using var pushedColor = ImRaii.PushColor(ImGuiCol.Text, Color(color ?? new Vector4(1, 1, 1, 1)), color != null);
        ImGui.TextWrapped(text);
    }


    public static void DrawIcon(this EquipItem item, TextureService textures, Vector2 size, EquipSlot slot)
    {
        var isEmpty = item.PrimaryId.Id == 0;
        var (ptr, textureSize, empty) = textures.GetIcon(item, slot);
        if (empty)
        {
            var (bgColor, tint) = isEmpty
                ? (ImGui.GetColorU32(ImGuiCol.FrameBg), new Vector4(0.1f,       0.1f, 0.1f, 0.5f))
                : (ImGui.GetColorU32(ImGuiCol.FrameBgActive), new Vector4(0.3f, 0.3f, 0.3f, 0.8f));
            var pos = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddRectFilled(pos, pos + size, bgColor, 5 * ImGuiHelpers.GlobalScale);
            if (ptr != nint.Zero)
                ImGui.Image(ptr, size, Vector2.Zero, Vector2.One, tint);
            else
                ImGui.Dummy(size);
        }
        else
        {
            ImGuiUtil.HoverIcon(ptr, textureSize, size);
        }
    }


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
    /// This function draws a checkbox with a label and tooltip, alternative to the other helper function "DrawCheckbox"
    /// <list type="bullet">
    /// <item><c>label</c><param name="label"> - The label to display outside the checkbox</param></item>
    /// <item><c>tooltip</c><param name="tooltip"> - The tooltip to display when hovering over the checkbox</param></item>
    /// <item><c>current</c><param name="current"> - The current value of the checkbox</param></item>
    /// <item><c>setter</c><param name="setter"> - The setter for the checkbox</param></item>
    /// </list>
    /// </summary>
    public static void Checkbox(string label, string tooltip, bool current, Action<bool> setter, GagSpeakConfig _config) {
        using var id  = ImRaii.PushId(label);
        var       tmp = current;
        if (ImGui.Checkbox(string.Empty, ref tmp) && tmp != current) {
            setter(tmp);
            _config.Save();
        }
        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker(label, tooltip);
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

    /// <summary>
    /// This method is used to get the player payload.
    /// </summary>
    /// <returns>The player payload.</returns>
    public static void GetPlayerPayload(IClientState _clientState, out PlayerPayload playerPayload) { // gets the player payload
        if(_clientState.LocalPlayer != null) {
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        } else {
            GagSpeak.Log.Debug("[PayloadFetch]: Failed to get player payload, returning null");
            throw new Exception("Player is null!");
        }
    }


    /// <summary>
    /// This helper function is used to create a text field that when right clicked can be modified and changed.
    /// <list type="Bullet">
    /// <item><c>popupId</c><param name="popupId"> - The id of the popup</param></item>
    /// <item><c>text</c><param name="text"> - The text to display</param></item>
    /// <item><c>maxLength</c><param name="maxLength"> - The max length of the text</param></item>
    /// <item><c>helpText</c><param name="helpText"> - The help text to display</param></item>
    /// <item><c>tooltip</c><param name="tooltip"> - The tooltip to display</param></item>
    /// </list> </summary>
    public static void EditableTextFieldWithPopup(string popupId, ref string text, uint maxLength, string helpText, string tooltip) {
        ImGui.TextWrapped(text);
        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup(popupId); // Open the context menu
        // open the popup if we satisfy that criteria
        if (ImGui.BeginPopup(popupId)) {
            // store our text from when we open it
            string currentText = text;
            var oldText = currentText;
            // set keyboard focus to the text box
            if (ImGui.IsWindowAppearing()) { ImGui.SetKeyboardFocusHere(0); }
            // pompt the user to enter a new name
            ImGui.TextUnformatted(helpText);
            if (ImGui.InputText("##Rename", ref currentText, maxLength, ImGuiInputTextFlags.EnterReturnsTrue)) {
                // if our text is updated, send the updated text to the output result as an action string
                if (currentText != oldText)
                    text = currentText;
                // close the popup
                ImGui.CloseCurrentPopup();
            }
            ImGuiUtil.HoverTooltip(tooltip);
            ImGui.EndPopup();
        }
    }
}