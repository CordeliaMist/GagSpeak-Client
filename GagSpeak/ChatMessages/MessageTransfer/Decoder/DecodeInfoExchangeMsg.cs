using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    private string AssignMatchValue(Match match, string groupName, string trueCondition)
    {
        return match.Groups[groupName].Value.Trim() == trueCondition ? "true" : "false";
    }
    /// <summary> decodes the recieved and sent messages related to the information exchange with the player.
    public void DecodeInfoExchangeMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {        
        // $"({playerPayload.PlayerName} from {playerPayload.World.Name} would enjoy it if you started our scene together by reminding them of all the various states you were left in, before we took a break from things for awhile~)";
        // decoder for requesting information from whitelisted player. [ ID == 36 ]
        if (encodedMsgIndex == 36) {
            // define the pattern using regular expressions
            string pattern = @"^\((?<playerInfo>.+) would enjoy it if you started our scene together by reminding them of all the various states you were left in\, before we took a break from things for awhile\~\)$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "requestInfo"; // assign "requestInfo" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: request info: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request info: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for sharing info about player (part 1)
        else if (encodedMsgIndex == 37) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+)\, their (?<theirStatusToYou>.+)\'s (?<yourStatusToThem>.+) nodded in agreement\, informing their partner of how when they last played together\, (?<safewordUsed>.+)\. (?<extendedLocks>.+)\, (?<gaggedVoice>.+)\, (?<sealedLips>.+)\. \-\>$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "shareInfoPartOne"; // assign "shareInfo" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // decodedMessage[1]
                decodedMessage[2] = match.Groups["theirStatusToYou"].Value.Trim(); // Assign theirStatusToYou to decodedMessage[2]
                decodedMessage[3] = match.Groups["yourStatusToThem"].Value.Trim(); // Assign yourStatusToThem to decodedMessage[3]
                // Assign safeword to decodedMessage[4]
                decodedMessage[4] = AssignMatchValue(match, "safewordUsed", "they had used their safeword");
                decodedMessage[5] = AssignMatchValue(match, "extendedLocks", "They didnt mind the enduring binds");
                decodedMessage[6] = AssignMatchValue(match, "gaggedVoice", "and they certain enjoyed their gagged voice");
                decodedMessage[7] = AssignMatchValue(match, "sealedLips", "for even now their lips were sealed tight");
                GagSpeak.Log.Debug($"[Message Decoder]: share info1: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} || "+
                $"(2) {decodedMessage[2]} || (3) {decodedMessage[3]} || (4) {decodedMessage[4]} || (5) {decodedMessage[5]} || "+
                $"(6) {decodedMessage[6]} || (7) {decodedMessage[7]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for sharing info about player (part 2)
        else if (encodedMsgIndex == 38) {
            // Split the message into substrings for each layer
            string pattern = @"\/tell (?<targetPlayer>\w+) \|\| When they had last played\, (?<layerInfo>.*?)(?= ->)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(recievedMessage);

            if (match.Success) {
                decodedMessage[0] = "shareInfoPartTwo"; // assign "shareInfo2" to decodedMessage[0]
                decodedMessage[1] = match.Groups["targetPlayer"].Value.Trim(); // PlayerInfo already assigned, dont need to worry.
                string[] layerInfoParts = match.Groups["layerInfo"].Value.Trim().Split(". ");
                for (int i = 0; i < 3; i++) {
                    // store the parts we want to look for
                    string layerName = i == 0 ? "undermost" : i == 1 ? "main" : "uppermost";
                    string startingWords = i == 0 ? "On her " + layerName + " layer, " : i == 1 ? "Over their mouths " + layerName + " layer, " : "Finally on her " + layerName + " layer, ";
                    // store the current regex for this section
                    string layerInfo = layerInfoParts[i];
                    // if it contains nothing present, then we know we have a blank entry.
                    if (layerInfo.Contains("nothing present")) {
                        decodedMessage[8 + i] = "None"; // gag type, [8, 9, 10]
                        decodedMessage[11+ i] = "None"; // padlock type, [11, 12, 13]
                        decodedMessage[17+ i] = DateTimeOffset.Now.ToString(); // timer, [17, 18, 19]
                        decodedMessage[20+ i] = ""; // assigner, [20, 21, 22]
                    } else {
                        // otherwise, check what the gagtype was.
                        decodedMessage[8 +i] = layerInfo.Split("she had a ")[1].Split(" fastened in good and tight")[0].Trim();
                        if (layerInfo.Contains("locked with a")) {
                            decodedMessage[11+ i] = layerInfo.Split("locked with a")[1].Trim().Split(",")[0].Trim();
                            if (layerInfo.Contains("which had been secured by")) {
                                decodedMessage[20+ i] = layerInfo.Split("which had been secured by")[1].Trim().Split("with")[0].Trim();
                            }
                            if (layerInfo.Contains("with") && layerInfo.Contains("remaining")) {
                                decodedMessage[17 +i] = layerInfo.Split("with")[1].Trim().Split("remaining")[0].Trim();
                            }
                            if(layerInfo.Contains("with the password")) {
                                decodedMessage[14 + i] = layerInfo.Split("with the password")[1].Trim().Split("on the lock")[0].Trim();
                            }
                        }
                    }
                }
                GagSpeak.Log.Debug($"[Message Decoder]: share info2: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} || "+
                $"(8) {decodedMessage[8]} || (9) {decodedMessage[9]} || (10) {decodedMessage[10]} || (11) {decodedMessage[11]} || "+
                $"(12) {decodedMessage[12]} || (13) {decodedMessage[13]} || (14) {decodedMessage[14]} || (15) {decodedMessage[15]} || "+
                $"(16) {decodedMessage[16]} || (17) {decodedMessage[17]} || (18) {decodedMessage[18]} || (19) {decodedMessage[19]} || "+
                $"(20) {decodedMessage[20]} || (21) {decodedMessage[21]} || (22) {decodedMessage[22]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info2: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for our sharing info about player (part 3)
        else if (encodedMsgIndex == 39) {
            // get the pattern
            string pattern = @"^\*(?<playerInfo>.+) \|\| (?<wardrobeState>.+)\. (?<gagStorageState>.+)\, (?<restraintSetEnable>.+)\. (?<restraintLock>.+)\, their partner whispering (?<puppeteerTrigger>.+)\, causing them to (?<sitRequestState>.+)\. (?<motionRequestState>.+)\, (?<allCommandsState>.+)\. (?<toyboxState>.+)\, (?<toggleToyState>.+) Within the drawer there (?<toyState>.+)\, (?<canControlIntensity>.+) currently set to (?<intensityLevel>.+)\. (?<toyPatternState>.+)\, (?<toyboxLockState>.+)$";

            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "shareInfoPartThree"; // assign "shareInfo3" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // decodedMessage[1]
                decodedMessage[23] = AssignMatchValue(match, "wardrobeState", "Their kink wardrobe was accessible for their partner");
                decodedMessage[24] = AssignMatchValue(match, "gagStorageState", "The wardrobes gag compartment was closed shut");
                decodedMessage[25] = AssignMatchValue(match, "restraintSetEnable", "and their restraint compartment was accessible for their partner");
                decodedMessage[26] = AssignMatchValue(match, "restraintLock", "They recalled their partner locking their restraints");
                decodedMessage[27] = match.Groups["puppeteerTrigger"].Value.Trim();
                decodedMessage[28] = AssignMatchValue(match, "sitRequestState", "sit down on command");
                decodedMessage[29] = AssignMatchValue(match, "motionRequestState", "For their partner controlled their movements");
                decodedMessage[30] = AssignMatchValue(match, "allCommandsState", "and all of their actions");
                decodedMessage[31] = AssignMatchValue(match, "toyboxState", "Their toybox compartment accessible to use");
                decodedMessage[32] = AssignMatchValue(match, "toggleToyState", "was powered Vibrator");
                decodedMessage[33] = AssignMatchValue(match, "canControlIntensity", "with an adjustable intensity level");
                decodedMessage[34] = match.Groups["intensityLevel"].Value.Trim();
                decodedMessage[35] = AssignMatchValue(match, "toyPatternState", "The vibrator was able to execute set patterns");
                decodedMessage[37] = AssignMatchValue(match, "toyboxLockState", "with the viberator strapped tight to their skin");
                GagSpeak.Log.Debug($"[Message Decoder]: share info3: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} || "+
                $"(23) {decodedMessage[23]} || (24) {decodedMessage[24]} || (25) {decodedMessage[25]} || (26) {decodedMessage[26]} || "+
                $"(27) {decodedMessage[27]} || (28) {decodedMessage[28]} || (29) {decodedMessage[29]} || (30) {decodedMessage[30]} || "+
                $"(31) {decodedMessage[31]} || (32) {decodedMessage[32]} || (33) {decodedMessage[33]} || (34) {decodedMessage[34]} || "+
                $"(35) {decodedMessage[35]} || (37) {decodedMessage[37]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info3: Failed to decode message: {recievedMessage}");
            }
        }
    }
}
