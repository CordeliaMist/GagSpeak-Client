using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Interop;
using ImGuiNET;
using Newtonsoft.Json;
using OtterGui;

namespace GagSpeak.UI.Tabs.PuppeteerTab;
public class PuppeteerSelector
{
    private readonly    CharacterHandler    _characterHandler;   // for getting the whitelist
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public PuppeteerSelector(CharacterHandler characterHandler) {
        _characterHandler = characterHandler;
    }

    private void DrawPuppeteerHeader(float width) // Draw our header
        => WindowHeader.Draw("Whitelist", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, width, WindowHeader.Button.Invisible);

    public void Draw(float width) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using (_ = ImRaii.Group()) {
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawPuppeteerHeader(width);
        DrawPuppeteerSelector(width);
        DrawPuppeteerButtons(width);
        style.Pop();
        }
    }

    private void DrawPuppeteerSelector(float width) {
        using var child = ImRaii.Child("##PuppeteerSelectorChild", 
        new Vector2(width, -(ImGui.GetTextLineHeightWithSpacing()+5*ImGuiHelpers.GlobalScale)), true);
        if (!child) { return; }

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
    private void DrawPuppeteerButtons(float width) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(width, ImGui.GetFrameHeight());
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), buttonWidth,
        $"Copy alias list for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]} to the clipboard", false, true)) {
            CopyAliasDataToClipboard();
        }
        style.Pop();
    }


    private void CopyAliasDataToClipboard() {
        try
        {
            // Check if there is an active pattern
            if (_characterHandler.playerChar._triggerAliases[_characterHandler.activeListIdx]._aliasTriggers.Count() == 0) {
                GSLogger.LogType.Warning("No Aliases to copy.");
                return;
            }
            // create a dictionary<string,string> where the key stores the input, and the value stores the output
            var aliasData = new Dictionary<string, string>();
            // add the alias data to the dictionary
            foreach (var alias in _characterHandler.playerChar._triggerAliases[_characterHandler.activeListIdx]._aliasTriggers) {
                aliasData.Add(alias._inputCommand, alias._outputCommand);
            }
            // Serialize the alias data to a string
            string json = JsonConvert.SerializeObject(aliasData);
            // Encode the string to a base64 string
            var compressed = json.Compress(6);
            string base64 = Convert.ToBase64String(compressed);
            // Copy the base64 string to the clipboard
            ImGui.SetClipboardText(base64);
            GSLogger.LogType.Debug($"Copied aliases to clipboard");
        }
        catch (Exception ex) {
            GSLogger.LogType.Warning($"{ex.Message} Could not copy alias data to clipboard.");
        }
    }
}