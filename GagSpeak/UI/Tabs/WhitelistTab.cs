using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ImGuiNET;
using OtterGui.Widgets;
using OtterGui;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.GagListings;
using GagSpeak.Chat;
using GagSpeak.Chat.MsgEncoder;
using GagSpeak.Services;
using GagSpeak.UI.UserProfile;
using GagSpeak.Utility.GagButtonHelpers;

namespace GagSpeak.UI.Tabs.WhitelistTab;

/// <summary> This class is used to handle the whitelist tab. </summary>
public class WhitelistTab : ITab, IDisposable
{
    private             UserProfileWindow           _userProfileWindow;
    private readonly    MessageEncoder              _gagMessages;               // for encoding messages to send
    private readonly    ChatManager                 _chatManager;               // for managing the chat
    private readonly    IChatGui                    _chatGui;                   // for interacting with the chatbox
    private readonly    GagSpeakConfig              _config;                    // the config
    private readonly    IClientState                _clientState;               // getting player objects
    private readonly    IDataManager                _dataManager;               // for parsing objects
    private readonly    GagListingsDrawer           _gagListingsDrawer;         // for drawing the gag listings
    private readonly    GagService                  _gagService;                // for getting the gag types
    private readonly    TimerService                _timerService;              // snag the timerservice from the main plugin for obvious reasons
    public              ReadOnlySpan<byte>          Label => "Whitelist"u8;     // set label for the whitelist tab
    private             int                         _currentWhitelistItem;      // store a value so we know which item is selected in whitelist
    private             int                         _layer;                     // layer of the gag
    private             string                      _gagLabel;                  // current selection on gag type DD
    private             string                      _lockLabel;                 // current selection on gag lock DD
    private readonly    GagTypeFilterCombo[]        _gagTypeFilterCombo;        // create an array of item combos
    private readonly    GagLockFilterCombo[]        _gagLockFilterCombo;        // create an array of item combos
    private             bool                        enableInteractions = false; // determines if we can interact with with whitelist buttons or not (for safety to prevent accidental tells)
    private             bool                        interactionButtonPressed;   // determines if we have pressed a button that communicates or not
    private             bool                        sendNext;
    private             Dictionary<string, string>  remainingTimes = new Dictionary<string, string>();

    // Constructor for the whitelist tab
    public WhitelistTab(GagSpeakConfig config, IClientState clientState, GagListingsDrawer gagListingsDrawer, ChatManager chatManager,
    IDataManager dataManager, TimerService timerService, UserProfileWindow userProfileWindow, IChatGui chatGui, GagService gagService) {
        // Set the readonlys
        _config = config;
        _chatGui = chatGui;
        _userProfileWindow = userProfileWindow;
        _timerService = timerService;
        _clientState = clientState;
        _dataManager = dataManager;
        _gagListingsDrawer = gagListingsDrawer;
        _gagService = gagService;
        _gagMessages = new MessageEncoder();
        _chatManager = chatManager;
        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;
        sendNext = false;

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
    }

    // Dispose of the whitelist tab
    public void Dispose() {
        // Unsubscribe from timer events
        _timerService.RemainingTimeChanged -= OnRemainingTimeChanged;
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
        
    // draw the actual whitelist
    private void DrawWhitelist() {
        // lets first draw in the child
        using var child = ImRaii.Child("WhitelistPanel", -Vector2.One, true);
        
        // Now we can begin by creating an array of strings that store the whitelist of appended character names
        List<WhitelistCharData> whitelist = _config.Whitelist;
        // create the dropdowns
        if(whitelist.Count == 0) {
            whitelist.Add(new WhitelistCharData("None", "None", "None"));
            _config.Save();
        }

        // Create a bool for if the player is targetted (more detail on this later after experimentation)
        bool playerTargetted = _clientState.LocalPlayer != null && _clientState.LocalPlayer.TargetObject != null;
        bool playerCloseEnough = playerTargetted && 
                                 Vector3.Distance( _clientState.LocalPlayer?.Position ?? default, _clientState.LocalPlayer?.TargetObject?.Position ?? default) < 3;
        // Create a table for the whitelist
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
        using (var table = ImRaii.Table("WhitelistTable", 2)) {
            if (!table) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("Player Whitelist", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 200);
            ImGui.TableSetupColumn("Relation Manager", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 260);

            // NextRow
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            
            // Create the listbox for the whitelist
            ImGui.SetNextItemWidth(ImGui.GetContentRegionMax().X-5);
            string[] whitelistNames = whitelist.Select(entry => entry.name).ToArray();
            ImGui.ListBox("##whitelist", ref _currentWhitelistItem, whitelistNames, whitelistNames.Length, 11);

            // Create the second column of the first row
            ImGui.TableNextColumn();

            // Create a table for the relations manager
            using (var table2 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
                if (!table2)
                    return;

                // Create the headers for the table
                ImGui.TableSetupColumn("Statistic", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Become Their Mistress").X);
                ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
                
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                UIHelpers.Checkbox("Interactions", "WARNING: Make sure other people on your whitelist have this plugin too! (sends tells to players)\n" +
                "Allows for direct communication. Encoded to look natural, but still look wierd out of context!", enableInteractions, v => enableInteractions = v, _config);
                
                // the cooldown timer should be displayed here
                if (interactionButtonPressed) {
                    ImGui.TableNextColumn();
                    ImGui.Text($"Usage CD: {remainingTimes.GetValueOrDefault("InteractionCooldown", "N/A")}");
                }
                ImGui.TableNextRow();


                // if we dont have interactions enabled, or a communication button was pressed and we are on cooldown, disable them!
                if(!enableInteractions || interactionButtonPressed)
                    ImGui.BeginDisabled();
                
                // Draw out the line that displays the players defined relationship towards you
                ImGuiUtil.DrawFrameColumn($"You are {whitelist[_currentWhitelistItem].name.Split(' ')[0]}'s: "); ImGui.TableNextColumn();
                var width2 = new Vector2(ImGui.GetContentRegionAvail().X, 0);
                ImGui.Text($"{whitelist[_currentWhitelistItem].relationshipStatus}");

                // Draw out the line that displays your defined relationship towards that player
                ImGuiUtil.DrawFrameColumn($"{whitelist[_currentWhitelistItem].name.Split(' ')[0]} is your: "); ImGui.TableNextColumn();
                ImGui.Text($"{whitelist[_currentWhitelistItem].relationshipStatusToYou}");



                ImGuiUtil.DrawFrameColumn("Commitment Length: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
                ImGui.Text($"{whitelist[_currentWhitelistItem].GetCommitmentDuration()}");

                style.Pop();
                
                // send a encoded request to the player to request beooming their mistress
                ImGuiUtil.DrawFrameColumn("Become Their Mistress: "); ImGui.TableNextColumn(); // Next Row (Request To Become Players Mistress)
                if (ImGui.Button("Request Relation##ReqMistress", width2)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their mistress");
                    GagButtonHelpers.RequestMistressToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                    // Start a 5-second cooldown timer
                    interactionButtonPressed = true;
                    _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
                }


                // send a encoded request to the player to request beooming their pet
                ImGuiUtil.DrawFrameColumn("Become Their Pet: "); ImGui.TableNextColumn();
                if (ImGui.Button("Request Relation##ReqPet", width2)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their pet");
                    GagButtonHelpers.RequestPetToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                    // Start a 5-second cooldown timer
                    interactionButtonPressed = true;
                    _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
                }
            
                // send a encoded request to the player to request beooming their slave
                ImGuiUtil.DrawFrameColumn("Become Their Slave: "); ImGui.TableNextColumn(); 
                if (ImGui.Button("Request Relation##ReqSlave", width2)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their slave");
                    GagButtonHelpers.RequestSlaveToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);

                    // Start a 5-second cooldown timer
                    interactionButtonPressed = true;
                    _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
                }
            
            } // end our relations manager table
            var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
            ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
            var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X/2, 22.0f * ImGuiHelpers.GlobalScale );
            // create a button for popping out the current players profile. Update the currently selected person for the profile list
            _userProfileWindow._profileIndexOfUserSelected = _currentWhitelistItem;
            // add a button to display it
            if (ImGui.Button("Show Profile", buttonWidth)) {
                // Get the currently selected user
                var selectedUser = _config.Whitelist[_currentWhitelistItem];
                // Check if the UserProfileWindow is already open
                _userProfileWindow.Toggle();
            }
            
            // draw the relationship removal
            ImGui.SameLine();
            if(whitelist[_currentWhitelistItem].relationshipStatus == "None") {
                ImGui.BeginDisabled();
                if (ImGui.Button("Remove Relation##RemoveOne", buttonWidth)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                    GagButtonHelpers.RequestRelationRemovalToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                    // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
                }
                ImGui.EndDisabled();
            } else {
                if (ImGui.Button("Remove Relation##RemoveTwo", buttonWidth)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                    GagButtonHelpers.RequestRelationRemovalToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                    // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
                }
            } 

            if(!enableInteractions || interactionButtonPressed)
                ImGui.EndDisabled();

            // Message to display based on target proximity
            string targetedPlayerText = "Add Targeted Player"; // Displays if no target
            if (!playerTargetted) {
                targetedPlayerText = "No Player Target!"; // If not tagetting a player, display "No Target"
                ImGui.BeginDisabled(); // Disable the button since no target to add
            } else if (playerTargetted && !playerCloseEnough) {
                targetedPlayerText = "Player Too Far!"; // If target is too far, display "Too Far"
                ImGui.BeginDisabled(); // Disable the button since target is too far
            }

            // Create a button for adding the targetted player to the whitelist, assuming they are within proxy.
            if (ImGui.Button(targetedPlayerText, buttonWidth)) {
                // prevent possible null in _clientState.LocalPlayer.TargetObject
                if (_clientState.LocalPlayer != null &&_clientState.LocalPlayer.TargetObject != null)
                {
                    if (_clientState.LocalPlayer.TargetObject.ObjectKind == ObjectKind.Player) { // if the player is targetting another player
                        GagSpeak.Log.Debug($"[Whitelist]: Targeted Player: {_clientState.LocalPlayer.TargetObject.Name.TextValue}");
                        string targetName = UIHelpers.CleanSenderName(_clientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                        // if the object kind of the target is a player, then get the character parse of that player
                        var targetCharacter = (PlayerCharacter)_clientState.LocalPlayer.TargetObject;
                        // now we can get the name and world from them
                        var world = targetCharacter.HomeWorld.Id;
                        var worldName = _dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow((uint)world)?.Name?.ToString() ?? "Unknown";
                        GagSpeak.Log.Debug($"[Whitelist]: Targeted Player: {targetName} from {world}, {worldName}");

                        // And now, if the player is not already in our whitelist, we will add them. Otherwise just do nothing.
                        if (!_config.Whitelist.Any(item => item.name == targetName)) {
                            GagSpeak.Log.Debug($"[Whitelist]: Adding targeted player to whitelist {_clientState.LocalPlayer.TargetObject})");
                            if(whitelist.Count == 1 && whitelist[0].name == "None") { // If our whitelist just shows none, replace it with first addition.
                                whitelist[0] = new WhitelistCharData(targetName, worldName, "None");
                                _config.Save();
                            } else {
                                _config.Whitelist.Add(new WhitelistCharData(targetName, worldName, "None")); // Add the player to the whitelist
                                _config.Save();
                            }
                        }
                    }
                }
            }
            // If the player is not targetted or not close enough, end the disabled button
            if (!playerTargetted || !playerCloseEnough)
                ImGui.EndDisabled();
            
            // Also give people the option to remove someone from the whitelist.
            ImGui.SameLine();
            if (ImGui.Button("Remove Player", buttonWidth)) {
                if (whitelist.Count == 1) {
                    whitelist[0] = new WhitelistCharData("None","None","None");
                } else {
                    _config.Whitelist.Remove(_config.Whitelist[_currentWhitelistItem]);
                }
                _currentWhitelistItem = 0;
                _config.Save();
            }
        } // end our main table

        // create button widths 
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X / 2, 25.0f * ImGuiHelpers.GlobalScale );
        // 
        if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Mistress" || 
            whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Pet" ||
            whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Slave") {
            // Display buttons only if there is an incoming request
            var relationText = whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer?.Split(' ')[0];
            if (ImGui.Button($"Accept {whitelist[_currentWhitelistItem].name.Split(' ')[0]} as your {relationText}", new Vector2(ImGui.GetContentRegionAvail().X/2, 25))) {
                // Handle accept button action here
                if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Mistress") {
                    GagButtonHelpers.AcceptMistressRequestFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                } else if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Pet") {
                    GagButtonHelpers.AcceptPetRequestFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                } else if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Slave") {
                    GagButtonHelpers.AcceptSlaveRequestFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                }
                // set the relation request to established
                whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer = "Established";
                GagSpeak.Log.Debug($"[Whitelist]: Accepting incoming relation request from {whitelist[_currentWhitelistItem].name}");
            }
            ImGui.SameLine();
            if (ImGui.Button($"Decline {whitelist[_currentWhitelistItem].name.Split(' ')[0]}'s Request", new Vector2(ImGui.GetContentRegionAvail().X, 25))) {
                // Handle decline button action here
                if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Mistress") {
                    GagButtonHelpers.DeclineMistressRequestFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                } else if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Pet") {
                    GagButtonHelpers.DeclinePetRequestFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                } else if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Slave") {
                    GagButtonHelpers.DeclineSlaveRequestFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem], 
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                }
                // set the relation request to none
                whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer = "";
                GagSpeak.Log.Debug($"[Whitelist]: Declining {whitelist[_currentWhitelistItem].name}'s relation request");
            }
        }

        // create a collapsing header for this.
        if(!ImGui.CollapsingHeader("PLAYER's Interaction Options")) { return; }

        if(!enableInteractions || interactionButtonPressed) { ImGui.BeginDisabled(); }

        // create a new table for this section
        using (var InfoTable = ImRaii.Table("InfoTable", 1)) {
            // for our first row, display the DD for layer, DD gag type, and apply gag to player buttons
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            // create a combo for the layer, with options 1, 2, 3. Store selection to variable layer
            int layer = _layer;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
            ImGui.Combo("##Layer", ref layer, new string[] { "Layer 1", "Layer 2", "Layer 3" }, 3);
            _layer = layer;
            ImGui.SameLine();
            // create a dropdown for the gag type,
            int width = (int)(ImGui.GetContentRegionAvail().X / 2.5);
            _gagListingsDrawer.DrawGagTypeItemCombo((layer)+10, whitelist[_currentWhitelistItem], ref _gagLabel, layer, false, width, _gagTypeFilterCombo[layer]);
            ImGui.SameLine();

            // Create the button for the first row, third column
            if (ImGui.Button("Apply Gag To Player")) {
                // execute the generation of the apply gag layer string
                GagButtonHelpers.ApplyGagOnPlayer(layer, _gagLabel, _currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
                _config, _chatManager, _gagMessages, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }

            // for our second row, gag lock options and buttons
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // set up a temp password storage field here.
            width = (int)(ImGui.GetContentRegionAvail().X / 2.8);
            _gagListingsDrawer.DrawGagLockItemCombo((layer)+10, whitelist[_currentWhitelistItem], ref _lockLabel, layer, false, width, _gagLockFilterCombo[layer]);
            ImGui.SameLine();
            if (ImGui.Button("Lock Gag")) {
                GagButtonHelpers.LockGagOnPlayer(layer, _lockLabel, _currentWhitelistItem, whitelist[_currentWhitelistItem],
                _config, _chatManager, _gagMessages, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.SameLine();
            if (ImGui.Button("Unlock Gag")) {
                GagButtonHelpers.UnlockGagOnPlayer(layer, _currentWhitelistItem, whitelist[_currentWhitelistItem],
                _config, _chatManager, _gagMessages, _clientState, _chatGui, _lockLabel);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // display the password field, if any.
            var tempwidth = ImGui.GetContentRegionAvail().X *.675f;
            ImGui.Columns(2,"Password Divider", false);
            ImGui.SetColumnWidth(0, tempwidth);
            Enum.TryParse(_lockLabel, out GagPadlocks parsedLockType);
            if(_config._whitelistPadlockIdentifier.DisplayPasswordField(parsedLockType)) {
                // display the password field
            } else {
                ImGui.NewLine();
            }

            // Gag removal
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            if (ImGui.Button("Remove This Gag")) {
                GagButtonHelpers.RemoveGagFromPlayer(layer, _gagLabel, _currentWhitelistItem, whitelist[_currentWhitelistItem],
                _config, _chatManager, _gagMessages, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.SameLine();
            if (ImGui.Button("Remove All Gags")) {
                GagButtonHelpers.RemoveAllGagsFromPlayer(_currentWhitelistItem, whitelist[_currentWhitelistItem],
                _config, _chatManager, _gagMessages, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            
            // add a filler row
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // Create the button for the sixth row, first column
            if (ImGui.Button("Toggle Lock Live Chat Garbler")) {
                var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem]; // get the selected whitelist item
                // the player you are doing this on must be a relationstatus of slave
                if(selectedWhitelistItem.relationshipStatus == "Slave") {
                    selectedWhitelistItem.lockedLiveChatGarbler = true; // modify property.
                    _config.Whitelist[_currentWhitelistItem] = selectedWhitelistItem; // update the whitelist
                    GagButtonHelpers.OrderLiveGarbleLockToPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
                    _config, _chatManager, _gagMessages, _clientState, _chatGui);
                } else {
                    GagSpeak.Log.Debug("[Whitelist]: Player must be a slave relation to you in order to toggle this!");
                }
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.SameLine();
            if (ImGui.Button("Request Player Info")) {
                // send a message to the player requesting their current info
                GagSpeak.Log.Debug("[Whitelist]: Sending Request for Player Info");
                GagButtonHelpers.RequestInfoFromPlayer(_currentWhitelistItem, _config.Whitelist[_currentWhitelistItem],
                _config, _chatManager, _gagMessages, _clientState, _chatGui);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
        } // end our info table

        // Use ImGui.EndDisabled() to end the disabled state
        if(!enableInteractions || interactionButtonPressed) { ImGui.EndDisabled(); }
    }

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


