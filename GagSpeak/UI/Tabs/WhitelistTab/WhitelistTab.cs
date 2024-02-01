using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using ImGuiNET;
using OtterGui.Widgets;
using OtterGui;
using OtterGui.Classes;
using GagSpeak.Data;
using GagSpeak.Utility;
using GagSpeak.UI.ComboListings;
using GagSpeak.Chat;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.Services;
using GagSpeak.UI.UserProfile;
using GagSpeak.Utility.GagButtonHelpers;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace GagSpeak.UI.Tabs.WhitelistTab;

/// <summary> This class is used to handle the _config.whitelist tab. </summary>
public class WhitelistTab : ITab, IDisposable
{
    private             WhitelistListDisplay        _listDisplay;      // for drawing the _config.whitelist
    private             WhitelistPermissionEditor   _permissionEditor;  // for drawing the _config.whitelist permissions
    private readonly    InteractOrPermButtonEvent   _buttonInteractionEvent; // for handling the button interaction event to start the timer
    private readonly    MessageEncoder              _messageEncoder;               // for encoding messages to send
    private readonly    ChatManager                 _chatManager;               // for managing the chat
    private readonly    IChatGui                    _chatGui;                   // for interacting with the chatbox
    private readonly    GagSpeakConfig              _config;                    // the config
    private readonly    IClientState                _clientState;               // getting player objects
    private readonly    IDataManager                _dataManager;               // for parsing objects
    private readonly    GagListingsDrawer           _gagListingsDrawer;         // for drawing the gag listings
    private readonly    GagService                  _gagService;                // for getting the gag types
    private readonly    TimerService                _timerService;              // snag the timerservice from the main plugin for obvious reasons
    public              ReadOnlySpan<byte>          Label => "Whitelist"u8;     // set label for the _config.whitelist tab
    private             int                         _currentWhitelistItem;      // store a value so we know which item is selected in _config.whitelist
    private             int                         _layer;                     // layer of the gag
    private             string                      _gagLabel;                  // current selection on gag type DD
    private             string                      _lockLabel;                 // current selection on gag lock DD
    private readonly    GagTypeFilterCombo[]        _gagTypeFilterCombo;        // create an array of item combos
    private readonly    GagLockFilterCombo[]        _gagLockFilterCombo;        // create an array of item combos
    private             bool                        enableInteractions = false; // determines if we can interact with with _config.whitelist buttons or not (for safety to prevent accidental tells)
    private             bool                        interactionButtonPressed;   // determines if we have pressed a button that communicates or not
    public              int                         currentWhitelistItem = 0;   // The current selected _config.whitelist item
    private             Dictionary<string, string>  remainingTimes = new Dictionary<string, string>();

    // Constructor for the _config.whitelist tab
    public WhitelistTab(WhitelistListDisplay listDisplay, WhitelistPermissionEditor permissionEditor, InteractOrPermButtonEvent buttonInteractionEvent,
    GagSpeakConfig config, IClientState clientState, GagListingsDrawer gagListingsDrawer, ChatManager chatManager, MessageEncoder messageEncoder,
    IDataManager dataManager, TimerService timerService, UserProfileWindow userProfileWindow, IChatGui chatGui, GagService gagService) {
        // Set the readonlys
        _listDisplay = listDisplay;
        _permissionEditor = permissionEditor;
        _buttonInteractionEvent = buttonInteractionEvent;
        _config = config;
        _chatGui = chatGui;
        _timerService = timerService;
        _clientState = clientState;
        _dataManager = dataManager;
        _gagListingsDrawer = gagListingsDrawer;
        _gagService = gagService;
        _messageEncoder = messageEncoder;
        _chatManager = chatManager;
        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;

        // draw out our gagtype filter combo listings
        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_gagService, _config),
            new GagTypeFilterCombo(_gagService, _config),
            new GagTypeFilterCombo(_gagService, _config)
        };
        // draw out our gagpadlock filter combo listings
        _gagLockFilterCombo = new GagLockFilterCombo[] {
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config)
        };

        // subscribe to our events
        _timerService.RemainingTimeChanged += OnRemainingTimeChanged;
        _buttonInteractionEvent.ButtonPressed += OnInteractOrPermButtonPressed;
    }

    // Dispose of the _config.whitelist tab
    public void Dispose() {
        // Unsubscribe from timer events
        _timerService.RemainingTimeChanged -= OnRemainingTimeChanged;
        _buttonInteractionEvent.ButtonPressed -= OnInteractOrPermButtonPressed;
        remainingTimes = new Dictionary<string, string>();
    }
    
    // Draw the content for the window of the Whitelist Tab
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;

        // Draw the child grouping for the Whitelist Tab
        using (var child2 = ImRaii.Child("WhitelistChild")) {
            DrawHeader();
            DrawWhitelist();
        }
    }

    private void DrawHeader() // Draw our header
        => WindowHeader.Draw("Whitelist Manager", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    // automates the startCooldown process across all our classes.
    private void OnInteractOrPermButtonPressed(object sender, InteractOrPermButtonEventArgs e) {
        interactionButtonPressed = true;
        _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    }

#region Whitelist Draw
    // draw the actual _config.whitelist
    private void DrawWhitelist() {
        // lets first draw in the child
        using var child = ImRaii.Child("WhitelistPanel", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
        using (var table = ImRaii.Table("WhitelistTable", 2)) {
            if (!table) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("Player Whitelist", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 200);
            ImGui.TableSetupColumn("Relation Manager", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 260);
            // NextRow
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            UIHelpers.Checkbox("Interactions", "WARNING: Make sure other people on your _config.whitelist have this plugin too! (sends tells to players)\n" +
            "Allows for direct communication. Encoded to look natural, but still look wierd out of context!", enableInteractions, v => enableInteractions = v, _config);
            // the cooldown timer should be displayed here
            if (interactionButtonPressed) {
                ImGui.SameLine();
                ImGui.Text($"Usage CD: {remainingTimes.GetValueOrDefault("InteractionCooldown", "N/A")}");
            }
            // Create the listbox for the _config.whitelist
            _listDisplay.Draw(ref _currentWhitelistItem);
            // Create the second column of the first row
            ImGui.TableNextColumn();
            // from here on out, if interactions is not checked, or our ispressed is on cooldown, disable everything below here
            if(!enableInteractions || interactionButtonPressed) { ImGui.BeginDisabled(); }
            // draw out our permissions manager
            _permissionEditor.Draw(ref _currentWhitelistItem);

            // make it enabled
            if(!enableInteractions || interactionButtonPressed) { ImGui.EndDisabled(); }
        }

        // create button widths 
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X / 2, 25.0f * ImGuiHelpers.GlobalScale );

        if (_config.whitelist[_currentWhitelistItem]._pendingRelationRequestFromPlayer != RoleLean.None) { 
            RoleLean pendingDynamic = _config.whitelist[_currentWhitelistItem]._pendingRelationRequestFromPlayer;
            // draw the accept and decline buttons for the pending relation request
            var relationText = pendingDynamic.ToString()?.Split(' ')[0];
            if (ImGui.Button($"Accept {_config.whitelist[_currentWhitelistItem]._name.Split(' ')[0]} as your {relationText}",
                new Vector2(ImGui.GetContentRegionAvail().X/2, 25)))
            {
                AcceptRequestForDynamic(_currentWhitelistItem);
                // set the relation request to established
                _config.whitelist[_currentWhitelistItem]._pendingRelationRequestFromPlayer = RoleLean.None;
                GagSpeak.Log.Debug($"[Whitelist]: Accepting incoming relation request from {_config.whitelist[_currentWhitelistItem]._name}");
            }
            ImGui.SameLine();
            if (ImGui.Button($"Decline {_config.whitelist[_currentWhitelistItem]._name.Split(' ')[0]}'s Request",
                new Vector2(ImGui.GetContentRegionAvail().X, 25)))
            {
                DeclineRequestForDynamic(_currentWhitelistItem);
                // set the relation request to none
                _config.whitelist[_currentWhitelistItem]._pendingRelationRequestFromPlayer = RoleLean.None;
                GagSpeak.Log.Debug($"[Whitelist]: Declining {_config.whitelist[_currentWhitelistItem]._name}'s relation request");
            }
        }

        // Replace this with a tab based heading later

        // create a collapsing header for this.
        //DrawGagInteractions();
        // create a collapsing header for this.
        //DrawWardrobeInteractions();
    }
#endregion Whitelist Draw

#region WhitelistHelpers

    /// <summary>  Applies logic for accepting a requested status from a player. </summary>
    public void AcceptRequestForDynamic(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // make sure the current whitelist item is valid
        if (currentWhitelistItem < 0 || currentWhitelistItem >= _config.whitelist.Count) { return; }
        // print to chat that you sent the request
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        RoleLean requestedDynamic = _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer;
        if(requestedDynamic == RoleLean.Owner || requestedDynamic == RoleLean.Master || requestedDynamic == RoleLean.Mistress) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_config.whitelist[currentWhitelistItem]._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // set the relationship status the player has towards you "They are your Mistress" here, because once you hit accept, both sides agree
            _config.whitelist[currentWhitelistItem]._theirStatusToYou = _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer;
            if(_config.whitelist[currentWhitelistItem]._yourStatusToThem != RoleLean.None) {
                _config.whitelist[currentWhitelistItem].Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestDominantStatus(playerPayload, targetPlayer, requestedDynamic));
        } else if(requestedDynamic == RoleLean.Pet || requestedDynamic == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_config.whitelist[currentWhitelistItem]._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // set the relationship status the player has towards you "They are your Pet" here, because once you hit accept, both sides agree
            _config.whitelist[currentWhitelistItem]._theirStatusToYou = _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer;
            if(_config.whitelist[currentWhitelistItem]._yourStatusToThem != RoleLean.None) {
                _config.whitelist[currentWhitelistItem].Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestSubmissiveStatus(playerPayload, targetPlayer, requestedDynamic));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_config.whitelist[currentWhitelistItem]._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // set the relationship status the player has towards you "They are your Absolute-Slave" here, because once you hit accept, both sides agree
            _config.whitelist[currentWhitelistItem]._theirStatusToYou = _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer;
            if(_config.whitelist[currentWhitelistItem]._yourStatusToThem != RoleLean.None) {
                _config.whitelist[currentWhitelistItem].Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer, requestedDynamic));
        }
    }

    /// <summary>  Controls logic for what to do once the the decline dynamic relation button is pressed in the whitelist tab. </summary>
    public void DeclineRequestForDynamic(int currentWhitelistItem) {
        // get the player payload    
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // make sure the current whitelist item is valid
        if (currentWhitelistItem < 0 || currentWhitelistItem >= _config.whitelist.Count) { return; }
        // print to chat that you sent the request
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        RoleLean requestedDynamic = _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer;
        // execute action based on dynamic type
        if(requestedDynamic == RoleLean.Owner || requestedDynamic == RoleLean.Master || requestedDynamic == RoleLean.Mistress) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_config.whitelist[currentWhitelistItem]._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _config.whitelist[currentWhitelistItem]._theirStatusToYou = RoleLean.None;
            _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer = RoleLean.None;
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestDominantStatus(playerPayload, targetPlayer));
        } else if(requestedDynamic == RoleLean.Pet || requestedDynamic == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_config.whitelist[currentWhitelistItem]._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _config.whitelist[currentWhitelistItem]._theirStatusToYou = RoleLean.None;
            _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer = RoleLean.None;
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestSubmissiveStatus(playerPayload, targetPlayer));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_config.whitelist[currentWhitelistItem]._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _config.whitelist[currentWhitelistItem]._theirStatusToYou = RoleLean.None;
            _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer = RoleLean.None;
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer));
        }
    }
#endregion WhitelistHelpers

#region Gag Related Interactions
    // private void DrawGagInteractions() {
    //     if(!ImGui.CollapsingHeader($"Gag Related Interactions to use on {_config.Whitelist[_currentWhitelistItem].name}")) { return; }

    //     if(!enableInteractions || interactionButtonPressed) { ImGui.BeginDisabled(); }
    //     // inform the player how this works
    //     ImGui.TextColored(new Vector4(1.0f, 0.5f, 0.0f, 1.0f), "Use REQUEST PLAYER INFO button each time you meet up to have accurate data!");
    //     // create a new table for this section
    //     using (var InfoTable = ImRaii.Table("InfoTable", 1)) {
    //         // for our first row, display the DD for layer, DD gag type, and apply gag to player buttons
    //         ImGui.TableNextRow();
    //         ImGui.TableNextColumn();
    //         // create a combo for the layer, with options 1, 2, 3. Store selection to variable layer
    //         int layer = _layer;
    //         ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
    //         ImGui.Combo("##Layer", ref layer, new string[] { "Layer 1", "Layer 2", "Layer 3" }, 3);
    //         _layer = layer;
    //         ImGui.SameLine();
    //         // create a dropdown for the gag type,
    //         int width = (int)(ImGui.GetContentRegionAvail().X / 2.5);
    //         _gagListingsDrawer.DrawGagTypeItemCombo((layer)+10, _currentWhitelistItem, ref _gagLabel,
    //                                                 layer, false, width, _gagTypeFilterCombo[layer]);
    //         ImGui.SameLine();

    //         // Create the button for the first row, third column
    //         if (ImGui.Button("Apply Gag To Player")) {
    //             // execute the generation of the apply gag layer string
    //             GagButtonHelpers.ApplyGagOnPlayer(layer, _gagLabel, _currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //             _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //             // Start a 5-second cooldown timer
    //             interactionButtonPressed = true;
    //             _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //         }

    //         // for our second row, gag lock options and buttons
    //         ImGui.TableNextRow(); ImGui.TableNextColumn();
    //         // set up a temp password storage field here.
    //         width = (int)(ImGui.GetContentRegionAvail().X / 2.8);
    //         _gagListingsDrawer.DrawGagLockItemCombo((layer)+10, _config.Whitelist[_currentWhitelistItem], ref _lockLabel, layer, false, width, _gagLockFilterCombo[layer]);
    //         ImGui.SameLine();
    //         if (ImGui.Button("Lock Gag")) {
    //             GagButtonHelpers.LockGagOnPlayer(layer, _lockLabel, _currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //             _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //             // Start a 5-second cooldown timer
    //             interactionButtonPressed = true;
    //             _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //         }
    //         ImGui.SameLine();
    //         if (ImGui.Button("Unlock Gag")) {
    //             // if our selected dropdown lock label doesnt match the currently equipped type of the player, send an error message to the chat
    //             if(_lockLabel != _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer].ToString()) {
    //                 GagSpeak.Log.Debug($"[Whitelist]: Selected lock type does not match equipped lock type of that player! ({_lockLabel} != {_config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer].ToString()})");
    //                 _chatGui.Print(
    //                     new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Selected lock type does not match equipped lock type of that player!").AddItalicsOff().BuiltString
    //                 );
    //             } else {
    //                 GagButtonHelpers.UnlockGagOnPlayer(layer, _lockLabel, _currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //                 _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //                 // Start a 5-second cooldown timer
    //                 interactionButtonPressed = true;
    //                 _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //             }
    //         }
    //         ImGui.TableNextRow(); ImGui.TableNextColumn();
    //         // display the password field, if any.
    //         var tempwidth = ImGui.GetContentRegionAvail().X *.675f;
    //         ImGui.Columns(2,"Password Divider", false);
    //         ImGui.SetColumnWidth(0, tempwidth);
    //         Enum.TryParse(_lockLabel, out Padlocks parsedLockType);
    //         if(_config.whitelistPadlockIdentifier.DisplayPasswordField(parsedLockType)) {
    //             // display the password field
    //         } else {
    //             ImGui.NewLine();
    //         }

    //         // Gag removal
    //         ImGui.TableNextRow(); ImGui.TableNextColumn();
    //         if (ImGui.Button("Remove This Gag")) {
    //             GagButtonHelpers.RemoveGagFromPlayer(layer, _gagLabel, _currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //             _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //             // Start a 5-second cooldown timer
    //             interactionButtonPressed = true;
    //             _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //         }
    //         ImGui.SameLine();
    //         if (ImGui.Button("Remove All Gags")) {
    //             GagButtonHelpers.RemoveAllGagsFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //             _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //             // Start a 5-second cooldown timer
    //             interactionButtonPressed = true;
    //             _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //         }
            
    //         // add a filler row
    //         ImGui.TableNextRow(); ImGui.TableNextColumn();
    //         // Create the button for the sixth row, first column
    //         if (ImGui.Button("Toggle Live Garbler Lock")) {
    //             var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem]; // get the selected _config.whitelist item
    //             // the player you are doing this on must be a relationstatus of slave
    //             if(selectedWhitelistItem._yourStatusToThem == "Mistress" || selectedWhitelistItem._theirStatusToYou == "Slave") {
    //                 selectedWhitelistItem.lockedLiveChatGarbler = true; // modify property.
    //                 _config.Whitelist[_currentWhitelistItem] = selectedWhitelistItem; // update the _config.whitelist
    //                 GagButtonHelpers.OrderLiveGarbleLockToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //                 _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //             } else {
    //                 GagSpeak.Log.Debug("[Whitelist]: Player must be a slave relation to you in order to toggle this!");
    //             }
    //             // Start a 5-second cooldown timer
    //             interactionButtonPressed = true;
    //             _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //         }
    //         ImGui.SameLine();
    //         if (ImGui.Button("Request Player Info")) {
    //             // send a message to the player requesting their current info
    //             GagSpeak.Log.Debug("[Whitelist]: Sending Request for Player Info");
    //             GagButtonHelpers.RequestInfoFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
    //             _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //             // Start a 5-second cooldown timer
    //             interactionButtonPressed = true;
    //             _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //         }
    //     } // end our info table

    //     // Use ImGui.EndDisabled() to end the disabled state
    //     if(!enableInteractions || interactionButtonPressed) { ImGui.EndDisabled(); }
    // }



#endregion Gag Related Interactions
#region Wardrobe Related Interactions
    // private void DrawWardrobeInteractions() {
    //     if(!ImGui.CollapsingHeader($"Wardrobe Related Interactions to use on {_config.Whitelist[_currentWhitelistItem].name}")) { return; }
        
    //     if(!enableInteractions || interactionButtonPressed) { ImGui.BeginDisabled(); }

    //     // draw out a textfield for a person to enter their restraint set name
    //     ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 2/3);
    //     if (ImGui.InputTextWithHint("##RestraintSetNameToApply", "Enter a Restraint Set Name...", ref restraintSetNameToApply, 36, ImGuiInputTextFlags.None));
    //     string result = restraintSetLockTimer; // get the input timer storage
    //     ImGui.SameLine();
    //     ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
    //     if (ImGui.InputTextWithHint("##RestraintSetTimer", "Lock Duration: 0h2m7s...", ref result, 24, ImGuiInputTextFlags.None)) {
    //         restraintSetLockTimer = result;
    //     }
    //     // next row
    //     ImGui.NextColumn();
    //     var width = ImGui.GetContentRegionAvail().X/2;
    //     if (ImGui.Button("Lock The Spesified Restraint Set", new Vector2(width, 25))) {
    //         // send a message to the player requesting their current info
    //         GagSpeak.Log.Debug("[Whitelist]: Sending Request to apply restraint set");
    //         GagButtonHelpers.LockRestraintSetToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], restraintSetNameToApply,
    //         restraintSetLockTimer, _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //         // Start a 5-second cooldown timer
    //         interactionButtonPressed = true;
    //         _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //     }
    //     ImGui.SameLine();
    //     if (ImGui.Button("Unlock The Spesified Restraint Set", new Vector2(width, 25))) {
    //         // send a message to the player requesting their current info
    //         GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove restraint set");
    //         GagButtonHelpers.UnlockRestraintSetToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], restraintSetNameToApply,
    //         _config, _chatManager, _gagMessages, _clientState, _chatGui);
    //         // Start a 5-second cooldown timer
    //         interactionButtonPressed = true;
    //         _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
    //     }
    //     // Use ImGui.EndDisabled() to end the disabled state
    //     if(!enableInteractions || interactionButtonPressed) { ImGui.EndDisabled(); }
    // }
#endregion Wardrobe Related Interactions
    /// <summary>
    /// This method is used to handle the remaining time changed event.
    /// </summary>
    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime) {
        if(timerName == "InteractionCooldown") {
            remainingTimes[timerName] = $"{remainingTime.TotalSeconds:F1}s";
            return;
        }
    }
}


