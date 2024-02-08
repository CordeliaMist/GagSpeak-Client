using System.Collections.Generic;
using System.Linq;
using GagSpeak.CharacterData;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
        
    // decoder for if the whitelisted user is toggling your permissions for allowing sit requests
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [28] = Boolean for if they allow sit requests
    public bool ReslogicToggleSitRequests(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
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
            if(tier >= DynamicTier.Tier1) {
                // set the boolean for if they allow sit requests
                _characterHandler.ToggleAllowSitRequests(index);
                return true;
            } else {
                // return false
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong enough to grant access to toggling all commands");
                return false;
            }
        }
        // return false
        return false;
    }

    // decoder for if the whitelisted user is toggling your permissions for allowing motion requests
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [29] = Boolean for if they allow motion requests
    public bool ReslogicToggleMotionRequests(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
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
            if(tier >= DynamicTier.Tier2) {
                // set the boolean for if they allow motion requests
                _characterHandler.ToggleAllowMotionRequests(index);
                return true;
            } else {
                // return false
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong enough to grant access to toggling all commands");
                return false;
            }
        }
        // return false
        return false;
    }

    // decoder for if the whitelisted user is toggling your permissions for allowing all commands
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [30] = Boolean for if they allow all commands
    public bool ReslogicToggleAllCommands(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
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
                // set the boolean for if they allow all commands
                _characterHandler.ToggleAllowAllCommands(index);
                return true;
            } else {
                // return false
                GagSpeak.Log.Error($"[Message Decoder]: Your dynamic with {decodedMessage[1]} is not strong enough to grant access to toggling all commands");
                return false;
            }
        }
        // return false
        return false;
    }
}
