using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    // Encodes msg that lets the whitelisted user toggle your _enableToybox permission [ ID == 30 ]
    public string EncodeToyboxToggleEnableToyboxOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "reached into the wardrobe and unlocked the lock securing their toybox drawer within the wardrobe.\"Let's have some fun sweetie, mm?\"";
    }

    // Encodes msg that lets the whitelisted user toggle the active state of your toybox [ ID == 31 ]
    public string EncodeToyboxToggleActiveToyboxOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "reached into the wardrobe and pulled out a vibrator device from the compartment, a smirk formed on her face while she returned to their pet.";
    }

    // Encodes msg that lets the toggles if the whitelisted user is allowed to have control over the intensity of an active toy [ ID == 32 ]
    public string EncodeToyboxToggleAllowingIntensityControl(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "reached into the wardrobe and pulled out a controlling device from the compartment, a smirk formed on her face while she returned to their pet.";
    }

    // Encodes msg that lets the whitelisted user update the intensity of your active toy [ ID == 33 ]
    public string EncodeToyboxUpdateActiveToyIntensity(PlayerPayload playerPayload, string targetPlayer, int newIntensityLevel) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "adjusted the slider on the viberators surface, altaring the intensity to a level of "+
        $"{newIntensityLevel}"+
        ".";
    }

    // Encodes msg that lets the whitelisted user execute a stored toy's pattern by its patternName [ ID == 34 ]
    public string EncodeToyboxExecuteStoredToyPattern(PlayerPayload playerPayload, string targetPlayer, string patternName) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "pulled out her tomestone and tappened on the "+
        $"{patternName} "+
        "pattern, which had been linked to the active vibe against their body, causing it to provide their submissive with a wonderous dose of pleasure.";
    }

    // Encodes msg that lets the whitelisted user toggle the lock state of the toybox UI [ ID == 35 ]
    public string EncodeToyboxToggleLockToyboxUI(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "wrapped a layer of durable and tight concealment over the vibe, making it remain locked in place against their submissive's skin.\"Enjoy~\"";
    }

    // Encodes msg that lets the whitelisted user turn on/off the toy [ ID == 36 ]
    public string EncodeToyboxToggleToyOnOff(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "placed her thumb over the remote, toggling the switch and inverted the device state.";
    }
}