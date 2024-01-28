using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using GagSpeak.Data;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using System.Linq;
using OtterGui.Classes;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Penumbra.GameData.Structs;

namespace GagSpeak.Wardrobe;

public class RestraintSetManager : ISavable
{
    private readonly SaveService _saveService;
    public List<RestraintSet> _restraintSets; // stores the restraint sets

    public RestraintSetManager(SaveService saveService) {
        _saveService = saveService;

        // create the new dictionaries (replace with loading a list later)
        _restraintSets = new List<RestraintSet>();
        AddNewRestraintSet(); // append new restraint set with default values
        // load the information from our storage file

    }
    
    #region Manager Methods

    public void AddNewRestraintSet() {
        var newSet = new RestraintSet();
        string baseName = newSet._name;
        int copyNumber = 1;
        while (_restraintSets.Any(set => set._name == newSet._name)) {
            newSet.ChangeSetName(baseName + $"(copy{copyNumber})");
            copyNumber++;
        }
        _restraintSets.Add(newSet);
        Save();
    }

    /// <summary> Deletes a restraint set spesified by index if it exists. </summary>
    public void DeleteRestraintSet(int index) {
        // delete a restraint set spesified by index if it exists
        if (index >= 0 && index < _restraintSets.Count) {
            _restraintSets.RemoveAt(index);
            Save();
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
        Save(); // update our json after updating the manager 
        // (will remove old set but transfer all info to newly serialized one)
    }

    /// <summary> Changes the description of a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetDescription(int restraintSetIdx, string newDescription) {
        _restraintSets[restraintSetIdx].ChangeSetDescription(newDescription);
        Save();
    }

    /// <summary> Sets the IsEnabled for a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetEnabled(int restraintSetIdx, bool isEnabled) {
        _restraintSets[restraintSetIdx].SetIsEnabled(isEnabled);
        Save();
    }

    /// <summary> Toggles the enabled state of a restraint set spesified by index if it exists. </summary>
    public void ToggleRestraintSetEnabled(int restraintSetIdx) {
        _restraintSets[restraintSetIdx].SetIsEnabled(!_restraintSets[restraintSetIdx]._enabled);
        Save();
    }

    public void ChangeRestraintSetLocked(int restraintSetIdx, bool isLocked) {
        _restraintSets[restraintSetIdx].SetIsLocked(isLocked);
        Save();
    }

    public void ChangeRestraintSetNewLockEndTime(int restraintSetIdx, DateTimeOffset newEndTime) {
        _restraintSets[restraintSetIdx].DeclareNewEndTimeForSet(newEndTime);
        Save();
    }

    public void ChangeSetDrawDataIsEnabled(int restraintSetIdx, EquipSlot DrawDataSlot, bool isEnabled) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataIsEnabled(isEnabled);
        Save();
    }

    public void ChangeSetDrawDataWasEquippedBy(int restraintSetIdx, EquipSlot DrawDataSlot, string wasEquippedBy) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataEquippedBy(wasEquippedBy);
        Save();
    }

    public void ChangeSetDrawDataIsLocked(int restraintSetIdx, EquipSlot DrawDataSlot, bool isLocked) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataLocked(isLocked);
        Save();
    }

    public void ChangeSetDrawDataSlot(int restraintSetIdx, EquipSlot DrawDataSlot, EquipSlot slot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataSlot(slot);
        Save();
    }

    public void ChangeSetDrawDataGameItem(int restraintSetIdx, EquipSlot DrawDataSlot, EquipItem gameItem) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataGameItem(gameItem);
        Save();
    }

    public void ChangeSetDrawDataGameStain(int restraintSetIdx, EquipSlot DrawDataSlot, StainId gameStain) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataGameStain(gameStain);
        Save();
    }


    #endregion Manager Methods

    #region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.RestraintSetsFile;

    public void Save(StreamWriter writer)
    {
        using var j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;
        Serialize().WriteTo(j);
    }

    private void Save()
        => _saveService.DelaySave(this);

    public JObject Serialize() {
        // create a new array for serialization
        var array = new JArray();
        // for each of our restraint sets, serialize them and add them to the array
        foreach (var set in _restraintSets)
            array.Add(set.Serialize());
        // return the new object under the label "RestraintSets"
        return new JObject() {
            ["RestraintSets"] = array,
        };
    }

    private void Load() {
        var file = _saveService.FileNames.RestraintSetsFile;
        // if we have information to process, then we should empty our restraint sets
        _restraintSets.Clear();
        // if the file doesnt exist, then we shouldnt try to load it
        if (!File.Exists(file)) {
            return;
        }
        // but if it does, then we should try to parse it out and load it
        try {
            // get all the text
            var text    = File.ReadAllText(file);
            // parse it into a fat object
            var obj     = JObject.Parse(text);
        } catch (Exception ex) {
            GagSpeak.Log.Error($"Failure to load automated designs: Error during parsing. {ex}");
        }
    }
}
#endregion Json ISavable & Loads