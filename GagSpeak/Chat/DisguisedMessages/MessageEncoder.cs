using Dalamud.Game.Text.SeStringHandling.Payloads;
using System.Collections.Generic;
using System.Linq;
using System;
using Lumina.Data.Parsing.Layer;


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
        if (password != "") {
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
        if (password != "") {
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

    // summarize later, for now, just know it encodes request mistress messages
    public string RequestMistressEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks down upon you from above, a smirk in her eyes as she sees the pleading look in your own* \"Well now darling, " +
        "your actions speak for you well enough, so tell me, do you wish for me to become your mistress?\"";
    }

    // summarize later, for now, just know it encodes request pet messages
    public string RequestPetEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks up at you, her nervous tone clear and cheeks blushing red as she studders out the words.* \"U-um, If it's ok " +
        "with you, could I become your pet?\"";
    }

    // summarize later, for now, just know it encodes request slave messages
    public string RequestSlaveEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} hears the sound of her leash's chain rattling along the floor as she crawls up to your feet. Stopping, looking up " +
        "with pleading eyes in an embarassed tone* \"Would it be ok if I became your slave?\"";
    }

    // summarize later, for now, just know it encodes relation removal messages
    public string RequestRemovalEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks up at you with tears in her eyes. She never wanted this moment to come, but also knows due to the circumstances " +
        "it was enivtable.* \"I'm sorry, but I cant keep our relationship going right now, there is just too much going on\"";
    }

    // summarize later, for now, just know it encodes the locking of live garbler messages
    public string OrderGarblerLockEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks down sternly at looks down sternly at the property they owned below them. They firmly slapped their " +
        "companion across the cheek and held onto her chin firmly.* \"You Belong to me, bitch. If i order you to stop pushing your gag out, you keep your gag in until i give you permission to take it out. Now do as I say.\"";
    }

    // summarize later, for now, just know it encodes the requesting of player information
    public string RequestInfoEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} looks down upon you with a smile,* \"I'd love to hear you describe your situation to me my dear, I want hear all about how you feel right now";
    }

    // for accepting a player as your mistress
    public string AcceptMistressEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} smiles and gracefully and nods in agreement* \"Oh yes, most certainly. I would love to have you as my mistress.\"";
    }

    // for accepting a player as your pet
    public string AcceptPetEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} smiles upon hearing the request and nods in agreement as their blushed companion had a collar clicked shut around their neck. \"Yes dear, I'd love to make you my pet.\"";
    }

    // for accepting a player as your slave
    public string AcceptSlaveEncodedMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} glanced back down at her companion who had just crawled up to their legs with the pleading look and smiled. \"Why I would love to make you my slave dearest.\"";
    }

////// DO THIS LAST ///////
    // for providing a player with your information (InDomMode, DirectChatGarbler, garbleLevel, selectedGagTypes, selectedGagPadlocks, selectedGagPadlocksPassword, selectedGagPadLockTimer, selectedGagPadlocksAssigner)
    public string ProvideInfoEncodedMessage(PlayerPayload playerPayload, string targetPlayer, bool _inDomMode, bool _directChatGarbler, int _garbleLevel, List<string> _selectedGagTypes,
    List<GagPadlocks> _selectedGagPadlocks, List<string> _selectedGagPadlocksAssigner, List<DateTimeOffset> _selectedGagPadlocksTimer, string relationship)
    {
        // we need to start applying some logic here, first create a base string
        string baseString = $"/tell {targetPlayer} *{playerPayload.PlayerName} from {playerPayload.World.Name} ";
        // now we need to append our next part of the message, the relationship to that player.
        if (relationship == "None") { baseString += "looks at their companion and smiles, "; } 
                               else { baseString += $"eyes their {relationship} and smiles, "; }
        baseString += $"While they were {(_inDomMode ? "Dominant" : "Submissive")} inside, the gags they wore for the last {_garbleLevel} minutes had them drooling already. ";

        // next we need to describe each gagtype they are wearing, if at any point the gag type is none, we should skip over each gag sections text entirely and just write that they had nothing on that layer
        string layerTerm = "underlayer";
        for (int i = 0; i < 3; i++) {
            if (i == 0) { layerTerm = "underlayer"; } else if (i == 1) { layerTerm = "surfacelayer"; } else if (i == 2) { layerTerm = "topmostlayer"; }
            if (_selectedGagTypes[i] == "None") { 
                baseString += $"Their {layerTerm} had nothing on it, "; // continuing on to describe the 2nd layer
            } else {
                baseString += $"Their {layerTerm} was sealed off with a {_selectedGagTypes[i]} gag";
            }
            if (_selectedGagPadlocks[i] != GagPadlocks.None) { // describe the lock, if any
                baseString += $", its strap locked with a {_selectedGagPadlocks[i]} ";
                //describe timer, if any
                if(true) {
                    baseString += $"set to unlock in {_selectedGagPadlocksTimer[i]} TIME, ";
                }
                // describe assigner, if any
                if(_selectedGagPadlocksAssigner[i] != "") { 
                    baseString += $"by {_selectedGagPadlocksAssigner[i]}"; }
            }
            baseString += $". ";
        }
        // finally, we need to describe the direct chat garbler, if any
        if (_directChatGarbler) {
            baseString += $"Ontop of it all, the bindings she had on weren't giving her much authority to change the way she sounded either..";
        }
        GagSpeak.Log.Debug($"Compiled Information Message: {baseString}");
        return baseString;
    }


}
