using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using Dalamud.Utility;
using OtterGui;
using GagSpeak.Utility;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Utility.Raii;
using OtterGuiInternal.Enums;
using System.IO;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using GagSpeak.Interop;
using Newtonsoft.Json;
using System.Linq;


namespace GagSpeak.UI.Tabs.ToyboxTab;
public class PatternSubtab
{
    private readonly    CharacterHandler    _charHandler;
    private readonly    IClientState        _client;
    private readonly    ToyboxPatternTable  _patternTable; // for getting the pattern table
    private readonly    PatternPlayback     _patternPlayback;
    private readonly    PatternHandler      _patternCollection;
    private readonly    FontService         _fontService; // for getting the font
    private             int?                _tempSliderValue; // for storing the slider value
    public PatternSubtab(DalamudPluginInterface pluginInterface, CharacterHandler charHandler,
    ToyboxPatternTable patternTable, PatternHandler patternCollection, FontService fontService,
    PatternPlayback patternPlayback, IClientState client) {
        _charHandler = charHandler;
        _client = client;
        _patternTable = patternTable;
        _patternPlayback = patternPlayback;
        _patternCollection = patternCollection;
        _fontService = fontService;    
    }

    public void Draw() {
        using var child = ImRaii.Child("##ToyboxPatternsChild", new Vector2(ImGui.GetContentRegionAvail().X, -ImGuiHelpers.GlobalScale*81), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // pop the zero spacing style var for everything inside
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw the import pattern button, spanning the width
        var width = ImGui.GetContentRegionAvail().X;
        var text = "Import Pattern From Clipboard To Toybox Pattern List";
        if(_patternCollection._activePatternIndex != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) {
            width = width / 2;
            text = "Import Pattern Data";
        }
        if (ImGui.Button($"{text}", new Vector2(width, ImGui.GetFrameHeight()))) {
            SetFromClipboard();
        }
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("To add Patterns to the pattern table, you need to either:\n"+
            "1) Import pattern data shared online from your clipboard.\n"+
            "2) Record a pattern yourself from the workshop tab!\nIf you down own a toy and want to hear what you make, turn on the simulated vibe to hear the vibrations!");
        }
        // display the current running time from the patternplayback class
        if(_patternCollection._activePatternIndex != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) {
            ImGui.SameLine();
            ImGuiUtil.Center($"Current Time: {_patternPlayback._recordingStopwatch.Elapsed.ToString(@"mm\:ss")}");
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
            while (_patternCollection._patterns.Any(set => set._name == pattern._name)) {
                pattern._name = baseName + $"(copy{copyNumber++})";
            }
            // Set the active pattern
            _patternCollection.AddNewPattern(pattern);
            GagSpeak.Log.Debug($"Set pattern data from clipboard");
        } catch (Exception ex) {
            GagSpeak.Log.Warning($"{ex.Message} Could not set pattern data from clipboard.");
        }
    }
}