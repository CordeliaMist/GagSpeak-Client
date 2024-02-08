using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> Decodes the general gag based msgs. </summary>
    public void DecodeGagSpeakMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        // decode the /gag apply message [ ID == 1 // apply ]
        // [0] = commandtype, [1] = GagAssigner (who sent this message), [2] = layerIndex, [8] = gagtype/gagname,
        if(encodedMsgIndex == 1) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) applies a (?<gagType>.+) over your mouth as the (?<layer>first|second|third) layer of your concealment\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "apply"; // Assign "apply" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Assign the layer to decodedMessage[2]
                decodedMessage[8] = match.Groups["gagType"].Value.Trim(); // Assign the gagtype to decodedMessage[8]
                GagSpeak.Log.Debug($"[Message Decoder]: /gag apply: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                                $"(2) {decodedMessage[2]} ||(8) {decodedMessage[8]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag apply: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag lock message [ ID == 2 // lock ]
        // [0] = commandtype, [1] = GagAssigner (who sent it), [2] = layer index, [11] = lockType,
        else if (encodedMsgIndex == 2) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes out a (?<lockType>.+) from her pocket and uses it to lock your (?<layer>first|second|third) layer\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "lock";
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Contains the layerindex
                decodedMessage[11] = match.Groups["lockType"].Value.Trim(); // contains the padlocktype name
                GagSpeak.Log.Debug($"[Message Decoder]: /gag lock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                                $"(2) {decodedMessage[2]} ||(11) {decodedMessage[11]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag lock: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag lock (password) message [ ID == 3 // lockPassword ]
        // [0] = commandtype, [1] = GagAssigner (who sent the msg), [2] = layerIndex, [11] = lockType, [14] = password/timer
        else if (encodedMsgIndex == 3) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes out a (?<lockType>.+) from (?<player>.+)\'s pocket and sets the password to (?<password>.+)\, locking your (?<layer>first|second|third) layer gag\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "lockPassword"; // Assign "lockPassword" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Contains the layerindex
                decodedMessage[11] = match.Groups["lockType"].Value.Trim(); // contains the padlocktype name
                decodedMessage[14] = match.Groups["password"].Value.Trim(); // contains the password
                GagSpeak.Log.Debug($"[Message Decoder]: /gag lock password: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                $"(2) {decodedMessage[2]} ||(11) {decodedMessage[11]} ||(14) {decodedMessage[14]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag lock password: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag lock (password) (passwordtimer) message [ ID == 4 // lockTimerPassword ]
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex, [11] = lockType, [14] = password, [15] = timer
        else if (encodedMsgIndex == 4) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes out a (?<lockType>.+) from her pocket and sets the password to (?<password>.+) with (?<timer>.+) left\, before locking your (?<layer>first|second|third) layer gag\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "lockTimerPassword"; // Assign "lockTimerPassword" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Contains the layerindex
                decodedMessage[11] = match.Groups["lockType"].Value.Trim(); // contains the padlocktype name
                decodedMessage[14] = match.Groups["password"].Value.Trim(); // contains the password
                decodedMessage[15] = match.Groups["timer"].Value.Trim(); // contains the timer
                GagSpeak.Log.Debug($"[Message Decoder]: /gag lock password timer: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                $"(2) {decodedMessage[2]} ||(11) {decodedMessage[11]} ||(14) {decodedMessage[14]} ||(15) {decodedMessage[15]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag lock password timer: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag unlock message [ ID == 5 // unlock ]
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex
        else if (encodedMsgIndex == 5) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck\, taking off the lock that was keeping your (?<layer>first|second|third) gag layer fastened nice and tight\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "unlock"; // Assign "unlock" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Contains the layerindex
                GagSpeak.Log.Debug($"[Message Decoder]: /gag unlock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                $"(2) {decodedMessage[2]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag unlock: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag unlock password message [ ID == 6 // unlockPassword ]
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex, [14] = password
        else if (encodedMsgIndex == 6) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck and sets the password to (?<password>.+) on your (?<layer>first|second|third) layer gagstrap\, unlocking it\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "unlockPassword"; // Assign "unlockPassword" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Contains the layerindex
                decodedMessage[14] = match.Groups["password"].Value.Trim(); // contains the password
                GagSpeak.Log.Debug($"[Message Decoder]: /gag unlock password: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                $"(2) {decodedMessage[2]} ||(14) {decodedMessage[14]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag unlock password: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag remove message [ ID == 7 // remove ]
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex
        else if (encodedMsgIndex == 7) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck and unfastens the buckle of your (?<layer>first|second|third) gagstrap\, allowing your voice to be a little clearer\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "remove"; // Assign "remove" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[2] = GetLayerNumber(match.Groups["layer"].Value.Trim()); // Contains the layerindex
                GagSpeak.Log.Debug($"[Message Decoder]: /gag remove: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                $"(2) {decodedMessage[2]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag remove: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag removeall message [ ID == 8 // removeall ]
        // [0] = commandtype, [1] = LockAssigner
        else if (encodedMsgIndex == 8) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck and unbuckles all of your gagstraps\, allowing you to speak freely once more\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "removeall"; // Assign "removeall" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: /gag removeall: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag removeall: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the toggle for enabling live garbler [ ID == 9 // toggleLiveChatGarbler ]
        // [0] = commandtype, [1] = ToggleAssigner
        else if (encodedMsgIndex == 9) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) brushes her finger overtop the gag resting over your mouth\.* ""Now be a good girl and be sure to give me those sweet muffled sounds whenever you speak\~""$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "toggleLiveChatGarbler"; // Assign "toggleLiveChatGarbler" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: /gag toggleLiveChatGarbler: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag toggleLiveChatGarbler: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the toggle for locking live garbler [ ID == 10 // toggleLiveChatGarblerLock ]
        // [0] = commandtype, [1] = ToggleAssigner
        else if (encodedMsgIndex == 10) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) chuckles in delight of seeing their gagged submissive below them\, a smile formed across their lips\.* ""Look's like you'll be stuck speaking in muffled moans for some time now\~""$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "toggleLiveChatGarblerLock"; // Assign "toggleLiveChatGarblerLock" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: /gag toggleLiveChatGarblerLock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag toggleLiveChatGarblerLock: Failed to decode message: {recievedMessage}");
            }
        }
    }
}


