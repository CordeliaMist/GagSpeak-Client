using System;
using System.Linq;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

namespace GagSpeak.Chat.MsgResultLogic;

public class MessageResultLogic { // Purpose of class : To perform logic on client based on the type of the sucessfully decoded message.
    /// <summary>
    /// Will take in a message, and determine what to do with it based on the contents of the message.
    /// <list>
    /// <item><c>apply GAGTYPE | PLAYER</c> - Equip Gagtype to defined layer</item>
    /// <item><c>lock LAYER LOCKTYPE | PLAYER</c> - Lock Gagtype to defined layer</item>
    /// <item><c>lock LAYER LOCKTYPE | PASSWORD | PLAYER</c> - Lock Gagtype to defined layer with password</item>
    /// <item><c>unlock LAYER | PLAYER</c> - Unlock Gagtype from defined layer</item>
    /// <item><c>unlock LAYER | PASSWORD | PLAYER</c> - Unlock Gagtype from defined layer with password</item>
    /// <item><c>removeall | PLAYER</c> - Remove all gags from player only when parameters are met</item>
    /// <item><c>remove LAYER | PLAYER</c> - Remove gag from defined layer</item></list>
    /// <para><c>recievedMessage</c><param name="receivedMessage"> - The message that was recieved from the player</param></para>
    /// </summary>
    public bool PerformMsgResultLogic(string receivedMessage, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        // execute sub-function based on the input
        // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]
        // if the parsed type is "lock" or "lockPassword"
        if (decodedMessage[0] == "lock" || decodedMessage[0] == "lockPassword") {
            return HandleLockMessage(ref decodedMessage, ref isHandled, clientChat, config);
        }
        // if the parsed type is "unlock" or "unlockPassword"
        else if (decodedMessage[0] == "unlock" || decodedMessage[0] == "unlockPassword") {
            return HandleUnlockMessage(ref decodedMessage, ref isHandled, clientChat, config);
        }
        // if the parsed type is "removeall"
        else if (decodedMessage[0] == "removeall") {
            return HandleRemoveAllMessage(ref decodedMessage, ref isHandled, clientChat, config);
        }
        // if the parsed type is "remove"
        else if (decodedMessage[0] == "remove") {
            return HandleRemoveMessage(ref decodedMessage, ref isHandled, clientChat, config);
        }
        else if (decodedMessage[0] == "apply") {
            return HandleApplyMessage(ref decodedMessage, ref isHandled, clientChat, config);
        }
        // remember to apply other keywords for the request commands.
        else {
            GagSpeak.Log.Debug("ERROR, Invalid message parse, If you see this report it to cordy ASAP.");
            clientChat.PrintError($"ERROR, Invalid message parse, If you see this report it to cordy ASAP.");
            return false;
        }
    }


    // Below are all of the sub-functions that are called by the main function above, each split to their own decoded message catagory.

    // handle the lock message (realistically these should be falses, but they work in both cases and i am lazy)
    private bool HandleLockMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // Our layer is valid, but we also need to make sure that we have a gag on this layer
        if (_config.selectedGagTypes[layer-1] == "None") {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is no gag applied for layer {layer}, so no lock can be applied.");
            _clientChat.PrintError($"ERROR, There is no gag applied for layer {layer}, so no lock can be applied.");
            return true;
        }
        // if we do have a gag on this layer, make sure that we dont already have a lock here
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is already a lock applied to gag layer {layer}!");
            _clientChat.PrintError($"ERROR, There is already a lock applied to gag layer {layer}!");
            return true;
        }
        // we already made sure that we applied a valid password in the command manager, so no need to check it here.
        if (decodedMessage[3] != "") {
            _config.selectedGagPadlocksPassword[layer-1] = decodedMessage[3]; // we have a password to set, so set it.
        }
        // and because everything above is valid, we can now set the lock type.
        if (Enum.TryParse(decodedMessage[2], out GagPadlocks parsedLockType)) {
            _config.selectedGagPadlocks[layer-1] = parsedLockType;
        } else {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid lock type sent in.");
            _clientChat.PrintError($"ERROR, Invalid lock type sent in.");
            return true;
        }
        // now that we have applied our gagtype, and potentially password, set the assigner to the player if it is a mistress padlock.
        if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock || _config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock) {
            _config.selectedGagPadlocksAssigner[layer-1] = decodedMessage[4];
        }
        GagSpeak.Log.Debug($"Determined income message as a [lock] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }

    // handle the unlock message
    private bool HandleUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // our layer is valid, but we also need to make sure that this layer has a lock on it
        if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.None) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is no lock applied for gag layer {layer}, so no lock can be removed.");
            _clientChat.PrintError($"ERROR, There is no lock applied for gag layer {layer}, so no lock can be removed.");
            return true;
        }
        // Case where it is just unlock
        if (decodedMessage[3] == "") {
            // Make sure it is not a MistressPadlock
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock && _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                _clientChat.PrintError($"ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                return true;
            }
            // if we made it here, we can just remove the lock
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "None";
        } else {
            // if we do have a password, we need to make sure it matches the password on the lock
            if (_config.selectedGagPadlocksPassword[layer-1] != decodedMessage[3]) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid Password, failed to unlock.");
                _clientChat.PrintError($"ERROR, Invalid Password, failed to unlock.");
                return true;
            }
            // if the passwords do match, so remove the lock IF it is not a mistress padlock.
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock &&
                _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                _clientChat.PrintError($"ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                return true;
            }
            // if we made it here, we can remove the lock.
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "None";
        }
        GagSpeak.Log.Debug($"Determined income message as a [unlock] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }

    // handle the remove message
    private bool HandleRemoveMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // our layer is valid, but we also need to make sure that this layer has a gag on it
        if (_config.selectedGagTypes[layer-1] == "None") {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is no gag applied for gag layer {layer}, so no gag can be removed.");
            _clientChat.PrintError($"ERROR, There is no gag applied for gag layer {layer}, so no gag can be removed.");
            return true;
        }
        // make sure there is no lock on that gags layer
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Cannot remove a gag while the lock is on for this layer.");
            _clientChat.PrintError($"ERROR, Cannot remove a gag while the lock is on for this layer.");
            return true;
        }
        // if we made it here, we can remove the gag
        _config.selectedGagTypes[layer-1] = "None";
        _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
        _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
        _config.selectedGagPadlocksAssigner[layer-1] = "None";
        GagSpeak.Log.Debug($"Determined income message as a [remove] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }


    // handle the removeall message
    private bool HandleRemoveAllMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_config.selectedGagPadlocks.Any(padlock => padlock != GagPadlocks.None)) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Cannot remove all gags while locks are on any of them.");
            _clientChat.PrintError($"ERROR, Cannot remove all gags while locks are on any of them.");
            return true;
        }
        // if we made it here, we can remove them all
        for (int i = 0; i < _config.selectedGagPadlocks.Count; i++) {
            _config.selectedGagTypes[i] = "None";
            _config.selectedGagPadlocks[i] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[i] = string.Empty;
            _config.selectedGagPadlocksAssigner[i] = "None";
        }
        GagSpeak.Log.Debug($"Determined income message as a [removeall] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }

    // handle the apply message
    private bool HandleApplyMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // see if our gagtype is in selectedGagTypes[layer-1]
        if (!_config.GagTypes.ContainsKey(decodedMessage[2])) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid gag type.");
            _clientChat.PrintError($"ERROR, Invalid gag type.");
            return true;
        }
        // make sure gagType is set to none
        if (_config.selectedGagTypes[layer-1] != "None") {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is already a gag applied for gag layer {layer}!");
            _clientChat.PrintError($"ERROR, There is already a gag applied for gag layer {layer}!");
            return true;
        }
        // if we made it here, we can apply the gag
        _config.selectedGagTypes[layer-1] = decodedMessage[2];
        GagSpeak.Log.Debug($"Determined income message as a [applier] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }

}
