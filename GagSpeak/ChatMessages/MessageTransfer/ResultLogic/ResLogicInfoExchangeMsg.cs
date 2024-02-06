using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.Gagsandlocks;
using OtterGui.Classes;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    /// <summary> Handles the information request message. </summary>
    private bool ResLogicInfoRequestMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { 
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _characterHandler.whitelistChars.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // they are in our whitelist, so set our information sender to the players name.
                _config.sendInfoName = playerName + "@" + world;
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is requesting an update on your info for the profile viewer." +
                "Providing Over the next 3 Seconds.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for recieving an information request message");

                // invoke the interaction button cooldown timer
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => {});
            }
        } catch {
            return LogError($"ERROR, Invalid information request message parse.");
        }
        return true;
    }

    /// <summary> Handles the information provide part 1-3 messages below. </summary>
    public bool ResLogicProvideInfoPartOne(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig config) {
        // we will need to update all of our information for this player.
        // this means WE are RECIEVING the provide info, so we should be updating to white player
        string senderName = decodedMessage[1];
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            // we can skip over the relationship status stuff, as that should be done by relation requests.
            _characterHandler.whitelistChars[Idx]._safewordUsed = bool.Parse(decodedMessage[4]);
            _characterHandler.whitelistChars[Idx]._grantExtendedLockTimes = bool.Parse(decodedMessage[5]);
            _characterHandler.whitelistChars[Idx]._directChatGarblerActive = bool.Parse(decodedMessage[6]);
            _characterHandler.whitelistChars[Idx]._directChatGarblerLocked = bool.Parse(decodedMessage[7]);
            config.Save();
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 1 message");
            return true;
        }
        else {
            return LogError($"ERROR, Invalid information provide part 1 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartTwo(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig config) {
        // we will need to update all of our information for this player.
        // this means WE are RECIEVING the provide info, so we should be updating to white player
        string senderName = decodedMessage[1];
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            _characterHandler.whitelistChars[Idx]._selectedGagTypes[0] = decodedMessage[8];
            _characterHandler.whitelistChars[Idx]._selectedGagTypes[1] = decodedMessage[9];
            _characterHandler.whitelistChars[Idx]._selectedGagTypes[2] = decodedMessage[10];
            _characterHandler.whitelistChars[Idx]._selectedGagPadlocks[0] = Enum.TryParse(decodedMessage[11], out Padlocks padlockType) ? padlockType : Padlocks.None;
            _characterHandler.whitelistChars[Idx]._selectedGagPadlocks[1] = Enum.TryParse(decodedMessage[12], out Padlocks padlockType2) ? padlockType2 : Padlocks.None;
            _characterHandler.whitelistChars[Idx]._selectedGagPadlocks[2] = Enum.TryParse(decodedMessage[13], out Padlocks padlockType3) ? padlockType3 : Padlocks.None;
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockPassword[0] = decodedMessage[14];
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockPassword[1] = decodedMessage[15];
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockPassword[2] = decodedMessage[16]; 
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockTimer[0] = DateTimeOffset.Parse(decodedMessage[17]);
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockTimer[1] = DateTimeOffset.Parse(decodedMessage[18]);
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockTimer[2] = DateTimeOffset.Parse(decodedMessage[19]);
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockAssigner[0] = decodedMessage[20];
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockAssigner[1] = decodedMessage[21];
            _characterHandler.whitelistChars[Idx]._selectedGagPadlockAssigner[2] = decodedMessage[22];
            config.Save();
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 2 message");
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 2 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartThree(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig config) {
        // we will need to update all of our information for this player.
        // this means WE are RECIEVING the provide info, so we should be updating to white player
        string senderName = decodedMessage[1];
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            _characterHandler.whitelistChars[Idx]._enableWardrobe               = bool.Parse(decodedMessage[23]);
            _characterHandler.whitelistChars[Idx]._lockGagStorageOnGagLock      = bool.Parse(decodedMessage[24]);
            _characterHandler.whitelistChars[Idx]._enableRestraintSets          = bool.Parse(decodedMessage[25]);
            _characterHandler.whitelistChars[Idx]._restraintSetLocking          = bool.Parse(decodedMessage[26]);
            _characterHandler.whitelistChars[Idx]._theirTriggerPhrase           = decodedMessage[27];
            _characterHandler.whitelistChars[Idx]._allowsSitRequests            = bool.Parse(decodedMessage[28]);
            _characterHandler.whitelistChars[Idx]._allowsMotionRequests         = bool.Parse(decodedMessage[29]);
            _characterHandler.whitelistChars[Idx]._allowsAllCommands            = bool.Parse(decodedMessage[30]);
            _characterHandler.whitelistChars[Idx]._enableToybox                 = bool.Parse(decodedMessage[31]);
            _characterHandler.whitelistChars[Idx]._allowsChangingToyState       = bool.Parse(decodedMessage[32]);
            _characterHandler.whitelistChars[Idx]._allowIntensityControl        = bool.Parse(decodedMessage[33]);
            _characterHandler.whitelistChars[Idx]._intensityLevel               = int.Parse(decodedMessage[34]);
            _characterHandler.whitelistChars[Idx]._allowsUsingPatterns          = bool.Parse(decodedMessage[35]);
            _characterHandler.whitelistChars[Idx]._allowToyboxLocking           = bool.Parse(decodedMessage[37]);
            config.Save();
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 3 message");
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 3 message parse.");
        }
    }
}
