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

    public UniquePlayerPerms() { }

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
        } catch (Exception e) {
            GagSpeak.Log.Error($"[UniquePlayerPerms]: Error deserializing UniquePlayerPerms: {e.Message}");
        }
    }
#endregion Serialization
}