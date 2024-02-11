﻿using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using Dalamud.Interface.Utility;
using System.Linq;

namespace GagSpeak.UI;
// probably can remove this later, atm it is literally just used for the debug window
public class SavePatternWindow : Window //, IDisposable
{
    private readonly PatternHandler _patternHandler;
    private readonly FontService _fontService;
    private readonly WorkshopMediator _workshopMediator;

    public SavePatternWindow(FontService fontService, PatternHandler patternHandler, 
    WorkshopMediator workshopMediator) : base(GetLabel()) {
        _fontService = fontService;
        _patternHandler = patternHandler;
        _workshopMediator = workshopMediator;
        Size = new Vector2(225, 130);
        // add flags that allow you to move, but not resize the window, also disable collapsible
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar;
    }

    public override void Draw() {
        if (ImGui.IsWindowAppearing()) { ImGui.SetKeyboardFocusHere(0); }
        ImGui.PushFont(_fontService.UidFont);
        ImGui.Text("Input Name for Pattern");
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputText("##patternName", ref _workshopMediator.patternName, 100);
        if (ImGui.Button("Save", new Vector2(ImGui.GetContentRegionAvail().X/2, -1))) {
            // submit new pattern
            _workshopMediator.tempNewPattern._name = _workshopMediator.patternName;
            _workshopMediator.tempNewPattern._duration = _workshopMediator.recordingStopwatch.Elapsed.ToString(@"mm\:ss");
            // when saving a pattern, cut off the first 50 values of the recorded positions
            _workshopMediator.tempNewPattern._patternData = _workshopMediator.storedRecordedPositions.Skip(25).ToList();;
            _patternHandler.AddNewPattern(_workshopMediator.tempNewPattern);
            // reset everything and close window
            _workshopMediator.tempNewPattern = new PatternData();
            _workshopMediator.finishedRecording = false;
            _workshopMediator.recordingStopwatch.Reset();  // Reset the stopwatch
            _workshopMediator.patternName = "";
            // close the window
            Toggle();
        }
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.Button("Discard", new Vector2(ImGui.GetContentRegionAvail().X, -1))) {
            _workshopMediator.finishedRecording = false;  
            _workshopMediator.recordingStopwatch.Reset();  // Reset the stopwatch
            _workshopMediator.patternName = "";
            Toggle();                      
        }
        ImGui.PopFont();
    }
    private static string GetLabel() => "GagSpeakSavePattern###GagSpeakSavePattern";
}
