using Dalamud.Plugin.Services;
using GagSpeak.Events;
using GagSpeak.ChatMessages;
using System;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.CharacterData;
using GagSpeak.UI.Tabs.WhitelistTab;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;

namespace GagSpeak.Services;

/// <summary>
/// This class is used to handle the info request service, and triggers every time it is invoked by its corresponding event.
/// </summary>
public class InfoRequestService : IDisposable
{
    private readonly GagSpeakConfig     _config;            // for getting the config
    private readonly CharacterHandler   _characterHandler;  // for getting the whitelis
    private readonly OnChatMsgManager        _chatManager;       // for sending messages to the chat
    private readonly MessageEncoder     _encoder;       // for sending messages to the chat
    private readonly IClientState       _clientState;       // for getting the player name
    private readonly IChatGui           _chatGui;           // for sending messages to the chat
    private readonly InfoRequestEvent   _infoRequestEvent;  // for getting the event

    public InfoRequestService(GagSpeakConfig config, CharacterHandler characterHandler, OnChatMsgManager chatManager,
    MessageEncoder encoder, IClientState clientState, IChatGui chatGui, InfoRequestEvent infoRequestEvent) {
        _config = config;
        _characterHandler = characterHandler;
        _chatManager = chatManager;
        _encoder = encoder;
        _clientState = clientState;
        _chatGui = chatGui;
        _infoRequestEvent = infoRequestEvent;
        // subscribe to the event
        _infoRequestEvent.InfoRequest += SendInformation;
    }

    public void Dispose() {
        // unsubscribe from the event
        _infoRequestEvent.InfoRequest -= SendInformation;
    }


    // this can be done more effectively with tasks and await but dont want to risk breaking gamework thread
    private async void SendInformation(object sender, InfoRequestEventArgs e) {
        if (_characterHandler.IsPlayerInWhitelist(e.PlayerName))
        {
            await Task.Delay(1000);
            GSLogger.LogType.Information("[InfoRequestService]: Player is in your whitelist, sending info...");
            await Task.Run(() => InfoSendAndRequestHelpers.SendInfoToPlayer(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config, e.PlayerName));
            _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{e.PlayerName}] with your details(1/4)").AddItalicsOff().BuiltString);
            
            await Task.Delay(1500);
            await Task.Run(() => InfoSendAndRequestHelpers.SendInfoToPlayer2(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config, e.PlayerName));
            _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{e.PlayerName}] with your details(2/4)").AddItalicsOff().BuiltString);
            
            await Task.Delay(1500);
            await Task.Run(() => InfoSendAndRequestHelpers.SendInfoToPlayer3(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config, e.PlayerName));
            _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{e.PlayerName}] with your details(3/4)").AddItalicsOff().BuiltString);
            
            await Task.Delay(1500);
            await Task.Run(() => InfoSendAndRequestHelpers.SendInfoToPlayer4(_characterHandler, _chatManager, _encoder, _clientState, _chatGui, _config, e.PlayerName)); 
            _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{e.PlayerName}] with your details(4/4)").AddItalicsOff().BuiltString);


            _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Finished updating whitelisted player [{e.PlayerName}] with your details.").AddItalicsOff().BuiltString);
            // clear the data
            _config.SetSendInfoName("");
            _config.SetAcceptInfoRequests(true);
            _config.SetprocessingInfoRequest(false);
        
        } 
        else
        {
            GSLogger.LogType.Information($"[Whitelist]: Player {e.PlayerName} is not in your whitelist, ignoring request...");
             _chatGui.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Player {e.PlayerName} is not in your whitelist, ignoring request...").AddItalicsOff().BuiltString);
            _config.SetSendInfoName("");
            _config.SetAcceptInfoRequests(true);
            _config.SetprocessingInfoRequest(false);
            return;
        }
    }
}