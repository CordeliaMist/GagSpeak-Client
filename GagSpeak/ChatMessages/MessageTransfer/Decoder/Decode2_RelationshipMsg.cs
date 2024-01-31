using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the relationship of your dynamic with this person.
    public void DecodeRelationshipMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {        
        // decoder for requesting a dominant based relationship (master/mistress/owner)
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
        if(encodedMsgIndex == 11) {

        }
        // decoder for requesting a submissive based relationship (slave/pet)
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
        else if(encodedMsgIndex == 12) {

        }
        // decoder for requesting a submission of total control (absolute-slave)
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
        else if(encodedMsgIndex == 13) {

        }
        // decoder for accepting a player as your new Mistress/Master/Owner (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 14) {

        }
        // decoder for accepting a player as your new Pet/Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 15) {

        }
        // decoder for accepting a player as your new Absolute-Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 16) {

        }
        // decoder for declining a players request to become your Mistress/Master/Owner (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 17) {

        }
        // decoder for declining a players request to become your Pet/Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 18) {

        }
        // decoder for declining a players request to become your Absolute-Slave (relation)
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if(encodedMsgIndex == 19) {

        }
        // decoder for requesting a removal of relationship
        // [0] = commandtype, [1] = playerMsgWasSentFrom
        else if (encodedMsgIndex == 20) {
            decodedMessage[0] = "removePlayerRelation"; // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks up at you with tears in her eyes. She never wanted this moment to come, but also knows due to the circumstances it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: remove relationship: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return;
        }
    }
}
