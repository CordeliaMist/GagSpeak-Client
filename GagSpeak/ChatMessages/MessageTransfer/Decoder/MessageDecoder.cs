using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message at the encodedMsgIndex into a decoded message list.
    /// <para> Will always contain the following format: </para> 
    /// <list type="bullet">
    /// <item><c>[0]</c> - The command/message type.</item>
    /// <item><c>[1]</c> - The assigner who sent it.</item>
    /// <item><c>[2]</c> - The layer index this command was meant for.</item>
    /// <item><c>[3]</c> - The DynamicLean request sent, if any.</item>
    /// <item><c>[4]</c> - if the safeword is used or not. (BOOL)</item>
    /// <item><c>[5]</c> - if they allow extendedLockTimes (BOOL)</item>
    /// <item><c>[6]</c> - if the direct chat garbler is active (BOOL)</item>
    /// <item><c>[7]</c> - if the direct chat garbler is locked (BOOL)</item>
    ///
    /// <item><c>[8]</c> - layer one gag name (or name of gag/restraint trying to be applied/locked here) </item>
    /// <item><c>[9]</c> - layer two gag name </item>
    /// <item><c>[10]</c> - layer three gag name </item>
    /// <item><c>[11]</c> - layer one padlock type (or used for lock/unlock padlock definitions) </item>
    /// <item><c>[12]</c> - layer two padlock type </item>
    /// <item><c>[13]</c> - layer three padlock type </item>
    /// <item><c>[14]</c> - layer one padlock password (or password/timer we use to try and lock/unlock) </item>
    /// <item><c>[15]</c> - layer two padlock password (or password/timer we use to try and lock/unlock) </item>
    /// <item><c>[16]</c> - layer three padlock password </item>
    /// <item><c>[17]</c> - layer one padlock timer </item>
    /// <item><c>[18]</c> - layer two padlock timer </item>
    /// <item><c>[19]</c> - layer three padlock timer </item>
    /// <item><c>[20]</c> - layer one padlock assigner (or name of person applying/locking/unlocking) </item>
    /// <item><c>[21]</c> - layer two padlock assigner </item>
    /// <item><c>[22]</c> - layer three padlock assigner </item>
    ///
    /// <item><c>[23]</c> - is wardrobe enabled (BOOL) </item>
    /// <item><c>[24]</c> - state of gag storage lock UI on gaglock? (BOOL) </item>
    /// <item><c>[25]</c> - is player allowed to enabled restraint sets? (BOOL) </item>
    /// <item><c>[26]</c> - is player allowed to lock restraint sets? (BOOL) </item>
    ///
    /// <item><c>[27]</c> - trigger phrase of messageSender for puppeteer compartment </item>
    /// <item><c>[28]</c> - does messageSender allow sit requests? (BOOL) </item>
    /// <item><c>[29]</c> - does messageSender allow motion requests? (BOOL) </item>
    /// <item><c>[30]</c> - does messageSender allow all commands? (BOOL) </item>
    ///
    /// <item><c>[31]</c> - is messageSenders toybox enabled? [_enableToybox] </item>
    /// <item><c>[32]</c> - state of active toy? ((or pending state update)) [_allowChangingToyState] </item>
    /// <item><c>[33]</c> - does messageSender allow adjusting intensity of toy? [_allowIntensityControl] </item>
    /// <item><c>[34]</c> - current intensity level of active toy ((or new intensity level being sent)) [_intensityLevel] </item>
    /// <item><c>[35]</c> - does messageSender allow you to execute storedToyPatterns? [_allowUsingPatterns] </item>
    /// <item><c>[36]</c> - name of pattern to execute (not given in infoRequests) (STRING) </item>
    /// <item><c>[37]</c> - does messageSender allow you to lock the toybox UI? [_allowToyboxLocking] </item>
    /// </list> </summary>
    /// <returns> The decoded message list. </returns>
    public List<string> DecodeMsgToList(string recievedMessage, int encodedMsgIndex) {
        // decoded messages will always contain the format: [commandtype, layer, gagtype/locktype, password, player]

        List<string> decodedMessage = new List<string>{"","","","","","","","","","","","","","","","","","","","","","","","","","","","","","","","","",""};

        // if the index is between 1 and 8, process the basic gag commands
        if(encodedMsgIndex >= 1 && encodedMsgIndex <= 10) {
            DecodeGagSpeakMsg(recievedMessage, encodedMsgIndex, ref decodedMessage);
            return decodedMessage;
        }

        // if the index is between 11 and 20, process the whitelist relation relation commands
        if(encodedMsgIndex >= 11 && encodedMsgIndex <= 20) {
            DecodeRelationshipMsg(recievedMessage, encodedMsgIndex, ref decodedMessage);
            return decodedMessage;
        }

        // if the encoded message is related to the is related to the wardrobe tab, process them here
        if(encodedMsgIndex >= 21 && encodedMsgIndex <= 26) {
            DecodeWardrobeMsg(recievedMessage, encodedMsgIndex, ref decodedMessage);
            return decodedMessage;
        }

        // if the encoded message is related to the is related to the puppeteer tab, process them here
        if(encodedMsgIndex >= 27 && encodedMsgIndex <= 29) {
            DecodePuppeteerMsg(recievedMessage, encodedMsgIndex, ref decodedMessage);
            return decodedMessage;
        }

        // if the encoded message is related to the is related to the toybox tab, process them here
        if(encodedMsgIndex >= 30 && encodedMsgIndex <= 35) {
            DecodeToyboxMsg(recievedMessage, encodedMsgIndex, ref decodedMessage);
            return decodedMessage;
        }

        // otherwise, it is a info request or recieved message, so process it here
        if(encodedMsgIndex >= 36 && encodedMsgIndex <= 39) {
            DecodeInfoExchangeMsg(recievedMessage, encodedMsgIndex, ref decodedMessage);
            return decodedMessage;
        }

        // we should never reach here, but if we do, then return the empty list
        return decodedMessage;
    }

    // Helper function for all decoders to get the correct layer out
    public string GetLayerNumber(string layer) {
        if (layer == "first")
        { 
            return "1";
        }
        else if (layer == "second")
        {
            return "2";
        }
        else if (layer == "third")
        {
            return "3";
        }
        else
        {
            return "Invalid Layer";
        }
    }
}
