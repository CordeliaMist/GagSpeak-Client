using System.Linq;
using GagSpeak.CharacterData;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
        
    // decoder for if the whitelisted user is toggling your permissions for allowing sit requests
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [28] = Boolean for if they allow sit requests
    public bool ReslogicToggleSitRequests(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier >= DynamicTier.Tier1) {
                // set the boolean for if they allow sit requests
                _characterHandler.ToggleAllowSitRequests(index);
                return true;
            } else {
                // return false
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to toggling all commands");
                return false;
            }
        }
        // return false
        return false;
    }

    // decoder for if the whitelisted user is toggling your permissions for allowing motion requests
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [29] = Boolean for if they allow motion requests
    public bool ReslogicToggleMotionRequests(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier >= DynamicTier.Tier2) {
                // set the boolean for if they allow motion requests
                _characterHandler.ToggleAllowMotionRequests(index);
                return true;
            } else {
                // return false
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to toggling all commands");
                return false;
            }
        }
        // return false
        return false;
    }

    // decoder for if the whitelisted user is toggling your permissions for allowing all commands
    // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [30] = Boolean for if they allow all commands
    public bool ReslogicToggleAllCommands(DecodedMessageMediator decodedMessageMediator, ref bool isHandled) {
        // get playerName
        string playerName = decodedMessageMediator.GetPlayerName(decodedMessageMediator.assignerName);
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get the dynamictier and index
            int index = _characterHandler.GetWhitelistIndex(playerName);
            DynamicTier tier = _characterHandler.GetDynamicTierNonClient(playerName);
            // make sure they have the correct tier to execute this
            if(tier == DynamicTier.Tier4) {
                // set the boolean for if they allow all commands
                _characterHandler.ToggleAllowAllCommands(index);
                return true;
            } else {
                // return false
                GSLogger.LogType.Error($"[Message Decoder]: Your dynamic with {playerName} is not strong enough to grant access to toggling all commands");
                return false;
            }
        }
        // return false
        return false;
    }
}
