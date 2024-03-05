using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodeHardcoreMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // decoder for blindfold toggle [ ID == 43 ]
        if(decodedMessageMediator.encodedMsgIndex == 43) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) wrapped the lace blindfold nicely around your head, blocking out almost all light from your eyes\, yet still allowing just enough through to keep things exciting\*$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleBlindfold"; // assign "toggleBlindfold" as the command type
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: toggle blindfold option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle blindfold option: Failed to decode message: {recievedMessage}");
            }
        }
    }
}