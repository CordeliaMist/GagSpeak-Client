using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.CharacterData;
using GagSpeak.Utility;
using ImGuiScene;
using OtterGui.Classes;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    // decoder message for toggling if the gagstorage UI will become inaccessable when a gag is locked or not [ ID == 21 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom
    public bool ReslogicToggleGagStorageUiLock(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTier(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength != DynamicTier.Tier0) {
                _characterHandler.ToggleLockGagStorageOnGagLock();
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Gag Storage UI Lock has been toggled to "+
                $"{_characterHandler.playerChar._lockGagStorageOnGagLock}.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for toggling gag storage UI lock");
                return true;
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to toggle the gag storage UI lock.");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return true;
    }

    // decoder message for toggling option that allows enabling your restraint sets [ ID == 22 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom
    public bool ResLogicToggleEnableRestraintSetsOption(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the whitelist index
            int whitelistIdx = _characterHandler.GetWhitelistIndex(playerName);
            // get its index
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTier(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength >= DynamicTier.Tier2) {
                _characterHandler.ToggleEnableRestraintSets(whitelistIdx);
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Enable Restraint Sets has been toggled "+
                $"to {_characterHandler.playerChar._enableRestraintSets[whitelistIdx]}.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for toggling enable restraint sets");
                return true;
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to toggle the enable restraint sets option.");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return true;
    }

    // decoder message for toggling option to allow locking restraint sets [ ID == 23 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom
    public bool ResLogicToggleAllowRestraintLockingOption(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            int idx = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTier(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength != DynamicTier.Tier0) {
                _characterHandler.ToggleRestraintSetLocking(idx);
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Allow Restraint Locking "+
                $"has been toggled to {_characterHandler.playerChar._restraintSetLocking[idx]}.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for toggling allow restraint locking");
                return true;
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to toggle the allow restraint locking option.");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return true;
    }

    // decoder message for enabling a restraint set [ ID == 24 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName
    public bool ResLogicEnableRestraintSet(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // toggle the gag storage if we have a tier above 1 or higher
            if(_characterHandler.playerChar._allowRestraintSetAutoEquip == true && _characterHandler.GetDynamicTier(playerName) >= DynamicTier.Tier2) {
                // see if our restraint set is anywhere in the list
                int setIdx = _restraintSetManager.GetRestraintSetIndex(decodedMessage[8]);
                // exit if the index is -1
                if (setIdx == -1) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Restraint Set Name {decodedMessage[8]} was attempted to be applied to you, but the set does not exist!");
                }
                // if it is, then set that restraint sets enabled flag to true.
                _restraintSetManager.ChangeRestraintSetEnabled(setIdx, true);
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Restraint Set {decodedMessage[8]} has been enabled.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for enabling restraint set");
                // update our apperance
                _jobChangedEvent.Invoke(); // filler until i become less lazy
                return true;
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to enable a restraint set.");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return false;
    }

    // decoder message for locking the restraint set onto the player [ ID == 25 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName, [14] = timer
    public bool ResLogicLockRestraintSet(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTier(playerName);
            int whitelistIdx = _characterHandler.GetWhitelistIndex(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength != DynamicTier.Tier0 && _characterHandler.playerChar._allowRestraintSetAutoEquip && _characterHandler.playerChar._restraintSetLocking[whitelistIdx]) {
                // see if our restraint set is anywhere in the list
                int setIdx = _restraintSetManager.GetRestraintSetIndex(decodedMessage[8]);
                // exit if the index is -1
                if (setIdx == -1) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Restraint Set Name {decodedMessage[8]} was attempted to be applied to you, but the set does not exist!");
                }
                // make sure that the formatted time is not longer than 12 hours
                if (UIHelpers.GetEndTime(decodedMessage[14]) - DateTimeOffset.Now > TimeSpan.FromHours(12) && _characterHandler.playerChar._grantExtendedLockTimes[whitelistIdx] == false) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Timer {decodedMessage[14]} is too long, it must be less than 12 hours unless your partner has allowd you to have extended lock times.");
                }

                // lock the restraint set, if it is enabled
                if(_restraintSetManager._restraintSets[setIdx]._enabled) {
                    // lock the restraint set
                    _lockManager.LockRestraintSet(decodedMessage[8], decodedMessage[1], decodedMessage[14], playerName);
                    // notify the user that the request as been sent. 
                    _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Restraint Set {decodedMessage[8]} has been locked for {decodedMessage[14]}.").AddItalicsOff().BuiltString);
                    GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for locking restraint set");
                    return true;
                } else {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Restraint Set {decodedMessage[8]} is not enabled, so can't lock it!");
                }
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to lock a restraint set.");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return false;
    }

    // decoder message for unlocking the restraint set from the player [ ID == 26 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName
    private bool ResLogicRestraintSetUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {

            // unlock the restraint set
            if(_lockManager.UnlockRestraintSet(decodedMessage[8], decodedMessage[1])) {
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Restraint Set {decodedMessage[8]} has been unlocked.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for unlocking restraint set");
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Restraint Set {decodedMessage[8]} could not be unlocked. You were not the assigner!");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return true;
    }
}
