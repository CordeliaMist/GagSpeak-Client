using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> Decodes the general gag based msgs. </summary>
    public void DecodeGagSpeakMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        // decode the /gag apply message
        // [0] = commandtype, [1] = GagAssigner (who sent this message), [2] = layerIndex, [8] = gagtype/gagname,
        if (encodedMsgIndex == 1) {
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag apply: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||"+
            $"(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoder for /gag lock
        // [0] = commandtype, [1] = GagAssigner (who sent this message), [2] = layerIndex, [8] = lockType,
        else if (encodedMsgIndex == 2) {
            decodedMessage[0] = "lock";                 // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split("from her pocket and uses it to lock your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer//
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                            " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: /gag lock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||"+
            $"(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoder for /gag lock (password)
        // [0] = commandtype, [1] = GagAssigner (who sent the msg), [2] = layerIndex, [8] = lockType, [11] = password/timer
        else if (encodedMsgIndex == 3) {
            decodedMessage[0] = "lockPassword";         // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split(", locking your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" layer gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from her pocket and sets the password to");
            decodedMessage[3] = messageParts[1].Trim(); // we found password
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                            " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: /gag apply: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||"+
            $"(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoded for /gag lock (password) (passwordtimer)
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex, [11] = lockType, [14] = password, [15] = timer
        else if (encodedMsgIndex == 4) {
            decodedMessage[0] = "lockTimerPassword";    // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split(" left, before locking your");
            string trimmedMessage = string.Empty;
            messageParts[1] = messageParts[1].Replace(" layer gag", "");
            decodedMessage[1] = messageParts[1].Trim(); // we found layer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split(" with ");
            decodedMessage[5] = messageParts[1].Trim(); // we found timer
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split(" from her pocket and sets the password to ");
            decodedMessage[3] = messageParts[1].Trim(); // we found password
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("takes out a ");
            decodedMessage[2] = messageParts[1].Trim(); // we found locktype
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split(" from ");
            decodedMessage[4] = messageParts[0].Trim() + 
                            " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: /gag lock password password2: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||"+
            $"(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]} ||(5) {decodedMessage[5]}");
            return;
        }

        // decoder for /gag unlock
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex
        else if (encodedMsgIndex == 5) {
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag unlock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoder for /gag unlock password
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex, [14] = password
        else if (encodedMsgIndex == 6) {
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag unlock password: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||"+
            $"(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoder for /gag remove
        // [0] = commandtype, [1] = LockAssigner, [2] = layerIndex
        else if (encodedMsgIndex == 7) {
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag remove: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(4) {decodedMessage[4]}");
            return;
        }
        // decoder for /gag removeall
        // [0] = commandtype, [1] = LockAssigner
        else if (encodedMsgIndex == 8) {
            decodedMessage[0] = "removeall";            // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace(
                "reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.", "");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: /gag removeall: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return;
        }
        // decoded for toggle/enable liveChatGarbler
        // [0] = commandtype, [1] = LockAssigner
        else if (encodedMsgIndex == 9) {
            decodedMessage[0] = "toggleLiveChatGarbler"; // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks down sternly at looks down sternly at the property they owned "+
            "below them. They firmly slapped their companion across the cheek and held onto her chin firmly.* \"You Belong "+
            "to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to "+
            "take it out. Now do as I say.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: toggle live garbler lock: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return;
        }
        // decoder for toggling the locking feature on liveChatGarbler
        // [0] = commandtype, [1] = LockAssigner
        else if (encodedMsgIndex == 10) {
            decodedMessage[0] = "toggleLiveChatGarblerLock"; // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks down sternly at looks down sternly at the property they owned "+
            "below them. They firmly slapped their companion across the cheek and held onto her chin firmly.* \"You Belong "+
            "to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to "+
            "take it out. Now do as I say.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: toggle live garbler lock: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return;
        }
    }
}

