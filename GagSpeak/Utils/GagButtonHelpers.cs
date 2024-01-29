using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterGui.Classes;
using GagSpeak.Data;
using GagSpeak.Chat;
using GagSpeak.UI.Helpers;
using GagSpeak.Chat.MsgEncoder;
using Dalamud.Plugin.Services;
using System.Text;

namespace GagSpeak.Utility.GagButtonHelpers;
 
/// <summary>
/// A class for all of the helpers regarding the buttons interacting with in the whitelist tab.
/// <para> GENERAL NOTE: All chat messages sent here are what the PLAYER will see. Chat Messages recieving user sees are defined in MsgResultLogic! </para>
/// </summary>
public static class GagButtonHelpers {

	/// <summary>  Controls logic for what to do once the Apply Gag button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void ApplyGagOnPlayer(int layer, string gagType, int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat so the player has a log of what they did
        if(selectedPlayer.selectedGagTypes[layer] != "None") {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot apply a gag to layer {layer},"+
                "a gag is already on this layer!").AddItalicsOff().BuiltString);
        } else {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Applying a "+ gagType+ $"to {selectedPlayer.name}").AddItalicsOff().BuiltString);
            // update information and send message
            string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
            chatManager.SendRealMessage(gagMessages.GagEncodedApplyMessage(playerPayload, targetPlayer, gagType, (layer+1).ToString()));
            selectedPlayer.selectedGagTypes[layer] = gagType; // note that this wont always be accurate, and is why request info exists.
        }
    }

    /// <summary>  Controls logic for what to do once the Lock Gag button is pressed in the whitelist tab.
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void LockGagOnPlayer(int layer, string lockLabel, int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui) {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        if (selectedPlayer.selectedGagPadlocks[layer] == LockableType.None && selectedPlayer.selectedGagTypes[layer] != "None") {
            Enum.TryParse(lockLabel, true, out LockableType padlockType);

            GagSpeak.Log.Debug($"Padlock Type: {padlockType}, assigning to whitelisted user {selectedPlayer.name}");
            config.whitelistPadlockIdentifier.SetType(padlockType);
            GagSpeak.Log.Debug($"Validating password for {selectedPlayer.name}");
            config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer.name, playerPayload.PlayerName);
            // if we make it here, we have a valid password, so we can send the message            
            string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
            // then we can apply the lock gag logic after we have verified it is an acceptable password
            if(config.whitelistPadlockIdentifier._padlockType == LockableType.MetalPadlock ||
            config.whitelistPadlockIdentifier._padlockType == LockableType.FiveMinutesPadlock ||
            config.whitelistPadlockIdentifier._padlockType == LockableType.MistressPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString()));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer.name}'s "+
                    selectedPlayer.selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer.selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;

                if(config.whitelistPadlockIdentifier._padlockType == LockableType.FiveMinutesPadlock) {
                    selectedPlayer.selectedGagPadlocksTimer[layer] = UIHelpers.GetEndTime("5m");
                
                } else if(config.whitelistPadlockIdentifier._padlockType == LockableType.MistressPadlock) {
                    selectedPlayer.selectedGagPadlocksAssigner[layer] = playerPayload.PlayerName;
                }
            }
            // for mistress timer padlock
            else if (config.whitelistPadlockIdentifier._padlockType == LockableType.MistressTimerPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputTimer));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer.name}'s "+
                    selectedPlayer.selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer.selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer.selectedGagPadlocksAssigner[layer] = playerPayload.PlayerName;
                selectedPlayer.selectedGagPadlocksTimer[layer] = UIHelpers.GetEndTime(config.whitelistPadlockIdentifier._inputTimer);
            }
            // for combination padlock
            else if (config.whitelistPadlockIdentifier._padlockType == LockableType.CombinationPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputCombination));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer.name}'s "+
                    selectedPlayer.selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer.selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer.selectedGagPadlocksPassword[layer] = config.whitelistPadlockIdentifier._inputCombination;
            }
            // for password padlock
            else if (config.whitelistPadlockIdentifier._padlockType == LockableType.PasswordPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputPassword));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer.name}'s "+
                    selectedPlayer.selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                // update whitelist information
                selectedPlayer.selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer.selectedGagPadlocksPassword[layer] = config.whitelistPadlockIdentifier._inputPassword;
            }
            // for timer password padlocks
            else if (config.whitelistPadlockIdentifier._padlockType == LockableType.TimerPasswordPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputPassword, config.whitelistPadlockIdentifier._inputTimer));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer.name}'s "+
                    selectedPlayer.selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer.selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer.selectedGagPadlocksPassword[layer] = config.whitelistPadlockIdentifier._inputPassword;
                selectedPlayer.selectedGagPadlocksTimer[layer] = UIHelpers.GetEndTime(config.whitelistPadlockIdentifier._inputTimer);
            }
        } else {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot lock {selectedPlayer.name}'s gag, it is already locked!").AddItalicsOff().BuiltString);
        }
    }

	/// <summary>  Controls logic for what to do once the Unlock Gag button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void UnlockGagOnPlayer(int layer, string lockLabel, int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // check which gag it is
        GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");

        if(config.whitelistPadlockIdentifier._padlockType == LockableType.MetalPadlock ||
        config.whitelistPadlockIdentifier._padlockType == LockableType.FiveMinutesPadlock ||
        config.whitelistPadlockIdentifier._padlockType == LockableType.MistressPadlock ||
        config.whitelistPadlockIdentifier._padlockType == LockableType.MistressTimerPadlock) {
            if(config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer.name, playerPayload.PlayerName)) {
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType} validated for unlock");
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {selectedPlayer.name}'s"+
                    selectedPlayer.selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString()));
                selectedPlayer.selectedGagPadlocks[layer] = LockableType.None;
                // set if appropriate
                if(config.whitelistPadlockIdentifier._padlockType == LockableType.FiveMinutesPadlock) {
                    selectedPlayer.selectedGagPadlocksTimer[layer] = DateTimeOffset.Now;
                }
                if(config.whitelistPadlockIdentifier._padlockType == LockableType.MistressPadlock
                || config.whitelistPadlockIdentifier._padlockType == LockableType.MistressTimerPadlock) {
                    selectedPlayer.selectedGagPadlocksTimer[layer] = DateTimeOffset.Now;
                    selectedPlayer.selectedGagPadlocksAssigner[layer] = "";
                }
            }
        }
        else if (config.whitelistPadlockIdentifier._padlockType == LockableType.CombinationPadlock) {
            if(config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer.name, playerPayload.PlayerName)) {
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType} validated for unlock");
                if(config.whitelistPadlockIdentifier._inputCombination == selectedPlayer.selectedGagPadlocksPassword[layer]) {
                    GagSpeak.Log.Debug($"Padlock Type: Input combination matches players lock combination, sending unlock message");
                    // update information after sending message and verifying, and setting cooldown timer
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {selectedPlayer.name}'s"+
                        selectedPlayer.selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                    chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(),
                    config.whitelistPadlockIdentifier._inputCombination));
                    selectedPlayer.selectedGagPadlocks[layer] = LockableType.None;
                    selectedPlayer.selectedGagPadlocksPassword[layer] = "";

                } else {
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Incorrect combination for {selectedPlayer.name}'s"+
                        selectedPlayer.selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                }
            }
        }
        else if (config.whitelistPadlockIdentifier._padlockType == LockableType.PasswordPadlock || 
        config.whitelistPadlockIdentifier._padlockType == LockableType.TimerPasswordPadlock) {
            if(config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer.name, playerPayload.PlayerName)) {
                if(config.whitelistPadlockIdentifier._inputPassword == selectedPlayer.selectedGagPadlocksPassword[layer]) {      
                    GagSpeak.Log.Debug($"Padlock Type: Input password matches players lock password, sending unlock message");       
                    // update information after sending message and verifying, and setting cooldown timer
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {selectedPlayer.name}'s"+
                        selectedPlayer.selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                    chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(),
                    config.whitelistPadlockIdentifier._inputPassword));
                    selectedPlayer.selectedGagPadlocks[layer] = LockableType.None;
                    selectedPlayer.selectedGagPadlocksPassword[layer] = "";
                    // set if appropriate
                    if(config.whitelistPadlockIdentifier._padlockType == LockableType.TimerPasswordPadlock) {
                        selectedPlayer.selectedGagPadlocksTimer[layer] = DateTimeOffset.Now;
                    }

                } else {
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Incorrect password for {selectedPlayer.name}'s"+
                        selectedPlayer.selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                }
            }
        }
    }
	/// <summary>  Controls logic for what to do once the Remove Gag button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RemoveGagFromPlayer(int layer, string gagType, int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // check if the current selected player's gag layer has a lock that isnt none. If it doesnt, unlock the gag, otherwise, let the player know they couldnt remove it
        if (selectedPlayer.selectedGagPadlocks[layer] != LockableType.None) {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot remove {selectedPlayer.name}'s "+gagType+" gag, it is locked!").AddItalicsOff().BuiltString);
            return;
        } else {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing {selectedPlayer.name}'s "+gagType+" gag").AddItalicsOff().BuiltString);
            // update information and send message
            string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
            chatManager.SendRealMessage(gagMessages.GagEncodedRemoveMessage(playerPayload, targetPlayer, (layer+1).ToString()));
            selectedPlayer.selectedGagTypes[layer] = "None";
            selectedPlayer.selectedGagPadlocks[layer] = LockableType.None;
            selectedPlayer.selectedGagPadlocksPassword[layer] = "";
            selectedPlayer.selectedGagPadlocksAssigner[layer] = "";
        }
    }

	/// <summary>  Controls logic for what to do once the Remove All Gags button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RemoveAllGagsFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer, GagSpeakConfig config,
    ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // if any gags have locks on them, then done extcute this logic
        for (int i = 0; i < selectedPlayer.selectedGagTypes.Count; i++) {
            if (selectedPlayer.selectedGagPadlocks[i] != LockableType.None) {
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot remove {selectedPlayer.name}'s gags, "+
                    "one or more of them are locked!").AddItalicsOff().BuiltString);
                return;
            }
        } // if we make it here, we are able to remove them, so remove them!
        // print to chat so the player has a log of what they did
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing all gags from {selectedPlayer.name}").AddItalicsOff().BuiltString);
        // update information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRemoveAllMessage(playerPayload, targetPlayer));
        for (int i = 0; i < selectedPlayer.selectedGagTypes.Count; i++) {
            selectedPlayer.selectedGagTypes[i] = "None";
            selectedPlayer.selectedGagPadlocks[i] = LockableType.None;
            selectedPlayer.selectedGagPadlocksPassword[i] = "";
            selectedPlayer.selectedGagPadlocksAssigner[i] = "";
        }
    }

	/// <summary>  Controls logic for what to do once the the request mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestMistressToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
            $"{selectedPlayer.name}, to see if they would like you to become their Mistress.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer.PendingRelationRequestFromYou = "Mistress";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.RequestMistressEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the request pet relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestPetToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
            $"{selectedPlayer.name}, to see if they would like you to become their Pet.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer.PendingRelationRequestFromYou = "Pet";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.RequestPetEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the request slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestSlaveToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
            $"{selectedPlayer.name}, to see if they would like you to become their Slave.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer.PendingRelationRequestFromYou = "Slave";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.RequestSlaveEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the accept mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void AcceptMistressRequestFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted {selectedPlayer.name} as your Mistress. "+
            "Updating their whitelist information").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // set the relationship status the player has towards you "They are your Mistress" here, because once you hit accept, both sides agree
        selectedPlayer.relationshipStatusToYou = selectedPlayer.PendingRelationRequestFromPlayer;
        if(selectedPlayer.relationshipStatus != "None") {
            selectedPlayer.SetTimeOfCommitment(); // set the commitment time if relationship is now two-way!
        }
        chatManager.SendRealMessage(gagMessages.AcceptMistressEncodedMessage(playerPayload, targetPlayer));
    }


	/// <summary>  Controls logic for what to do once the the accept pet relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void AcceptPetRequestFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted {selectedPlayer.name} as your pet. "+
            "Updating their whitelist information").AddItalicsOff().BuiltString);
        // update whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // set the relationship status the player has towards you "They are your Pet" here, because once you hit accept, both sides agree
        selectedPlayer.relationshipStatusToYou = selectedPlayer.PendingRelationRequestFromPlayer; // set the relationship status
        if(selectedPlayer.relationshipStatus != "None") {
            selectedPlayer.SetTimeOfCommitment(); // set the commitment time if relationship is now two-way!
        }
        chatManager.SendRealMessage(gagMessages.AcceptPetEncodedMessage(playerPayload, targetPlayer));
    }


	/// <summary>  Controls logic for what to do once the the accept slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void AcceptSlaveRequestFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted {selectedPlayer.name} as your slave. "+
            "Updating their whitelist information").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // set the relationship status the player has towards you "They are your Slave" here, because once you hit accept, both sides agree
        selectedPlayer.relationshipStatusToYou = selectedPlayer.PendingRelationRequestFromPlayer; // set the relationship status
        if(selectedPlayer.relationshipStatus != "None") {
            selectedPlayer.SetTimeOfCommitment(); // set the commitment time if relationship is now two-way!
        }
        chatManager.SendRealMessage(gagMessages.AcceptSlaveEncodedMessage(playerPayload, targetPlayer));
    }


	/// <summary>  Controls logic for what to do once the the decline mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void DeclineMistressRequestFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Declining {selectedPlayer.name}'s request to become your Mistress.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
        selectedPlayer.relationshipStatusToYou = "None";
        selectedPlayer.PendingRelationRequestFromPlayer = "";
        chatManager.SendRealMessage(gagMessages.DeclineMistressEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the decline pet relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void DeclinePetRequestFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Declining {selectedPlayer.name}'s request to become your Pet.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
        selectedPlayer.relationshipStatusToYou = "None";
        selectedPlayer.PendingRelationRequestFromPlayer = "";
        chatManager.SendRealMessage(gagMessages.DeclinePetEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the decline slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void DeclineSlaveRequestFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Declining {selectedPlayer.name}'s request to become your Slave.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
        selectedPlayer.relationshipStatusToYou = "None";
        selectedPlayer.PendingRelationRequestFromPlayer = "";
        chatManager.SendRealMessage(gagMessages.DeclineSlaveEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the remove relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestRelationRemovalToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing Relation Status "+
            $"with {selectedPlayer.name}.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer.relationshipStatus = "None";
        selectedPlayer.relationshipStatusToYou = "None";
        selectedPlayer.PendingRelationRequestFromYou = "";
        selectedPlayer.PendingRelationRequestFromPlayer = "";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.RequestRemovalEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the toggle lock live chat garbler button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void OrderLiveGarbleLockToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddRed($"[GagSpeak]").AddText($"Forcing silence upon your slave, " +
            $"hopefully {selectedPlayer.name} will behave herself~").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.OrderGarblerLockEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the request player info button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestInfoFromPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending information request to " +
            $"{selectedPlayer.name}, please wait...").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.RequestInfoEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for sending the first half of your info the player that requested it from you. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void SendInfoToPlayer(GagSpeakConfig config, ChatManager chatManager,MessageEncoder gagMessages,
    IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        // format a secondary string from the configs.sendinfoname's "firstname lastname@homeworld" to "firstname lastname"
        try {
            string targetPlayer = config.sendInfoName;
            string playername = config.sendInfoName.Substring(0, config.sendInfoName.IndexOf('@'));
            // Also, get your relationship to that player, if any. Search for their name in the whitelist.
            string relationshipVar = "None";
            config.Whitelist.ForEach(delegate(WhitelistCharData entry) {
                if (config.sendInfoName.Contains(entry.name)) {
                    relationshipVar = entry.relationshipStatus; 
                }
            });
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{targetPlayer}] "+
                "with your details(1/2)").AddItalicsOff().BuiltString);
            //send the message
            chatManager.SendRealMessage(gagMessages.ProvideInfoEncodedMessage(playerPayload, targetPlayer, config.InDomMode,
                config.DirectChatGarbler, config.GarbleLevel, config.selectedGagTypes, config.selectedGagPadlocks,
                config.selectedGagPadlocksAssigner, config.selectedGagPadLockTimer, relationshipVar));
        }
        catch (Exception e) {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Error: {e}").AddItalicsOff().BuiltString);
        }
    }

	/// <summary>  Controls logic for sending the second half of your info the player that requested it from you. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void SendInfoToPlayer2(GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages,
    IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        // format a secondary string from the configs.sendinfoname's "firstname lastname@homeworld" to "firstname lastname"
        try {
            string targetPlayer = config.sendInfoName;
            string playername = config.sendInfoName.Substring(0, config.sendInfoName.IndexOf('@'));
            // Also, get your relationship to that player, if any. Search for their name in the whitelist.
            string relationshipVar = "None";
            config.Whitelist.ForEach(delegate(WhitelistCharData entry) {
                if (config.sendInfoName.Contains(entry.name)) {
                    relationshipVar = entry.relationshipStatus; 
                }
            });
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{targetPlayer}] "+
                "with your details(2/2)").AddItalicsOff().BuiltString);
            // send the message
            chatManager.SendRealMessage(gagMessages.ProvideInfoEncodedMessage2(playerPayload, targetPlayer, config.InDomMode,
                config.DirectChatGarbler, config.GarbleLevel, config.selectedGagTypes, config.selectedGagPadlocks,
                config.selectedGagPadlocksAssigner, config.selectedGagPadLockTimer, relationshipVar));
        }
        catch (Exception e) {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Error: {e}").AddItalicsOff().BuiltString);
        }
    }   


	/// <summary>  Controls logic for what to do once the the accept slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void LockRestraintSetToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer, string restraintSetNameToApply,
    string lockTimer, GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload); // THIS IS THE SENDER
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Now attempting to lock {selectedPlayer.name}'s {restraintSetNameToApply} "+
            $"restraint set for {lockTimer}. Keep in mind if they have this option disabled, it will not apply.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRestraintSetLockMessage(playerPayload, restraintSetNameToApply, lockTimer, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the accept slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void UnlockRestraintSetToPlayer(int currentWhitelistItem, WhitelistCharData selectedPlayer, string restraintSetNameToApply,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload); // THIS IS THE SENDER
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.Whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Attempting to unlock {selectedPlayer.name}'s [{restraintSetNameToApply}] "+
            $"restraint set. Keep in mind if you are not the one who assigned it and they did not do it to themselves, it will not unlock.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRestraintSetUnlockMessage(playerPayload, restraintSetNameToApply, targetPlayer));
    }
}