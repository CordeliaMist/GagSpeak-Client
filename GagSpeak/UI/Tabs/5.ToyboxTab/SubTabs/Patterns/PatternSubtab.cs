using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using System;
using GagSpeak.Interop;
using Newtonsoft.Json;
using System.Linq;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class PatternSubtab
{
    private readonly    ToyboxPatternTable  _patternTable; // for getting the pattern table
    private readonly    PatternPlayback     _patternPlayback;
    private readonly    PatternHandler      _patternHandler;
    public PatternSubtab(DalamudPluginInterface pluginInterface, ToyboxPatternTable patternTable,
    PatternHandler patternCollection, PatternPlayback patternPlayback) {
        _patternTable = patternTable;
        _patternPlayback = patternPlayback;
        _patternHandler = patternCollection;
    }

    public void Draw() {
        // correct active index if it falls out of bounds
        if (_patternHandler._activePatternIndex >= _patternHandler._patterns.Count)
        {
            _patternHandler.SetActiveIdx(_patternHandler._patterns.Count > 0 ? _patternHandler._patterns.Count - 1 : -1);
            _patternHandler.Save();
        }

        // draw the subtab
        using var child = ImRaii.Child("##ToyboxPatternsChild", new Vector2(ImGui.GetContentRegionAvail().X, -ImGuiHelpers.GlobalScale*81), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // pop the zero spacing style var for everything inside
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw the import pattern button, spanning the width
        var width = ImGui.GetContentRegionAvail().X/2;
        if (ImGui.Button($"Import Pattern Data", new Vector2(width, ImGui.GetFrameHeight()))) {
            SetFromClipboard();
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("To add Patterns to the pattern table, you need to either:\n"+
            "1) Import pattern data shared online from your clipboard.\n"+
            "2) Record a pattern yourself from the workshop tab!\nIf you down own a toy and want to hear what you make, turn on the simulated vibe to hear the vibrations!");
        }
        ImGui.SameLine();

        // display the current running time from the patternplayback class
        if(_patternHandler._activePatternIndex != -1 && _patternHandler._patterns[_patternHandler._activePatternIndex]._isActive)
        {
            ImGuiUtil.Center($"Current Time: {_patternPlayback._recordingStopwatch.Elapsed.ToString(@"mm\:ss")}");
        }
        else if(_patternHandler._activePatternIndex == -1)
        {
            ImGuiUtil.Center("No Pattern Selected!");
        }
        else
        {
            ImGuiUtil.Center($"Total Duration: {_patternHandler._patterns[_patternHandler._activePatternIndex]._duration}");
        }

        // draw the pattern table
        _patternTable.Draw();
        ImGui.PopStyleVar();
    }

    private void SetFromClipboard() {
        try
        {
            // Get the JSON string from the clipboard
            string base64 = ImGui.GetClipboardText();
            // Deserialize the JSON string back to pattern data
            var bytes = Convert.FromBase64String(base64);
            // Decode the base64 string back to a regular string
            var version = bytes[0];
            version = bytes.DecompressToString(out var decompressed);
            // Deserialize the string back to pattern data
            PatternData pattern = JsonConvert.DeserializeObject<PatternData>(decompressed) ?? new PatternData();
            // Ensure the pattern has a unique name
            string baseName = pattern._name;
            int copyNumber = 1;
            while (_patternHandler._patterns.Any(set => set._name == pattern._name)) {
                pattern._name = baseName + $"(copy{copyNumber++})";
            }
            // Set the active pattern
            _patternHandler.AddNewPattern(pattern);
            GSLogger.LogType.Debug($"Set pattern data from clipboard");
        } catch (Exception ex) {
            GSLogger.LogType.Warning($"{ex.Message} Could not set pattern data from clipboard.");
        }
    }
}