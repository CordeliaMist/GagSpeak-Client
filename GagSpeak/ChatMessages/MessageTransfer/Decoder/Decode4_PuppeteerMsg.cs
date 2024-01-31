using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved message related to the wardrobe tab.
    public void DecodePuppeteerMsg(string recievedMessage, int encodedMsgIndex, ref List<string> decodedMessage) {
        
        // decoder for if the whitelisted user is toggling your permissions for allowing sit requests
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [28] = Boolean for if they allow sit requests
        if(encodedMsgIndex == 27) {
            // stuff
        }

        // decoder for if the whitelisted user is toggling your permissions for allowing motion requests
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [29] = Boolean for if they allow motion requests
        else if(encodedMsgIndex == 28) {
            // stuff
        }

        // decoder for if the whitelisted user is toggling your permissions for allowing all commands
        // [0] = playerMsgWasSentFrom, [1] = PlayerMesgWasSentFrom, [30] = Boolean for if they allow all commands
        else if(encodedMsgIndex == 29) {
            // stuff
        }
    }
}
