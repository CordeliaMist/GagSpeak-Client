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

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPlayerPermissions {
    private int _selectedDynamicIdx = 0;
    public void DrawOverview() {
        ///////////////// DRAW OUT THE BASIC INFORMATION FOR THE PLAYER HERE /////////////////////
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos - 5*ImGuiHelpers.GlobalScale);
        var width = ImGui.GetContentRegionAvail().X * 0.7f;
        ImGui.Columns(2, $"##Whitelist{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}Header", false);
        ImGui.SetColumnWidth(0, width);
        ImGui.PushFont(_fontService.UidFont);
        ImGui.Text($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}");
        ImGui.PopFont();
        ImGui.NextColumn();
        ImGuiUtil.RightAlign($"({_characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld})");
        ImGui.Columns(1);
        
        string[] roleLeanNames = Enum.GetValues<RoleLean>().Select(role => role.ToString()).ToArray(); // for inside the table
        // Create a table for the relations manager
        using (var tableGeneral = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!tableGeneral) { return; }
            // Create the headers for the table
            ImGui.TableSetupColumn("Relations Tag", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X * 0.55f);
            ImGui.TableSetupColumn("Relation Information", ImGuiTableColumnFlags.WidthStretch);            
            // underneath this, we will display the relationship dynamic, we will want to display "No Defined Dynamic" and lean a blank line under it, if no dynamic is defind
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"Your Dynamic Lean to {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}: ");
            ImGui.TableNextColumn();
            ImGui.Text($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem}");
            // Draw out the line that displays the players defined relationship towards you
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Dynamic lean to you: ");
            ImGui.TableNextColumn();
            ImGui.Text($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou}");
            // display commitment length
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn("Commitment Length: ");
            ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_characterHandler.activeListIdx].GetCommitmentDuration()}");
            
            ImGui.TableNextColumn(); // Next Row (Request Dynamic)
            // Get a RoleLean combo listing for requesting relations
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Select Lean:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.Combo("##DynamicLeanCombo", ref _selectedDynamicIdx, roleLeanNames, roleLeanNames.Length));
            ImGui.TableNextColumn();
            
            // draw a button spanning the 1st and 2nd columns of this row
            if (ImGui.Button("Send to Player##ReqDynamic", new Vector2(ImGui.GetContentRegionAvail().X * 0.7f, 0))) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their mistress");
                RoleLean selectedRole = (RoleLean)_selectedDynamicIdx;
                RequestDynamicToPlayer(selectedRole);
                _interactOrPermButtonEvent.Invoke();
            }

            ImGui.PopStyleVar();
        }
        // add the popups for relation requests
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X / 2, 25.0f * ImGuiHelpers.GlobalScale );

        if (_characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer != RoleLean.None) { 
            RoleLean pendingDynamic = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer;
            // draw the accept and decline buttons for the pending relation request
            var relationText = pendingDynamic.ToString()?.Split(' ')[0];
            if (ImGui.Button($"Accept {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]} as your {relationText}",
                new Vector2(ImGui.GetContentRegionAvail().X/2, 25)))
            {
                AcceptRequestForDynamicButton();
                // set the relation request to established
                _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer = RoleLean.None;
                GagSpeak.Log.Debug($"[Whitelist]: Accepting incoming relation request from "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}");
            }
            ImGui.SameLine();
            if (ImGui.Button($"Decline {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s Request",
                new Vector2(ImGui.GetContentRegionAvail().X, 25)))
            {
                DeclineRequestForDynamicButton();
                // set the relation request to none
                _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer = RoleLean.None;
                GagSpeak.Log.Debug($"[Whitelist]: Declining {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s relation request");
            }
        }
        // now make two buttons that span the width of the content region, one for showing profile, the other for removing relation
    }

#region DynamicRequestPopUp
    /// <summary>  Applies logic for accepting a requested status from a player. </summary>
    public void AcceptRequestForDynamicButton() {
        // get the player payload    
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        _characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx);
        // print to chat that you sent the request
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name
                            + "@"
                            + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // get dynamic
        RoleLean requestedDynamic = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer;
        // send request based on the dynamic
        if(requestedDynamic == RoleLean.Owner || requestedDynamic == RoleLean.Master || requestedDynamic == RoleLean.Mistress) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // set the relationship status the player has towards you "They are your Mistress" here, because once you hit accept, both sides agree
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer;
            if(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem != RoleLean.None) {
                _characterHandler.whitelistChars[_characterHandler.activeListIdx].Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestDominantStatus(playerPayload, targetPlayer, requestedDynamic));
        } else if(requestedDynamic == RoleLean.Submissive || requestedDynamic == RoleLean.Pet || requestedDynamic == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // set the relationship status the player has towards you "They are your Pet" here, because once you hit accept, both sides agree
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer;
            if(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem != RoleLean.None) {
                _characterHandler.whitelistChars[_characterHandler.activeListIdx].Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestSubmissiveStatus(playerPayload, targetPlayer, requestedDynamic));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // set the relationship status the player has towards you "They are your Absolute-Slave" here, because once you hit accept, both sides agree
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer;
            if(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem != RoleLean.None) {
                _characterHandler.whitelistChars[_characterHandler.activeListIdx].Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer, requestedDynamic));
        }
    }

    /// <summary>  Controls logic for what to do once the the decline dynamic relation button is pressed in the whitelist tab. </summary>
    public void DeclineRequestForDynamicButton() {
        // get the player payload    
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // make sure the current whitelist item is valid
        if (_characterHandler.activeListIdx < 0 || _characterHandler.activeListIdx >= _characterHandler.whitelistChars.Count) { return; }
        // print to chat that you sent the request
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name 
                            + "@"
                            + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // get dynamic
        RoleLean requestedDynamic = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer;
        // execute action based on dynamic type
        if(requestedDynamic == RoleLean.Owner || requestedDynamic == RoleLean.Master || requestedDynamic == RoleLean.Mistress) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = RoleLean.None;
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer = RoleLean.None;
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestDominantStatus(playerPayload, targetPlayer));
        } else if(requestedDynamic == RoleLean.Pet || requestedDynamic == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = RoleLean.None;
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer = RoleLean.None;
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestSubmissiveStatus(playerPayload, targetPlayer));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = RoleLean.None;
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer = RoleLean.None;
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer));
        }
    }
#endregion DynamicRequestPopUp


#region ButtonHelpers
    /// <summary>  Controls logic for what to do once the the request mistress relation button is pressed in the whitelist tab. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public void RequestDynamicToPlayer(RoleLean dynamicRole) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.activeListIdx < 0 || _characterHandler.activeListIdx >= _characterHandler.whitelistChars.Count) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        if(dynamicRole == RoleLean.Owner || dynamicRole == RoleLean.Master || dynamicRole == RoleLean.Mistress) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}, to see if they would like you to become their {dynamicRole}.").AddItalicsOff().BuiltString);
            //update information and send message
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromYou = dynamicRole;
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestDominantStatus(playerPayload, targetPlayer, dynamicRole));
        } 
        // if the dynamic role is a submissive role
        else if(dynamicRole == RoleLean.Pet || dynamicRole == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}, to see if they would like you to become their {dynamicRole}.").AddItalicsOff().BuiltString);
            //update information and send message
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromYou = dynamicRole;
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestSubmissiveStatus(playerPayload, targetPlayer, dynamicRole));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}, to see if they would like you to become their Absolute-Slave.").AddItalicsOff().BuiltString);
            //update information and send message
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromYou = RoleLean.AbsoluteSlave;
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer));
        }
    }

    /// <summary>  Controls logic for what to do once the the remove relation button is pressed in the whitelist tab. </summary>
    public void RequestRelationRemovalToPlayer() {
        // get player payload
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (_characterHandler.activeListIdx < 0 || _characterHandler.activeListIdx >= _characterHandler.whitelistChars.Count) { return; }
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing Relation Status "+
            $"with {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}.").AddItalicsOff().BuiltString);
        //update information and send message
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem = RoleLean.None;
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._theirStatusToYou = RoleLean.None;
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromYou = RoleLean.None;
        _characterHandler.whitelistChars[_characterHandler.activeListIdx]._pendingRelationRequestFromPlayer = RoleLean.None;
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        _chatManager.SendRealMessage(_messageEncoder.EncodeSendRelationRemovalMessage(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}