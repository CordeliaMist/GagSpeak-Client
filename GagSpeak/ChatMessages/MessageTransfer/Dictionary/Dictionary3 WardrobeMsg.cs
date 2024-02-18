namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupWardrobeMsg(string textVal, DecodedMessageMediator decodedMessageMediator) {
        // The GagStorageUI lock toggle [ ID == 21 // gagStorageUiLockToggle ]
        if (textVal.Contains("held their sluts chin firmly, forcing them to look them in the eyes* \"Let's make "+
        "sure your locks have a little bit more security, shall we?\""))
        {
            GagSpeak.Log.Debug($"[Message Dictionary]: Detected outgoing GagStorageUILock command");   
            decodedMessageMediator.encodedMsgIndex = 21;
            decodedMessageMediator.msgType = DecodedMessageType.Wardrobe;
            return true;
        }

        // The _allowRestraintSets toggle message [ ID == 22 // _enableRestraintSets ]
        if(textVal.Contains("looked down at their companion before walking over to their wardrobe, \"Now you'll be a good slut and not resist any restraint sets I try putting you in, understand?~\""))
        {  
            decodedMessageMediator.encodedMsgIndex = 22;
            decodedMessageMediator.msgType = DecodedMessageType.Wardrobe;
            return true;
        }

        // The toggle allowLockingRestraints option message [ ID == 23 // _restraintSetLocking ]
        if(textVal.Contains("looked down at their companion before walking ove to their wardrobe, \"Now you'll be a good slut and not resist any locks I try putting on your restraints, understand?~\""))
        {
            decodedMessageMediator.encodedMsgIndex = 23;
            decodedMessageMediator.msgType = DecodedMessageType.Wardrobe;
            return true;   
        }

        // The message for enabling a particular restraint set [ ID == 24 // enable restraint set by name ]
        if(textVal.Contains("opens up the compartment of restraints from their wardrobe, taking out the")
        && textVal.Contains("and brought it back over to their slut to help secure them inside it."))
        {
            decodedMessageMediator.encodedMsgIndex = 24;
            decodedMessageMediator.msgType = DecodedMessageType.Wardrobe;
            return true;   
        }

        // The message for locking an active restraint set if enabled [ ID == 25 // lock restraint set ]
        if(textVal.Contains("took out a timed padlock, and fastened it around the")
        && textVal.Contains("on its focal point, setting its duration to"))
        {
            decodedMessageMediator.encodedMsgIndex = 25;
            decodedMessageMediator.msgType = DecodedMessageType.Wardrobe;
            return true;   
        }

        // The message for unlocking an actively locked restraint set [ ID == 26 // unlock restraint set ]
        if(textVal.Contains("decided they wanted to use their companion for other things now, unlocking the ")
        && textVal.Contains("from their partner and allowing them to feel a little more free, for now~*"))
        {
            decodedMessageMediator.encodedMsgIndex = 26;
            decodedMessageMediator.msgType = DecodedMessageType.Wardrobe;
            return true;    
        }

        // return false if none are present
        return false;
    }
}

