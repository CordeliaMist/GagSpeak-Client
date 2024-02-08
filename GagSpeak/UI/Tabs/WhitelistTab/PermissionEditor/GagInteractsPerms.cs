using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Utility;
using OtterGui;
using GagSpeak.CharacterData;
using System.Linq;
using Dalamud.Interface.Utility;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using GagSpeak.UI.Equipment;
using GagSpeak.Gagsandlocks;
using Dalamud.Plugin.Services;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPlayerPermissions {
    protected   string                  _gagLabel;
    protected   string                  _lockLabel;
    protected   int                     _layer;
    protected   GagTypeFilterCombo[]    _gagTypeFilterCombo;        // create an array of item combos
    protected   GagLockFilterCombo[]    _gagLockFilterCombo;        // create an array of item combos

#region GeneralPerms
    public void DrawGagInteractions(ref bool _viewMode) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        ImGui.PushFont(_fontService.UidFont);
        ImGui.Text($"Gag Interactions for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}");
        ImGui.PopFont();

        // create a new table for this section
        using (var InfoTable = ImRaii.Table("InfoTable", 2)) {
            if (!InfoTable) return;

            ImGui.TableSetupColumn("Gag Dropdown", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Gag Button", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Remove All Gagsmm").X);
            // for our first row, display the DD for layer, DD gag type, and apply gag to player buttons
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            
            // create a combo for the layer, with options 1, 2, 3. Store selection to variable layer
            int layer = _layer;
            ImGui.Text("Select Gag Layer:");
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.95f);
            ImGui.Combo("##Layer", ref layer, new string[] { "Layer 1", "Layer 2", "Layer 3" }, 3);
            _layer = layer;
            
            // create a dropdown for the gag type,
            ImGui.Text("Select Gag Type:");
            int width = (int)(ImGui.GetContentRegionAvail().X * 0.95f);
            _gagListingsDrawer.DrawGagTypeItemCombo((layer)+10, _characterHandler.activeListIdx, ref _gagLabel,
                                                    layer, false, width, _gagTypeFilterCombo[layer]);
            
            // set up a temp password storage field here.
            ImGui.Text("Select Gag Lock Type:");
            width = (int)(ImGui.GetContentRegionAvail().X * 0.95f);
            _gagListingsDrawer.DrawGagLockItemCombo((layer)+10, _characterHandler.activeListIdx, ref _lockLabel, layer, false, width, _gagLockFilterCombo[layer]);
            
            
            // display the password field, if any.
            ImGui.Text("Password (If Nessisary):");
            var tempwidth = ImGui.GetContentRegionAvail().X * 0.95f;
            ImGui.Columns(2,"Password Divider", false);
            ImGui.SetColumnWidth(0, tempwidth);
            Enum.TryParse(_lockLabel, out Padlocks parsedLockType);
            if(_config.whitelistPadlockIdentifier.DisplayPasswordField(parsedLockType)) {}

            // now draw the gag buttons
            ImGui.TableNextColumn();
            ImGui.Spacing();
            if (ImGui.Button("Apply Gag", new Vector2(ImGui.GetContentRegionAvail().X, 25 * ImGuiHelpers.GlobalScale))) {
                // execute the generation of the apply gag layer string
                ApplyGagOnPlayer(layer, _gagLabel, _characterHandler.activeListIdx,
                _characterHandler, _chatManager, _messageEncoder, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                _interactOrPermButtonEvent.Invoke();
            }
            if (ImGui.Button("Lock Gag", new Vector2(ImGui.GetContentRegionAvail().X, 25 * ImGuiHelpers.GlobalScale))) {
                LockGagOnPlayer(layer, _lockLabel, _characterHandler.activeListIdx,
                _characterHandler, _chatManager, _messageEncoder, _clientState, _chatGui, _config);
                // Start a 5-second cooldown timer
                _interactOrPermButtonEvent.Invoke();
            }
            if (ImGui.Button("Unlock Gag", new Vector2(ImGui.GetContentRegionAvail().X, 25 * ImGuiHelpers.GlobalScale))) {
                // if our selected dropdown lock label doesnt match the currently equipped type of the player, send an error message to the chat
                if(_lockLabel != _characterHandler.whitelistChars[_characterHandler.activeListIdx]._selectedGagPadlocks[layer].ToString()) {
                    GagSpeak.Log.Debug($"[Whitelist]: Selected lock type does not match equipped lock type of that player! "+
                    $"({_lockLabel} != {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._selectedGagPadlocks[layer].ToString()})");
                    _chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Selected lock type does not match equipped lock "+
                        $"type of that player!").AddItalicsOff().BuiltString
                    );
                } else {
                    UnlockGagOnPlayer(layer, _lockLabel, _characterHandler.activeListIdx, _characterHandler,
                    _config, _chatGui, _chatManager, _messageEncoder, _clientState);
                    // Start a 5-second cooldown timer
                    _interactOrPermButtonEvent.Invoke();
                }
            }
            if (ImGui.Button("Remove This Gag", new Vector2(ImGui.GetContentRegionAvail().X, 25 * ImGuiHelpers.GlobalScale))) {
                RemoveGagFromPlayer(layer, _gagLabel, _characterHandler.activeListIdx,
                _characterHandler, _chatManager, _messageEncoder, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                _interactOrPermButtonEvent.Invoke();
            }
            if (ImGui.Button("Remove All Gags", new Vector2(ImGui.GetContentRegionAvail().X, 25 * ImGuiHelpers.GlobalScale))) {
                RemoveAllGagsFromPlayer(_characterHandler.activeListIdx,
                _characterHandler, _chatManager, _messageEncoder, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                _interactOrPermButtonEvent.Invoke();
            }
        } // end our info table
        // pop the spacing
        ImGui.PopStyleVar();
        
    }
#endregion GeneralPerms

#region ButtonHelpers
	/// <summary> Is called whenever a button on the UI is pressed to gag the player. It references the current active whitelist
    /// player, and applies only if they meet the conditions (relative to your stored info on them). </summary>
    public static void ApplyGagOnPlayer(int layer, string gagType, int listIdx, CharacterHandler characterHandler,
    ChatMessages.ChatManager chatManager, ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState,
    IChatGui chatGui) {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (characterHandler.IsIndexWithinBounds(listIdx)) { return; }
        // print to chat so the player has a log of what they did
        if(characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer] != "None")
        {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot apply a gag to layer {layer},"+
                "a gag is already on this layer!").AddItalicsOff().BuiltString);
        } 
        else
        {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Applying a "+ gagType+
                $"to {characterHandler.whitelistChars[listIdx]._name}").AddItalicsOff().BuiltString);
            // update information and send message
            string targetPlayer = characterHandler.whitelistChars[listIdx]._name + "@" + characterHandler.whitelistChars[listIdx]._homeworld;
            chatManager.SendRealMessage(gagMessages.GagEncodedApplyMessage(playerPayload, targetPlayer, gagType, (layer+1).ToString()));
            characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer] = gagType; // note that this wont always be accurate, and is why request info exists.
        }
    }
#region LockButtons
    /// <summary> Controls logic for what to do once the Lock Gag button is pressed in the whitelist tab. </summary>
    public static void LockGagOnPlayer(int layer, string lockLabel, int listIdx, CharacterHandler characterHandler,
    ChatMessages.ChatManager chatManager, ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState,
    IChatGui chatGui, GagSpeakConfig _config) {
        // get payload
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (characterHandler.IsIndexWithinBounds(listIdx)) { return; }
        // only continues if selected whitelisted player info says they already have a gag on, and it has no padlock
        if (characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] == Padlocks.None
         && characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer] != "None")
        {
            // get the padlock type
            Enum.TryParse(lockLabel, true, out Padlocks padlockType);
            GagSpeak.Log.Debug($"Padlock Type: {padlockType}, assigning to whitelisted user {characterHandler.whitelistChars[listIdx]._name}");
            _config.whitelistPadlockIdentifier.SetType(padlockType);
            GagSpeak.Log.Debug($"Validating password for {characterHandler.whitelistChars[listIdx]._name}");
            _config.whitelistPadlockIdentifier.ValidatePadlockPasswords(
                true, characterHandler, playerPayload.PlayerName, characterHandler.whitelistChars[listIdx]._name, playerPayload.PlayerName);
            
            // if we make it here, we have a valid password, so we can send the message            
            string targetPlayer = characterHandler.whitelistChars[listIdx]._name + "@" + characterHandler.whitelistChars[listIdx]._homeworld;
            // we'll execute a kind of padlock and do extra checks, based on the type it is.

            // these require no password
            if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.MetalPadlock
            || _config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock
            || _config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock)
            {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, (layer+1).ToString()));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {characterHandler.whitelistChars[listIdx]._name}"+
                $"'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");
                characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = _config.whitelistPadlockIdentifier._padlockType;
                if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock) {
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockTimer[layer] = UIHelpers.GetEndTime("5m");
                } else if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock) {
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockAssigner[layer] = playerPayload.PlayerName;
                }
            }
            // this requires mistress status and a timer
            else if (_config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressTimerPadlock)
            {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), _config.whitelistPadlockIdentifier._inputTimer));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {characterHandler.whitelistChars[listIdx]._name}"+
                $"'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");
                characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = _config.whitelistPadlockIdentifier._padlockType;
                characterHandler.whitelistChars[listIdx]._selectedGagPadlockAssigner[layer] = playerPayload.PlayerName;
                characterHandler.whitelistChars[listIdx]._selectedGagPadlockTimer[layer] = UIHelpers.GetEndTime(_config.whitelistPadlockIdentifier._inputTimer);
            }
            // for combination padlock
            else if (_config.whitelistPadlockIdentifier._padlockType == Padlocks.CombinationPadlock)
            {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), _config.whitelistPadlockIdentifier._inputCombination));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking {characterHandler.whitelistChars[listIdx]._name}"+
                $"'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} with a "+lockLabel+" padlock").AddItalicsOff().BuiltString);
                
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");
                characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = _config.whitelistPadlockIdentifier._padlockType;
                characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer] = _config.whitelistPadlockIdentifier._inputCombination;
            }
            // for password padlock
            else if (_config.whitelistPadlockIdentifier._padlockType == Padlocks.PasswordPadlock)
            {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), _config.whitelistPadlockIdentifier._inputPassword));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking "+
                $"{characterHandler.whitelistChars[listIdx]._name}'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} "+
                $"with a {lockLabel} padlock").AddItalicsOff().BuiltString);
                
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");
                characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = _config.whitelistPadlockIdentifier._padlockType;
                characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer] = _config.whitelistPadlockIdentifier._inputPassword;
            }
            // for timer password padlocks
            else if (_config.whitelistPadlockIdentifier._padlockType == Padlocks.TimerPasswordPadlock)
            {
                chatManager.SendRealMessage(gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockLabel, 
                (layer+1).ToString(), _config.whitelistPadlockIdentifier._inputPassword, _config.whitelistPadlockIdentifier._inputTimer));
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Locking "+
                $"{characterHandler.whitelistChars[listIdx]._name}'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} "+
                $"with a {lockLabel} padlock").AddItalicsOff().BuiltString);
                
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");
                characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = _config.whitelistPadlockIdentifier._padlockType;
                characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer] = _config.whitelistPadlockIdentifier._inputPassword;
                characterHandler.whitelistChars[listIdx]._selectedGagPadlockTimer[layer] = UIHelpers.GetEndTime(_config.whitelistPadlockIdentifier._inputTimer);
            }
        } else {
            chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot lock {characterHandler.whitelistChars[listIdx]._name}"+
            $"'s gag, it is already locked!").AddItalicsOff().BuiltString);
        }
    }
#endregion LockButtons
#region UnlockButtons
	/// <summary> Controls logic for what to do once the Unlock Gag button is pressed in the whitelist tab. </summary>
    public static void UnlockGagOnPlayer(int layer, string lockLabel, int listIdx, CharacterHandler characterHandler,
    GagSpeakConfig _config, IChatGui chatGui, ChatMessages.ChatManager chatManager, ChatMessages.MessageTransfer.MessageEncoder gagMessages,
    IClientState clientState) {
        // get payload
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (characterHandler.IsIndexWithinBounds(listIdx)) { return; }
        // get player string
        string targetPlayer = characterHandler.whitelistChars[listIdx]._name + "@" + characterHandler.whitelistChars[listIdx]._homeworld;
        // check which gag it is
        GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType}");

        if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.MetalPadlock
        || _config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock
        || _config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock
        || _config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressTimerPadlock)
        {
            if(_config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, characterHandler, playerPayload.PlayerName,
                                                                            characterHandler.whitelistChars[listIdx]._name, playerPayload.PlayerName))
            {
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType} validated for unlock");
                // update information after sending message and verifying, and setting cooldown timer
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking "+
                $"{characterHandler.whitelistChars[listIdx]._name}'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} gag")
                .AddItalicsOff().BuiltString);
                // send it
                chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString()));
                characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = Padlocks.None;
                // set if appropriate
                if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.FiveMinutesPadlock)
                {
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockTimer[layer] = DateTimeOffset.Now;
                }
                if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressPadlock
                || _config.whitelistPadlockIdentifier._padlockType == Padlocks.MistressTimerPadlock)
                {
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockTimer[layer] = DateTimeOffset.Now;
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockAssigner[layer] = "";
                }
            }
        }
        else if (_config.whitelistPadlockIdentifier._padlockType == Padlocks.CombinationPadlock)
        {
            if(_config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, characterHandler, playerPayload.PlayerName,
            characterHandler.whitelistChars[listIdx]._name, playerPayload.PlayerName))
            {
                GagSpeak.Log.Debug($"Padlock Type: {_config.whitelistPadlockIdentifier._padlockType} validated for unlock");
                if(_config.whitelistPadlockIdentifier._inputCombination == characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer]) {
                    GagSpeak.Log.Debug($"Padlock Type: Input combination matches players lock combination, sending unlock message");
                    // update information after sending message and verifying, and setting cooldown timer
                    chatGui.Print(
                        new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking {characterHandler.whitelistChars[listIdx]._name}'s"+
                        characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]+" gag").AddItalicsOff().BuiltString);
                    chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(),
                    _config.whitelistPadlockIdentifier._inputCombination));
                    // update whitelist info
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = Padlocks.None;
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer] = "";

                } else {
                    chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Incorrect combination for "+
                    $"{characterHandler.whitelistChars[listIdx]._name}'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} gag").AddItalicsOff().BuiltString);
                }
            }
        }
        else if (_config.whitelistPadlockIdentifier._padlockType == Padlocks.PasswordPadlock || _config.whitelistPadlockIdentifier._padlockType == Padlocks.TimerPasswordPadlock)
        {
            // if its one of these, validate the password is correct
            if(_config.whitelistPadlockIdentifier.ValidatePadlockPasswords(true, characterHandler, playerPayload.PlayerName, 
            characterHandler.whitelistChars[listIdx]._name, playerPayload.PlayerName))
            {
                // if they are correct, send the message
                if(_config.whitelistPadlockIdentifier._inputPassword == characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer])
                {      
                    GagSpeak.Log.Debug($"Padlock Type: Input password matches players lock password, sending unlock message");       
                    chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Unlocking "+
                    $"{characterHandler.whitelistChars[listIdx]._name}'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} gag")
                    .AddItalicsOff().BuiltString);
                    
                    chatManager.SendRealMessage(gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(),
                    _config.whitelistPadlockIdentifier._inputPassword));
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = Padlocks.None;
                    characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer] = "";
                    // set if appropriate
                    if(_config.whitelistPadlockIdentifier._padlockType == Padlocks.TimerPasswordPadlock) {
                        characterHandler.whitelistChars[listIdx]._selectedGagPadlockTimer[layer] = DateTimeOffset.Now;
                    }
                } else {
                    chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Incorrect password for "+
                    $"{characterHandler.whitelistChars[listIdx]._name}'s {characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer]} gag")
                    .AddItalicsOff().BuiltString);
                }
            }
        }
    }
#endregion UnlockButtons
#region RemoveGagButton
	/// <summary>  Controls logic for what to do once the Remove Gag button is pressed in the whitelist tab. </summary>
    public static void RemoveGagFromPlayer(int layer, string gagType, int listIdx, CharacterHandler characterHandler,
    ChatMessages.ChatManager chatManager, ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState,
    IChatGui chatGui) {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (characterHandler.IsIndexWithinBounds(listIdx)) { return; }
        // check if the current selected player's gag layer has a lock that isnt none. If it doesnt, unlock the gag, otherwise, let the player know they couldnt remove it
        if (characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] != Padlocks.None)
        {
            chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot remove {characterHandler.whitelistChars[listIdx]._name}'s "+
            $"{gagType} gag, it is locked!").AddItalicsOff().BuiltString);
            return;
        }
        else
        {
            chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing {characterHandler.whitelistChars[listIdx]._name}'s "+
            $"{gagType} gag").AddItalicsOff().BuiltString);
            // update information and send message
            string targetPlayer = characterHandler.whitelistChars[listIdx]._name + "@" + characterHandler.whitelistChars[listIdx]._homeworld;
            chatManager.SendRealMessage(gagMessages.GagEncodedRemoveMessage(playerPayload, targetPlayer, (layer+1).ToString()));
            characterHandler.whitelistChars[listIdx]._selectedGagTypes[layer] = "None";
            characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[layer] = Padlocks.None;
            characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[layer] = "";
            characterHandler.whitelistChars[listIdx]._selectedGagPadlockAssigner[layer] = "";
        }
    }

	/// <summary>  Controls logic for what to do once the Remove All Gags button is pressed in the whitelist tab. </summary>
    public static void RemoveAllGagsFromPlayer(int listIdx, CharacterHandler characterHandler, ChatMessages.ChatManager chatManager,
    ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (characterHandler.IsIndexWithinBounds(listIdx)) { return; }
        // if any gags have locks on them, then done extcute this logic
        for (int i = 0; i < characterHandler.whitelistChars[listIdx]._selectedGagTypes.Count; i++) {
            if (characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[i] != Padlocks.None) {
                chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Cannot remove "+
                $"{characterHandler.whitelistChars[listIdx]._name}'s gags, one or more of them are locked!").AddItalicsOff().BuiltString);
                return;
            }
        } // if we make it here, we are able to remove them, so remove them!
        // print to chat so the player has a log of what they did
        chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing all gags from "+
        $"{characterHandler.whitelistChars[listIdx]._name}").AddItalicsOff().BuiltString);
        // update information and send message
        string targetPlayer = characterHandler.whitelistChars[listIdx]._name + "@" + characterHandler.whitelistChars[listIdx]._homeworld;
        chatManager.SendRealMessage(gagMessages.GagEncodedRemoveAllMessage(playerPayload, targetPlayer));
        for (int i = 0; i < characterHandler.whitelistChars[listIdx]._selectedGagTypes.Count; i++) {
            characterHandler.whitelistChars[listIdx]._selectedGagTypes[i] = "None";
            characterHandler.whitelistChars[listIdx]._selectedGagPadlocks[i] = Padlocks.None;
            characterHandler.whitelistChars[listIdx]._selectedGagPadlockPassword[i] = "";
            characterHandler.whitelistChars[listIdx]._selectedGagPadlockAssigner[i] = "";
        }
    }
#endregion RemoveGagButton
#endregion ButtonHelpers
}