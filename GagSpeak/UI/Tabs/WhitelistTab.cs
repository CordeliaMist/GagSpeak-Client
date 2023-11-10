using System;
using ImGuiNET;
using OtterGui.Widgets;
using Dalamud.Game.ClientState.Objects.Enums;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Plugin.Services;
using OtterGui;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Utility;
using GagSpeak.Data;
using GagSpeak.UI.GagListings;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using GagSpeak.Chat;
using Lumina.Excel.GeneratedSheets;
using System.Runtime.CompilerServices;
using System.Dynamic;

// practicing modular design
namespace GagSpeak.UI.Tabs.WhitelistTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class WhitelistTab : ITab
{
    // When going back through this, be sure to try and reference anything possible to include from the glamourer convention, since it is more modular.
    private readonly GagMessages _gagMessages; // snag the whitelistchardata from the main plugin for obvious reasons
    private readonly ChatManager _chatManager; // snag the chatmanager from the main plugin for obvious reasons
    private readonly GagSpeakConfig _config; // snag the conmfig from the main plugin for obvious reasons
    private readonly IClientState _clientState; // snag the clientstate from the main plugin for obvious reasons
    private readonly GagListingsDrawer _gagListingsDrawer; // snag the gaglistingsdrawer from the main plugin for obvious reasons
    private int _currentWhitelistItem; // store a value so we know which item is selected in whitelist
    public ReadOnlySpan<byte> Label => "Whitelist"u8; // set label for the whitelist tab
    private string _tempPassword; // password temp stored during typing
    private string _storedPassword; // password stored to buffer

    // store real life time since button pressed
    private bool interactionButtonsDisabled = false; // enable this once we press a button
    private float interactionButtonsDisabledTime = 0f; // store the time since we pressed the button

    // Constructor for the whitelist tab
    public WhitelistTab(GagSpeakConfig config, IClientState clientState, GagListingsDrawer gagListingsDrawer, ChatManager chatManager) {
        // Set the readonlys
        _config = config;
        _clientState = clientState;
        _gagListingsDrawer = gagListingsDrawer;
        _gagMessages = new GagMessages();
        _chatManager = chatManager;
        _tempPassword = "";
        _storedPassword = "";
    }
    
    // Helper function to clean senders name off the list of clientstate objects
    public static string CleanSenderName(string senderName) {
        string[] senderStrings = SplitCamelCase(RemoveSpecialSymbols(senderName)).Split(" ");
        string playerSender = senderStrings.Length == 1 ? senderStrings[0] : senderStrings.Length == 2 ?
            (senderStrings[0] + " " + senderStrings[1]) :
            (senderStrings[0] + " " + senderStrings[2]);
        return playerSender;
    }

    // Helper functions for parsing payloads and clientstruct information
    public static string SplitCamelCase(string input) {
        return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }
    public static string RemoveSpecialSymbols(string value) {
        Regex rgx = new Regex(@"[^a-zA-Z:/._\ -]");
        return rgx.Replace(value, "");
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
        // If the whitelist is empty, then we should set the whitelist to "None"
        if (whitelist.Count == 0) {
            whitelist.Add(new WhitelistCharData("None", "None"));
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
            ImGui.TableSetupColumn("Relation Manager", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 320);

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
                ImGui.TableSetupColumn("Statistic", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Become Their Mistressm").X);
                ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextRow(); ImGui.TableHeader("Relations Manager");

                // Next Row (Relations towards you)
                ImGui.TableNextRow(); ImGuiUtil.DrawFrameColumn("Relation towards You:"); ImGui.TableNextColumn();
                var width2 = new Vector2(ImGui.GetContentRegionAvail().X,0);
                ImGui.Text("Player's Relation");

                ImGuiUtil.DrawFrameColumn("Commitment Length: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
                ImGui.Text($"{whitelist[_currentWhitelistItem].commitmentDuration} TIME UNITS");

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

            // here put if they want to request relation removal
            var width = new Vector2(ImGui.GetContentRegionAvail().X,0);
            if (ImGui.Button("Request To Remove Relation", width)) {
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            } 

            ImGui.NewLine(); // Now add two buttons, one for adding the player, another for removing
            var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X / 2, 0);
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
                    string targetName = CleanSenderName(_clientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                    // And now, if the player is not already in our whitelist, we will add them. Otherwise just do nothing.
                    if (!_config.Whitelist.Any(item => item.name == targetName)) {
                        _config.Whitelist.Add(new WhitelistCharData(targetName,"None")); // Add the player to the whitelist
                        _config.Save();
                    }
                }
            }
            // If the player is not targetted or not close enough, end the disabled button
            if (!playerTargetted || !playerCloseEnough)
                ImGui.EndDisabled();
            
            // Also give people the option to remove someone from the whitelist.
            ImGui.SameLine();
            if (ImGui.Button("Remove Selected Player", buttonWidth)) {
                _config.Whitelist.Remove(_config.Whitelist[_currentWhitelistItem]);
                _config.Save();
            }
            ImGui.NewLine();
        } // end our main table

        // create a collapsing header for this.
        if(!ImGui.CollapsingHeader("PLAYER's Interaction Options"))
            return;

        // see if our button disabler is true
        if(interactionButtonsDisabled) {
            ImGui.BeginDisabled();
        }
        

        // create a new table for this section
        using (var InfoTable = ImRaii.Table("InfoTable", 1)) {
            // for our first row, display the DD for layer, DD gag type, and apply gag to player buttons
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            // create a combo for the layer, with options 1, 2, 3. Store selection to variable layer
            int layer = 1;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
            ImGui.Combo("##Layer", ref layer, new string[] { "Layer 1", "Layer 2", "Layer 3" }, 3);
            ImGui.SameLine();
            // create a dropdown for the gag type,
            int width = (int)(ImGui.GetContentRegionAvail().X / 2.5);
            _gagListingsDrawer.DrawGagTypeItemCombo((layer-1)+10, _config.Whitelist[_currentWhitelistItem].selectedGagTypes[layer-1], layer-1, false, width);
            ImGui.SameLine();
            // Create the button for the first row, third column
            if (ImGui.Button("Apply Gag To Player")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                ApplyGagOnPlayer(layer, _config.Whitelist[_currentWhitelistItem].selectedGagTypes[layer-1], _config.Whitelist[_currentWhitelistItem]);
            }

            // for our second row, gag lock options and buttons
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            // set up a temp password storage field here.
            _gagListingsDrawer.DrawGagLockItemCombo((layer-1)+10, _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1], layer-1, false, width);
            ImGui.SameLine();
            var password = _tempPassword ?? _storedPassword; // temp storage to hold until we de-select the text input
            // if _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == CombinationPadlock, draw a text inputwith hint field for the password with a ref length of 4.
            if (_config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == GagPadlocks.CombinationPadlock) {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
                if (ImGui.InputText("CombinationPassword##CombinationPassword", ref password, 4, ImGuiInputTextFlags.None))
                    _tempPassword = password;
                if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                    _storedPassword = password;
                    _tempPassword = null;
                }
            }
            // if _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == TimerPasswordPadlock || MistressTimerPadlock, draw a text input with hint field labeled time set, with ref length of 3.
            if (_config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == GagPadlocks.TimerPasswordPadlock ||
                _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == GagPadlocks.MistressTimerPadlock) {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
                if (ImGui.InputText("TimerPassword##TimerPassword", ref password, 3, ImGuiInputTextFlags.None))
                    _tempPassword = password;
                if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                    _storedPassword = password;
                    _tempPassword = null;
                }
            }
            // if _config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == PasswordPadlock, draw a text input with hint field labeled assigner, with ref length of 30.
            if (_config.Whitelist[_currentWhitelistItem].selectedGagPadlocks[layer-1] == GagPadlocks.PasswordPadlock) {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X / 5);
                if (ImGui.InputText("Password##Password", ref password, 30, ImGuiInputTextFlags.None))
                    _tempPassword = password;
                if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                    _storedPassword = password;
                    _tempPassword = null;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Lock Selected Gag")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                LockGagOnPlayer(layer, _config.selectedGagPadlocks[layer-1].ToString(), _config.Whitelist[_currentWhitelistItem]);
            }
            ImGui.SameLine();
            if (ImGui.Button("Unlock Selected Gag")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                UnlockGagOnPlayer(layer, _config.Whitelist[_currentWhitelistItem]);
            }

            ImGui.TableNextRow(); ImGui.TableNextColumn();
            if (ImGui.Button("Remove This Gag")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                RemoveGagFromPlayer(layer, _config.Whitelist[_currentWhitelistItem]);
            }
            ImGui.SameLine();
            if (ImGui.Button("Remove All Gags")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                RemoveAllGagsFromPlayer(_config.Whitelist[_currentWhitelistItem]);
            }
            
            // add a filler row
            ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.NewLine();
            // Create the button for the sixth row, first column
            if (ImGui.Button("Lock Live Chat Garbler ON")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                var selectedWhitelistItem = _config.Whitelist[_currentWhitelistItem];
                // modify property.
                selectedWhitelistItem.lockedLiveChatGarbler = true;
                // update the whitelist
                _config.Whitelist[_currentWhitelistItem] = selectedWhitelistItem;
                // some message to force the gag onto them
            }
            ImGui.SameLine();
            if (ImGui.Button("Request Player Info")) {
                // execute the generation of the apply gag layer string
                interactionButtonsDisabled = true; // Disable buttons for 5 seconds
                interactionButtonsDisabledTime = (float)ImGui.GetTime();
                // send a message to the player requesting their current info
            }
        } // end our info table

        // Use ImGui.EndDisabled() to end the disabled state
        if (interactionButtonsDisabled) {
            ImGui.EndDisabled();
        }

        // Check if 30 seconds have passed, then re-enable buttons
        if (interactionButtonsDisabled && (ImGui.GetTime() - interactionButtonsDisabledTime) > 5.0f) {
            interactionButtonsDisabled = false;
        }

    } // end our draw whitelist function

    // Additional methods for applying, locking, unlocking, removing gags
    private void ApplyGagOnPlayer(int layer, string gagType, WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload; // your player payload
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); //  name and homeworld
        // Ensure a player is selected as a valid whitelist index
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // Assuming GagMessages is a class instance, replace it with the actual instance
        _chatManager.SendRealMessage(_gagMessages.GagApplyMessage(playerPayload, selectedPlayer.name, selectedPlayer.selectedGagTypes[layer], layer.ToString()));
        // Update the selected player's data
        selectedPlayer.selectedGagTypes[layer - 1] = gagType; // note that this wont always be accurate, and is why request info exists.
    }

    // we need a function for if the password is not given
    private void LockGagOnPlayer(int layer, string lockType, WhitelistCharData selectedPlayer) { LockGagOnPlayer(layer, lockType, selectedPlayer, ""); }
    private void LockGagOnPlayer(int layer, string lockType, WhitelistCharData selectedPlayer, string password) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the chat message
        _chatManager.SendRealMessage(_gagMessages.GagLockMessage(playerPayload, selectedPlayer.name, lockType, layer.ToString(), password));

        // Update the selected player's data
        if(Enum.TryParse(lockType, out GagPadlocks parsedLockType)) // update padlock
            selectedPlayer.selectedGagPadlocks[layer - 1] = parsedLockType;
        
        if(password != "") // logic for applying password
            selectedPlayer.selectedGagPadlocksPassword[layer - 1] = password;

        if ( (lockType == "MistressPadlock" || lockType == "MistressTimerPadlock") // logic for applying a mistress padlock
            && selectedPlayer.relationshipStatus == "Pet" || selectedPlayer.relationshipStatus == "Slave") {
            selectedPlayer.selectedGagPadlocksAssigner[layer - 1] = playerPayload.PlayerName;
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
        _chatManager.SendRealMessage(_gagMessages.GagLockMessage(playerPayload, selectedPlayer.name, layer.ToString(), password));

        // first handle logic for mistress padlocks
        if ( (selectedPlayer.selectedGagPadlocks[layer] == GagPadlocks.MistressPadlock || selectedPlayer.selectedGagPadlocks[layer] == GagPadlocks.MistressTimerPadlock) // logic for applying a mistress padlock
           && selectedPlayer.relationshipStatus == "Pet" || selectedPlayer.relationshipStatus == "Slave") {
            // remove it if the player is a pet or slave and your name matches the assigner
            if (selectedPlayer.selectedGagPadlocksAssigner[layer - 1] == playerPayload.PlayerName) {
                selectedPlayer.selectedGagPadlocksAssigner[layer - 1] = "";
                selectedPlayer.selectedGagPadlocksPassword[layer - 1] = "";
                selectedPlayer.selectedGagTypes[layer - 1] = "None";
                return;
            }
        }
        // next handle logic for other padlocks.
        else if (password != "" && selectedPlayer.selectedGagPadlocksPassword[layer - 1] == password) {
            selectedPlayer.selectedGagPadlocksPassword[layer - 1] = "";
            selectedPlayer.selectedGagTypes[layer - 1] = "None";
            return;
        } 
        // its not a password lock, so just remove it
        else {
            selectedPlayer.selectedGagPadlocksPassword[layer - 1] = "";
            selectedPlayer.selectedGagTypes[layer - 1] = "None";
            return;
        }
    }

    private void RemoveGagFromPlayer(int layer, WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;
        // send the message
        _chatManager.SendRealMessage(_gagMessages.GagRemoveMessage(playerPayload, selectedPlayer.name, layer.ToString()));
        // Update the selected player's data
        selectedPlayer.selectedGagTypes[layer - 1] = "None";
        selectedPlayer.selectedGagPadlocks[layer - 1] = GagPadlocks.None;
        selectedPlayer.selectedGagPadlocksPassword[layer - 1] = "";
        selectedPlayer.selectedGagPadlocksAssigner[layer - 1] = "";
    }

    private void RemoveAllGagsFromPlayer(WhitelistCharData selectedPlayer) {
        PlayerPayload playerPayload;
        playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        if (_currentWhitelistItem < 0 || _currentWhitelistItem >= _config.Whitelist.Count)
            return;

        _chatManager.SendRealMessage(_gagMessages.GagRemoveAllMessage(playerPayload, selectedPlayer.name));

        // Update the selected player's data
        for (int i = 0; i < selectedPlayer.selectedGagTypes.Count; i++) {
            selectedPlayer.selectedGagTypes[i] = "None";
            selectedPlayer.selectedGagPadlocks[i] = GagPadlocks.None;
            selectedPlayer.selectedGagPadlocksPassword[i] = "";
            selectedPlayer.selectedGagPadlocksAssigner[i] = "";
        }
    }
}
#pragma warning restore IDE1006