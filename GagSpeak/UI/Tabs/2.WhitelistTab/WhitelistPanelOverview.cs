using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Utility;
using OtterGui;
using GagSpeak.CharacterData;
using System.Linq;
using Dalamud.Interface.Utility;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using Dalamud.Interface.Components;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {
    private int _selectedDynamicIdx = 0;
    public void DrawOverview(ref bool _interactions) {
        ///////////////// DRAW OUT THE BASIC INFORMATION FOR THE PLAYER HERE /////////////////////
        // draw out info
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // start with center text of the player name and world
        ImGui.PushFont(_fontService.UidFont);
        try {
            ImGuiUtil.Center($"{_tempWhitelistChar._name} ({_tempWhitelistChar._homeworld})");
        } finally {
            ImGui.PopFont();
        }
        // draw out the dynamic tier that you have towards this player
        // manual center logic
        var offset = (ImGui.GetContentRegionAvail().X - 
                    ImGui.CalcTextSize($" You have {_characterHandler.GetDynamicTierClient(_tempWhitelistChar._name)} Dynamic Strength").X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.Text($"You have {_characterHandler.GetDynamicTierClient(_tempWhitelistChar._name)} Dynamic Strength");
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip($"Dynamic Tier Strength is a way to measure how much control you have over {_tempWhitelistChar._name.Split(' ')[0]}'s permissions.\n"+
                             $"It is also a way of making sure you are not overstepping your bounds with them,\n"+
                             $"and a way to make sure you are not being taken advantage of by them.");
        }
        // draw out the commitment length if it is present
        offset = _characterHandler.HasEstablishedCommitment(_tempWhitelistIdx) 
                    ? (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize($"  Committed for: {_tempWhitelistChar.GetCommitmentDuration()}").X) / 2
                    : (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("Cannot Display Commitment Time").X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        if(_characterHandler.HasEstablishedCommitment(_tempWhitelistIdx)) {
            ImGui.TextColored(new Vector4(1f,1f,0f,1f), $"Committed For: {_tempWhitelistChar.GetCommitmentDuration()}");
        } else {
            ImGui.TextColored(new Vector4(1f,0f,0f,1f), "Cannot Display Commitment Time");
        } 
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip($"The length of time you have been in a commitment with {_tempWhitelistChar._name.Split(' ')[0]}.");
        }
        // draw out a seperator
        ImGui.Spacing();
        ImGui.Separator();

        string[] roleLeanNames = Enum.GetValues<RoleLean>().Select(role => role.ToString()).ToArray(); // for inside the table
        // Create a table for the relations manager
        using (var tableGeneral = ImRaii.Table("RelationsManagerTable", 2)) {
            if (!tableGeneral) { return; }
            // Create the headers for the table
            ImGui.TableSetupColumn("Relations Tag", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X * 0.55f);
            ImGui.TableSetupColumn("Relation Information", ImGuiTableColumnFlags.WidthStretch);            
            // underneath this, we will display the relationship dynamic, we will want to display "No Defined Dynamic" and lean a blank line under it, if no dynamic is defind
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"Your Dynamic Lean to {_tempWhitelistChar._name.Split(' ')[0]}: ");
            ImGui.TableNextColumn();
            if(_tempWhitelistChar._yourStatusToThem == RoleLean.None) {
                ImGui.TextColored(new Vector4(1f, 0.0f, 0.0f, 1.0f), "Not Defined");
            } else {
                ImGui.TextColored(new Vector4(0.0f, 1f, 0.0f, 1.0f), $"{_tempWhitelistChar._yourStatusToThem}");
            }
            // Draw out the line that displays the players defined relationship towards you
            ImGui.AlignTextToFramePadding();
            ImGuiUtil.DrawFrameColumn($"{_tempWhitelistChar._name.Split(' ')[0]}'s Dynamic lean to you: ");
            ImGui.TableNextColumn();
            if(_tempWhitelistChar._theirStatusToYou == RoleLean.None) {
                ImGui.TextColored(new Vector4(1f, 0.0f, 0.0f, 1.0f), "Not Defined");
            } else {
                ImGui.TextColored(new Vector4(0.0f, 1f, 0.0f, 1.0f), $"{_tempWhitelistChar._theirStatusToYou}");
            }
            ImGui.TableNextColumn(); // Next Row (Request Dynamic)
            // Get a RoleLean combo listing for requesting relations
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Select Lean:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X*.75f);
            ImGui.Combo("##DynamicLeanCombo", ref _selectedDynamicIdx, roleLeanNames, roleLeanNames.Length);
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip($"Selects the dynamic lean you wish to have in the relationship with {_tempWhitelistChar._name.Split(' ')[0]}.");
            }
            ImGui.TableNextColumn();
            
            // draw a button spanning the 1st and 2nd columns of this row
            if (ImGui.Button("Send to Player##ReqDynamic", new Vector2(ImGui.GetContentRegionAvail().X * 0.7f, 0))) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to become their mistress");
                RoleLean selectedRole = (RoleLean)_selectedDynamicIdx;
                RequestDynamicToPlayer(selectedRole);
                _interactOrPermButtonEvent.Invoke(5);
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip($"Sends off to {_tempWhitelistChar._name.Split(' ')[0]} the request that you wish to be their {(RoleLean)_selectedDynamicIdx}.\n"+
                                 $"If the player accepts, the dynamic lean for you will be applied to both profiles.");
            }
        }
        // add the popups for relation requests
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X / 2 - ImGui.GetStyle().ItemSpacing.X / 2, 25.0f * ImGuiHelpers.GlobalScale );

        if (_tempWhitelistChar._pendingRelationRequestFromPlayer != RoleLean.None) { 
            RoleLean pendingDynamic = _tempWhitelistChar._pendingRelationRequestFromPlayer;
            // draw the accept and decline buttons for the pending relation request
            var relationText = pendingDynamic.ToString()?.Split(' ')[0];
            if (ImGui.Button($"Accept {_tempWhitelistChar._name.Split(' ')[0]} as your {relationText}",
                new Vector2(ImGui.GetContentRegionAvail().X*.65f, 25)))
            {
                AcceptRequestForDynamicButton();
                // set the relation request to established
                _characterHandler.UpdatePendingRelationRequestFromPlayer(_tempWhitelistIdx, RoleLean.None);
                GagSpeak.Log.Debug($"[Whitelist]: Accepting incoming relation request from "+
                $"{_tempWhitelistChar._name}");
            }
            ImGui.SameLine();
            var xPox = ImGui.GetCursorPosX();
            ImGui.SetCursorPosX(xPox + 2*ImGuiHelpers.GlobalScale);
            if (ImGui.Button($"Decline Request",
                new Vector2(ImGui.GetContentRegionAvail().X-2*ImGuiHelpers.GlobalScale, 25)))
            {
                DeclineRequestForDynamicButton();
                // set the relation request to none
                _characterHandler.UpdatePendingRelationRequestFromPlayer(_tempWhitelistIdx, RoleLean.None);
                GagSpeak.Log.Debug($"[Whitelist]: Declining {_tempWhitelistChar._name}'s relation request");
            }
        }
        // cast a seperator
        ImGui.Spacing();
        ImGui.Separator();
        // draw the warning
        if(_tempWhitelistChar._yourStatusToThem == RoleLean.None || _tempWhitelistChar._theirStatusToYou == RoleLean.None) {
            try{
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                ImGuiUtil.Center($"Before establishing a 2 way dynamic, make sure you have setup");
                ImGuiUtil.Center($"the options you want to grant {_tempWhitelistChar._name.Split(' ')[0]} Access to.");
                ImGuiUtil.Center("Doing so after will cause a lot of desync and not recommended!");
                ImGui.Separator();
            } finally {
                ImGui.PopStyleColor();
            }
        }
        // now we need to clarify how the dynamic tier system works
        ImGui.PushFont(_fontService.UidFont);
        try {
            ImGuiUtil.Center("\"How Do I Get A Certain Dynamic Tier?\"");
        } finally {
            ImGui.PopFont();
        }        
        using (var tableGeneral = ImRaii.Table("DynamicTierTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV)) {
            if (!tableGeneral) { return; }
            // Create the headers for the table
            ImGui.TableSetupColumn("Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.GetTextLineHeightWithSpacing()*2.4f);
            ImGui.TableSetupColumn("What lean you must have", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale*150);
            ImGui.TableSetupColumn($"What lean {_tempWhitelistChar._name.Split(' ')[0]} must have", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            // draw out the dynamic tier system
            ImGuiUtil.DrawFrameColumn("Tier 0");
            ImGuiComponents.HelpMarker("Used for Light Play, Mainly for Submissive/Dominant Relationships.\n"+
                                       "The Submissive can set up what permissions they want to give to the Dominant.\n"+
                                       "The Dominant will not be allowed to change these, and respect the Submissive's wishes.");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Any lean works here.");
            ImGui.TableNextColumn();
            ImGui.TextWrapped($"{_tempWhitelistChar._name.Split(' ')[0]} can be any lean.");
            // next
            ImGuiUtil.DrawFrameColumn("Tier 1");
            ImGuiComponents.HelpMarker("Used for Light Play and basic trust.\n"+
                                       "Aimed towards relationships for pets and their Mistress/Masters.\n"+
                                       "Grants the ability for the Mistress/Master to override their pet's light permissions.\n"+
                                       "These Include permissions like toggle toy state, allowing sit permissions, set locking.");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Mistress, Master, or Owner");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Pet, Slave, or Absolute-Slave");
            // next
            ImGuiUtil.DrawFrameColumn("Tier 2");
            ImGuiComponents.HelpMarker("For the degree of trust where commitments become real.\n"+
                                       "Aimed towards slaves to their Masters/Mistress's, grants access to moderately serious permission override.\n"+
                                       "Permissions such as allowing emote / expression control, extended lock timer durations,\n"+
                                       "restraint set toggle control, are all able to be toggled by the Mistress/Master.");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Mistress, Master, or Owner");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Slave, or Absolute-Slave");
            // next
            ImGuiUtil.DrawFrameColumn("Tier 3");
            ImGuiComponents.HelpMarker("For Commitments you have enough trust in to grant them control over your voice.\n"+
                                       "Catered towards owners of slaves and absolute-slaves.\n"+
                                       "The Mistress/Master will be able to toggle your direct chat garbler at will.");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Owner");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Slave, or Absolute-Slave");
            // next
            ImGuiUtil.DrawFrameColumn("Tier 4");
            ImGuiComponents.HelpMarker("For commitments of Absolute Devotion. Be careful enabling this, as it gives them full control.\n"+
                                       "For Owners of their Absolute-Slaves.\n"+
                                       "Grants access to direct chat garbler locking, toybox control, and all puppeteer commands.");            
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Owner");
            ImGui.TableNextColumn();
            ImGui.TextWrapped("Absolute-Slave");
        }
        // pop the style var
        ImGui.PopStyleVar();
    }

#region DynamicRequestPopUp
    /// <summary>  Applies logic for accepting a requested status from a player. </summary>
    public void AcceptRequestForDynamicButton() {
        // get the player payload    
        PlayerPayload playerPayload;
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        _characterHandler.IsIndexWithinBounds(_tempWhitelistIdx);
        // print to chat that you sent the request
        string targetPlayer = $"{_tempWhitelistChar._name}@{_tempWhitelistChar._homeworld}";
        // get dynamic
        RoleLean requestedDynamic = _tempWhitelistChar._pendingRelationRequestFromPlayer;
        // send request based on the dynamic
        if(requestedDynamic == RoleLean.Owner || requestedDynamic == RoleLean.Master
        || requestedDynamic == RoleLean.Mistress || requestedDynamic == RoleLean.Dominant) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_tempWhitelistChar._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // get the var to see if we should set the commitment time
            bool preventTimerRestart = _characterHandler.CheckForPreventTimeRestart(
                            _tempWhitelistIdx, _tempWhitelistChar._yourStatusToThem, _tempWhitelistChar._pendingRelationRequestFromPlayer);           
            // set the relationship status the player has towards you "They are your Mistress" here, because once you hit accept, both sides agree
            _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, _tempWhitelistChar._pendingRelationRequestFromPlayer);
            // reset the timer if we should
            if(_tempWhitelistChar._yourStatusToThem != RoleLean.None && !preventTimerRestart) {
                _tempWhitelistChar.Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            // send the message
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestDominantStatus(playerPayload, targetPlayer, requestedDynamic));
        }
        // if the dynamic role is a submissive role
        else if(requestedDynamic == RoleLean.Submissive || requestedDynamic == RoleLean.Pet || requestedDynamic == RoleLean.Slave)
        {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_tempWhitelistChar._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // get the var to see if we should set the commitment time
            bool preventTimerRestart = _characterHandler.CheckForPreventTimeRestart(
                            _tempWhitelistIdx, _tempWhitelistChar._yourStatusToThem, _tempWhitelistChar._pendingRelationRequestFromPlayer);     
            // set the relationship status the player has towards you "They are your Pet" here, because once you hit accept, both sides agree
            _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, _tempWhitelistChar._pendingRelationRequestFromPlayer);
            // reset the timer if we should
            if(_tempWhitelistChar._yourStatusToThem != RoleLean.None && !preventTimerRestart) {
                _tempWhitelistChar.Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
            }
            _chatManager.SendRealMessage(_messageEncoder.EncodeAcceptRequestSubmissiveStatus(playerPayload, targetPlayer, requestedDynamic));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now accepted "+
                $"{_tempWhitelistChar._name} as your {requestedDynamic}. Updating their whitelist information").AddItalicsOff().BuiltString);
            // prevert timer restart if we should
            bool preventTimerRestart = _characterHandler.CheckForPreventTimeRestart(
                            _tempWhitelistIdx, _tempWhitelistChar._yourStatusToThem, _tempWhitelistChar._pendingRelationRequestFromPlayer);  
            // set the relationship status the player has towards you "They are your Absolute-Slave" here, because once you hit accept, both sides agree
            _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, _tempWhitelistChar._pendingRelationRequestFromPlayer);
            // reset the timer if we should
            if(_tempWhitelistChar._yourStatusToThem != RoleLean.None && !preventTimerRestart) {
                _tempWhitelistChar.Set_timeOfCommitment(); // set the commitment time if relationship is now two-way!
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
        if (_tempWhitelistIdx < 0 || _tempWhitelistIdx >= _characterHandler.whitelistChars.Count) { return; }
        // print to chat that you sent the request
        string targetPlayer = $"{_tempWhitelistChar._name}@{_tempWhitelistChar._homeworld}";
        // get dynamic
        RoleLean requestedDynamic = _tempWhitelistChar._pendingRelationRequestFromPlayer;
        // execute action based on dynamic type
        if(requestedDynamic == RoleLean.Owner || requestedDynamic == RoleLean.Master
        || requestedDynamic == RoleLean.Mistress || requestedDynamic == RoleLean.Dominant) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_tempWhitelistChar._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, RoleLean.None);
            _characterHandler.UpdatePendingRelationRequestFromPlayer(_tempWhitelistIdx, RoleLean.None);
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestDominantStatus(playerPayload, targetPlayer));
        } else if(requestedDynamic == RoleLean.Pet || requestedDynamic == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_tempWhitelistChar._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, RoleLean.None);
            _characterHandler.UpdatePendingRelationRequestFromPlayer(_tempWhitelistIdx, RoleLean.None);
            _chatManager.SendRealMessage(_messageEncoder.EncodeDeclineRequestSubmissiveStatus(playerPayload, targetPlayer));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have now declined "+
                $"{_tempWhitelistChar._name}'s request to become their {requestedDynamic}.").AddItalicsOff().BuiltString);
            // clear the pending status and not change the relationship status, rather set it to none, because both sides do not agree.
            _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, RoleLean.None);
            _characterHandler.UpdatePendingRelationRequestFromPlayer(_tempWhitelistIdx, RoleLean.None);
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
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        string targetPlayer = $"{_tempWhitelistChar._name}@{_tempWhitelistChar._homeworld}";
        // print to chat that you sent the request
        if(dynamicRole == RoleLean.Owner || dynamicRole == RoleLean.Master || dynamicRole == RoleLean.Mistress || dynamicRole == RoleLean.Dominant) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_tempWhitelistChar._name}, to see if they would like you to become their {dynamicRole}.").AddItalicsOff().BuiltString);
            //update information and send message
            _characterHandler.UpdatePendingRelationRequestFromYou(_tempWhitelistIdx, dynamicRole);
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestDominantStatus(playerPayload, targetPlayer, dynamicRole));
        } 
        // if the dynamic role is a submissive role
        else if(dynamicRole == RoleLean.Submissive || dynamicRole == RoleLean.Pet || dynamicRole == RoleLean.Slave) {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_tempWhitelistChar._name}, to see if they would like you to become their {dynamicRole}.").AddItalicsOff().BuiltString);
            //update information and send message
            _characterHandler.UpdatePendingRelationRequestFromYou(_tempWhitelistIdx, dynamicRole);
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestSubmissiveStatus(playerPayload, targetPlayer, dynamicRole));
        } else {
            _chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending request to "+
                $"{_tempWhitelistChar._name}, to see if they would like you to become their Absolute-Slave.").AddItalicsOff().BuiltString);
            //update information and send message
            _characterHandler.UpdatePendingRelationRequestFromYou(_tempWhitelistIdx, RoleLean.AbsoluteSlave);
            _chatManager.SendRealMessage(_messageEncoder.EncodeRequestAbsoluteSubmissionStatus(playerPayload, targetPlayer));
        }
    }

    /// <summary>  Controls logic for what to do once the the remove relation button is pressed in the whitelist tab. </summary>
    public void RequestRelationRemovalToPlayer() {
        // get player payload
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_tempWhitelistIdx)) { return; }
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Removing Relation Status "+
            $"with {_tempWhitelistChar._name}.").AddItalicsOff().BuiltString);
        //update information and send message
        _characterHandler.UpdateYourStatusToThem(_tempWhitelistIdx, RoleLean.None);
        _characterHandler.UpdateTheirStatusToYou(_tempWhitelistIdx, RoleLean.None);
        _characterHandler.UpdatePendingRelationRequestFromYou(_tempWhitelistIdx, RoleLean.None);
        _characterHandler.UpdatePendingRelationRequestFromPlayer(_tempWhitelistIdx, RoleLean.None);
        string targetPlayer = _tempWhitelistChar._name + "@" + _tempWhitelistChar._homeworld;
        _chatManager.SendRealMessage(_messageEncoder.EncodeSendRelationRemovalMessage(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}