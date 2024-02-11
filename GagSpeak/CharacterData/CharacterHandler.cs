using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Gagsandlocks;
using GagSpeak.Utility;
using GagSpeak.Events;

/*
/////////////////////////////////////////////////////////////////////////////////////////////////
///
///                    Warning for anyone viewing this class:
///                    
///       Yes, I am aware this is long and ugly. However, I need to update saveservice
///       whenever a value is changed, and I didnt want to just implement a lazy quicksave
///       function and rather practice protected setting.
///       
///       Because of this, i have split it up into regions. If I could have done it by partial
///       class split i would have, but at this time i'm too deprived of energy to do that.
/////////////////////////////////////////////////////////////////////////////////////////////////
*/

namespace GagSpeak.CharacterData;

public class CharacterHandler : ISavable
{
    public PlayerCharacterInfo playerChar { get; protected set; }
    public List<WhitelistedCharacterInfo> whitelistChars { get; protected set; }
    // store the active whitelist index
    public int activeListIdx = 0;

    [JsonIgnore]
    private readonly SaveService _saveService;
    [JsonIgnore]
    private readonly GagSpeakGlamourEvent _gagSpeakGlamourEvent;

    public CharacterHandler(SaveService saveService, GagSpeakGlamourEvent gagSpeakGlamourEvent) {
        _saveService = saveService;
        _gagSpeakGlamourEvent = gagSpeakGlamourEvent;
        // initialize blank data
        playerChar = new PlayerCharacterInfo();
        whitelistChars = new List<WhitelistedCharacterInfo>();
        activeListIdx = 0;
        // load the information from our storage file stuff
        Load();

        // ensure all lists have the correct sizes
    }
#region Gag Setters
    public void SetPlayerGagType(int index, string gagName) {
        if(playerChar._selectedGagTypes[index] != gagName) {
            playerChar._selectedGagTypes[index] = gagName;
            _saveService.QueueSave(this);
        }
        // see if we reset it to none, and if so, we should remove our glamoured gag item
        if(gagName == "None") {
            _gagSpeakGlamourEvent.Invoke(UpdateType.GagUnEquipped);
        }
    }

    public void SetPlayerGagPadlock(int index, string padlockName) {
        Padlocks lockToAssign = Enum.TryParse(padlockName, out Padlocks padlockType) ? padlockType : Padlocks.None;
        if(playerChar._selectedGagPadlocks[index] != lockToAssign) {
            playerChar._selectedGagPadlocks[index] = lockToAssign;
            _saveService.QueueSave(this);
        }
    }

    public void SetPlayerGagPadlock(int index, Padlocks padlockName) {
        if(playerChar._selectedGagPadlocks[index] != padlockName) {
            playerChar._selectedGagPadlocks[index] = padlockName;
            _saveService.QueueSave(this);
        }
    }

    public void SetPlayerGagPadlockPassword(int index, string password) {
        if(playerChar._selectedGagPadlockPassword[index] != password) {
            playerChar._selectedGagPadlockPassword[index] = password;
            _saveService.QueueSave(this);
        }
    }

    public void SetPlayerGagPadlockTimer(int index, string endTimeOfTimerLock) {
        DateTimeOffset value = UIHelpers.GetEndTime(endTimeOfTimerLock);
        if(playerChar._selectedGagPadlockTimer[index] != value) {
            playerChar._selectedGagPadlockTimer[index] = value;
            _saveService.QueueSave(this);
        }
    }

    public void ResetPlayerGagTimers() {
        playerChar._selectedGagPadlockTimer = new List<DateTimeOffset> {
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            DateTimeOffset.Now
        };
        _saveService.QueueSave(this);
    }

    public void SetPlayerGagPadlockAssigner(int index, string AssignerName) {
        if(playerChar._selectedGagPadlockAssigner[index] != AssignerName) {
            playerChar._selectedGagPadlockAssigner[index] = AssignerName;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagTypes(int index, int gagIndex, string gagName) {
        if(whitelistChars[index]._selectedGagTypes[gagIndex] != gagName) {
            whitelistChars[index]._selectedGagTypes[gagIndex] = gagName;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlocks(int index, int padlockIndex, string padlockName) {
        Padlocks lockToAssign = Enum.TryParse(padlockName, out Padlocks padlockType) ? padlockType : Padlocks.None;
        if(whitelistChars[index]._selectedGagPadlocks[padlockIndex] != lockToAssign) {
            whitelistChars[index]._selectedGagPadlocks[padlockIndex] = lockToAssign;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlocks(int index, int padlockIndex, Padlocks padlockName) {
        if(whitelistChars[index]._selectedGagPadlocks[padlockIndex] != padlockName) {
            whitelistChars[index]._selectedGagPadlocks[padlockIndex] = padlockName;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockPassword(int index, int passwordIndex, string password) {
        if(whitelistChars[index]._selectedGagPadlockPassword[passwordIndex] != password) {
            whitelistChars[index]._selectedGagPadlockPassword[passwordIndex] = password;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockTimer(int index, int timerIndex, string endTimeOfTimerLock) {
        DateTimeOffset value = UIHelpers.GetEndTime(endTimeOfTimerLock);
        if(whitelistChars[index]._selectedGagPadlockTimer[timerIndex] != value) {
            whitelistChars[index]._selectedGagPadlockTimer[timerIndex] = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockTimer(int index, int timerIndex, DateTimeOffset endTimeOfTimerLock) {
        if(whitelistChars[index]._selectedGagPadlockTimer[timerIndex] != endTimeOfTimerLock) {
            whitelistChars[index]._selectedGagPadlockTimer[timerIndex] = endTimeOfTimerLock;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockAssigner(int index, int assignerIndex, string AssignerName) {
        if(whitelistChars[index]._selectedGagPadlockAssigner[assignerIndex] != AssignerName) {
            whitelistChars[index]._selectedGagPadlockAssigner[assignerIndex] = AssignerName;
            _saveService.QueueSave(this);
        }
    }
#endregion Gag Setters

#region PlayerChar Handler Functions
    public void SetRevertStyle(RevertStyle style) {
        playerChar._revertStyle = style;
        _saveService.QueueSave(this);
    }

    public void UpdateIntensityLevel(int intensity) {
        if(playerChar._intensityLevel != intensity) {
            playerChar._intensityLevel = intensity;
            Save();
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

#region WhitelistSetters
    public void SetWhitelistSafewordUsed(int index, bool value) {
        if(whitelistChars[index]._safewordUsed != value) {
            whitelistChars[index]._safewordUsed = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistGrantExtendedLockTimes(int index, bool value) {
        if(whitelistChars[index]._grantExtendedLockTimes != value) {
            whitelistChars[index]._grantExtendedLockTimes = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistDirectChatGarblerActive(int index, bool value) {
        if(whitelistChars[index]._directChatGarblerActive != value) {
            whitelistChars[index]._directChatGarblerActive = value;
            _saveService.QueueSave(this);
        }
    }
    
    public void SetWhitelistDirectChatGarblerLocked(int index, bool value) {
        if(whitelistChars[index]._directChatGarblerLocked != value) {
            whitelistChars[index]._directChatGarblerLocked = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistEnableWardrobe(int index, bool value) {
        if(whitelistChars[index]._enableWardrobe != value) {
            whitelistChars[index]._enableWardrobe = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistLockGagStorageOnGagLock(int index, bool value) {
        if(whitelistChars[index]._lockGagStorageOnGagLock != value) {
            whitelistChars[index]._lockGagStorageOnGagLock = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistEnableRestraintSets(int index, bool value) {
        if(whitelistChars[index]._enableRestraintSets != value) {
            whitelistChars[index]._enableRestraintSets = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistRestraintSetLocking(int index, bool value) {
        if(whitelistChars[index]._restraintSetLocking != value) {
            whitelistChars[index]._restraintSetLocking = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowPuppeteer(int index, bool value) {
        if(whitelistChars[index]._allowPuppeteer != value) {
            whitelistChars[index]._allowPuppeteer = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistTriggerPhraseForPuppeteer(int index, string value) {
        if(whitelistChars[index]._theirTriggerPhrase != value) {
            whitelistChars[index]._theirTriggerPhrase = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistTriggerPhraseStartChar(int index, string value) {
        if(whitelistChars[index]._theirTriggerStartChar != value) {
            whitelistChars[index]._theirTriggerStartChar = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistTriggerPhraseEndChar(int index, string value) {
        if(whitelistChars[index]._theirTriggerEndChar != value) {
            whitelistChars[index]._theirTriggerEndChar = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowSitRequests(int index, bool value) {
        if(whitelistChars[index]._allowsSitRequests != value) {
            whitelistChars[index]._allowsSitRequests = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowMotionRequests(int index, bool value) {
        if(whitelistChars[index]._allowsMotionRequests != value) {
            whitelistChars[index]._allowsMotionRequests = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowAllCommands(int index, bool value) {
        if(whitelistChars[index]._allowsAllCommands != value) {
            whitelistChars[index]._allowsAllCommands = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistEnableToybox(int index, bool value) {
        if(whitelistChars[index]._enableToybox != value) {
            whitelistChars[index]._enableToybox = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowChangingToyState(int index, bool value) {
        if(whitelistChars[index]._allowChangingToyState != value) {
            whitelistChars[index]._allowChangingToyState = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowIntensityControl(int index, bool value) {
        if(whitelistChars[index]._allowsIntensityControl != value) {
            whitelistChars[index]._allowsIntensityControl = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistIntensityLevel(int index, byte value) {
        if(whitelistChars[index]._intensityLevel != value) {
            whitelistChars[index]._intensityLevel = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowUsingPatterns(int index, bool value) {
        if(whitelistChars[index]._allowsUsingPatterns != value) {
            whitelistChars[index]._allowsUsingPatterns = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowToyboxLocking(int index, bool value) {
        if(whitelistChars[index]._lockToyboxUI != value) {
            whitelistChars[index]._lockToyboxUI = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistToyIsActive(int index, bool value) {
        if(whitelistChars[index]._isToyActive != value) {
            whitelistChars[index]._isToyActive = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistToyStepSize(int index, int value) {
        if(whitelistChars[index]._activeToystepSize != value) {
            whitelistChars[index]._activeToystepSize = value;
            _saveService.QueueSave(this);
        }
    }


    // for not spamming the save service during info request transfer, do it all in bulk
    public void UpdateWhitelistGagInfo(string whitelistName, string[] GagTypes, string[] Padlocks, string[] Passwords, string[] Timers, string[] Assigners) {
        int Idx = -1;
        if(IsPlayerInWhitelist(whitelistName)){
            Idx = GetWhitelistIndex(whitelistName);
        }
        if(Idx != -1) {
            whitelistChars[Idx]._selectedGagTypes[0] = GagTypes[0];
            whitelistChars[Idx]._selectedGagTypes[1] = GagTypes[1];
            whitelistChars[Idx]._selectedGagTypes[2] = GagTypes[2];
            whitelistChars[Idx]._selectedGagPadlocks[0] = Enum.TryParse(Padlocks[0], out Padlocks padlockType) ? padlockType : Gagsandlocks.Padlocks.None;
            whitelistChars[Idx]._selectedGagPadlocks[1] = Enum.TryParse(Padlocks[1], out Padlocks padlockType2) ? padlockType2 : Gagsandlocks.Padlocks.None;
            whitelistChars[Idx]._selectedGagPadlocks[2] = Enum.TryParse(Padlocks[2], out Padlocks padlockType3) ? padlockType3 : Gagsandlocks.Padlocks.None;
            whitelistChars[Idx]._selectedGagPadlockPassword[0] = Passwords[0];
            whitelistChars[Idx]._selectedGagPadlockPassword[1] = Passwords[1];
            whitelistChars[Idx]._selectedGagPadlockPassword[2] = Passwords[2];
            whitelistChars[Idx]._selectedGagPadlockTimer[0] = UIHelpers.GetEndTime(Timers[0]);
            whitelistChars[Idx]._selectedGagPadlockTimer[1] = UIHelpers.GetEndTime(Timers[1]);
            whitelistChars[Idx]._selectedGagPadlockTimer[2] = UIHelpers.GetEndTime(Timers[2]);
            whitelistChars[Idx]._selectedGagPadlockAssigner[0] = Assigners[0];
            whitelistChars[Idx]._selectedGagPadlockAssigner[1] = Assigners[1];
            whitelistChars[Idx]._selectedGagPadlockAssigner[2] = Assigners[2]; 
        }
        _saveService.QueueSave(this);
    }

#endregion WhitelistSetters

#region Whitelist Handler Functions
    public bool IsPlayerInWhitelist(string playerName) {
        return whitelistChars.Any(x => x._name == playerName);
    }

    public int GetWhitelistIndex(string playerName) {
        return whitelistChars.FindIndex(x => x._name == playerName);
    }

    public bool IsIndexWithinBounds(int index) {
        return index >= 0 && index < whitelistChars.Count;
    }

    public void AddNewWhitelistItem(string playerName, string playerWorld) {
        whitelistChars.Add(new WhitelistedCharacterInfo(playerName, playerWorld));
        // update the player chars things to match the whitelist edit
        playerChar._triggerAliases.Add(new AliasList());
        playerChar._grantExtendedLockTimes.Add(false);
        playerChar._enableRestraintSets.Add(false);
        playerChar._restraintSetLocking.Add(false);
        playerChar._triggerPhraseForPuppeteer.Add("");
        playerChar._StartCharForPuppeteerTrigger.Add("");
        playerChar._EndCharForPuppeteerTrigger.Add("");
        playerChar._allowSitRequests.Add(false);
        playerChar._allowMotionRequests.Add(false);
        playerChar._allowAllCommands.Add(false);
        playerChar._allowChangingToyState.Add(false);
        playerChar._allowIntensityControl.Add(false);
        playerChar._allowUsingPatterns.Add(false);
        // do a quicksave (happens on the next framework tick, very fast)
        EnsureListSizes();
        _saveService.QueueSave(this);
    }

    public void ReplaceWhitelistItem(int index, string playerName, string playerWorld) {
        whitelistChars[index] = new WhitelistedCharacterInfo(playerName, playerWorld);
        // update the player chars things to match the whitelist edit
        playerChar._triggerAliases[index] = new AliasList();
        playerChar._grantExtendedLockTimes[index] = false;
        playerChar._enableRestraintSets[index] = false;
        playerChar._restraintSetLocking[index] = false;
        playerChar._triggerPhraseForPuppeteer[index] = "";
        playerChar._StartCharForPuppeteerTrigger[index] = "(";
        playerChar._EndCharForPuppeteerTrigger[index] = ")";
        playerChar._allowSitRequests[index] = false;
        playerChar._allowMotionRequests[index] = false;
        playerChar._allowAllCommands[index] = false;
        playerChar._allowChangingToyState[index] = false;
        playerChar._allowIntensityControl[index] = false;
        playerChar._allowUsingPatterns[index] = false;
        // do a quicksave (happens on the next framework tick, very fast)
        EnsureListSizes();
        _saveService.QueueSave(this);
    }

    public void RemoveWhitelistItem(int index) {
        whitelistChars.RemoveAt(index);
        // update the player chars things to match the whitelist edit
        playerChar._triggerAliases.RemoveAt(index);
        playerChar._grantExtendedLockTimes.RemoveAt(index);
        playerChar._enableRestraintSets.RemoveAt(index);
        playerChar._restraintSetLocking.RemoveAt(index);
        playerChar._triggerPhraseForPuppeteer.RemoveAt(index);
        playerChar._StartCharForPuppeteerTrigger.RemoveAt(index);
        playerChar._EndCharForPuppeteerTrigger.RemoveAt(index);
        playerChar._allowSitRequests.RemoveAt(index);
        playerChar._allowMotionRequests.RemoveAt(index);
        playerChar._allowAllCommands.RemoveAt(index);
        playerChar._allowChangingToyState.RemoveAt(index);
        playerChar._allowIntensityControl.RemoveAt(index);
        playerChar._allowUsingPatterns.RemoveAt(index);
        // do a quicksave (happens on the next framework tick, very fast)
        EnsureListSizes();
        _saveService.QueueSave(this);
    }

    public void EnsureListSizes() {
        int targetSize = whitelistChars.Count;

        // Helper function to resize a list to the target size
        Action<List<AliasList>, AliasList> resizeAliasList = (list, defaultValue) => {
            int currentSize = list.Count;

            if (currentSize < targetSize) {
                // If the list is too small, add default elements
                for (int i = currentSize; i < targetSize; i++) {
                    list.Add(defaultValue);
                }
            } else if (currentSize > targetSize) {
                // If the list is too large, remove elements from the end
                list.RemoveRange(targetSize, currentSize - targetSize);
            }
        };

        Action<List<bool>, bool> resizeBoolList = (list, defaultValue) => {
            int currentSize = list.Count;

            if (currentSize < targetSize) {
                for (int i = currentSize; i < targetSize; i++) {
                    list.Add(defaultValue);
                }
            } else if (currentSize > targetSize) {
                list.RemoveRange(targetSize, currentSize - targetSize);
            }
        };

        Action<List<string>, string> resizeStringList = (list, defaultValue) => {
            int currentSize = list.Count;

            if (currentSize < targetSize) {
                for (int i = currentSize; i < targetSize; i++) {
                    list.Add(defaultValue);
                }
            } else if (currentSize > targetSize) {
                list.RemoveRange(targetSize, currentSize - targetSize);
            }
        };

        // Resize all the lists in playerChar
        resizeAliasList(playerChar._triggerAliases, new AliasList());
        resizeBoolList(playerChar._grantExtendedLockTimes, false);
        resizeBoolList(playerChar._enableRestraintSets, false);
        resizeBoolList(playerChar._restraintSetLocking, false);
        resizeStringList(playerChar._triggerPhraseForPuppeteer, "");
        resizeStringList(playerChar._StartCharForPuppeteerTrigger, "(");
        resizeStringList(playerChar._EndCharForPuppeteerTrigger, ")");
        resizeBoolList(playerChar._allowSitRequests, false);
        resizeBoolList(playerChar._allowMotionRequests, false);
        resizeBoolList(playerChar._allowAllCommands, false);
        resizeBoolList(playerChar._allowChangingToyState, false);
        resizeBoolList(playerChar._allowIntensityControl, false);
        resizeBoolList(playerChar._allowUsingPatterns, false);
    }

    public void UpdateYourStatusToThem(int index, RoleLean role) {
        whitelistChars[index]._yourStatusToThem = role;
        _saveService.QueueSave(this);
    }

    public void UpdateTheirStatusToYou(int index, RoleLean role) {
        whitelistChars[index]._theirStatusToYou = role;
        _saveService.QueueSave(this);
    }

    public void UpdatePendingRelationRequestFromYou(int index, RoleLean role) {
        whitelistChars[index]._pendingRelationRequestFromYou = role;
        _saveService.QueueSave(this);
    }

    public void UpdatePendingRelationRequestFromPlayer(int index, RoleLean role) {
        whitelistChars[index]._pendingRelationRequestFromPlayer = role;
        _saveService.QueueSave(this);
    }

    public void SetCommitmentTimeEstablished(int index) {
        whitelistChars[index].Set_timeOfCommitment();
        _saveService.QueueSave(this);
    }

    public DynamicTier GetDynamicTierClient(string playerName) {
        return whitelistChars
                .FirstOrDefault(x => x._name == playerName)
                ?.GetDynamicTierClient() ?? DynamicTier.Tier0;
    }

    public DynamicTier GetDynamicTierNonClient(string playerName) {
        return whitelistChars
                .FirstOrDefault(x => x._name == playerName)
                ?.GetDynamicTierNonClient() ?? DynamicTier.Tier0;
    }

    public string GetRoleLeanString(RoleLean role) => role.ToString();

    public RoleLean GetRoleLeanFromString(string roleLeanString) {
        if (Enum.TryParse(roleLeanString, out RoleLean roleLean)) {
            return roleLean;
        } else {
            throw new ArgumentException($"Invalid RoleLean value: {roleLeanString}");
        }
    }
#endregion Whitelist Handler Functions

#region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.CharacterDataFile;

    public void Save(StreamWriter writer)
    {
        using var j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;
        Serialize().WriteTo(j);
    }

    public void Quicksave()
        => _saveService.QueueSave(this);

    public void Save()
        => _saveService.DelaySave(this);

    public JObject Serialize() {
        // create a new array for serialization
        var array = new JArray();
        // for each of our restraint sets, serialize them and add them to the array
        foreach (var listedCharacter in whitelistChars) {
            array.Add(listedCharacter.Serialize());
        }
        // return the new object under the label "RestraintSets"
        return new JObject() {
            ["SelectedIdx"] = activeListIdx,
            ["PlayerCharacterData"] = playerChar.Serialize(),
            ["WhitelistData"] = array,
        };
    }

    public void Load() {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        var file = _saveService.FileNames.CharacterDataFile;
        whitelistChars.Clear();
        if (!File.Exists(file)) {
            CreateNewFile();
            GagSpeak.Log.Debug($"[CharacterHandler] CharacterData.json not found! Creating new file.");
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            activeListIdx = jsonObject["SelectedIdx"]?.Value<int>() ?? 0;
            // Deserialize PlayerCharacterData
            var playerCharacterData = jsonObject["PlayerCharacterData"]?.Value<JObject>();
            if (playerCharacterData != null) {
                playerChar = new PlayerCharacterInfo();
                playerChar.Deserialize(playerCharacterData);
            }
            var whitelistCharsArray = jsonObject["WhitelistData"].Value<JArray>();
            foreach (var item in whitelistCharsArray) {
                var listedCharacter = new WhitelistedCharacterInfo();
                listedCharacter.Deserialize(item.Value<JObject>());
                whitelistChars.Add(listedCharacter);
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"Failure to load Whitelisted Data: Error during parsing. {ex}");
        } finally {
            GagSpeak.Log.Debug($"[CharacterHandler] CharacterData.json loaded! Loaded {whitelistChars.Count} the whitelist.");
        }
        //#pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }

    public void CreateNewFile() {
        // create a new charater data file
        playerChar = new PlayerCharacterInfo();
        
        // Create a default WhitelistedCharacterInfo
        var defaultWhitelistUser = new WhitelistedCharacterInfo();
        whitelistChars.Add(defaultWhitelistUser);
        
        // Save the data
        _saveService.QueueSave(this);
    }

#endregion Json ISavable & Loads
}