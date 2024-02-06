using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace GagSpeak.ToyboxandPuppeteer;

public class PatternHandler : ISavable
{
    public List<PatternData> _patterns = []; // stores the restraint sets

    [JsonIgnore]
    private readonly SaveService _saveService;

    public PatternHandler(SaveService saveService) {
        _saveService = saveService;
        // load the information from our storage file
        //Load();
    }
    
    public int GetRestraintSetIndex(string setName) {
        // see if the set exists in our list of sets, and if it does, return the index
        for (int i = 0; i < _patterns.Count; i++) {
            if (_patterns[i]._name == setName) {
                return i;
            }
        }
        return -1; // Return -1 if the set name is not found
    }
    
    public void AddNewPattern() {
        var newSet = new PatternData();
        string baseName = newSet._name;
        int copyNumber = 1;
        while (_patterns.Any(set => set._name == newSet._name)) {
            //newSet.ChangeSetName(baseName + $"(copy{copyNumber})");
            copyNumber++;
        }
        _patterns.Add(newSet);
        Save();
    }

    public void RemovePattern(int index) {
        _patterns.RemoveAt(index);
        Save();
    }

    public bool ExecutePattern(string patternName) {
        int patternIdx = _patterns.FindIndex(p => p._name == patternName);
        if (patternIdx == -1) {
            return false;
        }
        // execute the pattern
        _patterns[patternIdx]._isActive = true;
        return true;
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
        };
    }

    public void Load() {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        var file = _saveService.FileNames.RestraintSetsFile;
        _patterns.Clear();
        if (!File.Exists(file)) {
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            var patternsArray = jsonObject["Pattern List"]?.Value<JArray>();
            foreach (var item in patternsArray) {
                var pattern = new PatternData();
                pattern.Deserialize(item.Value<JObject>()); 
                _patterns.Add(pattern);
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"Failure to load automated designs: Error during parsing. {ex}");
        } finally {
            GagSpeak.Log.Debug($"[GagStorageManager] PatternStorage.json loaded! Loaded {_patterns.Count} restraint sets.");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
