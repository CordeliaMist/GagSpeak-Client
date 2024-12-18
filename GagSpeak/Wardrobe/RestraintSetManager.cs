using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Penumbra.GameData.Structs;
using GagSpeak.Events;
using Dalamud.Plugin.Services;
using System.Threading.Tasks;
using GagSpeak.Interop.Penumbra;
using GagSpeak.Hardcore;

namespace GagSpeak.Wardrobe;

public class RestraintSetManager : ISavable, IDisposable
{
    public const int RestraintSetsFileVersion = 1;
    public List<RestraintSet> _restraintSets = []; // stores the restraint sets
    public int _selectedIdx = 0;

    [JsonIgnore]
    private readonly SaveService _saveService;
    [JsonIgnore]
    private readonly IClientState _clientState;
    [JsonIgnore]
    private readonly GagSpeakGlamourEvent _glamourEvent;
    [JsonIgnore]
    private readonly RS_ListChanged _restraintSetListChanged;
    [JsonIgnore]
    private readonly RS_ToggleEvent _RS_ToggleEvent;
    [JsonIgnore]
    private readonly HotbarLocker _hotbarLocker;
    [JsonIgnore]
    private readonly InitializationManager _manager;
    
    public RestraintSetManager(SaveService saveService, GagSpeakGlamourEvent gagSpeakGlamourEvent,
    InitializationManager initializationManager, RS_ListChanged restraintSetListChanged,
    RS_ToggleEvent RS_ToggleEvent, IClientState clientState, HotbarLocker hotbarLocker) {
        _manager = initializationManager;
        _saveService = saveService;
        _glamourEvent = gagSpeakGlamourEvent;
        _hotbarLocker = hotbarLocker;
        _restraintSetListChanged = restraintSetListChanged;
        _RS_ToggleEvent = RS_ToggleEvent;
        _clientState = clientState;
        //_penumbra = penumbra;
        GSLogger.LogType.Debug($"[RestraintSetManager] Attempting to load restraint sets p1");
        // load the information from our storage file
        Load();
        // if the load failed, meaning our _restraintSets is empty, then we need to add a default set
        if (_restraintSets == null || !_restraintSets.Any()) {
            _restraintSets = new List<RestraintSet> { new RestraintSet() };
        }
        // correctly account for any non-updated timed locked sets
        foreach (var set in _restraintSets) {
            if (set._locked && set._lockedTimer < DateTimeOffset.Now) {
                set._locked = false;
            }
        }
        // save to the file 
        Save();
        // wait for character handler to finish initializing, then do updates to our options requiring the character handler.
        _manager.CharacterHandlerInitialized += OnStepCompleted;
        // ready for the next step
        _manager._rsManagerReadyForEvent.SetResult(true);
    }
#region ConstructorOrder
    // fired when our character handler finishes loading, aka this is the 2nd main component that should load
    private void OnStepCompleted() {
        GSLogger.LogType.Information("  Completing Restraint Set Manager Initialization ");
        // if any of our sets are enabled, then prime the events
        MonitorLoginAndInvokeEvent();
    }
    public void Dispose() {
        _manager.CharacterHandlerInitialized -= OnStepCompleted;
    }
#endregion ConstructorOrder
#region Handle On-Login
    public void MonitorLoginAndInvokeEvent() {
        Task.Run(async () =>
        {
            if(!IsPlayerLoggedIn()) {
                GSLogger.LogType.Debug($"[RestraintSetManager] Waiting for login to complete before activating restraint set");
                while (!_clientState.IsLoggedIn || _clientState.LocalPlayer == null || _clientState.LocalPlayer.Address == IntPtr.Zero) {
                    await Task.Delay(1000); // Wait for 1 second before checking the login status again
                }
                // once we are logged in, delay for some time before load so we let glamourer load any automation, so that concurrency issues dont occur
                await Task.Delay(4000);
                // set the hotbar lock to its current state
                _hotbarLocker.SetHotbarStateToCurrentState();
            }
            // do update checks for active sets
            if (_restraintSets.Any(set => set._enabled)) {
                // get the active index of the set that is enabled
                int activeIdx = _restraintSets.FindIndex(set => set._enabled);
                _glamourEvent.Invoke(UpdateType.UpdateRestraintSet);
                _RS_ToggleEvent.Invoke(RestraintSetToggleType.Enabled, activeIdx, "self");
            }
            // still invoke the hardcore manager setup
            _manager.CompleteStep(InitializationSteps.RS_ManagerInitialized);
        });
    }

    private bool IsPlayerLoggedIn() => _clientState.IsLoggedIn && _clientState.LocalPlayer != null && _clientState.LocalPlayer.Address != IntPtr.Zero;
#endregion Handle On-Login

#region Manager Methods

    public int GetSelectedIdx() => _selectedIdx;

    public void SetSelectedIdx(int idx) {
        _selectedIdx = idx;
        _saveService.QueueSave(this);
    }

    public int GetRestraintSetIndex(string setName) {
        // see if the set exists in our list of sets, and if it does, return the index
        for (int i = 0; i < _restraintSets.Count; i++) {
            if (_restraintSets[i]._name == setName) {
                return i;
            }
        }
        return -1; // Return -1 if the set name is not found
    }

    /// <summary> Checks if any mods are associated with the restraint set requuires a toggle. </summary>
    public bool AnyModRequiresRedraw(int restraintSetIdxToCheck) {
        if(_restraintSets[restraintSetIdxToCheck]._associatedMods.Any(mod => mod.redrawAfterToggle)) {
            return true;
        }
        return false;
    }
    
    public void AddNewRestraintSet() {
        var newSet = new RestraintSet();
        string baseName = newSet._name;
        int copyNumber = 1;
        while (_restraintSets.Any(set => set._name == newSet._name)) {
            newSet.ChangeSetName(baseName + $"(copy{copyNumber})");
            copyNumber++;
        }
        _restraintSets.Add(newSet);
        // invoke the event to update character info
        _restraintSetListChanged.Invoke(ListUpdateType.AddedRestraintSet, _restraintSets.Count - 1);
        _saveService.QueueSave(this);
    }

    /// <summary> Deletes a restraint set spesified by index if it exists. </summary>
    public void DeleteRestraintSet(int index) {
        // delete a restraint set spesified by index if it exists
        if (index >= 0 && index < _restraintSets.Count) {
            _restraintSets.RemoveAt(index);
            // invoke the event to update character info
            _restraintSetListChanged.Invoke(ListUpdateType.RemovedRestraintSet, index);
            _saveService.QueueSave(this);
        }
    }

    /// <summary> Renames a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetName(int restraintSetIdx, string newName) {
        // Check if a set with the same name already exists
        if (_restraintSets.Any(set => _restraintSets[restraintSetIdx]._name == newName)) {
            // If it does, append "copy" to the new set name
            newName += "(copy)";
        }
        // append the new name       
        _restraintSets[restraintSetIdx].ChangeSetName(newName);
        // invoke the event to update character info
        _restraintSetListChanged.Invoke(ListUpdateType.NameChanged, restraintSetIdx);
        _saveService.QueueSave(this);
        // (will remove old set but transfer all info to newly serialized one)
    }

    /// <summary> Changes the description of a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetDescription(int restraintSetIdx, string newDescription) {
        _restraintSets[restraintSetIdx].ChangeSetDescription(newDescription);
        Save();
    }

    // see if any sets are currently locked
    public bool AreAnySetsLocked() {
        foreach (var set in _restraintSets) {
            if (set._locked) {
                return true;
            }
        }
        return false;
    }

    /// <summary> Looks to see if any restraint set is enabled. 
    /// <para> WARNING: THIS FUNCTION WILL STILL RETURN TRUE IF THE ASSIGNER IS SELF, UNLIKE HARDCORE MANAGER ONE </para>
    /// </summary>
    /// <returns> true if a set is enabled, false if not. The index of the enabled set is passed out </returns>
    public bool IsAnySetEnabled(out int enabledIdx, out string assignerOfSet) {
        // fetch the idx, we do this every time so that if someone changes their whitelist while a set is active, this will update to still match them.
        enabledIdx = _restraintSets.FindIndex(set => set._enabled);
        // get the person who enabled it
        if(enabledIdx == -1) {
            assignerOfSet = "INVALID";
            return false;
        } else {
            assignerOfSet = _restraintSets[enabledIdx]._wasEnabledBy;
            //GSLogger.LogType.Debug($"[RestraintSetManager] Found an enabled RestraintSet: {enabledIdx}, enabled by {assignerOfSet}");
            return true;
        }
    }

    /// <summary> Sets the IsEnabled for a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetState(int restraintSetIdx, bool isEnabled, string assignerName = "self") {
        // if we are wanting to enable this set, be sure to disable all other sets first
        if(isEnabled) {
            // we want to set this to true, so first disable all other sets
            for (int i = 0; i < _restraintSets.Count; i++) {
                if (_restraintSets[i]._enabled) {
                    GSLogger.LogType.Debug($"[RestraintSetManager] Disabling set {i} before enabling set {restraintSetIdx}");
                    _restraintSets[i].SetIsEnabled(false, assignerName);
                    // invoke the toggledSet event so we know which set was disabled, to send off to the hardcore panel
                    _glamourEvent.Invoke(UpdateType.DisableRestraintSet, "None", "", i); // center two values are placeholders because im tired of this shit
                    _RS_ToggleEvent.Invoke(RestraintSetToggleType.Disabled, i, assignerName);
                }
            }
            GSLogger.LogType.Debug($"[RestraintSetManager] Enabling set {restraintSetIdx}");
            // then set this one to true
            _restraintSets[restraintSetIdx].SetIsEnabled(true, assignerName);
            // and update our restraint set
            _glamourEvent.Invoke(UpdateType.UpdateRestraintSet);
            // invoke the toggledSet event so we know which set was enabled, to send off to the hardcore panel
            _RS_ToggleEvent.Invoke(RestraintSetToggleType.Enabled, restraintSetIdx, assignerName);         
        }
        // OTHERWISE, we want to set it to false, so just disable it 
        else {
            // disable it
            GSLogger.LogType.Debug($"[RestraintSetManager] Disabling set {restraintSetIdx}");
            _restraintSets[restraintSetIdx].SetIsEnabled(isEnabled, assignerName);
            // then fire a disable restraint set event to revert to automation
            _glamourEvent.Invoke(UpdateType.DisableRestraintSet, "None", "", restraintSetIdx);
            // invoke the toggledSet event so we know which set was disabled, to send off to the hardcore panel
            _RS_ToggleEvent.Invoke(RestraintSetToggleType.Disabled, restraintSetIdx, assignerName);
        }
        _saveService.QueueSave(this);
    }



    /// <summary> Toggle the enabled state of a slot piece in a spesified restraint set if it exists. </summary>
    public void ToggleRestraintSetPieceEnabledState(int restraintSetIdx, EquipSlot slot) {
        // get the current state
        bool currentState = _restraintSets[restraintSetIdx]._drawData[slot]._isEnabled;
        GSLogger.LogType.Debug($"[RestraintSetManager] Toggled {slot} visibility ({!currentState})");
        // invert the state
        _restraintSets[restraintSetIdx].SetPieceIsEnabled(slot, !currentState);
        // save the set
        _saveService.QueueSave(this);
    }

    public bool GetIsRestraintSetPieceEnabled(int restraintSetIdx, EquipSlot slot) {
        return _restraintSets[restraintSetIdx]._drawData[slot]._isEnabled;
    }

    public void LockRestraintSet(int restraintSetIdx, string wasLockedBy = "") {
        // if the set is not enabled, then you cant lock it
        if (!_restraintSets[restraintSetIdx]._enabled) {
            GSLogger.LogType.Debug($"[RestraintSetManager] Cannot lock a disabled set!");
            return;
        }
        _restraintSets[restraintSetIdx].SetIsLocked(true, wasLockedBy);
        _saveService.QueueSave(this);
    }

    public bool TryUnlockRestraintSet(int restraintSetIdx, string UnlockerName = "") {
        // if the set is not locked, then you cant unlock it
        if (!_restraintSets[restraintSetIdx]._locked) {
            GSLogger.LogType.Debug($"[RestraintSetManager] Cannot unlock an unlocked set!");
            return false;
        }
        // if the set is not enabled, then you cant unlock it
        if (!_restraintSets[restraintSetIdx]._enabled) {
            GSLogger.LogType.Debug($"[RestraintSetManager] Cannot unlock a disabled set!");
            return false;
        }
        // if the set is locked by someone else, then you cant unlock it
        if (_restraintSets[restraintSetIdx]._wasLockedBy != UnlockerName
        && _restraintSets[restraintSetIdx]._wasLockedBy != "self"
        && _restraintSets[restraintSetIdx]._wasLockedBy != "")
        {
            GSLogger.LogType.Debug($"[RestraintSetManager] Cannot unlock a set locked by someone else!");
            return false;
        }
        _restraintSets[restraintSetIdx].SetIsLocked(false);
        _saveService.QueueSave(this);
        return true;
    }
#endregion Manager Methods
#region ModAssociations
    /// <summary> Add an associated mod to a design. </summary>
    public void AddMod(int setIndex, Mod mod, ModSettings settings) {
        var modTuple = (mod, settings, false, false);
        if (_restraintSets[setIndex]._associatedMods.Any(x => x.Item1 == mod && x.Item2 == settings)) {
            return;
        }
        _restraintSets[setIndex]._associatedMods.Add(modTuple);
        Save();
        GSLogger.LogType.Debug($"Added associated mod {mod.DirectoryName} to Restraint Set: "+
        $"{_restraintSets[setIndex]._name}.");
    }

    /// <summary> Remove an associated mod from a design. </summary>
    public void RemoveMod(int setIndex, Mod mod) {
        var modTuple = _restraintSets[setIndex]._associatedMods.FirstOrDefault(x => x.mod == mod);
        if (modTuple == default) {
            return;
        }
        _restraintSets[setIndex]._associatedMods.Remove(modTuple);
        Save();
        GSLogger.LogType.Debug($"Removed associated mod {mod.DirectoryName} from Restraint Set: "+
        $"{_restraintSets[setIndex]._name}.");
    }

    public void UpdateMod(int setIndex, Mod mod, ModSettings settings, bool disableWhenInactive, bool redrawAfterToggle) {
        var modIndex = _restraintSets[setIndex]._associatedMods.FindIndex(x => x.mod == mod);
        if (modIndex == -1) {
            return;
        }
        _restraintSets[setIndex]._associatedMods[modIndex] = (mod, settings, disableWhenInactive, redrawAfterToggle);
        Save();
        GSLogger.LogType.Debug($"Updated associated mod {mod.DirectoryName} from Restraint Set: "+
        $"{_restraintSets[setIndex]._name}.");
    }
#endregion ModAssociations
#region Setters
    public void ChangeRestraintSetNewLockEndTime(int restraintSetIdx, DateTimeOffset newEndTime) {
        _restraintSets[restraintSetIdx].DeclareNewEndTimeForSet(newEndTime);
        _saveService.QueueSave(this);
    }

    public void ChangeSetDrawDataIsEnabled(int restraintSetIdx, EquipSlot DrawDataSlot, bool isEnabled) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataIsEnabled(isEnabled);
        _saveService.QueueSave(this);
    }

    public void ChangeSetDrawDataWasEquippedBy(int restraintSetIdx, EquipSlot DrawDataSlot, string wasEquippedBy) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataEquippedBy(wasEquippedBy);
        _saveService.QueueSave(this);
    }

    public void ChangeSetDrawDataIsLocked(int restraintSetIdx, EquipSlot DrawDataSlot, bool isLocked) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataLocked(isLocked);
        _saveService.QueueSave(this);
    }

    public void ChangeSetDrawDataSlot(int restraintSetIdx, EquipSlot DrawDataSlot, EquipSlot slot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataSlot(slot);
        _saveService.QueueSave(this);
    }

    public void ChangeSetDrawDataGameItem(int restraintSetIdx, EquipSlot DrawDataSlot, EquipItem gameItem) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataGameItem(gameItem);
        _saveService.QueueSave(this);
    }

    public void ChangeSetDrawDataGameStain(int restraintSetIdx, EquipSlot DrawDataSlot, StainId gameStain) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataGameStain(gameStain);
        _saveService.QueueSave(this);
    }

    public void ResetSetDrawDataGameItem(int restraintSetIdx, EquipSlot DrawDataSlot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].ResetDrawDataGameItem();
        _saveService.QueueSave(this);
    }

    public void ResetSetDrawDataGameStain(int restraintSetIdx, EquipSlot DrawDataSlot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].ResetDrawDataGameStain();
        _saveService.QueueSave(this);
    }

    public void ResetEverythingDueToSafeword() {
        foreach (var set in _restraintSets) {
            set._enabled = false;
            set._locked = false;
            set._wasLockedBy = "";
            set._lockedTimer = DateTimeOffset.Now; 
        }
        GSLogger.LogType.Debug($"[RestraintSetManager] Reset all restraint sets due to safeword!");
        Save();
    }
#endregion Setters
#region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.RestraintSetsFile;

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
        foreach (var set in _restraintSets)
            array.Add(set.Serialize());
        // return the new object under the label "RestraintSets"
        return new JObject() {
            ["Version"] = RestraintSetsFileVersion,
            ["ActiveSetIdx"] = _selectedIdx,
            ["RestraintSets"] = array,
        };
    }

    public void Load() {
        var file = _saveService.FileNames.RestraintSetsFile;
        _restraintSets.Clear();
        if (!File.Exists(file)) {
            return;
        }
        GSLogger.LogType.Debug($"[RestraintSetManager] Attempting to load restraint sets p2");
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);

            // first deserialize the activesetidx and the sets array, as these are supported under both deserializations
            _selectedIdx = jsonObject["ActiveSetIdx"]?.Value<int>() ?? 0;
            var restraintSetsArray = jsonObject["RestraintSets"]?.Value<JArray>();
            if(restraintSetsArray == null) {
                throw new Exception("RestraintSets.json is missing the RestraintSets array.");
            }

            // Check if "Version" property exists. Basically anything from version 0 is processed here to be migrated to version 1
            if (jsonObject["Version"] == null || jsonObject["Version"].Value<int>() < RestraintSetsFileVersion)
            {
                GSLogger.LogType.Information($"[RestraintSetManager] RestraintSetManager is behind in file versions, migrating!");
            }
            else
            {
                // let us know we are up to date
                GSLogger.LogType.Information($"[RestraintSetManager] Your RestraintSetManager file is up to date, migration not nessisary!");
            }
            // we are in version 1, so we should deserialize appropriately
            foreach (var item in restraintSetsArray) {
                var restraintSet = new RestraintSet();
                var ItemValue = item.Value<JObject>();
                if (ItemValue != null) {
                    restraintSet.Deserialize(ItemValue, RestraintSetsFileVersion);
                    _restraintSets.Add(restraintSet);
                } else {
                    GSLogger.LogType.Error($"[RestraintSetManager] Array contains an invalid entry (it is null), skipping!");
                }
            }
        } catch (Exception ex) {
            GSLogger.LogType.Error($"Failure to load automated designs: Error during parsing. {ex}");
        } finally {
            GSLogger.LogType.Debug($"[RestraintSetManager] RestraintSets.json loaded! Loaded {_restraintSets.Count} restraint sets.");
        }
    }
}
#endregion Json ISavable & Loads