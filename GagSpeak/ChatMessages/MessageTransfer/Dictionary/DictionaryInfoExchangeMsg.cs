namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupInfoExchangeMsg(string textVal, ref int encodedMsgIndex) {
        // The request for information from another user in the whitelist [ ID == 35 ]
        if(textVal.Contains("would enjoy it if you started our scene together by reminding them of "+
        "all the various states you were left in, before we took a break from things for awhile~")) {
            encodedMsgIndex = 35;
            return true;
        }

        // The sharing of information (part 1) [ ID == 36 ]
        if(textVal.Contains("from") && textVal.Contains(", their ") &&
        textVal.Contains("nodded in agreement, informing their partner of how when they last played together")
        && (textVal.Contains("they had used their safeword") || textVal.Contains("they had no need to use a safeword"))
        && (textVal.Contains("They didnt mind the enduring binds") || textVal.Contains("Preferring to avoid long term binds"))
        && (textVal.Contains("and they certain enjoyed their gagged voice") || textVal.Contains("and not wishing to keep a gagged voice"))
        && (textVal.Contains("for even now their lips were sealed tight") || textVal.Contains("but as of now, their lips were not sealed fully")))
        {
            encodedMsgIndex = 36;
            return true;
        }

        // The sharing of information (part 2) [ ID == 37 ]
        if(textVal.Contains("|| When they had last played, ") && textVal.Contains("On her undermost layer, ") && textVal.Contains("Over their mouths main layer, ")
        && textVal.Contains("Finally on her uppermost layer, ") && textVal.Contains(".  ->"))
        {
            encodedMsgIndex = 37;
            return true;
        }
        
        // The sharing of information (part 3) [ ID == 38 ]
        if((textVal.Contains("Their kink wardrobe was accessible for their partner") || textVal.Contains("Their kink wardrobe was closed off for their partner"))
        && (textVal.Contains("The wardrobes gag compartment was closed shut") || textVal.Contains("the wardrobes gag compartment had been pulled open"))
        && (textVal.Contains("and their restraint compartment was accessible for their partner") || textVal.Contains("and they had not allowed their partner to enable restraint sets"))
        && (textVal.Contains("They recalled their partner locking their restraints") || textVal.Contains("They recalled their partner leaving their restraints unlocked"))
        && textVal.Contains("their partner whispering")
        && (textVal.Contains("sit down on command") || textVal.Contains("sit down"))
        && (textVal.Contains("For their partner controlled their movements") || textVal.Contains("For their partner controlled most their movements"))
        && (textVal.Contains("and all of their actions") || textVal.Contains("and some of their actions"))
        && (textVal.Contains("Their toybox compartment accessible to use. For within the drawer") || textVal.Contains("Their toybox inaccessible for use. But within the drawer"))
        && (textVal.Contains("was powered Vibrator") || textVal.Contains("was an unpowered Vibrator"))
        && (textVal.Contains("with an adjustable intensity level") || textVal.Contains("with a static intensity level"))
        && textVal.Contains("currently set to")
        && (textVal.Contains("The vibrator was able to execute set patterns") || textVal.Contains("Unfortuintely, the vibrator couldnt execute any patterns"))
        && (textVal.Contains("with the viberator strapped tight to their skin") || textVal.Contains("with the vibrator loosely tied to their skin")))
        {  
            encodedMsgIndex = 38;
            return true;
        }

        // return false if none are present
        return false;
    }
}
