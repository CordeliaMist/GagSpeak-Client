using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodeToyboxMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // decoder for if the whitelisted user is toggling your _enableToybox permission [ ID == 30 ]
        if(decodedMessageMediator.encodedMsgIndex == 30) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reached into the wardrobe and unlocked the lock securing their toybox drawer within the wardrobe\.""Let\'s have some fun sweetie\, mm\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleEnableToyboxOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: toggle enable toybox option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle enable toybox option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is starting/stopping your active toy [ ID == 31 ]
        else if(decodedMessageMediator.encodedMsgIndex == 31) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reached into the wardrobe and pulled out a vibrator device from the compartment\, a smirk formed on her face while she returned to their pet\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleActiveToyOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: toggle active toybox option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle active toybox option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is allowed to have control over the intensity of an active toy  [ ID == 32 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
        else if(decodedMessageMediator.encodedMsgIndex == 32) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reached into the wardrobe and pulled out a controlling device from the compartment\, a smirk formed on her face while she returned to their pet\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleAllowingIntensityControl"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: toggle active toybox option: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle active toybox option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for updating the intensity of the active toy with a new intensity level [ ID == 33 ]
        else if(decodedMessageMediator.encodedMsgIndex == 33) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) adjusted the slider on the viberators surface\, altaring the intensity to a level of (?<intensityLevel>\d+)\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "updateActiveToyIntensity"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // intensity level
                decodedMessageMediator.intensityLevel = int.Parse(match.Groups["intensityLevel"].Value.Trim());
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: update active toy intensity: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} || (Intensity) {decodedMessageMediator.intensityLevel}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: update active toy intensity: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for executing a stored toy pattern by its name [ ID == 34 ]
        else if(decodedMessageMediator.encodedMsgIndex == 34) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) pulled out her tomestone and tappened on the (?<patternName>.+) pattern\, which had been linked to the active vibe against their body\, causing it to provide their submissive with a wonderous dose of pleasure\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "executeStoredToyPattern"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // pattern name
                decodedMessageMediator.patternNameToExecute = match.Groups["patternName"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: execute stored toy pattern: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} || (Pattern) {decodedMessageMediator.patternNameToExecute}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: execute stored toy pattern: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is toggling the lock state of the toybox UI [ ID == 35 ]
        else if(decodedMessageMediator.encodedMsgIndex == 35) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) wrapped a layer of durable and tight concealment over the vibe\, making it remain locked in place against their submissive\'s skin\.""Enjoy\~""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleLockToyboxUI"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: toggle lock toybox UI: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle lock toybox UI: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for turning on/off the device
        else if(decodedMessageMediator.encodedMsgIndex == 36) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) placed her thumb over the remote\, toggling the switch and inverted the device state\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "toggleToyOnOff"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: toggle toy on/off: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle toy on/off: Failed to decode message: {recievedMessage}");
            }
        }

    }
}