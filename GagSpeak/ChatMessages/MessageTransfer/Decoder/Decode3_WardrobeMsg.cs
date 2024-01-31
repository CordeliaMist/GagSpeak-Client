using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodeWardrobeMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        
        // decoder for toggling if the gagstorage UI will become inaccessable when a gag is locked or not
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [24] = Boolean for if UI should lock or not when gag is locked
        if(encodedMsgIndex == 21) {
            // stuff
        }

        // decoder for toggling the whitelisted user is allow to enable your restraint sets or not
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [25] = Boolean for if they can enable restraint sets
        else if(encodedMsgIndex == 22) {
            // stuff
        }

        // decoder for toggling the whitelisted user is allow to lock your restraint sets or not
        // [0] = commandtype, [1] = playerMsgWasSentFrom, [26] = Boolean for if they can lock restraint sets
        else if(encodedMsgIndex == 23) {
            // stuff
        }

        /////////////// Because Restraint Set locking is seperate from Gag Locking, we need to handle them seperate from the base gag commands ////////////
        
        // decoder for the restraint set enable message (Command/Button)
        else if(encodedMsgIndex == 24) {
            // stuff
        }

        // decoder for the restraint set lock message (Command/Button)
        else if (encodedMsgIndex == 25) {
            decodedMessage[0] = "restraintSetLock";     // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            string[] messageParts = recievedMessage.Split(". Bringing it over to their companion, they help secure them inside it, deciding to leave it in them for the next");
            string trimmedMessage = string.Empty;
            decodedMessage[2] = messageParts[1].Trim(); // found the timer
            
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split(" opens up the compartment of restraints from their wardrobe, taking out the");
            decodedMessage[1] = messageParts[1].Trim(); // we restraint set name

            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: /restraintset lock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoder for the restraint set unlock message (Command/Button)
        else if (encodedMsgIndex == 26) {
            decodedMessage[0] = "restraintSetUnlock";     // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("from their partner and allowing them to feel a little more free, for now~","");
            string trimmedMessage = string.Empty;
            string[] messageParts = recievedMessage.Split(" decided they wanted to use their companion for other things now, unlocking the");
            decodedMessage[1] = messageParts[1].Trim(); // we restraint set name
            trimmedMessage = messageParts[0].Trim();
            messageParts = trimmedMessage.Split("from");
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: /restraintset unlock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return;
        }
    }
}
