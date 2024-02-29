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
namespace GagSpeak.Hardcore;
public partial class HC_PerPlayerConfig
{
    // when whitelisted user says "sit", you will be forced to sit and your movement is restricted until they say "stand"
    public bool _allowForcedSit { get; private set; } = false;      // if you give player permission
    public bool _forcedSit { get; private set; } = false;           // if the player has activated it

    
    // When a whitelisted user says "follow me" you will be forced to follow them and your movement is restricted until your character stops moving for a set amount of time.
    public bool _allowForcedFollow { get; private set; } = false;   // if you give player permission
    public bool _forcedFollow { get; private set; } = false;        // if the player has activated it
    
    
    // after a whitelisted player says "stay here for now", teleport and return will be blocked, and your any exit US that pops up will automatically hit no on yesno confirmations... These permissions are restored when they say "come along now"
    public bool _allowForcedToStay { get; private set; } = false;  // if you give player permission
    public bool _forcedToStay { get; private set; } = false;       // if the player has activated it


    // if active, a blindfold overlay is visable (global setting, independant of restraint set
    public bool _allowBlindfold { get; private set; } = false;     // if you give player permission
    public bool _blindfolded { get; private set; } = false;        // if the player has activated it
    
    
    // the list of restraint set properties
    public List<HC_RestraintProperties> _rsProperties;             // the list of restraint set properties


    [JsonIgnore]
    private readonly RS_PropertyChangedEvent _rsPropertyChanged;
    public HC_PerPlayerConfig(RS_PropertyChangedEvent rsPropertyChanged) {
        // load base list
        _rsPropertyChanged = rsPropertyChanged;
        _rsProperties = new List<HC_RestraintProperties>();
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
            ["Blindfolded"] = _blindfolded,
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
            _blindfolded = jsonObject["Blindfolded"]?.Value<bool>() ?? false;
            // properties
            var rsPropertiesArray = jsonObject["RestraintProperties"]?.Value<JArray>();
            _rsProperties = new List<HC_RestraintProperties>();
            if (rsPropertiesArray != null) {
                foreach (var item in rsPropertiesArray) {
                    var rsProperty = new HC_RestraintProperties();
                    rsProperty.Deserialize(item.Value<JObject>());
                    _rsProperties.Add(rsProperty);
                }
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[HC_PerPlayerConfig] Error deserializing HC_PerPlayerConfig: {ex}");
        } finally {
            GagSpeak.Log.Debug($"[HC_PerPlayerConfig] HC_PerPlayerConfig deserialized!");
        }
    }
}
