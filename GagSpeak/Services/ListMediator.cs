using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Emote = Lumina.Excel.GeneratedSheets.Emote;

namespace GagSpeak.Services;
// a mediator for keeping the hardcore list and whitelist in sync
public class ListMediator
{
    private readonly CharacterHandler _characterHandler;
    private readonly HardcoreManager _hardcoreManager;

    public ListMediator(CharacterHandler characterHandler, HardcoreManager hardcoreManager) {
        _characterHandler = characterHandler;
        _hardcoreManager = hardcoreManager;
    }

    public void AddPlayerToList(string playerName, string playerWorld) {
        // update whitelist
        _characterHandler.AddNewWhitelistItem(playerName, playerWorld);
        // and update hardcore manager
        _hardcoreManager.AddNewPlayerConfig();
    }

    public void ReplacePlayerInList(int index, string playerName, string playerWorld) {
        // update whitelist
        _characterHandler.ReplaceWhitelistItem(index, playerName, playerWorld);
        // and update hardcore manager
        _hardcoreManager.ReplacePlayerConfig(index);
    }

    public void RemovePlayerInList() {
        // update whitelist
        _characterHandler.RemoveWhitelistItem(_characterHandler.activeListIdx);
        // and update hardcore manager
        _hardcoreManager.RemovePlayerConfig(_characterHandler.activeListIdx);
    }

    // insure hardcore manager has same size as whitelist
    public void EnsureHardcoreAndWhitelistSizeSync() {
        // get the difference in size
        int sizeDiff = _characterHandler.whitelistChars.Count - _hardcoreManager._perPlayerConfigs.Count;
        // if the difference is positive, we need to add to the hardcore manager
        if (sizeDiff > 0) {
            for (int i = 0; i < sizeDiff; i++) {
                _hardcoreManager.AddNewPlayerConfig();
            }
        }
        // if the difference is negative, we need to remove from the hardcore manager
        if (sizeDiff < 0) {
            for (int i = 0; i < Math.Abs(sizeDiff); i++) {
                _hardcoreManager.RemovePlayerConfig(_hardcoreManager._perPlayerConfigs.Count - 1);
            }
        }
    }

    // draws the whitelist
    public void DrawWhitelistSelector(float width, Vector2 _defaultItemSpacing) {
        using var child = ImRaii.Child("##WhitelistSelector", new Vector2(width, -(3*ImGui.GetFrameHeight() + 2*ImGuiHelpers.GlobalScale)), true);
        if (!child)
            return;

        using var style     = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, _defaultItemSpacing);
        var       skips     = OtterGui.ImGuiClip.GetNecessarySkips(ImGui.GetTextLineHeight());
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(
                                    _characterHandler.whitelistChars, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }

    public void DrawSelectable(WhitelistedCharacterInfo characterInfo) {
        var equals = _characterHandler.activeListIdx == _characterHandler.GetWhitelistIndex(characterInfo._name);
        if (ImGui.Selectable(characterInfo._name, equals) && !equals)
        {
            // update the active list index
            _characterHandler.activeListIdx = _characterHandler.GetWhitelistIndex(characterInfo._name);
        }
    }

    public bool IsPlayerInWhitelist(string targetName) {
        return _characterHandler.whitelistChars.Any(item => item._name == targetName);
    }

    // get new whitelist player list count
    public int GetNewWhitelistCount() {
        return _characterHandler.whitelistChars.Count;
    }

    // set new active index
    public void SetNewActiveIndex(int index) {
        _characterHandler.activeListIdx = index;
    }

    // get active list players name
    public string GetActiveListName() {
        return _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name;
    }

    public string GetNameAtIndexZero() {
        return _characterHandler.whitelistChars[0]._name;
    }

    // is whitelist index in bounds
    public bool IsWhitelistIndexInBounds() {
        return _characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx);
    }

    // store restraintsetlist
    public void StoreRestraintSetList(List<string> restraintSetList) {
        _characterHandler.StoreRestraintListForPlayer(_characterHandler.activeListIdx, restraintSetList);
    }

    // store alias list
    public void StoreAliasDetailsForWhitelistePlayer(Dictionary<string, string> aliasCommandList) {
        _characterHandler.StoredAliasDetailsForPlayer(_characterHandler.activeListIdx, aliasCommandList);
    }

    // store patterns
    public void StorePatternsForWhitelistedPlayer(List<string> patternList) {
        _characterHandler.StorePatternNames(_characterHandler.activeListIdx, patternList);
    }
    
}
