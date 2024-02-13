namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupInfoExchangeMsg(string textVal, DecodedMessageMediator decodedMessageMediator) {
        // The request for information from another user in the whitelist [ ID == 37 ]
        if(textVal.Contains("would enjoy it if you started our scene together by reminding them of "+
        "all the various states you were left in, before we took a break from things for awhile~")) {
            decodedMessageMediator.encodedMsgIndex = 37;
            decodedMessageMediator.msgType = DecodedMessageType.InfoExchange;
            return true;
        }

        // The sharing of information (part 1) [ ID == 38 ]
        if(textVal.Contains("from") && textVal.Contains(", their ") &&
        textVal.Contains("nodded in agreement, describing how") && textVal.Contains("On her undermost layer,")
        && (textVal.Contains("they carefully") || textVal.Contains("they easily"))
        && (textVal.Contains("strong bindings") || textVal.Contains("weak bindings"))
        && (textVal.Contains("muffling out") || textVal.Contains("speaking out"))
        && (textVal.Contains("gagged lips") || textVal.Contains("parted lips")))
        {
            decodedMessageMediator.encodedMsgIndex = 38;
            decodedMessageMediator.msgType = DecodedMessageType.InfoExchange;
            return true;
        }

        // The sharing of information (part 2) [ ID == 39 ]
        if(textVal.Contains("|| ") && textVal.Contains("Over their mouths main layer,")
        && textVal.Contains("Finally on her uppermost layer,") && textVal.Contains("->"))
        {
            decodedMessageMediator.encodedMsgIndex = 39;
            decodedMessageMediator.msgType = DecodedMessageType.InfoExchange;
            return true;
        }
        
        // The sharing of information (part 3) [ ID == 40 ]
        if((textVal.Contains("Their kink wardrobe was accessible for their partner") || textVal.Contains("Their kink wardrobe was closed off for their partner"))
        && (textVal.Contains("The wardrobes gag compartment was closed shut") || textVal.Contains("the wardrobes gag compartment had been pulled open"))
        && (textVal.Contains("and their restraint compartment was accessible for their partner") || textVal.Contains("and they had not allowed their partner to enable restraint sets"))
        && (textVal.Contains("They recalled their partner locking their restraints") || textVal.Contains("They recalled their partner leaving their restraints unlocked"))
        && textVal.Contains("their partner whispered")
        && (textVal.Contains("sit down on command") || textVal.Contains("sit down"))
        && (textVal.Contains("For their partner controlled their movements") || textVal.Contains("For their partner controlled most their movements"))
        && (textVal.Contains("and all of their actions") || textVal.Contains("and some of their actions")))
        {  
            decodedMessageMediator.encodedMsgIndex = 40;
            decodedMessageMediator.msgType = DecodedMessageType.InfoExchange;
            return true;
        }


        // The sharing of information (part 4) [ ID == 41 ]
        if((textVal.Contains("Their toybox compartment accessible to use.") || textVal.Contains("Their toybox inaccessible for use."))
        &&(textVal.Contains("was powered Vibrator") || textVal.Contains("was an unpowered Vibrator"))
        && (textVal.Contains("with an adjustable intensity level") || textVal.Contains("with a static intensity level"))
        && textVal.Contains("currently set to")
        && (textVal.Contains("The vibrator was able to execute set patterns") || textVal.Contains("Unfortuintely the vibrator couldnt execute any patterns"))
        && (textVal.Contains("with the viberator strapped tight to their skin") || textVal.Contains("with the vibrator loosely tied to their skin")))
        {
            decodedMessageMediator.encodedMsgIndex = 41;
            decodedMessageMediator.msgType = DecodedMessageType.InfoExchange;
            return true;
        }

        // return false if none are present
        return false;
    }
}
