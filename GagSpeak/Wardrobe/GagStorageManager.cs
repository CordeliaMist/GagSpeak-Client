using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Penumbra.GameData.Structs;
using GagSpeak.Gagsandlocks;
using GagSpeak.UI.Equipment;
using GagSpeak.Utility;

namespace GagSpeak.Wardrobe;
public class GagStorageManager : ISavable
{
    public GagList.GagType _selectedGag = 0;
    public Dictionary<GagList.GagType, EquipDrawData> _gagEquipData = [];
    
    [JsonIgnore]
    private readonly SaveService _saveService;

    public GagStorageManager(SaveService saveService) {
        _saveService = saveService;
        // load the information from our storage file
        Load();
        // if the load failed, set default values for first time installs
        if (_gagEquipData == null || !_gagEquipData.Any()) {
            GSLogger.LogType.Debug($"[GagStorageManager]: gagEquipData is null, creating new list");
            _gagEquipData = Enum.GetValues(typeof(GagList.GagType))             // create the data for a new Dictionary                 
                .Cast<GagList.GagType>()                                        // get the enum gaglist        
                .ToDictionary(gagType => gagType, gagType => new EquipDrawData(ItemIdVars.NothingItem(EquipSlot.Head)));
        } 
    }

#region Manager Methods
    public GagList.GagType GetSelectedIdx() => _selectedGag;

    public void SetSelectedIdx(GagList.GagType idx) {
        _selectedGag = idx;
        _saveService.QueueSave(this);
    }


    public void ChangeGagDrawDataIsEnabled(GagList.GagType gagType, bool isEnabled) {
        _gagEquipData[gagType].SetDrawDataIsEnabled(isEnabled);
        _saveService.QueueSave(this);
    }

    public void ChangeGagDrawDataWasEquippedBy(GagList.GagType gagType, string wasEquippedBy) {
        _gagEquipData[gagType].SetDrawDataEquippedBy(wasEquippedBy);
        _saveService.QueueSave(this);
    }

    public void ChangeGagDrawDataIsLocked(GagList.GagType gagType, bool isLocked) {
        _gagEquipData[gagType].SetDrawDataLocked(isLocked);
        _saveService.QueueSave(this);
    }

    public void ChangeGagDrawDataSlot(GagList.GagType gagType, EquipSlot slot) {
        _gagEquipData[gagType].SetDrawDataSlot(slot);
        _saveService.QueueSave(this);
    }
    public void ChangeGagDrawDataGameItem(GagList.GagType gagType, EquipItem gameItem) {
        _gagEquipData[gagType].SetDrawDataGameItem(gameItem);
        _saveService.QueueSave(this);
    }

    public void ChangeGagDrawDataGameStain(GagList.GagType gagType, StainId gameStain) {
        _gagEquipData[gagType].SetDrawDataGameStain(gameStain);
        _saveService.QueueSave(this);
    }

    public void ResetGagDrawDataGameItem(GagList.GagType gagType) {
        _gagEquipData[gagType].ResetDrawDataGameItem();
        _saveService.QueueSave(this);
    }

    public void ResetGagDrawDataGameStain(GagList.GagType gagType) {
        _gagEquipData[gagType].ResetDrawDataGameStain();
        _saveService.QueueSave(this);
    }

    public void ResetEverythingDueToSafeword() {
        foreach (var drawDataForGag in _gagEquipData.Values)
        {
            drawDataForGag.SetDrawDataEquippedBy("");
            drawDataForGag.SetDrawDataLocked(false);
            drawDataForGag.SetDrawDataIsEnabled(false);
        }
        GSLogger.LogType.Debug($"[GagStorageManager] Reset all gagdata's auto equip values sets due to safeword!");
        _saveService.QueueSave(this);
    }

#endregion Manager Methods

#region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.GagStorageFile;

    public void Save(StreamWriter writer) {
        using var j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;
        Serialize().WriteTo(j);
    }

    public void Save()
        => _saveService.DelaySave(this);

    public JObject Serialize() {
        var obj = new JObject();
        // serialize the selectedIdx
        foreach (var pair in _gagEquipData)
            obj[pair.Key.ToString()] = pair.Value.Serialize();
        return new JObject() {
            ["SelectedGag"] = _selectedGag.ToString(),
            ["GagEquipData"] = obj,
        };
    }

    public void Load() {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        var file = _saveService.FileNames.GagStorageFile;
        _gagEquipData.Clear();
        if (!File.Exists(file)) {
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            if(jsonObject["SelectedGag"] != null){
                _selectedGag = (GagList.GagType)Enum.Parse(typeof(GagList.GagType), jsonObject["SelectedGag"].Value<string>());
            }    
            var gagEquipDataToken = jsonObject["GagEquipData"].Value<JObject>();
            if (gagEquipDataToken != null) {
                foreach (var gagData in gagEquipDataToken) {
                    var gagType = (GagList.GagType)Enum.Parse(typeof(GagList.GagType), gagData.Key);
                    if (gagData.Value is JObject itemObject) {
                        string? slotString = itemObject["Slot"].Value<string>();
                        EquipSlot slot = (EquipSlot)Enum.Parse(typeof(EquipSlot), slotString);
                        var drawData = new EquipDrawData(ItemIdVars.NothingItem(slot));
                        drawData.Deserialize(itemObject);
                        _gagEquipData.Add(gagType, drawData);
                    }
                }
            }
        } catch (Exception ex) {
            GSLogger.LogType.Error($"[GagStorageManager] Error loading GagStorage.json: {ex}");
        } finally {
            GSLogger.LogType.Debug($"[GagStorageManager] GagStorage.json loaded!");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
#endregion Json ISavable & Loads