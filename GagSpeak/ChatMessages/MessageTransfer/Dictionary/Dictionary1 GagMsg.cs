namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupGagSpeakMsg(string textVal, DecodedMessageMediator decodedMessageMediator) {
        // The Apply Gag Message [ ID == 1 // apply ]
        if (textVal.Contains("applies a")
        && textVal.Contains("over your mouth as the")
        && textVal.Contains("layer of your concealment"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag apply command");
            decodedMessageMediator.encodedMsgIndex = 1;
            decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            return true;
        }

        // Encapsulated Gag Lock Dictionary Lookup, [ ID == 2,3,4 ]
        if (textVal.Contains("from")
        && textVal.Contains("takes out a")
        && ((textVal.Contains("from her pocket and sets the password to")
            && textVal.Contains("locking your") && textVal.Contains("layer gag*")) 
            || (textVal.Contains("from her pocket and uses it to lock your") && textVal.Contains("gag*"))))
        {
            // The Lock Gag Message [ ID == 4 // lockTimerPassword ]
            if(textVal.Contains("from her pocket and sets the password to") && textVal.Contains("with") && textVal.Contains("left, before locking your")) {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag lock password password2 command");
                decodedMessageMediator.encodedMsgIndex = 4;
                decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
                return true;
            }
            // The Lock Gag Message [ ID == 3 // lockPassword ]
            else if( textVal.Contains("from her pocket and sets the password to") && textVal.Contains("locking your") && textVal.Contains("layer gag*")) {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag lock password command");
                decodedMessageMediator.encodedMsgIndex = 3;
                decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
                return true;
            } 
            // The Lock Gag Message [ ID == 2 // lock ]
            else {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag lock command");
                decodedMessageMediator.encodedMsgIndex = 2;
                decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
                return true;
            }
        }

        // Encapsulated Gag Unlock Dictionary Lookup, [ ID == 5,6 ]
        if (textVal.Contains("from") && textVal.Contains("reaches behind your neck")
        && // and also either
        ((textVal.Contains("and sets the password to") && textVal.Contains("on your") && textVal.Contains("layer gagstrap, unlocking it.*"))
        || // or
        (textVal.Contains(", taking off the lock that was keeping your") && textVal.Contains("gag layer fastened nice and tight.*")))
        ) {
            // The Unlock Gag Message [ ID == 6 // unlockPassword ]
            if(textVal.Contains("and sets the password to") && textVal.Contains("on your") && textVal.Contains("layer gagstrap, unlocking it.*")) {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag unlock password command");
                decodedMessageMediator.encodedMsgIndex = 6;
                decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            }
            // The Unlock Gag Message [ ID == 5 // unlock ]
            else {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag unlock command");
                decodedMessageMediator.encodedMsgIndex = 5;
                decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            }
        } 
        
        // The gag remove message [ ID == 7 // remove ]
        if (textVal.Contains("from")
        && textVal.Contains("reaches behind your neck")
        && textVal.Contains("and unfastens the buckle of your")
        && textVal.Contains("gag layer strap, allowing your voice to be a little clearer.*"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag remove command");
            decodedMessageMediator.encodedMsgIndex = 7;
            decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            return true;
        }

        // The gag remove all message [ ID == 8 // removeAll ]
        if (textVal.Contains("from")
        && textVal.Contains("reaches behind your neck")
        && textVal.Contains("and unbuckles all of your gagstraps, allowing you to speak freely once more.*"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag remove all command");
            decodedMessageMediator.encodedMsgIndex = 8;
            decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            return true;
        }

        // The gag order "Toggle Live Garbler State" Message [ ID == 9 // toggleLiveChatGarbler ]
        if (textVal.Contains("brushes her finger overtop the gag resting over your mouth.* \"Now be a good girl and be sure to give me those sweet muffled sounds whenever you speak~\"")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing toggleLiveChatGarbler command");
            decodedMessageMediator.encodedMsgIndex = 9;
            decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            return true;
        }

        // the gag order "toggle Live Chat Garbler lock" Message [ ID == 10 // toggleLiveChatGarblerLock ]
        if (textVal.Contains("chuckles in delight of seeing their gagged submissive below them, a smile formed across their lips.* \"Look's like you'll be stuck speaking in muffled moans for some time now~\"")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing toggleLiveChatGarblerLock command");
            decodedMessageMediator.encodedMsgIndex = 10;
            decodedMessageMediator.msgType = DecodedMessageType.GagSpeak;
            return true;
        }
        
        // if no matches are found, go back (or maybe return false?)
        return false;
    }
}

