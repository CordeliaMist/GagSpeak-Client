using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.CharacterData;
using OtterGui.Classes;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class ResultLogic {
    
    // decoder for requesting a dominant based relationship (master/mistress/owner) [ ID == 11, 12, 13]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
    private bool HandleRequestRelationStatusMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            int idx = _characterHandler.GetWhitelistIndex(playerName);
            // declare the pending request status as the passed in status
            RoleLean lean = _characterHandler.GetRoleLeanFromString(decodedMessage[3]);
            _characterHandler.UpdatePendingRelationRequestFromPlayer(idx, lean);
            // notify the user that the request as been sent. 
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"{playerName} has sent a request to have a {lean} relationship dynamic with you.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for a relation relation request from {playerName}");
        }
        return true;
    }

    // decoder for accepting a player relation request [ ID == 14, 15, 16]
    // [0] = commandtype, [1] = playerMsgWasSentFrom, [3] = nameOfRelationSent
    private bool HandleAcceptRelationStatusMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            int idx = _characterHandler.GetWhitelistIndex(playerName);
            // declare the pending request status as the passed in status
            RoleLean lean = _characterHandler.GetRoleLeanFromString(decodedMessage[3]);
            // set the pending relationship to none and relationship with that player to none
            switch(lean) {
                case RoleLean.Owner:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.Owner); break;
                case RoleLean.Mistress:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.Mistress); break;
                case RoleLean.Master:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.Master); break;
                case RoleLean.Submissive:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.Submissive); break;
                case RoleLean.Pet:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.Pet); break;
                case RoleLean.Slave:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.Slave); break;
                case RoleLean.AbsoluteSlave:
                    _characterHandler.UpdateYourStatusToThem(idx, RoleLean.AbsoluteSlave); break;
            }
            _characterHandler.UpdatePendingRelationRequestFromYou(idx, RoleLean.None);
                        // set the commitment time if relationship is now two-way!
            if(_characterHandler.whitelistChars[idx]._theirStatusToYou != RoleLean.None) { 
                _characterHandler.SetCommitmentTimeEstablished(idx);
            }
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You are now {playerName}'s {lean}. Enjoy~.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for Accepting {lean} relation");
            return true;
        }
        return LogError($"ERROR, Player not in your whitelist!");
    }

    private bool HandleDeclineRelationStatusMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        // get playernameworld
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        // get playerName
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // see if they exist
        if( _characterHandler.IsPlayerInWhitelist(playerName)) {
            // get its index
            int idx = _characterHandler.GetWhitelistIndex(playerName);
            // declare the pending request status as the passed in status
            RoleLean lean = _characterHandler.GetRoleLeanFromString(decodedMessage[3]);
            // set the pending relationship to none and relationship with that player to none
            _characterHandler.UpdatePendingRelationRequestFromYou(idx, RoleLean.None);
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"You have declined {playerName}'s request.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for declining a relation request");
        }
        return true;
    }

    // result logic for removing a relationship
    // [0] = commandtype, [1] = playerMsgWasSentFrom
    private bool HandleRelationRemovalMessage(ref List<string> decodedMessage, ref bool isHandled, GagSpeakConfig _config) {
        string playerNameWorld = decodedMessage[1];
        string[] parts = playerNameWorld.Split(' ');
        string world = parts.Length > 1 ? parts.Last() : "";
        string playerName = string.Join(" ", parts.Take(parts.Length - 1));
        // locate player in whitelist
        if(_characterHandler.IsPlayerInWhitelist(playerName)) {
            int idx = _characterHandler.GetWhitelistIndex(playerName);
            // set the pending relationship to none and relationship with that player to none
            _characterHandler.UpdateYourStatusToThem(idx, RoleLean.None);
            _characterHandler.UpdateTheirStatusToYou(idx, RoleLean.None);
            _characterHandler.UpdatePendingRelationRequestFromYou(idx, RoleLean.None);
            _characterHandler.UpdatePendingRelationRequestFromPlayer(idx, RoleLean.None);
            _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Relation Status with {playerName} sucessfully removed.").AddItalicsOff().BuiltString);
            GagSpeak.Log.Debug($"[MsgResultLogic]: Sucessful Logic Parse for relation removal");
        }
        return true;
    }
}