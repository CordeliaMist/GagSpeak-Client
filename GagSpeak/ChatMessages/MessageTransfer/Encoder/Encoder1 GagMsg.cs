using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    // The Apply Gag Message [ ID == 1 // apply ]
    public string GagEncodedApplyMessage(PlayerPayload playerPayload, string targetPlayer, string gagType, string layer) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "applies a "+
        $"{gagType} "+
        "over your mouth as the "+
        $"{layer} "+
        "layer of your concealment*";
    }

    // The Lock Gag Message [ ID == 2 // lock ]
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer) { 
        return GagEncodedLockMessage(playerPayload, targetPlayer, lockType, layer, "");}

    // The Lock Gag Message [ ID == 3 // lockPassword ]
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer, string password) {
        return GagEncodedLockMessage(playerPayload, targetPlayer, lockType, layer, password, "");}
    
    // The Lock Gag Message [ ID == 4 // lockTimerPassword ]
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer, string password, string password2) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        // it is a password timer lock [ ID4 ]
        if (password != "" && password2 != "")
        {
            return $"/tell {targetPlayer} "+
            $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
            "takes out a "+
            $"{lockType} "+
            "from her pocket and sets the password to "+
            $"{password} "+
            "with "+
            $"{password2} "+
            "left, before locking your "+
            $"{layer} "+
            "layer gag*";
        }
        // it is any other password type lock [ ID3 ]
        else if (password != "" && password2 == "")
        {
            return $"/tell {targetPlayer} "+
            $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
            "takes out a "+
            $"{lockType} "+
            "from her pocket and sets the password to "+
            $"{password}"+
            ", locking your "+
            $"{layer} "+
            "layer gag*";
        }
        // no password padlock [ ID2 ]
        else
        {
            return $"/tell {targetPlayer} "+
            $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
            "takes out a "+
            $"{lockType} "+
            "from her pocket and uses it to lock your "+
            $"{layer} "+
            "gag*";
        }
    }

    // The Unlock Gag Message [ ID == 5 // unlock ]
    public string GagEncodedUnlockMessage(PlayerPayload playerPayload, string targetPlayer, string layer) {
        return GagEncodedUnlockMessage(playerPayload, targetPlayer, layer, "");
    }
    
    // The Unlock Gag Message [ ID == 6 // unlockPassword ]
    public string GagEncodedUnlockMessage(PlayerPayload playerPayload, string targetPlayer, string layer, string password) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        // it is a password unlock [ ID6 ]
        if (password != "")
        {
            return $"/tell {targetPlayer} "+
            $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
            "reaches behind your neck and sets the password to "+
            $"{password} "+
            "on your "+
            $"{layer} "+
            "layer gagstrap, unlocking it.*";
        } 
        // no password unlock [ ID5 ]
        else
        {
            return $"/tell {targetPlayer} "+
            $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
            "reaches behind your neck, taking off the lock that was keeping your "+
            $"{layer} "+
            "gag layer fastened nice and tight.*";
        }
    }

    // The Remove Gag Message [ ID == 7 // remove ]
    public string GagEncodedRemoveMessage(PlayerPayload playerPayload, string targetPlayer, string layer) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "reaches behind your neck and unfastens the buckle of your "+
        $"{layer} "+
        "gag layer strap, allowing your voice to be a little clearer.*";
    }

    // The Remove All Gags Message [ ID == 8 // removeAll ]
    public string GagEncodedRemoveAllMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.*";
    }

    // The gag order "Toggle Live Garbler State" Message [ ID == 9 // toggleLiveChatGarbler ]
    public string GagOrderToggleLiveChatGarbler(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "brushes her finger overtop the gag resting over your mouth.* \"Now be a good girl and be sure to give me those sweet muffled sounds whenever you speak~\"";
    }

    // the gag order "toggle Live Chat Garbler lock" Message [ ID == 10 // toggleLiveChatGarblerLock ]
    public string GagOrderToggleLiveChatGarblerLock(PlayerPayload playerPayload) {
        return $"/tell {targetPlayer} "+
        $"*{playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "chuckles in delight of seeing their gagged submissive below them, a smile formed across their lips.* \"Look's like you'll be stuck speaking in muffled moans for some time now~\"";
    }
}

