using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Forms;
using PInvoke;
using Dalamud.Utility;
using GagSpeak.GSLogger;
using System.Numerics;
using ImGuiNET;

namespace GagSpeak.Utility;

/// <summary> A class for all of the UI helpers, including basic functions for drawing repetative yet unique design elements </summary>
public static class GenericHelpers
{
    /// <summary> A generic function to iterate through a collection and perform an action on each item </summary>
    public static void Each<T>(this IEnumerable<T> collection, Action<T> function) {
        foreach(var x in collection) {
            function(x);
        }
    }

    public static bool EqualsAny<T>(this T obj, params T[] values) {
        return values.Any(x => x!.Equals(obj));
    }

    // execute agressive inlining functions safely
    public static void Safe(Action action, bool suppressErrors = false) {
        try {
            action();
        } catch (Exception e) {
            // log errors if not surpressed
            if (!suppressErrors) {
                GSLogger.LogType.Debug($"{e.Message}\n{e.StackTrace ?? ""}");
            }
        }
    }

    // determines if getkeystate or getkeystateasync is called
    public static bool UseAsyncKeyCheck = false;

    // see if a key is pressed
    public static bool IsKeyPressed(Keys key)
    {
        // if it isnt any key just return false
        if (key == Keys.None) {
            return false;
        }
        // if we are using async key check, use getkeystateasync, otherwise use getkeystate
        if (UseAsyncKeyCheck) {
            return IsBitSet(User32.GetKeyState((int)key), 15);
        }
        else {
            return IsBitSet(User32.GetAsyncKeyState((int)key), 15);
        }
    }

    // see if the key bit is set
    public static bool IsBitSet(short b, int pos) => (b & (1 << pos)) != 0;


    public static bool Copy(string text, bool silent = false) {
        try {
            if (text.IsNullOrEmpty()) {
                Clipboard.Clear();
                if (!silent) Notify.Success("Clipboard cleared");
            }
            else {
                Clipboard.SetText(text);
                if (!silent) Notify.Success("Text copied to clipboard");
            }
            return true;
        }
        catch(Exception e) {
            if (!silent) {
                Notify.Error($"Error copying to clipboard:\n{e.Message}\nPlease try again");
            }
            GSLogger.LogType.Warning($"Error copying to clipboard:");
            // e.LogWarning();
            return false;
        }
    }

    public static void TextWrappedCopy(Vector4 col, string text) {
        TextWrapped(col, text);
        if (ImGui.IsItemHovered()) {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left)) {
            Copy(text);
        }
    }

    public static void TextWrapped(Vector4 col, string s) {
        ImGui.PushTextWrapPos(0);
        try{
            ImGui.PushStyleColor(ImGuiCol.Text, col);
            try {
                ImGui.TextUnformatted(s);
            }
            finally {
                ImGui.PopStyleColor();
            }
        } finally {
            ImGui.PopTextWrapPos();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string s, string other) => s.Equals(other, StringComparison.OrdinalIgnoreCase);
}