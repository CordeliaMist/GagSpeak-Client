using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    /// <summary> For requesting for information from another user in the whitelist </summary>
    public string EncodeRequestInfoMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"({playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "would enjoy it if you started our scene together by reminding them of all the various states you were left in, before we took a break from things for awhile~)";
    }

    /// <summary> Encodes the sharing of information (part 1)
    public string EncodeProvideInfoMessage() {
        var baseString = " ->";
        return baseString;
    }

    /// <summary> Encodes the sharing of information (part 2)
    public string EncodeProvideInfoMessage2() {
        var baseString = " ->";
        return baseString;
    }

    /// <summary> Encodes the sharing of information (part 3)
    public string EncodeProvideInfoMessage3() {
        var baseString = "Done!";
        return baseString;
    }
}
