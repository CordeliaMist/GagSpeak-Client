using GagSpeak.CharacterData;
using GagSpeak.Utility;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    // decoder for if the whitelisted user is toggling your _enableToybox permission
    public bool ReslogicToggleEnableToybox(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName))
        {
            // get the dynamictier
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier == DynamicTier.Tier4) {
                // set the boolean for the new state _allowChangingToyState will be for you with them (if they can turn on/off your toy)
                _characterHandler.ToggleEnableToybox();
                GSLogger.LogType.Debug($"[Message Decoder]: {playerName} has toggled the _enableToybox permission "+
                $"to {_characterHandler.playerChar._enableToybox.ToString()}");
                return true;
            }
            else {
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong "+
                $"enough to grant access to toggling your _enableToybox permission");
                return false;
            }
        }
        else {
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }
    // decoder for if the whitelisted user is starting/stopping your active toy
    public bool ReslogicToggleActiveToy(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName, out int whitelistCharIdx, out int CharNameIdx))
        {
            // get the dynamictier
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier != DynamicTier.Tier0) {
                // likely change this to an individual item later
                _characterHandler.ToggleChangeToyState(whitelistCharIdx);
                return true;
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to toggling your active toy");
                return false;
            }
        } else {
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }

    // decoder for if the whitelisted user is starting/stopping your active toy
    public bool ReslogicToggleAllowingIntensityControl(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName, out int whitelistCharIdx))
        {
            // get the dynamictier
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier >= DynamicTier.Tier3) {
                // likely change this to an individual item later
                _characterHandler.ToggleAllowIntensityControl(whitelistCharIdx);
                return true;
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to toggling your active toy");
                return false;
            }
        } else {
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }

    // decoder for updating the intensity of the active toy with a new intensity level
    public bool ReslogicUpdateActiveToyIntensity(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName, out int whitelistCharIdx))
        {
            // update the active toy's vibe intensity levels
            if(_characterHandler.playerChar._isToyActive) {
                // now check to see if our device is turned on (via the setting changeState), and if the user has the permission to change the state
                if(_characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowChangingToyState 
                && _characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowIntensityControl)
                {
                    // update the active connected vibe, if one is
                    if(_plugService.HasConnectedDevice() && _plugService.IsClientConnected() && _plugService.anyDeviceConnected) {
                        _ = _plugService.ToyboxVibrateAsync((byte)((decodedMessageMediator.intensityLevel/(double)_plugService.stepCount)*100), 20);
                    }
                    // regardless, update the intensity level
                    _characterHandler.UpdateIntensityLevel((byte)decodedMessageMediator.intensityLevel);
                    // after, update the simulated vibe volume, if it is active
                    // update our simulated toy, if active
                    if(_characterHandler.playerChar._usingSimulatedVibe) {
                        var maxval = _plugService.stepCount == 0 ? 20 : _plugService.stepCount;
                        _soundPlayer.SetVolume((float)(decodedMessageMediator.intensityLevel/(double)maxval));
                    }
                    
                    GSLogger.LogType.Debug($"[Message Decoder]: {playerName} had its intensity updated to lv.{decodedMessageMediator.intensityLevel}");
                    return true;
                } else {
                    // we do not have the permission to change the state
                    GSLogger.LogType.Error($"[Message Decoder]: {playerName} Tried to update your intensity, but you have not given them access");
                    return false;
                }
            } else {
                // we do not have the permission to change the state
                GSLogger.LogType.Error($"[Message Decoder]: Your intensity was attempted to be updated, but there is no actively connected toy");
                return false;
            }
            
        } else {
            // return false
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }

    // decoder for executing a stored toy pattern by its name
    public bool ReslogicExecuteStoredToyPattern(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName, out int whitelistCharIdx))
        {
            // get the dynamictier
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(_characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowUsingPatterns) {
                // execute the stored toy pattern
                if(_patternHandler.ExecutePattern(decodedMessageMediator.patternNameToExecute)) {
                    GSLogger.LogType.Debug($"[Message Decoder]: {playerName} has executed the stored toy pattern {decodedMessageMediator.patternNameToExecute}");
                } else {
                    GSLogger.LogType.Error($"[Message Decoder]: {playerName} has failed to execute the stored toy pattern {decodedMessageMediator.patternNameToExecute}");
                }
                return true;
            } else {
                // return false
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to executing a stored toy pattern");
                return false;
            }
        }
        else {
            // return false
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }

    // decoder for if the whitelisted user is toggling the lock state of the toybox UI
    public bool ReslogicToggleLockToyboxUI(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName))
        {
            // get the dynamictier
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier == DynamicTier.Tier4 || tier == DynamicTier.Tier3) {
                // toogle the boolean for if their toybox UI is locked or not.
                _characterHandler.ToggleToyboxUILocking();
                return true;
            } else {
                // return false
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to toggling the lock state of the toybox UI");
                return false;
            }
        }
        else {
            // return false
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }

    // res logic for turning on/off the toy
    public bool ReslogicToggleToyOnOff(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(AltCharHelpers.IsPlayerInWhitelist(playerName, out int whitelistCharIdx))
        {
            // if the sender is someone in your whitelist who has the allow changing toy state permission enabled, allow them to toggle your toys state
            if(_characterHandler.playerChar._uniquePlayerPerms[whitelistCharIdx]._allowChangingToyState) {
                // toggle the toys state
                _characterHandler.ToggleToyState();
                return true;
            } else {
                // otherwise, they dont have the permission to do so
                GSLogger.LogType.Error($"[Message Decoder]: {playerName} could not enable/disable your device because ToyState permission is not enabled");
                return false;
            }
        }
        else {
            // return false
            GSLogger.LogType.Error($"[Message Decoder]: {playerName} is not in the whitelist");
            return false;
        }
    }
}