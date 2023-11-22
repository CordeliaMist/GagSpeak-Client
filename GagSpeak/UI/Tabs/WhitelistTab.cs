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
using GagSpeak.UI.UserProfile;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;


namespace GagSpeak.UI.Tabs.WhitelistTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class WhitelistTab : ITab, IDisposable
{
    private UserProfileWindow _userProfileWindow;
    private readonly MessageEncoder _gagMessages; // snag the whitelistchardata from the main plugin for obvious reasons
    private readonly ChatManager _chatManager; // snag the chatmanager from the main plugin for obvious reasons
    private readonly IChatGui _chatGui; // snag the chatgui from the main plugin for obvious reasons
    private readonly GagSpeakConfig _config; // snag the conmfig from the main plugin for obvious reasons
    private readonly IClientState _clientState; // snag the clientstate from the main plugin for obvious reasons
    private readonly IDataManager _dataManager; // for parsing objects
    private readonly GagListingsDrawer _gagListingsDrawer; // snag the gaglistingsdrawer from the main plugin for obvious reasons
    private readonly TimerService _timerService; // snag the timerservice from the main plugin for obvious reasons
    public ReadOnlySpan<byte> Label => "Whitelist"u8; // set label for the whitelist tab
    private bool emptyList = false; // if the whitelist is empty, disable lower section

    // temp variables for the whitelist tab
    private int _currentWhitelistItem; // store a value so we know which item is selected in whitelist
    private int _layer; // layer of the gag
    private string _gagLabel; // current selection on gag type DD
    private string _lockLabel; // current selection on gag lock DD
    private readonly GagTypeFilterCombo[] _gagTypeFilterCombo; // create an array of item combos
    private readonly GagLockFilterCombo[] _gagLockFilterCombo; // create an array of item combos

    // store time information & control locks
    private bool enableInteractions = false; // determines if we can interact with with whitelist buttons or not (for safety to prevent accidental tells)
    private bool interactionButtonPressed; // determines if we have pressed a button that communicates or not
    private bool sendNext;
    private Dictionary<string, string> remainingTimes = new Dictionary<string, string>();

    // Constructor for the whitelist tab
    public WhitelistTab(GagSpeakConfig config, IClientState clientState, GagListingsDrawer gagListingsDrawer, ChatManager chatManager,
    IDataManager dataManager, TimerService timerService, UserProfileWindow userProfileWindow, IChatGui chatGui) {
        // Set the readonlys
        _config = config;
        _chatGui = chatGui;
        _userProfileWindow = userProfileWindow;
        _timerService = timerService;
        _clientState = clientState;
        _dataManager = dataManager;
        _gagListingsDrawer = gagListingsDrawer;
        _gagMessages = new MessageEncoder();
        _chatManager = chatManager;
        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;
        sendNext = false;

        // draw out our gagtype filter combo listings
        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_config.GagTypes, _config),
            new GagTypeFilterCombo(_config.GagTypes, _config),
            new GagTypeFilterCombo(_config.GagTypes, _config)
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

        // If the whitelist is has only none listed, disable lower section
        if (whitelist.Count == 1 && whitelist[0].name == "None") {
            emptyList = true;
        } else {
            emptyList = false;
        }

        // Create a bool for if the player is targetted (more detail on this later after experimentation)
        bool playerTargetted = _clientState.LocalPlayer != null && _clientState.LocalPlayer.TargetObject != null;
        // Create a bool for if the player is close enough to the targetted player (more detail on this later after experimentation)
        bool playerCloseEnough = playerTargetted && Vector3.Distance( _clientState.LocalPlayer.Position, _clientState.LocalPlayer.TargetObject.Position) < 3;
        
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
            ImGui.ListBox("##whitelist", ref _currentWhitelistItem, whitelistNames, whitelistNames.Length, 10);;

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
                Checkbox("Interactions", "WARNING: Make sure other people on your whitelist have this plugin too! (sends tells to players)\n" +
                "Allows for direct communication. Encoded to look natural, but still look wierd out of context!", enableInteractions, v => enableInteractions = v);
                
                // the cooldown timer should be displayed here
                if (interactionButtonPressed) {
                    ImGui.TableNextColumn();
                    ImGui.Text($"Usage CD: {remainingTimes.GetValueOrDefault("InteractionCooldown", "N/A")}");
                }
                ImGui.TableNextRow();


                // if we dont have interactions enabled, or a communication button was pressed and we are on cooldown, disable them!
                if(!enableInteractions || interactionButtonPressed)
                    ImGui.BeginDisabled();
                
                ImGuiUtil.DrawFrameColumn($"{whitelist[_currentWhitelistItem].name.Split(' ')[0]} is your: "); ImGui.TableNextColumn();
                var width2 = new Vector2(ImGui.GetContentRegionAvail().X, 0);
                ImGui.Text($"{whitelist[_currentWhitelistItem].relationshipStatus}");

                ImGuiUtil.DrawFrameColumn("Commitment Length: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
                ImGui.Text($"{whitelist[_currentWhitelistItem].GetCommitmentDuration()}");

                style.Pop();
                
                // send a encoded request to the player to request beooming their mistress
                ImGuiUtil.DrawFrameColumn("Become Their Mistress: "); ImGui.TableNextColumn(); // Next Row (Request To Become Players Mistress)
                if (ImGui.Button("Request Relation##ReqMistress", width2)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their mistress");
                    RequestMistressToPlayer(_config.Whitelist[_currentWhitelistItem]);

                    // Start a 5-second cooldown timer
                    interactionButtonPressed = true;
                    _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
                }


                // send a encoded request to the player to request beooming their pet
                ImGuiUtil.DrawFrameColumn("Become Their Pet: "); ImGui.TableNextColumn();
                if (ImGui.Button("Request Relation##ReqPet", width2)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their pet");
                    RequestPetToPlayer(_config.Whitelist[_currentWhitelistItem]);
                    // Start a 5-second cooldown timer
                    interactionButtonPressed = true;
                    _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
                }
            
                // send a encoded request to the player to request beooming their slave
                ImGuiUtil.DrawFrameColumn("Become Their Slave: "); ImGui.TableNextColumn(); 
                if (ImGui.Button("Request Relation##ReqSlave", width2)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their slave");
                    RequestSlaveToPlayer(_config.Whitelist[_currentWhitelistItem]);

                    // Start a 5-second cooldown timer
                    interactionButtonPressed = true;
                    _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
                }
            
            } // end our relations manager table
            var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
            ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
            var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X/2, 25.0f * ImGuiHelpers.GlobalScale );
            // create a button for popping out the current players profile
            // update the currently selected person for the profile list

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
                    RequestRelationRemovealToPlayer(_config.Whitelist[_currentWhitelistItem]);
                    // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
                }
                ImGui.EndDisabled();
            } else {
                if (ImGui.Button("Remove Relation##RemoveTwo", buttonWidth)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                    RequestRelationRemovealToPlayer(_config.Whitelist[_currentWhitelistItem]);
                    // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
                }
            } 

            if(!enableInteractions || interactionButtonPressed)
                ImGui.EndDisabled();

            // Message to display based on target proximity
            string targetedPlayerText = "Add Targetted Player"; // Displays if no target
            if (!playerTargetted) {
                targetedPlayerText = "No Player Target!"; // If not tagetting a player, display "No Target"
                ImGui.BeginDisabled(); // Disable the button since no target to add
            } else if (playerTargetted && !playerCloseEnough) {
                targetedPlayerText = "Player Too Far!"; // If target is too far, display "Too Far"
                ImGui.BeginDisabled(); // Disable the button since target is too far
            }

            // Create a button for adding the targetted player to the whitelist, assuming they are within proxy.
            if (ImGui.Button(targetedPlayerText, buttonWidth)) {
                if (_clientState.LocalPlayer.TargetObject.ObjectKind == ObjectKind.Player) { // if the player is targetting another player
                    string targetName = UIHelpers.CleanSenderName(_clientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                    // if the object kind of the target is a player, then get the character parse of that player
                    var targetCharacter = (PlayerCharacter)_clientState.LocalPlayer.TargetObject;
                    // now we can get the name and world from them
                    var world = targetCharacter.HomeWorld.Id;
                    var worldName = _dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow((uint)world)?.Name?.ToString();
                    GagSpeak.Log.Debug($"[Whitelist]: Targetted Player: {targetName} from {world}, {worldName}");

                    // And now, if the player is not already in our whitelist, we will add them. Otherwise just do nothing.
                    if (!_config.Whitelist.Any(item => item.name == targetName)) {
                        GagSpeak.Log.Debug($"[Whitelist]: Adding targetted player to whitelist {_clientState.LocalPlayer.TargetObject})");
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
            if (ImGui.Button($"Accept {whitelist[_currentWhitelistItem].name.Split(' ')[0]} as your {relationText}", new Vector2(ImGui.GetContentRegionAvail().X/2, 25)))
            {
                // Handle accept button action here
                // if relationship status is mistress, trigger the accept mistress function. If pet, trigger accept pet function. If slave, trigger accept slave function.
                if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Mistress") {
                    AcceptMistressRequestFromPlayer(_config.Whitelist[_currentWhitelistItem]);
                } else if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Pet") {
                    AcceptPetRequestFromPlayer(_config.Whitelist[_currentWhitelistItem]);
                } else if (whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer == "Slave") {
                    AcceptSlaveRequestFromPlayer(_config.Whitelist[_currentWhitelistItem]);
                }


                // set the relation request to established
                whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer = "Established";
                GagSpeak.Log.Debug($"[Whitelist]: Accepting incoming relation request from {whitelist[_currentWhitelistItem].name}");
            }
            ImGui.SameLine();
            if (ImGui.Button($"Decline {whitelist[_currentWhitelistItem].name.Split(' ')[0]}'s Request", new Vector2(ImGui.GetContentRegionAvail().X, 25)))
            {
                // set the relation request to established
                whitelist[_currentWhitelistItem].PendingRelationRequestFromPlayer = "None";
                GagSpeak.Log.Debug($"[Whitelist]: Declining incoming relation request from {whitelist[_currentWhitelistItem].name}");
            }
        }
        
        // create a collapsing header for this.
        if(!ImGui.CollapsingHeader("PLAYER's Interaction Options"))
            return;


        if(!enableInteractions || interactionButtonPressed)
            ImGui.BeginDisabled();
        

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
                ApplyGagOnPlayer(layer, _gagLabel, _config.Whitelist[_currentWhitelistItem]);
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
                // get your data
                PlayerPayload playerPayload;
                playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
                // get whitelist data of selected
                var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem];
                    // then we can apply the lock gag logic
                Enum.TryParse(_lockLabel, true, out GagPadlocks padlockType);
                _config._whitelistPadlockIdentifier.SetType(padlockType);
                _config._whitelistPadlockIdentifier.ValidatePadlockPasswords(true, _config, playerPayload.PlayerName, selectedWhitelistItem.name);
                string targetPlayer = selectedWhitelistItem.name + "@" + selectedWhitelistItem.homeworld;
                
                if(_config._whitelistPadlockIdentifier._padlockType == GagPadlocks.MetalPadlock ||
                _config._whitelistPadlockIdentifier._padlockType == GagPadlocks.FiveMinutesPadlock ||
                _config._whitelistPadlockIdentifier._padlockType == GagPadlocks.MistressPadlock) {
                    _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, _lockLabel, 
                    (layer+1).ToString()));
                }
                else if (_config._whitelistPadlockIdentifier._padlockType == GagPadlocks.MistressTimerPadlock) {
                    _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, _lockLabel, 
                    (layer+1).ToString(), _config._whitelistPadlockIdentifier._inputTimer));
                }
                else if (_config._whitelistPadlockIdentifier._padlockType == GagPadlocks.CombinationPadlock) {
                    _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, _lockLabel, 
                    (layer+1).ToString(), _config._whitelistPadlockIdentifier._inputCombination));
                }
                else if (_config._whitelistPadlockIdentifier._padlockType == GagPadlocks.PasswordPadlock) {
                    _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, _lockLabel, 
                    (layer+1).ToString(), _config._whitelistPadlockIdentifier._inputPassword));
                }
                else if (_config._whitelistPadlockIdentifier._padlockType == GagPadlocks.TimerPasswordPadlock) {
                    _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, _lockLabel, 
                    (layer+1).ToString(), _config._whitelistPadlockIdentifier._inputPassword, _config._whitelistPadlockIdentifier._inputTimer));
                }
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.SameLine();
            if (ImGui.Button("Unlock Gag")) {
                // get your data
                PlayerPayload playerPayload;
                playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
                // get whitelist data of selected
                var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem]; // get the selected whitelist item
                // apply similar format to lock gag
                string targetPlayer = selectedWhitelistItem.name + "@" + selectedWhitelistItem.homeworld;
                _chatManager.SendRealMessage(_gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer,
                (layer+1).ToString(), _config._whitelistPadlockIdentifier._inputPassword));
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
                RemoveGagFromPlayer(layer, _config.Whitelist[_currentWhitelistItem]);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.SameLine();
            if (ImGui.Button("Remove All Gags")) {
                RemoveAllGagsFromPlayer(_config.Whitelist[_currentWhitelistItem]);
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            
            // add a filler row
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // Create the button for the sixth row, first column
            if (ImGui.Button("Tooggle Lock Live Chat Garbler")) {
                var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem]; // get the selected whitelist item
                // the player you are doing this on must be a relationstatus of slave
                if(selectedWhitelistItem.relationshipStatus == "Slave") {
                    selectedWhitelistItem.lockedLiveChatGarbler = true; // modify property.
                    _config.Whitelist[_currentWhitelistItem] = selectedWhitelistItem; // update the whitelist
                    OrderLiveGarbleLockToPlayer(_config.Whitelist[_currentWhitelistItem]);
                } else {
                    GagSpeak.Log.Debug("[Whitelist]: Player must be a slave relation to toggle this!");
                }
                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
            ImGui.SameLine();
            if (ImGui.Button("Request Player Info")) {
                // send a message to the player requesting their current info
                GagSpeak.Log.Debug("[Whitelist]: Sending Request for Player Info");
                RequestInfoFromPlayer(_config.Whitelist[_currentWhitelistItem]);

                // Start a 5-second cooldown timer
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "5s", 100, () => { interactionButtonPressed = false; });
            }
        } // end our info table

        // Use ImGui.EndDisabled() to end the disabled state
        if(!enableInteractions || interactionButtonPressed) {
            ImGui.EndDisabled();}

        if(_config.acceptingInfoRequests == true && _config.SendInfoName == "") {
            sendNext = false;
        }

        // add a section here that scans to see if we are recieving any incoming information requests, and if we are, to respond, if off cooldown.
        if(interactionButtonPressed == false && _config.SendInfoName != "") {
            // if we are accepting info requests, and we have a name to send info to, then we will send the info, switch acceptinfo requests to false, and the name back to nothing, also start a timer that expires in 10seconds
            GagSpeak.Log.Debug("[Whitelist]: Accepting Player Info");
            if(sendNext == false) {
                // send the first half
                SendInfoToPlayer();
                sendNext = true; // now it will scan the second half
                interactionButtonPressed = true;
                _timerService.StartTimer("InteractionCooldown", "2s", 1000, () => { interactionButtonPressed = false; });
            } else if(sendNext == true) {
                // send the second half
                SendInfoToPlayer2();
                sendNext = false; // now it will scan the first half
                _config.acceptingInfoRequests = false;
                _config.SendInfoName = "";
                interactionButtonPressed = true;
                // add another timer named "RequestInfoCooldown" that expires in 20seconds, and when it does, it will set acceptingInfoRequests to true
                _timerService.StartTimer("InteractionCooldown", "4s", 1000, () => { interactionButtonPressed = false; });
            }
        }
    } // end our draw whitelist function

    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime) {
        if(timerName == "InteractionCooldown") {
            remainingTimes[timerName] = $"{remainingTime.TotalSeconds:F1}s";
            return;
        }
    }


    // Additional methods for applying, locking, unlocking, removing gags
    private void ApplyGagOnPlayer(int layer, string gagType, WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // your player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); //  name and homeworld
        // Ensure a player is selected as a valid whitelist index
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // Assuming GagMessages is a class instance, replace it with the actual instance
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.GagEncodedApplyMessage(playerPayload, targetPlayer, gagType, (layer+1).ToString()));
        // Update the selected player's data
        selectedPlayer.selectedGagTypes[layer] = gagType; // note that this wont always be accurate, and is why request info exists.
    }

    // this logic button is by far the most inaccurate, because there is no way to tell if the unlock is sucessful.
    private void UnlockGagOnPlayer(PlayerPayload playerPayload, int layer, WhitelistCharData selectedPlayer) { UnlockGagOnPlayer(playerPayload, layer, selectedPlayer, "");} 
    private void UnlockGagOnPlayer(PlayerPayload playerPayload, int layer, WhitelistCharData selectedPlayer, string password) {
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the chat message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(), password));
    }

    private void RemoveGagFromPlayer(int layer, WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.GagEncodedRemoveMessage(playerPayload, targetPlayer, (layer+1).ToString()));
        // Update the selected player's data
        selectedPlayer.selectedGagTypes[layer] = "None";
        selectedPlayer.selectedGagPadlocks[layer] = GagPadlocks.None;
        selectedPlayer.selectedGagPadlocksPassword[layer] = "";
        selectedPlayer.selectedGagPadlocksAssigner[layer] = "";
    }

    private void RemoveAllGagsFromPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;

        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.GagEncodedRemoveAllMessage(playerPayload, targetPlayer));

        // Update the selected player's data
        for (int i = 0; i < selectedPlayer.selectedGagTypes.Count; i++) {
            selectedPlayer.selectedGagTypes[i] = "None";
            selectedPlayer.selectedGagPadlocks[i] = GagPadlocks.None;
            selectedPlayer.selectedGagPadlocksPassword[i] = "";
            selectedPlayer.selectedGagPadlocksAssigner[i] = "";
        }
    }

    private void RequestMistressToPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // print to chat that you sent the request
        _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request for "+
        $"{selectedPlayer.name} to become your Mistress.").AddItalicsOff().BuiltString);
        // set your requested status and send the message!
        selectedPlayer.PendingRelationRequestFromYou = "Mistress";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.RequestMistressEncodedMessage(playerPayload, targetPlayer));
    }


    private void RequestPetToPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // print to chat that you sent the request
        _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request for "+
        $"{selectedPlayer.name} to become your Pet.").AddItalicsOff().BuiltString);
        // set your requested status and send the message!
        selectedPlayer.PendingRelationRequestFromYou = "Pet";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.RequestPetEncodedMessage(playerPayload, targetPlayer));
    }

    private void RequestSlaveToPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // print to chat that you sent the request
        _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request for "+
        $"{selectedPlayer.name} to become your Mistress.").AddItalicsOff().BuiltString);
        // set your requested status and send the message!
        selectedPlayer.PendingRelationRequestFromYou = "Slave";
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.RequestSlaveEncodedMessage(playerPayload, targetPlayer));
    }

    private void RequestRelationRemovealToPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // print to chat that you sent the request
        _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing Relation Status "+
        $"with {selectedPlayer.name}.").AddItalicsOff().BuiltString);
        // send the message
        selectedPlayer.relationshipStatus = "None"; // set the relationship status
        selectedPlayer.PendingRelationRequestFromPlayer = ""; // set any pending relations to none
        selectedPlayer.PendingRelationRequestFromYou = ""; // set any pending relations to none
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.RequestRemovalEncodedMessage(playerPayload, targetPlayer));
    }

    private void OrderLiveGarbleLockToPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // print to chat that you sent the request
        _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddRed($"[GagSpeak]").AddText($"Forcing silence upon your slave, " +
        $"hopefully {selectedPlayer.name} will behave herself~").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.OrderGarblerLockEncodedMessage(playerPayload, targetPlayer));
    }


    private void RequestInfoFromPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // print to chat that you sent the request
        _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending information request to " +
        $"{selectedPlayer.name}, please wait...").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.RequestInfoEncodedMessage(playerPayload, targetPlayer));
    }

    private void SendInfoToPlayer() {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        // format the player name from "firstname lastname homeworld" to "firstname lastname@homeworld"
        int lastSpaceIndex = _config.SendInfoName.LastIndexOf(' ');
        if (lastSpaceIndex >= 0) { // if we can do this, then do it.
            string targetPlayer = _config.SendInfoName.Remove(lastSpaceIndex, 1).Insert(lastSpaceIndex, "@");
            // get your relationship to that player, if any. Search for their name in the whitelist.
            string relationshipVar = "None";
            _config.Whitelist.ForEach(delegate(WhitelistCharData entry) {
                if (_config.SendInfoName.Contains(entry.name)) {
                    // set the relationship
                    relationshipVar = entry.relationshipStatus;
                }
            });
            // send the message
            _chatManager.SendRealMessage(_gagMessages.ProvideInfoEncodedMessage(playerPayload, targetPlayer, _config.InDomMode,
                _config.DirectChatGarbler, _config.GarbleLevel, _config.selectedGagTypes, _config.selectedGagPadlocks,
                _config.selectedGagPadlocksAssigner, _config.selectedGagPadLockTimer, relationshipVar));
        }
    }

    private void SendInfoToPlayer2() {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        // format the player name from "firstname lastname homeworld" to "firstname lastname@homeworld"
        int lastSpaceIndex = _config.SendInfoName.LastIndexOf(' ');
        if (lastSpaceIndex >= 0) { // if we can do this, then do it.
            string targetPlayer = _config.SendInfoName.Remove(lastSpaceIndex, 1).Insert(lastSpaceIndex, "@");
            // get your relationship to that player, if any. Search for their name in the whitelist.
            string relationshipVar = "None";
            _config.Whitelist.ForEach(delegate(WhitelistCharData entry) {
                if (_config.SendInfoName.Contains(entry.name)) {
                    // set the relationship
                    relationshipVar = entry.relationshipStatus;
                }
            });
            // send the message
            _chatManager.SendRealMessage(_gagMessages.ProvideInfoEncodedMessage2(playerPayload, targetPlayer, _config.InDomMode,
                _config.DirectChatGarbler, _config.GarbleLevel, _config.selectedGagTypes, _config.selectedGagPadlocks,
                _config.selectedGagPadlocksAssigner, _config.selectedGagPadLockTimer, relationshipVar));
        }
    }



    private void AcceptMistressRequestFromPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        
        // you have accepted the request, meaning by the time you get this, the PendingRelationFromPlayer is the relation they want
        selectedPlayer.relationshipStatus = selectedPlayer.PendingRelationRequestFromPlayer; // set the relationship status
        selectedPlayer.SetTimeOfCommitment(); // set the commitment time!
        // let them know you accept the request
        _chatManager.SendRealMessage(_gagMessages.AcceptMistressEncodedMessage(playerPayload, targetPlayer));
    }

    private void AcceptPetRequestFromPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        selectedPlayer.relationshipStatus = selectedPlayer.PendingRelationRequestFromPlayer; // set the relationship status
        selectedPlayer.SetTimeOfCommitment(); // set the commitment time!
        _chatManager.SendRealMessage(_gagMessages.AcceptPetEncodedMessage(playerPayload, targetPlayer));
    }

    private void AcceptSlaveRequestFromPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // get player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        selectedPlayer.relationshipStatus = selectedPlayer.PendingRelationRequestFromPlayer; // set the relationship status
        selectedPlayer.SetTimeOfCommitment(); // set the commitment time!
        _chatManager.SendRealMessage(_gagMessages.AcceptSlaveEncodedMessage(playerPayload, targetPlayer));
    }




    private void Checkbox(string label, string tooltip, bool current, Action<bool> setter) {
        using var id  = ImRaii.PushId(label);
        var       tmp = current;
        if (ImGui.Checkbox(string.Empty, ref tmp) && tmp != current) {
            setter(tmp);
            _config.Save();
        }

        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker(label, tooltip);
    }
}
#pragma warning restore IDE1006


