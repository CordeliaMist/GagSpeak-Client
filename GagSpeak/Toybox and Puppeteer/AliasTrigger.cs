using Newtonsoft.Json.Linq;

namespace GagSpeak.ToyboxandPuppeteer;

/// <summary>
/// Stores a list of alias triggers. This is intended to be applied once for each player in your whitelist.
/// </summary>
public class AliasTrigger
{
    // the list stores the searched for input command on the left, and the output command on the right
    public bool _enabled;
    public string _inputCommand;
    public string _outputCommand;
    // potentially store subset commands that can chain with eachother like psuedo-macros, but for now just single.

    public AliasTrigger() {
        _enabled = false;
        _inputCommand = string.Empty;
        _outputCommand = string.Empty;
    }

    public JObject Serialize() {
        return new JObject()
        {
            ["Enabled"] = _enabled,
            ["InputCommand"] = _inputCommand,
            ["OutputCommand"] = _outputCommand
        };
    }

    public void Deserialize(JObject jsonObject) {
        _enabled = jsonObject["Enabled"]?.Value<bool>() ?? false;
        _inputCommand = jsonObject["InputCommand"]?.Value<string>() ?? string.Empty;
        _outputCommand = jsonObject["OutputCommand"]?.Value<string>() ?? string.Empty;
    }
}