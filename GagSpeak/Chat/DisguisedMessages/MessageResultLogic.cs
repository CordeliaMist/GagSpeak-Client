using System;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterGui.Classes;
using GagSpeak.Data;
using GagSpeak.UI.GagListings;
using GagSpeak.UI.Helpers;
using GagSpeak.Services;

namespace GagSpeak.Chat.MsgResultLogic;

/// <summary>
/// This class is used to handle the message result logic for decoded messages in the GagSpeak plugin.
/// </summary>
public class MessageResultLogic
{    
    private          GagListingsDrawer _gagListingsDrawer; // used to draw the gag listings
    private readonly IChatGui           _clientChat;       // used to print messages to the chat
    private readonly GagSpeakConfig     _config;           // used to get the config
    private readonly IClientState       _clientState;      // used to get the client state
    private readonly GagAndLockManager  _lockManager;      // used to get the lock manager
    private readonly GagService         _gagService;       // used to get the gag service

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageResultLogic"/> class.
    /// <list type="bullet">
    /// <item><c>gagListingsDrawer</c><param name="gagListingsDrawer"> - The GagListingsDrawer.</param></item>
    /// <item><c>clientChat</c><param name="clientChat"> - The IChatGui.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>clientState</c><param name="clientState"> - The IClientState.</param></item>
    /// <item><c>lockManager</c><param name="lockManager"> - The GagAndLockManager.</param></item>
    /// </list> </summary>
    public MessageResultLogic(GagListingsDrawer gagListingsDrawer, IChatGui clientChat, GagSpeakConfig config,
    IClientState clientState, GagAndLockManager lockManager, GagService gagService) {
        _gagListingsDrawer = gagListingsDrawer;
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
        _lockManager = lockManager;
        _gagService = gagService;
    }
    
    /// <summary>
    /// This function is used to handle the message result logic for decoded messages involing your player in the GagSpeak plugin.
    /// <list type="bullet">
    /// <item><c>receivedMessage</c><param name="receivedMessage"> - The message that was received.</param></item>
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>clientChat</c><param name="clientChat"> - The IChatGui.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool CommandMsgResLogic(string receivedMessage, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0].ToLowerInvariant();
        var _ = commandType switch
        {
            "lock"              => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "lockpassword"      => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "locktimerpassword" => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "unlock"            => HandleUnlockMessage(ref decodedMessage, ref isHandled, config),
            "unlockpassword"    => HandleUnlockMessage(ref decodedMessage, ref isHandled, config),
            "removeall"         => HandleRemoveAllMessage(ref decodedMessage, ref isHandled, config),
            "remove"            => HandleRemoveMessage(ref decodedMessage, ref isHandled, config),
            "apply"             => HandleApplyMessage(ref decodedMessage, ref isHandled, config),
            _                => LogError("Invalid message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary>
    /// This function is used to handle the message result logic for decoded messages involving a whitelisted player in the GagSpeak plugin.
    /// <list type="bullet">
    /// <item><c>receivedMessage</c><param name="receivedMessage"> - The message that was received.</param></item>
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>clientChat</c><param name="clientChat"> - The IChatGui.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled.</returns>
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
            "declineMistressRelation" => HandleDeclineMistressMessage(ref decodedMessage, ref isHandled, config),
            "declinePetRelation"      => HandleDeclinePetMessage(ref decodedMessage, ref isHandled, config),
            "declineSlaveRelation"    => HandleDeclineSlaveMessage(ref decodedMessage, ref isHandled, config), 
            "provideinfo"             => HandleProvideInfoMessage(ref decodedMessage, ref isHandled, config),
            "provideinfo2"            => HandleProvideInfo2Message(ref decodedMessage, ref isHandled, config),
            _                         => LogError("Invalid message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> A simple helper function to log errors to both /xllog and your chat. </summary>
    bool LogError(string errorMessage) {
        GagSpeak.Log.Debug(errorMessage);
        _clientChat.PrintError(errorMessage);
        return false;
    }

    /// <summary>
    /// handle the lock message [commandtype, layer, gagtype/locktype, password, player, password2]
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, check if we have valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure already have a gag on
        if (_config.selectedGagTypes[layer-1] == "None") {
            isHandled = true; return LogError($"[MsgResultLogic]: No gag applied for layer {layer}, cannot apply lock!");}
        // third, make sure we dont already have a lock here
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: Already a lock applied to gag layer {layer}!");}
        // all preconditions met, so now we can try to lock it.
        if (Enum.TryParse(decodedMessage[2], out GagPadlocks parsedLockType)) {
            // get our payload
            PlayerPayload playerPayload; // get player payload
            UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
            string[] nameParts = decodedMessage[4].Split(' ');
            decodedMessage[4] = nameParts[0] + " " + nameParts[1];
            // if the lock type is a mistress padlock, make sure the assigner is a mistress
            _config._padlockIdentifier[layer-1].SetType(parsedLockType); // set the type of the padlock
            _lockManager.Lock((layer-1), decodedMessage[4], decodedMessage[3], decodedMessage[5], playerPayload.PlayerName);
            // if we reached this point, it means we sucessfully locked the layer
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerPayload.PlayerName} locked your " +
            $"{_config.selectedGagTypes[layer-1]} with a {_config.selectedGagPadlocks[layer-1]}.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag lock");
            return true; // sucessful parse
        } else {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid /gag lock parameters sent in!");
        }
    }

    /// <summary>
    /// handle the unlock message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure we have a lock on
        if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: No lock applied for layer {layer}, cannot remove lock!");}
        // if we made it here, we can try to unlock it.
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        string[] nameParts = decodedMessage[4].Split(' ');
        decodedMessage[4] = nameParts[0] + " " + nameParts[1];
        // try to unlock it
        if(_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock || _config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock) {
            if(decodedMessage[4] != _config.selectedGagPadlocksAssigner[layer-1]) {
                isHandled = true; return LogError($"[MsgResultLogic]: {decodedMessage[4]} is not the assigner of the lock on layer {layer}!");
            }
        }
        _lockManager.Unlock((layer-1), decodedMessage[4], decodedMessage[3], playerPayload.PlayerName); // attempt to unlock
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{decodedMessage[4]} " +
        $"sucessfully unlocked the {_config.selectedGagPadlocks[layer-1]} from your {_config.selectedGagPadlocks}.").AddItalicsOff().BuiltString);        
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag unlock");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the remove message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleRemoveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // second, make sure that this layer has a gag on it
        if (_config.selectedGagTypes[layer-1] == "None") {
            isHandled = true; return LogError($"[MsgResultLogic]: There is no gag applied for gag layer {layer}, so no gag can be removed.");}
        // third, make sure there is no lock on that gags layer
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            isHandled = true; return LogError($"[MsgResultLogic]: There is a lock applied for gag layer {layer}, cannot remove gag!");}
        // finally, if we made it here, we can remove the gag
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} removed your {_config.selectedGagTypes[layer-1]}, how sweet.").AddItalicsOff().BuiltString);
        _lockManager.RemoveGag(layer-1); // remove the gag
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag remove");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the removeall message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns> Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleRemoveAllMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_config.selectedGagPadlocks.Any(padlock => padlock != GagPadlocks.None)) {
            isHandled = true; return LogError("[MsgResultLogic]: Cannot remove all gags while locks are on any of them.");}
        // if we made it here, we can remove them all
        string playerNameWorld = decodedMessage[4]; 
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has removed all of your gags.").AddItalicsOff().BuiltString);
        _lockManager.RemoveAllGags(); // remove all gags
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag removeall");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the gag apply message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleApplyMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // first, see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            isHandled = true; return LogError("[MsgResultLogic]: Invalid layer value.");}
        // secondly, see if our gagtype is in our list of gagtypes
        string gagName = decodedMessage[2];
        if (!_gagService._gagTypes.Any(gag => gag._gagName == gagName) && _config.selectedGagTypes[layer-1] != "None") {
            isHandled = true; return LogError("[MsgResultLogic]: Invalid gag type.");}
        // if we make it here, apply the gag
        _lockManager.ApplyGag(layer-1, decodedMessage[2]);
        // send sucessful message to chat
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You've been gagged by {playerName} with a {_config.selectedGagTypes[layer-1]}!").AddItalicsOff().BuiltString);
        GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /gag apply");
        return true; // sucessful parse
    }

    /// <summary>
    /// handle the request mistress message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
                playerInWhitelist.PendingRelationRequestFromPlayer = "Mistress"; // this means, they want to become YOUR mistress.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Mistress relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for a mistress relation request from {playerName}");
            }
        } catch {
            return LogError($"ERROR, Invalid requestMistress message parse.");
        }
        return true;
    }

    /// <summary>
    /// handle the request pet message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleRequestPetMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { 
            // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist.PendingRelationRequestFromPlayer = "Pet"; // this means, they want to become YOUR pet.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Pet relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse pet relation request from {playerName}: {playerInWhitelist.PendingRelationRequestFromPlayer}");
            }
        } catch {
            return LogError($"ERROR, Invalid request pet message parse.");
        }
        return true;
    }

    /// <summary>
    /// handle the request slave message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleRequestSlaveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { 
            // whole goal is just to apply the request update to the players whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string world = parts.Length > 1 ? parts.Last() : "";
            string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist.PendingRelationRequestFromPlayer = "Slave"; // this means, they want to become YOUR slave.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Slave relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for a slave relation request from {playerName}");
            }
        } catch {
            return LogError($"ERROR, Invalid request pet message parse.");
        }
        return true;
    }

    /// <summary>
    /// handle the relation removal message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
                playerInWhitelist.relationshipStatusToYou = "None";
                playerInWhitelist.PendingRelationRequestFromYou = "";
                playerInWhitelist.PendingRelationRequestFromPlayer = "";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Relation Status with {playerName} sucessfully removed.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for relation removal");
            }
        } catch {
            return LogError($"ERROR, Invalid relation removal message parse.");
        }
        return true;
    }

    /// <summary>
    /// handle the livechat garbler lock message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleLiveChatGarblerLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // locate the player in the whitelist matching the playername in the list
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
        // see if they exist AND sure they have a mistress relation on your end
        if(playerInWhitelist != null && playerInWhitelist.relationshipStatus == "Mistress") {
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
        }
        else {
            return LogError($"ERROR, Invalid live chat garbler lock message parse.");
        }
        return true;
    }

    /// <summary>
    /// handle the information request message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
            return LogError($"ERROR, Invalid information request message parse.");
        }
        return true;
    }

    /// <summary> handle the accept mistress (this comes from the player approving your request) <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item> </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
                playerInWhitelist.relationshipStatus = "Mistress";
                playerInWhitelist.PendingRelationRequestFromYou = "Established";
                if(playerInWhitelist.relationshipStatusToYou != "None") { playerInWhitelist.SetTimeOfCommitment(); } // set the commitment time if relationship is now two-way!
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s mistress. Enjoy~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Mistress relation");
            }
        } catch {
            return LogError($"ERROR, Invalid accept mistress message parse.");
        }
        return true;
    }

    /// <summary> handle the accept pet request (this comes from the player approving your request) <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item> </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
                playerInWhitelist.relationshipStatus = "Pet";
                playerInWhitelist.PendingRelationRequestFromYou = "Established";
                if(playerInWhitelist.relationshipStatusToYou != "None") { playerInWhitelist.SetTimeOfCommitment(); } // set the commitment time if relationship is now two-way!
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s pet. Enjoy yourself~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Pet relation");
            }
        } catch {
            return LogError($"ERROR, Invalid accept pet message parse.");
        }
        return true;
    }

    /// <summary> handle the accept slave (this comes from the player approving your request) <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item> </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
                playerInWhitelist.relationshipStatus = "Slave";
                playerInWhitelist.PendingRelationRequestFromYou = "Established";
                if(playerInWhitelist.relationshipStatusToYou != "None") { playerInWhitelist.SetTimeOfCommitment(); } // set the commitment time if relationship is now two-way!
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s slave, Be sure to Behave~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Slave relation");
            }
        } catch {
            return LogError($"ERROR, Invalid accept Slave message parse.");
        }
        return true;
    }

    /// <summary> handle the decline mistress (this comes from the player declining your request) <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item> </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleDeclineMistressMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
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
                playerInWhitelist.relationshipStatus = "Mistress";
                playerInWhitelist.PendingRelationRequestFromYou = "Established";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s mistress. Enjoy~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Declineing Mistress relation");
            }
        } catch {
            return LogError($"ERROR, Invalid Decline mistress message parse.");
        }
        return true;
    }

    /// <summary> handle the Decline pet request (this comes from the player declining your request) <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item> </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleDeclinePetMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
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
                playerInWhitelist.relationshipStatus = "Pet";
                playerInWhitelist.PendingRelationRequestFromYou = "Established";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s pet. Enjoy yourself~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Declineing Pet relation");
            }
        } catch {
            return LogError($"ERROR, Invalid Decline pet message parse.");
        }
        return true;
    }

    /// <summary> handle the Decline slave (this comes from the player declining your request) <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item> </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleDeclineSlaveMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
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
                playerInWhitelist.relationshipStatus = "Slave";
                playerInWhitelist.PendingRelationRequestFromYou = "Established";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s slave, Be sure to Behave~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Declineing Slave relation");
            }
        } catch {
            return LogError($"ERROR, Invalid Decline Slave message parse.");
        }
        return true;
    }

    private string playerNameTemp = "";
    /// <summary>
    /// handle the provide info message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
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
                playerInWhitelist.isDomMode = decodedMessage[1] == "true" ? true : false;
                playerInWhitelist.garbleLevel = int.Parse(decodedMessage[3]);
                playerInWhitelist.selectedGagTypes[0] = decodedMessage[6];
                playerInWhitelist.selectedGagTypes[1] = decodedMessage[7];
                playerInWhitelist.selectedGagPadlocks[0] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[9]);
                playerInWhitelist.selectedGagPadlocks[1] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[10]);
                playerInWhitelist.selectedGagPadlocksAssigner[0] = decodedMessage[12];
                playerInWhitelist.selectedGagPadlocksAssigner[1] = decodedMessage[13];
                playerInWhitelist.selectedGagPadlocksTimer[0] = UIHelpers.GetEndTime(decodedMessage[15]);
                playerInWhitelist.selectedGagPadlocksTimer[1] = UIHelpers.GetEndTime(decodedMessage[16]);

                GagSpeak.Log.Debug($"[MsgResultLogic]: Received information response from {playerName} [Part 1/2]");
                playerNameTemp = playerName; // transfer over to the 2nd function
            }            
        } catch {
            return LogError($"[MsgResultLogic]: Invalid provideInfo [1/2] message parse.");
        }        
        return true;
    }

    /// <summary>
    /// handle the provide info 2 message
    /// <list type="bullet">
    /// <item><c>decodedMessage</c><param name="decodedMessage"> - The decoded message.</param></item>
    /// <item><c>isHandled</c><param name="isHandled"> - Whether or not the message has been handled.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>Whether or not the message has been handled, along with the updated decoded message list.</returns>
    private bool HandleProvideInfo2Message(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try { 
            string playerName = playerNameTemp;
            // locate player in whitelist
            var playerInWhitelist = _config.Whitelist.FirstOrDefault(x => x.name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to non
                playerInWhitelist.lockedLiveChatGarbler = decodedMessage[2] == "True" ? true : false;
                //playerInWhitelist.relationshipStatus = decodedMessage[5];
                playerInWhitelist.selectedGagTypes[2] = decodedMessage[8];
                playerInWhitelist.selectedGagPadlocks[2] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[11]);
                playerInWhitelist.selectedGagPadlocksAssigner[2] = decodedMessage[14];
                playerInWhitelist.selectedGagPadlocksTimer[2] = UIHelpers.GetEndTime(decodedMessage[17]);
                
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Finished Recieving Information from {playerName}.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Received information response from {playerName} [Part 2/2]");
            }            
        } catch {
            return LogError($"[MsgResultLogic]: Invalid provideInfo [2/2] message parse.");
        }     
        return true;
    }
}