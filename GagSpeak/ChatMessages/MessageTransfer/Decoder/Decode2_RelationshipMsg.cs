using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the relationship of your dynamic with this person.
    public void DecodeRelationshipMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {
        // decoder for requesting a dominant based relationship (master/mistress/owner) [ ID == 11 ]
        if(decodedMessageMediator.encodedMsgIndex == 11) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looks down upon the submissive one before them\, their pleading eyes forcing a smile across their lips\. ""I take it you would like for me to become your (?<relationType>.+)\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "requestDominantStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // relation type
                decodedMessageMediator.dynamicLean = match.Groups["relationType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: request dominant status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(RelationType) {decodedMessageMediator.dynamicLean}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request dominant status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for requesting a submissive based relationship (slave/pet)
        else if(decodedMessageMediator.encodedMsgIndex == 12) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looks up at the dominant figure before them pleading eyes\, apperciating their presence deeply and desiring to grow closer towards them\.\* ""Would you please take me in as your (?<relationType>.+)\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "requestSubmissiveStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // relation type
                decodedMessageMediator.dynamicLean = match.Groups["relationType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: request submissive status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(RelationType) {decodedMessageMediator.dynamicLean}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request submissive status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for requesting a submission of total control (absolute-slave)
        else if(decodedMessageMediator.encodedMsgIndex == 13) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) hears the sound of her leash\'s chain rattling along the floor as she crawls up to your feet\. Stopping\, looking up with pleading eyes in an embarassed tone\* ""Would it be ok if I became your AbsoluteSlave\?""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "requestAbsoluteSubmissionStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // relation type
                decodedMessageMediator.dynamicLean = "AbsoluteSlave";
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: request absolute submission status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(RelationType) {decodedMessageMediator.dynamicLean}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request absolute submission status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for accepting a player as your new Mistress/Master/Owner (relation)
        else if(decodedMessageMediator.encodedMsgIndex == 14) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) nods in agreement with a smile\.\* ""Oh yes\, most certainly\. I would love for you to become my (?<relationType>.+)""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "acceptRequestDominantStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // relation type
                decodedMessageMediator.dynamicLean = match.Groups["relationType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: accept request dominant status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(RelationType) {decodedMessageMediator.dynamicLean}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: accept request dominant status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for accepting a player as your new Pet/Slave (relation)
        else if(decodedMessageMediator.encodedMsgIndex == 15) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) smiles upon hearing the request and nods in agreement as their blushed companion\. Reaching down to clasp a new collar snug around their submissives neck\.\* ""Yes dearest\, I\'d love to make you my (?<relationType>.+)""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "acceptRequestSubmissiveStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // relation type
                decodedMessageMediator.dynamicLean = match.Groups["relationType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: accept request submissive status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(RelationType) {decodedMessageMediator.dynamicLean}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: accept request submissive status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for accepting a player as your new Absolute-Slave (relation)
        else if(decodedMessageMediator.encodedMsgIndex == 16) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled\.\* ""Verywell\. And I hope you\'re able to devote yourself to the commitment of being my (?<relationType>.+)""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "acceptRequestAbsoluteSubmissionStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // relation type
                decodedMessageMediator.dynamicLean = match.Groups["relationType"].Value.Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: accept request absolute submission status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} ||" +
                $"(RelationType) {decodedMessageMediator.dynamicLean}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: accept request absolute submission status: Failed to decode message: {recievedMessage}");
            }
        }
    
        // decoder for declining a players request to become your Mistress/Master/Owner (relation)
        else if(decodedMessageMediator.encodedMsgIndex == 17) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) smiles gently and shakes their head\* ""I\'m sorry\, I just dont think I have enough space left in my daily life to commit to such a bond quite yet\.""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "declineRequestDominantStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: decline request dominant status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: decline request dominant status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for declining a players request to become your Pet/Slave (relation)
        else if(decodedMessageMediator.encodedMsgIndex == 18) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) shakes their head from side\, ""I apologize dear\, but I don\'t think im ready to commit myself to having that kind of dynamic at the moment\.""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "declineRequestSubmissiveStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: decline request submissive status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: decline request submissive status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for declining a players request to become your Absolute-Slave (relation)
        else if(decodedMessageMediator.encodedMsgIndex == 19) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) takes a step back in surprise\, ""Oh\, I apologize\, I didnt think you wanted a commitment that heavy\.\.\. As much as I\'d love to oblige\, I dont have enough space left in my life to commit to such a thing\.""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "declineRequestAbsoluteSubmissionStatus";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: decline request absolute submission status: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: decline request absolute submission status: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for requesting a removal of relationship
        else if(decodedMessageMediator.encodedMsgIndex == 20) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+) looks up at you with tears in her eyes\. She never wanted this moment to come\, but also knows due to the circumstances it was enivtable\.\* ""I\'m sorry\, but I cant keep our relationship going right now\, there is just too much going on""$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is successful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "sendRelationRemovalMessage";
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: send relation removal message: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: send relation removal message: Failed to decode message: {recievedMessage}");
            }
        }
    }
}
