using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Windows.Data.Widgets;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Interop;
using GagSpeak.Services;
using GagSpeak.Utility;
using ImGuiNET;
using Newtonsoft.Json;
using OtterGui;

namespace GagSpeak.UI.Tabs.WhitelistTab;

/// <summary> This class is used to handle the whitelist tab. </summary>
public class WhitelistSelector
{
    //private readonly    CharacterHandler    _characterHandler;   // for getting the whitelist
    private readonly    ListMediator        _listMediator;       // for keeping the hardcore list and whitelist in sync
    private readonly    IClientState        _clientState;        // for getting the local player
    private readonly    IDataManager        _dataManager;        // for getting the world name
    private             Vector2             _defaultItemSpacing; // for setting the item spacing
    
    public WhitelistSelector(IClientState clientState, IDataManager dataManager, ListMediator listMediator) {
        _clientState = clientState;
        _dataManager = dataManager;
        _listMediator = listMediator;
    }

    private void DrawWhitelistHeader(float width, Action<bool> setInteractions, bool _interactions) // Draw our header
        => WindowHeader.Draw("Whitelist", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, width, InteractionsButton(setInteractions, _interactions));

    public void Draw(float width, Action<bool> setInteractions, ref bool _interactions) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        using (_ = ImRaii.Group()) {
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        DrawWhitelistHeader(width, setInteractions, _interactions);
        // make content disabled
        _listMediator.DrawWhitelistSelector(width, _defaultItemSpacing);
        if(!_interactions) { ImGui.BeginDisabled(); }
        try{
            DrawWhitelistButtons(width);
        } finally {
            if(!_interactions) { ImGui.EndDisabled(); }
        }
        style.Pop();
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
                    GSLogger.LogType.Debug($"[Whitelist]: Targeted Player: {_clientState.LocalPlayer.TargetObject.Name.TextValue}");
                    string targetName = UIHelpers.CleanSenderName(_clientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                    // if the object kind of the target is a player, then get the character parse of that player
                    var targetCharacter = (PlayerCharacter)_clientState.LocalPlayer.TargetObject;
                    // now we can get the name and world from them
                    var world = targetCharacter.HomeWorld.Id;
                    var worldName = _dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow((uint)world)?.Name?.ToString() ?? "Unknown";
                    GSLogger.LogType.Debug($"[Whitelist]: Targeted Player: {targetName} from {world}, {worldName}");

                    // And now, if the player is not already in our _characterHandler.whitelistChars, we will add them. Otherwise just do nothing.
                    if (!_listMediator.IsPlayerInWhitelist(targetName)) {
                        GSLogger.LogType.Debug($"[Whitelist]: Adding targeted player to _characterHandler.whitelistChars {_clientState.LocalPlayer.TargetObject})");
                        if(_listMediator.GetNewWhitelistCount() == 1 && _listMediator.GetNameAtIndexZero() == "None None") {
                            _listMediator.ReplacePlayerInList(0, targetName, worldName);
                        } else {
                            _listMediator.AddPlayerToList(targetName, worldName); // Add the player to the _characterHandler.whitelistChars
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
            if (_listMediator.GetNewWhitelistCount() == 1) {
                _listMediator.ReplacePlayerInList(0, "None None","None");
            } else {
                _listMediator.RemovePlayerInList();
            }
            var newIdx = _listMediator.GetNewWhitelistCount() - 1;
            if (newIdx < 0) { newIdx = 0; }
            _listMediator.SetNewActiveIndex(newIdx);
        }
        xPos = ImGui.GetCursorPosX();
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPos, yPos + ImGuiHelpers.GlobalScale));
        // display the three buttons for pasting in restraint set data, alias commands, and pattern lists
        buttonWidth = new Vector2(width/3, 0);
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Handcuffs.ToIconString(), buttonWidth,
        $"Paste {_listMediator.GetActiveListName()}'s copied restraint set list", false, true)) {
            GSLogger.LogType.Debug($"[Whitelist]: Pasting in restraint set list for {_listMediator.GetActiveListName()}");
            ImportRestraintSetList();
        }
        ImGui.SameLine();
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.FilePen.ToIconString(), buttonWidth,
        $"Paste {_listMediator.GetActiveListName()}'s copied alias command list", false, true)) {
            GSLogger.LogType.Debug($"[Whitelist]: Pasting in alias command list for {_listMediator.GetActiveListName()}");
            ImportAliasCommandList();
        }
        ImGui.SameLine();
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.FileMedicalAlt.ToIconString(), buttonWidth,
        $"Paste {_listMediator.GetActiveListName()}'s copied pattern list", false, true)) {
            GSLogger.LogType.Debug($"[Whitelist]: Pasting in pattern list for {_listMediator.GetActiveListName()}");
            ImportPatternList();
        }
        // pop style
        style.Pop();
    }

    private WindowHeader.Button InteractionsButton(Action<bool> setInteractions, bool _enableInteractions)
            => !_listMediator.IsWhitelistIndexInBounds()
                ? WindowHeader.Button.Invisible
                : _enableInteractions
                    ? new WindowHeader.Button {
                        Description = "Disable interactions.",
                        Icon = FontAwesomeIcon.LockOpen,
                        OnClick = () => setInteractions(false),
                        Visible = true,
                        Disabled = false,
                    }
                    : new WindowHeader.Button {
                        Description = "Enable interactions.",
                        Icon = FontAwesomeIcon.Lock,
                        OnClick = () => setInteractions(true),
                        Visible = true,
                        Disabled = false,
                    };

    public void ImportRestraintSetList() {
        try {
            // Get the base64 string from the clipboard
            string base64 = ImGui.GetClipboardText();
            // Decode the base64 string back to a byte array
            var bytes = Convert.FromBase64String(base64);
            // Decompress the byte array back to a regular string
            var version = bytes[0];
            version = bytes.DecompressToString(out var decompressed);
            // Deserialize the string back to a list
            List<string> restraintSetList = JsonConvert.DeserializeObject<List<string>>(decompressed) ?? new List<string>();
            // Set the restraint set list
            _listMediator.StoreRestraintSetList(restraintSetList);
            GSLogger.LogType.Debug($"Set restraint set list from clipboard");
        } catch (Exception ex) {
            GSLogger.LogType.Warning($"{ex.Message} Could not set restraint set list from clipboard.");
        }
    }

    public void ImportAliasCommandList() {
        try {
            string base64 = ImGui.GetClipboardText();
            var bytes = Convert.FromBase64String(base64);
            var version = bytes[0];
            version = bytes.DecompressToString(out var decompressed);
            Dictionary<string, string> aliasCommandList = JsonConvert.DeserializeObject<Dictionary<string, string>>(decompressed) ?? new Dictionary<string, string>();
            _listMediator.StoreAliasDetailsForWhitelistePlayer(aliasCommandList);
            GSLogger.LogType.Debug($"Set alias command list from clipboard");
        } catch (Exception ex) {
            GSLogger.LogType.Warning($"{ex.Message} Could not set alias command list from clipboard.");
        }
    }

    public void ImportPatternList() {
        try {
            string base64 = ImGui.GetClipboardText();
            var bytes = Convert.FromBase64String(base64);
            var version = bytes[0];
            version = bytes.DecompressToString(out var decompressed);
            List<string> patternList = JsonConvert.DeserializeObject<List<string>>(decompressed) ?? new List<string>();
            _listMediator.StorePatternsForWhitelistedPlayer(patternList);
            GSLogger.LogType.Debug($"Set pattern list from clipboard");
        } catch (Exception ex) {
            GSLogger.LogType.Warning($"{ex.Message} Could not set pattern list from clipboard.");
        }
    }

}