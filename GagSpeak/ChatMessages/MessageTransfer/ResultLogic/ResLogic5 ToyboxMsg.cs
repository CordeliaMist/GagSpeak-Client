using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void ReslogicToyboxMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        
        // decoder for if the whitelisted user is toggling your _enableToybox permission
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [31] = Boolean for the new state _enabledToybox will be for you
        if(encodedMsgIndex == 30) {
            // stuff
        }

        // decoder for if the whitelisted user is starting/stopping your active toy
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [33] = Boolean for if the toybox is active or not
        else if(encodedMsgIndex == 31) {
            // stuff
        }

        // decoder for updating the intensity of the active toy with a new intensity level
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [35] = new intensity level of the active toy
        else if(encodedMsgIndex == 32) {
            // stuff
        }

        // decoder for executing a stored toy pattern by its name
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [37] = name of the stored toy pattern
        else if(encodedMsgIndex == 33) {
            // stuff
        }

        // decoder for if the whitelisted user is toggling the lock state of the toybox UI
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [38] = Boolean for if the toybox UI is locked or not
        else if(encodedMsgIndex == 34) {
            // stuff
        }
    }
}