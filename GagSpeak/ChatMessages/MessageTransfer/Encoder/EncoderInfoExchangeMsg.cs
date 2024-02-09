using GagSpeak.Utility;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using GagSpeak.CharacterData;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    // for making sure we can interface with the character Handler
    
    /// <summary> For requesting for information from another user in the whitelist </summary>
    public string EncodeRequestInfoMessage(PlayerPayload playerPayload, string targetPlayer) {
        return $"/tell {targetPlayer} "+
        $"({playerPayload.PlayerName} from {playerPayload.World.Name} "+
        "would enjoy it if you started our scene together by reminding them of all the various states you were left in, before we took a break from things for awhile~)";
    }

    /// <summary> Encodes the sharing of information (part 1) COVERS THE FOLLOWING:
    /// <list type="bullet">
    /// <item><c>[0]</c> - The command/message type.</item>
    /// <item><c>[1]</c> - The assigner who sent it.</item>
    /// <item><c>[2]</c> - The layer index this command was meant for.</item> (replaces in requestInfo with players TheirRelationtoYou)
    /// <item><c>[3]</c> - The DynamicLean request sent, if any.</item> (replaces in requestInfo with players YourRelationtoThem)
    /// <item><c>[4]</c> - if the safeword is used or not. (BOOL)</item>
    /// <item><c>[5]</c> - if they allow extendedLockTimes (BOOL)</item>
    /// <item><c>[6]</c> - if the direct chat garbler is active (BOOL)</item>
    /// <item><c>[7]</c> - if the direct chat garbler is locked (BOOL)</item>
    /// </list> </summary>
    public string HandleProvideInfoPartOne(PlayerPayload playerPayload, string targetPlayer, CharacterHandler _characterHandler) {
        // first we need to get which whitelisted player in out config this is going to
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(targetPlayer)) {
            Idx = _characterHandler.GetWhitelistIndex(targetPlayer);
        }
        if(Idx == -1) {
            throw new Exception("The target player is not in the whitelist, and thus cannot be sent a request for information.");
        }
        // if we reach here, it is successful, and we can begin to encode the message.
        string targetPlayerFormatted = targetPlayer + "@" + _characterHandler.whitelistChars[Idx]._homeworld;

        // we will need to define all things in the long list for decoding.
        var baseString = $"/tell {targetPlayerFormatted} *{playerPayload.PlayerName} from {playerPayload.World.Name}, their "; // fulfills [1]
        // Example: "their Master's Pet." or "their Pet's Mistress"
        
        // fulfills [2]
        baseString += $"{_characterHandler.whitelistChars[Idx]._theirStatusToYou.ToString()}"+
        "'s "+
        
        // fulfills [3]
        $"{_characterHandler.whitelistChars[Idx]._yourStatusToThem} ";
        baseString += "nodded in agreement, informing their partner of how when they last played together, ";
        
        // [4] - if the safeword is used or not (BOOL)
        baseString += _characterHandler.playerChar._safewordUsed
        ? "they had used their safeword" : "they had no need to use a safeword";
        baseString += ". ";

        // [5] - if they allow extendedLockTimes (BOOL)
        baseString += _characterHandler.playerChar._grantExtendedLockTimes[Idx]
        ? "They didnt mind the enduring binds" : "Preferring to avoid long term binds";
        baseString += ", ";
        
        // [6] - if the direct chat garbler is active (BOOL)
        baseString += _characterHandler.playerChar._directChatGarblerActive
        ? "and they certain enjoyed their gagged voice" : "and not wishing to keep a gagged voice";
        baseString += ", ";
        
        // [7] - if the direct chat garbler is locked (BOOL)
        baseString += _characterHandler.playerChar._directChatGarblerLocked
        ? "for even now their lips were sealed tight" : "but as of now, their lips were not sealed fully";

        baseString += ". ->";
        // POTENTIAL MESSAGE SPLIT
        return baseString;
    }

    /// <summary> Encodes the sharing of information (part 2). COVERS THE FOLLOWING:
    /// <list type="bullet">
    /// <item><c>[8]</c> - layer one gag name (or name of gag/restraint trying to be applied/locked here) </item>
    /// <item><c>[9]</c> - layer two gag name </item>
    /// <item><c>[10]</c> - layer three gag name </item>
    /// <item><c>[11]</c> - layer one padlock type (or used for lock/unlock padlock definitions) </item>
    /// <item><c>[12]</c> - layer two padlock type </item>
    /// <item><c>[13]</c> - layer three padlock type </item>
    /// <item><c>[17]</c> - layer one padlock timer </item>
    /// <item><c>[18]</c> - layer two padlock timer </item>
    /// <item><c>[19]</c> - layer three padlock timer </item>
    /// <item><c>[20]</c> - layer one padlock assigner (or name of person applying/locking/unlocking) </item>
    /// <item><c>[21]</c> - layer two padlock assigner </item>
    /// <item><c>[22]</c> - layer three padlock assigner </item>
    /// </list> </summary>
    public string HandleProvideInfoPartTwo(PlayerPayload playerPayload, string targetPlayer, CharacterHandler _characterHandler) {
        // first we need to get which whitelisted player in out config this is going to
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(targetPlayer)) {
            Idx = _characterHandler.GetWhitelistIndex(targetPlayer);
        }
        if(Idx == -1) {
            throw new Exception("The target player is not in the whitelist, and thus cannot be sent a request for information.");
        }
        // if we reach here, it is successful, and we can begin to encode the message.
        string targetPlayerFormatted = targetPlayer + "@" + _characterHandler.whitelistChars[Idx]._homeworld;


        string baseString = $"/tell {targetPlayerFormatted} || When they had last played, ";
        // we need to create a large lambda function or conditional function that can be easily reversible by a regex.
        for (int i = 0; i < 3; i++) {
            string layerName = i == 0 ? "undermost" : i == 1 ? "main" : "uppermost";
            string startingWords = i == 0 ? $"On her {layerName} layer, " : i == 1 ? $"Over their mouths {layerName} layer, " : $"Finally on her {layerName} layer, ";
            // begin with our starting words
            baseString += startingWords;
            
            // [8, 9, 10] - layer {i} gag name if it WAS "None"
            if (_characterHandler.playerChar._selectedGagTypes[i] == "None") {
                baseString += $"there was nothing present";
            }
            // [8, 9, 10] - layer {i} gag name if it WAS NOT "None"
            else {
                baseString += $"she had a {_characterHandler.playerChar._selectedGagTypes[i]} fastened in good and tight";
                // [11, 12, 13] - layer {i} padlock type IF A PADLOCK IS PRESENT
                if (_characterHandler.playerChar._selectedGagPadlocks[i].ToString() != "None") {
                    baseString += $", locked with a {_characterHandler.playerChar._selectedGagPadlocks[i].ToString()}"; // [11, 12, 13]
                    // [20, 21, 22] - layer {i} padlock assigner IF IT EXISTS
                    if (!string.IsNullOrEmpty(_characterHandler.playerChar._selectedGagPadlockAssigner[i])) {
                        baseString += $" which had been secured by {_characterHandler.playerChar._selectedGagPadlockAssigner[i]}";
                    }
                    // [17, 18, 19] - layer {i} timer for padlock IF IT EXISTS
                    if (_characterHandler.playerChar._selectedGagPadlockTimer[i] - DateTimeOffset.Now > TimeSpan.Zero) {
                        baseString += $" with {UIHelpers.FormatTimeSpan(
                            _characterHandler.playerChar._selectedGagPadlockTimer[i] - DateTimeOffset.Now)} remaining";
                    }

                    if (_characterHandler.playerChar._selectedGagPadlockPassword[i] != null) {
                        baseString += $", with the password {_characterHandler.playerChar._selectedGagPadlockPassword[i]} on the lock";
                    }
                }
            }
            baseString += ". ";
        }
        // for message splitting
        baseString += " ->";
        return baseString;
    }

    /// <summary> Encodes the sharing of information (part 3)
    /// <list type="bullet">
    /// <item><c>[23]</c> - is wardrobe enabled (BOOL) </item>
    /// <item><c>[24]</c> - state of gag storage lock UI on gaglock? (BOOL) </item>
    /// <item><c>[25]</c> - is player allowed to enabled restraint sets? (BOOL) </item>
    /// <item><c>[26]</c> - is player allowed to lock restraint sets? (BOOL) </item>
    ///
    /// <item><c>[27]</c> - trigger phrase of messageSender for puppeteer compartment </item>
    /// <item><c>[28]</c> - does messageSender allow sit requests? (BOOL) </item>
    /// <item><c>[29]</c> - does messageSender allow motion requests? (BOOL) </item>
    /// <item><c>[30]</c> - does messageSender allow all commands? (BOOL) </item>
    ///
    /// <item><c>[31]</c> - is messageSenders toybox enabled? (BOOL) </item>
    /// </list> </summary>
    public string HandleProvideInfoPartThree(PlayerPayload playerPayload, string targetPlayer, CharacterHandler _characterHandler) {
        // first we need to get which whitelisted player in out config this is going to
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(targetPlayer)) {
            GagSpeak.Log.Debug($"[MsgEncoder]: Target Player: {targetPlayer}");
            Idx = _characterHandler.GetWhitelistIndex(targetPlayer);
        }
        if(Idx == -1) {
            throw new Exception("The target player is not in the whitelist, and thus cannot be sent a request for information.");
        }
        GagSpeak.Log.Debug($"[MsgEncoder]: Index of Whitelisted Char: {Idx}");

        // if we reach here, it is successful, and we can begin to encode the message.
        string targetPlayerFormatted = targetPlayer + "@" + _characterHandler.whitelistChars[Idx]._homeworld;


        string baseString = $"/tell {targetPlayerFormatted} || ";

        // [23] - is wardrobe enabled (BOOL)
        baseString += _characterHandler.playerChar._enableWardrobe
        ? "Their kink wardrobe was accessible for their partner" : "Their kink wardrobe was closed off for their partner";
        baseString += ". ";

        // [24] - state of gag storage lock UI on gaglock? (BOOL)
        baseString += _characterHandler.playerChar._lockGagStorageOnGagLock
        ? "The wardrobes gag compartment was closed shut" : "the wardrobes gag compartment had been pulled open";
        baseString += ", ";

        // [25] - is player allowed to enabled restraint sets? (BOOL)
        baseString += _characterHandler.playerChar._enableRestraintSets[Idx]
        ? "and their restraint compartment was accessible for their partner" : "and they had not allowed their partner to enable restraint sets";
        baseString += ". ";
        
        // [26] - is player allowed to lock restraint sets? (BOOL)
        baseString += _characterHandler.playerChar._restraintSetLocking[Idx]
        ? "They recalled their partner locking their restraints" : "They recalled their partner leaving their restraints unlocked";
        baseString += ", ";

        // [27] - trigger phrase of messageSender for puppeteer compartment
        baseString += $"their partner whispering"+
        $"{_characterHandler.playerChar._triggerPhraseForPuppeteer[Idx]}";

        // [28] - does messageSender allow sit requests? (BOOL)
        baseString += ", causing them to ";
        baseString += _characterHandler.playerChar._allowSitRequests[Idx]
        ? "sit down on command" : " sit down";
        baseString += ". ";

        // [29] - does messageSender allow motion requests? (BOOL)
        baseString += _characterHandler.playerChar._allowMotionRequests[Idx]
        ? "For their partner controlled their movements" : "For their partner controlled most their movements";
        baseString += ", ";

        // [30] - does messageSender allow all commands? (BOOL)
        baseString += _characterHandler.playerChar._allowAllCommands[Idx]
        ? "and all of their actions" : "and some of their actions";
        baseString += ". ";

        // [31] - is messageSenders toybox enabled? (BOOL)
        baseString += _characterHandler.playerChar._enableToybox
        ? "Their toybox compartment accessible to use." : "Their toybox inaccessible for use.";
        baseString += "Within the drawer there ";

        // for message splitting
        baseString += " ->";
        return baseString;
    }


    /// <item><c>[32]</c> - does messageSender allow you to toggle their toys active state? (BOOL) </item>
    /// <item><c>[33]</c> - does messageSender allow adjusting intensity of toy? (BOOL) </item>
    /// <item><c>[34]</c> - current intensity level of active toy ((or new intensity level being sent)) (INT) </item>
    /// <item><c>[35]</c> - does messageSender allow you to execute storedToyPatterns? (BOOL) </item>
    /// <item><c>[36]</c> - name of pattern to execute (not given in infoRequests) (STRING) </item>
    /// <item><c>[37]</c> - does messageSender allow you to lock the toybox UI? (BOOL) </item>
    public string HandleProvideInfoPartFour(PlayerPayload playerPayload, string targetPlayer, CharacterHandler _characterHandler) {
        // first we need to get which whitelisted player in out config this is going to
        int Idx = -1;
        if(_characterHandler.IsPlayerInWhitelist(targetPlayer)) {
            GagSpeak.Log.Debug($"[MsgEncoder]: Target Player: {targetPlayer}");
            Idx = _characterHandler.GetWhitelistIndex(targetPlayer);
        }
        if(Idx == -1) {
            throw new Exception("The target player is not in the whitelist, and thus cannot be sent a request for information.");
        }
        GagSpeak.Log.Debug($"[MsgEncoder]: Index of Whitelisted Char: {Idx}");

        // if we reach here, it is successful, and we can begin to encode the message.
        string targetPlayerFormatted = targetPlayer + "@" + _characterHandler.whitelistChars[Idx]._homeworld;

        string baseString = $"/tell {targetPlayerFormatted} || ";
        
        // [32] - does messageSender allow you to toggle toy? (BOOL)
        baseString += _characterHandler.playerChar._allowChangingToyState[Idx]
        ? "was powered Vibrator" : "was an unpowered Vibrator";
        baseString += ", ";

        // [33] - does messageSender allow adjusting intensity of toy? (BOOL)
        baseString += _characterHandler.playerChar._allowIntensityControl[Idx]
        ? "with an adjustable intensity level" : "with a static intensity level";

        // [34] - current intensity level of active toy ((or new intensity level being sent)) (INT)
        baseString += $" currently set to ";
        baseString += $"{_characterHandler.playerChar._intensityLevel}";
        baseString += ". ";

        // [35] - does messageSender allow you to execute storedToyPatterns? (BOOL)
        baseString += _characterHandler.playerChar._allowUsingPatterns[Idx]
        ? "The vibrator was able to execute set patterns" : "Unfortuintely, the vibrator couldnt execute any patterns";
        baseString += ", ";

        // [37] - does messageSender allow you to lock the toybox UI? (BOOL)
        baseString += _characterHandler.playerChar._allowToyboxLocking
        ? "with the viberator strapped tight to their skin. " : "with the vibrator loosely tied to their skin. ";

        return baseString;
    }
}
