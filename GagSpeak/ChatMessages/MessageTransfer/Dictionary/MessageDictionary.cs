
namespace GagSpeak.ChatMessages.MessageTransfer;
// to quickly scan a message and see if it is one of our listed encoded messages
public partial class MessageDictionary {
    // personally dont know why this is here yet
    private int encodedMsgIndex = -1;

    // the dictionary for looking up encoded messages
    public bool LookupMsgDictionary(string textVal, ref int encodedMsgIndex) {

        // scan through the gagspeak messages
        if(LookupGagSpeakMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }
        
        // otherwise, scan through the relation messages
        if(LookupInfoExchangeMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }

        // otherwise, scan through the puppeteer messages
        if(LookupPuppeteerMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }

        // otherwise look through the toybox
        if(LookupToyboxMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }

        // finally, if it was none of those, check if it was an info exchange message
        if(LookupInfoExchangeMsg(textVal, ref encodedMsgIndex)) {
            // if it was one of them, we can early escape
            return true;
        }

        // if it was none of them, return false
        return false;
    }
}
