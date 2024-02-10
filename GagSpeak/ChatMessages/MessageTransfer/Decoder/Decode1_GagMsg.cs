using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> Decodes the general gag based msgs. </summary>
    public void DecodeGagSpeakMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // decode the /gag apply message [ ID == 1 // apply ]
        if(decodedMessageMediator.encodedMsgIndex == 1) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) applies a (?<gagType>.+) over your mouth as the (?<layer>first|second|third) layer of your concealment\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "apply";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); 
                // assigned layer
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // gag type (always applies to first layer gagtype for assignments (non info exchange))
                decodedMessageMediator.layerGagName[0] = match.Groups["gagType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag apply: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(LayerIdx) {decodedMessageMediator.layerIdx} ||(GagType) {decodedMessageMediator.layerGagName[0]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag apply: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag lock message [ ID == 2 // lock ]
        else if (decodedMessageMediator.encodedMsgIndex == 2) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes out a (?<lockType>.+) from her pocket and uses it to lock your (?<layer>first|second|third) gag\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "lock";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // assigned layer
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // lock type
                decodedMessageMediator.layerPadlock[0] = match.Groups["lockType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag lock: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(LayerIdx) {decodedMessageMediator.layerIdx} ||(LockType) {decodedMessageMediator.layerPadlock[0]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag lock: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag lock (password) message [ ID == 3 // lockPassword ]
        else if (decodedMessageMediator.encodedMsgIndex == 3) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes out a (?<lockType>.+) from (?<player>.+)\'s pocket and sets the password to (?<password>.+)\, locking your (?<layer>first|second|third) layer gag\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "lockPassword";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // assigned layer
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // lock type
                decodedMessageMediator.layerPadlock[0] = match.Groups["lockType"].Value.Trim();
                // password
                decodedMessageMediator.layerPassword[0] = match.Groups["password"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag lock password: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(LayerIdx) {decodedMessageMediator.layerIdx} ||(LockType) {decodedMessageMediator.layerPadlock[0]} ||" +
                $"(Password) {decodedMessageMediator.layerPassword[0]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag lock password: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag lock (password) (passwordtimer) message [ ID == 4 // lockTimerPassword ]
        else if (decodedMessageMediator.encodedMsgIndex == 4) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes out a (?<lockType>.+) from her pocket and sets the password to (?<password>.+) with (?<timer>.+) left\, before locking your (?<layer>first|second|third) layer gag\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "lockTimerPassword";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // assigned layer
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // lock type
                decodedMessageMediator.layerPadlock[0] = match.Groups["lockType"].Value.Trim();
                // password
                decodedMessageMediator.layerPassword[0] = match.Groups["password"].Value.Trim();
                // timer
                decodedMessageMediator.layerTimer[0] = match.Groups["timer"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag lock password timer: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(LayerIdx) {decodedMessageMediator.layerIdx} ||(LockType) {decodedMessageMediator.layerPadlock[0]} ||" +
                $"(Password) {decodedMessageMediator.layerPassword[0]} ||(Timer) {decodedMessageMediator.layerTimer[0]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag lock password timer: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag unlock message [ ID == 5 // unlock ]
        else if (decodedMessageMediator.encodedMsgIndex == 5) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck\, taking off the lock that was keeping your (?<layer>first|second|third) gag layer fastened nice and tight\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "unlock";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // assigned layer idx
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag unlock: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(LayerIdx) {decodedMessageMediator.layerIdx}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag unlock: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag unlock password message [ ID == 6 // unlockPassword ]
        else if (decodedMessageMediator.encodedMsgIndex == 6) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck and sets the password to (?<password>.+) on your (?<layer>first|second|third) layer gagstrap\, unlocking it\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "unlockPassword";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // assigned layer idx
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // password
                decodedMessageMediator.layerPassword[0] = match.Groups["password"].Value.Trim();
                // 
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag unlock password: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag remove message [ ID == 7 // remove ]
        else if (decodedMessageMediator.encodedMsgIndex == 7) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck and unfastens the buckle of your (?<layer>first|second|third) gagstrap\, allowing your voice to be a little clearer\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "remove";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // assigned layer idx
                decodedMessageMediator.AssignLayerIdx(match.Groups["layer"].Value.Trim());
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag remove: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(LayerIdx) {decodedMessageMediator.layerIdx}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag remove: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the /gag removeall message [ ID == 8 // removeall ]
        else if (decodedMessageMediator.encodedMsgIndex == 8) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reaches behind your neck and unbuckles all of your gagstraps\, allowing you to speak freely once more\*$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "removeall";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag removeall: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag removeall: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the toggle for enabling live garbler [ ID == 9 // toggleLiveChatGarbler ]
        else if (decodedMessageMediator.encodedMsgIndex == 9) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) brushes her finger overtop the gag resting over your mouth\.* ""Now be a good girl and be sure to give me those sweet muffled sounds whenever you speak\~""$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleLiveChatGarbler";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag toggleLiveChatGarbler: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag toggleLiveChatGarbler: Failed to decode message: {recievedMessage}");
            }
        }

        // decode the toggle for locking live garbler [ ID == 10 // toggleLiveChatGarblerLock ]
        else if (decodedMessageMediator.encodedMsgIndex == 10) {
            // Define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) chuckles in delight of seeing their gagged submissive below them\, a smile formed across their lips\.* ""Look's like you'll be stuck speaking in muffled moans for some time now\~""$";
            // Use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // Check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleLiveChatGarblerLock";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: /gag toggleLiveChatGarblerLock: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: /gag toggleLiveChatGarblerLock: Failed to decode message: {recievedMessage}");
            }
        }
    }
}


