using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {        
    // Encodes the message for toggling if the gagstorage UI will become inaccessable when a gag is locked or not [ ID == 21 ]
    public string EncodeWardrobeGagStorageUiLockToggle(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "held their sluts chin firmly, forcing them to look them in the eyes* \"Let's make sure your locks have a little bit more security, shall we?\"";
    }

    // Encodes the message that allows the dominant to toggle the permission to allow enabling restraint sets [ ID == 22 ]
    public string EncodeWardrobeEnableRestraintSetsOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "looked down at their companion before walking ove to their wardrobe, \"Now you'll be a good slut and not resist any restraint sets I try putting you in, understand?~\"";
    }

    // Encodes the message that allows the dominant to toggle the permission to allow locking restraint sets [ ID == 23 ]
    public string EncodeWardrobeEnableRestraintSetLockingOption(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "looked down at their companion before walking ove to their wardrobe, \"Now you'll be a good slut and not resist any locks I try putting on your restraints, understand?~\"";
    }
        
    // Encodes a message for enabling the restraint set onto the player [ ID == 24 ]
    public string EncodeWardrobeEnableRestraintSet(PlayerPayload playerPayload, string targetPlayer, string restraintSetName) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "opens up the compartment of restraints from their wardrobe, taking out the "+
        $"{restraintSetName} "+
        "and brought it back over to their slut to help secure them inside it.";
    }

    // Encodes a message for locking the restraint set onto the player [ ID == 25 ]
    public string EncodeWardrobeRestraintSetLock(PlayerPayload playerPayload, string targetPlayer, string restraintSetName, string timer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "took out a timed padlock, and fastened it around the "+
        $"{restraintSetName} "+
        "on its focal point, setting its duration to "+
        $"{timer}"+
        "*";    
    }

    // Encodes a message for unlocking the restraint set from the player [ ID == 26 ]
    public string EncodeWardrobeRestraintSetUnlock(PlayerPayload playerPayload, string targetPlayer, string restraintSetName) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "decided they wanted to use their companion for other things now, unlocking the "+
        $"{restraintSetName} "+
        "from their partner and allowing them to feel a little more free, for now~*";
    }
}
