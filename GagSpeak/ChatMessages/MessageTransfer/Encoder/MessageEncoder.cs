using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> Do not expect me to comment much on this, its just very repedative encoding/decoding </summary>
public partial class MessageEncoder {

    /// <summary>
    /// Lock a players restraint set for a spesified ammount of time provided a valid restraint set name
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is locking the restraint set.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieve your info.</param></item>
    /// <item><c>restraintSetName</c><param name="restraintSetName"> - the name of the restraint set you are locking.</param></item>
    /// <item><c>timer</c><param name="timer"> - the timer of the lock you are using.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string GagEncodedRestraintSetLockMessage(PlayerPayload playerPayload, string restraintSetName, string timer, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} opens up the compartment of restraints from their wardrobe, taking out "+
        $"the {restraintSetName}. Bringing it over to their companion, they help secure them inside it, deciding to leave it in them for the next {timer}*";    
    }

    /// <summary>
    /// Unlock a players restraint set provided a valid restraint set name
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is unlocking the restraint set.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieve your info.</param></item>
    /// <item><c>restraintSetName</c><param name="restraintSetName"> - the name of the restraint set you are unlocking.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string GagEncodedRestraintSetUnlockMessage(PlayerPayload playerPayload, string restraintSetName, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} decided they wanted to use their companion for other things now, unlocking "+
        $"the {restraintSetName} from their partner and allowing them to feel a little more free, for now~*";    
    }
}
