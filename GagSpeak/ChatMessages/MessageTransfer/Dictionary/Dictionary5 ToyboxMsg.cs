namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupToyboxMsg(string textVal, DecodedMessageMediator decodedMessageMediator) {
        // The toggle allowing the whitelisted user to toggle your _enableToybox permission [ ID == 30 ]
        if(textVal.Contains("reached into the wardrobe and unlocked the lock securing their toybox drawer within "+
        "the wardrobe.\"Let's have some fun sweetie, mm?\""))
        {
            decodedMessageMediator.encodedMsgIndex = 30;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // The toggle allowing the whitelisted user to toggle the active state of your toybox [ ID == 31 ]
        if(textVal.Contains("reached into the wardrobe and pulled out a vibrator device from the compartment, a smirk "+
        "formed on her face while she returned to their pet."))
        {
            decodedMessageMediator.encodedMsgIndex = 31;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // The toggle allowing the whitelisted user to toggle the active state of your toybox [ ID == 31 ]
        if(textVal.Contains("reached into the wardrobe and pulled out a controlling device from the compartment, a smirk "+
        "formed on her face while she returned to their pet."))
        {
            decodedMessageMediator.encodedMsgIndex = 32;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // The update of the intensity of the active toy with a new intensity level [ ID == 32 ]
        if(textVal.Contains("adjusted the slider on the viberators surface, altaring the intensity to a level of "))
        {
            decodedMessageMediator.encodedMsgIndex = 33;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // The execution of a stored toy's pattern by its patternName [ ID == 33 ]
        if(textVal.Contains("pulled out her tomestone and tappened on the "))
        {
            decodedMessageMediator.encodedMsgIndex = 34;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // The toggle allowing the whitelisted user to toggle the lock state of the toybox UI [ ID == 34 ]
        if(textVal.Contains("wrapped a layer of durable and tight concealment over the vibe, making it remain locked "+
        "in place against their submissive's skin.\"Enjoy~\""))
        {
            decodedMessageMediator.encodedMsgIndex = 35;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // the msg to toggle the on/off state of the toy [ ID == 35 ]
        if(textVal.Contains("placed her thumb over the remote, toggling the switch and inverted the device state.")) {
            decodedMessageMediator.encodedMsgIndex = 36;
            decodedMessageMediator.msgType = DecodedMessageType.Toybox;
            return true;
        }

        // return false if none are present
        return false;
    } 
}