using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class ToyboxSelector
{
    private readonly    PlugService         _plugService; // for getting the plug service
    private readonly    CharacterHandler    _charHandler;   // for getting the whitelist
    private readonly    PatternHandler      _patternHandler;     // for getting the patterns
    private readonly    ListCopier          _listCopier;         // for copying the pattern list
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public ToyboxSelector(CharacterHandler characterHandler, PatternHandler patternHandler,
    PlugService plugService) {
        _charHandler = characterHandler;
        _patternHandler = patternHandler;
        _plugService = plugService;
        _listCopier = new ListCopier(new List<string>());
        // should never occur but safeguarding
        if(_plugService == null) {
            throw new ArgumentNullException(nameof(plugService));
        }
    }

    public void Draw(float width) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using (_ = ImRaii.Group()) {
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawToyboxHeader(width);
        DrawToyboxSelector(width);
        DrawWhitelistButtons(width);
        // pop the style
        style.Pop();
        }
    }

    private void DrawToyboxHeader(float width) // Draw our header
        => WindowHeader.Draw("Whitelist", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, width, WindowHeader.Button.Invisible);

    private void DrawToyboxSelector(float width) {
        using var child = ImRaii.Child("##ToyboxSelectorChild", 
        new Vector2(width, -ImGui.GetFrameHeightWithSpacing()-ImGuiHelpers.GlobalScale), true);
        if (!child)
            return;

        using var style     = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, _defaultItemSpacing);
        var       skips     = OtterGui.ImGuiClip.GetNecessarySkips(ImGui.GetTextLineHeight());
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(_charHandler.whitelistChars, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }

    private void DrawSelectable(WhitelistedCharacterInfo characterInfo) {
        var equals = _charHandler.activeListIdx == _charHandler.GetWhitelistIndex(characterInfo._name);
        if (ImGui.Selectable(characterInfo._name, equals) && !equals)
        {
            // update the active list index
            _charHandler.activeListIdx = _charHandler.GetWhitelistIndex(characterInfo._name);
        }
    }


    // Draw the buttons for adding and removing players from the whitelist
    private void DrawWhitelistButtons(float width) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(width, 0) / 4;
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), buttonWidth,
        $"Copy Your Pattern List to the Clipboard", false, true)) {
            ImGui.OpenPopup("Copy Pattern List");
        }
        ImGui.SameLine();
        // Device State Button
        var deviceStateButton = _charHandler.playerChar._isToyActive ? FontAwesomeIcon.ToggleOn : FontAwesomeIcon.ToggleOff;
        // only make this togglable if the UI is not locked
        if(_charHandler.playerChar._lockToyboxUI) { ImGui.BeginDisabled(); }
        try
        {
            if (ImGuiUtil.DrawDisabledButton(deviceStateButton.ToIconString(), buttonWidth,
            _charHandler.playerChar._isToyActive ? "Toy is Active" : "Toy is Inactive", false, true))
            {
                _charHandler.ToggleToyState();
                // see what the new state is, and update the vibe accordingly
                if(_charHandler.playerChar._isToyActive) {
                    _ = _plugService.ToyboxVibrateAsync((byte)((_charHandler.playerChar._intensityLevel/(double)_plugService.stepCount)*100), 20);
                } else {
                    _ = _plugService.ToyboxVibrateAsync(0, 20);
                }
            }
        } finally {
            if(_charHandler.playerChar._lockToyboxUI) { ImGui.EndDisabled(); }
        }
        ImGui.SameLine();
        // the connect/disconnect button
        var isConnected = _plugService.IsClientConnected() ? FontAwesomeIcon.Link : FontAwesomeIcon.Unlink;
        if (ImGuiUtil.DrawDisabledButton(isConnected.ToIconString(), buttonWidth,
        _plugService.IsClientConnected() ? "Connected to Intiface" : "Disconnected from Intiface", false, true))
        {
            if(_plugService.IsClientConnected()) {
                _plugService.DisconnectAsync();
            } else {
                _plugService.ConnectToServerAsync();
            }
        }
        ImGui.SameLine();
        // the intiface button
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Globe.ToIconString(), buttonWidth,
        "Opens the Intiface website for download", false, true))
        {
            ImGui.OpenPopup("IntifacePopup");
        }

        // pop the style
        style.Pop();

        // update list copier details if opened
        // update the list copier if we need to
        if(_patternHandler._patterns.Count != _listCopier._items.Count) {
            _listCopier.UpdateListInfo(_patternHandler._patterns.Select(x => x._name).ToList());
        }
        _listCopier.DrawCopyButton("Copy Pattern List", "Copied pattern data to clipboard",
        "Could not copy pattern data to clipboard");

        // drawing the popup display
        // Set the size of the next window (the popup)
        ImGui.SetNextWindowSize(new Vector2(350, 100));
        if (ImGui.BeginPopup("IntifacePopup")) {
            ImGuiUtil.Center("Either click to watch a quick CK guide on the install & setup");
            ImGuiUtil.Center("Or go straight to the releases page for the download");
            if (ImGui.Button("Watch Quick Install Guide", new Vector2(-1, 0))) {
                Process.Start(new ProcessStartInfo {FileName = "https://www.youtube.com/watch?v=h3JBYiwtAXg&list=PLGzKipCtkx7EAyk1k5gRFG8ZyKB0FMTR3&index=6", UseShellExecute = true});
                ImGui.CloseCurrentPopup();
            }
            if (ImGui.Button("Go To Site Directly", new Vector2(-1, 0))) {
                Process.Start(new ProcessStartInfo {FileName = "https://github.com/intiface/intiface-central/releases/", UseShellExecute = true});
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }
}