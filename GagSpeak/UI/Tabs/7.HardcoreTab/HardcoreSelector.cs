using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore.Movement;
using ImGuiNET;
using OtterGui;
using GagSpeak.Utility;

namespace GagSpeak.UI.Tabs.HardcoreTab;
public class HardcoreSelector
{
    private readonly    CharacterHandler    _charHandler;   // for getting the whitelist
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public HardcoreSelector(CharacterHandler characterHandler) {
        _charHandler = characterHandler;
    }

    public void Draw(float width) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using (_ = ImRaii.Group()) {
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawHardcoreHeader(width);
        DrawHardcoreSelector(width);
        DrawPuppeteerButtons(width);
        // pop the style
        style.Pop();
        }
    }

    private void DrawHardcoreHeader(float width) // Draw our header
        => WindowHeader.Draw("Whitelist", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, width, WindowHeader.Button.Invisible);

    private void DrawHardcoreSelector(float width) {
        using var child = ImRaii.Child("##HardcoreSelectorChild", new Vector2(width, -ImGui.GetFrameHeight()), true);
        if (!child)
            return;

        using var style     = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, _defaultItemSpacing);
        var       skips     = OtterGui.ImGuiClip.GetNecessarySkips(ImGui.GetTextLineHeight());
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(
                                    _charHandler.whitelistChars, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }


    public void DrawSelectable(WhitelistedCharacterInfo characterInfo) {
        // if the character is in the whitelist,
        // might be able to modify this to be something besides the first index
        if(AltCharHelpers.IsPlayerInWhitelist(characterInfo._charNAW[0]._name, out int whitelistCharIdx))
        {
            // first we need to see if the active index is set to the current characters main name index
            var equals = _charHandler.activeListIdx == whitelistCharIdx;
            
            // if the selectable is not the active list index, update it
            string selectableLabel = characterInfo._charNAWIdxToProcess == 0
                ? characterInfo._charNAW[characterInfo._charNAWIdxToProcess]._name
                : $"{characterInfo._charNAW[characterInfo._charNAWIdxToProcess]._name} (Alt)";

            if (ImGui.Selectable(selectableLabel, equals) && !equals)
            {
                // update the active list index
                _charHandler.activeListIdx = whitelistCharIdx;
                _charHandler.Save();
            }
        }
    }

    // Draw the buttons for adding and removing players from the whitelist
    private void DrawPuppeteerButtons(float width) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(width, ImGui.GetFrameHeight());
        var modeToSwitchTo = GameConfig.UiControl.GetBool("MoveMode") ? MovementMode.Standard : MovementMode.Legacy;
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Wrench.ToIconString(), buttonWidth,
        $"Toggle Movement Mode to {modeToSwitchTo}\n(for fixing any desync issues, report this if you have any)", false, true))
        {
            if(modeToSwitchTo == MovementMode.Standard) {
                GameConfig.UiControl.Set("MoveMode", (int)MovementMode.Standard);
            } else {
                GameConfig.UiControl.Set("MoveMode", (int)MovementMode.Legacy);
            }
        }
        style.Pop();
    }
}