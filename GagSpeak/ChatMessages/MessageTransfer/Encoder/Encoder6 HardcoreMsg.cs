using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    // Encodes msg that lets the whitelisted user toggle your _enableToybox permission [ ID == 43 ]
    public string EncodeBlindfoldToggleOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "wrapped the lace blindfold nicely around your head, blocking out almost all light from your eyes, yet still allowing just enough through to keep things exciting*";
    }
}