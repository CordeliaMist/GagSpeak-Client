using System;
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
    public bool ReslogicToggleGagStorageUiLock(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTierNonClient(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength != DynamicTier.Tier0) {
                // toggle the gag storage UI lock
                _characterHandler.ToggleLockGagStorageOnGagLock();
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Gag Storage UI Lock has been toggled to "+
                $"{_characterHandler.playerChar._lockGagStorageOnGagLock}.").AddItalicsOff().BuiltString);
                // log the result
                GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for toggling gag storage UI lock");
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
    public bool ResLogicToggleEnableRestraintSetsOption(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the whitelist index
            int whitelistIdx = _characterHandler.GetWhitelistIndex(playerName);
            // get its index
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTierNonClient(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength >= DynamicTier.Tier2) {
                // toggle the enable restraint sets option for that player
                _characterHandler.ToggleEnableRestraintSets(whitelistIdx);
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Enable Restraint Sets has been toggled "+
                $"to {_characterHandler.playerChar._uniquePlayerPerms[whitelistIdx]._enableRestraintSets}.").AddItalicsOff().BuiltString);
                GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for toggling enable restraint sets");
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
    public bool ResLogicToggleAllowRestraintLockingOption(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            int idx = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTierNonClient(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength != DynamicTier.Tier0) {
                // toggle the allow restraint locking option for that player
                _characterHandler.ToggleRestraintSetLocking(idx);
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Allow Restraint Locking "+
                $"has been toggled to {_characterHandler.playerChar._uniquePlayerPerms[idx]._restraintSetLocking}.").AddItalicsOff().BuiltString);
                GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for toggling allow restraint locking");
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
    public bool ResLogicToggleRestraintSetState(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // toggle the gag storage if we have a tier above 1 or higher
            if(_characterHandler.playerChar._allowRestraintSetAutoEquip == true && _characterHandler.GetDynamicTierNonClient(playerName) >= DynamicTier.Tier2) {
                // see if our restraint set is anywhere in the list
                int setIdx = _restraintSetManager.GetRestraintSetIndex(decodedMessageMediator.setToLockOrUnlock);
                // exit if the index is -1
                if (setIdx == -1) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Restraint Set Name {decodedMessageMediator.setToLockOrUnlock} was attempted to be applied to you, but the set does not exist!");
                    return false;
                }
                // Check if any restraint sets are currently locked
                if (_restraintSetManager.AreAnySetsLocked()) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: A restraint set is currently locked. Cannot enable a new set.");
                    return false;
                }
                // if it is, then set that restraint sets enabled flag to true.
                _restraintSetManager.ChangeRestraintSetState(setIdx, !_restraintSetManager._restraintSets[setIdx]._enabled, playerName);
                // notify the user that the request as been sent. 
                var newText = _restraintSetManager._restraintSets[setIdx]._enabled ? "enabled" : "disabled";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Restraint Set {decodedMessageMediator.setToLockOrUnlock} has been {newText}.").AddItalicsOff().BuiltString);
                GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for enabling restraint set");
                return true;
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to enable a restraint set.");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
            return false;
        }
        return false;
    }

    // decoder message for locking the restraint set onto the player [ ID == 25 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName, [14] = timer
    public bool ResLogicLockRestraintSet(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            DynamicTier dynamicStrength = _characterHandler.GetDynamicTierNonClient(playerName);
            int whitelistIdx = _characterHandler.GetWhitelistIndex(playerName);
            // toggle the gag storage if we have a tier above 1 or higher
            if(dynamicStrength != DynamicTier.Tier0 && _characterHandler.playerChar._allowRestraintSetAutoEquip 
            && _characterHandler.playerChar._uniquePlayerPerms[whitelistIdx]._restraintSetLocking) {
                // see if our restraint set is anywhere in the list
                int setIdx = _restraintSetManager.GetRestraintSetIndex(decodedMessageMediator.setToLockOrUnlock);
                // exit if the index is -1
                if (setIdx == -1) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Restraint Set Name {decodedMessageMediator.setToLockOrUnlock} was attempted to be applied to you, but the set does not exist!");
                    return false;
                }
                // make sure that the formatted time is not longer than 12 hours
                if (UIHelpers.GetEndTime(decodedMessageMediator.layerTimer[0]) - DateTimeOffset.Now > TimeSpan.FromHours(12)
                && _characterHandler.playerChar._uniquePlayerPerms[whitelistIdx]._grantExtendedLockTimes == false) {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Timer {decodedMessageMediator.layerTimer[0]} is too long, it must be less than 12 hours unless your partner "+
                    "has allowd you to have extended lock times.");
                    return false;
                }

                // lock the restraint set, if it is enabled
                if(_restraintSetManager._restraintSets[setIdx]._enabled) {
                    if(!_restraintSetManager._restraintSets[setIdx]._locked) {
                        // lock the restraint set
                        _lockManager.LockRestraintSet(decodedMessageMediator.setToLockOrUnlock, playerName, decodedMessageMediator.layerTimer[0], playerName);
                        // notify the user that the request as been sent. 
                        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Restraint Set "+
                        $"{decodedMessageMediator.setToLockOrUnlock} has been locked for {decodedMessageMediator.layerTimer[0]}.").AddItalicsOff().BuiltString);
                        GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for locking restraint set");
                        return true;
                    } else {
                        isHandled = true;
                        LogError($"[MsgResultLogic]: Restraint Set {decodedMessageMediator.setToLockOrUnlock} is already locked!");
                        return false;
                    }
                } else {
                    isHandled = true;
                    LogError($"[MsgResultLogic]: Restraint Set {decodedMessageMediator.setToLockOrUnlock} is not enabled, so can't lock it!");
                    return false;
                }
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Player {playerName} does not have a high enough dynamic tier to lock a restraint set.");
                return false;
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
            return false;
        }
    }

    // decoder message for unlocking the restraint set from the player [ ID == 26 ]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName
    private bool ResLogicRestraintSetUnlockMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // unlock the restraint set
            if(_lockManager.UnlockRestraintSet(decodedMessageMediator.setToLockOrUnlock, playerName)) {
                // notify the user that the request as been sent. 
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Restraint Set {decodedMessageMediator.setToLockOrUnlock} has been unlocked.").AddItalicsOff().BuiltString);
                GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for unlocking restraint set");
            } else {
                isHandled = true;
                LogError($"[MsgResultLogic]: Restraint Set {decodedMessageMediator.setToLockOrUnlock} could not be unlocked. {playerName} tried, but unlockrestraintset logic failed!");
            }
        } else {
            isHandled = true;
            LogError($"[MsgResultLogic]: Player {playerName} is not in your whitelist.");
        }
        return true;
    }
}
