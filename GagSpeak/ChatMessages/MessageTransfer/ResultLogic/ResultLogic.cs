using System;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterGui.Classes;
using GagSpeak.Data;
using GagSpeak.UI.ComboListings;
using GagSpeak.Utility;
using GagSpeak.Services;
using GagSpeak.Wardrobe;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary>
/// This class is used to handle the message result logic for decoded messages in the GagSpeak plugin.
/// </summary>
public partial class ResultLogic {    
    private          GagListingsDrawer      _gagListingsDrawer;     // used to draw the gag listings
    private readonly IChatGui               _clientChat;            // used to print messages to the chat
    private readonly GagSpeakConfig         _config;                // used to get the config
    private readonly GagStorageManager      _gagStorageManager;     // used to get the gag storage
    private readonly RestraintSetManager    _restraintSetManager;   // used to get the restraint set manager
    private readonly IClientState           _clientState;           // used to get the client state
    private readonly GagAndLockManager      _lockManager;           // used to get the lock manager
    private readonly GagService             _gagService;            // used to get the gag service
    private readonly TimerService           _timerService;          // used to get the timer service

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageResultLogic"/> class.
    /// <list type="bullet">
    /// <item><c>gagListingsDrawer</c><param name="gagListingsDrawer"> - The GagListingsDrawer.</param></item>
    /// <item><c>clientChat</c><param name="clientChat"> - The IChatGui.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>clientState</c><param name="clientState"> - The IClientState.</param></item>
    /// <item><c>lockManager</c><param name="lockManager"> - The GagAndLockManager.</param></item>
    /// </list> </summary>
    public ResultLogic(GagListingsDrawer gagListingsDrawer, IChatGui clientChat, GagSpeakConfig config,
    IClientState clientState, GagAndLockManager lockManager, GagService gagService, TimerService timerService,
    GagStorageManager gagStorageManager, RestraintSetManager restraintSetManager) {
        _gagListingsDrawer = gagListingsDrawer;
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
        _lockManager = lockManager;
        _gagService = gagService;
        _timerService = timerService;
        _gagStorageManager = gagStorageManager;
        _restraintSetManager = restraintSetManager;
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
            _                => LogError("Invalid Order message parse, If you see this report it to cordy ASAP.")
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
            "declinemistressrelation" => HandleDeclineMistressMessage(ref decodedMessage, ref isHandled, config),
            "declinepetrelation"      => HandleDeclinePetMessage(ref decodedMessage, ref isHandled, config),
            "declineslaverelation"    => HandleDeclineSlaveMessage(ref decodedMessage, ref isHandled, config), 
            "provideinfo"             => HandleProvideInfoMessage(ref decodedMessage, ref isHandled, config),
            "provideinfo2"            => HandleProvideInfo2Message(ref decodedMessage, ref isHandled, config),
            "restraintsetlock"        => HandleRestraintSetLockMessage(ref decodedMessage, ref isHandled, config),
            "restraintsetunlock"      => HandleRestraintSetUnlockMessage(ref decodedMessage, ref isHandled, config),
            _                         => LogError("Invalid Whitelist message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> A simple helper function to log errors to both /xllog and your chat. </summary>
    bool LogError(string errorMessage) {
        GagSpeak.Log.Debug(errorMessage);
        _clientChat.PrintError(errorMessage);
        return false;
    }

#region GagSpeak Whitelist Command Logic
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist._pendingRelationRequestFromPlayer = "Mistress"; // this means, they want to become YOUR mistress.
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist._pendingRelationRequestFromPlayer = "Pet"; // this means, they want to become YOUR pet.
                // Notify the user that someone wishes to establish a relationship
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Pet relation request recieved from {playerName}. Accept or Decline via whitelist profile.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse pet relation request from {playerName}: {playerInWhitelist._pendingRelationRequestFromPlayer}");
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                playerInWhitelist._pendingRelationRequestFromPlayer = "Slave"; // this means, they want to become YOUR slave.
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._yourStatusToThem = "None";
                playerInWhitelist._theirStatusToYou = "None";
                playerInWhitelist._pendingRelationRequestFromYou = "";
                playerInWhitelist._pendingRelationRequestFromPlayer = "";
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
        var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
        // see if they exist AND sure they have a mistress relation on your end
        if(playerInWhitelist != null && playerInWhitelist._yourStatusToThem == "Slave" && playerInWhitelist._theirStatusToYou == "Mistress") {
            if(_config.playerInfo._directChatGarblerLocked == false) {
                _config.playerInfo._directChatGarblerActive = true; _config.playerInfo._directChatGarblerLocked = true;
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Your Mistress has decided you no longer have permission to speak clearly...").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse toggling livegarblerlock to ON for the slave.");
            }
            else {
                _config.playerInfo._directChatGarblerActive = false; _config.playerInfo._directChatGarblerLocked = false;
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Your Mistress returns your permission to speak once more. How Generous...").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse toggling livegarblerlock to OFF for the slave.");
            }
        }
        else {
            // let the user know they do not have the nessisary relation configuration to do this
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"You are not a slave to this person, or this person is not your Mistress. Both must be true for this to work.").AddItalicsOff().BuiltString);
            return false;
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // they are in our whitelist, so set our information sender to the players name.
                _config.sendInfoName = playerName + "@" + world;
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} is requesting an update on your info for the profile viewer." +
                "Providing Over the next 3 Seconds.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for recieving an information request message");

                // invoke the interaction button cooldown timer
                _timerService.StartTimer("InteractionCooldown", "3s", 100, () => {});
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._yourStatusToThem = "Mistress";
                playerInWhitelist._pendingRelationRequestFromYou = "Established";
                if(playerInWhitelist._theirStatusToYou != "None") { playerInWhitelist.Set_timeOfCommitment(); } // set the commitment time if relationship is now two-way!
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._yourStatusToThem = "Pet";
                playerInWhitelist._pendingRelationRequestFromYou = "Established";
                if(playerInWhitelist._theirStatusToYou != "None") { playerInWhitelist.Set_timeOfCommitment(); } // set the commitment time if relationship is now two-way!
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s pet. Enjoy yourself~.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting Pet relation");
            } else {
                GagSpeak.Log.Debug($"[MsgResultLogic]: Player {playerName} not found in whitelist.");
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._yourStatusToThem = "Slave";
                playerInWhitelist._pendingRelationRequestFromYou = "Established";
                if(playerInWhitelist._theirStatusToYou != "None") { playerInWhitelist.Set_timeOfCommitment(); } // set the commitment time if relationship is now two-way!
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._pendingRelationRequestFromYou = "";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has declined your offer to be their mistress.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Declining Mistress relation");
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._pendingRelationRequestFromYou = "";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has declined your offer to be their pet.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Declining Pet relation");
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
            var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // see if they exist
            if(playerInWhitelist != null) {
                // set the pending relationship to none and relationship with that player to none
                playerInWhitelist._pendingRelationRequestFromYou = "";
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has declined your offer to be their slave.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Declining Slave relation");
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
            // string playerNameWorld = decodedMessage[4];
            // string[] parts = playerNameWorld.Split(' ');
            // string world = parts.Length > 1 ? parts.Last() : "";
            // string playerName = string.Join(" ", parts.Take(parts.Length - 1));
            // // locate player in whitelist
            // var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // // see if they exist
            // if(playerInWhitelist != null) {
            //     // set the pending relationship to none and relationship with that player to non
            //     //playerInWhitelist.isDomMode = decodedMessage[1] == "true" ? true : false; DEPRICATED
            //     //playerInWhitelist.garbleLevel = int.Parse(decodedMessage[3]); DEPRICATED
            //     playerInWhitelist.selectedGagTypes[0] = decodedMessage[6];
            //     playerInWhitelist.selectedGagTypes[1] = decodedMessage[7];
            //     playerInWhitelist.selectedGagPadlocks[0] = (Padlocks)Enum.Parse(typeof(Padlocks), decodedMessage[9]);
            //     playerInWhitelist.selectedGagPadlocks[1] = (Padlocks)Enum.Parse(typeof(Padlocks), decodedMessage[10]);
            //     playerInWhitelist.selectedGagPadlocksAssigner[0] = decodedMessage[12];
            //     playerInWhitelist.selectedGagPadlocksAssigner[1] = decodedMessage[13];
            //     playerInWhitelist.selectedGagPadlocksTimer[0] = UIHelpers.GetEndTime(decodedMessage[15]);
            //     playerInWhitelist.selectedGagPadlocksTimer[1] = UIHelpers.GetEndTime(decodedMessage[16]);

            //     GagSpeak.Log.Debug($"[MsgResultLogic]: Received information response from {playerName} [Part 1/2]");
            //     playerNameTemp = playerName; // transfer over to the 2nd function
            GagSpeak.Log.Debug($"If you are ever debugging this cordy, you have yet to implement the new provide info message.");            
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
            // string playerName = playerNameTemp;
            // // locate player in whitelist
            // var playerInWhitelist = _config.whitelist.FirstOrDefault(x => x._name == playerName);
            // // see if they exist
            // if(playerInWhitelist != null) {
            //     // set the pending relationship to none and relationship with that player to non
            //     playerInWhitelist.lockedLiveChatGarbler = decodedMessage[2] == "True" ? true : false;
            //     //playerInWhitelist._yourStatusToThem = decodedMessage[5];
            //     playerInWhitelist.selectedGagTypes[2] = decodedMessage[8];
            //     playerInWhitelist.selectedGagPadlocks[2] = (Padlocks)Enum.Parse(typeof(Padlocks), decodedMessage[11]);
            //     playerInWhitelist.selectedGagPadlocksAssigner[2] = decodedMessage[14];
            //     playerInWhitelist.selectedGagPadlocksTimer[2] = UIHelpers.GetEndTime(decodedMessage[17]);
                
            //     _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Finished Recieving Information from {playerName}.").AddItalicsOff().BuiltString);
            //     GagSpeak.Log.Debug($"[MsgResultLogic]: Received information response from {playerName} [Part 2/2]");

                GagSpeak.Log.Debug($"If you are ever debugging this cordy, you have yet to implement the new provide info message.");          
        } catch {
            return LogError($"[MsgResultLogic]: Invalid provideInfo [2/2] message parse.");
        }     
        return true;
    }
#endregion GagSpeak Whitelist Command Logic

#region GagSpeak Wardrobe Command Logic
    /// <summary>
    /// Locks the defined restraint set if it exists and player is in your whitelist
    /// { [0] = "restraintSetLock", [1] = restraint set name, [2] = timer in formatted string, [4] = "playerName world" }
    /// </summary>
    private bool HandleRestraintSetLockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try {
            if(_config.InDomMode) {
                isHandled = true;
                return LogError($"[MsgResultLogic]: You cannot lock a restraint set while in Dom Mode.");
            }
            PlayerPayload playerPayload; // get player payload
            UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
            // first, see if our restraint set exists
            string restraintSetName = decodedMessage[1];
            if (!_restraintSetManager._restraintSets.Any(restraintSet => restraintSet._name == restraintSetName)) {
                isHandled = true; return LogError($"[MsgResultLogic]: Restraint Set Name {restraintSetName} was attempted to be applied to you, but the set does not exist!");}
            // secondly, see if our player is in our whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string AssignerPlayerName = string.Join(" ", parts.Take(parts.Length - 1));
            // if we make it here, the timer was already validated upon sending it, so it will be valid here, and we can set it.
            if(_lockManager.LockRestraintSet(restraintSetName, AssignerPlayerName, decodedMessage[2], playerPayload.PlayerName)) {
                // send sucessful message to chat
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{AssignerPlayerName} locked {playerPayload.PlayerName}'s "+
                $"{restraintSetName} restraint set for {decodedMessage[2]}.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /restraintset lock");
                return true; // sucessful parse
            } else {
                isHandled = true;
                return false;
            }
        } catch {
            return LogError($"ERROR, Invalid restraintSetLock message parse.");
        }
    }

    /// <summary>
    /// Unlocks the defined restraint set if it exists and player is in your whitelist
    /// { [0] = "restraintSetUnlock", [1] = restraint set name, [4] = "playerName world ASSIGNER" }
    /// </summary>
    private bool HandleRestraintSetUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        try {
            if(_config.InDomMode) {
                isHandled = true;
                return LogError($"[MsgResultLogic]: You cannot lock a restraint set while in Dom Mode.");
            }
            PlayerPayload playerPayload; // get player payload
            UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
            // first, see if our restraint set exists
            string restraintSetName = decodedMessage[1];
            if (!_restraintSetManager._restraintSets.Any(restraintSet => restraintSet._name == restraintSetName)) {
                isHandled = true; return LogError($"[MsgResultLogic]: Restraint Set Name {restraintSetName} was attempted to be applied to you, but the set does not exist!");}
            // secondly, see if our player is in our whitelist
            string playerNameWorld = decodedMessage[4];
            string[] parts = playerNameWorld.Split(' ');
            string AssignerPlayerName = string.Join(" ", parts.Take(parts.Length - 1));
            if (!_config.whitelist.Any(player => player._name == AssignerPlayerName)) {
                if(AssignerPlayerName != playerPayload.PlayerName){
                    isHandled = true;
                    return LogError("[MsgResultLogic]: Player is not in your whitelist.");
                }
            }
            // if we make it here, the timer was already validated upon sending it, so it will be valid here, and we can set it.
            if(_lockManager.UnlockRestraintSet(restraintSetName, AssignerPlayerName)) {
                // send sucessful message to chat
                _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{AssignerPlayerName} unlocked {playerPayload.PlayerName}'s {restraintSetName} restraint set.").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for /restraintset unlock");
                return true; // sucessful parse
            } else {
                isHandled = true;
                return false;
            }
        } catch {
            return LogError($"ERROR, Invalid restraintSetUnlock message parse.");
        }
    }
#endregion GagSpeak Wardrobe Command Logic
}