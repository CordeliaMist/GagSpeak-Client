using GagSpeak.Utility;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using System;
using GagSpeak.CharacterData;
using GagSpeak.Services;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageEncoder {
    private readonly PlugService _plugService;
    public MessageEncoder(PlugService plugService) {
        _plugService = plugService;
    }
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
        
        // fulfills their
        baseString += $"{_characterHandler.whitelistChars[Idx]._theirStatusToYou.ToString()}";
        baseString += "'s ";

        // fulfills your lean
        baseString += $"{_characterHandler.whitelistChars[Idx]._yourStatusToThem} ";
        baseString += "nodded in agreement, describing how ";
        
        // if the safeword is used or not (BOOL)
        baseString += _characterHandler.playerChar._safewordUsed
        ? "they carefully" : "they easily";
        baseString += " endured ";

        // if they allow extendedLockTimes (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._grantExtendedLockTimes
        ? "strong bindings" : "weak bindings";
        baseString += ", ";
        
        //  if the direct chat garbler is active (BOOL)
        baseString += _characterHandler.playerChar._directChatGarblerActive
        ? "muffling out" : "speaking out";
        baseString += " through ";
        
        // if the direct chat garbler is locked (BOOL)
        baseString += _characterHandler.playerChar._directChatGarblerLocked
        ? "gagged lips" : "parted lips";
        baseString += ". ";

        // include the first iteration of the gag here.
        baseString += $"On her undermost layer, ";
        // layer 0 gag name if it WAS "None"
        if (_characterHandler.playerChar._selectedGagTypes[0] == "None") {
            baseString += $"there was nothing present";
        }
        // layer 0 gag name if it WAS NOT "None"
        else {
            baseString += $"there was a {_characterHandler.playerChar._selectedGagTypes[0]} fastened in good and tight, ";
            // layer 0 padlock type IF A PADLOCK IS PRESENT
            if (_characterHandler.playerChar._selectedGagPadlocks[0].ToString() != "None") {
                baseString += $"locked with a {_characterHandler.playerChar._selectedGagPadlocks[0].ToString()}, ";
                // layer 0 padlock assigner IF IT EXISTS
                if (!string.IsNullOrEmpty(_characterHandler.playerChar._selectedGagPadlockAssigner[0])) {
                    baseString += $" which had been secured by {_characterHandler.playerChar._selectedGagPadlockAssigner[0]}, ";
                }
                //  layer 0 timer for padlock IF IT EXISTS
                if (_characterHandler.playerChar._selectedGagPadlockTimer[0] - DateTimeOffset.Now > TimeSpan.Zero) {
                    baseString += $"with {UIHelpers.FormatTimeSpan(_characterHandler.playerChar._selectedGagPadlockTimer[0] - DateTimeOffset.Now)} remaining, ";
                }
                // password if it exists
                if (_characterHandler.playerChar._selectedGagPadlockPassword[0] != null) {
                        baseString += $"with the password {_characterHandler.playerChar._selectedGagPadlockPassword[0]} on the lock";
                    }
                }
            }
            baseString += ".";


        baseString += " ->";
        // POTENTIAL MESSAGE SPLIT
        return baseString;
    }

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

        string baseString = $"/tell {targetPlayerFormatted} || ";
        // captures the second and third layer of the gag details
        for (int i = 0; i < 2; i++) {
            string startingWords = i == 0 ? $"Over their mouths main layer, " : $"Finally on her uppermost layer, ";
            // begin with our starting words
            baseString += startingWords;
            
            // layer {i} gag name if it WAS "None"
            if (_characterHandler.playerChar._selectedGagTypes[i+1] == "None") {
                baseString += $"there was nothing present";
            }
            // layer {i} gag name if it WAS NOT "None"
            else {
                baseString += $"there was a {_characterHandler.playerChar._selectedGagTypes[i+1]} fastened in good and tight, ";
                // layer {i} padlock type IF A PADLOCK IS PRESENT
                if (_characterHandler.playerChar._selectedGagPadlocks[i+1].ToString() != "None") {
                    baseString += $"locked with a {_characterHandler.playerChar._selectedGagPadlocks[i+1].ToString()}, ";
                    // layer {i} padlock assigner IF IT EXISTS
                    if (!string.IsNullOrEmpty(_characterHandler.playerChar._selectedGagPadlockAssigner[i+1])) {
                        baseString += $" which had been secured by {_characterHandler.playerChar._selectedGagPadlockAssigner[i+1]}, ";
                    }
                    //  layer {i} timer for padlock IF IT EXISTS
                    if (_characterHandler.playerChar._selectedGagPadlockTimer[i+1] - DateTimeOffset.Now > TimeSpan.Zero) {
                        baseString += $"with {UIHelpers.FormatTimeSpan(_characterHandler.playerChar._selectedGagPadlockTimer[i+1] - DateTimeOffset.Now)} remaining, ";
                    }

                    if (!string.IsNullOrWhiteSpace(_characterHandler.playerChar._selectedGagPadlockPassword[i+1])) {
                        baseString += $"with the password {_characterHandler.playerChar._selectedGagPadlockPassword[i+1]} on the lock";
                    }
                }
            }
            baseString += ". ";
        }
        // for message splitting
        baseString += "->";
        return baseString;
    }

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

        // is wardrobe enabled (BOOL)
        baseString += _characterHandler.playerChar._enableWardrobe
        ? "Their kink wardrobe was accessible for their partner" : "Their kink wardrobe was closed off for their partner";
        baseString += ". ";

        // state of gag storage lock UI on gaglock? (BOOL)
        baseString += _characterHandler.playerChar._lockGagStorageOnGagLock
        ? "The wardrobes gag compartment was closed shut" : "the wardrobes gag compartment had been pulled open";
        baseString += ", ";

        // is player allowed to enabled restraint sets? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._enableRestraintSets
        ? "and their restraint compartment was accessible for their partner" : "and they had not allowed their partner to enable restraint sets";
        baseString += ". ";
        
        // is player allowed to lock restraint sets? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._restraintSetLocking
        ? "They recalled their partner locking their restraints" : "They recalled their partner leaving their restraints unlocked";
        baseString += ", ";

        // is the puppeteer enabled? (BOOL)
        baseString += _characterHandler.playerChar._allowPuppeteer
        ? "loyal as ever when " : "questionably complying when ";

        // trigger phrase of messageSender for puppeteer compartment
        baseString += $"their partner whispered "+
        $"{_characterHandler.playerChar._uniquePlayerPerms[Idx]._StartCharForPuppeteerTrigger}"+
        $"{_characterHandler.playerChar._uniquePlayerPerms[Idx]._triggerPhraseForPuppeteer}"+
        $"{_characterHandler.playerChar._uniquePlayerPerms[Idx]._EndCharForPuppeteerTrigger}";

        // does messageSender allow sit requests? (BOOL)
        baseString += " causing them to ";
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._allowSitRequests
        ? "sit down on command" : "sit down";
        baseString += ". ";

        // does messageSender allow motion requests? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._allowMotionRequests
        ? "For their partner controlled their movements" : "For their partner controlled most their movements";
        baseString += ", ";

        // does messageSender allow all commands? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._allowAllCommands
        ? "and all of their actions" : "and some of their actions";
        baseString += ". ";

        // for message splitting
        baseString += "->";
        return baseString;
    }

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

        // is messageSenders toybox enabled? (BOOL)
        baseString += _characterHandler.playerChar._enableToybox
        ? "Their toybox compartment accessible to use" : "Their toybox inaccessible for use";
        baseString += ". Within the drawer there ";

        // does messageSender allow you to toggle toy? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._allowChangingToyState
        ? "was powered Vibrator" : "was an unpowered Vibrator";
        baseString += ", ";

        // does messageSender allow adjusting intensity of toy? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._allowIntensityControl
        ? "with an adjustable intensity level" : "with a static intensity level";

        // current intensity level of active toy ((or new intensity level being sent)) (INT)
        baseString += $" currently set to ";
        baseString += $"{_characterHandler.playerChar._intensityLevel}";
        baseString += ". ";

        // does messageSender allow you to execute storedToyPatterns? (BOOL)
        baseString += _characterHandler.playerChar._uniquePlayerPerms[Idx]._allowUsingPatterns
        ? "The vibrator was able to execute set patterns" : "Unfortuintely the vibrator couldnt execute any patterns";
        baseString += ", ";

        // does messageSender allow you to lock the toybox UI? (BOOL)
        baseString += _characterHandler.playerChar._lockToyboxUI
        ? "with the viberator strapped tight to their skin" : "with the vibrator loosely tied to their skin";
        baseString += ". ";

        // for toy state
        baseString +=  _characterHandler.playerChar._isToyActive
        ? "Left on to buzz away" : "Left powered off";

        // for toy steps
        baseString += " with ";
        baseString += _plugService.stepCount;
        baseString += " notches on the slider.";

        return baseString;
    }
}
