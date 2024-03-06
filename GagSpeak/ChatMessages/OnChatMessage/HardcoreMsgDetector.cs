using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;

namespace GagSpeak.ChatMessages;
/// <summary>
/// Used for checking messages send to the games chatbox, not meant for detouring or injection
/// Messages passed through here are scanned to see if they are encoded, for puppeteer, or include any hardcore features.
public class HardcoreMsgDetector
{
    public FFXIVClientStructs.FFXIV.Client.Game.Control.EmoteController EmoteController;
    private readonly GagSpeakConfig     _config;
    private readonly HardcoreManager    _hcManager; // hardcore manager
    private readonly CharacterHandler   _characterHandler; // character handler
    private readonly IClientState       _client; // client state
    private readonly ITargetManager     _targetManager; // target manager
    private readonly IObjectTable       _objectTable; // object table
    //private readonly HardcoreMediator _hardcoreMediator; // hardcore mediator
    
    public HardcoreMsgDetector(HardcoreManager hardcoreManager, CharacterHandler characterHandler,
    IClientState clientState, ITargetManager targetManager, IObjectTable objectTable, GagSpeakConfig config) {
        _hcManager = hardcoreManager;
        _config = config;
        _characterHandler = characterHandler;
        _client = clientState;
        _targetManager = targetManager;
        _objectTable = objectTable;
    }

    // we want to make sure that regardless of if this sends as true or false, that we perform the nessisary logic for it so long as the relative option is enabled.
    public bool IsValidMsgTrigger(string senderName, SeString chatmessage, XivChatType type, out SeString messageToSend) {
        // create the string that will be sent out
        messageToSend = new SeString();
        
        // before we process anything, let's first see if the message was from us
        if(senderName == _client.LocalPlayer!.Name.TextValue) {
            List<string> names = _characterHandler.whitelistChars
                .Where(x => x._inHardcoreMode)
                .Select(x => x._name)
                .ToList();
            // detect if the message contains any of the hardcore commands
            foreach (string name in names) {
                // follow me
                if(chatmessage.TextValue.ToLowerInvariant().Contains($"{name.Split(' ')[0].ToLowerInvariant()}, follow me.")) {
                    // get the index of the name
                    int index = _characterHandler.GetWhitelistIndex(name);
                    // toggle that players forcedfollow to true
                    _characterHandler.whitelistChars[index]._forcedFollow = true;
                    _characterHandler.Save();
                    return false; // return false to avoid processing anymore logic
                }
                // sit start
                if(chatmessage.TextValue.ToLowerInvariant().Contains($"{name.Split(' ')[0].ToLowerInvariant()}, sit.")) {
                    int index = _characterHandler.GetWhitelistIndex(name);
                    _characterHandler.whitelistChars[index]._forcedSit = true;
                    _characterHandler.Save();
                    return false;
                }
                // set end
                if(chatmessage.TextValue.ToLowerInvariant().Contains($"you may stand now {name.Split(' ')[0].ToLowerInvariant()}.")) {
                    int index = _characterHandler.GetWhitelistIndex(name);
                    _characterHandler.whitelistChars[index]._forcedSit = false;
                    _characterHandler.Save();
                    return false;
                }
                // stay here start
                if(chatmessage.TextValue.ToLowerInvariant().Contains($"{name.Split(' ')[0].ToLowerInvariant()}, stay here until i return.")) {
                    int index = _characterHandler.GetWhitelistIndex(name);
                    _characterHandler.whitelistChars[index]._forcedToStay = true;
                    _characterHandler.Save();
                    return false;
                }
                // stay here end
                if(chatmessage.TextValue.ToLowerInvariant().Contains($"thank you for waiting, {name.Split(' ')[0].ToLowerInvariant()}.")) {
                    int index = _characterHandler.GetWhitelistIndex(name);
                    _characterHandler.whitelistChars[index]._forcedToStay = false;
                    _characterHandler.Save();
                    return false;
                }
            }
            return false;
        }

        // dont process anything else if not in hardcore mode.
        if(!_config.hardcoreMode) {
            return false;
        }

        // first, let us get the index of the sender name since we already know they are in our whitelist 
        int senderIdx = _characterHandler.GetWhitelistIndex(senderName);
        // set our object to scan as the object in the same index of the list
        HC_PerPlayerConfig playerConfig = _hcManager._perPlayerConfigs[senderIdx];
        // check to see if the message even matched before performing logic
        if(playerConfig._allowForcedFollow) {
            // if the chat message contains the follow command
            if(chatmessage.TextValue.ToLowerInvariant().Contains($"{_client.LocalPlayer!.Name.ToString().Split(' ')[0].ToLowerInvariant()}, follow me.")) {
                // and nobody else currently is forcing you to follow
                if(_hcManager.IsForcedFollowingForAny(out int enabledIdx, out string playerWhoForceFollowedYou)) {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] Forced to follow is already enabled by {playerWhoForceFollowedYou}, declining.");
                    return false;
                }

                // if nobody is, then we are ok to begin.
                if (TryGetPlayerFromObjectTable(senderName, out GameObject SenderObj)) {
                    // the player is valid, and they are targetable, and we have forced to follow set to false
                    if(playerConfig._forcedFollow == false && SenderObj != null && SenderObj.IsTargetable) {
                        // we meet all the conditions to perform our logic, so we should set forced to folloow to true, locking our movement
                        _hcManager.LastMovementTime = DateTimeOffset.Now;
                        _hcManager.SetForcedFollow(senderIdx, true);
                        // then we should target the player
                        SetTarget(SenderObj);
                        // then we should execute /follow
                        messageToSend = "follow <t>";
                        // it is a valid trigger, so return true
                        return true;
                    }
                }
            }
        }

        // if allowed forced sit, then scan to see if the incoming message contains the phrase required for our forced sit
        if(playerConfig._allowForcedSit) {
            // if the message is the type to enable sitting
            if(chatmessage.TextValue.ToLowerInvariant().Contains($"{_client.LocalPlayer!.Name.ToString().Split(' ')[0].ToLowerInvariant()}, sit."))
            {
                // here, we want to make sure we are not already being forced to sit
                if(_hcManager.IsForcedSittingForAny(out int enabledIdx, out string playerWhoForceSatYou) == false) {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] Forced to sit is not enabled by anyone, but we are trying to enable it. Declining.");
                    return false;
                }
                // we need to make sure that the player who forced us to sit is the same as the sender
                GSLogger.LogType.Debug($"[HardcoreMsgDetector] {senderName} is now forcing you to sit, behave well ♥");
                _hcManager.SetForcedSit(senderIdx, true);
                // then we should execute /sit
                messageToSend = "sit";
                // it is a valid trigger, so return true
                return true;
            }
            else if(chatmessage.TextValue.ToLowerInvariant().Contains($"{_client.LocalPlayer!.Name.ToString().Split(' ')[0].ToLowerInvariant()}, on your knees."))
            {
                // here, we want to make sure we are not already being forced to sit
                if(_hcManager.IsForcedSittingForAny(out int enabledIdx, out string playerWhoForceSatYou) == false) {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] Forced to sit is not enabled by anyone, but we are trying to enable it. Declining.");
                    return false;
                }

                // we need to make sure that the player who forced us to sit is the same as the sender
                GSLogger.LogType.Debug($"[HardcoreMsgDetector] {senderName} is now forcing you to sit, behave well ♥");
                _hcManager.SetForcedSit(senderIdx, true);
                // then we should execute /sit
                messageToSend = "groundsit";
                // it is a valid trigger, so return true
                return true;
            }
            // otherwise, see if we the command is to end our forced sit
            else if(chatmessage.TextValue.ToLowerInvariant().Contains($"you may stand now {_client.LocalPlayer!.Name.ToString().Split(' ')[0].ToLowerInvariant()}."))
            {
                // if we are not forced to sit by anyone currently, exit early
                if(_hcManager.IsForcedSittingForAny(out int enabledIdx, out string playerWhoForceSatYou) == false) {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] Forced to sit is not enabled by anyone, but we are trying to release it. Declining.");
                    return false;
                }

                // we need to make sure that the player who forced us to sit is the same as the sender
                if(playerWhoForceSatYou == senderName) {
                    // then we should set forced to sit to false, unlocking our movement
                    _hcManager.SetForcedSit(senderIdx, false);
                    // while we performed the logic, we dont want to execute any chat commands, so return false
                    return false;
                } else {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] {senderName} tried to release you, but that were not the person who forced you to sit! Declining.");
                    return false;
                }
            }
        }

        // if allowed forced to stay, then scan to see if the incoming message contains the phrase required for our forced to stay
        if(playerConfig._allowForcedToStay) {
            // if the message contains the phrase to enable forced to stay
            if(chatmessage.TextValue.ToLowerInvariant().Contains($"{_client.LocalPlayer!.Name.ToString().Split(' ')[0].ToLowerInvariant()}, stay here until i return."))
            {
                // here, we want to make sure we are not already forced to stay
                if(_hcManager.IsForcedToStayForAny(out int enabledIdx, out string playerWhoForceStayedYou)) {
                    // this is true, meaning that we are already forced to stay, so we should decline the request
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] Forced to stay is already enabled by {playerWhoForceStayedYou}, declining.");
                    return false;
                }

                // if we reach here, it means we are not forced to stay by anyone, so let's enable it
                GSLogger.LogType.Debug($"[HardcoreMsgDetector] {senderName} is now forcing you to stay put in this area until they allow you to leave!");
                _hcManager.SetForcedToStay(senderIdx, true);
                return false;
            }
            // if the message contains the phrase to end forced to stay
            else if(chatmessage.TextValue.ToLowerInvariant().Contains($"thank you for waiting, {_client.LocalPlayer!.Name.ToString().Split(' ')[0].ToLowerInvariant()}."))
            {
                // if we are not forced to stay by anyone currently, exit early
                if(_hcManager.IsForcedToStayForAny(out int enabledIdx, out string playerWhoForceStayedYou) == false) {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] Forced to stay is not enabled by anyone, but we are trying to release it. Declining.");
                    return false;
                }

                // we need to make sure that the player who forced us to stay is the same as the sender
                if(playerWhoForceStayedYou == senderName) {
                    // then we should set forced to stay to false, unlocking our movement
                    _hcManager.SetForcedToStay(senderIdx, false);
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] {playerWhoForceStayedYou} has decided to release you from staying in the area.");
                    return false;
                } else {
                    GSLogger.LogType.Debug($"[HardcoreMsgDetector] {senderName} tried to release you, but that were not the person who forced you to stay! Declining.");
                    return false;
                }
            }
        }
        // if none of these were true, just return false
        return false;
    }

    private bool TryGetPlayerFromObjectTable(string senderName, out GameObject SenderObj) {
        foreach (var obj in _objectTable) {
            if (obj is PlayerCharacter pc && pc.Name.TextValue == senderName) {
                SenderObj = pc;
                return true;
            }
        }
        SenderObj = null;
        return false;
    }
    public void SetTarget(GameObject obj) {
        _targetManager.Target = obj;
    }
}