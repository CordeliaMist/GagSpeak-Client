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


namespace GagSpeak.UI.Tabs.ToyboxTab;
public class PatternSubtab
{
    private readonly    CharacterHandler    _charHandler;
    private readonly    ToyboxPatternTable  _patternTable; // for getting the pattern table
    private readonly    PatternPlayback     _patternPlayback;
    private readonly    PatternHandler      _patternCollection;
    private readonly    FontService         _fontService; // for getting the font
    private             int?                _tempSliderValue; // for storing the slider value
    public PatternSubtab(DalamudPluginInterface pluginInterface, CharacterHandler charHandler,
    ToyboxPatternTable patternTable, PatternHandler patternCollection, FontService fontService,
    PatternPlayback patternPlayback) {
        _charHandler = charHandler;
        _patternTable = patternTable;
        _patternPlayback = patternPlayback;
        _patternCollection = patternCollection;
        _fontService = fontService;    
    }

    public void Draw() {
        // draw out the table
        ImGui.BeginChild("PatternsTableChild", new Vector2(ImGui.GetContentRegionAvail().X, -1));
        using (var table = ImRaii.Table($"PatternsTable", 2, ImGuiTableFlags.None)) {
            if (!table) { return; }
            // Create the headers for the table
            ImGui.TableSetupColumn("PatternInformation", ImGuiTableColumnFlags.WidthFixed, 200*ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("PatternTable", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();
            var height = ImGui.GetContentRegionAvail().Y;
            DrawPatternInfo(height);
            ImGui.TableNextColumn();
            // draw out the information about the pattern
            _patternTable.Draw();
        }
        ImGui.EndChild();
    }

    private void DrawPatternInfo(float height) {
        // draw info of selected Pattern, if one is selected
        if (_patternCollection._activePatternIndex >=0) {
            ImGui.PushFont(_fontService.UidFont);
            string newPatternName = _patternCollection._patterns[_patternCollection._activePatternIndex]._name;
            UIHelpers.EditableTextFieldWithPopup("RestraintSetName", ref newPatternName, 20,
            "Rename your Pattern:", "Enter a new name for the Pattern here");
            if (newPatternName != _patternCollection._patterns[_patternCollection._activePatternIndex]._name) {
                _patternCollection.RenamePattern(_patternCollection._activePatternIndex, newPatternName);
            }
            ImGui.PopFont();
            string newPatternDesc =_patternCollection._patterns[_patternCollection._activePatternIndex]._description;
            UIHelpers.EditableTextFieldWithPopup("RestraintSetDesc", ref newPatternDesc, 200,
            "Modify your Description:", "Modify your pattern description here");
            if (newPatternDesc != _patternCollection._patterns[_patternCollection._activePatternIndex]._description) {
                _patternCollection.ModifyDescription(_patternCollection._activePatternIndex, newPatternDesc);
            }
            ImGui.Text($"Pattern Duration: {_patternCollection._patterns[_patternCollection._activePatternIndex]._duration}");
            bool loop = _patternCollection._patterns[_patternCollection._activePatternIndex]._loop;
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Loop Pattern: ");
            ImGui.SameLine();
            if (ImGui.Checkbox($"##LoopCheckFor{_patternCollection._patterns[_patternCollection._activePatternIndex]._name}", ref loop)) {
                _patternCollection._patterns[_patternCollection._activePatternIndex]._loop = loop;
            }
            ImGui.NewLine();
            // now draw out the button for starting/stopping a pattern, and a checkbox for if we should loop it or not
            ImGui.SetCursorPosY(height - ImGui.GetFrameHeightWithSpacing());
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 25*ImGuiHelpers.GlobalScale);
            var width = 150*ImGuiHelpers.GlobalScale;
            var buttonText = _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive ? "Stop Pattern Playback" : "Start Pattern Playback";
            if (ImGui.Button(buttonText, new Vector2(width, ImGui.GetFrameHeight()))) {
                if (!_patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) {
                    _patternCollection.ExecutePatternProper();
                } else {
                    _patternCollection.StopPattern();
                }
            }
            ImGui.SetCursorPosY(height - ImGui.GetFrameHeightWithSpacing()*2.25f);

            ImGui.PushFont(_fontService.UidFont);
            // display the current running time from the patternplayback class
            if(_patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) {
                ImGuiUtil.Center($"Current Time: {_patternPlayback._recordingStopwatch.Elapsed.ToString(@"mm\:ss")}");
            }
            ImGui.PopFont();
        }
    }
}