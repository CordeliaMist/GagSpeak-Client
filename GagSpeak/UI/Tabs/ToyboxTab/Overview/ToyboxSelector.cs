using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using ImGuiNET;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class ToyboxSelector
{
    private readonly    CharacterHandler    _characterHandler;   // for getting the whitelist
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public ToyboxSelector(CharacterHandler characterHandler) {
        _characterHandler = characterHandler;
    }

    public void Draw(float width) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawToyboxSelector(width);
        style.Pop();
    }

    private void DrawToyboxSelector(float width) {
        using var child = ImRaii.Child("##ToyboxSelectorChild", new Vector2(width, -1), true);
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
}