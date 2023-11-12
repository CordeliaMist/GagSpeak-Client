using System.Collections.Generic;

namespace GagSpeak.Chat.MsgDecoder;
// a struct to hold information on whitelisted players.
public class MessageDecoder {
    public List<string> DecodeMsgToList(string recievedMessage, int encodedMsgIndex) {
        // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]
        List<string> decodedMessage = new List<string>{"", "", "", "", ""};

        // decode the /gag apply message
        if (encodedMsgIndex == 1) { // Handle the /gag base command
            decodedMessage[0] = "apply";                // Assign "apply" to decodedMessage[0]
            recievedMessage = recievedMessage.Trim('*');                               // trim off the *'s from the message
            string[] messageParts = recievedMessage.Split("over your mouth as the");   // Message = {playerPayload.PlayerName} from {playerPayload.World.Name} applies a {gagType} && {layer} layer of your concealment
            string trimmedMessage = string.Empty;                                      // setting here for future use
            messageParts[1] = messageParts[1].Replace(" layer of your concealment", "");                 // trim off the "layers of your concealment" from the message     
            decodedMessage[1] = messageParts[1].Trim(); // Assign the layer to decodedMessage[1]
            trimmedMessage = messageParts[0].Trim();                                   // trim off the extra spaces from the message
            messageParts = trimmedMessage.Split("applies a");                          // split messageParts[0] by "applies a". Message = {playerPayload.PlayerName} from {playerPayload.World.Name} && {gagType}
            decodedMessage[2] = messageParts[1].Trim(); // Assign the gagtype to decodedMessage[2]
            trimmedMessage = messageParts[0].Trim();                                   // trim off the extra spaces from the message
            messageParts = trimmedMessage.Split("from");                               // split messageParts[0] by "from". Message = {playerPayload.PlayerName} && {playerPayload.World.Name}
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // Assign messageParts[0] + " " + messageParts[1] to decodedMessage[4]
            GagSpeak.Log.Debug($"Determined Message Outcome: APPLY || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag lock
        else if (encodedMsgIndex == 2) {
            decodedMessage[0] = "lock";                 // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split("from her pocket and uses it to lock your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"Determined Message Outcome: LOCK || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag lock (password)
        else if (encodedMsgIndex == 3) {
            decodedMessage[0] = "lockPassword";         // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split("before locking your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" layer gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from her pocket and sets the combination password to");
            decodedMessage[3] = messageParts[1].Trim(); // we found password
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"Determined Message Outcome: LOCK PASSWORD || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag unlock
        else if (encodedMsgIndex == 4) {
            decodedMessage[0] = "unlock";               // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("gag layer fastened nice and tight.", "");
            string[] messageParts = recievedMessage.Split("reaches behind your neck, taking off the lock that was keeping your");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: UNLOCK || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag unlock password
        else if (encodedMsgIndex == 5) {
            decodedMessage[0] = "unlockPassword";       // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("layer gagstrap, unlocking it.", "");
            string[] messageParts = recievedMessage.Split("on your");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("reaches behind your neck and sets the password to");
            decodedMessage[3] = messageParts[1].Trim(); // we found password
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"Determined Message Outcome: UNLOCK PASSWORD || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag remove
        else if (encodedMsgIndex == 6) {
            decodedMessage[0] = "remove";               // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("gag layer strap, allowing your voice to be a little clearer.", "");
            string[] messageParts = recievedMessage.Split("reaches behind your neck and unfastens the buckle of your");
            string trimmedMessage = string.Empty;
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"Determined Message Outcome: REMOVE || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag removeall
        else if (encodedMsgIndex == 7) {
            decodedMessage[0] = "removeall";            // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.", "");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            // FINISHED DECODING THE MESSAGE
            GagSpeak.Log.Debug($"Determined Message Outcome: REMOVEALL || decodedMessage[0]: {decodedMessage[0]}, decodedMessage[1]: {decodedMessage[1]}, decodedMessage[2]: {decodedMessage[2]}, decodedMessage[3]: {decodedMessage[3]}, decodedMessage[4]: {decodedMessage[4]}");
            return decodedMessage;
        } else if (encodedMsgIndex == 8) {
    // handle the request mistress message

            return decodedMessage;
        } else if (encodedMsgIndex == 9) {
    // handle the request pet message

            return decodedMessage;
        } else if (encodedMsgIndex == 10) {
    // handle the request slave message

            return decodedMessage;
        } else if (encodedMsgIndex == 11) {
    // handle the relation removal message

            return decodedMessage;
        } else if (encodedMsgIndex == 12) {
    // handle the live chat garbler lock message

            return decodedMessage;
        } else if (encodedMsgIndex == 13) {
    // handle the information request
    
            return decodedMessage;
        } else {
            // should return a list of empty strings, letting us know it isnt any of the filters.
            decodedMessage[0] = "none";
            return decodedMessage;
        }
    }
}
