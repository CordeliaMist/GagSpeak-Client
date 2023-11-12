// Purpose: To decode incoming messages quickly so we can know if it is a normal message or not.
// This does no additional checks beyond a string check, so we can move on as quick as possible 
// All other non-encoded messages

namespace GagSpeak.Chat.MsgDictionary;
// to quickly scan a message and see if it is one of our listed encoded messages
public static class MessageDictionary {
    // dummy overloaded func for no index passed in
    public static bool EncodedMsgDictionary(string textVal) {
        int encodedMsgIndex = 0;
        return EncodedMsgDictionary(textVal, ref encodedMsgIndex);
    }

    // determine if a message recieved is an encoded message
    public static bool EncodedMsgDictionary(string textVal, ref int encodedMsgIndex) {
        // the gag apply encoded message (1)
        if (textVal.Contains("from") && textVal.Contains("applies a")
            && textVal.Contains("over your mouth as the") && textVal.Contains("layer of your concealment*")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING/INCOMING /gag ENCODED TELL");
            encodedMsgIndex = 1; 
            return true;

        // the gag lock encoded message and lock password
        } else if (textVal.Contains("from") && textVal.Contains("takes out a") &&
                 ((textVal.Contains("from her pocket and sets the combination password to") &&
                   textVal.Contains("before locking your") && textVal.Contains("layer gag*")) || (
                   textVal.Contains("from her pocket and uses it to lock your") && textVal.Contains("gag*")))) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag lock ENCODED TELL");
            encodedMsgIndex = 2;
            return true;

        // the gag unlock and unlock password encoded message
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
                 ((textVal.Contains("and sets the password to") && textVal.Contains("on your") &&
                   textVal.Contains("layer gagstrap, unlocking it.*")) || (
                   textVal.Contains(", taking off the lock that was keeping your") &&
                   textVal.Contains("gag layer fastened nice and tight.*")))) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag unlock ENCODED TELL");
            encodedMsgIndex = 3;
            return true;

        // the gag remove encoded message
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
                   textVal.Contains("and unfastens the buckle of your") &&
                   textVal.Contains("gag layer strap, allowing your voice to be a little clearer.*")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag remove ENCODED TELL");
            encodedMsgIndex = 4;
            return true;

        // the gag removeall encoded message
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
                   textVal.Contains("and unbuckles all of your gagstraps, allowing you to speak freely once more.*")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag removeall ENCODED TELL");
            encodedMsgIndex = 5;
            return true;

        // the gag request mistress encoded message
        } else if (textVal == null){
            // Encoded message for the REQUEST MISTRESS button

            return true;

        // the gag request pet encoded message
        } else if (textVal == null){
            // Encoded message for the REQUEST TO PET button
            return true;

        // the gag request slave encoded message
        } else if (textVal == null) {
            // Encoded message for the REQUEST TO SLAVE button
        
        // the gag request removal encoded message
        } else if (textVal == null) {
            // Encoded message for the REQUEST REMOVAL button
            return true;
        
        // the gag request lock encoded message
        } else if (textVal == null) {
            // Encoded message for the LOCK LIVE CHAT GARBLED button
            return true;
        
        // the gag request unlock encoded message
        } else if (textVal == null) {
            // Encoded message for the REQUEST PLAYER INFO button
            return true;
        }

        // Not encoded message
        return false;
    }
}
