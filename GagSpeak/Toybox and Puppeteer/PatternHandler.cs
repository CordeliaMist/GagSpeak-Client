using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using GagSpeak.UI.Tabs.ToyboxTab;

namespace GagSpeak.ToyboxandPuppeteer;

public class PatternHandler : ISavable
{
    public List<PatternData> _patterns = []; // stores the restraint sets
    public int _activePatternIndex = -1; // the index of the active pattern

    [JsonIgnore]
    private readonly SaveService _saveService;
    [JsonIgnore]
    private readonly PatternPlayback _patternPlayback;

    public PatternHandler(SaveService saveService, PatternPlayback patternPlayback) {
        _saveService = saveService;
        _patternPlayback = patternPlayback;
        // load the information from our storage file
        Load();
        // make sure our active index is within range of the list
        if (_activePatternIndex >= _patterns.Count) {
            _activePatternIndex = -1;
        }
    }
    
    public int GetActiveIdx() => _activePatternIndex;

    public bool IsActivePatternInBounds() => _activePatternIndex >= 0 && _activePatternIndex < _patterns.Count;
    
    public void ApplyImportedPatternData(List<byte> data) {
        if (_activePatternIndex == -1) {
            return;
        }
        _patterns[_activePatternIndex]._patternData = data;
        GagSpeak.Log.Debug($"[Pattern Handler] Imported pattern data: {_patterns[_activePatternIndex]._patternData}");
        _saveService.QueueSave(this);
    }

    public void SetActiveIdx(int idx) {
        _activePatternIndex = idx;
        // change that patterns _selected state to true, and all other patterns selected state to false
        for (int i = 0; i < _patterns.Count; i++) {
            _patterns[i]._selected = i == idx;
        }
        _saveService.QueueSave(this);
    }
    public void AddNewPattern() {
        var newSet = new PatternData();
        newSet._name = GenerateUniqueName(newSet._name);
        AddNewPattern(newSet);
    }

    public string GenerateUniqueName(string baseName) {
        int copyNumber = 1;
        string newName = baseName;

        while (_patterns.Any(set => set._name == newName)) {
            newName = baseName + $"(copy{copyNumber++})";
        }

        return newName;
    }

    public void ReplacePattern(int idx, PatternData newPattern) {
        _patterns[idx] = newPattern;
        _saveService.QueueSave(this);
    }

    // add a new pattern by passing in a pattern object
    public void AddNewPattern(PatternData newPattern) {
        _patterns.Add(newPattern);
        // set the pattern index to the first index if it is -1
        if (_activePatternIndex == -1) {
            _activePatternIndex = 0;
        }
        _saveService.QueueSave(this);
    }

    public void RemovePattern(int index) {
        _patterns.RemoveAt(index);
        _activePatternIndex = -1;
        _saveService.QueueSave(this);
    }

    public bool ExecutePattern(string patternName) {
        int patternIdx = _patterns.FindIndex(p => p._name == patternName);
        if (patternIdx == -1) {
            return false;
        }
        // execute the pattern
        ExecutePatternProper();
        return true;
    }

    public void ExecutePatternProper() {
        if (_activePatternIndex == -1) {
            return;
        }
        _patternPlayback.StartPlayback(_patterns[_activePatternIndex], _activePatternIndex);
    }

    public void StopPattern() {
        if (_activePatternIndex == -1) {
            return;
        }
        _patternPlayback.StopPlayback();
    }

    public void RenamePattern(int index, string newName) {
        _patterns[index]._name = newName;
        _saveService.QueueSave(this);
    }

    public void ModifyDescription(int index, string newDescription) {
        _patterns[index]._description = newDescription;
        _saveService.QueueSave(this);
    }
    

    public string ToFilename(FilenameService filenameService)
        => filenameService.PatternStorageFile;

    public void Save(StreamWriter writer)
    {
        using var j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;
        Serialize().WriteTo(j);
    }

    private void Save()
        => _saveService.DelaySave(this);

    public JObject Serialize() {
        var array = new JArray();
        // for each of our restraint sets, serialize them and add them to the array
        foreach (var set in _patterns) {
            array.Add(set.Serialize());
        }
        return new JObject() {
            ["Pattern List"] = array,
            ["Active Pattern Index"] = _activePatternIndex
        };
    }

    public void Load() {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        var file = _saveService.FileNames.PatternStorageFile;
        _patterns.Clear();
        _activePatternIndex = -1; // Reset the active pattern index
        if (!File.Exists(file)) {
            // create default data for the new file if it does not exist
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            var patternsArray = jsonObject["Pattern List"]?.Value<JArray>();
            if(patternsArray != null) {
                foreach (var item in patternsArray) {
                    var pattern = new PatternData();
                    pattern.Deserialize(item.Value<JObject>()); 
                    _patterns.Add(pattern);
                }
            }
            _activePatternIndex = jsonObject["Active Pattern Index"]?.Value<int>() ?? -1;
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Pattern Handler] Failure to load patterns: Error during parsing. {ex}");
        } finally {
            GagSpeak.Log.Debug($"[Pattern Handler] PatternStorage.json loaded! Loaded {_patterns.Count} patterns to profile.");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
