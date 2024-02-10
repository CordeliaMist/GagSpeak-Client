using System.Collections.Generic;
using System.Linq;
using GagSpeak.ToyboxandPuppeteer;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

// Settings set by the player & not visable to whitelisted players (REQUESTING INFO SENDS THEM INFO FROM WHITELISTCHARDATA & INFO BASE)
public class PlayerCharacterInfo : CharacterInfoBase
{
    //////////////////////////////////////// PREFERENCES FOR NON-WHITELISTED PLAYERS  ///////////////////////////////////////
    public  string          _safeword { get; set; } = "safeword";               // What is the safeword?
    public  bool            _doCmdsFromFriends { get; set; } = false;           // gives anyone on your friendlist access to use GagSpeak commands on you
    public  bool            _doCmdsFromParty { get; set; } = false;             // gives anyone in your party access to use GagSpeak commands on you
    public  bool            _liveGarblerWarnOnZoneChange { get; set; } = false; // enables or disables the live garbler warning on zone change
    ///////////////////////////////////////////// WARDROBE COMPONENT SETTINGS  /////////////////////////////////////////////
    public  bool            _allowItemAutoEquip { get; set; } = false;          // lets player know if anything in the GagStorage compartment will function
    public  bool            _allowRestraintSetAutoEquip { get; set; } = false;  // lets gagspeak know if anything in the Restraintset compartment will function
    ///////////////////////////////////////////// GAGSPEAK PUPPETEER SETTINGS  /////////////////////////////////////////////
    public  List<AliasList> _triggerAliases { get; set; }                       // lets the player set the trigger phrases for each whitelisted player
    ///////////////////////////////////////////// FUTURE MODULES CAN GO HERE /////////////////////////////////////////////

    ///////////////////////////////////////// FIELDS UNIQUE FOR EACH WHITELIST USER ////////////////////////////////////////////////////
    public  List<bool>      _grantExtendedLockTimes { get; set; }         // [TIER 2] If you allow the whitelisted player to use extended lock times
    public  List<bool>      _enableRestraintSets { get; set; }            // [TIER 2] allows dom to enable spesific restraint sets
    public  List<bool>      _restraintSetLocking { get; set; }            // [TIER 1] enables / disables restraint set locking 
    public  List<string>    _triggerPhraseForPuppeteer { get; set; }      // [TIER 0] YOUR spesific trigger phrase FOR EACH whitelisted player
    public  List<string>    _StartCharForPuppeteerTrigger { get; set; }   // [TIER 0] what to have instead of () surrounding full commands
    public  List<string>    _EndCharForPuppeteerTrigger { get; set; }     // [TIER 0] what to have instead of () surrounding full commands
    public  List<bool>      _allowSitRequests { get; set; }               // [TIER 1] if you allow the whitelisted player is allowed to use sit requests
    public  List<bool>      _allowMotionRequests { get; set; }            // [TIER 2] if the whitelisted player is allowed to use motion requests
    public  List<bool>      _allowAllCommands { get; set; }               // [TIER 4] if the whitelisted user has access to use all commands on you
    public  List<bool>      _allowChangingToyState { get; set; }          // [TIER 1] Basically, "They can turn on my vibe, at my selected slider position"
    public  List<bool>      _allowIntensityControl { get; set; }          // [TIER 3] Basically says "This person can adjust the intensity slider"
    public  List<bool>      _allowUsingPatterns { get; set; }             // [TIER 4] if the whitelisted player is allowed to execute stored patterns

    public PlayerCharacterInfo() {
        _triggerAliases = new List<AliasList>() { new AliasList() };
        _grantExtendedLockTimes = new List<bool>() { false };
        _enableRestraintSets = new List<bool>() { false };
        _restraintSetLocking = new List<bool>() { false };
        _triggerPhraseForPuppeteer = new List<string>() { "" };
        _StartCharForPuppeteerTrigger = new List<string>() { "(" };
        _EndCharForPuppeteerTrigger = new List<string>() { ")" };
        _allowSitRequests = new List<bool>() { false };
        _allowMotionRequests = new List<bool>() { false };
        _allowAllCommands = new List<bool>() { false };
        _allowChangingToyState = new List<bool>() { false };
        _allowIntensityControl = new List<bool>() { false };
        _allowUsingPatterns = new List<bool>() { false };
    }

#region Serialization
    public override JObject Serialize() {
        JObject derivedSerialized = new JObject() {
            ["Safeword"] = _safeword,
            ["DoCmdsFromFriends"] = _doCmdsFromFriends,
            ["DoCmdsFromParty"] = _doCmdsFromParty,
            ["LiveGarblerWarnOnZoneChange"] = _liveGarblerWarnOnZoneChange,
            ["AllowItemAutoEquip"] = _allowItemAutoEquip,
            ["AllowRestraintSetAutoEquip"] = _allowRestraintSetAutoEquip,
            ["TriggerAliasesList"] = new JArray(_triggerAliases.Select(alias => alias.Serialize())),
            ["ExtendedLockTimes"] = new JArray(_grantExtendedLockTimes),
            ["EnableRestraintSets"] = new JArray(_enableRestraintSets),
            ["RestraintSetLocking"] = new JArray(_restraintSetLocking),
            ["TriggerPhrase"] = new JArray(_triggerPhraseForPuppeteer),
            ["StartCharForPuppeteerTrigger"] = new JArray(_StartCharForPuppeteerTrigger),
            ["EndCharForPuppeteerTrigger"] = new JArray(_EndCharForPuppeteerTrigger),
            ["AllowSitRequests"] = new JArray(_allowSitRequests),
            ["AllowMotionRequests"] = new JArray(_allowMotionRequests),
            ["AllowAllCommands"] = new JArray(_allowAllCommands),
            ["AllowChangingToyState"] = new JArray(_allowChangingToyState),
            ["AllowIntensityControl"] = new JArray(_allowIntensityControl),
            ["AllowUsingPatterns"] = new JArray(_allowUsingPatterns)
        };
        JObject baseSerialized = base.Serialize();
        derivedSerialized.Merge(baseSerialized);
        return derivedSerialized;
    }

    public override void Deserialize(JObject jsonObject) {
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
            _allowItemAutoEquip = jsonObject["AllowItemAutoEquip"]?.Value<bool>() ?? false;
            _allowRestraintSetAutoEquip = jsonObject["AllowRestraintSetAutoEquip"]?.Value<bool>() ?? false;
            _grantExtendedLockTimes = jsonObject["ExtendedLockTimes"]?.Values<bool>().ToList() ?? new List<bool>();
            _enableRestraintSets = jsonObject["EnableRestraintSets"]?.Values<bool>().ToList() ?? new List<bool>();
            _restraintSetLocking = jsonObject["RestraintSetLocking"]?.Values<bool>().ToList() ?? new List<bool>();
            _triggerPhraseForPuppeteer = jsonObject["TriggerPhrase"]?.Values<string>().Select(s => s ?? "").ToList() ?? new List<string>();
            _StartCharForPuppeteerTrigger = jsonObject["StartCharForPuppeteerTrigger"]?.Values<string>().Select(s => s ?? "(").ToList() ?? new List<string> { "(" };
            _EndCharForPuppeteerTrigger = jsonObject["EndCharForPuppeteerTrigger"]?.Values<string>().Select(s => s ?? ")").ToList() ?? new List<string> { ")" };
            _allowSitRequests = jsonObject["AllowSitRequests"]?.Values<bool>().ToList() ?? new List<bool>();
            _allowMotionRequests = jsonObject["AllowMotionRequests"]?.Values<bool>().ToList() ?? new List<bool>();
            _allowAllCommands = jsonObject["AllowAllCommands"]?.Values<bool>().ToList() ?? new List<bool>();
            _allowChangingToyState = jsonObject["AllowChangingToyState"]?.Values<bool>().ToList() ?? new List<bool>();
            _allowIntensityControl = jsonObject["AllowIntensityControl"]?.Values<bool>().ToList() ?? new List<bool>();
            _allowUsingPatterns = jsonObject["AllowUsingPatterns"]?.Values<bool>().ToList() ?? new List<bool>();
        }
        catch (System.Exception e) {
            GagSpeak.Log.Error($"[PlayerCharacterInfo]: Error deserializing PlayerCharacterInfo: {e}");
        }
        #pragma warning restore CS8604 // Possible null reference argument.
    }
#endregion Serialization
}