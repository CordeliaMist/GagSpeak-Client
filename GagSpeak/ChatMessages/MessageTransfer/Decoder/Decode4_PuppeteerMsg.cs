using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodePuppeteerMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // decoder for if the whitelisted user is toggling your permissions for allowing sit requests [ ID == 27 ]
        if(decodedMessageMediator.encodedMsgIndex == 27) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) approached their submissive\, ""Say now my love\, how would you like to grant me access to control where you can and cant sit down\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleOnlySitRequestOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: toggle only sit request option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle only sit request option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is toggling your permissions for allowing motion requests [ ID == 28 ]
        else if(decodedMessageMediator.encodedMsgIndex == 28) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) approached their submissive\, ""Say now my love\, how would you like to submit yourself to move about and dance for me whenever I say the word\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleOnlyMotionRequestOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: toggle only motion request option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle only motion request option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is toggling your permissions for allowing all commands [ ID == 29 ]
        else if(decodedMessageMediator.encodedMsgIndex == 29) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) approached their submissive\, ""We both know you\'ve submitted yourself to me fully\, so why not accept that you\'ll do whatever I say without a second thought\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleAllCommandsOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: toggle all commands option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle all commands option: Failed to decode message: {recievedMessage}");
            }
        }
    }
}
