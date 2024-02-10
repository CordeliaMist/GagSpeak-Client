using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.CharacterData;
using OtterGui.Classes;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    /// <summary> Handles the information request message. </summary>
    /// THIS IS FIRED WHEN YOU RECIEVE A INFO REQUEST MESSAGE, NOT WHEN YOU ARE THE ONE SENDING IT
    private bool ResLogicInfoRequestingMessage(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        try {
            // extract a sendable format to set to the sendInfoName
            string playerNameWorld = decodedMessageMediator.assignerName;
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            if(_characterHandler.IsPlayerInWhitelist(playerName)) {
                // Player has sent us an info request, so we are setting this to their name to know who we are sending it to
                _config.SetSendInfoName(playerName + "@" + world);
                // print it
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is requesting an update on your info for the profile viewer." +
                "Providing Over the next 3 Seconds.").AddItalicsOff().BuiltString);
                // log it
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for recieving an information request message");
                // invoke the interaction button cooldown timer
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => {});
            }
        } catch {
            return LogError($"ERROR, Invalid information request message parse.");
        }
        return true;
    }

    /// <summary> Handles the information provide part 1 of 4 messages below. </summary>
    public bool ResLogicProvideInfoPartOne(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // because we stored the name of who we need to send it to in our config before p.
        _config.SetprocessingInfoRequest(true);
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            // i know this is confusing, but remember, when they send the message to them, they were sending their lean to you,
            // which means when you recieve it, that lean is really your lean to them.
            // AKA: If for them, "Your lean to them" is pet, then it would fall under, then when you recieve this,
            // you will see the field "Your lean to them" is pet, but you are the dominant. Therfore we must swap them around.
            RoleLean yourLean = _characterHandler.GetRoleLeanFromString(decodedMessageMediator.theirDynamicLean);
            RoleLean theirLean = _characterHandler.GetRoleLeanFromString(decodedMessageMediator.dynamicLean);
            // now the variables are arranged correctly for you, so update
            _characterHandler.UpdateYourStatusToThem(Idx, yourLean);
            _characterHandler.UpdateTheirStatusToYou(Idx, theirLean);
            _characterHandler.SetWhitelistSafewordUsed(Idx, decodedMessageMediator.safewordUsed);
            _characterHandler.SetWhitelistGrantExtendedLockTimes(Idx, decodedMessageMediator.extendedLockTimes);
            _characterHandler.SetWhitelistDirectChatGarblerActive(Idx, decodedMessageMediator.directChatGarblerActive);
            _characterHandler.SetWhitelistDirectChatGarblerLocked(Idx, decodedMessageMediator.directChatGarblerLocked);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 1 message");
            return true;
        }
        else {
            return LogError($"ERROR, Invalid information provide part 1 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartTwo(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // we will need to update all of our information for this player.
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            // this prevents our saveservive from being spammed with 15+ requests
            _characterHandler.UpdateWhitelistGagInfo(
                senderName,
                decodedMessageMediator.layerGagName,
                decodedMessageMediator.layerPadlock,
                decodedMessageMediator.layerPassword,
                decodedMessageMediator.layerTimer,
                decodedMessageMediator.layerAssigner
            );
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 2 message");
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 2 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartThree(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // we will need to update all of our information for this player.
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            _characterHandler.SetWhitelistEnableWardrobe(Idx, decodedMessageMediator.isWardrobeEnabled);
            _characterHandler.SetWhitelistLockGagStorageOnGagLock(Idx, decodedMessageMediator.isGagStorageLockUIEnabled);
            _characterHandler.SetWhitelistEnableRestraintSets(Idx, decodedMessageMediator.isEnableRestraintSets);
            _characterHandler.SetWhitelistRestraintSetLocking(Idx, decodedMessageMediator.isRestraintSetLocking);
            _characterHandler.SetWhitelistAllowPuppeteer(Idx, decodedMessageMediator.isPuppeteerEnabled);
            _characterHandler.SetWhitelistTriggerPhraseForPuppeteer(Idx, decodedMessageMediator.triggerPhrase);
            _characterHandler.SetWhitelistTriggerPhraseStartChar(Idx, decodedMessageMediator.triggerStartChar);
            _characterHandler.SetWhitelistTriggerPhraseEndChar(Idx, decodedMessageMediator.triggerEndChar);
            _characterHandler.SetWhitelistAllowSitRequests(Idx, decodedMessageMediator.allowSitRequests);
            _characterHandler.SetWhitelistAllowMotionRequests(Idx, decodedMessageMediator.allowMotionRequests);
            _characterHandler.SetWhitelistAllowAllCommands(Idx, decodedMessageMediator.allowAllCommands);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 3 message");
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 3 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartFour(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // we will need to update all of our information for this player.
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(senderName)) {
            Idx = _characterHandler.GetWhitelistIndex(senderName);
        }
        // we have the index, so now we can update with the variables.
        if(Idx != -1) {
            _characterHandler.SetWhitelistEnableToybox(Idx, decodedMessageMediator.isToyboxEnabled);
            _characterHandler.SetWhitelistAllowChangingToyState(Idx, decodedMessageMediator.isChangingToyStateAllowed);
            _characterHandler.SetWhitelistAllowIntensityControl(Idx, decodedMessageMediator.isIntensityControlAllowed);
            _characterHandler.SetWhitelistIntensityLevel(Idx, (byte)decodedMessageMediator.intensityLevel);
            _characterHandler.SetWhitelistAllowUsingPatterns(Idx, decodedMessageMediator.isUsingPatternsAllowed);
            _characterHandler.SetWhitelistAllowToyboxLocking(Idx, decodedMessageMediator.isToyboxLockingAllowed);
            _characterHandler.SetWhitelistToyIsActive(Idx, decodedMessageMediator.toyState);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 4 message");
            // we have finished revcieving info from this person, make sure to clear the sendInfoName
            _config.SetSendInfoName("");
            _config.SetAcceptInfoRequests(true);
            _config.SetprocessingInfoRequest(false);
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 4 message parse.");
        }
    }
}
