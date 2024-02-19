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
        playerChar._grantExtendedLockTimes[activeListIdx] = !playerChar._grantExtendedLockTimes[activeListIdx];
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
        playerChar._enableRestraintSets[idx] = !playerChar._enableRestraintSets[idx];
        _saveService.QueueSave(this);
    }

    public void ToggleRestraintSetLocking(int idx) {
        playerChar._restraintSetLocking[idx] = !playerChar._restraintSetLocking[idx];
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
        playerChar._allowSitRequests[idx] = !playerChar._allowSitRequests[idx];
        _saveService.QueueSave(this);
    }

    public void ToggleAllowMotionRequests(int idx) {
        playerChar._allowMotionRequests[idx] = !playerChar._allowMotionRequests[idx];
        _saveService.QueueSave(this);
    }

    public void ToggleAllowAllCommands(int idx) {
        playerChar._allowAllCommands[idx] = !playerChar._allowAllCommands[idx];
        _saveService.QueueSave(this);
    }

    public void ToggleEnableToybox() {
        playerChar._enableToybox = !playerChar._enableToybox;
        _saveService.QueueSave(this);
    
    }

    public void ToggleAllowIntensityControl(int idx) {
        playerChar._allowIntensityControl[idx] = !playerChar._allowIntensityControl[idx];
        _saveService.QueueSave(this);
    }

    public void ToggleChangeToyState(int idx) {
        playerChar._allowChangingToyState[idx] = !playerChar._allowChangingToyState[idx];
        _saveService.QueueSave(this);
    }

    public void ToggleAllowPatternExecution(int idx) {
        playerChar._allowUsingPatterns[idx] = !playerChar._allowUsingPatterns[idx];
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
        playerChar._triggerPhraseForPuppeteer[activeListIdx] = newPhrase;
        _saveService.QueueSave(this);
    }

    public void UpdateAllowSitRequests(bool value) {
        if(playerChar._allowSitRequests[activeListIdx] != value) {
            playerChar._allowSitRequests[activeListIdx] = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._allowSitRequests[activeListIdx] = value;
        }
    }

    public void UpdateAllowMotionRequests(bool value) {
        if(playerChar._allowMotionRequests[activeListIdx] != value) {
            playerChar._allowMotionRequests[activeListIdx] = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._allowMotionRequests[activeListIdx] = value;
        }
    }

    public void UpdateAllowAllCommands(bool value) {
        if(playerChar._allowAllCommands[activeListIdx] != value) {
            playerChar._allowAllCommands[activeListIdx] = value;
            _saveService.QueueSave(this);
        }
        else {
            playerChar._allowAllCommands[activeListIdx] = value;
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
        playerChar._StartCharForPuppeteerTrigger[activeListIdx] = newStartChar;
        _saveService.QueueSave(this);
    }

    public void SetNewEndCharForPuppeteerTrigger(string newEndChar) {
        playerChar._EndCharForPuppeteerTrigger[activeListIdx] = newEndChar;
        _saveService.QueueSave(this);
    }
#endregion PlayerChar Handler Functions
}