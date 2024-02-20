using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

// this will replace all the list variables are normal variables, and remove all non list variables
public class UniquePlayerPerms {
    public bool _grantExtendedLockTimes { get; set; } = false;      // [TIER 2] If you allow the whitelisted player to use extended lock times
    public bool _enableRestraintSets { get; set; } = false;         // [TIER 2] allows dom to enable spesific restraint sets
    public bool _restraintSetLocking { get; set; } = false;         // [TIER 1] enables / disables restraint set locking
    public string _triggerPhraseForPuppeteer { get; set; } = "";    // [TIER 0] YOUR spesific trigger phrase FOR EACH whitelisted player
    public string _StartCharForPuppeteerTrigger { get; set; } = "(";// [TIER 0] what to have instead of () surrounding full commands
    public string _EndCharForPuppeteerTrigger { get; set; } = ")";  // [TIER 0] what to have instead of () surrounding full commands
    public bool _allowSitRequests { get; set; } = false;            // [TIER 1] if you allow the whitelisted player is allowed to use sit requests
    public bool _allowMotionRequests { get; set; } = false;         // [TIER 2] if the whitelisted player is allowed to use motion requests
    public bool _allowAllCommands { get; set; } = false;            // [TIER 4] if the whitelisted user has access to use all commands on you
    public bool _allowChangingToyState { get; set; } = false;       // [TIER 1] Basically, "They can turn on my vibe, at my selected slider position"
    public bool _allowIntensityControl { get; set; } = false;       // [TIER 3] Basically says "This person can adjust the intensity slider"
    public bool _allowUsingPatterns { get; set; } = false;          // [TIER 4] if the whitelisted player is allowed to execute stored patterns
    ///////////////////////////////////////// HARDCORE OPTOINS FOR EACH WHITELIST USER ////////////////////////////////////////////////////
    public List<bool> _legsRestraintedProperty { get; set; }        // (Any action which typically involves fast leg movement is restricted)
    public List<bool> _armsRestraintedProperty { get; set; }        // (Any action which typically involves fast arm movement is restricted)
    public List<bool> _gaggedProperty { get; set; }                 // (Any action requiring speech is restricted)
    public List<bool> _blindfoldedProperty { get; set; }            // (Any actions requiring awareness or sight is restricted)
    public List<bool> _immobileProperty { get; set; }               // (Player becomes unable to move in this set)
    public List<bool> _weightyProperty { get; set; }                // (Player is forced to only walk while wearing this restraint)
    public List<bool> _lightStimulationProperty { get; set; }       // (Any action requring focus or concentration has its casttime being slightly slower)
    public List<bool> _mildStimulationProperty { get; set; }        // (Any action requring focus or concentration has its casttime being noticably slower)
    public List<bool> _heavyStimulationProperty { get; set; }       // (Any action requring focus or concentration has its casttime being significantly slower)
    public bool _followMe { get; set; } = false;                    // When a whitelisted user says "follow me" you will be forced to follow them and your movement is restricted until your character stops moving for a set amount of time.
    public bool _sit { get; set; } = false;                         // when whitelisted user says "sit", you will be forced to sit and your movement is restricted until they say "stand"
    public bool _stayHereForNow { get; set; } = false;              // after a whitelisted player says "stay here for now", teleport and return will be blocked, and your any exit US that pops up will automatically hit no on yesno confirmations... These permissions are restored when they say "come along now"

    public UniquePlayerPerms() {
        _legsRestraintedProperty = new List<bool>() { false };
        _armsRestraintedProperty = new List<bool>() { false };
        _gaggedProperty = new List<bool>() { false };
        _blindfoldedProperty = new List<bool>() { false };
        _immobileProperty = new List<bool>() { false };
        _weightyProperty = new List<bool>() { false };
        _lightStimulationProperty = new List<bool>() { false };
        _mildStimulationProperty = new List<bool>() { false };
        _heavyStimulationProperty = new List<bool>() { false };
    }

    public void ListIntegrityCheck(int sizeOfRestraintSets) {
        // go through all the list variables. If they are not equal to the size of the input, fix them
        _legsRestraintedProperty = CheckAndResizeList(_legsRestraintedProperty, sizeOfRestraintSets);
        _armsRestraintedProperty = CheckAndResizeList(_armsRestraintedProperty, sizeOfRestraintSets);
        _gaggedProperty = CheckAndResizeList(_gaggedProperty, sizeOfRestraintSets);
        _blindfoldedProperty = CheckAndResizeList(_blindfoldedProperty, sizeOfRestraintSets);
        _immobileProperty = CheckAndResizeList(_immobileProperty, sizeOfRestraintSets);
        _weightyProperty = CheckAndResizeList(_weightyProperty, sizeOfRestraintSets);
        _lightStimulationProperty = CheckAndResizeList(_lightStimulationProperty, sizeOfRestraintSets);
        _mildStimulationProperty = CheckAndResizeList(_mildStimulationProperty, sizeOfRestraintSets);
        _heavyStimulationProperty = CheckAndResizeList(_heavyStimulationProperty, sizeOfRestraintSets);
        GagSpeak.Log.Debug($"[UniquePlayerPerms]: Integrity check complete with new size {_legsRestraintedProperty.Count}");
    }

    private List<bool> CheckAndResizeList(List<bool> list, int size) {
        while (list.Count < size) {
            list.Add(false);
        }
        while (list.Count > size) {
            list.RemoveAt(list.Count - 1);
        }
        return list;
    }

#region Serialization
    public JObject Serialize() {
        return new JObject() {
            ["GrantExtendedLockTimes"] = _grantExtendedLockTimes,
            ["EnableRestraintSets"] = _enableRestraintSets,
            ["RestraintSetLocking"] = _restraintSetLocking,
            ["TriggerPhraseForPuppeteer"] = _triggerPhraseForPuppeteer,
            ["StartCharForPuppeteerTrigger"] = _StartCharForPuppeteerTrigger,
            ["EndCharForPuppeteerTrigger"] = _EndCharForPuppeteerTrigger,
            ["AllowSitRequests"] = _allowSitRequests,
            ["AllowMotionRequests"] = _allowMotionRequests,
            ["AllowAllCommands"] = _allowAllCommands,
            ["AllowChangingToyState"] = _allowChangingToyState,
            ["AllowIntensityControl"] = _allowIntensityControl,
            ["AllowUsingPatterns"] = _allowUsingPatterns,
            ["LegsRestraintedProperty"] = JToken.FromObject(_legsRestraintedProperty),
            ["ArmsRestraintedProperty"] = JToken.FromObject(_armsRestraintedProperty),
            ["GaggedProperty"] = JToken.FromObject(_gaggedProperty),
            ["BlindfoldedProperty"] = JToken.FromObject(_blindfoldedProperty),
            ["ImmobileProperty"] = JToken.FromObject(_immobileProperty),
            ["WeightyProperty"] = JToken.FromObject(_weightyProperty),
            ["LightStimulationProperty"] = JToken.FromObject(_lightStimulationProperty),
            ["MildStimulationProperty"] = JToken.FromObject(_mildStimulationProperty),
            ["HeavyStimulationProperty"] = JToken.FromObject(_heavyStimulationProperty),
            ["FollowMe"] = _followMe,
            ["Sit"] = _sit,
            ["StayHereForNow"] = _stayHereForNow,
        };
    }

    public void Deserialize(JObject jsonObject) {
        try{
        _grantExtendedLockTimes = jsonObject["GrantExtendedLockTimes"]?.Value<bool>() ?? false;
        _enableRestraintSets = jsonObject["EnableRestraintSets"]?.Value<bool>() ?? false;
        _restraintSetLocking = jsonObject["RestraintSetLocking"]?.Value<bool>() ?? false;
        _triggerPhraseForPuppeteer = jsonObject["TriggerPhraseForPuppeteer"]?.Value<string>() ?? "";
        _StartCharForPuppeteerTrigger = jsonObject["StartCharForPuppeteerTrigger"]?.Value<string>() ?? "(";
        _EndCharForPuppeteerTrigger = jsonObject["EndCharForPuppeteerTrigger"]?.Value<string>() ?? ")";
        _allowSitRequests = jsonObject["AllowSitRequests"]?.Value<bool>() ?? false;
        _allowMotionRequests = jsonObject["AllowMotionRequests"]?.Value<bool>() ?? false;
        _allowAllCommands = jsonObject["AllowAllCommands"]?.Value<bool>() ?? false;
        _allowChangingToyState = jsonObject["AllowChangingToyState"]?.Value<bool>() ?? false;
        _allowIntensityControl = jsonObject["AllowIntensityControl"]?.Value<bool>() ?? false;
        _allowUsingPatterns = jsonObject["AllowUsingPatterns"]?.Value<bool>() ?? false;
        _legsRestraintedProperty = jsonObject["LegsRestraintedProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _armsRestraintedProperty = jsonObject["ArmsRestraintedProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _gaggedProperty = jsonObject["GaggedProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _blindfoldedProperty = jsonObject["BlindfoldedProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _immobileProperty = jsonObject["ImmobileProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _weightyProperty = jsonObject["WeightyProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _lightStimulationProperty = jsonObject["LightStimulationProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _mildStimulationProperty = jsonObject["MildStimulationProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _heavyStimulationProperty = jsonObject["HeavyStimulationProperty"]?.ToObject<List<bool>>() ?? new List<bool>() { false };
        _followMe = jsonObject["FollowMe"]?.Value<bool>() ?? false;
        _sit = jsonObject["Sit"]?.Value<bool>() ?? false;
        _stayHereForNow = jsonObject["StayHereForNow"]?.Value<bool>() ?? false;
        } catch (Exception e) {
            GagSpeak.Log.Error($"[UniquePlayerPerms]: Error deserializing UniquePlayerPerms: {e.Message}");
        }
    }
#endregion Serialization
}