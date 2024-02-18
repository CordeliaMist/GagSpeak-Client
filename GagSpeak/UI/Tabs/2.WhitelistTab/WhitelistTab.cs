using System;
using System.Collections.Generic;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using GagSpeak.Services;
using ImGuiNET;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.WhitelistTab;

/// <summary> This class is used to handle the _characterHandler.whitelistChars tab. </summary>
public class WhitelistTab : ITab, IDisposable
{
    private readonly    WhitelistSelector _selector;
    private readonly    WhitelistPlayerPermissions  _playerPermEditor;
    private readonly    CharacterHandler _characterHandler;
    private readonly    TimerService     _timerService;
    private readonly    InteractOrPermButtonEvent _buttonInteractionEvent;
    private             bool _enableInteractions = false;
    private             bool _viewMode = true;


    public WhitelistTab(WhitelistSelector selector, WhitelistPlayerPermissions playerPermissionEditor,
    CharacterHandler characterHandler, TimerService timerService, InteractOrPermButtonEvent buttonInteractionEvent) {
        _selector = selector;
        _playerPermEditor  = playerPermissionEditor;
        _characterHandler = characterHandler;
        _timerService = timerService;
        _buttonInteractionEvent = buttonInteractionEvent;
        // subscribe to our events
        _timerService.RemainingTimeChanged += OnRemainingTimeChanged;
        _buttonInteractionEvent.ButtonPressed += OnInteractOrPermButtonPressed;
    }

    // Dispose of the _characterHandler.whitelistChars tab
    public void Dispose() {
        // Unsubscribe from timer events
        _timerService.RemainingTimeChanged -= OnRemainingTimeChanged;
        _buttonInteractionEvent.ButtonPressed -= OnInteractOrPermButtonPressed;
    }

    public ReadOnlySpan<byte> Label
        => "Whitelist"u8;

    public void DrawContent()
    {
        // draw the selector for the set
        _selector.Draw(GetSetSelectorWidth(), ref _enableInteractions);
        ImGui.SameLine();
        // draw the editor for that set
        _playerPermEditor.Draw(SetEnableInteractions, SetViewMode, ref _enableInteractions, ref _viewMode);
        // remove the disabled state
    }

    public float GetSetSelectorWidth()
        => 160f * ImGuiHelpers.GlobalScale;


    public void SetEnableInteractions(bool value) {
        _enableInteractions = value;
    }

    public void SetViewMode(bool value) {
        _viewMode = value;
    }

    // automates the startCooldown process across all our classes.
    private void OnInteractOrPermButtonPressed(object sender, InteractOrPermButtonEventArgs e) {
        _enableInteractions = false;
        
        _timerService.StartTimer("InteractionCooldown", $"{e.Seconds}s", 100, () => { _enableInteractions = true; });
    }

    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime) {
        if(timerName == "InteractionCooldown") {
            _timerService.remainingTimes[timerName] = $"{remainingTime.TotalSeconds:F1}s";
            return;
        }
    }
}
