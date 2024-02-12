using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.CharacterData;
using System.Linq;
using GagSpeak.ToyboxandPuppeteer;
using Dalamud.Interface;
using OtterGui;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using GagSpeak.Interop;
using Dalamud.Interface.Utility;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public partial class ToyboxPatternTable {

    private readonly    CharacterHandler            _characterHandler;
    private             PatternHandler              _patternHandler;
    private             PatternData                 _tempNewPattern;
    public ToyboxPatternTable(CharacterHandler characterHandler, PatternHandler patternHandler) {
        _characterHandler = characterHandler;
        _patternHandler = patternHandler;

        _tempNewPattern = new PatternData();
    }

    public void Draw() {
        var _ = ImRaii.Group();
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw out the table
        DrawPatternHeader();
        DrawPatternsTable();
    }

    private void DrawPatternHeader() // Draw our header
        => WindowHeader.Draw("Stored Patterns", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 1, 0, ExportToClipboardButton(), SetFromClipboardButton());


    // create a temp list of items to remove
    public List<int> itemsToRemove = new List<int>();
    // draw the pattern table
    private void DrawPatternsTable() {
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.2f, ImGui.GetStyle().CellPadding.Y)); // Modify the X padding
        try{
            if(ImGui.BeginChild("PatternContent", new Vector2(0, -85*ImGuiHelpers.GlobalScale), false, ImGuiWindowFlags.NoScrollbar)) {
                using (var table = ImRaii.Table("UniquePatternListCreator", 3, ImGuiTableFlags.RowBg, new Vector2(0, -1*ImGuiHelpers.GlobalScale))) {
                    if (!table) { return; }
                    // draw the header row
                    ImGui.AlignTextToFramePadding();
                    ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
                    ImGui.TableSetupColumn("Pattern Name", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupColumn("Use##", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
                    ImGui.TableHeadersRow();
                    // Replace this with your actual data
                    foreach (var (pattern, idx) in _patternHandler._patterns.Select((value, index) => (value, index))) {
                        using var id = ImRaii.PushId(idx);
                        bool shouldRemove = DrawAssociatedPatternRow(pattern, idx);
                        if(shouldRemove) {
                            itemsToRemove.Add(idx);
                        }
                    }
                    // now remove any items before we draw our mod rows
                    foreach (var item in itemsToRemove) {
                        _patternHandler.RemovePattern(item);
                    }
                    // clear the items
                    itemsToRemove.Clear();
                    // draw the rows
                    DrawNewPatternRow();
                }
                ImGui.EndChild();
            }
        } catch (System.Exception e) {
            GagSpeak.Log.Debug($"{e} Error drawing the pattern table");
        } finally {
            ImGui.PopStyleVar();
        }
    }

    private bool DrawAssociatedPatternRow(PatternData pattern, int idx) {
        bool shouldRemove = false;
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
                "Delete this pattern", false, true))
            shouldRemove = true;

        ImGui.TableNextColumn();
        string patternName = pattern._name;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText($"##patternName{idx}", ref patternName, 50)) {
            pattern.ChangePatternName(patternName); // Update the pattern name
        }

        ImGui.TableNextColumn();
        bool isActive = _patternHandler._activePatternIndex == idx;
        if (ImGui.Checkbox($"##isActive{idx}", ref isActive)) {
            if(isActive) {
                _patternHandler.SetActiveIdx(idx); // Update the selected pattern
            } else {
                _patternHandler.SetActiveIdx(-1); // Update the selected pattern
            }
        }
        return shouldRemove;
    }

    private void DrawNewPatternRow() {
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetFrameHeight()), "", false, true)){
            _patternHandler.AddNewPattern(_tempNewPattern); // Use the helper function to add the new pattern
            _tempNewPattern = new PatternData();
        }
        ImGui.TableNextColumn();
        string newPatternName = _tempNewPattern._name;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##newPatternName", ref newPatternName, 50)){
            _tempNewPattern.ChangePatternName(newPatternName); // Update the new pattern name
        }
        ImGui.TableNextColumn();
    }


    private WindowHeader.Button SetFromClipboardButton()
        => new()
        {
            Description =
                "Paste a pattern from your clipboard.",
            Icon     = FontAwesomeIcon.Clipboard,
            OnClick  = SetFromClipboard,
            Visible  = true,
            Disabled = false,
        };

    private WindowHeader.Button ExportToClipboardButton()
        => new()
        {
            Description =
                "Store the selected pattern to your clipboard.",
            Icon    = FontAwesomeIcon.Copy,
            OnClick = ExportToClipboard,
            Visible  = true,
            Disabled = false,
        };

    private void ExportToClipboard()
    {
        try
        {
            // Check if there is an active pattern
            if (!_patternHandler.IsActivePatternInBounds()) {
                GagSpeak.Log.Warning("No active pattern to export.");
                return;
            }
            // Serialize the active pattern data to a string
            string json = JsonConvert.SerializeObject(_patternHandler._patterns[_patternHandler._activePatternIndex]);
            // Encode the string to a base64 string
            var compressed = json.Compress(6);
            string base64 = Convert.ToBase64String(compressed);
            // Copy the base64 string to the clipboard
            ImGui.SetClipboardText(base64);
            GagSpeak.Log.Debug($"Copied pattern data to clipboard");
        }
        catch (Exception ex)
        {
            GagSpeak.Log.Warning($"{ex.Message} Could not copy pattern data to clipboard.");
        }
    }

    private void SetFromClipboard()
    {
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
            GagSpeak.Log.Debug($"Set pattern data from clipboard");
        }
        catch (Exception ex)
        {
            GagSpeak.Log.Warning($"{ex.Message} Could not set pattern data from clipboard.");
        }
    }
}