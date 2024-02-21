using System;
using System.Collections.Generic;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using GagSpeak.Services;
using ImGuiNET;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.WhitelistTab;

public enum WhitelistPanelTab {
    Overview,
    TheirSettings,
    YourSettings,
}
/// <summary> This class is used to handle the _characterHandler.whitelistChars tab. </summary>
public class WhitelistTab : ITab, IDisposable
{
    private readonly    WhitelistSelector           _selector;
    private readonly    WhitelistPanel              _panel;
    private readonly    CharacterHandler            _characterHandler;
    private readonly    TimerService                _timerService;
    private readonly    InteractOrPermButtonEvent   _buttonInteractionEvent;
    private bool                                    _interactions;

    public WhitelistTab(WhitelistSelector selector, WhitelistPanel panel,
    CharacterHandler characterHandler, TimerService timerService, InteractOrPermButtonEvent buttonInteractionEvent) {
        _selector = selector;
        _panel = panel;
        _characterHandler = characterHandler;
        _timerService = timerService;
        _buttonInteractionEvent = buttonInteractionEvent;
        // set the helpers to defaults
        _interactions = false;
        // subscribe to our events
        _buttonInteractionEvent.ButtonPressed += OnInteractOrPermButtonPressed;
    }

    // Dispose of the _characterHandler.whitelistChars tab
    public void Dispose() {
        // Unsubscribe from timer events
        _buttonInteractionEvent.ButtonPressed -= OnInteractOrPermButtonPressed;
    }

    public ReadOnlySpan<byte> Label
        => "Whitelist"u8;

    public void DrawContent()
    {
        // draw the selector for the set
        _selector.Draw(GetSetSelectorWidth(), SetEnableInteractions, ref _interactions);
        ImGui.SameLine();
        // draw the editor for that set
        _panel.Draw(ref _interactions);
        // remove the disabled state
    }

    public float GetSetSelectorWidth()
        => 140f * ImGuiHelpers.GlobalScale;


    public void SetEnableInteractions(bool value) {
        _interactions = value;
    }

    // automates the startCooldown process across all our classes.
    private void OnInteractOrPermButtonPressed(object sender, InteractOrPermButtonEventArgs e) {
        _interactions = false;
        
        _timerService.StartTimer("InteractionCooldown", $"{e.Seconds}s", 100, () => { _interactions = true; });
    }
}
