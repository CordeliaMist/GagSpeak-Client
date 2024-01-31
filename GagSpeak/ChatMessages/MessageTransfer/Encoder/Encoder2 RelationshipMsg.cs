using System.Collections.Generic;
using System.Text.RegularExpressions;
using GagSpeak.CharacterData;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    // encoder for requesting a dominant based relationship (master/mistress/owner) [ ID == 11 ]
    public string EncodeRequestDominantStatus(PlayerPayload playerPayload, string targetPlayer, RoleLean relationType) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "looks down upon the submissive one before them, their pleading eyes forcing a smile across their lips. \"I take it you would like for me to become your "+
        $"{relationType.ToString()}"+
        "?\"";
    }

    // encoder for requesting a submissive based relationship (slave/pet) [ ID == 12 ]
    public string EncodeRequestSubmissiveStatus(PlayerPayload playerPayload, string targetPlayer, RoleLean relationType) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "looks up at the dominant figure before them pleading eyes, apperciating their presence deeply and desiring to grow closer towards them.* \"Would you please take me in as your "+
        $"{relationType.ToString()}"+
        "?\"";
    }

    // encoder for requesting a submission of total control (absolute-slave) [ ID == 13 ]
    public string EncodeRequestAbsoluteSubmissionStatus(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "hears the sound of her leash's chain rattling along the floor as she crawls up to your feet. Stopping, looking up " +
        "with pleading eyes in an embarassed tone* \"Would it be ok if I became your slave?\"";
    }

    // encoder for accepting a player as your new Mistress/Master/Owner (relation) [ ID == 14 ]
    public string EncodeAcceptRequestDominantStatus(PlayerPayload playerPayload, string targetPlayer, RoleLean acceptedRelationType) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "nods in agreement with a smile.* \"Oh yes, most certainly. I would love for you to become my "+
        $"{acceptedRelationType}"+
        "\"";
    }

    // encoder for accepting a player as your new Pet/Slave (relation) [ ID == 15 ]
    public string EncodeAcceptRequestSubmissiveStatus(PlayerPayload playerPayload, string targetPlayer, RoleLean acceptedRelationType) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "smiles upon hearing the request and nods in agreement as their blushed companion. Reaching down to clasp a new collar snug around their submissives neck.* \"Yes dearest, I'd love to make you my "+
        $"{acceptedRelationType}"+
        "\"";
    }

    // encoder for accepting a player as your new Absolute-Slave (relation) [ ID == 16 ]
    public string EncodeAcceptRequestAbsoluteSubmissionStatus(PlayerPayload playerPayload, string targetPlayer, RoleLean acceptedRelationType) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled.* \"Verywell. And I hope you're able to devote yourself to the commitment of being my "+
        $"{acceptedRelationType}"+
        "\"";
    }

    // encoder for declining a players request to become your Mistress/Master/Owner (relation) [ ID == 17 ]
    public string EncodeDeclineRequestDominantStatus(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "smiles gently and shakes their head* \"I'm sorry, I just dont think I have enough space left in my daily life to commit to such a bond quite yet.\"";
    }

    // encoder for declining a players request to become your Pet/Slave (relation) [ ID == 18 ]
    public string EncodeDeclineRequestSubmissiveStatus(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "shakes their head from side, \"I apologize dear, but I don't think im ready to commit myself to having that kind of dynamic at the moment.\"";
    }

    // encoder for declining a players request to become your Absolute-Slave (relation) [ ID == 19 ]
    public string EncodeDeclineRequestAbsoluteSubmissionStatus(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "takes a step back in surprise, \"Oh, I apologize, I didnt think you wanted a commitment that heavy... As much as I'd love to oblige, I dont have enough space left in my life to commit to such a thing.\"";
    }
    
    // encoder for requesting a removal of relationship [ ID == 20 ]
    public string EncodeSendRelationRemovalMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "looks up at you with tears in her eyes. She never wanted this moment to come, but also knows due to the circumstances it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"";
    }
}
