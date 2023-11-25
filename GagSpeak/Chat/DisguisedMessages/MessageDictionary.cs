// Purpose: To decode incoming messages quickly so we can know if it is a normal message or not.
// This does no additional checks beyond a string check, so we can move on as quick as possible 
// All other non-encoded messages

namespace GagSpeak.Chat.MsgDictionary;
// to quickly scan a message and see if it is one of our listed encoded messages
public static class MessageDictionary {

    /// <summary>
    /// A dictionary of all the encoded messages we can detect, so that we know if the message we recieve if from the plugin, or anything but our plugin.
    /// <list type="bullet">
    /// <item><c>textVal</c><param name="textVal"> - The message to check.</param></item>
    /// </list> </summary>
    /// <returns>True if the message is encoded, false if not.</returns>
    public static bool EncodedMsgDictionary(string textVal) {
        int encodedMsgIndex = 0;
        return EncodedMsgDictionary(textVal, ref encodedMsgIndex);
    }

    /// <summary>
    /// A dictionary of all the encoded messages we can detect, so that we know if the message we recieve if from the plugin, or anything but our plugin.
    /// <list type="bullet">
    /// <item><c>textVal</c><param name="textVal"> - The message to check.</param></item>
    /// <item><c>encodedMsgIndex</c><param name="encodedMsgIndex"> - The index of the encoded message.</param></item>
    /// </list> </summary>
    /// <returns>True if the message is encoded, false if not.</returns>
    public static bool EncodedMsgDictionary(string textVal, ref int encodedMsgIndex) {
        // the gag apply encoded message (1)
        if (textVal.Contains("from") && textVal.Contains("applies a")
            && textVal.Contains("over your mouth as the") && textVal.Contains("layer of your concealment*")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing or incoming /gag command");
            encodedMsgIndex = 1; 
            return true;

        // the gag lock encoded message and lock password
        } else if (textVal.Contains("from") && textVal.Contains("takes out a") &&
              ((textVal.Contains("from her pocket and sets the password to") &&
                textVal.Contains("locking your") && textVal.Contains("layer gag*")) 
                || 
                (textVal.Contains("from her pocket and uses it to lock your") && textVal.Contains("gag*")))) {
            // see if we set index for password or not
            if(textVal.Contains("from her pocket and sets the password to") && textVal.Contains("with") && textVal.Contains("left, before locking your")) {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag lock password password2 command");
                encodedMsgIndex = 4; // double password lock == 4
            } else if( textVal.Contains("from her pocket and sets the password to") && textVal.Contains("locking your") && textVal.Contains("layer gag*")) {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag lock password command");
                encodedMsgIndex = 3; // password lock == 3
            } else {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag lock command");
                encodedMsgIndex = 2; // normal lock == 2
            }
            return true;

        // the gag unlock encoded message and unlock password
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
                 ((textVal.Contains("and sets the password to") && textVal.Contains("on your") &&
                   textVal.Contains("layer gagstrap, unlocking it.*")) || (
                   textVal.Contains(", taking off the lock that was keeping your") &&
                   textVal.Contains("gag layer fastened nice and tight.*")))) {
            // see if we set index for password or not
            if(textVal.Contains("and sets the password to") && textVal.Contains("on your") && textVal.Contains("layer gagstrap, unlocking it.*")) {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag unlock password command");
                encodedMsgIndex = 6; // password unlock == 5
            } else {
                GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag unlock command");
                encodedMsgIndex = 5; // normal unlock == 4
            }
            return true;

        // FOR /gag REMOVE COMMAND
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
                   textVal.Contains("and unfastens the buckle of your") &&
                   textVal.Contains("gag layer strap, allowing your voice to be a little clearer.*")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag remove command");
            encodedMsgIndex = 7;
            return true;

        // FOR /gag REMOVE ALL COMMAND
        } else if (textVal.Contains("from") && textVal.Contains("reaches behind your neck") &&
                   textVal.Contains("and unbuckles all of your gagstraps, allowing you to speak freely once more.*")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /gag remove all command");
            encodedMsgIndex = 8;
            return true;

        // REQUEST MISTRESS RELATION
        } else if (textVal.Contains("from") && textVal.Contains("looks down upon you from above") &&
                   textVal.Contains("a smirk in her eyes as she sees the pleading look in your own* \"Well now darling, " +
                   "your actions speak for you well enough, so tell me, do you wish for me to become your mistress?\"")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing request mistress interaction");
            encodedMsgIndex = 9;
            return true;

        // REQUEST PET RELATION
        } else if (textVal.Contains("from") && textVal.Contains("looks up at you") &&
                   textVal.Contains("her nervous tone clear and cheeks blushing red as she studders out the words.* \"U-um, If it's ok " +
                   "with you, could I become your pet?\"")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing request pet interaction");
            encodedMsgIndex = 10;
            return true;

        // REQUEST SLAVE RELATION
        } else if (textVal.Contains("from") && textVal.Contains("hears the sound of her leash's chain rattling along the floor") &&
                   textVal.Contains("as she crawls up to your feet. Stopping, looking up with pleading eyes in an embarassed tone* \"Would it be ok if I became your slave?\"")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing request slave interaction");
            encodedMsgIndex = 11;
            return true;

        // DECLARE A RELATION REMOVAL
        } else if (textVal.Contains("from") && textVal.Contains("looks up at you with tears in her eyes") &&
                   textVal.Contains("She never wanted this moment to come, but also knows due to the circumstances it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing relation removal");
            encodedMsgIndex = 12;
            return true;
        
        // AUTO GARBLED LOCK ENABLER
        } else if (textVal.Contains("from") && textVal.Contains("looks down sternly at looks down sternly at the property they owned below them.") &&
                   textVal.Contains("They firmly slapped their companion across the cheek and held onto her chin firmly.") && 
                   textVal.Contains("You Belong to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to take it out. Now do as I say.")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing lock garbler toggle order");
            encodedMsgIndex = 13;
            return true;
        
        // REQUEST PLAYER INFORMATION
        } else if (textVal.Contains("from") && textVal.Contains("looks down upon you with a smile,*") &&
                   textVal.Contains("I'd love to hear you describe your situation to me my dear, I want hear all about how you feel right now")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected a request player info message");
            encodedMsgIndex = 14;
            return true;
        }
        // ACCEPT MISTRESS RELATION
        else if (textVal.Contains("from") && textVal.Contains("smiles and gracefully and nods in agreement") &&
                textVal.Contains("Oh yes, most certainly. I would love to have you as my mistress.")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing accept mistress interaction");
            encodedMsgIndex = 15;
            return true;
        }

        // ACCEPT PET RELATION
        else if (textVal.Contains("from") && textVal.Contains("smiles upon hearing the request and nods in agreement as their blushed companion had a collar clicked shut around their neck.") &&
                textVal.Contains("Yes dear, I'd love to make you my pet.")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing accept pet interaction");
            encodedMsgIndex = 16;
            return true;
        }

        // ACCEPT SLAVE RELATION
        else if (textVal.Contains("from") && textVal.Contains("glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled.") &&
                textVal.Contains("Why I would love to make you my slave dearest.")) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing accept slave interaction");
            encodedMsgIndex = 17;
            return true;
        }

        // SHARE INFO
        else if (textVal.Contains("from") 
             && (textVal.Contains("eyes their"))
             && textVal.Contains("in a") && ((textVal.Contains("state, silenced over") && textVal.Contains("minutes, already drooling"))
             && textVal.Contains("Their underlayer"))) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming shareinfo part 1 interaction");
            
            encodedMsgIndex = 18;
            return true;
        }

        // SHARE INFO PART 2 ELECTRIC BOOGAGLOO
        else if (textVal.Contains("|| Finally, their topmostlayer ")
              && (textVal.Contains("had nothing on it") || textVal.Contains("was covered with a"))
              && (textVal.Contains(".*"))) {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming shareinfo part 2 interaction");
            encodedMsgIndex = 19;
            return true;
        }

        // Not encoded message
        return false;
    }
}
