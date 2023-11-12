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
        } else if (textVal.Contains("from") && textVal.Contains("looks down upon you from above") &&
                   textVal.Contains("a smirk in her eyes as she sees the pleading look in your own* \"Well now darling, " +
                   "your actions speak for you well enough, so tell me, do you wish for me to become your mistress?\"")) {
            GagSpeak.Log.Debug($"THIS IS IN OUTGOING /gag request mistress ENCODED TELL");
            encodedMsgIndex = 6;
            return true;

        // the gag request pet encoded message
        } else if (textVal.Contains("from") && textVal.Contains("looks up at you") &&
                   textVal.Contains("her nervous tone clear and cheeks blushing red as she studders out the words.* \"U-um, If it's ok " +
                   "with you, could I become your pet?\"")) {
            return true;

        // the gag request slave encoded message
        } else if (textVal.Contains("from") && textVal.Contains("hears the sound of her leash's chain rattling along the floor") &&
                   textVal.Contains("as she crawls up to your feet. Stopping, looking up with pleading eyes in an embarassed tone* \"Would it be ok if I became your slave?\"")) {
            return true;

        // the gag request removal encoded message
        } else if (textVal.Contains("from") && textVal.Contains("looks up at you with tears in her eyes") &&
                   textVal.Contains("She never wanted this moment to come, but also knows due to the circumstances it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"")) {
            return true;
        
        // the gag request lock encoded message
        } else if (textVal.Contains("from") && textVal.Contains("looks down sternly at looks down sternly at the property they owned below them.") &&
                   textVal.Contains("firmly slapped her companion across the cheek and held onto her chin firmly.") && 
                   textVal.Contains("You Belong to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to take it out. Now do as I say.")) {
            return true;
        
        // the gag request info encoded message
        } else if (textVal.Contains("from") && textVal.Contains("looks down upon you with a smile,*") &&
                   textVal.Contains("I'd love to hear you describe your situation to me my dear, I want hear all about how you feel right now")) {
            return true;
        }

        // Not encoded message
        return false;
    }
}
