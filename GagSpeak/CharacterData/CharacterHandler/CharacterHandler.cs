using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using GagSpeak.Utility;
using GagSpeak.Events;
using GagSpeak.Wardrobe;

namespace GagSpeak.CharacterData;
public partial class CharacterHandler : ISavable
{
    public PlayerGlobalPerms playerChar { get; protected set; }
    public List<WhitelistedCharacterInfo> whitelistChars { get; protected set; }
    // store the active whitelist index
    public int activeListIdx = 0;

    [JsonIgnore]
    private readonly SaveService _saveService;
    [JsonIgnore]
    private readonly GagSpeakGlamourEvent _gagSpeakGlamourEvent;
    [JsonIgnore]
    private readonly GagStorageManager _gagStorageManager;
    [JsonIgnore]
    private readonly InitializationManager _initializationManager;
    public CharacterHandler(SaveService saveService, GagSpeakGlamourEvent gagSpeakGlamourEvent,
    GagStorageManager gagStorageManager, InitializationManager initializationManager) {
        _saveService = saveService;
        _initializationManager = initializationManager;
        _gagSpeakGlamourEvent = gagSpeakGlamourEvent;
        _gagStorageManager = gagStorageManager;
        // initialize blank data
        playerChar = new PlayerGlobalPerms();
        whitelistChars = new List<WhitelistedCharacterInfo>();
        activeListIdx = 0;
        // load the information from our storage file stuff
        Load();
        // ensure all lists have the correct sizes
        _saveService.QueueSave(this);
        // let the initialization manager know that we have loaded the character handler
        GagSpeak.Log.Debug("======================== [ Completing CharacterHandler Initialization ] ========================");
        _initializationManager.CompleteStep(InitializationSteps.CharacterHandlerInitialized);
    }


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
        playerChar.AddNewWhitelistItemPerms();
        // save
        _saveService.QueueSave(this);
    }

    public void ReplaceWhitelistItem(int index, string playerName, string playerWorld) {
        whitelistChars[index] = new WhitelistedCharacterInfo(playerName, playerWorld);
        // update the player chars things to match the whitelist edit
        playerChar.ReplaceWhitelistItemPerms(index);
        // save
        _saveService.QueueSave(this);
    }

    public void RemoveWhitelistItem(int index) {
        whitelistChars.RemoveAt(index);
        // update the player chars things to match the whitelist edit
        playerChar.RemoveWhitelistItemPerms(index);
        // save
        _saveService.QueueSave(this);
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

    public bool HasEstablishedCommitment(int index) {
        if(whitelistChars[index]._yourStatusToThem != RoleLean.None && whitelistChars[index]._theirStatusToYou != RoleLean.None) {
            return true;
        }
        return false;
    }

    public bool IsLeanLesserThanPartner(int index) {
        // just return false if two way commitment is not yet established
        if(whitelistChars[index]._yourStatusToThem == RoleLean.None || whitelistChars[index]._theirStatusToYou == RoleLean.None) {
            return false;
        }
        // if the two way commitment is established, we can check if the leans are compatible
        if(whitelistChars[index]._yourStatusToThem < whitelistChars[index]._theirStatusToYou) {
            // your lean is less than your partners, so return true
            return true;
        }
        // lean is not less than partner's, so return false
        return false;
    }

    public void SetCommitmentTimeEstablished(int index) {
        whitelistChars[index].Set_timeOfCommitment();
        _saveService.QueueSave(this);
    }

    public bool CheckForPreventTimeRestart(int whitelistIdxToCheck, RoleLean newLeanYourStatusToThem, RoleLean newLeanTheirStatusToYou) {
        // firstly, we need to get the current leans of both and store them
        RoleLean curYourStatusToThem = whitelistChars[whitelistIdxToCheck]._yourStatusToThem;
        RoleLean curTheirStatusToYou = whitelistChars[whitelistIdxToCheck]._theirStatusToYou;
        // now we need to store if the two way commitment if made
        bool twoWayCommitmentMade = curYourStatusToThem != RoleLean.None && curTheirStatusToYou != RoleLean.None;
        // if it is the same, we can do checks, otherwise, return false
        if(twoWayCommitmentMade) {
            if(
                // your status to them remains dominant or remains submissive
                UIHelpers.IsRoleLeanDomHelper(curYourStatusToThem) && UIHelpers.IsRoleLeanDomHelper(newLeanYourStatusToThem)
                ||
                UIHelpers.IsRoleLeanSubHelper(curYourStatusToThem) && UIHelpers.IsRoleLeanSubHelper(newLeanYourStatusToThem)
                && // and their status to you remains dominant or remains submissive
                UIHelpers.IsRoleLeanDomHelper(curTheirStatusToYou) && UIHelpers.IsRoleLeanDomHelper(newLeanTheirStatusToYou)
                ||
                UIHelpers.IsRoleLeanSubHelper(curTheirStatusToYou) && UIHelpers.IsRoleLeanSubHelper(newLeanTheirStatusToYou)
            ) { // then we return true to prevent the timer.
                return true;
            }
            // otherwise we just return false;
        }
        return false;
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
                playerChar = new PlayerGlobalPerms();
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
        playerChar = new PlayerGlobalPerms();
        
        // Create a default WhitelistedCharacterInfo
        var defaultWhitelistUser = new WhitelistedCharacterInfo();
        whitelistChars.Add(defaultWhitelistUser);
        
        // Save the data
        _saveService.QueueSave(this);
    }

#endregion Json ISavable & Loads
}