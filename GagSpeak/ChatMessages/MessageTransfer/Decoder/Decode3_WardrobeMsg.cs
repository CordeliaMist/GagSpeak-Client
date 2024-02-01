using System.Collections.Generic;
using System.Text.RegularExpressions;
using GagSpeak.Data;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodeWardrobeMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        // decoder message for toggling if the gagstorage UI will become inaccessable when a gag is locked or not [ ID == 21 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        if(encodedMsgIndex == 21) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) held their sluts chin firmly, forcing them to look them in the
             eyes\* ""Let's make sure your locks have a little bit more security, shall we?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleGagStorageUiLock"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle gag storage UI lock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle gag storage UI lock: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for toggling option that allows enabling your restraint sets [ ID == 22 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 22) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looked down at their companion before walking over to their
             wardrobe, ""Now you'll be a good slut and not resist any restraint sets I try putting you in, understand?~""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleEnableRestraintSetsOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle _enableRestraintSets: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle _enableRestraintSets: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for toggling option to allow locking restraint sets [ ID == 23 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 23) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looked down at their companion before walking ove to their
             wardrobe, ""Now you'll be a good slut and not resist any locks I try putting on your restraints, understand?~""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleAllowRestraintLockingOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle _restraintSetLocking: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle _restraintSetLocking: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for enabling a certain restraint set onto the player [ ID == 24 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName
        else if(encodedMsgIndex == 24) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) opens up the compartment of restraints from their wardrobe, taking
             out the (?<restraintSetName>.+) and brought it back over to their honny to help secure them inside it.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "enableRestraintSet"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[8] = match.Groups["restraintSetName"].Value.Trim(); // assign the restraintSetName to decodedMessage[2]
                GagSpeak.Log.Debug($"[Message Decoder]: enable restraint set: "+
                $"(0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(8) {decodedMessage[8]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: enable restraint set: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for locking the restraint set onto the player [ ID == 25 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName, [14] = timer
        else if(encodedMsgIndex == 25) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) took out a timed padlock, and fastned it around the (?<restraintSetName>.+)
             on its focal point, setting its duration to (?<timer>.+)*$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "lockRestraintSet"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[8] = match.Groups["restraintSetName"].Value.Trim(); // assign the restraintSetName to decodedMessage[2]
                decodedMessage[14] = match.Groups["timer"].Value.Trim(); // assign the timer to decodedMessage[3]
                GagSpeak.Log.Debug($"[Message Decoder]: lock restraint set: "+
                $"(0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(8) {decodedMessage[8]} ||(14) {decodedMessage[14]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: lock restraint set: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder message for unlocking the restraint set from the player [ ID == 26 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [8] = restraintSetName
        else if(encodedMsgIndex == 26) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) decided they wanted to use their companion for other things now, unlocking
             the (?<restraintSetName>.+) from their partner and allowing them to feel a little more free, for now~*$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "unlockRestraintSet"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[8] = match.Groups["restraintSetName"].Value.Trim(); // assign the restraintSetName to decodedMessage[2]
                GagSpeak.Log.Debug($"[Message Decoder]: unlock restraint set: "+
                $"(0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(8) {decodedMessage[8]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: unlock restraint set: Failed to decode message: {recievedMessage}");
            }
        }
    }
}

