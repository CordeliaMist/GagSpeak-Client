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

namespace GagSpeak.UI.Tabs.WhitelistTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class WhitelistTab : ITab
{
    // When going back through this, be sure to try and reference anything possible to include from the glamourer convention, since it is more modular.
    private readonly MessageEncoder _gagMessages; // snag the whitelistchardata from the main plugin for obvious reasons
    private readonly ChatManager _chatManager; // snag the chatmanager from the main plugin for obvious reasons
    private readonly GagSpeakConfig _config; // snag the conmfig from the main plugin for obvious reasons
    private readonly IClientState _clientState; // snag the clientstate from the main plugin for obvious reasons
    private readonly IDataManager _dataManager; // for parsing objects
    private readonly GagListingsDrawer _gagListingsDrawer; // snag the gaglistingsdrawer from the main plugin for obvious reasons
    public ReadOnlySpan<byte> Label => "Whitelist"u8; // set label for the whitelist tab
    private bool emptyList = false; // if the whitelist is empty, disable lower section

    // temp variables for the whitelist tab
    private int _currentWhitelistItem; // store a value so we know which item is selected in whitelist
    private string _tempPassword; // password temp stored during typing
    private string _storedPassword; // password stored to buffer
    private int _layer; // layer of the gag
    private string _gagLabel; // current selection on gag type DD
    private string _lockLabel; // current selection on gag lock DD
    private readonly GagTypeFilterCombo[] _gagTypeFilterCombo; // create an array of item combos
    private readonly GagLockFilterCombo[] _gagLockFilterCombo; // create an array of item combos

    // store time information & control locks
    private bool enableInteractions = false; // determines if we can interact with with whitelist buttons or not (for safety to prevent accidental tells)
    private bool interactionButtonPressed = false; // determines if we have pressed a button that communicates or not
    private DateTimeOffset ? startInteractLockTime; // time to lock interactions once a button is pressed to prevent spamming of tells

    // Constructor for the whitelist tab
    public WhitelistTab(GagSpeakConfig config, IClientState clientState, GagListingsDrawer gagListingsDrawer, ChatManager chatManager, IDataManager dataManager) {
        // Set the readonlys
        _config = config;
        _clientState = clientState;
        _dataManager = dataManager;
        _gagListingsDrawer = gagListingsDrawer;
        _gagMessages = new MessageEncoder();
        _chatManager = chatManager;
        _tempPassword = "";
        _storedPassword = "";
        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;

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
                

                //////////////////////////////////////////////////////////////////////////////////////////////////////
                //// Here is the start of our relations manager. So calculate our timers and interaction toggles. ////
                interactionButtonPressed = (startInteractLockTime == null || (DateTimeOffset.Now - startInteractLockTime.Value).TotalSeconds > 5);
                // create handlers for interactions & permissions
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                Checkbox("Interactions", "WARNING: Make sure other people on your whitelist have this plugin too! (sends tells to players)\n" +
                "Allows for direct communication. Encoded to look natural, but still look wierd out of context!", enableInteractions, v => enableInteractions = v);
                if (!interactionButtonPressed) {
                    ImGui.TableNextColumn();
                    var remainingTime = 5 - (DateTimeOffset.Now - startInteractLockTime.Value).TotalSeconds;
                    ImGui.Text($"Usage CD: {remainingTime:F1}s");
                }
                ImGui.TableNextRow();


                // if we dont have interactions enabled, or a communication button was pressed and we are on cooldown, disable them!
                if(!enableInteractions || !interactionButtonPressed)
                    ImGui.BeginDisabled();
                
                ImGuiUtil.DrawFrameColumn("Relation towards You:"); ImGui.TableNextColumn();
                var width2 = new Vector2(ImGui.GetContentRegionAvail().X-13, 0);
                ImGui.Text($"{whitelist[_currentWhitelistItem].relationshipStatus}");

                ImGuiUtil.DrawFrameColumn("Commitment Length: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
                ImGui.Text($"{whitelist[_currentWhitelistItem].GetCommitmentDuration()}");

                ImGuiUtil.DrawFrameColumn("Become Their Mistress: "); ImGui.TableNextColumn(); // Next Row (Request To Become Players Mistress)
                if (ImGui.Button("Request Relation", width2)) // send a encoded request to the player to request beooming their mistress
                    GagSpeak.Log.Debug("Sending Request to become their mistress"); // _chatManager.SendRealMessage($"/tell {whitelist[_currentWhitelistItem].name} *{CleanSenderName(_clientState.LocalPlayer.Name.TextValue)} from {_clientState.LocalPlayer.HomeWorld.Id} requests to become your Mistress*");
                
                ImGuiUtil.DrawFrameColumn("Become Their Pet: "); ImGui.TableNextColumn(); // Next Row (Request To Become Players pet)
                if (ImGui.Button("Request Relation", width2)) // send a encoded request to the player to request beooming their pet
                    GagSpeak.Log.Debug("Sending Request to become their pet"); // _chatManager.SendRealMessage($"/tell {whitelist[_currentWhitelistItem].name} *{CleanSenderName(_clientState.LocalPlayer.Name.TextValue)} from {_clientState.LocalPlayer.HomeWorld.Id} requests to become your pet*");
            
                ImGuiUtil.DrawFrameColumn("Become Their Slave: "); ImGui.TableNextColumn(); // Next Row (Request To Become Players slave)
                if (ImGui.Button("Request Relation", width2)) // send a encoded request to the player to request beooming their slave
                    GagSpeak.Log.Debug("Sending Request to become their slave"); // _chatManager.SendRealMessage($"/tell {whitelist[_currentWhitelistItem].name} *{CleanSenderName(_clientState.LocalPlayer.Name.TextValue)} from {_clientState.LocalPlayer.HomeWorld.Id} requests to become your slave*");
            } // end our relations manager table

            style.Pop();
            // here put if they want to request relation removal
            var width = new Vector2(-1, 25.0f * ImGuiHelpers.GlobalScale);
            if (ImGui.Button("Remove Relation To Player", width)) {
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            } 

            if(!enableInteractions || !interactionButtonPressed)
                ImGui.EndDisabled();

            var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X / 2, 25.0f * ImGuiHelpers.GlobalScale );
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
                    
                    
                    GagSpeak.Log.Debug($"Targetted Player: {targetName} from {world}, {worldName}");

                    // And now, if the player is not already in our whitelist, we will add them. Otherwise just do nothing.
                    if (!_config.Whitelist.Any(item => item.name == targetName)) {
                        GagSpeak.Log.Debug($"Adding targetted player to whitelist {_clientState.LocalPlayer.TargetObject})");
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
                    whitelist[0] = new WhitelistCharData("None", "None", "None");
                } else {
                    _config.Whitelist.Remove(_config.Whitelist[_currentWhitelistItem]);
                }
                _config.Save();
            }
        } // end our main table

        // create a collapsing header for this.
        if(!ImGui.CollapsingHeader("PLAYER's Interaction Options"))
            return;


        if(!enableInteractions || !interactionButtonPressed)
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
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }

            // for our second row, gag lock options and buttons
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // set up a temp password storage field here.
            width = (int)(ImGui.GetContentRegionAvail().X / 2.8);
            _gagListingsDrawer.DrawGagLockItemCombo((layer)+10, whitelist[_currentWhitelistItem], ref _lockLabel, layer, false, width, _gagLockFilterCombo[layer]);
            ImGui.SameLine();
            var password = _tempPassword ?? _storedPassword; // temp storage to hold until we de-select the text input
            // if _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer] == CombinationPadlock, draw a text inputwith hint field for the password with a ref length of 4.
            Enum.TryParse(_lockLabel, out GagPadlocks parsedLockType);
            if (parsedLockType == GagPadlocks.CombinationPadlock) {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
                if (ImGui.InputText("##CombinationPassword", ref password, 4, ImGuiInputTextFlags.None))
                    _tempPassword = password;
                if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                    _storedPassword = password;
                    _tempPassword = null;
                }
            }

            // if _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer] == TimerPasswordPadlock || MistressTimerPadlock, draw a text input with hint field labeled time set, with ref length of 3.
            if (parsedLockType  == GagPadlocks.TimerPasswordPadlock || parsedLockType == GagPadlocks.MistressTimerPadlock) {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 4);
                if (ImGui.InputText("##TimerPassword", ref password, 3, ImGuiInputTextFlags.None))
                    _tempPassword = password;
                if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                    _storedPassword = password;
                    _tempPassword = null;
                }
            }
            // if _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer] == PasswordPadlock, draw a text input with hint field labeled assigner, with ref length of 30.
            if (parsedLockType == GagPadlocks.PasswordPadlock) {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 2);
                if (ImGui.InputText("##Password", ref password, 20, ImGuiInputTextFlags.None))
                    _tempPassword = password;
                if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                    _storedPassword = password;
                    _tempPassword = null;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Lock Gag")) {
                LockGagOnPlayer(layer, _lockLabel, _config.Whitelist[_currentWhitelistItem]);
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }
            ImGui.SameLine();
            if (ImGui.Button("Unlock Gag")) {
                UnlockGagOnPlayer(layer, _config.Whitelist[_currentWhitelistItem]);
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }

            ImGui.TableNextRow(); ImGui.TableNextColumn();
            if (ImGui.Button("Remove This Gag")) {
                RemoveGagFromPlayer(layer, _config.Whitelist[_currentWhitelistItem]);
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }
            ImGui.SameLine();
            if (ImGui.Button("Remove All Gags")) {
                RemoveAllGagsFromPlayer(_config.Whitelist[_currentWhitelistItem]);
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }
            
            // add a filler row
            ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.NewLine();
            // Create the button for the sixth row, first column
            if (ImGui.Button("Tooggle Lock Live Chat Garbler")) {
                var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem]; // get the selected whitelist item
                selectedWhitelistItem.lockedLiveChatGarbler = true; // modify property.
                _config.Whitelist[_currentWhitelistItem] = selectedWhitelistItem; // update the whitelist
                // some message to force the gag 
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }
            ImGui.SameLine();
            if (ImGui.Button("Request Player Info")) {
                // send a message to the player requesting their current info
                
                startInteractLockTime = DateTimeOffset.Now; // Record the time when the Submissive button is pressed
            }
        } // end our info table

        // Use ImGui.EndDisabled() to end the disabled state
        if(!enableInteractions || !interactionButtonPressed)
            ImGui.EndDisabled();
    } // end our draw whitelist function

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

    // we need a function for if the password is not given
    private void LockGagOnPlayer(int layer, string lockType, WhitelistCharData selectedPlayer) { LockGagOnPlayer(layer, lockType, selectedPlayer, ""); }
    private void LockGagOnPlayer(int layer, string lockType, WhitelistCharData selectedPlayer, string password) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the chat message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetPlayer, lockType, (layer+1).ToString(), password));

        // Update the selected player's data
        if(Enum.TryParse(lockType, out GagPadlocks parsedLockType)) // update padlock
            selectedPlayer.selectedGagPadlocks[layer] = parsedLockType;
        
        if(password != "") // logic for applying password
            selectedPlayer.selectedGagPadlocksPassword[layer] = password;

        if ( (lockType == "MistressPadlock" || lockType == "MistressTimerPadlock") // logic for applying a mistress padlock
            && selectedPlayer.relationshipStatus == "Pet" || selectedPlayer.relationshipStatus == "Slave") {
            selectedPlayer.selectedGagPadlocksAssigner[layer] = playerPayload.PlayerName;
        }
    }

    // this logic button is by far the most inaccurate, because there is no way to tell if the unlock is sucessful.
    private void UnlockGagOnPlayer(int layer, WhitelistCharData selectedPlayer) { UnlockGagOnPlayer(layer, selectedPlayer, "");} 
    private void UnlockGagOnPlayer(int layer, WhitelistCharData selectedPlayer, string password) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the chat message
        string targetPlayer = selectedPlayer.name + "@" + selectedPlayer.homeworld;
        _chatManager.SendRealMessage(_gagMessages.GagEncodedUnlockMessage(playerPayload, targetPlayer, (layer+1).ToString(), password));

        // first handle logic for mistress padlocks
        if ( (selectedPlayer.selectedGagPadlocks[layer] == GagPadlocks.MistressPadlock || selectedPlayer.selectedGagPadlocks[layer] == GagPadlocks.MistressTimerPadlock) // logic for applying a mistress padlock
           && selectedPlayer.relationshipStatus == "Pet" || selectedPlayer.relationshipStatus == "Slave") {
            // remove it if the player is a pet or slave and your name matches the assigner
            if (selectedPlayer.selectedGagPadlocksAssigner[layer] == playerPayload.PlayerName) {
                selectedPlayer.selectedGagPadlocksAssigner[layer] = "";
                selectedPlayer.selectedGagPadlocksPassword[layer] = "";
                selectedPlayer.selectedGagTypes[layer] = "None";
                return;
            }
        }
        // next handle logic for other padlocks.
        else if (password != "" && selectedPlayer.selectedGagPadlocksPassword[layer] == password) {
            selectedPlayer.selectedGagPadlocksPassword[layer] = "";
            selectedPlayer.selectedGagTypes[layer] = "None";
            return;
        } 
        // its not a password lock, so just remove it
        else {
            selectedPlayer.selectedGagPadlocksPassword[layer] = "";
            selectedPlayer.selectedGagTypes[layer] = "None";
            return;
        }
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


