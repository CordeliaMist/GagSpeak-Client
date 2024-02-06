namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    // lookup function for the relationship messages
    public bool LookupRelationshipMsg(string textVal, ref int index) {
        // IF ANY CONDITIONS ARE MET, DO AN EARLY EXIT RETURN

        // Request to have dominant status [ ID == 11 // request (Mistress/Master/Owner) ]
        if (textVal.Contains("looks down upon the submissive one before them, their pleading eyes forcing a smile across their lips. \"I take "+
        "it you would like for me to become your"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /relationship request (Mistress/Master/Owner) command");
            index = 11;
            return true;
        }

        // Request to have submissive status [ ID == 12 // request (Slave/Pet) ]
        else if (textVal.Contains("looks up at the dominant figure before them pleading eyes, apperciating their presence deeply and desiring "+
        "to grow closer towards them.* \"Would you please take me in as your"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /relationship request (Slave/Pet) command");
            index = 12;
            return true;
        }

        // Request to have absolute submission status [ ID == 13 // request (Absolute-Slave) ]
        else if (textVal.Contains("hears the sound of her leash's chain rattling along the floor as she crawls up to your feet. Stopping, looking "+
        "up with pleading eyes in an embarassed tone* \"Would it be ok if I became your AbsoluteSlave?\""))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /relationship request (Absolute-Slave) command");
            index = 13;
            return true;
        }

        // Acceptance of a player as your new Mistress/Master/Owner (relation) [ ID == 14 ]
        else if (textVal.Contains("nods in agreement with a smile.* \"Oh yes, most certainly. I would love for you to become my"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming /relationship acceptance (Mistress/Master/Owner) command");
            index = 14;
            return true;
        }

        // Acceptance of a player as your new Pet/Slave (relation) [ ID == 15 ]
        else if (textVal.Contains("smiles upon hearing the request and nods in agreement as their blushed companion. Reaching down to clasp a new "+
        "collar snug around their submissives neck.* \"Yes dearest, I'd love to make you my"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming /relationship acceptance (Slave/Pet) command");
            index = 15;
            return true;
        }

        // Acceptance of a player as your new Absolute-Slave (relation) [ ID == 16 ]
        else if (textVal.Contains("glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled.* "+
        "\"Verywell. And I hope you're able to devote yourself to the commitment of being my"))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming /relationship acceptance (Absolute-Slave) command");
            index = 16;
            return true;
        }

        // Declining a players request to become your Mistress/Master/Owner (relation) [ ID == 17 ]
        else if (textVal.Contains("smiles gently and shakes their head* \"I'm sorry, I just dont think I have enough space left in my daily life to "+
        "commit to such a bond quite yet.\""))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming /relationship decline (Mistress/Master/Owner) command");
            index = 17;
            return true;
        }

        // Declining a players request to become your Pet/Slave (relation) [ ID == 18 ]
        else if (textVal.Contains("shakes their head from side, \"I apologize dear, but I don't think im ready to commit myself to having that kind "+
        "of dynamic at the moment.\""))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming /relationship decline (Slave/Pet) command");
            index = 18;
            return true;
        }

        // Declining a players request to become your Absolute-Slave (relation) [ ID == 19 ]
        else if (textVal.Contains("takes a step back in surprise, \"Oh, I apologize, I didnt think you wanted a commitment that heavy... As much as "+
        "I'd love to oblige, I dont have enough space left in my life to commit to such a thing.\""))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected incoming /relationship decline (Absolute-Slave) command");
            index = 19;
            return true;
        }

        // Requesting a removal of relationship [ ID == 20 ]
        else if (textVal.Contains("looks up at you with tears in her eyes. She never wanted this moment to come, but also knows due to the "+
        "circumstances it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\""))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing /relationship removal command");
            index = 20;
            return true;
        }

        // none of the conditions were met, so return false?
        return false;
    }
}
