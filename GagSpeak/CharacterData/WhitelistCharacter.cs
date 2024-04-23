using System;
using System.Collections.Generic;
using GagSpeak.Gagsandlocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

/// <summary> A class to hold the data for the whitelist character </summary>
public class WhitelistedCharacterInfo : CharacterInfoBase
{
    public List<(string _name, string _homeworld)> _charNAW { get; set; }  // get the character name and world
    public int              _charNAWIdxToProcess { get; set; } = 0;                 // get the index of the character name and world to process
    public RoleLean         _yourStatusToThem { get; set; }                         // who you are to them
    public RoleLean         _theirStatusToYou { get; set; }                         // who they are to you
    public RoleLean         _pendingRelationRequestFromYou { get; set; }            // displays the current dyanmic request sent by you to this player
    public RoleLean         _pendingRelationRequestFromPlayer { get; set; }         // displays the current dynamic request from this player to you
    public DateTimeOffset   _timeOfCommitment { get; set; }                         // how long has your commitment lasted?
    //////////////////// FIELDS THAT ARE TWO WAY MODIFIABLE ////////////////////
    public bool             _grantExtendedLockTimes { get; set; } = false;          // [TIER 2] if whitelisted user allows you to use extended lock times
    public bool             _inHardcoreMode { get; set; } = false;                  // [TIER 0] if they are in hardcore mode
    public bool             _enableRestraintSets { get; set; } = false;             // [TIER 2] allows dom to enable spesific restraint sets
    public bool             _restraintSetLocking { get; set; } = false;             // [TIER 1] enables / disables all restraint set locking 
    public string           _theirTriggerPhrase { get; set; } = "";                 // [TIER 0] this whitelisted user's trigger phrase
    public string           _theirTriggerStartChar { get; set; } = "(";             // [TIER 0] what to have instead of () surrounding full commands
    public string           _theirTriggerEndChar { get; set; } = ")";               // [TIER 0] what to have instead of () surrounding full commands
    public bool             _allowsSitRequests { get; set; } = false;               // [TIER 1] if they allow you to use sit requests
    public bool             _allowsMotionRequests { get; set; } = false;            // [TIER 2] if they allow you to use motion requests
    public bool             _allowsAllCommands { get; set; } = false;               // [TIER 4] If they allow you to use all commands on them
    public bool             _allowChangingToyState { get; set; } = false;           // [TIER 1] Basically, "They can turn on my vibe, at my selected slider position"
    public bool             _allowsIntensityControl { get; set; } = false;          // [TIER 3] Basically says "This person can adjust the intensity slider"
    public bool             _allowsUsingPatterns { get; set; } = false;             // [TIER 0] Do they allow you to execute stored patterns
    public int              _activeToystepSize { get; set; } = 0;                   // [TIER 0] the step count of the vibe
    /////////////// STORED INFORMATION ABOUT A PLAYERS HARDCORE SETTINGS ///////////////////////
    public bool             _allowForcedFollow { get; set; } = false;               // ONLYGIVENBYPLAYER if they allow you to use follow orders
    public bool             _forcedFollow { get; set; } = false;                    // [TIER 0] Any tier can, but player must consent by enabling above perm
    public bool             _allowForcedSit { get; set; } = false;                  // ONLYGIVENBYPLAYER if they allow you to use sit orders
    public bool             _forcedSit { get; set; } = false;                       // [TIER 0] Any tier can, but player must consent by enabling above perm
    public bool             _allowForcedToStay { get; set; } = false;               // ONLYGIVENBYPLAYER if they allow you to use stay orders
    public bool             _forcedToStay { get; set; } = false;                    // [TIER 0] Any tier can, but player must consent by enabling above perm
    public bool             _allowBlindfold { get; set; } = false;                  // ONLYGIVENBYPLAYER if they allow you to use blindfold
    public bool             _blindfolded { get; set; } = false;                     // [TIER 0] Any tier can, but player must consent by enabling above perm

    //////////////////// FIELDS TO BE USED TO STORE THIS PERSONS LIST NAMES ////////////////////
    public List<string>                 _storedRestraintSets { get; set; }          // contains the list of restraint set names (if imported)
    public Dictionary<string, string>   _storedAliases { get; set; }                // contains the list of gag types (if imported)
    public List<string>                 _storedPatternNames { get; set; }           // contains the list of pattern names (if imported)

    ////////////////////////////////////////////////// PROTECTED FIELDS ////////////////////////////////////////////////////
    
    public WhitelistedCharacterInfo() : this("None None", "None") { }
    public WhitelistedCharacterInfo(string name, string homeworld) {
        _charNAW = new List<(string, string)>() { (name, homeworld) };
        // _name = name;
        // _homeworld = homeworld;
        _yourStatusToThem = RoleLean.None;
        _theirStatusToYou = RoleLean.None;
        _pendingRelationRequestFromPlayer = RoleLean.None;
        _pendingRelationRequestFromYou = RoleLean.None;
        _timeOfCommitment = DateTimeOffset.Now;
        // create new lists for the stored items
        _storedRestraintSets = new List<string>();
        _storedAliases = new Dictionary<string, string>();
        _storedPatternNames = new List<string>();
    }
#region General Interactions
    public void RemoveAltCharacter(int index) {
        if (index < 0 || index >= _charNAW.Count) return;
        _charNAW.RemoveAt(index);
        _charNAWIdxToProcess = 0;
    }

    public bool IsRoleLeanDominant(RoleLean roleLean) {
        if(roleLean == RoleLean.Dominant || roleLean == RoleLean.Mistress || roleLean == RoleLean.Master || roleLean == RoleLean.Owner) {
            return true;
        }
        return false;
    }

    public bool IsRoleLeanSubmissive(RoleLean roleLean) {
        if(roleLean == RoleLean.Submissive || roleLean == RoleLean.Pet || roleLean == RoleLean.Slave || roleLean == RoleLean.AbsoluteSlave) {
            return true;
        }
        return false;
    }
    /// <summary> Sets the time of commitment </summary>
    public void Set_timeOfCommitment() {
        _timeOfCommitment = DateTimeOffset.Now;
    }

    /// <summary> gets the time of commitment </summary>
    /// <returns>The time of commitment.</returns>
    public string GetCommitmentDuration() {
        if (_timeOfCommitment == default(DateTimeOffset))
            return ""; // Display nothing if commitment time is not set
        TimeSpan duration = DateTimeOffset.Now - _timeOfCommitment; // Get the duration
        int days = duration.Days;
        // Display the duration in the desired format
        return $"{days}d, {duration.Hours}h, {duration.Minutes}m, {duration.Seconds}s";
    }

    /// <summary> gets the duration left on a timed padlock type specified by the index </summary>
    /// <returns>The duration left on the padlock.</returns>
    public string GetPadlockTimerDurationLeft(int index) {
        TimeSpan duration = _selectedGagPadlockTimer[index] - DateTimeOffset.Now; // Get the duration
        if (duration < TimeSpan.Zero) {
            // check if the padlock type was a type with a timer, and if so, set the other stuff to none
            if (_selectedGagPadlocks[index] == Padlocks.FiveMinutesPadlock
            ||  _selectedGagPadlocks[index] == Padlocks.MistressTimerPadlock
            ||  _selectedGagPadlocks[index] == Padlocks.TimerPasswordPadlock)
            {
                // set the padlock type to none
                _selectedGagPadlocks[index] = Padlocks.None;
                _selectedGagPadlockPassword[index] = "";
                _selectedGagPadlockAssigner[index] = "";
            }
            return "";
        }
        // get the format to display
        string ret = "";
        if (duration.Days > 0)
            ret += $"{duration.Days}d, ";
        
        if (duration.Hours > 0)
            ret += $"{duration.Hours}h, ";
        
        if (duration.Minutes > 0)
            ret += $"{duration.Minutes}m, ";
        
        ret += $"{duration.Seconds}s";

        return ret;
    }
#endregion General Interactions
#region State Fetching / Setting
    /// <summary> get the spesified tier of a current dynamic </summary>
    public DynamicTier GetDynamicTierClient() {
        // TIER 4 == dynamic of ABSOLUTE-SLAVE with OWNER.
        if (_yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier4;
        }}
        // TIER 3 == dynamic of SLAVE/ABSOLUTE-SLAVE with OWNER.
        if (_yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier3;
        }}
        // TIER 2 == dynamic of SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        if (_yourStatusToThem == RoleLean.Mistress || _yourStatusToThem == RoleLean.Master || _yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier2;
        }}
        // TIER 1 == dynamic of PET/SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        if (_yourStatusToThem == RoleLean.Mistress || _yourStatusToThem == RoleLean.Master || _yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Pet || _theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier1;
        }}
        // If a two way dyanamic is note yet established, then our tier is 0.
        if (_yourStatusToThem == RoleLean.None || _yourStatusToThem == RoleLean.Dominant || _theirStatusToYou == RoleLean.None || _theirStatusToYou == RoleLean.Submissive) {
                return DynamicTier.Tier0;
        }
        // we should never make it here, but if we do, set the dynamic to 0 anyways
        return DynamicTier.Tier0;
    }

    /// <summary> get the spesified tier of a current dynamic (for using someone who isnt you) (meant for result logic) </summary>
    public DynamicTier GetDynamicTierNonClient() {
        // TIER 4 == dynamic of ABSOLUTE-SLAVE with OWNER.
        if (_theirStatusToYou == RoleLean.Owner) {
            if(_yourStatusToThem == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier4;
        }}
        // TIER 3 == dynamic of SLAVE/ABSOLUTE-SLAVE with OWNER.
        if (_theirStatusToYou == RoleLean.Owner) {
            if(_yourStatusToThem == RoleLean.Slave || _yourStatusToThem == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier3;
        }}
        // TIER 2 == dynamic of SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        if (_theirStatusToYou == RoleLean.Mistress || _theirStatusToYou == RoleLean.Master || _theirStatusToYou == RoleLean.Owner) {
            if(_yourStatusToThem == RoleLean.Slave || _yourStatusToThem == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier2;
        }}
        // TIER 1 == dynamic of PET/SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        if (_theirStatusToYou == RoleLean.Mistress || _theirStatusToYou == RoleLean.Master || _theirStatusToYou == RoleLean.Owner) {
            if(_yourStatusToThem == RoleLean.Pet || _yourStatusToThem == RoleLean.Slave || _yourStatusToThem == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier1;
        }}
        // If a two way dyanamic is note yet established, then our tier is 0.
        if (_theirStatusToYou == RoleLean.None || _theirStatusToYou == RoleLean.Dominant || _yourStatusToThem == RoleLean.None || _yourStatusToThem == RoleLean.Submissive) {
                return DynamicTier.Tier0;
        }
        // we should never make it here, but if we do, set the dynamic to 0 anyways
        return DynamicTier.Tier0;
    }
#endregion State Fetching / Setting

#region Serialization and Deserialization
    public override JObject Serialize() {
        try{
            JObject derivedSerialized = new JObject() {
                ["CharNameAndWorld"] = JToken.FromObject(_charNAW), // ["Name"] = _name, ["Homeworld"] = _homeworld,
                ["CharNAWIdxToProcess"] = _charNAWIdxToProcess,
                ["YourStatusToThem"] = _yourStatusToThem.ToString(),
                ["TheirStatusToYou"] = _theirStatusToYou.ToString(),
                ["PendingRequestFromYou"] = _pendingRelationRequestFromYou.ToString(),
                ["PendingRequestFromPlayer"] = _pendingRelationRequestFromPlayer.ToString(),
                ["TimeOfCommitment"] = JsonConvert.SerializeObject(_timeOfCommitment),
                ["ExtendedLockTimes"] = _grantExtendedLockTimes,
                ["InHardcoreMode"] = _inHardcoreMode,
                
                ["EnableRestraintSets"] = _enableRestraintSets,
                ["RestraintSetLocking"] = _restraintSetLocking,
                
                ["TriggerPhrase"] = _theirTriggerPhrase,
                ["StartCharForPuppeteerTrigger"] = _theirTriggerStartChar,
                ["EndCharForPuppeteerTrigger"] = _theirTriggerEndChar,
                ["AllowsSitRequests"] = _allowsSitRequests,
                ["AllowsMotionRequests"] = _allowsMotionRequests,
                ["AllowsAllCommands"] = _allowsAllCommands,
                
                ["AllowsChangingToyState"] = _allowChangingToyState,
                ["AllowIntensityControl"] = _allowsIntensityControl,
                ["AllowsUsingPatterns"] = _allowsUsingPatterns,
                ["ActiveToystepSize"] = _activeToystepSize,

                ["AllowForcedFollow"] = _allowForcedFollow,
                ["ForcedFollow"] = _forcedFollow,
                ["AllowForcedSit"] = _allowForcedSit,
                ["ForcedSit"] = _forcedSit,
                ["AllowForcedToStay"] = _allowForcedToStay,
                ["ForcedToStay"] = _forcedToStay,
                ["AllowBlindfold"] = _allowBlindfold,
                ["Blindfolded"] = _blindfolded,
                // our list storage stuff
                ["StoredRestraintSets"] = JToken.FromObject(_storedRestraintSets),
                ["StoredAliases"] = JToken.FromObject(_storedAliases),
                ["StoredPatternNames"] = JToken.FromObject(_storedPatternNames),
            };
            // merge with the base serialization
            JObject baseSerialized = base.Serialize();
            derivedSerialized.Merge(baseSerialized);
            // return it
            return derivedSerialized;
        } catch (Exception e) {
            Console.WriteLine($"[WhitelistedCharacterInfo] Error in Serialize: {e}");
            return new JObject();
        }
    }

    public override void Deserialize(JObject jsonObject, int version) {
        base.Deserialize(jsonObject, version);
        // handle the player name and world based on the version
        if (version == 1)
        {
            var name = jsonObject["Name"]?.Value<string>() ?? "None";
            var homeworld = jsonObject["Homeworld"]?.Value<string>() ?? "None";
            _charNAW = new List<(string, string)> { (name, homeworld) };
        }
        else if (version == 2)
        {
            _charNAW = jsonObject["CharNameAndWorld"]?.ToObject<List<(string, string)>>() ?? new List<(string, string)> { ("None", "None") };
        }
        _charNAWIdxToProcess = jsonObject["CharNAWIdxToProcess"]?.Value<int>() ?? 0;
        // _name = jsonObject["Name"]?.Value<string>() ?? "None";
        // _homeworld = jsonObject["Homeworld"]?.Value<string>() ?? "None";
        _yourStatusToThem = Enum.TryParse(jsonObject["YourStatusToThem"]?.Value<string>(), out RoleLean statusToThem) ? statusToThem : RoleLean.None;
        _theirStatusToYou = Enum.TryParse(jsonObject["TheirStatusToYou"]?.Value<string>(), out RoleLean statusToYou) ? statusToYou : RoleLean.None;
        _pendingRelationRequestFromYou = Enum.TryParse(jsonObject["PendingRequestFromYou"]?.Value<string>(), out RoleLean requestFromYou) ? requestFromYou : RoleLean.None;
        _pendingRelationRequestFromPlayer = Enum.TryParse(jsonObject["PendingRequestFromPlayer"]?.Value<string>(), out RoleLean requestFromPlayer) ? requestFromPlayer : RoleLean.None;
        _timeOfCommitment = JsonConvert.DeserializeObject<DateTimeOffset>(jsonObject["TimeOfCommitment"]?.Value<string>() ?? "");
        _grantExtendedLockTimes = jsonObject["ExtendedLockTimes"]?.Value<bool>() ?? false;
        _inHardcoreMode = jsonObject["InHardcoreMode"]?.Value<bool>() ?? false;
        
        _enableRestraintSets = jsonObject["EnableRestraintSets"]?.Value<bool>() ?? false;
        _restraintSetLocking = jsonObject["RestraintSetLocking"]?.Value<bool>() ?? false;
        
        _theirTriggerPhrase = jsonObject["TriggerPhrase"]?.Value<string>() ?? "";
        _theirTriggerStartChar = jsonObject["StartCharForPuppeteerTrigger"]?.Value<string>() ?? "(";
        _theirTriggerEndChar = jsonObject["EndCharForPuppeteerTrigger"]?.Value<string>() ?? ")";
        _allowsSitRequests = jsonObject["AllowsSitRequests"]?.Value<bool>() ?? false;
        _allowsMotionRequests = jsonObject["AllowsMotionRequests"]?.Value<bool>() ?? false;
        _allowsAllCommands = jsonObject["AllowsAllCommands"]?.Value<bool>() ?? false;
        
        _allowChangingToyState = jsonObject["AllowsChangingToyState"]?.Value<bool>() ?? false;
        _allowsIntensityControl = jsonObject["AllowIntensityControl"]?.Value<bool>() ?? false;
        _allowsUsingPatterns = jsonObject["AllowsUsingPatterns"]?.Value<bool>() ?? false;
        _activeToystepSize = jsonObject["ActiveToystepSize"]?.Value<int>() ?? 0;
        
        _allowForcedFollow = jsonObject["AllowForcedFollow"]?.Value<bool>() ?? false;
        _forcedFollow = jsonObject["ForcedFollow"]?.Value<bool>() ?? false;
        _allowForcedSit = jsonObject["AllowForcedSit"]?.Value<bool>() ?? false;
        _forcedSit = jsonObject["ForcedSit"]?.Value<bool>() ?? false;
        _allowForcedToStay = jsonObject["AllowForcedToStay"]?.Value<bool>() ?? false;
        _forcedToStay = jsonObject["ForcedToStay"]?.Value<bool>() ?? false;
        _allowBlindfold = jsonObject["AllowBlindfold"]?.Value<bool>() ?? false;
        _blindfolded = jsonObject["Blindfolded"]?.Value<bool>() ?? false;
        
        // our list storage stuff
        _storedRestraintSets = jsonObject["StoredRestraintSets"]?.ToObject<List<string>>() ?? new List<string>();
        _storedAliases = jsonObject["StoredAliases"]?.ToObject<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        _storedPatternNames = jsonObject["StoredPatternNames"]?.ToObject<List<string>>() ?? new List<string>();
    }

#endregion Serialization and Deserialization
}

