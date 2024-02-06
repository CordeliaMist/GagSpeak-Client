using System;
using System.Collections.Generic;
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
    // Attempts to determine the result for the decoded lock messages.
    private string GetPlayerName(string playerNameWorld) {
        string[] parts = playerNameWorld.Split(' ');
        return string.Join(" ", parts.Take(parts.Length - 1));
    }

    // Handle the gag apply message logic.
    // [0] = commandtype, [1] = GagAssigner (who sent this message), [2] = layerIndex, [8] = gagtype/gagname,
    private bool HandleApplyMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer(2) is a valid layer
        if (!int.TryParse(decodedMessage[2], out int layer)) { 
            isHandled = true;
            return LogError("[MsgResultLogic]: Invalid layer value.");
        }

        // secondly, see if our gagtype(8) is in our list of gagtypes
        string gagName = decodedMessage[8];
        if (!_gagService._gagTypes.Any(gag => gag._gagName == gagName) && _characterHandler.playerChar._selectedGagTypes[layer-1] != "None") {
            isHandled = true;
            return LogError("[MsgResultLogic]: Invalid gag type.");
        }

        // attempt to apply the gag via out lock manager
        _lockManager.ApplyGag(layer-1, decodedMessage[8], decodedMessage[2]);
        // send sucessful message to chat
        string playerName = GetPlayerName(decodedMessage[2]);
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You've been gagged by {playerName} with a {_characterHandler.playerChar._selectedGagTypes[layer-1]}!").AddItalicsOff().BuiltString);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag apply");

        return true; // sucessful parse
    }

    // handle the gag lock message logic
    // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex, [11] = lockType, [14] = password, [15] = timer
    private bool HandleLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, check if we have valid layer
        if (!int.TryParse(decodedMessage[2], out int layer)) { 
            isHandled = true;
            return LogError("[MsgResultLogic]: Invalid layer value.");
        }

        // second, make sure already have a gag on
        if (_characterHandler.playerChar._selectedGagTypes[layer-1] == "None") {
            isHandled = true;
            return LogError($"[MsgResultLogic]: No gag applied for layer {layer}, cannot apply lock!");
        }

        // third, make sure we dont already have a lock here
        if (_characterHandler.playerChar._selectedGagPadlocks[layer-1] != Padlocks.None) {
            isHandled = true;
            return LogError($"[MsgResultLogic]: Already a lock applied to gag layer {layer}!");
        }
        
        // all preconditions met, so now we can try to lock it.
        if (Enum.TryParse(decodedMessage[11], out Padlocks parsedLockType)) {
            // get the player payload to then properly format the assigner name
            PlayerPayload playerPayload; // get player payload
            UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
            string[] nameParts = decodedMessage[1].Split(' ');
            decodedMessage[1] = nameParts[0] + " " + nameParts[1];
            // assign our lock type to the padlock identifier
            _config.padlockIdentifier[layer-1].SetType(parsedLockType);
            // now attempt to lock the padlock. If the lock fails for any reason, the lock manager should let you know.
            _lockManager.Lock((layer-1), decodedMessage[1], decodedMessage[14], decodedMessage[15], playerPayload.PlayerName);
            // if we reached this point, it means we sucessfully locked the layer
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{decodedMessage[1]} locked your " +
            $"{_characterHandler.playerChar._selectedGagTypes[layer-1]} with a {_characterHandler.playerChar._selectedGagPadlocks[layer-1]}.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag lock");
            return true; // sucessful parse
        } else {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid /gag lock parameters sent in!");
        }
    }

    // handles the unlock message logic
    // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex, [14] = password
    private bool HandleUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (!int.TryParse(decodedMessage[2], out int layer)) { 
            isHandled = true;
            return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure we have a lock on
        if (_characterHandler.playerChar._selectedGagPadlocks[layer-1] == Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: No lock applied for layer {layer}, cannot remove lock!");}
        // if we made it here, we can try to unlock it.
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        string[] nameParts = decodedMessage[1].Split(' ');
        decodedMessage[1] = nameParts[0] + " " + nameParts[1];
        // try to unlock it
        if(_characterHandler.playerChar._selectedGagPadlocks[layer-1] == Padlocks.MistressPadlock || _characterHandler.playerChar._selectedGagPadlocks[layer-1] == Padlocks.MistressTimerPadlock) {
            if(decodedMessage[1] != _characterHandler.playerChar._selectedGagPadlockAssigner[layer-1]) {
                isHandled = true; return LogError($"[MsgResultLogic]: {decodedMessage[1]} is not the assigner of the lock on layer {layer}!");
            }
        }

        // store the padlock
        Padlocks tempPadlock = _characterHandler.playerChar._selectedGagPadlocks[layer-1];
        
        // attempt to unlock
        _lockManager.Unlock((layer-1), decodedMessage[1], decodedMessage[14], playerPayload.PlayerName, playerPayload.PlayerName);
        // print the sucessful message
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{decodedMessage[1]} " +
        $"sucessfully unlocked the {Enum.GetName(typeof(Padlocks), tempPadlock)} from your {_characterHandler.playerChar._selectedGagTypes[layer-1]}.").AddItalicsOff().BuiltString);        
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag unlock");
        return true; // sucessful parse
    }

    // logic for /gag remove message [ ID == 7 // remove ]
    // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex
    private bool HandleRemoveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (!int.TryParse(decodedMessage[2], out int layer)) { 
            isHandled = true;
            return LogError("[MsgResultLogic]: Invalid layer value.");
        }

        // second, make sure that this layer has a gag on it
        if (_characterHandler.playerChar._selectedGagTypes[layer-1] == "None") {
            isHandled = true;
            return LogError($"[MsgResultLogic]: There is no gag applied for gag layer {layer}, so no gag can be removed.");
        }

        // third, make sure there is no lock on that gags layer
        if (_characterHandler.playerChar._selectedGagPadlocks[layer-1] != Padlocks.None) {
            isHandled = true;
            return LogError($"[MsgResultLogic]: There is a lock applied for gag layer {layer}, cannot remove gag!");
        }

        // finally, if we made it here, we can remove the gag
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} removed your {_characterHandler.playerChar._selectedGagTypes[layer-1]}, how sweet.").AddItalicsOff().BuiltString);
        // remove the gag
        _lockManager.RemoveGag(layer-1);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag remove");
        return true; // sucessful parse
    }

    // logic /gag removeall message [ ID == 8 // removeall ]
    // [0] = commandtype, [1] = LockAssigner
    private bool HandleRemoveAllMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_characterHandler.playerChar._selectedGagPadlocks.Any(padlock => padlock != Padlocks.None)) {
            isHandled = true;
            return LogError("[MsgResultLogic]: Cannot remove all gags while locks are on any of them.");
        }

        // if we made it here, we can remove them all
        string playerNameWorld = decodedMessage[1]; 
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has removed all of your gags.").AddItalicsOff().BuiltString);
        _lockManager.RemoveAllGags(); // remove all gags
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag removeall");
        return true; // sucessful parse
    }

    // decode the toggle for enabling live garbler [ ID == 9 // toggleLiveChatGarbler ]
    // [0] = commandtype, [1] = ToggleAssigner
    private bool HandleToggleLiveChatGarbler(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // extract the player name
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));

        // see if they exist
        if(!_characterHandler.IsPlayerInWhitelist(playerName)) {
            isHandled = true;
            return LogError("[MsgResultLogic]: Cannot toggle live chat garbler for non-whitelisted player.");
        }
        // get the dynamic tier
        DynamicTier tier = _characterHandler.GetDynamicTier(playerName);

        if(tier == DynamicTier.Tier0 || tier == DynamicTier.Tier1 || tier == DynamicTier.Tier2 || tier == DynamicTier.Tier3) {
            isHandled = true;
            return LogError($"[MsgResultLogic]: {playerName} tried to toggle your live chat garbler but failed. Your dynamic is not strong enough.");
        }

        // if we are already locked, then we can't toggle it
        if (_characterHandler.playerChar._directChatGarblerLocked) {
            isHandled = true;
            return LogError("[MsgResultLogic]: Cannot toggle live chat garbler while it is locked.");
        }

        // if we reach here, we meet the conditions to set it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has toggled your live chat garbler.").AddItalicsOff().BuiltString);
        _characterHandler.playerChar._directChatGarblerActive = !_characterHandler.playerChar._directChatGarblerActive; // toggle the live chat garbler
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag toggleLiveChatGarbler");
        return true; // sucessful parse
    }

    // decode the toggle for locking live garbler [ ID == 10 // toggleLiveChatGarblerLock ]
    // [0] = commandtype, [1] = ToggleAssigner
    private bool HandleToggleLiveChatGarblerLock(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // extract the player name
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));

        // see if they exist
        if(!_characterHandler.IsPlayerInWhitelist(playerName)) {
            isHandled = true;
            return LogError("[MsgResultLogic]: Cannot toggle live chat garbler lock for non-whitelisted player.");
        }
        // get the dynamic tier
        DynamicTier tier = _characterHandler.GetDynamicTier(playerName);

        // if our tier is not strong enough, exit.
        if(tier == DynamicTier.Tier0 || tier == DynamicTier.Tier1 || tier == DynamicTier.Tier2) {
            isHandled = true;
            return LogError($"[MsgResultLogic]: {playerName} tried to toggle your live chat garbler lock but failed. Your dynamic is not strong enough.");
        }

        // if we reach here, we meet the conditions to set it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has toggled your live chat garbler lock.").AddItalicsOff().BuiltString);
        _characterHandler.playerChar._directChatGarblerLocked = !_characterHandler.playerChar._directChatGarblerLocked; // toggle the live chat garbler lock
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag toggleLiveChatGarblerLock");
        return true; // sucessful parse
    }
}

