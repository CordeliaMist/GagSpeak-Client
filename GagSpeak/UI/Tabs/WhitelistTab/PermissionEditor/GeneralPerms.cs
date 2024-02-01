using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Utility;
using Dalamud.Plugin.Services;
using OtterGui;
using GagSpeak.CharacterData;
using System.Linq;
using GagSpeak.UI.UserProfile;
using Dalamud.Interface.Utility;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPermissionEditor {
    private int _selectedDynamicIdx = 0;

#region GeneralPerms
    public void DrawGeneralPerms(int currentWhitelistItem) {
        // Create a table for the relations manager
        using (var tableGeneral = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!tableGeneral) { return; }
            // Create the headers for the table
            ImGui.TableSetupColumn("Statistic", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Become Their Mistress").X);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            // draw the first row
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            // Create a bool for if the player is targetted (more detail on this later after experimentation)
            bool playerTargetted = _clientState.LocalPlayer != null && _clientState.LocalPlayer.TargetObject != null;
            bool playerCloseEnough = playerTargetted && Vector3.Distance( _clientState.LocalPlayer?.Position ?? default, _clientState.LocalPlayer?.TargetObject?.Position ?? default) < 3;
            
            // Draw out the line that displays the players defined relationship towards you
            ImGuiUtil.DrawFrameColumn($"You are {_config.whitelist[currentWhitelistItem]._name.Split(' ')[0]}'s: ");
            ImGui.TableNextColumn();
            var width2 = new Vector2(ImGui.GetContentRegionAvail().X, 0);
            ImGui.Text($"{_config.whitelist[currentWhitelistItem]._yourStatusToThem}");
            
            // Draw out the line that displays your defined relationship towards that player
            ImGuiUtil.DrawFrameColumn($"{_config.whitelist[currentWhitelistItem]._name.Split(' ')[0]} is your: ");
            ImGui.TableNextColumn();
            ImGui.Text($"{_config.whitelist[currentWhitelistItem]._theirStatusToYou}");

            ImGuiUtil.DrawFrameColumn("Commitment Length: ");
            ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.whitelist[currentWhitelistItem].GetCommitmentDuration()}");
            
            // Get a RoleLean combo listing
            string[] roleLeanNames = Enum.GetValues<RoleLean>().Select(role => role.ToString()).ToArray();
            if (ImGui.Combo("Dynamic Lean##DynamicLeanCombo", ref _selectedDynamicIdx, roleLeanNames, roleLeanNames.Length));
            ImGui.TableNextColumn();
            // draw a button spanning the 1st and 2nd columns of this row
            if (ImGui.Button("Request Relation##ReqDynamic", width2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their mistress");
                RoleLean selectedRole = (RoleLean)_selectedDynamicIdx;
                RequestDynamicToPlayer(currentWhitelistItem, selectedRole);
                _interactOrPermButtonEvent.Invoke();
            }
            
            // Display "Allowed / Not Allowed" based on _grantExtendedLockTimes
            ImGuiUtil.DrawFrameColumn("Allow Extended Times: "); ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._grantExtendedLockTimes ? "Allowed" : "Not Allowed");

            // Display "Active / Inactive" based on _directChatGarblerActive
            ImGuiUtil.DrawFrameColumn("Live Chat Garbler: "); ImGui.TableNextColumn();
            ImGui.Text(_config.whitelist[currentWhitelistItem]._directChatGarblerActive ? "Active" : "Inactive");

            // now we need to update our spacing
            var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
            ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
            var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X/2, 22.0f * ImGuiHelpers.GlobalScale );
            // create a button for popping out the current players profile. Update the currently selected person for the profile list
            _userProfileWindow._profileIndexOfUserSelected = currentWhitelistItem;
            // add a button to display it
            if (ImGui.Button("Show Profile", buttonWidth)) {
                // Get the currently selected user
                var selectedUser = _config.whitelist[currentWhitelistItem];
                // Check if the UserProfileWindow is already open
                _userProfileWindow.Toggle();
            }

            // draw the relationship removal
            ImGui.SameLine();
            if(_config.whitelist[currentWhitelistItem]._yourStatusToThem == RoleLean.None) {
                ImGui.BeginDisabled();
                if (ImGui.Button("Remove Relation##RemoveOne", buttonWidth)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                    RequestRelationRemovalToPlayer(currentWhitelistItem);
                    // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
                }
                ImGui.EndDisabled();
            } else {
                if (ImGui.Button("Remove Relation##RemoveTwo", buttonWidth)) {
                    GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                    RequestRelationRemovalToPlayer(currentWhitelistItem);
                    // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
                }
            } 

            // Message to display based on target proximity
            string targetedPlayerText = "Add Targeted Player"; // Displays if no target
            if (!playerTargetted) {
                targetedPlayerText = "No Player Target!"; // If not tagetting a player, display "No Target"
                ImGui.BeginDisabled(); // Disable the button since no target to add
            } else if (playerTargetted && !playerCloseEnough) {
                targetedPlayerText = "Player Too Far!"; // If target is too far, display "Too Far"
                ImGui.BeginDisabled(); // Disable the button since target is too far
            }

            // Create a button for adding the targetted player to the _config.whitelist, assuming they are within proxy.
            if (ImGui.Button(targetedPlayerText, buttonWidth)) {
                // prevent possible null in _clientState.LocalPlayer.TargetObject
                if (_clientState.LocalPlayer != null &&_clientState.LocalPlayer.TargetObject != null) {
                    if (_clientState.LocalPlayer.TargetObject.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) { // if the player is targetting another player
                        GagSpeak.Log.Debug($"[Whitelist]: Targeted Player: {_clientState.LocalPlayer.TargetObject.Name.TextValue}");
                        string targetName = UIHelpers.CleanSenderName(_clientState.LocalPlayer.TargetObject.Name.TextValue); // Clean the sender name
                        // if the object kind of the target is a player, then get the character parse of that player
                        var targetCharacter = (PlayerCharacter)_clientState.LocalPlayer.TargetObject;
                        // now we can get the name and world from them
                        var world = targetCharacter.HomeWorld.Id;
                        var worldName = _dataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow((uint)world)?.Name?.ToString() ?? "Unknown";
                        GagSpeak.Log.Debug($"[Whitelist]: Targeted Player: {targetName} from {world}, {worldName}");

                        // And now, if the player is not already in our _config.whitelist, we will add them. Otherwise just do nothing.
                        if (!_config.whitelist.Any(item => item._name == targetName)) {
                            GagSpeak.Log.Debug($"[Whitelist]: Adding targeted player to _config.whitelist {_clientState.LocalPlayer.TargetObject})");
                            if(_config.whitelist.Count == 1 && _config.whitelist[0]._name == "None") { // If our _config.whitelist just shows none, replace it with first addition.
                                WhitelistHelpers.ReplaceWhitelistItem(0, targetName, worldName, _config);
                                _config.Save();
                            } else {
                                WhitelistHelpers.AddNewWhitelistItem(targetName, worldName, "None", _config); // Add the player to the _config.whitelist
                                _config.Save();
                            }
                        }
                    }
                }
            }
            // If the player is not targetted or not close enough, end the disabled button
            if (!playerTargetted || !playerCloseEnough) { ImGui.EndDisabled(); }
            
            // Also give people the option to remove someone from the _config.whitelist.
            ImGui.SameLine();
            if (ImGui.Button("Remove Player", buttonWidth)) {
                if (_config.whitelist.Count == 1) {
                    WhitelistHelpers.ReplaceWhitelistItem(0, "None","None", _config);
                } else {
                    WhitelistHelpers.RemoveWhitelistItemAtIndex(currentWhitelistItem, _config);
                }
                currentWhitelistItem = 0;
                _config.Save();
            }
        }
    }
#endregion GeneralPerms
#region ButtonHelpers
    /// <summary>  Controls logic for what to do once the the request mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public void RequestDynamicToPlayer(int currentWhitelistItem, RoleLean dynamicRole) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= _config.whitelist.Count) { return; }
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        // print to chat that you sent the request
        if(dynamicRole == RoleLean.Owner || dynamicRole == RoleLean.Master || dynamicRole == RoleLean.Mistress) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_config.whitelist[currentWhitelistItem]._name}, to see if they would like you to become their {dynamicRole}.").AddItalicsOff().BuiltString);
            //update information and send message
            _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromYou = dynamicRole;
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestDominantStatus(playerPayload, targetPlayer, dynamicRole));
        } 
        // if the dynamic role is a submissive role
        else if(dynamicRole == RoleLean.Pet || dynamicRole == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_config.whitelist[currentWhitelistItem]._name}, to see if they would like you to become their {dynamicRole}.").AddItalicsOff().BuiltString);
            //update information and send message
            _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromYou = dynamicRole;
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestSubmissiveStatus(playerPayload, targetPlayer, dynamicRole));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_config.whitelist[currentWhitelistItem]._name}, to see if they would like you to become their Absolute-Slave.").AddItalicsOff().BuiltString);
            //update information and send message
            _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromYou = RoleLean.AbsoluteSlave;
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer));
        }
    }

    /// <summary>  Controls logic for what to do once the the remove relation button is pressed in the whitelist tab. </summary>
    public void RequestRelationRemovalToPlayer(int currentWhitelistItem) {
        // get player payload
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (currentWhitelistItem < 0 || currentWhitelistItem >= _config.whitelist.Count) { return; }
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing Relation Status "+
            $"with {_config.whitelist[currentWhitelistItem]._name}.").AddItalicsOff().BuiltString);
        //update information and send message
        _config.whitelist[currentWhitelistItem]._yourStatusToThem = RoleLean.None;
        _config.whitelist[currentWhitelistItem]._theirStatusToYou = RoleLean.None;
        _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromYou = RoleLean.None;
        _config.whitelist[currentWhitelistItem]._pendingRelationRequestFromPlayer = RoleLean.None;
        string targetPlayer = _config.whitelist[currentWhitelistItem]._name + "@" + _config.whitelist[currentWhitelistItem]._homeworld;
        _chatManager.SendRealMessage(_messageEncoder.EncodeSendRelationRemovalMessage(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}