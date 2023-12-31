using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.Chat.MsgDecoder;

/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public class MessageDecoder {
    /// <summary>
    /// This function attempts to decode the recieved message if it is identified as a gag speak message, and return it as a list of strings that have its data extracted from it
    /// <list type="bullet">
    /// <item><c>recievedMessage</c><param name="recievedMessage"> - The message to be decoded.</param></item>
    /// <item><c>encodedMsgIndex</c><param name="encodedMsgIndex"> - The index of the encoded message.</param></item>
    /// </list> </summary>
    /// <returns> A list of strings that have its data extracted from it. </returns>
    public List<string> DecodeMsgToList(string recievedMessage, int encodedMsgIndex) {
        // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]
        List<string> decodedMessage = new List<string>{"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "","", ""};

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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag apply: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag lock
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag lock: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag lock (password)
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag apply: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoded for /gag lock (password) (passwordtimer)
        else if (encodedMsgIndex == 4) {
            //*{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {lockType} from her pocket and sets the password to {password} with {password2} left, before locking your {layer} layer gag*";
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag lock password password2: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(2) {decodedMessage[2]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]} ||(5) {decodedMessage[5]}");
            return decodedMessage;
        }

        // decoder for /gag unlock
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
            return decodedMessage;
        }

        // decoder for /gag unlock password
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
            GagSpeak.Log.Debug($"[Message Decoder]: /gag unlock password: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for /gag remove
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
            return decodedMessage;
        }

        // decoder for /gag removeall
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
            return decodedMessage;
        } 

        // decoder for requesting a mistress relationship
        else if (encodedMsgIndex == 9) {
            decodedMessage[0] = "requestMistressRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace(
                "looks down upon you from above, a smirk in her eyes as she sees the pleading look in your own* \"Well now darling, your actions speak for you well enough, so tell me, do you wish for me to become your mistress?\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: request mistress: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for requesting a pet relationship        
        else if (encodedMsgIndex == 10) {
            decodedMessage[0] = "requestPetRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace(
                "looks up at you, her nervous tone clear and cheeks blushing red as she studders out the words.* \"U-um, If it's ok with you, could I become your pet?\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: request pet: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for requesting a slave relationship
        else if (encodedMsgIndex == 11) {
            decodedMessage[0] = "requestSlaveRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace(
                "hears the sound of her leash's chain rattling along the floor as she crawls up to your feet. Stopping, looking up with pleading eyes in an embarassed tone* \"Would it be ok if I became your slave?\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: request slave: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for accepting a player as your new Mistress (relation)
        else if (encodedMsgIndex == 12) {
            decodedMessage[0] = "acceptMistressRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace(
                "smiles and gracefully and nods in agreement* \"Oh yes, most certainly. I would love to have you as my mistress.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() +
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: accepting a players request to become your mistress (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for accepting a player as your new pet (relation)
        else if (encodedMsgIndex == 13) {
            decodedMessage[0] = "acceptPetRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace(
                "smiles upon hearing the request and nods in agreement as their blushed companion had a collar clicked shut around their neck.* \"Yes dear, I'd love to make you my pet.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() +
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: accepting a players request to become your pet (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for accepting a player as your new slave (relation)
        else if (encodedMsgIndex == 14) {
            decodedMessage[0] = "acceptSlaveRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled.* \"Why I would love to make you my slave dearest.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() +
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]:  accepting a players request to become your slave (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for declining a players request to become your mistress (relation)
        else if (encodedMsgIndex == 15) {
            decodedMessage[0] = "declineMistressRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("smiles gently and shakes their head* \"I'm sorry miss, but I dont think I have enough space left in my daily life to commit to such a bond quite yet.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() +
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]:  declining a players request to become your Mistress (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for declining a players request to become your pet (relation)
        else if (encodedMsgIndex == 16) {
            decodedMessage[0] = "declinePetRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("shakes their head from side, \"I apologize dear, but I don't think im ready to commit myself to having a pet at the moment.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() +
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]:  declining a players request to become your pet (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for declining a players request to become your slave (relation)
        else if (encodedMsgIndex == 17) {
            decodedMessage[0] = "declineSlaveRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("takes a step back in surprise, \"Oh, I apologize, I didnt think you wanted a commitment that heavy... I Don't think im ready to have a slave just yet..\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() +
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]:  declining a players request to become your slave (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }

        // decoder for requesting a removal of relationship
        else if (encodedMsgIndex == 18) {
            decodedMessage[0] = "removePlayerRelation";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks up at you with tears in her eyes. She never wanted this moment to come, but also knows due to the circumstances it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: remove relationship: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for forcing a garbler locker
        else if (encodedMsgIndex == 19) {
            decodedMessage[0] = "orderForceGarbleLock";     // we found the command type
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks down sternly at looks down sternly at the property they owned below them. They firmly slapped their companion across the cheek and held onto her chin firmly.* \"You Belong to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to take it out. Now do as I say.\"","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: force garbler locker: (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }
        // decoder for requesting information from whitelisted player.
        else if (encodedMsgIndex == 20) {
            decodedMessage[0] = "requestInfo";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks down upon you with a smile,* \"I'd love to hear you describe your situation to me my dear, I want hear all about how you feel right now","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: request info (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return decodedMessage;
        }        
        // decoder for sharing info about player
        else if (encodedMsgIndex == 21) {
            decodedMessage[0] = "provideInfo";      // we found commandtype
            try
            {
                GagSpeak.Log.Debug($"[Message Decoder]: provideinfo recieved: {recievedMessage}");
                // Use regex to match the required information
                var match = Regex.Match(recievedMessage,
                    @"\*(.*?) from (.*?) eyes their (.*?), in a (.*?) state, silenced over (.*?) minutes, already drooling.");
                // Store the matched values in decodedMessage
                decodedMessage[0] = "provideInfo";
                decodedMessage[1] = match.Groups[4].Value == "dominant" ? "true" : "false";
                decodedMessage[3] = match.Groups[5].Value;
                decodedMessage[4] = match.Groups[1].Value + " " + match.Groups[2].Value;
                decodedMessage[5] = match.Groups[3].Value;
                // Use separate regex for each gag layer
                int droolingStartIndex = recievedMessage.IndexOf("drooling.");
                if (droolingStartIndex != -1) { recievedMessage = recievedMessage.Substring(droolingStartIndex + "drooling.".Length); }
                // for the gags
                int surfaceLayerStartIndex = recievedMessage.IndexOf("Their surfacelayer");
                string firstSubstring, secondSubstring;
                if (surfaceLayerStartIndex != -1) {
                    firstSubstring = recievedMessage.Substring(0, surfaceLayerStartIndex);
                    secondSubstring = recievedMessage.Substring(surfaceLayerStartIndex);
                } else {
                    firstSubstring = recievedMessage;
                    secondSubstring = "";
                }
                for (int i = 0; i < 2; i++) {
                    string currentMessage = i == 0 ? firstSubstring : secondSubstring;
                    // if our message contains "had nothing on it", then we know that the gag is none, so skip over and set all to defaults
                    if (currentMessage.Contains("had nothing on it")) {
                        decodedMessage[6+i] = "None";
                        decodedMessage[9+i] = "None";
                        decodedMessage[12+i] = "";
                        decodedMessage[15+i] = "0s";
                        continue;
                    }
                    int gagTypeStartIndex = currentMessage.IndexOf("sealed off by a") + 16;
                    int gagTypeEndIndex = currentMessage.IndexOf(", a");
                    if(gagTypeEndIndex == -1) { 
                        gagTypeEndIndex = currentMessage.IndexOf("."); } // throw this is no padlock is equipped instead
                    string gagTypeSubstring = currentMessage.Substring(gagTypeStartIndex, gagTypeEndIndex - gagTypeStartIndex).Trim();
                    decodedMessage[6+i] = gagTypeSubstring;
                    if (currentMessage.Contains("securing it")) {
                        int padlockStartIndex = currentMessage.IndexOf(", a") + 3;
                        int padlockEndIndex = currentMessage.IndexOf(" securing it");
                        string padlockSubstring = currentMessage.Substring(padlockStartIndex, padlockEndIndex - padlockStartIndex).Trim();
                        decodedMessage[9+i] = padlockSubstring;
                        if (currentMessage.Contains("with")) {
                            int timerStartIndex = currentMessage.IndexOf("with") + 5;
                            int timerEndIndex = currentMessage.IndexOf(" left");
                            string timerSubstring = currentMessage.Substring(timerStartIndex, timerEndIndex - timerStartIndex).Trim();
                            decodedMessage[15+i] = timerSubstring;
                        }
                        else {
                            decodedMessage[15+i] = "0s";
                        }

                        if (currentMessage.Contains("from")) {
                            int assignerStartIndex = currentMessage.IndexOf("from") + 4;
                            int assignerEndIndex = currentMessage.IndexOf(".", assignerStartIndex);
                            string assignerSubstring = currentMessage.Substring(assignerStartIndex, assignerEndIndex - assignerStartIndex).Trim();
                            decodedMessage[12+i] = assignerSubstring;
                        }
                        else {
                            decodedMessage[12+i] = "";
                        }
                    }
                    else {
                        decodedMessage[9+i] = "None";
                        decodedMessage[12+i] = "";
                        decodedMessage[15+i] = "0s";
                    }
                    int nextLayerStartIndex = currentMessage.IndexOf("Their surfacelayer");
                    if (nextLayerStartIndex != -1) { currentMessage = currentMessage.Substring(nextLayerStartIndex);}
                }
                // print result to debug
                GagSpeak.Log.Debug($"[Message Decoder]: provideinfo send: (0) = {decodedMessage[0]} ||(1) {decodedMessage[1]} ||(3) {decodedMessage[3]} ||(4) {decodedMessage[4]} ||(6) {decodedMessage[6]} || " + 
                    $"(7) {decodedMessage[7]} ||(9) {decodedMessage[9]} ||(10) {decodedMessage[10]} ||(12) {decodedMessage[12]} ||(13) {decodedMessage[13]} ||(15) {decodedMessage[15]} ||(16) {decodedMessage[16]}");               
            }
            catch {
                GagSpeak.Log.Error("Error decoding message!");
            } 
            return decodedMessage;
        }
        // its the second half of the request info message
        else if (encodedMsgIndex == 22) {
            decodedMessage[0] = "provideInfo2";      // we found commandtype
            /* decodedMessageFormat:
                [0] = commandtype
                [2] = directChatGarblerLock
                [5] = relationship
                [8] = selectedGagType3
                [11] = selectedGagPadlock3
                [14] = selectedGagPadlockAssigner3
                [17] = selectedGagPadlockTimer3 */
            try {
                decodedMessage[8] = "None";   // gagtype
                decodedMessage[11] = "None";  // padlock
                decodedMessage[14] = "";  // assigner
                decodedMessage[17] = "0s";  // timer
                // start by removing the || from the front
                recievedMessage = recievedMessage.Replace("||", "");
                if (recievedMessage.Contains("had nothing on it")) {
                    // Set layer gag info to all none
                }
                else {
                    // Extract the gag type
                    GagSpeak.Log.Debug($"[Message Decoder]: provideinfo recieved: {recievedMessage}");
                    int gagTypeStartIndex = recievedMessage.IndexOf("was covered with a") + 18;
                    int gagTypeEndIndex = recievedMessage.IndexOf(", a");
                    if(gagTypeEndIndex == -1) { // this this if no padlock is equipped instead
                        gagTypeEndIndex = recievedMessage.IndexOf(".*"); }
                    // parse out and store the gag type
                    decodedMessage[8] = recievedMessage.Substring(gagTypeStartIndex, gagTypeEndIndex - gagTypeStartIndex).Trim();  // gagtype
                    // Extract padlock information
                    if (recievedMessage.Contains("sealing it")) {
                        int padlockStartIndex = gagTypeEndIndex + 4;
                        int padlockEndIndex = recievedMessage.Contains("sealing it") ? recievedMessage.IndexOf("sealing it") : recievedMessage.IndexOf(".");
                        decodedMessage[11] = recievedMessage.Substring(padlockStartIndex, padlockEndIndex - padlockStartIndex).Trim();  // padlock
                        // Extract timer information (if present)
                        if (recievedMessage.Contains("sealing it with")) {
                            int timerStartIndex = recievedMessage.IndexOf("sealing it with") + 15;
                            int timerEndIndex = recievedMessage.Contains("left") ? recievedMessage.IndexOf("left") : recievedMessage.IndexOf(".");
                            decodedMessage[17] = recievedMessage.Substring(timerStartIndex, timerEndIndex - timerStartIndex).Trim();  // timer
                        }
                        // Extract assigner information (if present)
                        if (recievedMessage.Contains("from")) {
                            int assignerStartIndex = recievedMessage.IndexOf("from") + 4;
                            int assignerEndIndex = recievedMessage.Contains(".") ? recievedMessage.IndexOf(".") : recievedMessage.IndexOf(",");
                            decodedMessage[14] = recievedMessage.Substring(assignerStartIndex, assignerEndIndex - assignerStartIndex).Trim();  // assigner
                        }
                    }
                }
                // at this point we have reached the last bit
                // finally, we need to describe the direct chat garbler, if any
                if (recievedMessage.Contains("their strained sounds muffled by everything")) { decodedMessage[2] = "True"; } else { decodedMessage[2] = "False"; }
                GagSpeak.Log.Debug($"[Message Decoder]: provideinfo2 send: (0) = {decodedMessage[0]} ||(2) {decodedMessage[2]} ||(5) {decodedMessage[5]} ||(8) " + 
                    $"{decodedMessage[8]} ||(11) {decodedMessage[11]} ||(14) {decodedMessage[14]} ||(17) {decodedMessage[17]}");

            } 
            catch {
                GagSpeak.Log.Error("Error decoding message!");
            }
            return decodedMessage;
        }
        // its not something meant to be decoded
        else {
            // should return a list of empty strings, letting us know it isnt any of the filters.
            decodedMessage[0] = "none";
            return decodedMessage;
        }
    }
}
