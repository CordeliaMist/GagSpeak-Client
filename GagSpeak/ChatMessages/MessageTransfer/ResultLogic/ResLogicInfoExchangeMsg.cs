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
            if(_characterHandler.IsPlayerInWhitelist(playerName)) {
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
            _characterHandler.SetWhitelistSafewordUsed(Idx, bool.Parse(decodedMessage[4]));
            _characterHandler.SetWhitelistGrantExtendedLockTimes(Idx, bool.Parse(decodedMessage[5]));
            _characterHandler.SetWhitelistDirectChatGarblerActive(Idx, bool.Parse(decodedMessage[6]));
            _characterHandler.SetWhitelistDirectChatGarblerLocked(Idx, bool.Parse(decodedMessage[7]));
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
            _characterHandler.UpdateWhitelistGagInfo(decodedMessage);
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
            _characterHandler.SetWhitelistEnableWardrobe(Idx, bool.Parse(decodedMessage[23]));
            _characterHandler.SetWhitelistLockGagStorageOnGagLock(Idx, bool.Parse(decodedMessage[24]));
            _characterHandler.SetWhitelistEnableRestraintSets(Idx, bool.Parse(decodedMessage[25]));
            _characterHandler.SetWhitelistRestraintSetLocking(Idx, bool.Parse(decodedMessage[26]));
            _characterHandler.SetWhitelistTriggerPhraseForPuppeteer(Idx, decodedMessage[27]);
            _characterHandler.SetWhitelistAllowSitRequests(Idx, bool.Parse(decodedMessage[28]));
            _characterHandler.SetWhitelistAllowMotionRequests(Idx, bool.Parse(decodedMessage[29]));
            _characterHandler.SetWhitelistAllowAllCommands(Idx, bool.Parse(decodedMessage[30]));
            _characterHandler.SetWhitelistEnableToybox(Idx, bool.Parse(decodedMessage[31]));
            _characterHandler.SetWhitelistAllowChangingToyState(Idx, bool.Parse(decodedMessage[32]));
            _characterHandler.SetWhitelistAllowIntensityControl(Idx, bool.Parse(decodedMessage[33]));
            _characterHandler.SetWhitelistIntensityLevel(Idx, byte.Parse(decodedMessage[34]));
            _characterHandler.SetWhitelistAllowUsingPatterns(Idx, bool.Parse(decodedMessage[35]));
            _characterHandler.SetWhitelistAllowToyboxLocking(Idx, bool.Parse(decodedMessage[37]));
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 3 message");
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 3 message parse.");
        }
    }
}
