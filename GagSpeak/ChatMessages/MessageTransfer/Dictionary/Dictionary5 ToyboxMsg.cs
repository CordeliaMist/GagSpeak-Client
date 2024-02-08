namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupToyboxMsg(string textVal, ref int encodedMsgIndex) {
        // The toggle allowing the whitelisted user to toggle your _enableToybox permission [ ID == 30 ]
        if(textVal.Contains("reached into the wardrobe and unlocked the lock securing their toybox drawer within "+
        "the wardrobe.\"Let's have some fun sweetie, mm?\""))
        {
            encodedMsgIndex = 30;
            return true;
        }

        // The toggle allowing the whitelisted user to toggle the active state of your toybox [ ID == 31 ]
        if(textVal.Contains("reached into the wardrobe and pulled out a vibrator device from the compartment, a smirk "+
        "formed on her face while she returned to their pet."))
        {
            encodedMsgIndex = 31;
            return true;
        }

        // The toggle allowing the whitelisted user to toggle the active state of your toybox [ ID == 31 ]
        if(textVal.Contains("reached into the wardrobe and pulled out a controlling device from the compartment, a smirk "+
        "formed on her face while she returned to their pet."))
        {
            encodedMsgIndex = 32;
            return true;
        }

        // The update of the intensity of the active toy with a new intensity level [ ID == 32 ]
        if(textVal.Contains("adjusted the slider on the viberators surface, altaring the intensity to a level of "))
        {
            encodedMsgIndex = 33;
            return true;
        }

        // The execution of a stored toy's pattern by its patternName [ ID == 33 ]
        if(textVal.Contains("pulled out her tomestone and tappened on the "))
        {
            encodedMsgIndex = 34;
            return true;
        }

        // The toggle allowing the whitelisted user to toggle the lock state of the toybox UI [ ID == 34 ]
        if(textVal.Contains("wrapped a layer of durable and tight concealment over the vibe, making it remain locked "+
        "in place against their submissive's skin.\"Enjoy~\""))
        {
            encodedMsgIndex = 35;
            return true;
        }

        // return false if none are present
        return false;
    } 
}