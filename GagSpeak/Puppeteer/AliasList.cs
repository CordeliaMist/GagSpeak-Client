using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GagSpeak.ToyboxandPuppeteer;

/// <summary>
/// Stores a list of alias triggers. This is intended to be applied once for each player in your whitelist.
/// </summary>
public class AliasList
{
    // the list stores the searched for input command on the left, and the output command on the right
    public List<AliasTrigger> _aliasTriggers;

    /// <summary> Constructor for the AliasTriggerList. </summary>
    public AliasList() {
        _aliasTriggers = new List<AliasTrigger>();
    }

    /// <summary> Returns the number of aliases in the list. </summary>
    public bool IsValidAlias(AliasTrigger alias) {
        return !string.IsNullOrEmpty(alias._inputCommand) && !string.IsNullOrEmpty(alias._outputCommand);
    }

    /// <summary> Returns the number of aliases in the list. </summary>
    public AliasTrigger GetAliasByIdx(int idx) {
        return _aliasTriggers[idx];
    }

    /// <summary> Adds the alias to the list. </summary>
    public void AddAlias(AliasTrigger alias) {
        if (IsValidAlias(alias)) {
            _aliasTriggers.Add(alias);
        }
    }

    /// <summary> Removes the alias at the specified index. </summary>
    public void RemoveAlias(int idx) {
        _aliasTriggers.RemoveAt(idx);
    }

    /// <summary> Returns the number of aliases in the list. </summary>
    public JObject Serialize() {
        // we will create another array, storing the draw data for the restraint set
        var aliasArray = new JArray();
        // for each of the draw data, serialize them and add them to the array
        foreach (var alias in _aliasTriggers) {
            aliasArray.Add(alias.Serialize());
        }

        return new JObject()
        {
            ["AliasTriggers"] = aliasArray
        };
    }

    /// <summary> Deserializes the alias list from the provided JSON object. </summary>
    public void Deserialize(JObject jsonObject) {
        _aliasTriggers.Clear();
        var aliasArray = jsonObject["AliasTriggers"]?.Value<JArray>();
        if (aliasArray != null) {
            foreach (var item in aliasArray) {
                var aliasObject = item.Value<JObject>();
                if (aliasObject != null) {
                    var alias = new AliasTrigger();
                    alias.Deserialize(aliasObject);
                    if (IsValidAlias(alias)) {
                        _aliasTriggers.Add(alias);
                    }
                }
            }
        }
    }
}