using System;
using System.Linq;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using GagSpeak.UI.GagListings;

namespace GagSpeak.Chat.MsgResultLogic;

public class MessageResultLogic { // Purpose of class : To perform logic on client based on the type of the sucessfully decoded message.
    
    private GagListingsDrawer _gagListingsDrawer;

    public MessageResultLogic(GagListingsDrawer gagListingsDrawer) {
        _gagListingsDrawer = gagListingsDrawer;
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
            "lock"           => HandleLockMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "lockpassword"   => HandleLockMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "unlock"         => HandleUnlockMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "unlockpassword" => HandleUnlockMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "removeall"      => HandleRemoveAllMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "remove"         => HandleRemoveMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "apply"          => HandleApplyMessage(ref decodedMessage, ref isHandled, clientChat, config),
            _                => LogError("Invalid message parse, If you see this report it to cordy ASAP.")
        };
        return true;

        bool LogError(string errorMessage) {
            GagSpeak.Log.Debug(errorMessage);
            clientChat.PrintError(errorMessage);
            return false;
        }
    }

    // another function nearly identical to the above, but for handling whitelist messages. These dont take as much processing.
    public bool WhitelistMsgResLogic(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0].ToLowerInvariant();
        var _ = commandType switch
        {
            "requestmistressrelation" => HandleRequestMistressMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "requestpetrelation"      => HandleRequestPetMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "requestslaverelation"    => HandleRequestSlaveMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "removeplayerrelation"    => HandleRelationRemovalMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "orderforcegarblelock"    => HandleLiveChatGarblerLockMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "requestinfo"             => HandleInformationRequestMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "acceptmistressrelation"  => HandleAcceptMistressMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "acceptpetrelation"       => HandleAcceptPetMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "acceptslaverelation"     => HandleAcceptSlaveMessage(ref decodedMessage, ref isHandled, clientChat, config),
            "provideinfo"             => HandleProvideInfoMessage(ref decodedMessage, ref isHandled, clientChat, config),
            _                         => LogError("Invalid message parse, If you see this report it to cordy ASAP.")
        };
        return true;

        bool LogError(string errorMessage) {
            GagSpeak.Log.Debug(errorMessage);
            clientChat.PrintError(errorMessage);
            return false;
        }
    }


    // Below are all of the sub-functions that are called by the main function above, each split to their own decoded message catagory.

    // handle the lock message (realistically these should be falses, but they work in both cases and i am lazy)
    private bool HandleLockMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // Our layer is valid, but we also need to make sure that we have a gag on this layer
        if (_config.selectedGagTypes[layer-1] == "None") {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is no gag applied for layer {layer}, so no lock can be applied.");
            _clientChat.PrintError($"ERROR, There is no gag applied for layer {layer}, so no lock can be applied.");
            return true;
        }
        // if we do have a gag on this layer, make sure that we dont already have a lock here
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is already a lock applied to gag layer {layer}!");
            _clientChat.PrintError($"ERROR, There is already a lock applied to gag layer {layer}!");
            return true;
        }
        // we already made sure that we applied a valid password in the command manager, so no need to check it here.
        if (decodedMessage[3] != "") {
            _config.selectedGagPadlocksPassword[layer-1] = decodedMessage[3]; // we have a password to set, so set it.
            // updating according thing in general tab.
            _config._padlockIdentifier[layer-1]._storedCombination = decodedMessage[3];
            GagSpeak.Log.Debug($"setting padlock identifier #{layer-1} to password {decodedMessage[3]}");
        }
        // and because everything above is valid, we can now set the lock type.
        if (Enum.TryParse(decodedMessage[2], out GagPadlocks parsedLockType)) {
            _config.selectedGagPadlocks[layer-1] = parsedLockType;
            // updating according thing in general tab.
            _config._padlockIdentifier[layer-1]._padlockType = parsedLockType;
            GagSpeak.Log.Debug($"setting padlock identifier #{layer-1} to type {parsedLockType}");
        } else {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid lock type sent in.");
            _clientChat.PrintError($"ERROR, Invalid lock type sent in.");
            return true;
        }
        // now that we have applied our gagtype, and potentially password, set the assigner to the player if it is a mistress padlock.
        if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock || _config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock) {
            _config.selectedGagPadlocksAssigner[layer-1] = decodedMessage[4];
        }
        GagSpeak.Log.Debug($"Determined income message as a [lock] type encoded message, hiding from chat!");
        _config._isLocked[layer-1] = true;
        _config._padlockIdentifier[layer-1].UpdateConfigPadlockPasswordInfo(layer-1, false, _config);
        return true; // sucessful parse
    }

    // handle the unlock message
    private bool HandleUnlockMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // our layer is valid, but we also need to make sure that this layer has a lock on it
        if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.None) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is no lock applied for gag layer {layer}, so no lock can be removed.");
            _clientChat.PrintError($"ERROR, There is no lock applied for gag layer {layer}, so no lock can be removed.");
            return true;
        }
        // Case where it is just unlock
        if (decodedMessage[3] == "") {
            // Make sure it is not a MistressPadlock
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressPadlock && _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                _clientChat.PrintError($"ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                return true;
            }
            // if we made it here, we can just remove the lock
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "None";
        } else {
            // if we do have a password, we need to make sure it matches the password on the lock
            if (_config.selectedGagPadlocksPassword[layer-1] != decodedMessage[3]) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Invalid Password, failed to unlock.");
                _clientChat.PrintError($"ERROR, Invalid Password, failed to unlock.");
                return true;
            }
            // if the passwords do match, so remove the lock IF it is not a mistress padlock.
            if (_config.selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock &&
                _config.selectedGagPadlocksAssigner[layer-1] != decodedMessage[4]) {
                // hide original message & throw exception
                isHandled = true;
                GagSpeak.Log.Debug("ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                _clientChat.PrintError($"ERROR, Cannot remove a mistress padlock's unless you are the one who assigned it.");
                return true;
            }
            // if we made it here, we can remove the lock.
            _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
            _config.selectedGagPadlocksAssigner[layer-1] = "None";
        }
        GagSpeak.Log.Debug($"Determined income message as a [unlock] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }

    // handle the remove message
    private bool HandleRemoveMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // our layer is valid, but we also need to make sure that this layer has a gag on it
        if (_config.selectedGagTypes[layer-1] == "None") {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is no gag applied for gag layer {layer}, so no gag can be removed.");
            _clientChat.PrintError($"ERROR, There is no gag applied for gag layer {layer}, so no gag can be removed.");
            return true;
        }
        // make sure there is no lock on that gags layer
        if (_config.selectedGagPadlocks[layer-1] != GagPadlocks.None) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Cannot remove a gag while the lock is on for this layer.");
            _clientChat.PrintError($"ERROR, Cannot remove a gag while the lock is on for this layer.");
            return true;
        }
        // if we made it here, we can remove the gag
        _config.selectedGagTypes[layer-1] = "None";
        _config.selectedGagPadlocks[layer-1] = GagPadlocks.None;
        _config.selectedGagPadlocksPassword[layer-1] = string.Empty;
        _config.selectedGagPadlocksAssigner[layer-1] = "None";
        GagSpeak.Log.Debug($"Determined income message as a [remove] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }


    // handle the removeall message
    private bool HandleRemoveAllMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // make sure all of our gagpadlocks are none, if they are not, throw exception
        if (_config.selectedGagPadlocks.Any(padlock => padlock != GagPadlocks.None)) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Cannot remove all gags while locks are on any of them.");
            _clientChat.PrintError($"ERROR, Cannot remove all gags while locks are on any of them.");
            return true;
        }
        // if we made it here, we can remove them all
        for (int i = 0; i < _config.selectedGagPadlocks.Count; i++) {
            _config.selectedGagTypes[i] = "None";
            _config.selectedGagPadlocks[i] = GagPadlocks.None;
            _config.selectedGagPadlocksPassword[i] = string.Empty;
            _config.selectedGagPadlocksAssigner[i] = "None";
        }
        GagSpeak.Log.Debug($"Determined income message as a [removeall] type encoded message, hiding from chat!");
        return true; // sucessful parse
    }

    // handle the apply message
    private bool HandleApplyMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // see if our layer is a valid layer
        if (decodedMessage[1] == "first") { decodedMessage[1] = "1"; } else if (decodedMessage[1] == "second") { decodedMessage[1] = "2"; } else if (decodedMessage[1] == "third") { decodedMessage[1] = "3"; }
        if (!int.TryParse(decodedMessage[1], out int layer)) { 
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid layer value.");
            _clientChat.PrintError($"ERROR, Invalid layer value.");
            return true;
        }
        // see if our gagtype is in selectedGagTypes[layer-1]
        if (!_config.GagTypes.ContainsKey(decodedMessage[2])) {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug("ERROR, Invalid gag type.");
            _clientChat.PrintError($"ERROR, Invalid gag type.");
            return true;
        }
        // make sure gagType is set to none
        if (_config.selectedGagTypes[layer-1] != "None") {
            // hide original message & throw exception
            isHandled = true;
            GagSpeak.Log.Debug($"ERROR, There is already a gag applied for gag layer {layer}!");
            _clientChat.PrintError($"ERROR, There is already a gag applied for gag layer {layer}!");
            return true;
        }
        // if we made it here, we can apply the gag
        _config.selectedGagTypes[layer-1] = decodedMessage[2];
        GagSpeak.Log.Debug($"Determined income message as a [applier] type encoded message, hiding from chat!");
        string playerNameWorld = decodedMessage[4];
        string[] parts = playerNameWorld.Split(' ');
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        _clientChat.Print(new SeStringBuilder().AddYellow($"[GAGSPEAK] You've been gagged by {playerName} with a {_config.selectedGagTypes[layer-1]}!").BuiltString);
        return true; // sucessful parse
    }

    // handle the request mistress message
    private bool HandleRequestMistressMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"Received a relationship request from {playerName}. View their Profile in the whitelist Tab to accept or deny it.").BuiltString);
                GagSpeak.Log.Debug($"Received mistress request from {playerName}. Pending approval.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid requestMistress message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid requestMistress message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the request pet message, will be exact same as mistress one.
    private bool HandleRequestPetMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"Received a relationship request from {playerName}. View their Profile in the whitelist Tab to accept or deny it.").BuiltString);
                GagSpeak.Log.Debug($"Received pet request from {playerName}. Pending approval.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid requestPet message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid requestPet message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the request slave message
    private bool HandleRequestSlaveMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"Received a relationship request from {playerName}. View their Profile in the whitelist Tab to accept or deny it.").BuiltString);
                GagSpeak.Log.Debug($"Received slave request from {playerName}. Pending approval.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid requestSlave message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid requestSlave message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the relation removal message
    private bool HandleRelationRemovalMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"Relationship with player removed sucessfully.").BuiltString);
                GagSpeak.Log.Debug($"Received relationship removal request from {playerName}. Pending approval.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid requestRemoval message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid requestRemoval message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the live chat garbler lock message
    private bool HandleLiveChatGarblerLockMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
        // just set the lockedLiveChatGarbler to true and print message
        // if they are true, set to false, if false, set to true
        if(_config.LockDirectChatGarbler == false) {
            _config.DirectChatGarbler = true;
            _config.LockDirectChatGarbler = true;
            _clientChat.Print(new SeStringBuilder().AddRed($"Your Mistress has decided you no longer have permission to speak clearly....").BuiltString);
            GagSpeak.Log.Debug($"Your Mistress has enforced a on toggle on your livechatgarbler lock..");
        }
        else {
            _config.DirectChatGarbler = false;
            _config.LockDirectChatGarbler = false;
            _clientChat.Print(new SeStringBuilder().AddRed($"Your Mistress returns your permission to speak once more. How Generous...").BuiltString);
            GagSpeak.Log.Debug($"Your Mistress has enforced a off toggle on your livechatgarbler lock..");
        }
        return true;
    }

    // handle the information request
    private bool HandleInformationRequestMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"Received a information request from {playerName}. Responding with updated info.").BuiltString);
                GagSpeak.Log.Debug($"Received information request from {playerName}.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid requestInfo message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid requestInfo message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the accept mistress request
    private bool HandleAcceptMistressMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"{playerName} is now your Mistress, enjoy~.").BuiltString);
                GagSpeak.Log.Debug($"{playerName} is now your Mistress, enjoy~.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid acceptMistress message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid acceptMistress message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the accept pet request
    private bool HandleAcceptPetMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"{playerName} is now your Pet, enjoy~.").BuiltString);
                GagSpeak.Log.Debug($"{playerName} is now your Pet, enjoy~.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid acceptPet message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid acceptPet message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the accept slave request
    private bool HandleAcceptSlaveMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
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
                _clientChat.Print(new SeStringBuilder().AddYellow($"{playerName} is now your Slave, enjoy~.").BuiltString);
                GagSpeak.Log.Debug($"{playerName} is now your Slave, enjoy~.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid acceptSlave message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid acceptSlave message parse. {e}");
            return false;
        }
        return true;
    }

    // handle the provide information message
    private bool HandleProvideInfoMessage(ref List<string> decodedMessage, ref bool isHandled, IChatGui _clientChat, GagSpeakConfig _config) {
    /* decodedMessageFormat:
        [0] = commandtype  //  [1] = isDomMode // [2] = directChatGarblerLock // [3] = garbleLevel
        [4] = player // [5] = relationship // [6] = selectedGagType1 // [7] = selectedGagType2 //
        [8] = selectedGagType3 // [9] = selectedGagPadlock1 // [10] = selectedGagPadlock2 // 
        [11] = selectedGagPadlock3 // [12] = selectedGagPadlockAssigner1 // [13] = selectedGagPadlockAssigner2 //
        [14] = selectedGagPadlockAssigner3 // [15] = selectedGagPadlockTimer1 //  [16] = selectedGagPadlockTimer2 //
        [17] = selectedGagPadlockTimer3  */
        // find the user in the whitelist with the same name as the player identified in decodedmessage[4]
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
                playerInWhitelist.lockedLiveChatGarbler = decodedMessage[2] == "True" ? true : false;
                playerInWhitelist.garbleLevel = int.Parse(decodedMessage[3]);
                playerInWhitelist.relationshipStatus = decodedMessage[5];
                playerInWhitelist.selectedGagTypes[0] = decodedMessage[6];
                playerInWhitelist.selectedGagTypes[1] = decodedMessage[7];
                playerInWhitelist.selectedGagTypes[2] = decodedMessage[8];
                playerInWhitelist.selectedGagPadlocks[0] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[9]);
                playerInWhitelist.selectedGagPadlocks[1] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[10]);
                playerInWhitelist.selectedGagPadlocks[2] = (GagPadlocks)Enum.Parse(typeof(GagPadlocks), decodedMessage[11]);
                playerInWhitelist.selectedGagPadlocksAssigner[0] = decodedMessage[12];
                playerInWhitelist.selectedGagPadlocksAssigner[1] = decodedMessage[13];
                playerInWhitelist.selectedGagPadlocksAssigner[2] = decodedMessage[14];
                playerInWhitelist.selectedGagPadlocksTimer[0] = TimeSpan.Parse(decodedMessage[15]);
                playerInWhitelist.selectedGagPadlocksTimer[1] = TimeSpan.Parse(decodedMessage[16]);
                playerInWhitelist.selectedGagPadlocksTimer[2] = TimeSpan.Parse(decodedMessage[17]);
                
                _clientChat.Print(new SeStringBuilder().AddYellow($"Recieved Information from {playerName}. Updated their profile in the whitelist tab.").BuiltString);
                GagSpeak.Log.Debug($"Received information response from {playerName}.");
            }
        } catch (Exception e) {
            GagSpeak.Log.Debug($"ERROR, Invalid provideInfo message parse. {e}");
            _clientChat.PrintError($"ERROR, Invalid provideInfo message parse. {e}");
            return false;
        }        

        return true;
    }

}
