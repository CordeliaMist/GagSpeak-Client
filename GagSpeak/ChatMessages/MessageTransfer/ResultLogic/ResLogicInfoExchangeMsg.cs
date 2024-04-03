using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.CharacterData;
using GagSpeak.Utility;
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
            if(AltCharHelpers.IsPlayerInWhitelist(playerName, out int whitelistCharIdx, out int CharNameIdx) 
            && _config.acceptingInfoRequests
            && !_config.processingInfoRequest)
            {
                    // only if the following is satisfied are we able to provide info. 
                    _config.SetSendInfoName(playerName + "@" + world);
                    _config.SetAcceptInfoRequests(false);
                    _config.SetprocessingInfoRequest(true);
                    _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is requesting an update on your info.. Providing..").AddItalicsOff().BuiltString);
                    // invoke the info request service
                    _infoRequestedEvent.Invoke(playerName);
                
            } else {
                return LogError($"ERROR, {playerName} requested info but you do not satisfy the criteria to provide info to them! (Plz let cordy know about this if you believe it a bug)");
            }
            GSLogger.LogType.Debug($"[MsgResultLogic]: Sucessful Logic Parse for recieving an information request message");
        } catch {
            return LogError($"ERROR, Invalid information request message parse.");
        }
        return true;
    }

    /// <summary> Handles the information provide part 1 of 4 messages below. </summary>
    public bool ResLogicProvideInfoPartOne(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        // see if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(senderName, out int whitelistCharIdx, out int CharNameIdx))
        {
            // we can assume they are, so now we need to set the infoName
            _config.SetSendInfoName(AltCharHelpers.FetchNameWorldFormatByTupleIdx(whitelistCharIdx, CharNameIdx));

            // i know this is confusing, but remember, when they send the message to them, they were sending their lean to you,
            // which means when you recieve it, that lean is really your lean to them.
            // AKA: If for them, "Your lean to them" is pet, then it would fall under, then when you recieve this,
            // you will see the field "Your lean to them" is pet, but you are the dominant. Therfore we must swap them around.
            RoleLean yourLean = _characterHandler.GetRoleLeanFromString(decodedMessageMediator.theirDynamicLean);
            RoleLean theirLean = _characterHandler.GetRoleLeanFromString(decodedMessageMediator.dynamicLean);
            // now the variables are arranged correctly for you, so update
            _characterHandler.UpdateYourStatusToThem(whitelistCharIdx, yourLean);
            _characterHandler.UpdateTheirStatusToYou(whitelistCharIdx, theirLean);
            _characterHandler.SetWhitelistSafewordUsed(whitelistCharIdx, decodedMessageMediator.safewordUsed);
            _characterHandler.SetWhitelistGrantExtendedLockTimes(whitelistCharIdx, decodedMessageMediator.extendedLockTimes);
            _characterHandler.SetWhitelistDirectChatGarblerActive(whitelistCharIdx, decodedMessageMediator.directChatGarblerActive);
            _characterHandler.SetWhitelistDirectChatGarblerLocked(whitelistCharIdx, decodedMessageMediator.directChatGarblerLocked);
            // update with gag info
            _characterHandler.UpdateWhitelistGagInfoPart1(
                senderName,
                decodedMessageMediator.layerGagName,
                decodedMessageMediator.layerPadlock,
                decodedMessageMediator.layerPassword,
                decodedMessageMediator.layerTimer,
                decodedMessageMediator.layerAssigner
            );
            GSLogger.LogType.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 1 message");
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Recieved [{senderName}]'s Information details(1/4)").AddItalicsOff().BuiltString);
            return true;
        }
        else {
            return LogError($"ERROR, Invalid information provide part 1 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartTwo(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // we will need to update all of our information for this player.
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        // see if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(senderName))
        {
            // this prevents our saveservive from being spammed with 15+ requests
            _characterHandler.UpdateWhitelistGagInfoPart2(
                senderName,
                decodedMessageMediator.layerGagName,
                decodedMessageMediator.layerPadlock,
                decodedMessageMediator.layerPassword,
                decodedMessageMediator.layerTimer,
                decodedMessageMediator.layerAssigner
            );
            GSLogger.LogType.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 2 message");
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Recieved [{senderName}]'s Information details(2/4)").AddItalicsOff().BuiltString);
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 2 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartThree(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // we will need to update all of our information for this player.
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        // see if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(senderName, out int whitelistCharIdx))
        {
            _characterHandler.SetWhitelistEnableWardrobe(whitelistCharIdx, decodedMessageMediator.isWardrobeEnabled);
            _characterHandler.SetWhitelistLockGagStorageOnGagLock(whitelistCharIdx, decodedMessageMediator.isGagStorageLockUIEnabled);
            _characterHandler.SetWhitelistEnableRestraintSets(whitelistCharIdx, decodedMessageMediator.isEnableRestraintSets);
            _characterHandler.SetWhitelistRestraintSetLocking(whitelistCharIdx, decodedMessageMediator.isRestraintSetLocking);
            _characterHandler.SetWhitelistAllowPuppeteer(whitelistCharIdx, decodedMessageMediator.isPuppeteerEnabled);
            _characterHandler.SetWhitelistTriggerPhraseForPuppeteer(whitelistCharIdx, decodedMessageMediator.triggerPhrase);
            _characterHandler.SetWhitelistTriggerPhraseStartChar(whitelistCharIdx, decodedMessageMediator.triggerStartChar);
            _characterHandler.SetWhitelistTriggerPhraseEndChar(whitelistCharIdx, decodedMessageMediator.triggerEndChar);
            _characterHandler.SetWhitelistAllowSitRequests(whitelistCharIdx, decodedMessageMediator.allowSitRequests);
            _characterHandler.SetWhitelistAllowMotionRequests(whitelistCharIdx, decodedMessageMediator.allowMotionRequests);
            _characterHandler.SetWhitelistAllowAllCommands(whitelistCharIdx, decodedMessageMediator.allowAllCommands);
            GSLogger.LogType.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 3 message");
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Recieved [{senderName}]'s Information details(3/4)").AddItalicsOff().BuiltString);
            return true;
        } else {
            return LogError($"ERROR, Invalid information provide part 3 message parse.");
        }
    }

    public bool ResLogicProvideInfoPartFour(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // we will need to update all of our information for this player.
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        // see if the player is in the whitelist
        if(AltCharHelpers.IsPlayerInWhitelist(senderName, out int whitelistCharIdx))
        {
            _characterHandler.SetWhitelistEnableToybox(whitelistCharIdx, decodedMessageMediator.isToyboxEnabled);
            _characterHandler.SetWhitelistAllowChangingToyState(whitelistCharIdx, decodedMessageMediator.isChangingToyStateAllowed);
            _characterHandler.SetWhitelistAllowIntensityControl(whitelistCharIdx, decodedMessageMediator.isIntensityControlAllowed);
            _characterHandler.SetWhitelistIntensityLevel(whitelistCharIdx, (byte)decodedMessageMediator.intensityLevel);
            _characterHandler.SetWhitelistAllowUsingPatterns(whitelistCharIdx, decodedMessageMediator.isUsingPatternsAllowed);
            _characterHandler.SetWhitelistAllowToyboxLocking(whitelistCharIdx, decodedMessageMediator.isToyboxLockingAllowed);
            _characterHandler.SetWhitelistToyIsActive(whitelistCharIdx, decodedMessageMediator.toyState);
            _characterHandler.SetWhitelistToyStepSize(whitelistCharIdx, decodedMessageMediator.toyStepCount);
            _characterHandler.SetWhitelistHardcoreSettings(whitelistCharIdx,
                decodedMessageMediator.AllowForcedFollow,
                decodedMessageMediator.ForcedFollow,
                decodedMessageMediator.AllowForcedSit,
                decodedMessageMediator.ForcedSit,
                decodedMessageMediator.AllowForcedToStay,
                decodedMessageMediator.ForcedToStay,
                decodedMessageMediator.AllowBlindfold,
                decodedMessageMediator.Blindfolded);
            _characterHandler.SetWhitelistInHardcoreMode(whitelistCharIdx, decodedMessageMediator.inHardcoreMode);
            
            GSLogger.LogType.Debug($"[MsgResultLogic]: Recieved Sucessful parse for information provide part 4 message");
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Recieved [{senderName}]'s Information details(4/4)").AddItalicsOff().BuiltString);
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
