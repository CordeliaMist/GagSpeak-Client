using System.Linq;
using Dalamud.Plugin.Services;
using GagSpeak.Events;
using GagSpeak.Chat;
using GagSpeak.Chat.MsgEncoder;
using GagSpeak.Utility.GagButtonHelpers;
using System;

namespace GagSpeak.Services;

/// <summary>
/// This class is used to handle the info request service, and triggers every time it is invoked by its corresponding event.
/// </summary>
public class InfoRequestService : IDisposable
{
    private readonly GagSpeakConfig     _config;            // for getting the config
    private readonly TimerService       _timerService;      // for setting up timers
    private readonly ChatManager        _chatManager;       // for sending messages to the chat
    private readonly MessageEncoder     _gagMessages;       // for sending messages to the chat
    private readonly IClientState       _clientState;       // for getting the player name
    private readonly IChatGui           _chatGui;           // for sending messages to the chat
    private readonly InfoRequestEvent   _infoRequestEvent;  // for getting the event

    public InfoRequestService(GagSpeakConfig config, TimerService timerService, ChatManager chatManager,
    MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui, InfoRequestEvent infoRequestEvent) {
        _config = config;
        _timerService = timerService;
        _chatManager = chatManager;
        _gagMessages = gagMessages;
        _clientState = clientState;
        _chatGui = chatGui;
        _infoRequestEvent = infoRequestEvent;
        // subscribe to the event
        _infoRequestEvent.InfoRequest += SendInformationPartOne;
        
        GagSpeak.Log.Debug("[InfoRequestService] SERVICE CONSUTRCTOR INITIALIZED");
    }

    public void Dispose() {
        // unsubscribe from the event
        _infoRequestEvent.InfoRequest -= SendInformationPartOne;
    }

    private void SendInformationPartOne(object sender, InfoRequestEventArgs e) {
        GagSpeak.Log.Debug("[Whitelist]: Received Player Info Request, Verifying if player is in your whitelist and we are accepting info requests at the moment...");
        // check if the player is in your whitelist
        string senderName = _config.SendInfoName.Substring(0, _config.SendInfoName.IndexOf('@'));
        if (_config.Whitelist.Any(x => x.name == senderName)) {
            //_config.acceptingInfoRequests = false;
            GagSpeak.Log.Debug("[Whitelist]: Player is in your whitelist, sending info...");
        } else {
            GagSpeak.Log.Debug("[Whitelist]: Player is not in your whitelist, ignoring request...");
            _config.SendInfoName = "";
            _config.acceptingInfoRequests = true;
            return;
        }
        GagButtonHelpers.SendInfoToPlayer(_config, _chatManager, _gagMessages, _clientState, _chatGui);
        // set up a timer that triggers the second half when expired
        _timerService.StartTimer("InfoRequestCDTimer", "2s", 1000, () => { SendInformationPartTwo(); });
    }

    private void SendInformationPartTwo() {
        GagSpeak.Log.Debug("[Whitelist]: Sending second half of info...");
        // send the second half
        GagButtonHelpers.SendInfoToPlayer2(_config, _chatManager, _gagMessages, _clientState, _chatGui);
        // can set it back to blank after we do the sendinfoplayer2
        _config.SendInfoName = "";
        // set up a timer that triggers the second half when expired
        _timerService.StartTimer("InfoRequestTimer", "4s", 1000, () => { _config.acceptingInfoRequests = true; });
    }
}