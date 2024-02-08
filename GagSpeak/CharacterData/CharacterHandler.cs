using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using GagSpeak.ToyboxandPuppeteer;

namespace GagSpeak.CharacterData;

public class CharacterHandler : ISavable
{
    public PlayerCharacterInfo playerChar;
    public List<WhitelistedCharacterInfo> whitelistChars = [];
    // store the active whitelist index
    public int activeListIdx = 0;

    [JsonIgnore]
    private readonly SaveService _saveService;

    public CharacterHandler(SaveService saveService) {
        _saveService = saveService;
        // initialize blank data
        playerChar = new PlayerCharacterInfo();
        whitelistChars = new List<WhitelistedCharacterInfo>();
        activeListIdx = 0;
        // load the information from our storage file stuff
        Load();
    }
#region Config Settings
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

    public void ToggleRestraintSetAutoEquip() {
        playerChar._allowRestraintSetAutoEquip = !playerChar._allowRestraintSetAutoEquip;
        _saveService.QueueSave(this);
    }

    public void TogglePuppeteer() {
        playerChar._allowPuppeteer = !playerChar._allowPuppeteer;
        _saveService.QueueSave(this);
    }

    public void ToggleEnableToybox() {
        playerChar._enableToybox = !playerChar._enableToybox;
        _saveService.QueueSave(this);
    
    }

    public void ToggleAllowIntensityControl() {
        playerChar._allowIntensityControl = !playerChar._allowIntensityControl;
        _saveService.QueueSave(this);
    }

    public void ToggleChangeToyState() {
        playerChar._allowChangingToyState[activeListIdx] = !playerChar._allowChangingToyState[activeListIdx];
        _saveService.QueueSave(this);
    }

    public void ToggleAllowPatternExecution() {
        playerChar._allowUsingPatterns[activeListIdx] = !playerChar._allowUsingPatterns[activeListIdx];
        _saveService.QueueSave(this);
    }

    public void ToggleToyboxUILocking() {
        playerChar._allowToyboxLocking = !playerChar._allowToyboxLocking;
        _saveService.QueueSave(this);
    }

    
#endregion Config Settings
#region PlayerChar Handler Functions
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
        playerChar._grantExtendedLockTimes.Add(false);
        playerChar._triggerPhraseForPuppeteer.Add("");
        playerChar._allowSitRequests.Add(false);
        playerChar._allowMotionRequests.Add(false);
        playerChar._allowAllCommands.Add(false);
        playerChar._allowChangingToyState.Add(false);
        playerChar._allowUsingPatterns.Add(false);
        // do a quicksave (happens on the next framework tick, very fast)
        _saveService.QueueSave(this);
    }

    public void ReplaceWhitelistItem(int index, string playerName, string playerWorld) {
        whitelistChars[index] = new WhitelistedCharacterInfo(playerName, playerWorld);
        // update the player chars things to match the whitelist edit
        playerChar._grantExtendedLockTimes[index] = false;
        playerChar._triggerPhraseForPuppeteer[index] = "";
        playerChar._allowSitRequests[index] = false;
        playerChar._allowMotionRequests[index] = false;
        playerChar._allowAllCommands[index] = false;
        playerChar._allowChangingToyState[index] = false;
        playerChar._allowUsingPatterns[index] = false;
        // do a quicksave (happens on the next framework tick, very fast)
        _saveService.QueueSave(this);
    }

    public void RemoveWhitelistItem(int index) {
        whitelistChars.RemoveAt(index);
        // update the player chars things to match the whitelist edit
        playerChar._grantExtendedLockTimes.RemoveAt(index);
        playerChar._triggerPhraseForPuppeteer.RemoveAt(index);
        playerChar._allowSitRequests.RemoveAt(index);
        playerChar._allowMotionRequests.RemoveAt(index);
        playerChar._allowAllCommands.RemoveAt(index);
        playerChar._allowChangingToyState.RemoveAt(index);
        playerChar._allowUsingPatterns.RemoveAt(index);
        // do a quicksave (happens on the next framework tick, very fast)
        _saveService.QueueSave(this);
    }

    public DynamicTier GetDynamicTier(string playerName) {
        return whitelistChars
                .FirstOrDefault(x => x._name == playerName)
                ?.GetDynamicTier() ?? DynamicTier.Tier0;
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
#endregion Json ISavable & Loads
}