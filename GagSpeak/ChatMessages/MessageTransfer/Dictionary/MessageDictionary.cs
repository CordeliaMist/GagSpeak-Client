
namespace GagSpeak.ChatMessages.MessageTransfer;
// to quickly scan a message and see if it is one of our listed encoded messages
public partial class MessageDictionary {
    // the dictionary for looking up encoded messages
    
    // overloaded method without a mediator used for processing chat messages
    public bool LookupMsgDictionary(string textVal) {
        // Create a temporary DecodedMessageMediator object
        DecodedMessageMediator tempMediator = new DecodedMessageMediator();
        // Call the overloaded method with the temporary mediator
        return LookupMsgDictionary(textVal, tempMediator);
    }

    // dictionary used for scanning chat messages
    public bool LookupMsgDictionary(string textVal, DecodedMessageMediator decodedMessageMediator) {

        // scan through the gagspeak messages
        if(LookupGagSpeakMsg(textVal, decodedMessageMediator)) {
            // if it was one of them, we can early escape
            GagSpeak.Log.Debug($"[Message Dictionary]: Was a GagSpeak message");
            return true;
        }
        // otherwise, scan through the relation messages
        if(LookupRelationshipMsg(textVal, decodedMessageMediator)) {
            // if it was one of them, we can early escape
            GagSpeak.Log.Debug($"[Message Dictionary]: Was a Relationship message");
            return true;
        }

        if(LookupWardrobeMsg(textVal, decodedMessageMediator)) {
            // if it was one of them, we can early escape
            GagSpeak.Log.Debug($"[Message Dictionary]: Was a Wardrobe message");
            return true;
        }
    
        // otherwise, scan through the puppeteer messages
        if(LookupPuppeteerMsg(textVal, decodedMessageMediator)) {
            // if it was one of them, we can early escape
            GagSpeak.Log.Debug($"[Message Dictionary]: Was a Puppeteer message");
            return true;
        }
        // otherwise look through the toybox
        if(LookupToyboxMsg(textVal, decodedMessageMediator)) {
            // if it was one of them, we can early escape
            GagSpeak.Log.Debug($"[Message Dictionary]: Was a Toybox message");
            return true;
        }
        // finally, if it was none of those, check if it was an info exchange message
        if(LookupInfoExchangeMsg(textVal, decodedMessageMediator)) {
            // if it was one of them, we can early escape
            GagSpeak.Log.Debug($"[Message Dictionary]: Was an Info Exchange message");
            return true;
        }
        // if it was none of them, return false
        return false;
    }
}
