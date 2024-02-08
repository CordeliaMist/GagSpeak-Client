using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the relationship of your dynamic with this person.
    public void DecodeRelationshipMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {        
        // decoder for requesting a dominant based relationship (master/mistress/owner) [ ID == 11 ]
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
        if(encodedMsgIndex == 11) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looks down upon the submissive one before them\, their pleading eyes forcing a smile across their lips\. ""I take it you would like for me to become your (?<relationType>.+)\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "requestDominantStatus"; // assign "requestDominantStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[3] = match.Groups["relationType"].Value.Trim(); // assign the relation type to decodedMessage[3]
                GagSpeak.Log.Debug($"[Message Decoder]: request dominant status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                                $"(3) {decodedMessage[3]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request dominant status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for requesting a submissive based relationship (slave/pet)
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
        else if(encodedMsgIndex == 12) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looks up at the dominant figure before them pleading eyes\, apperciating their presence deeply and desiring to grow closer towards them\.\* ""Would you please take me in as your (?<relationType>.+)\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "requestSubmissiveStatus"; // assign "requestSubmissiveStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[3] = match.Groups["relationType"].Value.Trim(); // assign the relation type to decodedMessage[3]
                GagSpeak.Log.Debug($"[Message Decoder]: request submissive status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||" +
                                $"(3) {decodedMessage[3]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request submissive status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for requesting a submission of total control (absolute-slave)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 13) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) hears the sound of her leash\'s chain rattling along the floor as she crawls up to your feet\. Stopping\, looking up with pleading eyes in an embarassed tone\* ""Would it be ok if I became your AbsoluteSlave\?""$";
            // use regex to match the pattern
            try{
                Match match = Regex.Match(recievedMessage, pattern);
                // check if the match is successful
                if (match.Success) {
                    decodedMessage[0] = "requestAbsoluteSubmissionStatus"; // assign "requestAbsoluteSubmissionStatus" to decodedMessage[0]
                    string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                    decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                    decodedMessage[3] = "AbsoluteSlave"; // assign the relation type to decodedMessage[3]
                    GagSpeak.Log.Debug($"[Message Decoder]: request absolute submission status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
                } else {
                    GagSpeak.Log.Error($"[Message Decoder]: request absolute submission status: Failed to decode message: {recievedMessage}");
                }
            } catch(System.ArgumentException e) {
                GagSpeak.Log.Error($"[Message Decoder]: Match failed: Arguement Expression error {e}\n for message: {recievedMessage}");
            } catch(System.Text.RegularExpressions.RegexMatchTimeoutException e) {
                GagSpeak.Log.Error($"[Message Decoder]: Match failed: Regex Match Timeout error {e}\n for message: {recievedMessage}");
            }
        }

        // decoder for accepting a player as your new Mistress/Master/Owner (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 14) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) nods in agreement with a smile\.\* ""Oh yes\, most certainly\. I would love for you to become my (?<relationType>.+)""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "acceptRequestDominantStatus"; // assign "acceptRequestDominantStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[3] = match.Groups["relationType"].Value.Trim(); // assign the relation type to decodedMessage[3]
                GagSpeak.Log.Debug($"[Message Decoder]: accept request dominant status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: accept request dominant status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for accepting a player as your new Pet/Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 15) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) smiles upon hearing the request and nods in agreement as their blushed companion\. Reaching down to clasp a new collar snug around their submissives neck\.\* ""Yes dearest\, I\'d love to make you my (?<relationType>.+)""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "acceptRequestSubmissiveStatus"; // assign "acceptRequestSubmissiveStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[3] = match.Groups["relationType"].Value.Trim(); // assign the relation type to decodedMessage[3]
                GagSpeak.Log.Debug($"[Message Decoder]: accept request submissive status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: accept request submissive status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for accepting a player as your new Absolute-Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
        else if(encodedMsgIndex == 16) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled\.\* ""Verywell\. And I hope you\'re able to devote yourself to the commitment of being my (?<relationType>.+)""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "acceptRequestAbsoluteSubmissionStatus"; // assign "acceptRequestAbsoluteSubmissionStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                decodedMessage[3] = match.Groups["relationType"].Value.Trim(); // assign the relation type to decodedMessage[3]
                GagSpeak.Log.Debug($"[Message Decoder]: accept request absolute submission status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: accept request absolute submission status: Failed to decode message: {recievedMessage}");
            }
        }
    
        // decoder for declining a players request to become your Mistress/Master/Owner (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 17) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) smiles gently and shakes their head\* ""I\'m sorry\, I just dont think I have enough space left in my daily life to commit to such a bond quite yet\.""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "declineRequestDominantStatus"; // assign "declineRequestDominantStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: decline request dominant status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: decline request dominant status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for declining a players request to become your Pet/Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 18) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) shakes their head from side\, ""I apologize dear\, but I don\'t think im ready to commit myself to having that kind of dynamic at the moment\.""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "declineRequestSubmissiveStatus"; // assign "declineRequestSubmissiveStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: decline request submissive status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: decline request submissive status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for declining a players request to become your Absolute-Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 19) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes a step back in surprise\, ""Oh\, I apologize\, I didnt think you wanted a commitment that heavy\.\.\. As much as I\'d love to oblige\, I dont have enough space left in my life to commit to such a thing\.""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "declineRequestAbsoluteSubmissionStatus"; // assign "declineRequestAbsoluteSubmissionStatus" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: decline request absolute submission status: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: decline request absolute submission status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for requesting a removal of relationship
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 20) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looks up at you with tears in her eyes\. She never wanted this moment to come\, but also knows due to the circumstances it was enivtable\.\* ""I\'m sorry\, but I cant keep our relationship going right now\, there is just too much going on""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                decodedMessage[0] = "sendRelationRemovalMessage"; // assign "sendRelationRemovalMessage" to decodedMessage[0]
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessage[1] = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim(); // Assign player info to decodedMessage[1]
                GagSpeak.Log.Debug($"[Message Decoder]: send relation removal message: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: send relation removal message: Failed to decode message: {recievedMessage}");
            }
        }
    }
}
