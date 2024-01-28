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
public class GagStorageManager : ISavable
{
    private readonly SaveService _saveService;
    private readonly GagSpeakConfig _config;
    public Dictionary<GagList.GagType, EquipDrawData> _gagEquipData { get; set; }

    public GagStorageManager(SaveService saveService, GagSpeakConfig config) {
        _saveService = saveService;
        _config = config;
        // create the new dictionaries (replace with loading a list later)
        _gagEquipData = _config.gagEquipData;
        /*
        _gagEquipData = Enum.GetValues(typeof(GagList.GagType))
            .Cast<GagList.GagType>()
            .ToDictionary(gagType => gagType, gagType => new EquipDrawData(ItemIdVars.NothingItem(EquipSlot.Head)));
        */
        // load the information from our storage file
    
    }
    
    #region Manager Methods




    #endregion Manager Methods

    #region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.GagStorageFile;

    public void Save(StreamWriter writer) {
        using var j = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
        Serialize().WriteTo(j);
    }

    public void Save()
        => _saveService.DelaySave(this);

    public JObject Serialize() {
        var obj = new JObject();
        foreach (var pair in _gagEquipData)
            obj[pair.Key.ToString()] = JToken.FromObject(pair.Value);
        return new JObject() {
            ["GagEquipData"] = obj,
        };
    }

    public void Load() {
        var file = _saveService.FileNames.GagStorageFile;
        _gagEquipData.Clear();
        if (!File.Exists(file)) {
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var obj = JObject.Parse(text);
            var gagEquipDataToken = obj["GagEquipData"];
            if (gagEquipDataToken != null) {
                _gagEquipData = gagEquipDataToken.ToObject<Dictionary<GagList.GagType, EquipDrawData>>();
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[GagStorageManager] Error loading GagStorage.json: {ex}");
        }
    }
}
#endregion Json ISavable & Loads