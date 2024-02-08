using System.Collections.Generic;
using System.Linq;
using GagSpeak.CharacterData;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    // decoder for if the whitelisted user is toggling your _enableToybox permission
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
    public bool ReslogicToggleEnableToybox(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTier(playerName);
            // make sure they have the correct tier to execute this
            if(tier == DynamicTier.Tier4) {
                // set the boolean for the new state _allowChangingToyState will be for you with them (if they can turn on/off your toy)
                _characterHandler.playerChar._allowChangingToyState[index] = !_characterHandler.playerChar._allowChangingToyState[index];
                GagSpeak.Log.Debug($"[Message Decoder]: {decodedMessage[1]} has toggled the _enableToybox permission "+
                $"to {_characterHandler.playerChar._allowChangingToyState[index]}");
                _config.Save();
                return true;
            }
            else {
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong "+
                $"enough to grant access to toggling your _enableToybox permission");
                return false;
            }
        }
        else {
            GagSpeak.Log.Error($"[Message Decoder]: {decodedMessage[1]} is not in the whitelist");
            return false;
        }
    } 
    // decoder for if the whitelisted user is starting/stopping your active toy
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
    public bool ReslogicToggleActiveToy(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTier(playerName);
            // make sure they have the correct tier to execute this
            if(tier != DynamicTier.Tier0) {
                // likely change this to an individual item later
                _characterHandler.playerChar._allowChangingToyState[index] = !_characterHandler.playerChar._allowChangingToyState[index];
                _config.Save();
                return true;
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong enough to grant access to toggling your active toy");
                return false;
            }
        } else {
            GagSpeak.Log.Error($"[Message Decoder]: {decodedMessage[1]} is not in the whitelist");
            return false;
        }
    }

    // decoder for updating the intensity of the active toy with a new intensity level
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [35] = new intensity level of the active toy
    public bool ReslogicUpdateActiveToyIntensity(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTier(playerName);
            if(tier != DynamicTier.Tier0 && tier != DynamicTier.Tier1) {
                _characterHandler.playerChar._intensityLevel = byte.Parse(decodedMessage[35]);
                GagSpeak.Log.Debug($"[Message Decoder]: {decodedMessage[1]} had its intensity updated "+
                $"to lv.{decodedMessage[35]}");
                _config.Save();
                return true;
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong "+
                "enough to grant access to updating the intensity of the active toy");
                return false;
            }
        } else {
            // return false
            GagSpeak.Log.Error($"[Message Decoder]: {decodedMessage[1]} is not in the whitelist");
            return false;
        }
    }

    // decoder for executing a stored toy pattern by its name
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [37] = name of the stored toy pattern
    public bool ReslogicExecuteStoredToyPattern(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTier(playerName);
            // make sure they have the correct tier to execute this
            if(tier == DynamicTier.Tier4 && _characterHandler.whitelistChars[index]._allowsUsingPatterns == true) {
                // execute the stored toy pattern
                if(_patternHandler.ExecutePattern(decodedMessage[37])) {
                    GagSpeak.Log.Debug($"[Message Decoder]: {decodedMessage[1]} has executed the stored toy pattern {decodedMessage[37]}");
                } else {
                    GagSpeak.Log.Error($"[Message Decoder]: {decodedMessage[1]} has failed to execute the stored toy pattern {decodedMessage[37]}");
                }
                return true;
            } else {
                // return false
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong enough to grant access to executing a stored toy pattern");
                return false;
            }
        }
        else {
            // return false
            GagSpeak.Log.Error($"[Message Decoder]: {decodedMessage[1]} is not in the whitelist");
            return false;
        }
    }

    // decoder for if the whitelisted user is toggling the lock state of the toybox UI
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
    public bool ReslogicToggleLockToyboxUI(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTier(playerName);
            // make sure they have the correct tier to execute this
            if(tier == DynamicTier.Tier4 || tier == DynamicTier.Tier3) {
                // toogle the boolean for if their toybox UI is locked or not.
                _characterHandler.playerChar._allowToyboxLocking = !_characterHandler.playerChar._allowToyboxLocking;
                _config.Save();
                return true;
            } else {
                // return false
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong enough to grant access to toggling the lock state of the toybox UI");
                return false;
            }
        }
        else {
            // return false
            GagSpeak.Log.Error($"[Message Decoder]: {decodedMessage[1]} is not in the whitelist");
            return false;
        }
    }
}