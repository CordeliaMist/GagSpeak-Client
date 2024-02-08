
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
    public bool CommandMsgResLogic(string receivedMessage, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0].ToLowerInvariant();
        var _ = commandType switch
        {
            "apply"             => HandleApplyMessage(ref decodedMessage, ref isHandled, config),
            "lock"              => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "lockpassword"      => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "locktimerpassword" => HandleLockMessage(ref decodedMessage, ref isHandled, config),
            "unlock"            => HandleUnlockMessage(ref decodedMessage, ref isHandled, config),
            "unlockpassword"    => HandleUnlockMessage(ref decodedMessage, ref isHandled, config),
            "remove"            => HandleRemoveMessage(ref decodedMessage, ref isHandled, config),
            "removeall"         => HandleRemoveAllMessage(ref decodedMessage, ref isHandled, config),
            _                => LogError("Invalid Order message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function is used to handle the message result logic for decoded messages involving a whitelisted player in the GagSpeak plugin. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool WhitelistMsgResLogic(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0];
        var _ = commandType switch
        {
            "requestDominantStatus"                 => HandleRequestRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "requestSubmissiveStatus"               => HandleRequestRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "requestAbsoluteSubmissionStatus"       => HandleRequestRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "acceptRequestDominantStatus"           => HandleAcceptRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "acceptRequestSubmissiveStatus"         => HandleAcceptRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "acceptRequestAbsoluteSubmissionStatus" => HandleAcceptRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "declineRequestDominantStatus"          => HandleDeclineRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "declineRequestSubmissiveStatus"        => HandleDeclineRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "declineRequestAbsoluteSubmissionStatus"=> HandleDeclineRelationStatusMessage(ref decodedMessage, ref isHandled, config),
            "sendRelationRemovalMessage"            => HandleRelationRemovalMessage(ref decodedMessage, ref isHandled, config),
            _                                       => LogError("Invalid Whitelist message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the wardrobe function logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool WardrobeMsgResLogic(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0];
        var _ = commandType switch
        {
            "toggleGagStorageUiLock"            => ReslogicToggleGagStorageUiLock(ref decodedMessage, ref isHandled, config),
            "toggleEnableRestraintSetsOption"   => ResLogicToggleEnableRestraintSetsOption(ref decodedMessage, ref isHandled, config),
            "toggleAllowRestraintLockingOption" => ResLogicToggleAllowRestraintLockingOption(ref decodedMessage, ref isHandled, config),
            "enableRestraintSet"                => ResLogicEnableRestraintSet(ref decodedMessage, ref isHandled, config),
            "lockRestraintSet"                  => ResLogicLockRestraintSet(ref decodedMessage, ref isHandled, config),
            "unlockRestraintSet"                => ResLogicRestraintSetUnlockMessage(ref decodedMessage, ref isHandled, config),
            _                                   => LogError("Invalid Wardrobe message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the puppeteer function logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool PuppeteerMsgResLogic(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0];
        var _ = commandType switch
        {
            "toggleOnlySitRequestOption"    => ReslogicToggleSitRequests(ref decodedMessage, ref isHandled, config),
            "toggleOnlyMotionRequestOption" => ReslogicToggleMotionRequests(ref decodedMessage, ref isHandled, config),
            "toggleAllCommandsOption"       => ReslogicToggleAllCommands(ref decodedMessage, ref isHandled, config),
            _                        => LogError("Invalid Puppeteer message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the toybox function logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool ToyboxMsgResLogic(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0];
        var _ = commandType switch
        {
            "toggleEnableToyboxOption"       => ReslogicToggleEnableToybox(ref decodedMessage, ref isHandled, config),
            "toggleActiveToyOption"          => ReslogicToggleActiveToy(ref decodedMessage, ref isHandled, config),
            "toggleAllowingIntensityControl" => ReslogicToggleAllowingIntensityControl(ref decodedMessage, ref isHandled, config),
            "updateActiveToyIntensity"       => ReslogicUpdateActiveToyIntensity(ref decodedMessage, ref isHandled, config),
            "executeStoredToyPattern"        => ReslogicExecuteStoredToyPattern(ref decodedMessage, ref isHandled, config),
            "toggleLockToyboxUI"             => ReslogicToggleLockToyboxUI(ref decodedMessage, ref isHandled, config),
            _ => LogError("Invalid Toybox message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> This function handles the provide info message logic. </summary>
    /// <returns>Whether or not the message has been handled.</returns>
    public bool ResLogicInfoRequestMessage(string recieved, List<string> decodedMessage, bool isHandled,
            IChatGui clientChat, GagSpeakConfig config)
    {
        var commandType = decodedMessage[0];
        var _ = commandType switch
        {
            "requestInfo"       => ResLogicInfoRequestMessage(ref decodedMessage, ref isHandled, config),
            "shareInfoPartOne"  => ResLogicProvideInfoPartOne(ref decodedMessage, ref isHandled, config),
            "shareInfoPartTwo"  => ResLogicProvideInfoPartTwo(ref decodedMessage, ref isHandled, config),
            "shareInfoPartThree"=> ResLogicProvideInfoPartThree(ref decodedMessage, ref isHandled, config),
            _ => LogError("Invalid Provide Info message parse, If you see this report it to cordy ASAP.")
        };
        return true;
    }

    /// <summary> A simple helper function to log errors to both /xllog and your chat. </summary>
    bool LogError(string errorMessage) {
        GagSpeak.Log.Debug(errorMessage);
        _clientChat.PrintError(errorMessage);
        return false;
    }
}