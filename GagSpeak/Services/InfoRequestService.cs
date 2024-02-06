using Dalamud.Plugin.Services;
using GagSpeak.Events;
using GagSpeak.ChatMessages;
using System;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.CharacterData;
using GagSpeak.UI.Tabs.WhitelistTab;

namespace GagSpeak.Services;

/// <summary>
/// This class is used to handle the info request service, and triggers every time it is invoked by its corresponding event.
/// </summary>
public class InfoRequestService : IDisposable
{
    private readonly GagSpeakConfig     _config;            // for getting the config
    private readonly CharacterHandler   _characterHandler;  // for getting the whitelist
    private readonly TimerService       _timerService;      // for setting up timers
    private readonly ChatManager        _chatManager;       // for sending messages to the chat
    private readonly MessageEncoder     _encoder;       // for sending messages to the chat
    private readonly IClientState       _clientState;       // for getting the player name
    private readonly IChatGui           _chatGui;           // for sending messages to the chat
    private readonly InfoRequestEvent   _infoRequestEvent;  // for getting the event

    public InfoRequestService(GagSpeakConfig config, CharacterHandler characterHandler, TimerService timerService, ChatManager chatManager,
    MessageEncoder encoder, IClientState clientState, IChatGui chatGui, InfoRequestEvent infoRequestEvent) {
        _config = config;
        _characterHandler = characterHandler;
        _timerService = timerService;
        _chatManager = chatManager;
        _encoder = encoder;
        _clientState = clientState;
        _chatGui = chatGui;
        _infoRequestEvent = infoRequestEvent;
        // subscribe to the event
        _infoRequestEvent.InfoRequest += SendInformationPartOne;
    }

    public void Dispose() {
        // unsubscribe from the event
        _infoRequestEvent.InfoRequest -= SendInformationPartOne;
    }


    // this can be done more effectively with tasks and await but dont want to risk breaking gamework thread
    private void SendInformationPartOne(object sender, InfoRequestEventArgs e) {
        GagSpeak.Log.Debug("[Whitelist]: Received Player Info Request, Verifying if player is in your whitelist and we are accepting info requests at the moment...");
        // check if the player is in your whitelist
        string senderName = _config.sendInfoName.Substring(0, _config.sendInfoName.IndexOf('@'));
        if (_characterHandler.IsPlayerInWhitelist(senderName))
        {
            //_config.acceptingInfoRequests = false;
            GagSpeak.Log.Debug("[Whitelist]: Player is in your whitelist, sending info...");
            InfoSendAndRequestHelpers.SendInfoToPlayer(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config);
            // set up a timer that triggers the second half when expired
            _timerService.StartTimer("InfoRequestCDTimer", "2s", 1000, () => { SendInformationPartTwo(); });
            _config.sendInfoName = "";
        } 
        else
        {
            GagSpeak.Log.Debug("[Whitelist]: Player is not in your whitelist, ignoring request...");
            _config.sendInfoName = "";
            _config.acceptingInfoRequests = true;
            return;
        }
    }

    private void SendInformationPartTwo() {
        GagSpeak.Log.Debug("[Whitelist]: Sending second chunk of info...");
        InfoSendAndRequestHelpers.SendInfoToPlayer2(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config);
        _timerService.StartTimer("InfoRequestTimer", "2s", 1000, () => { SendInformationPartThree(); });
    }

    private void SendInformationPartThree() {
        GagSpeak.Log.Debug("[Whitelist]: Sending final chunk of info...");
        InfoSendAndRequestHelpers.SendInfoToPlayer2(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config);
        _timerService.StartTimer("InfoRequestTimer", "4s", 1000, () => { _config.acceptingInfoRequests = true; });
    }
}