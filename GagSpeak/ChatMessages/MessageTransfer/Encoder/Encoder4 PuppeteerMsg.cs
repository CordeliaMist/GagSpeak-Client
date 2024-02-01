using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    // Encodes msg that lets the whitelisted player to toggle your permissions for allowing sit requests [ ID == 27 ]
    public string EncodePuppeteerToggleOnlySitRequestOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "approached their submissive, \"Say now my love, how would you like to grant me access to control where you can and cant sit down?\"";
    }

    // Encodes msg that lets the whitelisted player to toggle your permissions for allowing motion requests [ ID == 28 ]
    public string EncodePuppeteerToggleOnlyMotionRequestOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "approached their submissive, \"Say now my love, how would you like to submit yourself to move about and dance for me whenever I say the word?\"";
    }

    // Encodes msg that lets the whitelisted player to toggle your permissions for allowing all commands [ ID == 29 ]
    public string EncodePuppeteerToggleAllCommandsOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "approached their submissive, \"We both know you've submitted yourself to me fully, so why not accept that you'll do whatever I say without a second thought?\"";
    }
}
