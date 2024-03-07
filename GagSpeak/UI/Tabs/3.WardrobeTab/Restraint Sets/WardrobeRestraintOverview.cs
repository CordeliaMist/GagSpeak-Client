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
using System.Collections.Generic;
using Newtonsoft.Json;
using GagSpeak.Interop;
using Dalamud.Interface.Utility;
using GagSpeak.Events;
using GagSpeak.Interop.Penumbra;

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class RestraintSetOverview : IDisposable
{
    private readonly    RestraintSetManager _rsManager; // for getting the restraint sets
    private readonly    RS_ListChanged      _rsListChanged; // for getting the restraint set list changed event
    private readonly    ModAssociations     _modAssociations;     // for getting the mod associations
    private readonly    TimerService        _timerService;        // for getting the timer service
    private             string              _inputTimer = "";     // for getting the input timer
    private             Vector2             _defaultItemSpacing;

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public RestraintSetOverview(RestraintSetManager restraintSetManager, TimerService timerService,
    RS_ListChanged restraintSetListChanged, ModAssociations modAssociations) {
        _rsManager = restraintSetManager;
        _rsListChanged = restraintSetListChanged;
        _modAssociations = modAssociations;
        _timerService = timerService;

        _timerService.RemainingTimeChanged += OnRemainingTimeChanged;
    }

    public void Dispose() {
        _timerService.RemainingTimeChanged -= OnRemainingTimeChanged;
    }

#region RestraintSetOverview
    public void Draw() {
        using var group = ImRaii.Group();
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        RestraintOverview();
        OverviewButtons();
    }

    private void RestraintOverview() {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using var child = ImRaii.Child("##SelectedSetOverview", new Vector2(0, ImGui.GetContentRegionAvail().Y - 300*ImGuiHelpers.GlobalScale), true);
        if (!child) return;
        // draw out the associated mod list
        _modAssociations.Draw();
        ImGui.PopStyleVar();

    }
    // For getting timer text updates
    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime) {
        // only display our restraints timer
        foreach(var restraintset in _rsManager._restraintSets) {
            if(timerName == $"RestraintSet_{restraintset._name}") {
                _timerService.remainingTimes[timerName] = $"{remainingTime.Days}d, "+
                $"{remainingTime.Hours}h, {remainingTime.Minutes}m, {remainingTime.Seconds}s";
            }
        }
    }

    private void OverviewButtons() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        var lefthalfWidth = ImGui.GetContentRegionAvail().X*.4f;
        var righthalfWidth= ImGui.GetContentRegionAvail().X*.6f;
        // draw out the options
        string lambdaText = _rsManager._restraintSets[_rsManager._selectedIdx]._enabled ? $"Enabled by {_rsManager._restraintSets[_rsManager._selectedIdx]._wasEnabledBy.Split(' ')[0]}" : "Currently Disabled";
        // draw out set status
        ImGuiUtil.DrawDisabledButton($"{lambdaText}", new Vector2(lefthalfWidth - 2*ImGui.GetTextLineHeight(), 0), "Current State of the restraint set (Enabled/Disabled Status)", true, false);
        
        ImGui.SameLine();
        
        if(ImGuiUtil.DrawDisabledButton(_rsManager._restraintSets[_rsManager._selectedIdx]._enabled ? FontAwesomeIcon.ToggleOn.ToIconString() : FontAwesomeIcon.ToggleOff.ToIconString(),
            new Vector2(2*ImGui.GetTextLineHeight(),0), $"Toggle the State of this restraint set!", false, true))
        {
            _rsManager.ChangeRestraintSetState(_rsManager._selectedIdx, !_rsManager._restraintSets[_rsManager._selectedIdx]._enabled);
        }

        // draw the input for the lock timer
        ImGui.SameLine();
        string timerName = $"RestraintSet_{_rsManager._restraintSets[_rsManager._selectedIdx]._name}";
        string result;
        if (_rsManager._restraintSets[_rsManager._selectedIdx]._locked) {
            if (_timerService.remainingTimes.ContainsKey(timerName)) {
                result = "Locked by "+$"{_rsManager._restraintSets[_rsManager._selectedIdx]._wasLockedBy.Split(' ')[0]} for: "
                +_timerService.remainingTimes[timerName];
            } else {
                result = ""; // Display an empty string if the timer hasn't started yet
            }
        } else {
            result = _inputTimer;
        }
        ImGui.SetNextItemWidth(righthalfWidth - 2*ImGui.GetTextLineHeight());
        if (ImGui.InputTextWithHint("##RestraintSetTimer", " Lock Time; Ex: 0h2m7s", ref result, 12, ImGuiInputTextFlags.None)) {
            _inputTimer = result;
        }
        // in the same line, place a button that enables the lock for the spesified time
        ImGui.SameLine();
        var helperText = _rsManager._restraintSets[_rsManager._selectedIdx]._locked
            ? "Current Set is unlocked, Enable set and click this to lock it after inserting a valid time"
            : "Current Set is locked, click this to attempt unlocking it. (Will not worked if locked by another player)";
        if(ImGuiUtil.DrawDisabledButton(_rsManager._restraintSets[_rsManager._selectedIdx]._locked ? FontAwesomeIcon.Lock.ToIconString() : FontAwesomeIcon.LockOpen.ToIconString(),
            new Vector2(2*ImGui.GetTextLineHeight(),0), $"{helperText}", false, true))
        {
            if(UIHelpers.ValidateTimer(_inputTimer)) {
                int currentIndex = _rsManager._selectedIdx; // Capture the current index by value
                _rsManager.ChangeRestraintSetNewLockEndTime(currentIndex, UIHelpers.GetEndTime(_inputTimer));
                _rsManager.LockRestraintSet(currentIndex, "self");
                _timerService.StartTimer($"RestraintSet_{_rsManager._restraintSets[currentIndex]._name}",
                    _inputTimer, 1000, () =>
                    {
                        _rsManager.TryUnlockRestraintSet(currentIndex, "self"); // attempts to lock it
                        _timerService.ClearRestraintSetTimer();
                    });
            } else {
                _inputTimer = "ERROR: Invalid Timer";
            }
        }
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Lock your own restraint set for the time specified in the input field.\n"+
            "You can unlock yourself at any time IF you did it.");
        };
    }
#endregion RestraintSetOverview
}
