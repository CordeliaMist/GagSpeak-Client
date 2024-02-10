using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Enums;
using XivCommon.Functions;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.CharacterData;
using GagSpeak.ToyboxandPuppeteer;

namespace GagSpeak.ChatMessages;
/// <summary> This class is used to handle the incoming chat messages from the game, and decided what to do with them based off what is processed. </summary>
public class ChatManager
{
    private readonly    IChatGui               _clientChat;                        // client chat 
    private readonly    GagSpeakConfig         _config;                            // config from GagSpeak
    private readonly    CharacterHandler       _characterHandler;                   // character data manager
    private readonly    PuppeteerMediator      _puppeteerMediator;                 // puppeteer mediator
    private readonly    IClientState           _clientState;                       // client state for player info
    private readonly    IObjectTable           _objectTable;                       // object table for scanning through rendered objects
    private readonly    RealChatInteraction    _realChatInteraction;               // real chat interaction
    private readonly    MessageDictionary      _messageDictionary;                 // dictionary for looking up encoded messages
    private readonly    MessageDecoder         _messageDecoder;                    // decoder for encoded messages
    private readonly    ResultLogic            _msgResultLogic;                    // logic for what happens to the player as a result of the tell
    private             DecodedMessageMediator _decodedMessageMediator;           // mediator for decoded messages
    private readonly    IFramework             _framework;                         // framework for dalamud and the game
    public              Queue<string>          messageQueue = new Queue<string>(); // stores any messages to be sent on the next framework update
    private             Stopwatch              messageTimer = new Stopwatch();     // timer for the queue of messages to be sent

    /// <summary> This is the constructor for the ChatManager class. </summary>
    public ChatManager(IChatGui clientChat, GagSpeakConfig config, IClientState clientState, IObjectTable objectTable,
    RealChatInteraction realChatInteraction, MessageDictionary messageDictionary, MessageDecoder messageDecoder, DecodedMessageMediator decodedMessageMediator,
    ResultLogic messageResultLogic ,IFramework framework, CharacterHandler characterHandler, PuppeteerMediator puppeteerMediator) {
        _clientChat = clientChat;
        _config = config;
        _clientState = clientState;
        _objectTable = objectTable;
        _realChatInteraction = realChatInteraction;
        _messageDictionary = messageDictionary;
        _messageDecoder = messageDecoder;
        _msgResultLogic = messageResultLogic;
        _framework = framework;
        _characterHandler = characterHandler;
        _puppeteerMediator = puppeteerMediator;
        _decodedMessageMediator = decodedMessageMediator;

        _framework.Update += framework_Update;
        _clientChat.CheckMessageHandled += Chat_OnCheckMessageHandled;
        _clientChat.ChatMessage += Chat_OnChatMessage;
    }

    /// <summary> This is the disposer for the ChatManager class. </summary>
    public void Dispose() {
        _framework.Update -= framework_Update;
        _clientChat.CheckMessageHandled -= Chat_OnCheckMessageHandled;
        _clientChat.ChatMessage -= Chat_OnChatMessage;
    }

    /// <summary>
    /// Called every time a message goes through the chatbox prior to be handled. Meaning, if we handle
    /// it before it reaches Chat_OnChatMessage, setting its handled var to true, then it won't show up in chat.
    /// </summary>
    private void Chat_OnCheckMessageHandled(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool isHandled) {
        
        // if the message is a outgoing tell
        if ( type == XivChatType.TellOutgoing) {
            // Scan if the message contains all words from the an ncoded tell message
            if(_messageDictionary.LookupMsgDictionary(message.TextValue, _decodedMessageMediator)) {
                // if we reach here it is an encoded tell so hide it
                isHandled = true;
                // then reset its variables
                _decodedMessageMediator.encodedMsgIndex = 0;
                _decodedMessageMediator.msgType = DecodedMessageType.None;
                return;
            }
        }
    }

    /// <summary>
    /// Function that is called every time a message is sent to your chatbox. Used to detecting anything we want to do with chat.
    /// This included all of our encoded message :D
    /// </summary>
    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
        // create some new SeStrings for the message and the new line
        var fmessage = new SeString(new List<Payload>());
        var nline = new SeString(new List<Payload>());
        nline.Payloads.Add(new TextPayload("\n"));
        // make payload for the player
        PlayerPayload playerPayload;
        //removes special characters in party listings [https://na.finalfantasyxiv.com/lodestone/character/10080203/blog/2891974/]
        List<char> toRemove = new() {
            '','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','',
        };
        // convert the sender from SeString to String
        var sanitized = sender.ToString();
        // loop through each character in the toRemove list
        foreach(var c in toRemove) { sanitized = sanitized.Replace(c.ToString(), string.Empty); } // remove all special characters
        
        // if the sender is the local player, set the player payload to the local player 
        if (sanitized == _clientState.LocalPlayer?.Name.TextValue) {
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            if (type == XivChatType.CustomEmote) {
                var playerName = new SeString(new List<Payload>());
                playerName.Payloads.Add(new TextPayload(_clientState.LocalPlayer.Name.TextValue));
                fmessage.Append(playerName);
            }
        }
        // if the sender is not the local player, set the player payload to the sender
        #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type, dont care didnt ask.
        else {
            if(type == XivChatType.StandardEmote) {
                playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload ?? 
                                chatmessage.Payloads.FirstOrDefault(x => x is PlayerPayload) as PlayerPayload;
            } 
            else {
                playerPayload = sender.Payloads.SingleOrDefault(x => x is PlayerPayload) as PlayerPayload; 
                if (type == XivChatType.CustomEmote && playerPayload != null) {
                    fmessage.Append(playerPayload.PlayerName);
                }
            }
        }
        #pragma warning restore CS8600 // let us see if we have any others
        // append the chat message to the new formatted message 
        fmessage.Append(chatmessage);
        var isEmoteType = type is XivChatType.CustomEmote or XivChatType.StandardEmote;
        if (isEmoteType) {
            fmessage.Payloads.Insert(0, new EmphasisItalicPayload(true));
            fmessage.Payloads.Add(new EmphasisItalicPayload(false));
        }
        // set the player name to the player payload, otherwise set it to the local player
        var pName = playerPayload == default(PlayerPayload) ? _clientState.LocalPlayer?.Name.TextValue : playerPayload.PlayerName;
        var sName = sender.Payloads.SingleOrDefault( x => x is PlayerPayload) as PlayerPayload; // get the player payload from the sender 
        var senderName = sName?.PlayerName != null ? sName.PlayerName : pName;
        // if the message is an incoming tell
        if (type == XivChatType.TellIncoming) 
        {
            if (senderName == null) { GagSpeak.Log.Error("senderName is null"); return; } // removes the possibly null reference warning

            switch (true) {
                case var _ when _characterHandler.playerChar._doCmdsFromFriends && _characterHandler.playerChar._doCmdsFromParty: //  both friend and party options are checked
                    if (!(IsFriend(senderName) || IsPartyMember(senderName) || IsWhitelistedPlayer(senderName))) { return; } break;

                case var _ when _characterHandler.playerChar._doCmdsFromFriends: // When only friend is checked
                    if (!(IsFriend(senderName) || IsWhitelistedPlayer(senderName))) { return; } break;

                case var _ when _characterHandler.playerChar._doCmdsFromParty: // When only party is checked
                    if (!(IsPartyMember(senderName) || IsWhitelistedPlayer(senderName))) { return; } break;

                default: // None of the filters were checked, so just accept the message anyways because it works for everyone.
                    if (!IsWhitelistedPlayer(senderName)) { return; } break;
            }
            
            ////// Once we have reached this point, we know we have recieved a tell, and that it is from one of our filtered players. //////
            GagSpeak.Log.Debug($"[Chat Manager]: Recieved tell from: {senderName} with message: {fmessage.ToString()}");

            // if the incoming tell is an encoded message, lets check if we are in dom mode before accepting changes
            
            // if the message is a encoded message, then we can process it
            if (_messageDictionary.LookupMsgDictionary(chatmessage.TextValue, _decodedMessageMediator)) {
                // if we reach here, we have the encodedMsgIndex and the msgType stored into our mediator, and we know it will process our message, so do it
                _messageDecoder.DecodeMsgToList(fmessage.ToString(), _decodedMessageMediator);

                // Process the message based on its type
                switch (_decodedMessageMediator.msgType) {
                    case DecodedMessageType.GagSpeak:
                        // try and process the result logic, if successful, set isHandled to true
                        if(_msgResultLogic.CommandMsgResLogic(fmessage.ToString(), _decodedMessageMediator, isHandled)){
                            isHandled = true;
                            // because it is handled now, we should reset the mediators values and do an early escape / return
                            _decodedMessageMediator.ResetAttributes();
                            return;
                        }
                        break;
                    case DecodedMessageType.Relationship:
                        // try and process the result logic, if successful, set isHandled to true
                        if(_msgResultLogic.WhitelistMsgResLogic(fmessage.ToString(), _decodedMessageMediator, isHandled)) {
                            isHandled = true;
                            // because it is handled now, we should reset the mediators values and do an early escape / return
                            _decodedMessageMediator.ResetAttributes();
                            return;
                        }
                        break;
                    case DecodedMessageType.Wardrobe:
                        // try and process the result logic, if successful, set isHandled to true
                        if(_msgResultLogic.WardrobeMsgResLogic(fmessage.ToString(), _decodedMessageMediator, isHandled)) {
                            isHandled = true;
                            // because it is handled now, we should reset the mediators values and do an early escape / return
                            _decodedMessageMediator.ResetAttributes();
                            return;
                        }
                        break;
                    case DecodedMessageType.Puppeteer:
                        // try and process the result logic, if successful, set isHandled to true
                        if(_msgResultLogic.PuppeteerMsgResLogic(fmessage.ToString(), _decodedMessageMediator, isHandled)) {
                            isHandled = true;
                            // because it is handled now, we should reset the mediators values and do an early escape / return
                            _decodedMessageMediator.ResetAttributes();
                            return;
                        }
                        break;
                    case DecodedMessageType.Toybox:
                        // try and process the result logic, if successful, set isHandled to true
                        if(_msgResultLogic.ToyboxMsgResLogic(fmessage.ToString(), _decodedMessageMediator, isHandled)) {
                            isHandled = true;
                            // because it is handled now, we should reset the mediators values and do an early escape / return
                            _decodedMessageMediator.ResetAttributes();
                            return; 
                        }
                        break;
                    case DecodedMessageType.InfoExchange:
                        // try and process the result logic, if successful, set isHandled to true
                        if(_msgResultLogic.ResLogicInfoRequestMessage(fmessage.ToString(), _decodedMessageMediator, isHandled)) {
                            isHandled = true;
                            // because it is handled now, we should reset the mediators values and do an early escape / return
                            _decodedMessageMediator.ResetAttributes();
                            return;
                        }
                        break;
                }
            } // skipped to this point if not encoded message
        } // skips directly to here if not a tell

        // at this point, we have determined that it is not an encoded message, and we still have the sender info.
        // This Conditional Says it will only meet if the following is true:
        // --- The sender name is not null
        // --- The sender allows puppeteer
        // --- The sender is in your whitelist
        // --- the message was not processed by the encoded messages
        if(senderName != null && _characterHandler.playerChar._allowPuppeteer && _characterHandler.IsPlayerInWhitelist(senderName) && isHandled == false) {
            GagSpeak.Log.Debug($"[ChatManager] Puppeteer was enabled, scanning message from {senderName}, as they are in your whitelist");
            // see if it contains your trigger word for them
            if(_puppeteerMediator.ContainsTriggerWord(senderName, chatmessage.TextValue, out string puppeteerMessageToSend)){
                GagSpeak.Log.Debug($"[ChatManager] Puppeteer message to send: {puppeteerMessageToSend}");
                if(puppeteerMessageToSend != string.Empty) {
                    // apply any alias translations, if any
                    SeString aliasedMessageToSend = _puppeteerMediator.ConvertAliasCommandsIfAny(senderName, puppeteerMessageToSend);
                    // if it does, then our message is valid, but we should also make sure we are in one of our enabled channels
                    if(_config.ChannelsPuppeteer.Contains(ChatChannel.GetChatChannel())
                    && _puppeteerMediator.MeetsSettingCriteria(senderName, aliasedMessageToSend))
                    {
                        // if we are in a valid chatchannel, then send it
                        messageQueue.Enqueue("/" + aliasedMessageToSend);
                    } else {
                        GagSpeak.Log.Debug($"[ChatManager] Not an Enabled Chat Channel, or command didnt abide by your settings aborting");
                    }
                } else {
                    GagSpeak.Log.Debug($"[ChatManager] Puppeteer message to send was empty, aborting");
                }
            } else {
                GagSpeak.Log.Debug($"[ChatManager] Message does not contain trigger word");
            }
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

    /// <summary>
    /// will send a real message to the chat, server side, USE WITH CAUTION
    /// <list type="bullet">
    /// <item><c>message</c><param name="message"> - The message to be sent to the server</param></item>
    /// </list></summary>
    public void SendRealMessage(string message) {
        try {
            _realChatInteraction.SendMessage(message);
        } catch(Exception e) {
            GagSpeak.Log.Error($"[Chat Manager]: Failed to send message {e}: {message}");
        }
    }

    /// <summary>
    /// This function will handle the framework update, and will send messages to the server if there are any in the queue.
    /// <list type="bullet">
    /// <item><c>framework</c><param name="framework"> - The framework to be updated.</param></item>
    /// </list></summary>
    private void framework_Update(IFramework framework) {
        if (_config != null && _config.Enabled) {
            try {
                if (messageQueue.Count > 0 && _realChatInteraction != null) {
                    if (!messageTimer.IsRunning) {
                        messageTimer.Start();
                    } else {
                        if (messageTimer.ElapsedMilliseconds > 1500) {
                            try {
                                _realChatInteraction.SendMessage(messageQueue.Dequeue());
                            } catch (Exception e) {
                                GagSpeak.Log.Warning($"{e},{e.Message}");
                            }
                            messageTimer.Restart();
                        }
                    }
                }
            } catch {
                GagSpeak.Log.Error($"[Chat Manager]: Failed to process Framework Update!");
            }
        }
    } 
}
