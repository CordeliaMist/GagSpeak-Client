
namespace GagSpeak.ChatMessages.MessageTransfer;
// to quickly scan a message and see if it is one of our listed encoded messages
public partial class MessageDictionary {
    // the dictionary for looking up encoded messages
    public bool LookupMsgDictionary(string textVal, ref int encodedMsgIndex) {

        // scan through the gagspeak messages
        if(LookupGagSpeakMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }
        GagSpeak.Log.Debug($"[Message Dictionary]: Not a GagSpeak message");
        // otherwise, scan through the relation messages
        if(LookupRelationshipMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }
        GagSpeak.Log.Debug($"[Message Dictionary]: Not a Relationship message");
        // otherwise, scan through the puppeteer messages
        if(LookupPuppeteerMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }
        GagSpeak.Log.Debug($"[Message Dictionary]: Not a Puppeteer message");
        // otherwise look through the toybox
        if(LookupToyboxMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }
        GagSpeak.Log.Debug($"[Message Dictionary]: Not a Toybox message");
        // finally, if it was none of those, check if it was an info exchange message
        if(LookupInfoExchangeMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }
        GagSpeak.Log.Debug($"[Message Dictionary]: Not an Info Exchange message");
        // if it was none of them, return false
        return false;
    }
}
