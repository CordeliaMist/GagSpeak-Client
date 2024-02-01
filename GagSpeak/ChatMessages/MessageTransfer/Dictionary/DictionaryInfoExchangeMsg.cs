using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDictionary {
    public bool LookupInfoExchangeMsg(string textVal, ref int encodedMsgIndex) {
        // The request for information from another user in the whitelist [ ID == 35 ]
        if(textVal.Contains("would enjoy it if you started our scene together by reminding them of all the various states you were left in, before we took a break from things for awhile~")) {
            encodedMsgIndex = 35;
            return true;
        }

        // The sharing of information (part 1) [ ID == 36 ]
        if(textVal.Contains(" ->")) {
            encodedMsgIndex = 36;
            return true;
        }

        // The sharing of information (part 2) [ ID == 37 ]
        if(textVal.Contains(" ->")) {
            encodedMsgIndex = 37;
            return true;
        }

        // The sharing of information (part 3) [ ID == 38 ]
        if(textVal.Contains("Done!")) {
            encodedMsgIndex = 38;
            return true;
        }

        // return false if none are present
        return false;
    }
}
