using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodeToyboxMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        // decoder for if the whitelisted user is toggling your _enableToybox permission [ ID == 30 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
        if(encodedMsgIndex == 30) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reached into the wardrobe and unlocked the lock securing their toybox drawer within the wardrobe\.""Let\'s have some fun sweetie\, mm\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleEnableToyboxOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle enable toybox option: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle enable toybox option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is starting/stopping your active toy [ ID == 31 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
        else if(encodedMsgIndex == 31) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reached into the wardrobe and pulled out a vibrator device from the compartment\, a smirk formed on her face while she returned to their pet\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleActiveToyOption"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle active toybox option: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle active toybox option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is allowed to have control over the intensity of an active toy  [ ID == 32 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
        else if(encodedMsgIndex == 32) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) reached into the wardrobe and pulled out a controlling device from the compartment\, a smirk formed on her face while she returned to their pet\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleAllowingIntensityControl"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle active toybox option: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle active toybox option: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for updating the intensity of the active toy with a new intensity level [ ID == 33 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [35] = new intensity level of the active toy
        else if(encodedMsgIndex == 33) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) adjusted the slider on the viberators surface\, altaring the intensity to a level of (?<intensityLevel>\d+)\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "updateActiveToyIntensity"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[35] = match.Groups["intensityLevel"].Value.Trim(); // Assign intensity level to decodedMessage[35]
                GagSpeak.Log.Debug($"[Message Decoder]: update active toy intensity: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(35) {decodedMessage[35]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: update active toy intensity: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for executing a stored toy pattern by its name [ ID == 34 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [37] = name of the stored toy pattern
        else if(encodedMsgIndex == 34) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) pulled out her tomestone and tappened on the (?<patternName>.+) pattern\, which had been linked to the active vibe against their body\, causing it to provide their submissive with a wonderous dose of pleasure\.$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "executeStoredToyPattern"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[37] = match.Groups["patternName"].Value.Trim(); // Assign pattern name to decodedMessage[37]
                GagSpeak.Log.Debug($"[Message Decoder]: execute stored toy pattern: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(37) {decodedMessage[37]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: execute stored toy pattern: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for if the whitelisted user is toggling the lock state of the toybox UI [ ID == 35 ]
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom
        else if(encodedMsgIndex == 35) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) wrapped a layer of durable and tight concealment over the vibe\, making it remain locked in place against their submissive\'s skin\.""Enjoy\~""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                decodedMessage[0] = "toggleLockToyboxUI"; // assign "toggleGagStorageSecurity" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: toggle lock toybox UI: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: toggle lock toybox UI: Failed to decode message: {recievedMessage}");
            }
        }
    }
}