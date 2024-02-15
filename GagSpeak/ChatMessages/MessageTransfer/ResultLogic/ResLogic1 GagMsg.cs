using System;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using GagSpeak.CharacterData;
using GagSpeak.Gagsandlocks;
using GagSpeak.Utility;
using OtterGui.Classes;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    // Handle the gag apply message logic.
    private bool HandleApplyMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // secondly, see if our gagtype(8) is in our list of gagtypes
        string gagName = decodedMessageMediator.layerGagName[0];
        if (!_gagService._gagTypes.Any(gag => gag._gagName == gagName) && _characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx] != "None") {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid gag type.");
        }
        // attempt to apply the gag via out lock manager
        _lockManager.ApplyGag(decodedMessageMediator.layerIdx, decodedMessageMediator.layerGagName[0], decodedMessageMediator.assignerName);
        // send sucessful message to chat
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You've been gagged by {playerName} with a {_characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx]}!").AddItalicsOff().BuiltString);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag apply");

        return true; // sucessful parse
    }

    // handle the gag lock message logic
    private bool HandleLockMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // second, make sure already have a gag on
        if (_characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx] == "None") {
            isHandled = true; return LogError($"[MsgResultLogic]: No gag applied for layer {decodedMessageMediator.layerIdx}, cannot apply lock!");
        }
        // third, make sure we dont already have a lock here
        if (_characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx] != Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: Already a lock applied to gag layer {decodedMessageMediator.layerIdx}!");
        }
        // all preconditions met, so now we can try to lock it.
        if (Enum.TryParse(decodedMessageMediator.layerPadlock[0], out Padlocks parsedLockType)) {
            // get the player payload to then properly format the assigner name
            PlayerPayload playerPayload; // get player payload
            UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
            // get the assigner name
            string assignerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
            // assign our lock type to the padlock identifier
            _config.padlockIdentifier[decodedMessageMediator.layerIdx].SetType(parsedLockType);
            _config.Save();
            // now attempt to lock the padlock. If the lock fails for any reason, the lock manager should let you know.
            _lockManager.Lock(decodedMessageMediator.layerIdx, assignerName, decodedMessageMediator.layerPassword[0], decodedMessageMediator.layerTimer[0], playerPayload.PlayerName);
            // if we reached this point, it means we sucessfully locked the layer
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{assignerName} locked your " +
            $"{_characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx]} with a "+
            $"{_characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx]}.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag lock");
            return true; // sucessful parse
        } else {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid /gag lock parameters sent in!");
        }
    }

    // handles the unlock message logic
    private bool HandleUnlockMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // second, make sure we have a lock on
        if (_characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx] == Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: No lock applied for layer {decodedMessageMediator.layerIdx}, cannot remove lock!");}
        // if we made it here, we can try to unlock it.
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // get the assigner name
        string assignerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // try to unlock it
        if(_characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx] == Padlocks.MistressPadlock
        || _characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx] == Padlocks.MistressTimerPadlock)
        {
            if(assignerName != _characterHandler.playerChar._selectedGagPadlockAssigner[decodedMessageMediator.layerIdx]) {
                isHandled = true; return LogError($"[MsgResultLogic]: {assignerName} is not the assigner of the lock on layer {decodedMessageMediator.layerIdx}!");
            }
        }
        // store the padlock
        Padlocks tempPadlock = _characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx];
        // attempt to unlock
        _lockManager.Unlock(decodedMessageMediator.layerIdx, assignerName, decodedMessageMediator.layerPassword[0], playerPayload.PlayerName, playerPayload.PlayerName);
        // print the sucessful message
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{assignerName} " +
        $"sucessfully unlocked the {Enum.GetName(typeof(Padlocks), tempPadlock)} from your {_characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx]}.").AddItalicsOff().BuiltString);        
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag unlock");
        return true; // sucessful parse
    }

    // logic for /gag remove message [ ID == 7 // remove ]
    private bool HandleRemoveMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // second, make sure that this layer has a gag on it
        if (_characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx] == "None") {
            isHandled = true; return LogError($"[MsgResultLogic]: There is no gag applied for gag layer {decodedMessageMediator.layerIdx}, so no gag can be removed.");
        }
        // third, make sure there is no lock on that gags layer
        if (_characterHandler.playerChar._selectedGagPadlocks[decodedMessageMediator.layerIdx] != Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: There is a lock applied for gag layer {decodedMessageMediator.layerIdx}, cannot remove gag!");
        }
        // finally, if we made it here, we can remove the gag
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // print the message
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} removed your "+
        $"{_characterHandler.playerChar._selectedGagTypes[decodedMessageMediator.layerIdx]}, how sweet.").AddItalicsOff().BuiltString);
        // remove the gag
        _lockManager.RemoveGag(decodedMessageMediator.layerIdx);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag remove");
        return true; // sucessful parse
    }

    // logic /gag removeall message [ ID == 8 // removeall ]
    private bool HandleRemoveAllMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_characterHandler.playerChar._selectedGagPadlocks.Any(padlock => padlock != Padlocks.None)) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot remove all gags while locks are on any of them.");
        }
        // if we made it here, we can remove them all
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // print the message
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has removed all of your gags.").AddItalicsOff().BuiltString);
        // remove all gags
        _lockManager.RemoveAllGags();
        // log result
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag removeall");
        return true; // sucessful parse
    }

    // decode the toggle for enabling live garbler [ ID == 9 // toggleLiveChatGarbler ]
    private bool HandleToggleLiveChatGarbler(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // extract the player name
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if player exists in your whitelist
        if(!_characterHandler.IsPlayerInWhitelist(playerName)) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot toggle live chat garbler for non-whitelisted player.");
        }
        // get the dynamic tier of your relation with that person
        DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
        // make sure we have a valid tier to do this
        if(tier != DynamicTier.Tier4) {
            isHandled = true; return LogError($"[MsgResultLogic]: {playerName} tried to toggle your live chat garbler but failed. Your dynamic is not strong enough.");
        }
        // if we are already locked, then we can't toggle it
        if (_characterHandler.playerChar._directChatGarblerLocked) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot toggle live chat garbler while it is locked, must unlock first!.");
        }
        // if we reach here, we meet the conditions to set it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has toggled your live chat garbler.").AddItalicsOff().BuiltString);
        // toggle the live chat garbler
        _characterHandler.ToggleDirectChatGarbler();
        // log result
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag toggleLiveChatGarbler");
        return true; // sucessful parse
    }

    // decode the toggle for locking live garbler [ ID == 10 // toggleLiveChatGarblerLock ]
    private bool HandleToggleLiveChatGarblerLock(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // extract the player name
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(!_characterHandler.IsPlayerInWhitelist(playerName)) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot toggle live chat garbler lock for non-whitelisted player.");
        }
        // get the dynamic tier
        DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
        // if our tier is not strong enough, exit.
        if(tier < DynamicTier.Tier3) {
            isHandled = true; return LogError($"[MsgResultLogic]: {playerName} failed to lock your direct garbler, Your dynamic is not strong enough.");
        }
        // if we reach here, we meet the conditions to set it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has toggled your live chat garbler lock.").AddItalicsOff().BuiltString);
        // toggle the live chat garbler lock
        _characterHandler.ToggleDirectChatGarblerLock();
        // log result
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag toggleLiveChatGarblerLock");
        return true; // sucessful parse
    }

    private bool HandleToggleExtendedLockTimes(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // extract the player name
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(!_characterHandler.IsPlayerInWhitelist(playerName)) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot toggle extended lock times for non-whitelisted player.");
        }
        // get the dynamic tier
        DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
        // if our tier is not strong enough, exit.
        if(tier < DynamicTier.Tier2) {
            isHandled = true; return LogError($"[MsgResultLogic]: {playerName} failed to toggle extended lock times, Your dynamic is not strong enough.");
        }
        // if we reach here, we meet the conditions to set it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has toggled your extended lock times.").AddItalicsOff().BuiltString);
        // toggle the extended lock times
        _characterHandler.ToggleExtendedLockTimes();
        // log result
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag toggleExtendedLockTimes");
        return true; // sucessful parse
    }
}

