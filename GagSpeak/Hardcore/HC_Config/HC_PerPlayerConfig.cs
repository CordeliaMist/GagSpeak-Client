using System;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
using Dalamud.Game.ClientState.Keys;
using GagSpeak.UI.Equipment;
using Penumbra.GameData.Enums;
using GagSpeak.Utility;
namespace GagSpeak.Hardcore;
public partial class HC_PerPlayerConfig
{
    // When a whitelisted user says "follow me" you will be forced to follow them and your movement is restricted until your character stops moving for a set amount of time.
    public bool _allowForcedFollow { get; private set; } = false;   // if you give player permission
    public bool _forcedFollow { get; private set; } = false;        // if the player has activated it


    // when whitelisted user says "sit", you will be forced to sit and your movement is restricted until they say "stand"
    public bool _allowForcedSit { get; private set; } = false;      // if you give player permission
    public bool _forcedSit { get; private set; } = false;           // if the player has activated it   
    
    
    // after a whitelisted player says "stay here for now", teleport and return will be blocked, and your any exit US that pops up will automatically hit no on yesno confirmations... These permissions are restored when they say "come along now"
    public bool _allowForcedToStay { get; private set; } = false;  // if you give player permission
    public bool _forcedToStay { get; private set; } = false;       // if the player has activated it


    // if active, a blindfold overlay is visable (global setting, independant of restraint set
    public bool _allowBlindfold { get; private set; } = false;     // if you give player permission
    public bool _forceLockFirstPerson { get; private set; } = false; // if you force first person view
    public bool _blindfolded { get; private set; } = false;        // if the player has activated it
    public EquipDrawData _blindfoldItem; // the item bound to the blindfold slot
    
    // the list of restraint set properties
    public List<HC_RestraintProperties> _rsProperties;


    [JsonIgnore]
    private readonly RS_PropertyChangedEvent _rsPropertyChanged;
    public HC_PerPlayerConfig(RS_PropertyChangedEvent rsPropertyChanged) {
        // load base list
        _rsPropertyChanged = rsPropertyChanged;
        _rsProperties = new List<HC_RestraintProperties>();
        _blindfoldItem = new EquipDrawData(ItemIdVars.NothingItem(EquipSlot.Head));
    }

    public JObject Serialize() {
        var obj = new JObject();
        // serialize the selectedIdx
        return new JObject() {
            ["AllowForcedSit"] = _allowForcedSit,
            ["ForcedSit"] = _forcedSit,
            ["AllowForcedFollow"] = _allowForcedFollow,
            ["ForcedFollow"] = _forcedFollow,
            ["AllowForcedToStay"] = _allowForcedToStay,
            ["ForcedToStay"] = _forcedToStay,
            ["AllowBlindfold"] = _allowBlindfold,
            ["ForceFirstPerson"] = _forceLockFirstPerson,
            ["Blindfolded"] = _blindfolded,
            ["BlindfoldItem"] = _blindfoldItem.Serialize(),
            ["RestraintProperties"] = new JArray(_rsProperties.Select(setProps => setProps.Serialize())),
        };
    }
    public void Deserialize(JObject jsonObject) {
        try{
            // deserialize the selectedIdx
            _allowForcedSit = jsonObject["AllowForcedSit"]?.Value<bool>() ?? false;
            _forcedSit = jsonObject["ForcedSit"]?.Value<bool>() ?? false;
            _allowForcedFollow = jsonObject["AllowForcedFollow"]?.Value<bool>() ?? false;
            _forcedFollow = jsonObject["ForcedFollow"]?.Value<bool>() ?? false;
            _allowForcedToStay = jsonObject["AllowForcedToStay"]?.Value<bool>() ?? false;
            _forcedToStay = jsonObject["ForcedToStay"]?.Value<bool>() ?? false;
            _allowBlindfold = jsonObject["AllowBlindfold"]?.Value<bool>() ?? false;
            _forceLockFirstPerson = jsonObject["ForceFirstPerson"]?.Value<bool>() ?? false;
            _blindfolded = jsonObject["Blindfolded"]?.Value<bool>() ?? false;
            var blindfoldItemObject = jsonObject["BlindfoldItem"]?.Value<JObject>();
            if (blindfoldItemObject != null) {
                string? slotString = blindfoldItemObject["Slot"]?.Value<string>() ?? "Head";
                EquipSlot slot = (EquipSlot)Enum.Parse(typeof(EquipSlot), slotString);
                var drawData = new EquipDrawData(ItemIdVars.NothingItem(slot));
                drawData.Deserialize(blindfoldItemObject);
                _blindfoldItem = drawData;
            }
            // properties
            var rsPropertiesArray = jsonObject["RestraintProperties"]?.Value<JArray>();
            _rsProperties = new List<HC_RestraintProperties>();
            if (rsPropertiesArray != null) {
                foreach (var item in rsPropertiesArray) {
                    var rsProperty = new HC_RestraintProperties();
                    var itemValue = item.Value<JObject>();
                    if (itemValue != null) {
                        rsProperty.Deserialize(itemValue);
                        _rsProperties.Add(rsProperty);
                    } else {
                        GSLogger.LogType.Error($"[HC_PerPlayerConfig] itemValue in the array of objects is null, skipping over!");
                    }
                }
            }
        } catch (Exception ex) {
            GSLogger.LogType.Error($"[HC_PerPlayerConfig] Error deserializing HC_PerPlayerConfig: {ex}");
        }
    }
}
