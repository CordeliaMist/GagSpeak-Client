using Dalamud.Game.Text.SeStringHandling.Payloads;


namespace GagSpeak.Chat.MsgEncoder;
// a struct to hold information on whitelisted players.
public class MessageEncoder // change to message encoder later
{
    // summarize later, for now, just know it encodes /gag apply messages
    public string GagEncodedApplyMessage(PlayerPayload playerPayload, string targetPlayer, string gagType, string layer) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} applies a {gagType} over your mouth as the {layer} layer of your concealment*";
    }

    // summarize later, for now, just know it encodes /gag lock messages
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer) { 
        return GagEncodedLockMessage(playerPayload, targetPlayer, lockType, layer, "");
    }
    
    // summarize later, for now, just know it encodes /gag lock password messages
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer, string password) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        if (password != null) {
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {lockType} from her pocket and sets the combination password to {password} before locking your {layer} layer gag*";
        } else {
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {lockType} from her pocket and uses it to lock your {layer} gag*";
        }
    }

    // summarize later, for now, just know it encodes /gag unlock messages
    public string GagEncodedUnlockMessage(PlayerPayload playerPayload, string targetPlayer, string layer) {
        return GagEncodedUnlockMessage(playerPayload, targetPlayer, layer, "");
    }
    
    // summarize later, for now, just know it encodes /gag unlock password messages
    public string GagEncodedUnlockMessage(PlayerPayload playerPayload, string targetPlayer, string layer, string password) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        if (password != null) {
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and sets the password to {password} on your {layer} layer gagstrap, unlocking it.*";
        } else {
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck, taking off the lock that was keeping your {layer} gag layer fastened nice and tight.*";
        }
    }

    // summarize later, for now, just know it encodes /gag remove messages
    public string GagEncodedRemoveMessage(PlayerPayload playerPayload, string targetPlayer, string layer) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and unfastens the buckle of your {layer} gag layer strap, allowing your voice to be a little clearer.*";
    }

    // summarize later, for now, just know it encodes /gag removeall messages
    public string GagEncodedRemoveAllMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.*";
    }
}
