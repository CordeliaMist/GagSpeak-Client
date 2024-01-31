using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved and sent messages related to the information exchange with the player.
    public void DecodeInfoExchangeMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {        
        // decoder for requesting information from whitelisted player.
        if (encodedMsgIndex == 35) {
            decodedMessage[0] = "requestInfo";      // we found commandtype
            recievedMessage = recievedMessage.Trim('*');
            recievedMessage = recievedMessage.Replace("looks down upon you with a smile,* \"I'd love to hear you describe your situation to me my dear, I want hear all about how you feel right now","");
            string[] messageParts = recievedMessage.Split("from");
            string trimmedMessage = string.Empty;
            decodedMessage[4] = messageParts[0].Trim() + 
                          " " + messageParts[1].Trim(); // we found player
            GagSpeak.Log.Debug($"[Message Decoder]: request info (0) = {decodedMessage[0]} ||(4) {decodedMessage[4]}");
            return;
        }

        // decoder for sharing info about player (part 1)
        else if (encodedMsgIndex == 36) {
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
            return;
        }

        // decoder for sharing info about player (part 2)
        else if (encodedMsgIndex == 37) {
            decodedMessage[0] = "provideInfo2";      // we found commandtype

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
            return;
        }
    }
}
