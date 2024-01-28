using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using GagSpeak.Data;
using System.Linq;
using OtterGui.Classes;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Penumbra.GameData.Structs;

namespace GagSpeak.Wardrobe;
public class GagStorageManager : ISavable
{
    private readonly SaveService _saveService;
    public Dictionary<GagList.GagType, EquipDrawData> _gagEquipData = [];

    public GagStorageManager(SaveService saveService) {
        _saveService = saveService;
        // load the information from our storage file
        Load();
        // if the load failed, set default values for first time installs
        if (_gagEquipData == null || !_gagEquipData.Any()) {
            GagSpeak.Log.Debug($"[Config]: gagEquipData is null, creating new list");
            _gagEquipData = Enum.GetValues(typeof(GagList.GagType))             // create the data for a new Dictionary                 
                .Cast<GagList.GagType>()                                        // get the enum gaglist        
                .ToDictionary(gagType => gagType, gagType => new EquipDrawData(ItemIdVars.NothingItem(EquipSlot.Head)));
        } else {
            GagSpeak.Log.Debug($"[Config]: File Loading Sucessful! GagStroage Applied!");
        } 
    }

    #region Manager Methods
    public void ChangeGagDrawDataIsEnabled(GagList.GagType gagType, bool isEnabled) {
        _gagEquipData[gagType].SetDrawDataIsEnabled(isEnabled);
        Save();
    }

    public void ChangeGagDrawDataWasEquippedBy(GagList.GagType gagType, string wasEquippedBy) {
        _gagEquipData[gagType].SetDrawDataEquippedBy(wasEquippedBy);
        Save();
    }

    public void ChangeGagDrawDataIsLocked(GagList.GagType gagType, bool isLocked) {
        _gagEquipData[gagType].SetDrawDataLocked(isLocked);
        Save();
    }

    public void ChangeGagDrawDataSlot(GagList.GagType gagType, EquipSlot slot) {
        _gagEquipData[gagType].SetDrawDataSlot(slot);
        Save();
    }

    public void ChangeGagDrawDataGameItem(GagList.GagType gagType, EquipItem gameItem) {
        _gagEquipData[gagType].SetDrawDataGameItem(gameItem);
        Save();
    }

    public void ChangeGagDrawDataGameStain(GagList.GagType gagType, StainId gameStain) {
        _gagEquipData[gagType].SetDrawDataGameStain(gameStain);
        Save();
    }

    public void ResetGagDrawDataGameItem(GagList.GagType gagType) {
        _gagEquipData[gagType].ResetDrawDataGameItem();
        Save();
    }

    public void ResetGagDrawDataGameStain(GagList.GagType gagType) {
        _gagEquipData[gagType].ResetDrawDataGameStain();
        Save();
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
        foreach (var pair in _gagEquipData)
            obj[pair.Key.ToString()] = pair.Value.Serialize();
        return new JObject() {
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
            GagSpeak.Log.Error($"[GagStorageManager] Error loading GagStorage.json: {ex}");
        } finally {
            GagSpeak.Log.Debug($"[GagStorageManager] GagStorage.json loaded!");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
#endregion Json ISavable & Loads