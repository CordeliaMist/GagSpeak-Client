using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XivCommon.Functions;
using GagSpeak.CharacterData;
using GagSpeak.ToyboxandPuppeteer;

namespace GagSpeak.ChatMessages;
/// <summary>
/// Used for checking messages send to the games chatbox, not meant for detouring or injection
/// Messages passed through here are scanned to see if they are encoded, for puppeteer, or include any hardcore features.
/// </summary>
public class OnChatMsgManager
{
    private readonly    IChatGui               _clientChat;         // client chat
    private readonly    IClientState           _clientState;        // client state for player info
    private readonly    IFramework             _framework;          // framework for dalamud and the game
    private readonly    GagSpeakConfig         _config;             // config from GagSpeak
    private readonly    CharacterHandler       _characterHandler;   // character data manager
    private readonly    PuppeteerMediator      _puppeteerMediator;  // puppeteer mediator
    private readonly    RealChatInteraction    _realChatInteraction;// real chat interaction
    private readonly    EncodedMsgDetector     _encodedMsgDetector; // detector for encoded messages
    private readonly    TriggerWordDetector    _triggerWordDetector;// detector for trigger words
    private readonly    HardcoreMsgDetector    _hardcoreMsgDetector;// detector for hardcore features
    public              Queue<string>          messageQueue;        // stores any messages to be sent on the next framework update
    private             Stopwatch              messageTimer;        // timer for the queue of messages to be sent

    /// <summary> This is the constructor for the OnChatMsgManager class. </summary>
    public OnChatMsgManager(IChatGui clientChat, IClientState clientState, IFramework framework,
    GagSpeakConfig config, CharacterHandler characterHandler, PuppeteerMediator puppeteerMediator,
    RealChatInteraction realChatInteraction, EncodedMsgDetector encodedMsgDetector,
    TriggerWordDetector triggerWordDetector, HardcoreMsgDetector hardcoreMsgDetector) {
        _clientChat = clientChat;
        _clientState = clientState;
        _framework = framework;
        _config = config;
        _characterHandler = characterHandler;
        _puppeteerMediator = puppeteerMediator;
        _realChatInteraction = realChatInteraction;
        _encodedMsgDetector = encodedMsgDetector;
        _triggerWordDetector = triggerWordDetector;
        _hardcoreMsgDetector = hardcoreMsgDetector;
        // set variables
        messageQueue = new Queue<string>();
        messageTimer = new Stopwatch();
        // set up the event handlers
        _framework.Update += framework_Update;
        _clientChat.CheckMessageHandled += Chat_OnCheckMessageHandled;
        _clientChat.ChatMessage += Chat_OnChatMessage;
    }

    /// <summary> This is the disposer for the OnChatMsgManager class. </summary>
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
        if (type == XivChatType.TellOutgoing) {
            // Scan if the message contains all words from the an ncoded tell message
            if(_encodedMsgDetector.IsMessageEncoded(message)) {
                // if we reach here it is an encoded tell so hide it
                isHandled = true;
                return;
            }
        }
    }

    /// <summary>
    /// Function that is called every time a message is sent to your chatbox. Used to detecting anything we want to do with chat.
    /// This included all of our encoded message :D
    /// </summary>
    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
#region Sanatizing Chat Message Before Logic
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
#endregion Sanatizing Chat Message Before Logic
        // if the message is an incoming tell
        if (type == XivChatType.TellIncoming) {
            if (senderName == null) { GagSpeak.Log.Error("senderName is null"); return ; } // removes the possibly null reference warning
            // if not null, handle the tell incase it is encoded.
            _encodedMsgDetector.HandleInTellMsgForEncoding(senderName, chatmessage, fmessage, ref isHandled);
        } // skips directly to here if not a tell


        // check for global puppeteer trigger.
        if(senderName != _clientState.LocalPlayer?.Name.TextValue && type != XivChatType.TellOutgoing && isHandled == false)
        {
            // if it is a valid global trigger word, send the message to the server
            if(_triggerWordDetector.IsValidGlobalTriggerWord(chatmessage, type, out SeString messageToSend)) {
                // if we are in a valid chatchannel, then send it
                GagSpeak.Log.Debug($"[OnChatMsgManager] Global Puppeteer message to send: {messageToSend.TextValue}");
                messageQueue.Enqueue("/" + messageToSend.TextValue);
            }
        }
        
        // personal puppeteer trgger
        if(senderName != null  && _characterHandler.playerChar._allowPuppeteer && _characterHandler.IsPlayerInWhitelist(senderName) && isHandled == false)
        {
            // if it contains a trigger word, then process it
            if(_triggerWordDetector.IsValidPuppeteerTriggerWord(senderName, chatmessage, type, ref isHandled, out SeString messageToSend)) {
                // if we are in a valid chatchannel, then send it
                GagSpeak.Log.Debug($"[OnChatMsgManager] Puppeteer message to send: {messageToSend.TextValue}");
                messageQueue.Enqueue("/" + messageToSend.TextValue);
            }
        }

        // check for incoming verbal hardcore features (future)
        if(senderName != null && isHandled == false && (_characterHandler.IsPlayerInWhitelist(senderName) || senderName == _clientState.LocalPlayer?.Name.TextValue))
        {
            if(_hardcoreMsgDetector.IsValidMsgTrigger(senderName, chatmessage, type, out SeString messageToSend)) {
                // if we are in a valid chatchannel, then send it
                GagSpeak.Log.Debug($"[OnChatMsgManager] Hardcore message to send: {messageToSend.TextValue}");
                messageQueue.Enqueue("/" + messageToSend.TextValue);
            }
        }

        // check for handling triggers for the vibe toybox (future) [MsTress Project]
        if(senderName != null  && _characterHandler.playerChar._enableToybox
        && _characterHandler.IsPlayerInWhitelist(senderName) && isHandled == false)
        {
            // scan for any vibe toybox triggers
        }

    }

    /// <summary> This function will send a message to the server. </summary>
    public void SendRealMessage(string message) {
        try {
            _realChatInteraction.SendMessage(message);
        } catch(Exception e) {
            GagSpeak.Log.Error($"[Chat Manager]: Failed to send message {e}: {message}");
        }
    }

    /// <summary>
    /// This function will handle the framework update, 
    /// and will send messages to the server if there are any in the queue.
    /// </summary>
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
