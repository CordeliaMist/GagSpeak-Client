using GagSpeak.ToyboxandPuppeteer;

namespace GagSpeak.CharacterData;

public partial class CharacterHandler
{
#region PlayerChar Global Variable Setters
    public void SetGlobalTriggerPhrase(string newPhrase) {
        playerChar._globalTriggerPhrase = newPhrase;
        _saveService.QueueSave(this);
    }

    public void SetGlobalAllowSitRequests(bool value) {
        if(playerChar._globalAllowSitRequests != value) {
            playerChar._globalAllowSitRequests = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._globalAllowSitRequests = value;
        }
    }

    public void SetGlobalAllowMotionRequests(bool value) {
        if(playerChar._globalAllowMotionRequests != value) {
            playerChar._globalAllowMotionRequests = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._globalAllowMotionRequests = value;
        }
    }

    public void SetGlobalAllowAllCommands(bool value) {
        if(playerChar._globalAllowAllCommands != value) {
            playerChar._globalAllowAllCommands = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._globalAllowAllCommands = value;
        }
    }
#endregion PlayerChar Global Variable Setters

#region PlayerChar Handler Functions
    public void SetRevertStyle(RevertStyle style) {
        playerChar._revertStyle = style;
        _saveService.QueueSave(this);
    }

    public void UpdateIntensityLevel(int intensity) {
        if(playerChar._intensityLevel != intensity) {
            playerChar._intensityLevel = intensity;
            _saveService.QueueSave(this);
        }
    }

    public void ToggleUsingSimulatedVibe() {
        playerChar._usingSimulatedVibe = !playerChar._usingSimulatedVibe;
        _saveService.QueueSave(this);
    }

    public void SetSafewordUsed(bool value) {
        if(playerChar._safewordUsed != value) {
            playerChar._safewordUsed = value;
            _saveService.QueueSave(this);
        }
    }

    public void ToggleCmdFromFriends() {
        playerChar._doCmdsFromFriends = !playerChar._doCmdsFromFriends;
        _saveService.QueueSave(this);
    }

    public void ToggleCmdFromParty() {
        playerChar._doCmdsFromParty = !playerChar._doCmdsFromParty;
        _saveService.QueueSave(this);
    }

    public void ToggleDirectChatGarbler() {
        playerChar._directChatGarblerActive = !playerChar._directChatGarblerActive;
        _saveService.QueueSave(this);
    }

    public void ToggleDirectChatGarblerLock() {
        playerChar._directChatGarblerLocked = !playerChar._directChatGarblerLocked;
        _saveService.QueueSave(this);
    }

    public void SetDirectChatGarblerLock(bool value) {
        if(playerChar._directChatGarblerLocked != value) {
            playerChar._directChatGarblerLocked = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._directChatGarblerLocked = value;
        }
    }

    public void ToggleExtendedLockTimes() {
        playerChar._uniquePlayerPerms[activeListIdx]._grantExtendedLockTimes = !playerChar._uniquePlayerPerms[activeListIdx]._grantExtendedLockTimes;
        _saveService.QueueSave(this);
    }

    public void ToggleZoneWarnings() {
        playerChar._liveGarblerWarnOnZoneChange = !playerChar._liveGarblerWarnOnZoneChange;
        _saveService.QueueSave(this);
    }

    public void ToggleEnableWardrobe() {
        playerChar._enableWardrobe = !playerChar._enableWardrobe;
        _saveService.QueueSave(this);
    }

    public void ToggleGagItemAutoEquip() {
        playerChar._allowItemAutoEquip = !playerChar._allowItemAutoEquip;
        _saveService.QueueSave(this);
    }

    public void ToggleLockGagStorageOnGagLock() {
        playerChar._lockGagStorageOnGagLock = !playerChar._lockGagStorageOnGagLock;
        _saveService.QueueSave(this);
    }

    public void ToggleEnableRestraintSets(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._enableRestraintSets = !playerChar._uniquePlayerPerms[activeListIdx]._enableRestraintSets;
        _saveService.QueueSave(this);
    }

    public void ToggleRestraintSetLocking(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._restraintSetLocking = !playerChar._uniquePlayerPerms[activeListIdx]._restraintSetLocking;
        _saveService.QueueSave(this);
    }

    public void ToggleRestraintSetAutoEquip() {
        playerChar._allowRestraintSetAutoEquip = !playerChar._allowRestraintSetAutoEquip;
        _saveService.QueueSave(this);
    }

    public void TogglePuppeteer() {
        playerChar._allowPuppeteer = !playerChar._allowPuppeteer;
        _saveService.QueueSave(this);
    }

    public void ToggleAllowSitRequests(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowSitRequests = !playerChar._uniquePlayerPerms[activeListIdx]._allowSitRequests;
        _saveService.QueueSave(this);
    }

    public void ToggleAllowMotionRequests(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowMotionRequests = !playerChar._uniquePlayerPerms[activeListIdx]._allowMotionRequests;
        _saveService.QueueSave(this);
    }

    public void ToggleAllowAllCommands(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowAllCommands = !playerChar._uniquePlayerPerms[activeListIdx]._allowAllCommands;
        _saveService.QueueSave(this);
    }

    public void ToggleEnableToybox() {
        playerChar._enableToybox = !playerChar._enableToybox;
        _saveService.QueueSave(this);
    
    }

    public void ToggleAllowIntensityControl(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowIntensityControl = !playerChar._uniquePlayerPerms[activeListIdx]._allowIntensityControl;
        _saveService.QueueSave(this);
    }

    public void ToggleChangeToyState(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowChangingToyState = !playerChar._uniquePlayerPerms[activeListIdx]._allowChangingToyState;
        _saveService.QueueSave(this);
    }

    public void ToggleAllowPatternExecution(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowUsingPatterns = !playerChar._uniquePlayerPerms[activeListIdx]._allowUsingPatterns;
        _saveService.QueueSave(this);
    }

    public void ToggleAllowToyboxTriggers(int idx) {
        playerChar._uniquePlayerPerms[activeListIdx]._allowUsingTriggers = !playerChar._uniquePlayerPerms[activeListIdx]._allowUsingTriggers;
        _saveService.QueueSave(this);
    }

    public void ToggleToyboxUILocking() {
        playerChar._lockToyboxUI = !playerChar._lockToyboxUI;
        _saveService.QueueSave(this);
    }

    public void ToggleToyState() {
        playerChar._isToyActive = !playerChar._isToyActive;
        _saveService.QueueSave(this);
    }

    public void SetNewTriggerPhrase(string newPhrase) {
        playerChar._uniquePlayerPerms[activeListIdx]._triggerPhraseForPuppeteer = newPhrase;
        _saveService.QueueSave(this);
    }

    public void UpdateAllowSitRequests(bool value) {
        if(playerChar._uniquePlayerPerms[activeListIdx]._allowSitRequests != value) {
            playerChar._uniquePlayerPerms[activeListIdx]._allowSitRequests = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._uniquePlayerPerms[activeListIdx]._allowSitRequests = value;
        }
    }

    public void UpdateAllowMotionRequests(bool value) {
        if(playerChar._uniquePlayerPerms[activeListIdx]._allowMotionRequests != value) {
            playerChar._uniquePlayerPerms[activeListIdx]._allowMotionRequests = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._uniquePlayerPerms[activeListIdx]._allowMotionRequests = value;
        }
    }

    public void UpdateAllowAllCommands(bool value) {
        if(playerChar._uniquePlayerPerms[activeListIdx]._allowAllCommands != value) {
            playerChar._uniquePlayerPerms[activeListIdx]._allowAllCommands = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._uniquePlayerPerms[activeListIdx]._allowAllCommands = value;
        }
    }
    public void AddNewAliasEntry(AliasTrigger alias) {
        playerChar._triggerAliases[activeListIdx]._aliasTriggers.Add(alias);
        _saveService.QueueSave(this);
    }

    public void RemoveAliasEntry(int index) {
        playerChar._triggerAliases[activeListIdx]._aliasTriggers.RemoveAt(index);
        _saveService.QueueSave(this);
    }
    public void UpdateAliasEntryInput(int index, string newInput) {
        playerChar._triggerAliases[activeListIdx]._aliasTriggers[index]._inputCommand = newInput;
        _saveService.QueueSave(this);
    }

    public void UpdateAliasEntryOutput(int index, string newOutput) {
        playerChar._triggerAliases[activeListIdx]._aliasTriggers[index]._outputCommand = newOutput;
        _saveService.QueueSave(this);
    }

    public void UpdateAliasEntryEnabled(int index, bool value) {
        if(playerChar._triggerAliases[activeListIdx]._aliasTriggers[index]._enabled != value) {
            playerChar._triggerAliases[activeListIdx]._aliasTriggers[index]._enabled = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._triggerAliases[activeListIdx]._aliasTriggers[index]._enabled = value;
        }
    }

    public void SetNewStartCharForPuppeteerTrigger(string newStartChar) {
        playerChar._uniquePlayerPerms[activeListIdx]._StartCharForPuppeteerTrigger = newStartChar;
        _saveService.QueueSave(this);
    }

    public void SetNewEndCharForPuppeteerTrigger(string newEndChar) {
        playerChar._uniquePlayerPerms[activeListIdx]._EndCharForPuppeteerTrigger = newEndChar;
        _saveService.QueueSave(this);
    }
#endregion PlayerChar Handler Functions
}