using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodeWardrobeMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // decoder message for toggling if the gagstorage UI will become inaccessable when a gag is locked or not [ ID == 21 ]
        if(decodedMessageMediator.encodedMsgIndex == 21) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) held their sluts chin firmly\, forcing them to look them in the eyes\* ""Let\'s make sure your locks have a little bit more security\, shall we\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleGagStorageUiLock";
                //player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: request dominant status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle gag storage UI lock: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for toggling option that allows enabling your restraint sets [ ID == 22 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(decodedMessageMediator.encodedMsgIndex == 22) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looked down at their companion before walking over to their wardrobe\, ""Now you\'ll be a good slut and not resist any restraint sets I try putting you in\, understand\?\~""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleEnableRestraintSetsOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: toggle EnableRestraintSetsOption: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle _enableRestraintSets: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for toggling option to allow locking restraint sets [ ID == 23 ]
        else if(decodedMessageMediator.encodedMsgIndex == 23) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looked down at their companion before walking over to their wardrobe\, ""Now you\'ll be a good slut and not resist any locks I try putting on your restraints\, understand\?\~""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessageMediator.encodedCmdType = "toggleAllowRestraintLockingOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                //player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: toggle AllowRestraintLockingOption: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: toggle AllowRestraintLockingOption: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for enabling a certain restraint set onto the player [ ID == 24 ]
        else if(decodedMessageMediator.encodedMsgIndex == 24) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) opens up the compartment of restraints from their wardrobe\, taking out the (?<restraintSetName>.+) and brought it back over to their slut to help secure them inside it\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "enableRestraintSet"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // restraint set name
                decodedMessageMediator.setToLockOrUnlock = match.Groups["restraintSetName"].Value.Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: enable restraint set: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} || (Restraint Set) {decodedMessageMediator.setToLockOrUnlock}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: enable restraint set: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for locking the restraint set onto the player [ ID == 25 ]
        else if(decodedMessageMediator.encodedMsgIndex == 25) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) took out a timed padlock\, and fastened it around the (?<restraintSetName>.+) on its focal point\, setting its duration to (?<timer>.+)\*$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "lockRestraintSet"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // restraint set name
                decodedMessageMediator.setToLockOrUnlock = match.Groups["restraintSetName"].Value.Trim();
                // timer
                decodedMessageMediator.layerTimer[0] = match.Groups["timer"].Value.Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: lock restraint set: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} || "+
                $"(Restraint Set) {decodedMessageMediator.setToLockOrUnlock} || (Timer) {decodedMessageMediator.layerTimer[0]}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: lock restraint set: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for unlocking the restraint set from the player [ ID == 26 ]
        else if(decodedMessageMediator.encodedMsgIndex == 26) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) decided they wanted to use their companion for other things now\, unlocking the (?<restraintSetName>.+) from their partner and allowing them to feel a little more free\, for now\~\*$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "unlockRestraintSet"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // restraint set name
                decodedMessageMediator.setToLockOrUnlock = match.Groups["restraintSetName"].Value.Trim();
                // debug result
                GSLogger.LogType.Debug($"[Message Decoder]: unlock restraint set: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} || "+
                $"(Restraint Set) {decodedMessageMediator.setToLockOrUnlock}");
            } else {
                GSLogger.LogType.Error($"[Message Decoder]: unlock restraint set: Failed to decode message: {recievedMessage}");
            }
        }
    }
}

