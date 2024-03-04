using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using Newtonsoft.Json.Linq;
using GagSpeak.UI.Equipment;
using GagSpeak.Utility;
using GagSpeak.Interop.Penumbra;
using OtterGui.Classes;
using Dalamud.Interface.Internal.Notifications;

namespace GagSpeak.Wardrobe;

public class RestraintSet //: IDisposable
{
    public string _name; // lets you define the name of the set
    public string _description; // lets you define the description of the set
    public bool _enabled; // lets you define if the set is enabled
    public bool _locked; // lets you define if the set is locked
    public string _wasEnabledBy; // know who toggled the set
    public string _wasLockedBy; // lets you define the name of the character that equipped the set
    public DateTimeOffset _lockedTimer { get; set; } // stores the timespan left until unlock of the player.
    public Dictionary<EquipSlot, EquipDrawData> _drawData; // stores the equipment draw data for the set
    public SortedList<Mod, ModSettings> _associatedMods { get; private set; }  = []; // the associated mods to enable with this set
    public List<bool> _disableModsWhenInactive { get; set; } // lets you define if the mods should be disabled when the set is disabled

    public RestraintSet() {
        // define default data for the set
        _name = "New Restraint Set";
        _description = "This is a blank description!\nYou can right click the title or description to modify them!";
        _enabled = false;
        _locked = false;
        _wasEnabledBy = "";
        _wasLockedBy = "";
        _lockedTimer = DateTimeOffset.Now;
        // create the new dictionaries
        _drawData = new Dictionary<EquipSlot, EquipDrawData>();
        foreach (var slot in EquipSlotExtensions.EqdpSlots) {
            _drawData[slot] = new EquipDrawData(ItemIdVars.NothingItem(slot));
            _drawData[slot].SetDrawDataSlot(slot);
            _drawData[slot].SetDrawDataIsEnabled(false);
        }
        // create the new associated mods list
        _associatedMods = new SortedList<Mod, ModSettings>();
        _disableModsWhenInactive = new List<bool>();
    }

    public void ChangeSetName(string name) {
        _name = name;
    }

    public void ChangeSetDescription(string description) {
        _description = description;
    }

    public void SetIsEnabled(bool enabled, string wasEnabledBy = "self") {
        _enabled = enabled;
        if(enabled) {
            _wasEnabledBy = wasEnabledBy;
        } else {
            _wasEnabledBy = "";
        }
    }

    public void SetPieceIsEnabled(EquipSlot slot, bool enabled) {
        _drawData[slot].SetDrawDataIsEnabled(enabled);
    }

    public void SetIsLocked(bool locked, string wasLockedBy = "") {
        _locked = locked;
        _wasLockedBy = wasLockedBy;
        // if this was an unlock, then reset the waslockedBy
        if (!locked)
            _wasLockedBy = "";
    }

    public void DeclareNewEndTimeForSet(DateTimeOffset lockedTimer) {
        _lockedTimer = lockedTimer;
    }
    public JObject Serialize() {
        // we will create another array, storing the draw data for the restraint set
        var drawDataArray = new JArray();
        // for each of the draw data, serialize them and add them to the array
        foreach (var pair in _drawData)
            drawDataArray.Add(new JObject() {
                ["EquipmentSlot"] = pair.Key.ToString(),
                ["DrawData"] = pair.Value.Serialize()
            });

        return new JObject()
        {
            ["Name"] = _name,
            ["Description"] = _description,
            ["IsEnabled"] = _enabled,
            ["Locked"] = _locked,
            ["WasEnabledBy"] = _wasEnabledBy,
            ["WasLockedBy"] = _wasLockedBy,
            ["LockedTimer"] = _lockedTimer.ToString(),
            ["DrawData"] = drawDataArray,
            ["AssociatedMods"] = SerializeMods(),
            ["DisableModsWhenInactive"] = JToken.FromObject(_disableModsWhenInactive),
        };
    }

    private JArray SerializeMods() {
        // otherwise we will create a new array to store the mods
        var ret = new JArray();
        // for each mod in are associated mods
        foreach (var (mod, settings) in _associatedMods) {
            // create a jobject representing it
            var obj = new JObject() {
                ["Name"]      = mod.Name,
                ["Directory"] = mod.DirectoryName,
                ["Enabled"]   = settings.Enabled,
            };
            if (settings.Enabled) {
                obj["Priority"] = settings.Priority;
                obj["Settings"] = JObject.FromObject(settings.Settings);
            }
            // and add it to the JArray
            ret.Add(obj);
        }

        return ret;
    }

    public void Deserialize(JObject jsonObject) {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        _name = jsonObject["Name"]?.Value<string>() ?? string.Empty;
        _description = jsonObject["Description"]?.Value<string>() ?? string.Empty;
        _enabled = jsonObject["IsEnabled"]?.Value<bool>() ?? false;
        _locked = jsonObject["Locked"]?.Value<bool>() ?? false;
        _wasEnabledBy = jsonObject["WasEnabledBy"]?.Value<string>() ?? string.Empty;
        _wasLockedBy = jsonObject["WasLockedBy"]?.Value<string>() ?? string.Empty;
        _lockedTimer = jsonObject["LockedTimer"] != null ? DateTimeOffset.Parse(jsonObject["LockedTimer"].Value<string>()) : default;

        _drawData.Clear();
        var drawDataArray = jsonObject["DrawData"]?.Value<JArray>();
        if (drawDataArray != null) {
            foreach (var item in drawDataArray) {
                var itemObject = item.Value<JObject>();
                if (itemObject != null) {
                    var equipmentSlot = (EquipSlot)Enum.Parse(typeof(EquipSlot), itemObject["EquipmentSlot"]?.Value<string>() ?? string.Empty);
                    var drawData = new EquipDrawData(ItemIdVars.NothingItem(equipmentSlot));
                    drawData.Deserialize(itemObject["DrawData"]?.Value<JObject>());
                    _drawData.Add(equipmentSlot, drawData);
                }
            }
        }
        // load the mods
        DeserializeMods(jsonObject["AssociatedMods"]);
        _disableModsWhenInactive = jsonObject["DisableModsWhenInactive"]?.ToObject<List<bool>>() ?? new List<bool>();
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }

    private void DeserializeMods(JToken? mods) {
        if (mods is not JArray array)
            return;

        foreach (var tok in array) {
            var name      = tok["Name"]?.ToObject<string>();
            var directory = tok["Directory"]?.ToObject<string>();
            var enabled   = tok["Enabled"]?.ToObject<bool>();
            if (name == null || directory == null || enabled == null) {
                GagSpeak.Messager.NotificationMessage("The loaded design contains an invalid mod, skipped.", NotificationType.Warning);
                continue;
            }

            var settingsDict = tok["Settings"]?.ToObject<Dictionary<string, IList<string>>>() ?? new Dictionary<string, IList<string>>();
            var priority = tok["Priority"]?.ToObject<int>() ?? 0;

            var mod = new Mod(name, directory);
            var modSettings = new ModSettings(settingsDict, priority, enabled.Value);

            if (!_associatedMods.TryAdd(mod, modSettings))
                GagSpeak.Messager.NotificationMessage("The loaded design contains a mod more than once, skipped.", NotificationType.Warning);
        }
    }
}