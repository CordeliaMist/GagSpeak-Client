using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterGui.Classes;
using GagSpeak.Data;
using GagSpeak.Chat;
using GagSpeak.UI.Helpers;
using GagSpeak.ChatMessages;
using Dalamud.Plugin.Services;
using System.Text;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.CharacterData;

// SPLIT THIS INTO MANY SUB CLASSES, ONE FOR EACH SECTION / CATAGORY

namespace GagSpeak.Utility.GagButtonHelpers;
 
/// <summary>
/// A class for all of the helpers regarding the buttons interacting with in the whitelist tab.
/// <para> GENERAL NOTE: All chat messages sent here are what the PLAYER will see. Chat Messages recieving user sees are defined in MsgResultLogic! </para>
/// </summary>
public static class GagButtonHelpers {

	/// <summary>  Controls logic for what to do once the Apply Gag button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void ApplyGagOnPlayer(int layer, string gagType, int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat so the player has a log of what they did
        if(selectedPlayer._selectedGagTypes[layer] != "None") {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot apply a gag to layer {layer},"+
                "a gag is already on this layer!").AddItalicsOff().BuiltString);
        } else {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Applying a "+ gagType+ $"to {selectedPlayer._name}").AddItalicsOff().BuiltString);
            // update information and send message
            string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
            chatManager.SendRealMessage(gagMessages.GagEncodedApplyMessage(playerPayload, targetPlayer, gagType, (layer+1).ToString()));
            selectedPlayer._selectedGagTypes[layer] = gagType; // note that this wont always be accurate, and is why request info exists.
        }
    }

    /// <summary>  Controls logic for what to do once the Lock Gag button is pressed in the whitelist tab.
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void LockGagOnPlayer(int layer, string lockLabel, int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui) {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        if (selectedPlayer._selectedGagPadlocks[layer] == Padlocks.None && selectedPlayer._selectedGagTypes[layer] != "None") {
            Enum.TryParse(lockLabel, true, out Padlocks padlockType);

            GagSpeak.Log.Debug($"Padlock Type: {padlockType}, assigning to whitelisted user {selectedPlayer._name}");
            config.whitelistPadlockIdentifier.SetType(padlockType);
            GagSpeak.Log.Debug($"Validating password for {selectedPlayer._name}");
            config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer._name, playerPayload.PlayerName);
            // if we make it here, we have a valid password, so we can send the message            
            string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
            // then we can apply the lock gag logic after we have verified it is an acceptable password
            if(config.whitelistPadlockIdentifier._padlockType == Padlocks.MetalPadlock ||
            config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock ||
            config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString()));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer._name}'s "+
                    selectedPlayer._selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer._selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;

                if(config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock) {
                    selectedPlayer._selectedGagPadlockTimer[layer] = UIHelpers.GetEndTime("5m");
                
                } else if(config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock) {
                    selectedPlayer._selectedGagPadlockAssigner[layer] = playerPayload.PlayerName;
                }
            }
            // for mistress timer padlock
            else if (config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressTimerPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputTimer));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer._name}'s "+
                    selectedPlayer._selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer._selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer._selectedGagPadlockAssigner[layer] = playerPayload.PlayerName;
                selectedPlayer._selectedGagPadlockTimer[layer] = UIHelpers.GetEndTime(config.whitelistPadlockIdentifier._inputTimer);
            }
            // for combination padlock
            else if (config.whitelistPadlockIdentifier._padlockType == Padlocks.CombinationPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputCombination));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer._name}'s "+
                    selectedPlayer._selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer._selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer._selectedGagPadlockPassword[layer] = config.whitelistPadlockIdentifier._inputCombination;
            }
            // for password padlock
            else if (config.whitelistPadlockIdentifier._padlockType == Padlocks.PasswordPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputPassword));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer._name}'s "+
                    selectedPlayer._selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                // update whitelist information
                selectedPlayer._selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer._selectedGagPadlockPassword[layer] = config.whitelistPadlockIdentifier._inputPassword;
            }
            // for timer password padlocks
            else if (config.whitelistPadlockIdentifier._padlockType == Padlocks.TimerPasswordPadlock) {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), config.whitelistPadlockIdentifier._inputPassword, config.whitelistPadlockIdentifier._inputTimer));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {selectedPlayer._name}'s "+
                    selectedPlayer._selectedGagTypes[layer]+" with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");
                selectedPlayer._selectedGagPadlocks[layer] = config.whitelistPadlockIdentifier._padlockType;
                selectedPlayer._selectedGagPadlockPassword[layer] = config.whitelistPadlockIdentifier._inputPassword;
                selectedPlayer._selectedGagPadlockTimer[layer] = UIHelpers.GetEndTime(config.whitelistPadlockIdentifier._inputTimer);
            }
        } else {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot lock {selectedPlayer._name}'s gag, it is already locked!").AddItalicsOff().BuiltString);
        }
    }

	/// <summary>  Controls logic for what to do once the Unlock Gag button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void UnlockGagOnPlayer(int layer, string lockLabel, int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // check which gag it is
        GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType}");

        if(config.whitelistPadlockIdentifier._padlockType == Padlocks.MetalPadlock ||
        config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock ||
        config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock ||
        config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressTimerPadlock) {
            if(config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer._name, playerPayload.PlayerName)) {
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType} validated for unlock");
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {selectedPlayer._name}'s"+
                    selectedPlayer._selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString()));
                selectedPlayer._selectedGagPadlocks[layer] = Padlocks.None;
                // set if appropriate
                if(config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock) {
                    selectedPlayer._selectedGagPadlockTimer[layer] = DateTimeOffset.Now;
                }
                if(config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock
                || config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressTimerPadlock) {
                    selectedPlayer._selectedGagPadlockTimer[layer] = DateTimeOffset.Now;
                    selectedPlayer._selectedGagPadlockAssigner[layer] = "";
                }
            }
        }
        else if (config.whitelistPadlockIdentifier._padlockType == Padlocks.CombinationPadlock) {
            if(config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer._name, playerPayload.PlayerName)) {
                GagSpeak.Log.Debug($"Padlock Type: {config.whitelistPadlockIdentifier._padlockType} validated for unlock");
                if(config.whitelistPadlockIdentifier._inputCombination == selectedPlayer._selectedGagPadlockPassword[layer]) {
                    GagSpeak.Log.Debug($"Padlock Type: Input combination matches players lock combination, sending unlock message");
                    // update information after sending message and verifying, and setting cooldown timer
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {selectedPlayer._name}'s"+
                        selectedPlayer._selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                    chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(),
                    config.whitelistPadlockIdentifier._inputCombination));
                    selectedPlayer._selectedGagPadlocks[layer] = Padlocks.None;
                    selectedPlayer._selectedGagPadlockPassword[layer] = "";

                } else {
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Incorrect combination for {selectedPlayer._name}'s"+
                        selectedPlayer._selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                }
            }
        }
        else if (config.whitelistPadlockIdentifier._padlockType == Padlocks.PasswordPadlock || 
        config.whitelistPadlockIdentifier._padlockType == Padlocks.TimerPasswordPadlock) {
            if(config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, config, playerPayload.PlayerName, selectedPlayer._name, playerPayload.PlayerName)) {
                if(config.whitelistPadlockIdentifier._inputPassword == selectedPlayer._selectedGagPadlockPassword[layer]) {      
                    GagSpeak.Log.Debug($"Padlock Type: Input password matches players lock password, sending unlock message");       
                    // update information after sending message and verifying, and setting cooldown timer
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {selectedPlayer._name}'s"+
                        selectedPlayer._selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                    chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(),
                    config.whitelistPadlockIdentifier._inputPassword));
                    selectedPlayer._selectedGagPadlocks[layer] = Padlocks.None;
                    selectedPlayer._selectedGagPadlockPassword[layer] = "";
                    // set if appropriate
                    if(config.whitelistPadlockIdentifier._padlockType == Padlocks.TimerPasswordPadlock) {
                        selectedPlayer._selectedGagPadlockTimer[layer] = DateTimeOffset.Now;
                    }

                } else {
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Incorrect password for {selectedPlayer._name}'s"+
                        selectedPlayer._selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                }
            }
        }
    }
	/// <summary>  Controls logic for what to do once the Remove Gag button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RemoveGagFromPlayer(int layer, string gagType, int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // check if the current selected player's gag layer has a lock that isnt none. If it doesnt, unlock the gag, otherwise, let the player know they couldnt remove it
        if (selectedPlayer._selectedGagPadlocks[layer] != Padlocks.None) {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot remove {selectedPlayer._name}'s "+gagType+" gag, it is locked!").AddItalicsOff().BuiltString);
            return;
        } else {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing {selectedPlayer._name}'s "+gagType+" gag").AddItalicsOff().BuiltString);
            // update information and send message
            string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
            chatManager.SendRealMessage(gagMessages.GagEncodedRemoveMessage(playerPayload, targetPlayer, (layer+1).ToString()));
            selectedPlayer._selectedGagTypes[layer] = "None";
            selectedPlayer._selectedGagPadlocks[layer] = Padlocks.None;
            selectedPlayer._selectedGagPadlockPassword[layer] = "";
            selectedPlayer._selectedGagPadlockAssigner[layer] = "";
        }
    }

	/// <summary>  Controls logic for what to do once the Remove All Gags button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RemoveAllGagsFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer, GagSpeakConfig config,
    ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // if any gags have locks on them, then done extcute this logic
        for (int i = 0; i < selectedPlayer._selectedGagTypes.Count; i++) {
            if (selectedPlayer._selectedGagPadlocks[i] != Padlocks.None) {
                chatGui.Print(
                    new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot remove {selectedPlayer._name}'s gags, "+
                    "one or more of them are locked!").AddItalicsOff().BuiltString);
                return;
            }
        } // if we make it here, we are able to remove them, so remove them!
        // print to chat so the player has a log of what they did
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing all gags from {selectedPlayer._name}").AddItalicsOff().BuiltString);
        // update information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRemoveAllMessage(playerPayload, targetPlayer));
        for (int i = 0; i < selectedPlayer._selectedGagTypes.Count; i++) {
            selectedPlayer._selectedGagTypes[i] = "None";
            selectedPlayer._selectedGagPadlocks[i] = Padlocks.None;
            selectedPlayer._selectedGagPadlockPassword[i] = "";
            selectedPlayer._selectedGagPadlockAssigner[i] = "";
        }
    }

	/// <summary>  Controls logic for what to do once the the request mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestMistressToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
            $"{selectedPlayer._name}, to see if they would like you to become their Mistress.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer._pendingRelationRequestFromYou = "Mistress";
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.RequestMistressEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the request pet relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestPetToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
            $"{selectedPlayer._name}, to see if they would like you to become their Pet.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer._pendingRelationRequestFromYou = "Pet";
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.RequestPetEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the request slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestSlaveToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
            $"{selectedPlayer._name}, to see if they would like you to become their Slave.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer._pendingRelationRequestFromYou = "Slave";
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.RequestSlaveEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the accept mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void AcceptMistressRequestFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted {selectedPlayer._name} as your Mistress. "+
            "Updating their whitelist information").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // set the relationship status the player has towards you "They are your Mistress" here, because once you hit accept, both sides agree
        selectedPlayer._theirStatusToYou = selectedPlayer._pendingRelationRequestFromPlayer;
        if(selectedPlayer._yourStatusToThem != "None") {
            selectedPlayer.Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
        }
        chatManager.SendRealMessage(gagMessages.AcceptMistressEncodedMessage(playerPayload, targetPlayer));
    }


	/// <summary>  Controls logic for what to do once the the accept pet relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void AcceptPetRequestFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted {selectedPlayer._name} as your pet. "+
            "Updating their whitelist information").AddItalicsOff().BuiltString);
        // update whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // set the relationship status the player has towards you "They are your Pet" here, because once you hit accept, both sides agree
        selectedPlayer._theirStatusToYou = selectedPlayer._pendingRelationRequestFromPlayer; // set the relationship status
        if(selectedPlayer._yourStatusToThem != "None") {
            selectedPlayer.Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
        }
        chatManager.SendRealMessage(gagMessages.AcceptPetEncodedMessage(playerPayload, targetPlayer));
    }


	/// <summary>  Controls logic for what to do once the the accept slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void AcceptSlaveRequestFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted {selectedPlayer._name} as your slave. "+
            "Updating their whitelist information").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // set the relationship status the player has towards you "They are your Slave" here, because once you hit accept, both sides agree
        selectedPlayer._theirStatusToYou = selectedPlayer._pendingRelationRequestFromPlayer; // set the relationship status
        if(selectedPlayer._yourStatusToThem != "None") {
            selectedPlayer.Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
        }
        chatManager.SendRealMessage(gagMessages.AcceptSlaveEncodedMessage(playerPayload, targetPlayer));
    }


	/// <summary>  Controls logic for what to do once the the decline mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void DeclineMistressRequestFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Declining {selectedPlayer._name}'s request to become your Mistress.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
        selectedPlayer._theirStatusToYou = "None";
        selectedPlayer._pendingRelationRequestFromPlayer = "";
        chatManager.SendRealMessage(gagMessages.DeclineMistressEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the decline pet relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void DeclinePetRequestFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Declining {selectedPlayer._name}'s request to become your Pet.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
        selectedPlayer._theirStatusToYou = "None";
        selectedPlayer._pendingRelationRequestFromPlayer = "";
        chatManager.SendRealMessage(gagMessages.DeclinePetEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the decline slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void DeclineSlaveRequestFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Declining {selectedPlayer._name}'s request to become your Slave.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
        selectedPlayer._theirStatusToYou = "None";
        selectedPlayer._pendingRelationRequestFromPlayer = "";
        chatManager.SendRealMessage(gagMessages.DeclineSlaveEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the remove relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestRelationRemovalToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing Relation Status "+
            $"with {selectedPlayer._name}.").AddItalicsOff().BuiltString);
        //update information and send message
        selectedPlayer._yourStatusToThem = "None";
        selectedPlayer._theirStatusToYou = "None";
        selectedPlayer._pendingRelationRequestFromYou = "";
        selectedPlayer._pendingRelationRequestFromPlayer = "";
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.RequestRemovalEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the toggle lock live chat garbler button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void OrderLiveGarbleLockToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddRed($"[GagSpeak]").AddText($"Forcing silence upon your slave, " +
            $"hopefully {selectedPlayer._name} will behave herself~").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.OrderGarblerLockEncodedMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the request player info button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void RequestInfoFromPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending information request to " +
            $"{selectedPlayer._name}, please wait...").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
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
            config.whitelist.ForEach(delegate(WhitelistedCharacterInfo entry) {
                if (config.sendInfoName.Contains(entry._name)) {
                    relationshipVar = entry._yourStatusToThem; 
                }
            });
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{targetPlayer}] "+
                "with your details(1/2)").AddItalicsOff().BuiltString);
            //send the message
            chatManager.SendRealMessage(gagMessages.ProvideInfoEncodedMessage(playerPayload, targetPlayer, config.InDomMode,
                config.DirectChatGarbler, config.GarbleLevel, config.selectedGagTypes, config._selectedGagPadlocks,
                config._selectedGagPadlockAssigner, config.selectedGagPadLockTimer, relationshipVar));
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
            config.whitelist.ForEach(delegate(WhitelistedCharacterInfo entry) {
                if (config.sendInfoName.Contains(entry._name)) {
                    relationshipVar = entry._yourStatusToThem; 
                }
            });
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{targetPlayer}] "+
                "with your details(2/2)").AddItalicsOff().BuiltString);
            // send the message
            chatManager.SendRealMessage(gagMessages.ProvideInfoEncodedMessage2(playerPayload, targetPlayer, config.InDomMode,
                config.DirectChatGarbler, config.GarbleLevel, config.selectedGagTypes, config._selectedGagPadlocks,
                config._selectedGagPadlockAssigner, config.selectedGagPadLockTimer, relationshipVar));
        }
        catch (Exception e) {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Error: {e}").AddItalicsOff().BuiltString);
        }
    }   


	/// <summary>  Controls logic for what to do once the the accept slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void LockRestraintSetToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer, string restraintSetNameToApply,
    string lockTimer, GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload); // THIS IS THE SENDER
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Now attempting to lock {selectedPlayer._name}'s {restraintSetNameToApply} "+
            $"restraint set for {lockTimer}. Keep in mind if they have this option disabled, it will not apply.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRestraintSetLockMessage(playerPayload, restraintSetNameToApply, lockTimer, targetPlayer));
    }

	/// <summary>  Controls logic for what to do once the the accept slave relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void UnlockRestraintSetToPlayer(int currentWhitelistItem, WhitelistedCharacterInfo selectedPlayer, string restraintSetNameToApply,
    GagSpeakConfig config, ChatManager chatManager, MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload); // THIS IS THE SENDER
        if (currentWhitelistItem < 0 || currentWhitelistItem >= config.whitelist.Count) { return; }
        // print to chat that you sent the request
        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Attempting to unlock {selectedPlayer._name}'s [{restraintSetNameToApply}] "+
            $"restraint set. Keep in mind if you are not the one who assigned it and they did not do it to themselves, it will not unlock.").AddItalicsOff().BuiltString);
        // updating whitelist with new information and send message
        string targetPlayer = selectedPlayer._name + "@" + selectedPlayer._homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRestraintSetUnlockMessage(playerPayload, restraintSetNameToApply, targetPlayer));
    }
}