using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using ImGuiNET;

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

    public void Draw(float width, float height) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using (_ = ImRaii.Group()) {
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawPuppeteerHeader(width);
        DrawPuppeteerSelector(width, height);
        style.Pop();
        }
    }

    private void DrawPuppeteerSelector(float width, float height) {
        using var child = ImRaii.Child("##PuppeteerSelectorChild", new Vector2(width, height), true);
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
}