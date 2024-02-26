using System;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
namespace GagSpeak.Hardcore;
public partial class HardcoreManager : ISavable, IDisposable
{
    // when whitelisted user says "sit", you will be forced to sit and your movement is restricted until they say "stand"
    public bool _forcedSit { get; private set; } = false;
     // When a whitelisted user says "follow me" you will be forced to follow them and your movement is restricted until your character stops moving for a set amount of time.
    public bool _forcedFollow { get; private set; } = false;
    // after a whitelisted player says "stay here for now", teleport and return will be blocked, and your any exit US that pops up will automatically hit no on yesno confirmations... These permissions are restored when they say "come along now"
    public bool _forcedToStay { get; private set; } = false;
    // if active, a blindfold overlay is visable (global setting, independant of restraint set)
    public bool _blindfolded { get; private set; } = false;
    // the list of restraint set properties
    public List<HC_RestraintProperties> _rsProperties;
    // the list of entries to auto select no from when forced to stay is active
    public TextFolderNode StoredEntriesFolder { get; private set; } = new TextFolderNode { Name = "ForcedDeclineList" };

#region Ignores
    [JsonIgnore]
    private readonly SaveService _saveService;
    [JsonIgnore]
    private readonly RestraintSetManager _restraintSetManager;
    [JsonIgnore]
    private readonly RS_PropertyChangedEvent _rsPropertyChanged;
    [JsonIgnore]
    private readonly RestraintSetListChanged _restraintSetListChanged;
    [JsonIgnore]
    private readonly GagSpeakGlamourEvent _gsGlamourEvent;
    [JsonIgnore]
    public int ActiveSetIdxEnabled = -1; // will be set to the index of whichever set get's enabled
    [JsonIgnore]
    public double StimulationMultipler = 1.0; // the multipler added to cooldowns when stimulation is active, default is 1.0
    [JsonIgnore]
    internal Tuple<string, List<string>> LastSeenDialogText { get; set; } = Tuple.Create(string.Empty, new List<string>()); // stores the last seen dialog text, will always be temp, and localized here as a source location to pull info from
    [JsonIgnore]
    public string LastSeenListTarget { get; set; } = string.Empty; // stores the last seen list target, will always be temp, and localized here as a source location to pull info from
    [JsonIgnore]
    public string LastSeenListSelection { get; set; } = string.Empty; // stores the last seen list selection, will always be temp, and localized here as a source location to pull info from
#endregion Ignores

    public HardcoreManager(SaveService saveService, RestraintSetListChanged restraintSetListChanged,
    RS_PropertyChangedEvent rsPropertyChanged, RestraintSetManager restraintSetManager, GagSpeakGlamourEvent gsGlamourEvent) {
        _saveService = saveService;
        _restraintSetListChanged = restraintSetListChanged;
        _rsPropertyChanged = rsPropertyChanged;
        _restraintSetManager = restraintSetManager;
        _gsGlamourEvent = gsGlamourEvent;
        // load base list
        _rsProperties = new List<HC_RestraintProperties>();
        // load the information from our storage file
        Load();
        // run size integrity check
        IntegrityCheck(_restraintSetManager._restraintSets.Count);
        // set the actively enabled set index to -1
        ActiveSetIdxEnabled = -1;
        // apply correct multiplier
        ApplyMultipler();
        // prune empty TextFolderNode enteries
        StoredEntriesFolder.CheckAndInsertRequired();
        StoredEntriesFolder.PruneEmpty();
        // save the information
        Save();
        // subscribe to the events
        _gsGlamourEvent.GlamourEventFired += OnJobChange;
        _restraintSetListChanged.SetListModified += OnRestraintSetListModified;
    }

    public void Dispose() {
        _gsGlamourEvent.GlamourEventFired -= OnJobChange;
        _restraintSetListChanged.SetListModified -= OnRestraintSetListModified;
    }

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
            ["ForcedSit"] = _forcedSit,
            ["ForcedFollow"] = _forcedFollow,
            ["ForcedToStay"] = _forcedToStay,
            ["Blindfolded"] = _blindfolded,
            ["RestraintProperties"] = new JArray(_rsProperties.Select(setProps => setProps.Serialize())),
            ["StoredEntriesFolder"] = JObject.FromObject(StoredEntriesFolder, new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto })
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
            _forcedSit = jsonObject["ForcedSit"]?.Value<bool>() ?? false;
            _forcedFollow = jsonObject["ForcedFollow"]?.Value<bool>() ?? false;
            _forcedToStay = jsonObject["ForcedToStay"]?.Value<bool>() ?? false;
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
            // stored entries
            var storedEntriesFolder = jsonObject["StoredEntriesFolder"]?.ToObject<TextFolderNode>();
            if (storedEntriesFolder != null) {
                StoredEntriesFolder = storedEntriesFolder;
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[HardcoreManager] Error loading HardcoreManager.json: {ex}");
        } finally {
            GagSpeak.Log.Debug($"[HardcoreManager] HardcoreManager.json loaded!");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
