
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using GagSpeak.Services;
using GagSpeak.Wardrobe;
using GagSpeak.Events;
using GagSpeak.CharacterData;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Gagsandlocks;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary>
/// This class is used to handle the message result logic for decoded messages in the GagSpeak plugin.
/// </summary>
public partial class ResultLogic {    
    private readonly    IChatGui               _clientChat;            // used to print messages to the chat
    private readonly    IClientState           _clientState;           // used to get the client state
    private readonly    GagSpeakConfig         _config;                // used to get the config
    private readonly    CharacterHandler        _characterHandler;      // used to get the character handler
    private readonly    PatternHandler         _patternHandler;        // used to get the pattern handler
    private readonly    GagStorageManager      _gagStorageManager;     // used to get the gag storage
    private readonly    RestraintSetManager    _restraintSetManager;   // used to get the restraint set manager
    private readonly    GagAndLockManager      _lockManager;           // used to get the lock manager
    private readonly    GagService             _gagService;            // used to get the gag service
    private readonly    TimerService           _timerService;          // used to get the timer service
    private readonly    JobChangedEvent        _jobChangedEvent;       // for whenever we change jobs
    private readonly    PlugService            _plugService;           // used to get the plug service

    public ResultLogic(IChatGui clientChat, IClientState clientState, GagSpeakConfig config, CharacterHandler characterHandler,
    PatternHandler patternHandler, GagStorageManager gagStorageManager, RestraintSetManager restraintSetManager, PlugService plugService,
    GagAndLockManager lockManager, GagService gagService, TimerService timerService, JobChangedEvent jobChangedEvent) {
        _clientChat = clientChat;
        _clientState = clientState;
        _config = config;
        _characterHandler = characterHandler;
        _patternHandler = patternHandler;
        _gagStorageManager = gagStorageManager;
        _restraintSetManager = restraintSetManager;
        _lockManager = lockManager;
        _gagService = gagService;
        _timerService = timerService;
        _plugService = plugService;
        _jobChangedEvent = jobChangedEvent;
    }
    /// <summary> This function is used to handle the message result logic for decoded messages involing your player in the GagSpeak plugin. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool CommandMsgResLogic(string receivedMessage, DecodedMessageMediator decodedMessageMediator, bool isHandled)
    {
        var commandType = decodedMessageMediator.encodedCmdType.ToLowerInvariant();
        var _ = commandType switch
        {
            "apply"                     => HandleApplyMessage(decodedMessageMediator, ref isHandled),
            "lock"                      => HandleLockMessage(decodedMessageMediator, ref isHandled),
            "lockpassword"              => HandleLockMessage(decodedMessageMediator, ref isHandled),
            "locktimerpassword"         => HandleLockMessage(decodedMessageMediator, ref isHandled),
            "unlock"                    => HandleUnlockMessage(decodedMessageMediator, ref isHandled),
            "unlockpassword"            => HandleUnlockMessage(decodedMessageMediator, ref isHandled),
            "remove"                    => HandleRemoveMessage(decodedMessageMediator, ref isHandled),
            "removeall"                 => HandleRemoveAllMessage(decodedMessageMediator, ref isHandled),
            "toggleLiveChatGarbler"     => HandleToggleLiveChatGarbler(decodedMessageMediator, ref isHandled),
            "toggleLiveChatGarblerLock" => HandleToggleLiveChatGarblerLock(decodedMessageMediator, ref isHandled),
            _                => LogError("Invalid Order message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function is used to handle the message result logic for decoded messages involving a whitelisted player in the GagSpeak plugin. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool WhitelistMsgResLogic(string recieved, DecodedMessageMediator decodedMessageMediator, bool isHandled)
    {
        var commandType = decodedMessageMediator.encodedCmdType;
        var _ = commandType switch
        {
            "requestDominantStatus"                 => HandleRequestRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "requestSubmissiveStatus"               => HandleRequestRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "requestAbsoluteSubmissionStatus"       => HandleRequestRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "acceptRequestDominantStatus"           => HandleAcceptRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "acceptRequestSubmissiveStatus"         => HandleAcceptRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "acceptRequestAbsoluteSubmissionStatus" => HandleAcceptRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "declineRequestDominantStatus"          => HandleDeclineRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "declineRequestSubmissiveStatus"        => HandleDeclineRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "declineRequestAbsoluteSubmissionStatus"=> HandleDeclineRelationStatusMessage(decodedMessageMediator, ref isHandled),
            "sendRelationRemovalMessage"            => HandleRelationRemovalMessage(decodedMessageMediator, ref isHandled),
            _                                       => LogError("Invalid Whitelist message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the wardrobe function logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool WardrobeMsgResLogic(string recieved, DecodedMessageMediator decodedMessageMediator, bool isHandled)
    {
        var commandType = decodedMessageMediator.encodedCmdType;
        var _ = commandType switch
        {
            "toggleGagStorageUiLock"            => ReslogicToggleGagStorageUiLock(decodedMessageMediator, ref isHandled),
            "toggleEnableRestraintSetsOption"   => ResLogicToggleEnableRestraintSetsOption(decodedMessageMediator, ref isHandled),
            "toggleAllowRestraintLockingOption" => ResLogicToggleAllowRestraintLockingOption(decodedMessageMediator, ref isHandled),
            "enableRestraintSet"                => ResLogicEnableRestraintSet(decodedMessageMediator, ref isHandled),
            "lockRestraintSet"                  => ResLogicLockRestraintSet(decodedMessageMediator, ref isHandled),
            "unlockRestraintSet"                => ResLogicRestraintSetUnlockMessage(decodedMessageMediator, ref isHandled),
            _                                   => LogError("Invalid Wardrobe message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the puppeteer function logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool PuppeteerMsgResLogic(string recieved, DecodedMessageMediator decodedMessageMediator, bool isHandled)
    {
        var commandType = decodedMessageMediator.encodedCmdType;
        var _ = commandType switch
        {
            "toggleOnlySitRequestOption"    => ReslogicToggleSitRequests(decodedMessageMediator, ref isHandled),
            "toggleOnlyMotionRequestOption" => ReslogicToggleMotionRequests(decodedMessageMediator, ref isHandled),
            "toggleAllCommandsOption"       => ReslogicToggleAllCommands(decodedMessageMediator, ref isHandled),
            _                        => LogError("Invalid Puppeteer message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the toybox function logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool ToyboxMsgResLogic(string recieved, DecodedMessageMediator decodedMessageMediator, bool isHandled)
    {
        var commandType = decodedMessageMediator.encodedCmdType;
        var _ = commandType switch
        {
            "toggleEnableToyboxOption"       => ReslogicToggleEnableToybox(decodedMessageMediator, ref isHandled),
            "toggleActiveToyOption"          => ReslogicToggleActiveToy(decodedMessageMediator, ref isHandled),
            "toggleAllowingIntensityControl" => ReslogicToggleAllowingIntensityControl(decodedMessageMediator, ref isHandled),
            "updateActiveToyIntensity"       => ReslogicUpdateActiveToyIntensity(decodedMessageMediator, ref isHandled),
            "executeStoredToyPattern"        => ReslogicExecuteStoredToyPattern(decodedMessageMediator, ref isHandled),
            "toggleLockToyboxUI"             => ReslogicToggleLockToyboxUI(decodedMessageMediator, ref isHandled),
            "toggleToyOnOff"                 => ReslogicToggleToyOnOff(decodedMessageMediator, ref isHandled),
            _ => LogError("Invalid Toybox message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the provide info message logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool ResLogicInfoRequestMessage(string recieved, DecodedMessageMediator decodedMessageMediator, bool isHandled)
    {
        var commandType = decodedMessageMediator.encodedCmdType;
        var _ = commandType switch
        {
            "requestInfo"       => ResLogicInfoRequestingMessage(decodedMessageMediator, ref isHandled),
            "shareInfoPartOne"  => ResLogicProvideInfoPartOne(decodedMessageMediator, ref isHandled),
            "shareInfoPartTwo"  => ResLogicProvideInfoPartTwo(decodedMessageMediator, ref isHandled),
            "shareInfoPartThree"=> ResLogicProvideInfoPartThree(decodedMessageMediator, ref isHandled),
            "shareInfoPartFour" => ResLogicProvideInfoPartFour(decodedMessageMediator, ref isHandled),
            _ => LogError("Invalid Provide Info message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> A simple helper function to log errors to both /xllog and your chat. </summary>
    bool LogError(string errorMessage) {
        GagSpeak.Log.Debug($"[Result Logic] {errorMessage}");
        _clientChat.PrintError($"[Result Logic] {errorMessage}");
        return false;
    }
}