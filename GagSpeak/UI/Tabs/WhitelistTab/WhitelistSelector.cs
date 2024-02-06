using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Utility;
using ImGuiNET;

namespace GagSpeak.UI.Tabs.WhitelistTab;

/// <summary> This class is used to handle the whitelist tab. </summary>
public class WhitelistSelector
{
    private readonly    CharacterHandler    _characterHandler;   // for getting the whitelist
    private readonly    IClientState        _clientState;        // for getting the local player
    private readonly    IDataManager        _dataManager;        // for getting the world name
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public WhitelistSelector(CharacterHandler characterHandler, IClientState clientState, IDataManager dataManager) {
        _characterHandler = characterHandler;
        _clientState = clientState;
        _dataManager = dataManager;
    }

    private void DrawWhitelistHeader(float width) // Draw our header
        => WindowHeader.Draw("Whitelist", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, width, WindowHeader.Button.Invisible);

    public void Draw(float width, ref bool _enableInteractions) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using (_ = ImRaii.Group()) {
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawWhitelistHeader(width);
        // make content disabled
        if(!_enableInteractions) { ImGui.BeginDisabled(); }
        DrawWhitelistSelector(width);
        DrawWhitelistButtons(width);
        // end the disabled state
        if(!_enableInteractions) { ImGui.EndDisabled(); }
        style.Pop();
        }
    }

    private void DrawWhitelistSelector(float width) {
        using var child = ImRaii.Child("##WhitelistSelector", new Vector2(width, -(2*ImGui.GetFrameHeight() + ImGuiHelpers.GlobalScale)), true);
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
        // get basic player informaion
        bool playerTargetted = _clientState.LocalPlayer != null && _clientState.LocalPlayer.TargetObject != null;
        bool playerCloseEnough = playerTargetted && Vector3.Distance( _clientState.LocalPlayer?.Position ?? default, _clientState.LocalPlayer?.TargetObject?.Position ?? default) < 3;

        // Message to display based on target proximity
        string targetedPlayerText = "Add Targeted Player"; // Displays if no target
        if (!playerTargetted) {
            targetedPlayerText = "No Player Target!"; // If not tagetting a player, display "No Target"
            ImGui.BeginDisabled(); // Disable the button since no target to add
        } else if (playerTargetted && !playerCloseEnough) {
            targetedPlayerText = "Player Too Far!"; // If target is too far, display "Too Far"
            ImGui.BeginDisabled(); // Disable the button since target is too far
        }
        // Create a button for adding the targetted player to the _characterHandler.whitelistChars, assuming they are within proxy.
        if (ImGui.Button(targetedPlayerText, buttonWidth)) {
            // prevent possible null in _clientState.LocalPlayer.TargetObject
            if (_clientState.LocalPlayer != null &&_clientState.LocalPlayer.TargetObject != null) {
                if (_clientState.LocalPlayer.TargetObject.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) { // if the player is targetting another player
                    GagSpeak.Log.Debug($"[Whitelist]: Targeted Player: {_clientState.LocalPlayer.TargetObject.Name.TextValue}");
                    string targetName = UIHelpers.CleanSenderName(_clientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                    // if the object kind of the target is a player, then get the character parse of that player
                    var targetCharacter = (PlayerCharacter)_clientState.LocalPlayer.TargetObject;
                    // now we can get the name and world from them
                    var world = targetCharacter.HomeWorld.Id;
                    var worldName = _dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow((uint)world)?.Name?.ToString() ?? "Unknown";
                    GagSpeak.Log.Debug($"[Whitelist]: Targeted Player: {targetName} from {world}, {worldName}");

                    // And now, if the player is not already in our _characterHandler.whitelistChars, we will add them. Otherwise just do nothing.
                    if (!_characterHandler.whitelistChars.Any(item => item._name == targetName)) {
                        GagSpeak.Log.Debug($"[Whitelist]: Adding targeted player to _characterHandler.whitelistChars {_clientState.LocalPlayer.TargetObject})");
                        if(_characterHandler.whitelistChars.Count == 1 && _characterHandler.whitelistChars[0]._name == "None") { // If our _characterHandler.whitelistChars just shows none, replace it with first addition.
                            _characterHandler.ReplaceWhitelistItem(0, targetName, worldName);
                        } else {
                            _characterHandler.AddNewWhitelistItem(targetName, worldName); // Add the player to the _characterHandler.whitelistChars
                        }
                    }
                }
            }
        }
        // If the player is not targetted or not close enough, end the disabled button
        if (!playerTargetted || !playerCloseEnough) { ImGui.EndDisabled(); }
        var xPos = ImGui.GetCursorPosX();
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPos, yPos + ImGuiHelpers.GlobalScale));
        if (ImGui.Button("Remove Player", buttonWidth)) {
            if (_characterHandler.whitelistChars.Count == 1) {
                _characterHandler.ReplaceWhitelistItem(0, "None","None");
            } else {
                _characterHandler.RemoveWhitelistItem(_characterHandler.activeListIdx);
            }
            var newIdx = _characterHandler.whitelistChars.Count - 1;
            if (newIdx < 0) { newIdx = 0; }
            _characterHandler.activeListIdx = newIdx;
        }
        // pop style
        style.Pop();
    }
}