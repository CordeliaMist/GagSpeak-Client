using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GagSpeak.Utility;

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

    public void AddPlayerAsAltPlayer(string playerName, string playerWorld) {
        // update whitelist
        _characterHandler.AddAlternateNameToPlayer(playerName, playerWorld);
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
        using var child = ImRaii.Child("##WhitelistSelector", new Vector2(width, -(4*ImGui.GetFrameHeight() + 3*ImGuiHelpers.GlobalScale)), true);
        if (!child)
            return;

        using var style     = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, _defaultItemSpacing);
        var       skips     = OtterGui.ImGuiClip.GetNecessarySkips(ImGui.GetTextLineHeight());
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(
                                    _characterHandler.whitelistChars, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }

    public void DrawSelectable(WhitelistedCharacterInfo characterInfo) {
        // if the character is in the whitelist,
        // might be able to modify this to be something besides the first index
        if(AltCharHelpers.IsPlayerInWhitelist(characterInfo._charNAW[0]._name, out int whitelistCharIdx))
        {
            // first we need to see if the active index is set to the current characters main name index
            var equals = _characterHandler.activeListIdx == whitelistCharIdx;
            
            // if the selectable is not the active list index, update it
            string selectableLabel = characterInfo._charNAWIdxToProcess == 0
                ? characterInfo._charNAW[characterInfo._charNAWIdxToProcess]._name
                : $"{characterInfo._charNAW[characterInfo._charNAWIdxToProcess]._name} (Alt)";

            if (ImGui.Selectable(selectableLabel, equals) && !equals)
            {
                // update the active list index
                SetNewActiveIndex(whitelistCharIdx);
            }
        }
    }

    public bool IsPlayerInWhitelist(string targetName) {
        return _characterHandler.whitelistChars.Any(x => x._charNAW.Any(tuple => tuple._name == targetName));
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
        return _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAW[0]._name;
    }

    public string GetNameAtIndexZero() {
        return _characterHandler.whitelistChars[0]._charNAW[0]._name;
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
