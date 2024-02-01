using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    // Attempts to determine the result for the decoded lock messages.

    private bool HandleLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, check if we have valid layer
        if (!int.TryParse(decodedMessage[2], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure already have a gag on
        if (_config.playerInfo._selectedGagTypes[layer-1] == "None") {
            isHandled = true; return LogError($"[MsgResultLogic]: No gag applied for layer {layer}, cannot apply lock!");}
        // third, make sure we dont already have a lock here
        if (_config.playerInfo._selectedGagPadlocks[layer-1] != Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: Already a lock applied to gag layer {layer}!");}
        // all preconditions met, so now we can try to lock it.
        if (Enum.TryParse(decodedMessage[2], out Padlocks parsedLockType)) {
            // get our payload
            PlayerPayload playerPayload; // get player payload
            UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
            string[] nameParts = decodedMessage[4].Split(' ');
            decodedMessage[4] = nameParts[0] + " " + nameParts[1];
            // if the lock type is a mistress padlock, make sure the assigner is a mistress
            _config.padlockIdentifier[layer-1].SetType(parsedLockType); // set the type of the padlock
            _lockManager.Lock((layer-1), decodedMessage[4], decodedMessage[3], decodedMessage[5], playerPayload.PlayerName);
            // if we reached this point, it means we sucessfully locked the layer
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{decodedMessage[4]} locked your " +
            $"{_config.playerInfo._selectedGagTypes[layer-1]} with a {_config.playerInfo._selectedGagPadlocks[layer-1]}.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag lock");
            return true; // sucessful parse
        } else {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid /gag lock parameters sent in!");
        }
    }

    /// <summary>
    /// handle the unlock message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure we have a lock on
        if (_config.playerInfo._selectedGagPadlocks[layer-1] == Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: No lock applied for layer {layer}, cannot remove lock!");}
        // if we made it here, we can try to unlock it.
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        string[] nameParts = decodedMessage[4].Split(' ');
        decodedMessage[4] = nameParts[0] + " " + nameParts[1];
        // try to unlock it
        if(_config.playerInfo._selectedGagPadlocks[layer-1] == Padlocks.MistressPadlock || _config.playerInfo._selectedGagPadlocks[layer-1] == Padlocks.MistressTimerPadlock) {
            if(decodedMessage[4] != _config.playerInfo._selectedGagPadlockAssigner[layer-1]) {
                isHandled = true; return LogError($"[MsgResultLogic]: {decodedMessage[4]} is not the assigner of the lock on layer {layer}!");
            }
        }

        Padlocks tempPadlock = _config.playerInfo._selectedGagPadlocks[layer-1]; // store the padlock
        _lockManager.Unlock((layer-1), decodedMessage[4], decodedMessage[3], playerPayload.PlayerName, playerPayload.PlayerName); // attempt to unlock
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{decodedMessage[4]} " +
        $"sucessfully unlocked the {Enum.GetName(typeof(Padlocks), tempPadlock)} from your {_config.playerInfo._selectedGagTypes[layer-1]}.").AddItalicsOff().BuiltString);        
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag unlock");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the remove message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleRemoveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure that this layer has a gag on it
        if (_config.playerInfo._selectedGagTypes[layer-1] == "None") {
            isHandled = true; return LogError($"[MsgResultLogic]: There is no gag applied for gag layer {layer}, so no gag can be removed.");}
        // third, make sure there is no lock on that gags layer
        if (_config.playerInfo._selectedGagPadlocks[layer-1] != Padlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: There is a lock applied for gag layer {layer}, cannot remove gag!");}
        // finally, if we made it here, we can remove the gag
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} removed your {_config.playerInfo._selectedGagTypes[layer-1]}, how sweet.").AddItalicsOff().BuiltString);
        _lockManager.RemoveGag(layer-1); // remove the gag
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag remove");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the removeall message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns> Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleRemoveAllMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_config.playerInfo._selectedGagPadlocks.Any(padlock => padlock != Padlocks.None)) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot remove all gags while locks are on any of them.");}
        // if we made it here, we can remove them all
        string playerNameWorld = decodedMessage[4]; 
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has removed all of your gags.").AddItalicsOff().BuiltString);
        _lockManager.RemoveAllGags(); // remove all gags
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag removeall");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the gag apply message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleApplyMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // secondly, see if our gagtype is in our list of gagtypes
        string gagName = decodedMessage[2];
        if (!_gagService._gagTypes.Any(gag => gag._gagName == gagName) && _config.playerInfo._selectedGagTypes[layer-1] != "None") {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid gag type.");}
        // if we make it here, apply the gag
        _lockManager.ApplyGag(layer-1, decodedMessage[2], decodedMessage[4]);
        // send sucessful message to chat
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You've been gagged by {playerName} with a {_config.playerInfo._selectedGagTypes[layer-1]}!").AddItalicsOff().BuiltString);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag apply");
        return true; // sucessful parse
    }
}

