using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.CharacterData;

namespace GagSpeak.ChatMessages;
/// <summary>
/// Used for checking messages send to the games chatbox, not meant for detouring or injection
/// Messages passed through here are scanned to see if they are encoded, for puppeteer, or include any hardcore features.
public class EncodedMsgDetector
{
    private readonly    CharacterHandler       _characterHandler;                   // character data manager
    private readonly    IClientState           _clientState;                       // client state for player info
    private readonly    IObjectTable           _objectTable;                       // object table for scanning through rendered objects
    private readonly    MessageDictionary      _messageDictionary;                 // dictionary for looking up encoded messages
    private readonly    MessageDecoder         _messageDecoder;                    // decoder for encoded messages
    private readonly    ResultLogic            _msgResultLogic;                    // logic for what happens to the player as a result of the tell
    private             DecodedMessageMediator _decodedMessageMediator;           // mediator for decoded messages

    /// <summary> This is the constructor for the OnChatMsgManager class. </summary>
    public EncodedMsgDetector(CharacterHandler characterHandler, IClientState clientState,
    IObjectTable objectTable, MessageDictionary messageDictionary, MessageDecoder messageDecoder,
    ResultLogic msgResultLogic, DecodedMessageMediator decodedMessageMediator) {
        _characterHandler = characterHandler;
        _clientState = clientState;
        _objectTable = objectTable;
        _messageDictionary = messageDictionary;
        _messageDecoder = messageDecoder;
        _msgResultLogic = msgResultLogic;
        _decodedMessageMediator = decodedMessageMediator;
    }

    // handles searching to see if something is a encoded message, createa a temp mediator to so do.
    public bool IsMessageEncoded(SeString message) {
        // if the message is a encoded message, then we can process it
        if(_messageDictionary.LookupMsgDictionary(message.TextValue)) {
            return true;
        }
        return false;
    }

    /// <summary> This function is used to handle the incoming chat messages. </summary>
    public void HandleInTellMsgForEncoding(string senderName, SeString chatmessage, SeString fmessage, ref bool isHandled) {
        // otherwise, lets make sure we are following the correct checkboxes
        switch (true) {
            case var _ when _characterHandler.playerChar._doCmdsFromFriends && _characterHandler.playerChar._doCmdsFromParty: //  both friend and party options are checked
                if (!(IsFriend(senderName) || IsPartyMember(senderName) || IsWhitelistedPlayer(senderName))) { return ; } break;
            case var _ when _characterHandler.playerChar._doCmdsFromFriends: // When only friend is checked
                if (!(IsFriend(senderName) || IsWhitelistedPlayer(senderName))) { return ; } break;
            case var _ when _characterHandler.playerChar._doCmdsFromParty: // When only party is checked
                if (!(IsPartyMember(senderName) || IsWhitelistedPlayer(senderName))) { return ; } break;
            default: // None of the filters were checked, so just accept the message anyways because it works for everyone.
                if (!IsWhitelistedPlayer(senderName)) { return ; } break;
        }
        ////// Once we have reached this point, we know we have recieved a tell, and that it is from one of our filtered players. //////
        GagSpeak.Log.Debug($"[Chat Manager]: Recieved tell from: {senderName} with message: {fmessage.ToString()}");
        // if the message is a encoded message, then we can process it
        if (_messageDictionary.LookupMsgDictionary(chatmessage.TextValue, _decodedMessageMediator)) {
            // if we reach here, we have the encodedMsgIndex and the msgType stored into our mediator,
            // and we know it will process our message, so do it
            _messageDecoder.DecodeMsgToList(fmessage.ToString(), _decodedMessageMediator);
            // now process the resuly logic, if sucessful, return and hide from chat
            if(ProcessDecodedMessage(fmessage.ToString(), _decodedMessageMediator.msgType, isHandled)) {
                isHandled = true;
                _decodedMessageMediator.ResetAttributes();
                return ;
            }
        }
    } 

    /// <summary> For processing the result logic of the decoded message. </summary>
    private bool ProcessDecodedMessage(string message, DecodedMessageType messageType, bool isHandled) {
        switch (messageType) {
            case DecodedMessageType.GagSpeak:
                return _msgResultLogic.CommandMsgResLogic(message, _decodedMessageMediator, isHandled);
            case DecodedMessageType.Relationship:
                return _msgResultLogic.WhitelistMsgResLogic(message, _decodedMessageMediator, isHandled);
            case DecodedMessageType.Wardrobe:
                return _msgResultLogic.WardrobeMsgResLogic(message, _decodedMessageMediator, isHandled);
            case DecodedMessageType.Puppeteer:
                return _msgResultLogic.PuppeteerMsgResLogic(message, _decodedMessageMediator, isHandled);
            case DecodedMessageType.Toybox:
                return _msgResultLogic.ToyboxMsgResLogic(message, _decodedMessageMediator, isHandled);
            case DecodedMessageType.InfoExchange:
                return _msgResultLogic.ResLogicInfoRequestMessage(message, _decodedMessageMediator, isHandled);
            case DecodedMessageType.Hardcore:
                return _msgResultLogic.HardcoreMsgResLogic(message, _decodedMessageMediator, isHandled);
            default:
                return false;
        }
    }

    /// <summary> Will search through the senders friend list to see if they are a friend or not. </summary>
    private bool IsFriend(string nameInput) {
        // Check if it is possible for the client to grab the local player name, if so by default set to true.
        if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;
        // after, scan through each object in the object table
        foreach (var t in _objectTable) {
            // If the object is a player character (us), we found ourselves, so conmtinue on..
            if (!(t is PlayerCharacter pc)) continue;
            // If the player characters name matches the list of names from local players 
            if (pc.Name.TextValue == nameInput) {
                // See if they have a status of being a friend, if so return true, otherwise return false.
                return pc.StatusFlags.HasFlag(StatusFlags.Friend);
            }
        }
        return false;
    }

    /// <summary> Will search through the senders party list to see if they are a party member or not. </summary>
    private bool IsPartyMember(string nameInput) {
        if (nameInput == _clientState.LocalPlayer?.Name.TextValue) return true;
        foreach (var t in _objectTable) {
            if (!(t is PlayerCharacter pc)) continue;
            if (pc.Name.TextValue == nameInput)
                return pc.StatusFlags.HasFlag(StatusFlags.PartyMember);
        }
        return false;
    }

    /// <summary> Will search through the senders party list to see if they are a party member or not. </summary>
    private bool IsWhitelistedPlayer(string nameInput) {
        // Check if it is possible for the client to grab the local player name, if so by default set to true.
        if (nameInput == _clientState.LocalPlayer?.Name.TextValue) {
            return true;
        }
        foreach (var t in _objectTable) {
            if (!(t is PlayerCharacter pc)) continue;
            if (pc.Name.TextValue == nameInput) {
                if(_characterHandler.IsPlayerInWhitelist(nameInput)) {
                    return true;
                }
            }
        }
        return false;
    }
}