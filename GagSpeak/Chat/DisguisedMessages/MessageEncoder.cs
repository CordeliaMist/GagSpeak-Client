using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System;
using GagSpeak.Data;

using GagSpeak.UI.Helpers;

namespace GagSpeak.Chat.MsgEncoder;

/// <summary> This class is used to encode information that is sent off to other gagspeak users </summary>
public class MessageEncoder
{
    /// <summary> Composes a diguised encoded message for the /gag apply command.
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is applying the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is being gagged.</param></item>
    /// <item><c>gagType</c><param name="gagType"> - The type of gag being applied.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being applied.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag apply command. </returns>
    public string GagEncodedApplyMessage(PlayerPayload playerPayload, string targetPlayer, string gagType, string layer) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} applies a {gagType} over your mouth as the {layer} layer of your concealment*";
    }

    /// <summary> Composes a diguised encoded message for the /gag lock command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is locking the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag locked.</param></item>
    /// <item><c>lockType</c><param name="lockType"> - The type of lock being applied.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being locked.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag lock command. </returns> 
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer) { 
        return GagEncodedLockMessage(playerPayload, targetPlayer, lockType, layer, "");}
    
    /// <summary> Composes a diguised encoded message for the /gag lock password command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is locking the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag locked.</param></item>
    /// <item><c>lockType</c><param name="lockType"> - The type of lock being applied.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being locked.</param></item>
    /// <item><c>password</c><param name="password"> - The password of the lock being applied.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag lock password command. </returns>
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer, string password) {
        return GagEncodedLockMessage(playerPayload, targetPlayer, lockType, layer, password, "");}
    
    /// <summary> Composes a diguised encoded message for the /gag lock password password2 command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is locking the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag locked.</param></item>
    /// <item><c>lockType</c><param name="lockType"> - The type of lock being applied.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being locked.</param></item>
    /// <item><c>password</c><param name="password"> - The password of the lock being applied.</param></item>
    /// <item><c>password2</c><param name="password2"> - The password2 of the lock being applied.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag lock password password2 command. </returns>
    public string GagEncodedLockMessage(PlayerPayload playerPayload, string targetPlayer, string lockType, string layer, string password, string password2) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        if (password != "" && password2 != "") { // it is a password timer lock
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {lockType} from her pocket and sets the password to {password} with {password2} left, before locking your {layer} layer gag*";
        } else if (password != "" && password2 == "") { // it is any other password type lock
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {lockType} from her pocket and sets the password to {password}, locking your {layer} layer gag*";
        } else { // no password padlock
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} takes out a {lockType} from her pocket and uses it to lock your {layer} gag*";
        }
    }

    /// <summary> Composes a diguised encoded message for the /gag unlock command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is unlocking the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag unlocked.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being unlocked.</param></item>
    /// </list> </summary>
    public string GagEncodedUnlockMessage(PlayerPayload playerPayload, string targetPlayer, string layer) {
        return GagEncodedUnlockMessage(playerPayload, targetPlayer, layer, "");
    }
    
    /// <summary> Composes a diguised encoded message for the /gag unlock password command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is unlocking the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag unlocked.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being unlocked.</param></item>
    /// <item><c>password</c><param name="password"> - The password of the lock being unlocked.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag unlock password command. </returns>
    public string GagEncodedUnlockMessage(PlayerPayload playerPayload, string targetPlayer, string layer, string password) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        if (password != "") {
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and sets the password to {password} on your {layer} layer gagstrap, unlocking it.*";
        } else {
            return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck, taking off the lock that was keeping your {layer} gag layer fastened nice and tight.*";
        }
    }

    /// <summary> Composes a diguised encoded message for the /gag remove command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is removing the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag removed.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer of the gag being removed.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag remove command. </returns>
    public string GagEncodedRemoveMessage(PlayerPayload playerPayload, string targetPlayer, string layer) {
        if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and unfastens the buckle of your {layer} gag layer strap, allowing your voice to be a little clearer.*";
    }

    /// <summary> Composes a diguised encoded message for the /gag remove all command
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is removing the gag.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their gag removed.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag remove all command. </returns>
    public string GagEncodedRemoveAllMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more.*";
    }

    /// <summary> Composes a diguised encoded message for the request to become someones mistress
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player sending the mistress request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is accepting or declining for you to become their mistress.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message for the /gag remove all command. </returns>
    public string RequestMistressEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks down upon you from above, a smirk in her eyes as she sees the pleading look in your own* \"Well now darling, " +
        "your actions speak for you well enough, so tell me, do you wish for me to become your mistress?\"";
    }

    /// <summary> Composes a diguised encoded message for the request to become someones pet
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is getting the pet request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who you would become the pet of.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string RequestPetEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks up at you, her nervous tone clear and cheeks blushing red as she studders out the words.* \"U-um, If it's ok " +
        "with you, could I become your pet?\"";
    }

    /// <summary> Composes a diguised encoded message for the request to become someones slave
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player who is getting slave request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who you would become the slave of.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string RequestSlaveEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} hears the sound of her leash's chain rattling along the floor as she crawls up to your feet. Stopping, looking up " +
        "with pleading eyes in an embarassed tone* \"Would it be ok if I became your slave?\"";
    }

    /// <summary> Composes a diguised encoded message for the request to remove a relation
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player sending the request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their relation with them removed.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string RequestRemovalEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks up at you with tears in her eyes. She never wanted this moment to come, but also knows due to the circumstances " +
        "it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"";
    }

    /// <summary> Composes a diguised encoded message for the request to remove a relation
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player sending the order.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their garble speech locked.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string OrderGarblerLockEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks down sternly at looks down sternly at the property they owned below them. They firmly slapped their " +
        "companion across the cheek and held onto her chin firmly.* \"You Belong to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to take it out. Now do as I say.\"";
    }

    /// <summary>
    /// Requesting player info
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player payload of the player sending the order.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the player who is having their garble speech locked.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string RequestInfoEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks down upon you with a smile,* \"I'd love to hear you describe your situation to me my dear, I want hear all about how you feel right now";
    }

    /// <summary>
    /// Accept Mistress Request
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is accepting the target players request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieved that you accepted their request.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string AcceptMistressEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} smiles and gracefully and nods in agreement* \"Oh yes, most certainly. I would love to have you as my mistress.\"";
    }

    /// <summary>
    /// Accept Pet Request
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is accepting the target players request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieved that you accepted their request.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string AcceptPetEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} smiles upon hearing the request and nods in agreement as their blushed companion had a collar clicked shut around their neck. \"Yes dear, I'd love to make you my pet.\"";
    }

    /// <summary>
    /// Accept Slave Request
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is accepting the target players request.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieved that you accepted their request.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string AcceptSlaveEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled. \"Why I would love to make you my slave dearest.\"";
    }

    /// <summary>
    /// Provide the user with your encoded information about yourself
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is providing the information.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieve your info.</param></item>
    /// <item><c>_inDomMode</c><param name="_inDomMode"> - if you are in dominant mode.</param></item>
    /// <item><c>_directChatGarbler</c><param name="_directChatGarbler"> - if you are garbling direct chat.</param></item>
    /// <item><c>_garbleLevel</c><param name="_garbleLevel"> - the level of garble you are using.</param></item>
    /// <item><c>_selectedGagTypes</c><param name="_selectedGagTypes"> - the types of gags you are wearing.</param></item>
    /// <item><c>_selectedGagPadlocks</c><param name="_selectedGagPadlocks"> - the padlocks you are using.</param></item>
    /// <item><c>_selectedGagPadlocksAssigner</c><param name="_selectedGagPadlocksAssigner"> - the assigners of the padlocks you are using.</param></item>
    /// <item><c>_selectedGagPadlocksTimer</c><param name="_selectedGagPadlocksTimer"> - the timers of the padlocks you are using.</param></item>
    /// <item><c>relationship</c><param name="relationship"> - the relationship you have with the target player.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string ProvideInfoEncodedMessage(PlayerPayload playerPayload, string targetPlayer, bool _inDomMode, bool _directChatGarbler, int _garbleLevel, List<string> _selectedGagTypes,
    List<GagPadlocks> _selectedGagPadlocks, List<string> _selectedGagPadlocksAssigner, List<DateTimeOffset> _selectedGagPadlocksTimer, string relationship) {
        // we need to start applying some logic here, first create a base string
        string baseString = $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} ";
        // now we need to append our next part of the message, the relationship to that player.
        if (relationship == "None") { baseString += "eyes their companion, "; } 
                               else { baseString += $"eyes their {relationship}, "; }
        baseString += $"in a {(_inDomMode ? "dominant" : "submissive")} state, silenced over {_garbleLevel} minutes, already drooling. ";

        // next we need to describe each gagtype they are wearing, if at any point the gag type is none, we should skip over each gag sections text entirely and just write that they had nothing on that layer
        string layerTerm = "underlayer";
        for (int i = 0; i < 2; i++) {
            if (i == 0) { layerTerm = "underlayer"; } else if (i == 1) { layerTerm = "surfacelayer"; }
            if (_selectedGagTypes[i] == "None") { 
                baseString += $"Their {layerTerm} had nothing on it"; // continuing on to describe the 2nd layer
            } else {
                baseString += $"Their {layerTerm} sealed off by a {_selectedGagTypes[i]}";
            }
            if (_selectedGagPadlocks[i] != GagPadlocks.None) { // describe the lock, if any
                baseString += $", a {_selectedGagPadlocks[i]} securing it";
                //describe timer, if any
                if((_selectedGagPadlocksTimer[i] - DateTimeOffset.Now ) > TimeSpan.Zero) {
                    baseString += $" with {UIHelpers.FormatTimeSpan(_selectedGagPadlocksTimer[i] - DateTimeOffset.Now)} left";
                }
                // describe assigner, if any
                if(_selectedGagPadlocksAssigner[i] != "") { 
                    baseString += $", locked shut by {_selectedGagPadlocksAssigner[i]}"; }
            }
            baseString += $". ";
        }
        baseString += " ->";
        return baseString;
    }

    /// <summary>
    /// Provide the user with your encoded information about yourself (part 2 electric boogaloo)
    /// <list type="bullet">
    /// <item><c>playerPayload</c><param name="playerPayload"> - The player who is providing the information.</param></item>
    /// <item><c>targetPlayer</c><param name="targetPlayer"> - The name of the target player that will recieve your info.</param></item>
    /// <item><c>_inDomMode</c><param name="_inDomMode"> - if you are in dominant mode.</param></item>
    /// <item><c>_directChatGarbler</c><param name="_directChatGarbler"> - if you are garbling direct chat.</param></item>
    /// <item><c>_garbleLevel</c><param name="_garbleLevel"> - the level of garble you are using.</param></item>
    /// <item><c>_selectedGagTypes</c><param name="_selectedGagTypes"> - the types of gags you are wearing.</param></item>
    /// <item><c>_selectedGagPadlocks</c><param name="_selectedGagPadlocks"> - the padlocks you are using.</param></item>
    /// <item><c>_selectedGagPadlocksAssigner</c><param name="_selectedGagPadlocksAssigner"> - the assigners of the padlocks you are using.</param></item>
    /// <item><c>_selectedGagPadlocksTimer</c><param name="_selectedGagPadlocksTimer"> - the timers of the padlocks you are using.</param></item>
    /// <item><c>relationship</c><param name="relationship"> - the relationship you have with the target player.</param></item>
    /// </list> </summary>
    /// <returns> The disguised encoded message. </returns>
    public string ProvideInfoEncodedMessage2(PlayerPayload playerPayload, string targetPlayer, bool _inDomMode, bool _directChatGarbler, int _garbleLevel, List<string> _selectedGagTypes,
    List<GagPadlocks> _selectedGagPadlocks, List<string> _selectedGagPadlocksAssigner, List<DateTimeOffset> _selectedGagPadlocksTimer, string relationship) {
        string baseString = $"/tell {targetPlayer} || ";
        if (_selectedGagTypes[2] == "None") { 
            baseString += $"Finally, their topmostlayer had nothing on it"; // continuing on to describe the 2nd layer
        } else {
            baseString += $"Finally, their topmostlayer was covered with a {_selectedGagTypes[2]}";
        }
        if (_selectedGagPadlocks[2] != GagPadlocks.None) { // describe the lock, if any
            baseString += $", a {_selectedGagPadlocks[2]} sealing it";
            //describe timer, if any
            if(true) {
                baseString += $" with {UIHelpers.FormatTimeSpan(_selectedGagPadlocksTimer[2] - DateTimeOffset.Now)} left";
            }
            // describe assigner, if any
            if(_selectedGagPadlocksAssigner[2] != "") { 
                baseString += $" from {_selectedGagPadlocksAssigner[2]}"; }
        }
        // finally, we need to describe the direct chat garbler, if any
        if (_directChatGarbler) {
            baseString += $", their strained sounds muffled by everything.*";
        } else {
            baseString += ".*";
        }
        return baseString; 
    }
}
