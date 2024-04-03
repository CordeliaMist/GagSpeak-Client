using System;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore.Movement;
using GagSpeak.UI;
using System.Numerics;

namespace GagSpeak.Hardcore;
public partial class HardcoreManager : ISavable, IDisposable
{
    // this makes sure all of our options are independant for each player
    public List<HC_PerPlayerConfig> _perPlayerConfigs;

    // the list of entries to auto select no from when forced to stay is active
    public TextFolderNode StoredEntriesFolder { get; private set; } = new TextFolderNode { Name = "ForcedDeclineList" };

    [JsonIgnore] public double StimulationMultipler = 1.0; // the multiplier for the stimulation
    [JsonIgnore] internal Tuple<string, List<string>> LastSeenDialogText { get; set; } = Tuple.Create(string.Empty, new List<string>()); // stores the last seen list of dialog options
    [JsonIgnore] public string LastSeenListTarget { get; set; } = string.Empty; // stores last seen list target
    [JsonIgnore] public string LastSeenListSelection { get; set; } = string.Empty; // stores last seen list selection
    [JsonIgnore] public DateTimeOffset LastMovementTime = DateTimeOffset.Now;
    [JsonIgnore] public Vector3 LastPosition = Vector3.Zero;

    [JsonIgnore] private readonly SaveService _saveService;
    [JsonIgnore] private readonly RestraintSetManager _restraintSetManager;
    [JsonIgnore] private readonly CharacterHandler _characterHandler;
    [JsonIgnore] private readonly RS_ListChanged _restraintSetListChanged;
    [JsonIgnore] private readonly RS_ToggleEvent _rsToggleEvent;
    [JsonIgnore] private readonly RS_PropertyChangedEvent _rsPropertyChanged;
    [JsonIgnore] private readonly GagSpeakGlamourEvent _glamourEvent;
    [JsonIgnore] private readonly InitializationManager _manager;

    public HardcoreManager(SaveService saveService, RS_ListChanged restraintSetListChanged, GagSpeakGlamourEvent glamourEvent,
    BlindfoldWindow blindfoldWindow, InitializationManager manager, RS_PropertyChangedEvent propertyChanged,
    CharacterHandler characterHandler, RestraintSetManager restraintSetManager, RS_ToggleEvent rsToggleEvent) {
        _saveService = saveService;
        _glamourEvent = glamourEvent;
        _characterHandler = characterHandler;
        _blindfoldWindow = blindfoldWindow;
        _rsPropertyChanged = propertyChanged;
        _manager = manager;
        _restraintSetListChanged = restraintSetListChanged;
        _restraintSetManager = restraintSetManager;
        _rsToggleEvent = rsToggleEvent;
        // setup a blank list
        _perPlayerConfigs = new List<HC_PerPlayerConfig>();
        // load the information from our storage file
        Load();
        // if the size of the list is still 0, set it to the size of our whitelist
        ListIntegrityCheck(_characterHandler.whitelistChars.Count);
        // save the information
        Save();
        // subscribe to the events
        _restraintSetListChanged.SetListModified += OnRestraintSetListModified;
        // subscribe to the initializer, so we can finish setting everything up once we are ready
        _manager.RS_ManagerInitialized += ManagerReadyForHardcoreManager;
        // set completion task to true
        _manager._hardcoreManagerReadyForEvent.SetResult(true);
    }
#region Manager Helpers
    public void ManagerReadyForHardcoreManager() {
        GSLogger.LogType.Information(" Completing Hardcore Manager Initialization ");
        // run size integrity check
        IntegrityCheck(_restraintSetManager._restraintSets.Count);
        // find who it was that enabled the set, if it is enabled
        if(_restraintSetManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet)) {
            GSLogger.LogType.Debug($"[HardcoreManager]  Active set {enabledIdx} was enabled by: {assignerOfSet}");
            // if the index if not -1, set up the multiplier
            if (enabledIdx != -1) {
                ApplyMultipler();
            } else {
                StimulationMultipler = 1.0;
            }
        } else {
            StimulationMultipler = 1.0;
        }
        // prune empty TextFolderNode enteries
        StoredEntriesFolder.CheckAndInsertRequired();
        StoredEntriesFolder.PruneEmpty();
        // save the information
        Save();
        // invoke the hardcoreManagerFinished method
        _manager.CompleteStep(InitializationSteps.HardcoreManagerInitialized);
    }
    public void Dispose() {
        _manager.RS_ManagerInitialized -= ManagerReadyForHardcoreManager;
        _restraintSetListChanged.SetListModified -= OnRestraintSetListModified;
        // reset movement controls
        if(GagSpeakConfig.usingLegacyControls == false
        && GameConfig.UiControl.GetBool("MoveMode") == true) {
            // we have legacy on but dont normally have it on, so make sure that we set it back to normal!
            GameConfig.UiControl.Set("MoveMode", (int)MovementMode.Standard);
        }
    }
#endregion Manager Helpers

    public void AddNewPlayerConfig() {
        _perPlayerConfigs.Add(new HC_PerPlayerConfig(_rsPropertyChanged));
        // Perform integrity check
        _perPlayerConfigs[_perPlayerConfigs.Count - 1].IntegrityCheck(_restraintSetManager._restraintSets.Count);
        // Save
        _saveService.QueueSave(this);
    }

    public void ReplacePlayerConfig(int index) {
        if (index < 0 || index >= _perPlayerConfigs.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range of the _perPlayerConfigs list.");

        _perPlayerConfigs[index] = new HC_PerPlayerConfig(_rsPropertyChanged);
        // Perform integrity check
        _perPlayerConfigs[index].IntegrityCheck(index);
        // Save
        _saveService.QueueSave(this);
    }

    public void RemovePlayerConfig(int index) {
        if (index < 0 || index >= _perPlayerConfigs.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range of the _perPlayerConfigs list.");

        _perPlayerConfigs.RemoveAt(index);
        // Save
        _saveService.QueueSave(this);
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
        // serialize the selectedIdx
        var array = new JArray();
        // for each of our restraint sets, serialize them and add them to the array
        foreach (var HC_SettingsforPlayer in _perPlayerConfigs) {
            array.Add(HC_SettingsforPlayer.Serialize());
        }
        return new JObject() {
            ["CharacterSettings"] = array,
            ["StoredEntriesFolder"] = JObject.FromObject(StoredEntriesFolder, new JsonSerializer { TypeNameHandling = TypeNameHandling.Auto })
        };
    }

    public void Load() {
        var file = _saveService.FileNames.HardcoreSettingsFile;
        if (!File.Exists(file)) {
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            var characterSettingsArray = jsonObject["CharacterSettings"]?.Value<JArray>();
            _perPlayerConfigs = new List<HC_PerPlayerConfig>();
            if (characterSettingsArray != null) {
                foreach (var item in characterSettingsArray) {
                    var HC_SettingsforPlayer = new HC_PerPlayerConfig(_rsPropertyChanged);
                    var itemValue = item.Value<JObject>();
                    if (itemValue != null) {
                        HC_SettingsforPlayer.Deserialize(itemValue);
                        _perPlayerConfigs.Add(HC_SettingsforPlayer);
                    } else {
                        GSLogger.LogType.Error($"[HardcoreManager] Array contains an invalid entry (it is null), skipping!");
                    }
                }
            }
            // stored entries
            var storedEntriesFolder = jsonObject["StoredEntriesFolder"]?.ToObject<TextFolderNode>();
            if (storedEntriesFolder != null) {
                StoredEntriesFolder = storedEntriesFolder;
            }
        } catch (Exception ex) {
            GSLogger.LogType.Error($"[HardcoreManager] Error loading HardcoreManager.json: {ex}");
        } finally {
            GSLogger.LogType.Debug($"[HardcoreManager] HardcoreManager.json loaded!");

        }
    }
}
