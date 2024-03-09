using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using GagSpeak.Events;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Wardrobe;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

// Settings set by the player & not visable to whitelisted players (REQUESTING INFO SENDS THEM INFO FROM WHITELISTCHARDATA & INFO BASE)
public enum RevertStyle {
    ToGameOnly,
    ToAutomationOnly,
    ToGameThenAutomation,
}

public class PlayerGlobalPerms : CharacterInfoBase
{
    //////////////////////////////////////// PREFERENCES FOR NON-WHITELISTED PLAYERS  ///////////////////////////////////////
    public  string          _safeword { get; set; } = "safeword";                       // What is the safeword?
    public  bool            _doCmdsFromFriends { get; set; } = false;                   // gives anyone on your friendlist access to use GagSpeak commands on you
    public  bool            _doCmdsFromParty { get; set; } = false;                     // gives anyone in your party access to use GagSpeak commands on you
    public  bool            _liveGarblerWarnOnZoneChange { get; set; } = false;         // enables or disables the live garbler warning on zone change
    public  RevertStyle     _revertStyle { get; set; } = RevertStyle.ToGameOnly;  // determines if you revert to automation or to game
    public  string          _globalTriggerPhrase { get; set; } = "";                    // the global trigger phrase for the puppeteer
    public  bool            _globalAllowSitRequests { get; set; } = false;              // if you allow anyone to use sit requests
    public  bool            _globalAllowMotionRequests { get; set; } = false;           // if you allow anyone to use motion requests
    public  bool            _globalAllowAllCommands { get; set; } = false;              // if you allow anyone to use all commands on you

    ///////////////////////////////////////////// WARDROBE COMPONENT SETTINGS  /////////////////////////////////////////////
    public  bool            _allowItemAutoEquip { get; set; } = false;                  // lets player know if anything in the GagStorage compartment will function
    public  bool            _allowRestraintSetAutoEquip { get; set; } = false;          // lets gagspeak know if anything in the Restraintset compartment will function
    ///////////////////////////////////////////// GAGSPEAK PUPPETEER SETTINGS  /////////////////////////////////////////////
    public  List<AliasList> _triggerAliases { get; set; }                               // lets the player set the trigger phrases for each whitelisted player
    //////////////////////////////////////////////// TOYBOX MODULE SETTING /////////////////////////////////////////////////
    public  bool            _usingSimulatedVibe { get; set; } = false;                  // lets the player know if they are using a simulated vibe
    
    ///////////////////////////////////////// FIELDS UNIQUE FOR EACH WHITELIST USER ////////////////////////////////////////////////////
    public List<UniquePlayerPerms> _uniquePlayerPerms { get; set; }                      // list of unique player permissions set for each whitelisted user.
        
    public PlayerGlobalPerms() {
        _triggerAliases = new List<AliasList>() { new AliasList() };
        _uniquePlayerPerms = new List<UniquePlayerPerms>() { new UniquePlayerPerms() };
    }

    public void AddNewWhitelistItemPerms() {
        // update the player chars things to match the whitelist edit
        _triggerAliases.Add(new AliasList());
        _uniquePlayerPerms.Add(new UniquePlayerPerms());
    }

    public void ReplaceWhitelistItemPerms(int index) {
        // update the player chars things to match the whitelist edit
        _triggerAliases[index] = new AliasList();
        _uniquePlayerPerms[index] = new UniquePlayerPerms();
    }

    public void RemoveWhitelistItemPerms(int index) {
        // update the player chars things to match the whitelist edit
        _triggerAliases.RemoveAt(index);
        _uniquePlayerPerms.RemoveAt(index);
    }

#region Serialization
    public override JObject Serialize() {
        JObject derivedSerialized = new JObject() {
            ["Safeword"] = _safeword,
            ["DoCmdsFromFriends"] = _doCmdsFromFriends,
            ["DoCmdsFromParty"] = _doCmdsFromParty,
            ["LiveGarblerWarnOnZoneChange"] = _liveGarblerWarnOnZoneChange,
            ["RevertStyle"] = _revertStyle.ToString(),
            ["GlobalTriggerPhrase"] = _globalTriggerPhrase,
            ["GlobalAllowSitRequests"] = _globalAllowSitRequests,
            ["GlobalAllowMotionRequests"] = _globalAllowMotionRequests,
            ["GlobalAllowAllCommands"] = _globalAllowAllCommands,
            ["AllowItemAutoEquip"] = _allowItemAutoEquip,
            ["AllowRestraintSetAutoEquip"] = _allowRestraintSetAutoEquip,
            ["TriggerAliasesList"] = new JArray(_triggerAliases.Select(alias => alias.Serialize())),
            ["UsingSimulatedVibe"] = _usingSimulatedVibe,
            ["UniquePlayerPerms"] = new JArray(_uniquePlayerPerms.Select(perm => perm.Serialize())),
        };
        JObject baseSerialized = base.Serialize();
        derivedSerialized.Merge(baseSerialized);
        return derivedSerialized;
    }

    public override void Deserialize(JObject jsonObject) {
        // we need to know which config we have before we deserialize, if it was the old one or the new one.
        _uniquePlayerPerms.Clear();
        var uniquePlayerPermsArray = jsonObject["UniquePlayerPerms"]?.Value<JArray>();
        // see if uniquePlayerPerms is an empty array
        if (uniquePlayerPermsArray == null || uniquePlayerPermsArray.Count == 0) {
            GSLogger.LogType.Debug($"[PlayerGlobalPerms]: We Have an outdated file!");
            DeserializeOld(jsonObject);
        } else {
            GSLogger.LogType.Debug($"[PlayerGlobalPerms]: We Have Most Recently updated file!");
            DeserializeNew(jsonObject);
            // will need to clear and then deserialize the trigger aliass
        }
    }

    private void DeserializeOld(JObject jsonObject) {
        #pragma warning disable CS8604 // Possible null reference argument.
        try{
            // will need to clear and then deserialize the trigger aliass
            _triggerAliases.Clear();
            var triggerAliasesArray = jsonObject["TriggerAliasesList"]?.Value<JArray>();
            if (triggerAliasesArray != null) {
                foreach (var item in triggerAliasesArray) {
                    var alias = new AliasList();
                    alias.Deserialize(item.Value<JObject>());
                    _triggerAliases.Add(alias);
                }
            }
            base.Deserialize(jsonObject);
            _safeword = jsonObject["Safeword"]?.Value<string>() ?? "safeword";
            _doCmdsFromFriends = jsonObject["DoCmdsFromFriends"]?.Value<bool>() ?? false;
            _doCmdsFromParty = jsonObject["DoCmdsFromParty"]?.Value<bool>() ?? false;
            _liveGarblerWarnOnZoneChange = jsonObject["LiveGarblerWarnOnZoneChange"]?.Value<bool>() ?? false;
            _revertStyle = Enum.TryParse(jsonObject["RevertStyle"]?.Value<string>(), out RevertStyle revertstyle) ? revertstyle : RevertStyle.ToAutomationOnly;
            _globalTriggerPhrase = jsonObject["GlobalTriggerPhrase"]?.Value<string>() ?? "";
            _globalAllowSitRequests = jsonObject["GlobalAllowSitRequests"]?.Value<bool>() ?? false;
            _globalAllowMotionRequests = jsonObject["GlobalAllowMotionRequests"]?.Value<bool>() ?? false;
            _globalAllowAllCommands = jsonObject["GlobalAllowAllCommands"]?.Value<bool>() ?? false;
            _allowItemAutoEquip = jsonObject["AllowItemAutoEquip"]?.Value<bool>() ?? false;
            _allowRestraintSetAutoEquip = jsonObject["AllowRestraintSetAutoEquip"]?.Value<bool>() ?? false;
            _usingSimulatedVibe = jsonObject["UsingSimulatedVibe"]?.Value<bool>() ?? false;
            var tempLockTimesBoolList = jsonObject["ExtendedLockTimes"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempEnableRestraintSets = jsonObject["EnableRestraintSets"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempRestraintSetLocking = jsonObject["RestraintSetLocking"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempTriggerPhraseForPuppeteer = jsonObject["TriggerPhrase"]?.Values<string>().Select(s => s ?? "").ToList() ?? new List<string>();
            var tempStartCharForPuppeteerTrigger = jsonObject["StartCharForPuppeteerTrigger"]?.Values<string>().Select(s => s ?? "(").ToList() ?? new List<string> { "(" };
            var tempEndCharForPuppeteerTrigger = jsonObject["EndCharForPuppeteerTrigger"]?.Values<string>().Select(s => s ?? ")").ToList() ?? new List<string> { ")" };
            var tempAllowSitRequests = jsonObject["AllowSitRequests"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempAllowMotionRequests = jsonObject["AllowMotionRequests"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempAllowAllCommands = jsonObject["AllowAllCommands"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempAllowChangingToyState = jsonObject["AllowChangingToyState"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempAllowIntensityControl = jsonObject["AllowIntensityControl"]?.Values<bool>().ToList() ?? new List<bool>();
            var tempAllowUsingPatterns = jsonObject["AllowUsingPatterns"]?.Values<bool>().ToList() ?? new List<bool>();
            // Assuming all the temporary lists have the same count
            for (int i = 0; i < tempLockTimesBoolList.Count; i++)
            {
                var perm = new UniquePlayerPerms
                {
                    _grantExtendedLockTimes = tempLockTimesBoolList[i],
                    _enableRestraintSets = tempEnableRestraintSets[i],
                    _restraintSetLocking = tempRestraintSetLocking[i],
                    _triggerPhraseForPuppeteer = tempTriggerPhraseForPuppeteer[i],
                    _StartCharForPuppeteerTrigger = tempStartCharForPuppeteerTrigger[i],
                    _EndCharForPuppeteerTrigger = tempEndCharForPuppeteerTrigger[i],
                    _allowSitRequests = tempAllowSitRequests[i],
                    _allowMotionRequests = tempAllowMotionRequests[i],
                    _allowAllCommands = tempAllowAllCommands[i],
                    _allowChangingToyState = tempAllowChangingToyState[i],
                    _allowIntensityControl = tempAllowIntensityControl[i],
                    _allowUsingPatterns = tempAllowUsingPatterns[i]
                };
                _uniquePlayerPerms.Add(perm);
            }
        }
        catch (System.Exception e) {
            GSLogger.LogType.Error($"[PlayerCharacterInfo]: Error deserializing PlayerCharacterInfo: {e}");
        }
    }

    private void DeserializeNew(JObject jsonObject) {
        try{
            // will need to clear and then deserialize the trigger aliass
            _triggerAliases.Clear();
            var triggerAliasesArray = jsonObject["TriggerAliasesList"]?.Value<JArray>();
            if (triggerAliasesArray != null) {
                foreach (var item in triggerAliasesArray) {
                    var alias = new AliasList();
                    alias.Deserialize(item.Value<JObject>());
                    _triggerAliases.Add(alias);
                }
                GSLogger.LogType.Debug($"[PlayerGlobalPerms]: Deserialized {triggerAliasesArray.Count} TriggerAliases");
            }
            // for the unique player perms
            _uniquePlayerPerms.Clear();
            var uniquePlayerPermsArray = jsonObject["UniquePlayerPerms"]?.Value<JArray>();
            if (uniquePlayerPermsArray != null) {
                foreach (var item in uniquePlayerPermsArray) {
                    var perm = new UniquePlayerPerms();
                    perm.Deserialize(item.Value<JObject>());
                    _uniquePlayerPerms.Add(perm);
                }
                GSLogger.LogType.Debug($"[PlayerGlobalPerms]: Deserialized {uniquePlayerPermsArray.Count} UniquePlayerPerms");
            }
            // deserialize the rest of the base class
            base.Deserialize(jsonObject);
            _safeword = jsonObject["Safeword"]?.Value<string>() ?? "safeword";
            _doCmdsFromFriends = jsonObject["DoCmdsFromFriends"]?.Value<bool>() ?? false;
            _doCmdsFromParty = jsonObject["DoCmdsFromParty"]?.Value<bool>() ?? false;
            _liveGarblerWarnOnZoneChange = jsonObject["LiveGarblerWarnOnZoneChange"]?.Value<bool>() ?? false;
            _revertStyle = Enum.TryParse(jsonObject["RevertStyle"]?.Value<string>(), out RevertStyle revertstyle) ? revertstyle : RevertStyle.ToAutomationOnly;
            _globalTriggerPhrase = jsonObject["GlobalTriggerPhrase"]?.Value<string>() ?? "";
            _globalAllowSitRequests = jsonObject["GlobalAllowSitRequests"]?.Value<bool>() ?? false;
            _globalAllowMotionRequests = jsonObject["GlobalAllowMotionRequests"]?.Value<bool>() ?? false;
            _globalAllowAllCommands = jsonObject["GlobalAllowAllCommands"]?.Value<bool>() ?? false;
            _allowItemAutoEquip = jsonObject["AllowItemAutoEquip"]?.Value<bool>() ?? false;
            _allowRestraintSetAutoEquip = jsonObject["AllowRestraintSetAutoEquip"]?.Value<bool>() ?? false;
            _usingSimulatedVibe = jsonObject["UsingSimulatedVibe"]?.Value<bool>() ?? false;

        }
        catch (System.Exception e) {
            GSLogger.LogType.Error($"[PlayerGlobalPerms]: Error deserializing PlayerGlobalPerms: {e}");
        }
        #pragma warning restore CS8604 // Possible null reference argument.
    }
#endregion Serialization
}