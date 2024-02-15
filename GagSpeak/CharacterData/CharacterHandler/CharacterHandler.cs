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

public partial class CharacterHandler : ISavable
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
        playerChar._StartCharForPuppeteerTrigger.Add("(");
        playerChar._EndCharForPuppeteerTrigger.Add(")");
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