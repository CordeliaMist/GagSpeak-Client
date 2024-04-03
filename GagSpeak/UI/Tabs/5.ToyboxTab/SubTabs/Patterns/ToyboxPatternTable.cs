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
        DrawPatternsTable();
    }

    // create a temp list of items to remove
    public List<int> itemsToRemove = new List<int>();
    // draw the pattern table
    private void DrawPatternsTable() {
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.2f, ImGui.GetStyle().CellPadding.Y)); // Modify the X padding
        try{
            using (var table = ImRaii.Table("UniquePatternListCreator", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, -1*ImGuiHelpers.GlobalScale))) {
                if (!table) { return; }
                // draw the header row
                ImGui.AlignTextToFramePadding();
                ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
                ImGui.TableSetupColumn("Pattern Info", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("##Loop", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
                ImGui.TableSetupColumn("Use", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Use").X);
                ImGui.TableSetupColumn(" Play", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Playn").X);
                ImGui.TableSetupColumn("Copy", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Copy").X);
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
            }
        } catch (System.Exception e) {
            GSLogger.LogType.Debug($"{e} Error drawing the pattern table");
        } finally {
            ImGui.PopStyleVar();
        }
    }

    private unsafe bool DrawAssociatedPatternRow(PatternData pattern, int idx) {
        bool shouldRemove = false;
        bool isActive = _patternHandler._activePatternIndex == idx;
        bool isAnyPlaying = _patternHandler.IsAnyPatternPlaying(out int playingIdx);
        // control the enabled / disabled sections by knowing if the row is currently active or not
        if(isAnyPlaying) { ImGui.BeginDisabled(); }
        try
        {
            // the delete pattern button
            ImGui.TableNextColumn();
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight(), ImGui.GetFrameHeightWithSpacing()+ImGui.GetFrameHeight()),
            "Delete this pattern from the list.\nHold SHIFT in order to delete.", !ImGui.GetIO().KeyShift, true))
            {
                shouldRemove = true;
            }
        
            // the pattern name field
            ImGui.TableNextColumn();
            string patternName = pattern._name;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.InputTextWithHint($"##patternName{idx}", "Input a name for your pattern here...", ref patternName, 64))
            {
                pattern.ChangePatternName(patternName); // Update the pattern name
                _patternHandler.Save();
            }    
        
            // next line, draw out description field
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            string patternDesc = pattern._description;
            if (ImGui.InputTextWithHint($"##patternDesc{idx}", "Set a description for your pattern here!", ref patternDesc, 300))
            {
                pattern.ChangePatternDescription(patternDesc); // Update the pattern desc
                _patternHandler.Save();
            }
        
            // pattern loop button
            ImGui.TableNextColumn();
            var iconstring2 = pattern._loop ? FontAwesomeIcon.Repeat : FontAwesomeIcon.None; 
            if(ImGuiUtil.DrawDisabledButton(iconstring2.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeightWithSpacing()+ImGui.GetFrameHeight()),
            "Set this pattern to loop.", false, true))
            {
                pattern.ChangePatternLoop(!pattern._loop); // Update the pattern loop
                _patternHandler.Save();
            }
            if(ImGui.IsItemHovered())
            {
                string text = pattern._loop ? "This pattern will loop." : "This pattern will not loop.";
                ImGui.SetTooltip($"{text}");
            }

            // draw the is active checkbox
            ImGui.TableNextColumn();
            // try disabled button for the checkbox instead
            var iconString1 = isActive ? FontAwesomeIcon.Check : FontAwesomeIcon.Times;
            if(ImGuiUtil.DrawDisabledButton(iconString1.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeightWithSpacing()+ImGui.GetFrameHeight()),
            "Set this pattern as the actively selected pattern.", false, true))
            {
                if(!isActive) {
                    _patternHandler.SetActiveIdx(idx); // Update the selected pattern
                } else {
                    _patternHandler.SetActiveIdx(-1); // Update the selected pattern
                }
                _patternHandler.Save();
            }
            if(ImGui.IsItemHovered())
            {
                ImGui.SetTooltip($"Must be checked to enable the pattern.\nSet this pattern as the actively selected pattern.");
            }
        } 
        catch (System.Exception e) {
            GSLogger.LogType.Debug($"{e.Message} Error drawing the pattern table");
        } 
        finally {
            if(isAnyPlaying) { ImGui.EndDisabled(); }
        }
        
        // draw the play / stop button
        ImGui.TableNextColumn();
        bool isPlaying = pattern._isActive;
        // if it isnt the active pattern change the buttons color to be somewhat faded, by getting the current stylevar color and then adjusting the vector4 to have lower opacity
        var iconString = !isPlaying ? FontAwesomeIcon.PlayCircle : FontAwesomeIcon.StopCircle;
        // allow us to stop this pattern if it is enabled
        if (ImGuiUtil.DrawDisabledButton(iconString.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeightWithSpacing()+ImGui.GetFrameHeight()),
        "Play this pattern", !isActive, true)) {
            // Update the active index
            if (!_patternHandler._patterns[_patternHandler._activePatternIndex]._isActive) {
                _patternHandler.ExecutePatternProper();
            } else {
                _patternHandler.StopPattern();
            }
            _patternHandler.Save();
        }

        // draw the copy button
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeightWithSpacing()+ImGui.GetFrameHeight()),
        "Copy this pattern", false, true)) {
            ExportToClipboard(pattern);
        }
        return shouldRemove;
    }

    private void ExportToClipboard(PatternData pattern)
    {
        try
        {
            // Serialize the active pattern data to a string
            string json = JsonConvert.SerializeObject(pattern);
            // Encode the string to a base64 string
            var compressed = json.Compress(6);
            string base64 = Convert.ToBase64String(compressed);
            // Copy the base64 string to the clipboard
            ImGui.SetClipboardText(base64);
            GSLogger.LogType.Debug($"Copied pattern data to clipboard");
        }
        catch (Exception ex)
        {
            GSLogger.LogType.Warning($"{ex.Message} Could not copy pattern data to clipboard.");
        }
    }
}