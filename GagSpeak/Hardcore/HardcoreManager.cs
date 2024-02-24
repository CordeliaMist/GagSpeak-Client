using System;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace GagSpeak.Hardcore;
public class HardcoreManager : ISavable
{
    public bool _forcedWalk { get; private set; } = false;
    public bool _movementDisabled { get; private set; } = false;
    // when whitelisted user says "sit", you will be forced to sit and your movement is restricted until they say "stand"
    public bool _forcedSit { get; private set; } = false;
     // When a whitelisted user says "follow me" you will be forced to follow them and your movement is restricted until your character stops moving for a set amount of time.
    public bool _forcedFollow { get; private set; } = false;
    // after a whitelisted player says "stay here for now", teleport and return will be blocked, and your any exit US that pops up will automatically hit no on yesno confirmations... These permissions are restored when they say "come along now"
    public bool _forcedToStay { get; private set; } = false;
    // if active, a blindfold overlay is visable
    public bool _blindfolded { get; private set; } = false;
    public List<HC_RestraintProperties> _rsProperties;

    [JsonIgnore]
    private readonly SaveService _saveService;
    [JsonIgnore]
    private readonly RestraintSetManager _restraintSetManager;
    [JsonIgnore]
    private readonly RS_PropertyChangedEvent _rsPropertyChanged;
    [JsonIgnore]
    private readonly RestraintSetListChanged _restraintSetListChanged;

    public HardcoreManager(SaveService saveService, RestraintSetListChanged restraintSetListChanged,
    RS_PropertyChangedEvent rsPropertyChanged, RestraintSetManager restraintSetManager) {
        _saveService = saveService;
        _restraintSetListChanged = restraintSetListChanged;
        _rsPropertyChanged = rsPropertyChanged;
        _restraintSetManager = restraintSetManager;
        // load base list
        _rsProperties = new List<HC_RestraintProperties>();
        // load the information from our storage file
        Load();
        // run size integrity check
        IntegrityCheck(_restraintSetManager._restraintSets.Count);
        Save();

        _restraintSetListChanged.SetListModified += OnRestraintSetListModified;
    }
#region events
    public void OnRestraintSetListModified(object sender, RestraintSetListChangedArgs e) {
        // update the player chars things to match the restraint set list change
        switch(e.UpdateType) {
            case ListUpdateType.AddedRestraintSet : {
                _rsProperties.Add(new HC_RestraintProperties());
                }
                break;
            case ListUpdateType.ReplacedRestraintSet: {
                _rsProperties[e.SetIndex] = new HC_RestraintProperties();
                }
                break;
            case ListUpdateType.RemovedRestraintSet: {
                _rsProperties.RemoveAt(e.SetIndex);
                }
                break;
            case ListUpdateType.SizeIntegrityCheck: {
                // call the integrity check function from uniquePlayerPerms
                IntegrityCheck(e.SetIndex);
                break;
            }
        }
        Save();
    }
#endregion events

#region Manager Methods

    public void SetForcedWalk(bool forcedWalk) {
        _forcedWalk = forcedWalk;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedWalk,
                        forcedWalk ? RestraintSetChangeType.Enabled
                                   : RestraintSetChangeType.Disabled);
    }
    public void SetMovementDisabled(bool movementDisabled, bool invokeChange = true) {
        
        _movementDisabled = movementDisabled;
        _saveService.QueueSave(this);
        // invoke the change
        if(invokeChange) {
            _rsPropertyChanged.Invoke(HardcoreChangeType.MovementDisabled, 
                    movementDisabled ? RestraintSetChangeType.Enabled
                                        : RestraintSetChangeType.Disabled);
        }
    }
    public void SetForcedSit(bool forcedSit) { 
        _forcedSit = forcedSit;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedSit, forcedSit ? RestraintSetChangeType.Enabled
                                                                          : RestraintSetChangeType.Disabled);
    }
    
    public void SetForcedFollow(bool forcedFollow) {
        _forcedFollow = forcedFollow;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedFollow, forcedFollow ? RestraintSetChangeType.Enabled
                                                                                : RestraintSetChangeType.Disabled);
    }
    public void SetForcedToStay(bool forcedToStay) {
        _forcedToStay = forcedToStay;
        _saveService.QueueSave(this);
        // invoke the change
        //_rsPropertyChanged.Invoke(HardcoreChangeType.Immobile, forcedToStay ? RestraintSetChangeType.Enabled
        //                                                                    : RestraintSetChangeType.Disabled);
    }
    public void SetBlindfolded(bool blindfolded) {
        _blindfolded = blindfolded;
        _saveService.QueueSave(this);
        // invoke the change
        //_rsPropertyChanged.Invoke(HardcoreChangeType.Blindfolded, blindfolded ? RestraintSetChangeType.Enabled
        //                                                                      : RestraintSetChangeType.Disabled);
    }
    public void ResetEverythingDueToSafeword() {
        _forcedWalk = false;
        _movementDisabled = false;
        _forcedSit = false;
        _forcedFollow = false;
        _forcedToStay = false;
        _blindfolded = false;
        _saveService.QueueSave(this);
        // invoke safeword
        _rsPropertyChanged.Invoke(HardcoreChangeType.Safeword, RestraintSetChangeType.Disabled);
    }

    // run integrity check to make sure the size of _rsProperties is the same as the restraint set size
    private void IntegrityCheck(int setIndex) {
        if(_rsProperties.Count < setIndex) {
            for(int i = _rsProperties.Count; i < setIndex; i++) {
                _rsProperties.Add(new HC_RestraintProperties());
            }
        } else if(_rsProperties.Count > setIndex) {
            _rsProperties.RemoveRange(setIndex, _rsProperties.Count - setIndex);
        }
    }

#endregion Manager Methods
#region property setters
    public void SetLegsRestraintedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._legsRestraintedProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetArmsRestraintedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._armsRestraintedProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetGaggedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._gaggedProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetBlindfoldedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._blindfoldedProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetImmobileProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._immobileProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetWeightedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._weightyProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetLightStimulationProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._lightStimulationProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetMildStimulationProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._mildStimulationProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

    public void SetHeavyStimulationProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._heavyStimulationProperty = value;
        _saveService.QueueSave(this);
        // doesnt madder if enabled or disabled, we will just refresh the active actions if so (for now)
        _rsPropertyChanged.Invoke(HardcoreChangeType.RS_PropertyModified, RestraintSetChangeType.Disabled);
    }

#endregion property setters


#region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.HardcoreSettingsFile;

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
        return new JObject() {
            ["ForcedWalk"] = _forcedWalk,
            ["MovementDisabled"] = _movementDisabled,
            ["ForcedSit"] = _forcedSit,
            ["ForcedFollow"] = _forcedFollow,
            ["ForcedToStay"] = _forcedToStay,
            ["Blindfolded"] = _blindfolded,
            ["RestraintProperties"] = new JArray(_rsProperties.Select(setProps => setProps.Serialize())),
        };
    }

    public void Load() {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        var file = _saveService.FileNames.HardcoreSettingsFile;
        if (!File.Exists(file)) {
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            _forcedWalk = jsonObject["ForcedWalk"]?.Value<bool>() ?? false;
            _movementDisabled = jsonObject["MovementDisabled"]?.Value<bool>() ?? false;
            _forcedSit = jsonObject["ForcedSit"]?.Value<bool>() ?? false;
            _forcedFollow = jsonObject["ForcedFollow"]?.Value<bool>() ?? false;
            _forcedToStay = jsonObject["ForcedToStay"]?.Value<bool>() ?? false;
            _blindfolded = jsonObject["Blindfolded"]?.Value<bool>() ?? false;

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
            GagSpeak.Log.Error($"[HardcoreManager] Error loading HardcoreManager.json: {ex}");
        } finally {
            GagSpeak.Log.Debug($"[HardcoreManager] HardcoreManager.json loaded!");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
#endregion Json ISavable & Loads