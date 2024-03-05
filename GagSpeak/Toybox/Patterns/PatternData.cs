using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GagSpeak.ToyboxandPuppeteer;
public class PatternData
{
    public string _name; // the name of the pattern
    public string _description; // the description of the pattern
    public string _duration; // the duration of the pattern
    public bool _selected; // can this pattern be executed
    public bool _isActive; // is the pattern currently active/running
    public bool _loop; // should the pattern loop
    public List<byte> _patternData; // the data for the pattern. Stores the expected intensity of the vibrator every 20ms from range of 0 to 100

    public PatternData() {
        // define default data for the set
        _name = "New Pattern";
        _description = "No Description";
        _duration = "0h00m00s";
        _selected = false;
        _isActive = false;
        _loop = false;
        _patternData = new List<byte>();
    }

    public void ChangePatternName(string name) {
        _name = name;
    }

    public void ChangePatternDescription(string description) {
        _description = description;
    }

    public void ChangeSelectedState(bool selected) {
        _selected = selected;
    }

    public void ChangePatternActive(bool isActive) {
        _isActive = isActive;
    }

    public void ChangePatternLoop(bool loop) {
        _loop = loop;
    }

    public JObject Serialize() {
        // Convert _patternData to a comma-separated string
        string patternDataString = string.Join(",", _patternData);

        return new JObject()
        {
            ["Name"] = _name,
            ["Description"] = _description,
            ["Duration"] = _duration,
            ["Selected"] = _selected,
            ["IsActive"] = _isActive,
            ["Loop"] = _loop,
            ["PatternData"] = patternDataString
        };
    }

    public void Deserialize(JObject jsonObject) {
        try{
            _name = jsonObject["Name"]?.Value<string>() ?? string.Empty;
            _description = jsonObject["Description"]?.Value<string>() ?? string.Empty;
            _duration = jsonObject["Duration"]?.Value<string>() ?? string.Empty;
            _selected = jsonObject["Selected"]?.Value<bool>() ?? false;
            _isActive = jsonObject["IsActive"]?.Value<bool>() ?? false;
            _loop = jsonObject["Loop"]?.Value<bool>() ?? false;
            
            _patternData.Clear();
            var patternDataString = jsonObject["PatternData"]?.Value<string>();
            if (string.IsNullOrEmpty(patternDataString)) {
                // If the string is null or empty, generate a list with a single byte of 0
                _patternData = new List<byte> { (byte)0 };
            } else {
                // Otherwise, split the string into an array and convert each element to a byte
                _patternData = patternDataString.Split(',')
                    .Select(byte.Parse)
                    .ToList();
            }
        } catch (System.Exception e) {
            GSLogger.LogType.Debug($"{e} Error deserializing pattern data");
        }
    }
}