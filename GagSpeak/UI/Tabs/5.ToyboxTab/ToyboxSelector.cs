using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Interop;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Utility;
using ImGuiNET;
using Newtonsoft.Json;
using OtterGui;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class ToyboxSelector
{
    private readonly    CharacterHandler    _characterHandler;   // for getting the whitelist
    private readonly    PatternHandler      _patternHandler;     // for getting the patterns
    private readonly    ListCopier          _listCopier;         // for copying the pattern list
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public ToyboxSelector(CharacterHandler characterHandler, PatternHandler patternHandler) {
        _characterHandler = characterHandler;
        _patternHandler = patternHandler;
        _listCopier = new ListCopier(new List<string>());
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
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(
                                    _characterHandler.whitelistChars, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }

    private void DrawSelectable(WhitelistedCharacterInfo characterInfo) {
        var equals = _characterHandler.activeListIdx == _characterHandler.GetWhitelistIndex(characterInfo._name);
        if (ImGui.Selectable(characterInfo._name, equals) && !equals)
        {
            // update the active list index
            _characterHandler.activeListIdx = _characterHandler.GetWhitelistIndex(characterInfo._name);
        }
    }


    // Draw the buttons for adding and removing players from the whitelist
    private void DrawWhitelistButtons(float width) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(width, 0);
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), buttonWidth,
        $"Copy Your Pattern List to the Clipboard", false, true)) {
            ImGui.OpenPopup("Copy Pattern List");
        }
        style.Pop();

    _listCopier.UpdateListInfo(_patternHandler._patterns.Select(x => x._name).ToList());
    _listCopier.DrawCopyButton("Copy Pattern List", "Copied pattern data to clipboard",
    "Could not copy pattern data to clipboard");
    }
}