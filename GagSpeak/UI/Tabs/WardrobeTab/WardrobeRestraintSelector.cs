using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using System.Linq;
using GagSpeak.Wardrobe;
using GagSpeak.Gagsandlocks;
using OtterGui;
using Dalamud.Interface;
using GagSpeak.Services;
using GagSpeak.Utility;

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class RestraintSetSelector
{
    private readonly    RestraintSetManager _restraintSetManager; // for getting the restraint sets
    private readonly    FontService         _fontService;         // for getting the font service
    private readonly    TimerService        _timerService;        // for getting the timer service
    private             string              _inputTimer = "";     // for getting the input timer
    private             Vector2             _defaultItemSpacing;

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public RestraintSetSelector(RestraintSetManager restraintSetManager, FontService fontService,
    TimerService timerService) {
        _restraintSetManager = restraintSetManager;
        _fontService = fontService;
        _timerService = timerService;

        _timerService.RemainingTimeChanged += OnRemainingTimeChanged;
    }

    public void Draw(float width, float height) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        DrawSelector(width, height);
        ImGui.SameLine();
        DrawRestraintSetOverview(height);
    }

    private void DrawSelector(float width, float height) {
        // group these
        using var group = ImRaii.Group();
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);

        DrawRestraintSetSelector(width, height);
        DrawSelectionButtons(width);
    }

#region  RestraintSetSelector
    private void DrawRestraintSetSelector(float width, float height) {
        using var child = ImRaii.Child("##Selector", new Vector2(width, height), true);
        if (!child)
            return;

        using var style     = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, _defaultItemSpacing);
        var       skips     = OtterGui.ImGuiClip.GetNecessarySkips(ImGui.GetTextLineHeight());
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(
                                    _restraintSetManager._restraintSets, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }

    private void DrawSelectionButtons(float width) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(width / 2, 0);

        if (ImGui.Button("Add Set", buttonWidth)) {
            _restraintSetManager.AddNewRestraintSet();
        }
        ImGui.SameLine();
        if (ImGui.Button("Remove Set", buttonWidth)) {
            // if the set only has one item, just replace it with a blank template
            if (_restraintSetManager._restraintSets.Count == 1) {
                _restraintSetManager._restraintSets[0] = new RestraintSet();
                _restraintSetManager._selectedIdx = 0;
            } else {
                _restraintSetManager.DeleteRestraintSet(_restraintSetManager._selectedIdx);
                _restraintSetManager._selectedIdx = 0;
            }
        }
    }

    private void DrawSelectable(RestraintSet restraintSet) {
        var equals = _restraintSetManager._selectedIdx == _restraintSetManager.GetRestraintSetIndex(restraintSet._name);
        if (ImGui.Selectable(restraintSet._name, equals) && !equals)
        {
            // update the selected index in our restraint set manager
            _restraintSetManager._selectedIdx = _restraintSetManager.GetRestraintSetIndex(restraintSet._name);
        }
    }
#endregion RestraintSetSelector
#region RestraintSetOverview
    private void DrawRestraintSetOverview(float height) {
        using var group = ImRaii.Group();
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);

        RestraintSetOverview(height);
        OverviewButtons();
    }

    private void RestraintSetOverview(float height) {
        using var child = ImRaii.Child("##SelectedSetOverview", new Vector2(0, height), true);
        if (!child) return;

        // restraint set name
        ImGui.PushFont(_fontService.UidFont);
        string newName = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._name;
        UIHelpers.EditableTextFieldWithPopup("RestraintSetName", ref newName, 26,
        "Rename your Restraint Set:", "Enter a new restraint set name here");
        if (newName != _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._name) {
            _restraintSetManager.ChangeRestraintSetName(_restraintSetManager._selectedIdx, newName);
        }
        ImGui.PopFont();

        // descirption
        string newDescription = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._description;
        UIHelpers.EditableTextFieldWithPopup("RestraintSetDescription", ref newDescription, 128,
        "Write out a description for the set:", "Sets a description field for the restraint set, purely cosmetic feature");
        if (newDescription != _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._description) {
            _restraintSetManager.ChangeRestraintSetDescription(_restraintSetManager._selectedIdx, newDescription);
        }
    }
    // For getting timer text updates
    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime) {
        // only display our restraints timer
        foreach(var restraintset in _restraintSetManager._restraintSets) {
            if(timerName == $"RestraintSet_{restraintset._name}") {
                _timerService.remainingTimes[timerName] = $"{remainingTime.Days}d, "+
                $"{remainingTime.Hours}h, {remainingTime.Minutes}m, {remainingTime.Seconds}s";
            }
        }
    }

    private void OverviewButtons() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X*.175f, 0);
        // draw out the options
        string lambdaText = _restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._enabled
                          ? "ACTIVE" : "INACTIVE";
        // the enabled toggle button
        if (ImGui.Button($"{lambdaText}", buttonWidth)) {
            _restraintSetManager.ChangeRestraintSetState(_restraintSetManager._selectedIdx, !_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._enabled);
        }
        // draw the input for the lock timer
        ImGui.SameLine();
        string timerName = $"RestraintSet_{_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._name}";
        string result;
        if (_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._locked) {
            if (_timerService.remainingTimes.ContainsKey(timerName)) {
                result = "Locked by "+$"{_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._wasLockedBy.Split(' ')[0]} for: "
                +_timerService.remainingTimes[timerName];
            } else {
                result = ""; // Display an empty string if the timer hasn't started yet
            }
        } else {
            result = _inputTimer;
        }
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X *.785f);
        if (ImGui.InputTextWithHint("##RestraintSetTimer", " Lock Time; Ex: 0h2m7s", ref result, 12, ImGuiInputTextFlags.None)) {
            _inputTimer = result;
        }
        // in the same line, place a button that enables the lock for the spesified time
        ImGui.SameLine();
        if (ImGui.Button("Self-Lock", new Vector2(ImGui.GetContentRegionAvail().X, 0))) {
            if(UIHelpers.ValidateTimer(_inputTimer)) {
                int currentIndex = _restraintSetManager._selectedIdx; // Capture the current index by value
                _restraintSetManager.ChangeRestraintSetNewLockEndTime(currentIndex, UIHelpers.GetEndTime(_inputTimer));
                _restraintSetManager.LockRestraintSet(currentIndex, "self");
                _timerService.StartTimer($"RestraintSet_{_restraintSetManager._restraintSets[currentIndex]._name}",
                    _inputTimer, 1000, () =>
                    {
                        _restraintSetManager.TryUnlockRestraintSet(currentIndex, "self"); // attempts to lock it
                        _timerService.ClearRestraintSetTimer();
                    });
            } else {
                _inputTimer = "ERROR: Invalid Timer";
            }
        }
    }
#endregion RestraintSetOverview
}