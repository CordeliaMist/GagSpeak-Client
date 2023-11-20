using System;
using System.Linq;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using GagSpeak.UI.GagListings;
using System.Text.RegularExpressions;


using System;
using ImGuiNET;
using OtterGui.Widgets;
using Dalamud.Game.ClientState.Objects.Enums;
using System.Numerics;
using Dalamud.Plugin.Services;
using OtterGui;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Utility;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.GagListings;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using GagSpeak.Chat;
using Dalamud.Game.ClientState.Objects.SubKinds;
using GagSpeak.Chat.MsgEncoder;
using GagSpeak.Services;
namespace GagSpeak.Chat.MsgResultLogic;

public class MessageResultLogic { // Purpose of class : To perform logic on client based on the type of the sucessfully decoded message.
    
    private GagListingsDrawer _gagListingsDrawer;
    private readonly IChatGui _clientChat;
    private readonly GagSpeakConfig _config;
    private readonly IClientState _clientState;

    public MessageResultLogic(GagListingsDrawer gagListingsDrawer, IChatGui clientChat, GagSpeakConfig config, IClientState clientState) {
        _gagListingsDrawer = gagListingsDrawer;
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
    }
    
    /// <summary>
    /// Will take in a message, and determine what to do with it based on the contents of the message.
    /// <list>
    /// <item><c>apply GAGTYPE | PLAYER</c> - Equip Gagtype to defined layer</item>
    /// <item><c>lock LAYER LOCKTYPE | PLAYER</c> - Lock Gagtype to defined layer</item>
    /// <item><c>lock LAYER LOCKTYPE | PASSWORD | PLAYER</c> - Lock Gagtype to defined layer with password</item>
    /// <item><c>unlock LAYER | PLAYER</c> - Unlock Gagtype from defined layer</item>
    /// <item><c>unlock LAYER | PASSWORD | PLAYER</c> - Unlock Gagtype from defined layer with password</item>
    /// <item><c>removeall | PLAYER</c> - Remove all gags from player only when parameters are met</item>
    /// <item><c>remove LAYER | PLAYER</c> - Remove gag from defined layer</item></list>
    /// <para><c>recievedMessage</c><param name="receivedMessage"> - The message that was recieved from the player</param></para>
    /// </summary>
    public bool CommandMsgResLogic(string receivedMessage, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0].ToLowerInvariant();
        var _ = commandType switch
        {
            "lock"           => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "lockpassword"   => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "unlock"         => HandleUnlockMessage(ref decodedMessage, ref isHandled, config),
            "unlockpassword" => HandleUnlockMessage(ref decodedMessage, ref isHandled, config),
            "removeall"      => HandleRemoveAllMessage(ref decodedMessage, ref isHandled, config),
            "remove"         => HandleRemoveMessage(ref decodedMessage, ref isHandled, config),
            "apply"          => HandleApplyMessage(ref decodedMessage, ref isHandled, config),
            _                => LogError("Invalid message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    // another function nearly identical to the above, but for handling whitelist messages. These dont take as much processing.
    public bool WhitelistMsgResLogic(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0].ToLowerInvariant();
        var _ = commandType switch
        {
            "requestmistressrelation" => HandleRequestMistressMessage(ref decodedMessage, ref isHandled, config),
            "requestpetrelation"      => HandleRequestPetMessage(ref decodedMessage, ref isHandled, config),
            "requestslaverelation"    => HandleRequestSlaveMessage(ref decodedMessage, ref isHandled, config),
            "removeplayerrelation"    => HandleRelationRemovalMessage(ref decodedMessage, ref isHandled, config),
            "orderforcegarblelock"    => HandleLiveChatGarblerLockMessage(ref decodedMessage, ref isHandled, config),
            "requestinfo"             => HandleInformationRequestMessage(ref decodedMessage, ref isHandled, config),
            "acceptmistressrelation"  => HandleAcceptMistressMessage(ref decodedMessage, ref isHandled, config),
            "acceptpetrelation"       => HandleAcceptPetMessage(ref decodedMessage, ref isHandled, config),
            "acceptslaverelation"     => HandleAcceptSlaveMessage(ref decodedMessage, ref isHandled, config),
            "provideinfo"             => HandleProvideInfoMessage(ref decodedMessage, ref isHandled, config),
            "provideinfo2"            => HandleProvideInfo2Message(ref decodedMessage, ref isHandled, config),
            _                         => LogError("Invalid message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    bool LogError(string errorMessage) { // error log helper function
        GagSpeak.Log.Debug(errorMessage);
        _clientChat.PrintError(errorMessage);
        return false;
    }

    private bool IsMistress(string playerName) {
        PlayerPayload playerPayload; // get the current player info
        try { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); } catch { LogError("[MsgResultLogic]: Failed to get player payload."); return false; }
        // GagSpeak.Log.Debug($"PlayerName: {playerPayload.PlayerName}");
        // see if decodedMessage[4] == playerPayload.name
        if (playerName == playerPayload.PlayerName) {
            return true;}
        // see if any names in our whitelist are in decodedMessage[4] AND have their relation set to mistress
        if (_config.Whitelist.Any(w => playerName.Contains(w.name) && w.relationshipStatus == "Mistress")) {
            return true;}
        // otherwise return false
        return false;
    }

    // handle the lock message (realistically these should be falses, but they work in both cases and i am lazy)
    private bool HandleLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, check if we have valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; LogError("[MsgResultLogic]: Invalid layer value.");}
        
        // second, make sure already have a gag on
        if (_config.selectedGagTypes[layer-1] == "None") {
            isHandled = true; LogError($"[MsgResultLogic]: No gag applied for layer {layer}, cannot apply lock!");}
        
        // third, make sure we dont already have a lock here
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            isHandled = true; LogError($"[MsgResultLogic]: Already a lock applied to gag layer {layer}!");}
        
        // forth, now that everything is valid, perform remaining operations
        if (Enum.TryParse(decodedMessage[2], out GagPadlocks parsedLockType)) {
            // if the lock type is a mistress padlock
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock || _config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock) {
                // make sure we have someone who is a valid mistress doing this.
                if(IsMistress(decodedMessage[4])) {
                    _config.selectedGagPadlocksAssigner[layer-1] = decodedMessage[4];}
                else {
                    isHandled = true; LogError("[MsgResultLogic]: You must be a mistress to apply a mistress padlock!");}
            }
            // otherwise, handle the rest of the cases.
            _config.selectedGagPadlocks[layer-1] = parsedLockType; // update config list with new locktype
            _config._padlockIdentifier[layer-1]._padlockType = parsedLockType; // update the padlockidentifier with the new locktype
            if (decodedMessage[3] != "") { // If padlock contains password, make sure we set it to the appropriate padlockidentifier type
                _config.selectedGagPadlocksPassword[layer-1] = decodedMessage[3]; // set password in config
                if (decodedMessage[5] != "") { // set password in padlockIdentifier
                    _config._padlockIdentifier[layer-1].SetAndValidate(decodedMessage[2], decodedMessage[3], decodedMessage[5]);
                } else { // update padlockIdentifier with password
                    _config._padlockIdentifier[layer-1].SetAndValidate(decodedMessage[2], decodedMessage[3]);
                }
            }
        } else {
            isHandled = true; LogError("[MsgResultLogic]: Invalid /gag lock parameters sent in!");
        }
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag lock");
        _config._isLocked[layer-1] = true;
        _config._padlockIdentifier[layer-1].UpdateConfigPadlockPasswordInfo(layer-1, false, _config);
        // send sucessful message to chat
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} locked your {_config.selectedGagPadlocks} with a {_config.selectedGagPadlocks[layer-1]}.").AddItalicsOff().BuiltString);
        return true; // sucessful parse
    }

    // handle the unlock message
    private bool HandleUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure we have a lock on
        if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.None) {
            isHandled = true; LogError($"[MsgResultLogic]: No lock applied for layer {layer}, cannot remove lock!");}
    
        // third, see if we are unlocking without any password field
        if (decodedMessage[3] == "") {
            // if our padlock contains a password field, then throw error
            if (_config.selectedGagPadlocksPassword[layer-1] != string.Empty) {
                isHandled = true; LogError("[MsgResultLogic]: Cannot remove a password lock without a password!");}
            // If our padlock is a mistress related padlock, unlocker must match assigner.
            if ((_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock || _config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock)
            && !decodedMessage[4].Contains(_config.selectedGagPadlocksAssigner[layer-1])) {
                isHandled = true; LogError("[MsgResultLogic]: Cannot remove a mistress padlock's unless you are the one who assigned it.");}
            // if we made it here, we can just remove the lock
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "";
            _config._padlockIdentifier[layer-1].ClearPasswords(); // update padlockIdentifier to reflect changes
            _config._padlockIdentifier[layer-1].UpdateConfigPadlockPasswordInfo(layer-1, true, _config);
        } 
        // finally, see if we are unlocking with a password
        else {
            // if our passwords to not match, throw error
            if (_config.selectedGagPadlocksPassword[layer-1] != decodedMessage[3]) {
                isHandled = true; LogError("[MsgResultLogic]: Invalid password for this lock!");}
            // Assuming they do match (if we reached this point), check if it is a mistress padlock
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock && _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                isHandled = true; LogError("[MsgResultLogic]: Cannot remove a mistress padlock's unless you are the one who assigned it.");}
            // send sucessful message to chat
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} sucessfully unlocked the {_config.selectedGagPadlocks[layer-1]} from your {_config.selectedGagPadlocks}.").AddItalicsOff().BuiltString);
            // Remove the lock
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "";
            _config._padlockIdentifier[layer-1].ClearPasswords(); // update padlockIdentifier to reflect changes
            _config._padlockIdentifier[layer-1].UpdateConfigPadlockPasswordInfo(layer-1, true, _config);
        }
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag unlock");
        return true; // sucessful parse
    }

    // handle the remove message
    private bool HandleRemoveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure that this layer has a gag on it
        if (_config.selectedGagTypes[layer-1] == "None") {
            isHandled = true; LogError($"[MsgResultLogic]: There is no gag applied for gag layer {layer}, so no gag can be removed.");}
        // third, make sure there is no lock on that gags layer
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            isHandled = true; LogError($"[MsgResultLogic]: There is a lock applied for gag layer {layer}, cannot remove gag!");}
        // finally, if we made it here, we can remove the gag
        // but first, send sucessful message to chat
        // send sucessful message to chat
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} removed your {_config.selectedGagTypes[layer-1]}, how sweet.").AddItalicsOff().BuiltString);
        _config.selectedGagTypes[layer-1] = "None";
        _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
        _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
        _config.selectedGagPadlocksAssigner[layer-1] = "";
        _config._padlockIdentifier[layer-1].ClearPasswords();
        _config._padlockIdentifier[layer-1].UpdateConfigPadlockPasswordInfo(layer-1, true, _config);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag remove");
        return true; // sucessful parse
    }

    // handle the removeall message
    private bool HandleRemoveAllMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_config.selectedGagPadlocks.Any(padlock => padlock != GagPadlocks.None)) {
            isHandled = true; LogError("[MsgResultLogic]: Cannot remove all gags while locks are on any of them.");}
        // if we made it here, we can remove them all
        // but first, send sucessful message to chat
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has removed all of your gags.").AddItalicsOff().BuiltString);
        
        for (int i = 0; i < _config.selectedGagPadlocks.Count; i++) {
            _config.selectedGagTypes[i] = "None";
            _config.selectedGagPadlocks[i] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[i] = string.Empty;
            _config.selectedGagPadlocksAssigner[i] = "";
            _config._padlockIdentifier[i].ClearPasswords();
            _config._padlockIdentifier[i].UpdateConfigPadlockPasswordInfo(i, true, _config);
        }
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag removeall");
        return true; // sucessful parse
    }

    // handle the apply message
    private bool HandleApplyMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; LogError("[MsgResultLogic]: Invalid layer value.");}
        // secondly, see if our gagtype is in our list of gagtypes
        if (!_config.GagTypes.ContainsKey(decodedMessage[2]) && _config.selectedGagTypes[layer-1] != "None") {
            isHandled = true; LogError("[MsgResultLogic]: Invalid gag type.");}
        // if we make it here, apply the gag
        _config.selectedGagTypes[layer-1] = decodedMessage[2];
        // send sucessful message to chat
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You've been gagged by {playerName} with a {_config.selectedGagTypes[layer-1]}!").AddItalicsOff().BuiltString);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag apply");
        return true; // sucessful parse
    }

    // handle the request mistress message
    private bool HandleRequestMistressMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist.PendingRelationshipRequest = "Mistress"; // this means, they want to become YOUR mistress.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Mistress relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for a mistress relation request from {playerName}");
            }
        } catch {
            LogError($"ERROR, Invalid requestMistress message parse.");
        }
        return true;
    }

    // handle the request pet message, will be exact same as mistress one.
    private bool HandleRequestPetMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist.PendingRelationshipRequest = "Pet"; // this means, they want to become YOUR pet.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Pet relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for a pet relation request from {playerName}");
            }
        } catch {
            LogError($"ERROR, Invalid request pet message parse.");
        }
        return true;
    }

    // handle the request slave message
    private bool HandleRequestSlaveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist.PendingRelationshipRequest = "Slave"; // this means, they want to become YOUR slave.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Slave relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for a slave relation request from {playerName}");
            }
        } catch {
            LogError($"ERROR, Invalid request pet message parse.");
        }
        return true;
    }

    // handle the relation removal message
    private bool HandleRelationRemovalMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist.relationshipStatus = "None";
                playerInWhitelist.PendingRelationshipRequest = "None"; // this means, they want to become YOUR slave.
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Relation Status with {playerName} sucessfully removed.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for relation removal");
            }
        } catch {
            LogError($"ERROR, Invalid relation removal message parse.");
        }
        return true;
    }

    // handle the live chat garbler lock message
    private bool HandleLiveChatGarblerLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // just set the lockedLiveChatGarbler to true and print message. This will act like a toggle and do the inverse of whatever is currently set
        if(_config.LockDirectChatGarbler == false) {
            _config.DirectChatGarbler = true; _config.LockDirectChatGarbler = true;
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Your Mistress has decided you no longer have permission to speak clearly...").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse toggling livegarblerlock to ON for the slave.");
        }
        else {
            _config.DirectChatGarbler = false; _config.LockDirectChatGarbler = false;
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Your Mistress returns your permission to speak once more. How Generous...").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse toggling livegarblerlock to OFF for the slave.");
        }
        return true;
    }

    // handle the information request
    private bool HandleInformationRequestMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // because this command spits out our information about ourselves, we need an extra layer of security, making SURE the person 
        // using this on us HAS TO BE inside of our whitelist.
        try { 
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // they are in our whitelist, so set our information sender to the players name.
                _config.SendInfoName = playerNameWorld;
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Received info request from {playerName}. Providing Information in 4 seconds.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for recieving an information request message");
            }
        } catch {
            LogError($"ERROR, Invalid information request message parse.");
        }
        return true;
    }

    // handle the accept mistress request
    private bool HandleAcceptMistressMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist.relationshipStatus = "Mistress"; // no long pending, now official
                playerInWhitelist.PendingRelationshipRequest = "None"; // no long pending, now official
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is now your Mistress, enjoy~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Mistress relation");
            }
        } catch {
            LogError($"ERROR, Invalid accept mistress message parse.");
        }
        return true;
    }

    // handle the accept pet request
    private bool HandleAcceptPetMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist.relationshipStatus = "Pet"; // no long pending, now official
                playerInWhitelist.PendingRelationshipRequest = "None"; // no long pending, now official
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is now your Pet, enjoy~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Pet relation");
            }
        } catch {
            LogError($"ERROR, Invalid accept pet message parse.");
        }
        return true;
    }

    // handle the accept slave request
    private bool HandleAcceptSlaveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist.relationshipStatus = "Slave"; // no long pending, now official
                playerInWhitelist.PendingRelationshipRequest = "None"; // no long pending, now official
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is now your Slave, enjoy~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Slave relation");
            }
        } catch {
            LogError($"ERROR, Invalid accept Slave message parse.");
        }
        return true;
    }

    private string playerNameTemp = "";
    // handle the provide information message
    private bool HandleProvideInfoMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try {
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to non
                playerInWhitelist.isDomMode = decodedMessage[1] == "True" ? true : false;
                playerInWhitelist.garbleLevel = int.Parse(decodedMessage[3]);
                playerInWhitelist.selectedGagTypes[0] = decodedMessage[6];
                playerInWhitelist.selectedGagTypes[1] = decodedMessage[7];
                playerInWhitelist.selectedGagPadlocks[0] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[9]);
                playerInWhitelist.selectedGagPadlocks[1] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[10]);
                playerInWhitelist.selectedGagPadlocksAssigner[0] = decodedMessage[12];
                playerInWhitelist.selectedGagPadlocksAssigner[1] = decodedMessage[13];
                playerInWhitelist.selectedGagPadlocksTimer[0] = GetEndTime(decodedMessage[15]);
                playerInWhitelist.selectedGagPadlocksTimer[1] = GetEndTime(decodedMessage[16]);

                GagSpeak.Log.Debug($"[MsgResultLogic]: Received information response from {playerName} [Part 1/2]");
                playerNameTemp = playerName; // transfer over to the 2nd function
            }            
        } catch {
            LogError($"[MsgResultLogic]: Invalid provideInfo [1/2] message parse.");
        }        
        return true;
    }

    public static DateTimeOffset GetEndTime(string input) {
        // Match days, hours, minutes, and seconds in the input string
        var match = Regex.Match(input, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");

        if (match.Success) { 
            // Parse days, hours, minutes, and seconds
            int.TryParse(match.Groups[1].Value, out int days);
            int.TryParse(match.Groups[2].Value, out int hours);
            int.TryParse(match.Groups[3].Value, out int minutes);
            int.TryParse(match.Groups[4].Value, out int seconds);
            // Create a TimeSpan from the parsed values
            TimeSpan duration = new TimeSpan(days, hours, minutes, seconds);
            // Add the duration to the current DateTime to get a DateTimeOffset
            return DateTimeOffset.Now.Add(duration);
        }

        // If the input string is not in the correct format, throw an exception
        throw new FormatException($"[MsgResultLogic]: Invalid duration format: {input}");
    }

    private bool HandleProvideInfo2Message(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { 
            string playerName = playerNameTemp;
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to non
                playerInWhitelist.lockedLiveChatGarbler = decodedMessage[2] == "True" ? true : false;
                playerInWhitelist.relationshipStatus = decodedMessage[5];
                playerInWhitelist.selectedGagTypes[2] = decodedMessage[8];
                playerInWhitelist.selectedGagPadlocks[2] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[11]);
                playerInWhitelist.selectedGagPadlocksAssigner[2] = decodedMessage[14];
                playerInWhitelist.selectedGagPadlocksTimer[2] = GetEndTime(decodedMessage[17]);
                
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Finished Recieving Information from {playerName}.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Received information response from {playerName} [Part 2/2]");
            }            
        } catch {
            LogError($"[MsgResultLogic]: Invalid provideInfo [2/2] message parse.");
        }     
        return true;
    }
}
