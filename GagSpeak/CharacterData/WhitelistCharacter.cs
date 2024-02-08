using System;
using GagSpeak.Gagsandlocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

/// <summary> A class to hold the data for the whitelist character </summary>
public class WhitelistedCharacterInfo : CharacterInfoBase
{
    public string           _name { get; set; }                              // get the character name
    public string           _homeworld { get; set; }                         // get the characters world (dont know how to get this for now)
    public RoleLean         _yourStatusToThem { get; set; }                  // who you are to them
    public RoleLean         _theirStatusToYou { get; set; }                  // who they are to you
    public RoleLean         _pendingRelationRequestFromYou { get; set; }     // displays the current dyanmic request sent by you to this player
    public RoleLean         _pendingRelationRequestFromPlayer { get; set; }  // displays the current dynamic request from this player to you
    public DateTimeOffset   _timeOfCommitment { get; set; }                  // how long has your commitment lasted?
    public bool             _grantExtendedLockTimes { get; set; } = false;   // [TIER 2] if whitelisted user allows you to use extended lock times
    public bool             _enableRestraintSets { get; set; } = false;      // [TIER 2] allows dom to enable spesific restraint sets
    public bool             _restraintSetLocking { get; set; } = false;      // [TIER 1] enables / disables all restraint set locking 
    public string           _theirTriggerPhrase { get; set; } = "";          // [TIER 0] this whitelisted user's trigger phrase
    public string           _theirTriggerStartChar { get; set; } = "(";      // [TIER 0] what to have instead of () surrounding full commands
    public string           _theirTriggerEndChar { get; set; } = ")";        // [TIER 0] what to have instead of () surrounding full commands
    public bool             _allowsSitRequests { get; set; } = false;        // [TIER 1] if they allow you to use sit requests
    public bool             _allowsMotionRequests { get; set; } = false;     // [TIER 2] if they allow you to use motion requests
    public bool             _allowsAllCommands { get; set; } = false;        // [TIER 4] If they allow you to use all commands on them
    public bool             _allowsChangingToyState { get; set; } = false;   // [TIER 1] Basically, "They can turn on my vibe, at my selected slider position"
    public bool             _allowsIntensityControl { get; set; } = false;   // [TIER 3] Basically says "This person can adjust the intensity slider"
    public bool             _allowsUsingPatterns { get; set; } = false;      // [TIER 0] Do they allow you to execute stored patterns
    public  int             _activeToystepSize { get; set; } = 0;            // [TIER 0] the step count of the vibe
    public  double          _activeToystepInterval { get; set; } = 0;        // [TIER 0] step size interval of the vibe

    ////////////////////////////////////////////////// PROTECTED FIELDS ////////////////////////////////////////////////////
    
    public WhitelistedCharacterInfo() : this("None", "None") { }
    public WhitelistedCharacterInfo(string name, string homeworld) {
        _name = name;
        _homeworld = homeworld;
        _yourStatusToThem = RoleLean.None;
        _theirStatusToYou = RoleLean.None;
        _pendingRelationRequestFromPlayer = RoleLean.None;
        _pendingRelationRequestFromYou = RoleLean.None;
        _timeOfCommitment = DateTimeOffset.Now;
    }
#region General Interactions
    public bool IsRoleLeanDominant(RoleLean roleLean) {
        if(roleLean == RoleLean.Mistress || roleLean == RoleLean.Master || roleLean == RoleLean.Owner) {
            return true;
        }
        return false;
    }

    public bool IsRoleLeanSubmissive(RoleLean roleLean) {
        if(roleLean == RoleLean.Pet || roleLean == RoleLean.Slave || roleLean == RoleLean.AbsoluteSlave) {
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
        int days = duration.Days % 30;
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
        // Display the duration in the desired format
        return $"{duration.Hours}h, {duration.Minutes}m, {duration.Seconds}s";
    }
#endregion General Interactions
#region State Fetching / Setting
    /// <summary> get the spesified tier of a current dynamic </summary>
    public DynamicTier GetDynamicTier() {
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
            if(_theirStatusToYou == RoleLean.Submissive || _theirStatusToYou == RoleLean.Pet
            || _theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier1;
        }}
        // If a two way dyanamic is note yet established, then our tier is 0.
        if (_yourStatusToThem == RoleLean.None || _theirStatusToYou == RoleLean.None) {
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
                ["Name"] = _name,
                ["Homeworld"] = _homeworld,
                ["YourStatusToThem"] = _yourStatusToThem.ToString(),
                ["TheirStatusToYou"] = _theirStatusToYou.ToString(),
                ["PendingRequestFromYou"] = _pendingRelationRequestFromYou.ToString(),
                ["PendingRequestFromPlayer"] = _pendingRelationRequestFromPlayer.ToString(),
                ["TimeOfCommitment"] = JsonConvert.SerializeObject(_timeOfCommitment),
                ["ExtendedLockTimes"] = _grantExtendedLockTimes,
                ["EnableRestraintSets"] = _enableRestraintSets,
                ["RestraintSetLocking"] = _restraintSetLocking,
                ["TriggerPhrase"] = _theirTriggerPhrase,
                ["StartCharForPuppeteerTrigger"] = _theirTriggerStartChar,
                ["EndCharForPuppeteerTrigger"] = _theirTriggerEndChar,
                ["AllowsSitRequests"] = _allowsSitRequests,
                ["AllowsMotionRequests"] = _allowsMotionRequests,
                ["AllowsAllCommands"] = _allowsAllCommands,
                ["AllowsChangingToyState"] = _allowsChangingToyState,
                ["AllowIntensityControl"] = _allowsIntensityControl,
                ["AllowsUsingPatterns"] = _allowsUsingPatterns,
                ["ActiveToystepSize"] = _activeToystepSize,
                ["ActiveToystepInterval"] = _activeToystepInterval
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

    public override void Deserialize(JObject jsonObject) {
        base.Deserialize(jsonObject);
        _name = jsonObject["Name"]?.Value<string>() ?? "None";
        _homeworld = jsonObject["Homeworld"]?.Value<string>() ?? "None";
        _yourStatusToThem = Enum.TryParse(jsonObject["YourStatusToThem"]?.Value<string>(), out RoleLean statusToThem) ? statusToThem : RoleLean.None;
        _theirStatusToYou = Enum.TryParse(jsonObject["TheirStatusToYou"]?.Value<string>(), out RoleLean statusToYou) ? statusToYou : RoleLean.None;
        _pendingRelationRequestFromYou = Enum.TryParse(jsonObject["PendingRequestFromYou"]?.Value<string>(), out RoleLean requestFromYou) ? requestFromYou : RoleLean.None;
        _pendingRelationRequestFromPlayer = Enum.TryParse(jsonObject["PendingRequestFromPlayer"]?.Value<string>(), out RoleLean requestFromPlayer) ? requestFromPlayer : RoleLean.None;
        _timeOfCommitment = JsonConvert.DeserializeObject<DateTimeOffset>(jsonObject["TimeOfCommitment"]?.Value<string>() ?? "");
        _grantExtendedLockTimes = jsonObject["ExtendedLockTimes"]?.Value<bool>() ?? false;
        _theirTriggerPhrase = jsonObject["TheirTriggerPhrase"]?.Value<string>() ?? "";
        _theirTriggerStartChar = jsonObject["StartCharForPuppeteerTrigger"]?.Value<string>() ?? "(";
        _theirTriggerEndChar = jsonObject["EndCharForPuppeteerTrigger"]?.Value<string>() ?? ")";
        _allowsSitRequests = jsonObject["AllowsSitRequests"]?.Value<bool>() ?? false;
        _allowsMotionRequests = jsonObject["AllowsMotionRequests"]?.Value<bool>() ?? false;
        _allowsAllCommands = jsonObject["AllowsAllCommands"]?.Value<bool>() ?? false;
        _allowsChangingToyState = jsonObject["AllowsChangingToyState"]?.Value<bool>() ?? false;
        _allowsIntensityControl = jsonObject["AllowIntensityControl"]?.Value<bool>() ?? false;
        _allowsUsingPatterns = jsonObject["AllowsUsingPatterns"]?.Value<bool>() ?? false;
        _activeToystepSize = jsonObject["ActiveToystepSize"]?.Value<int>() ?? 0;
        _activeToystepInterval = jsonObject["ActiveToystepInterval"]?.Value<double>() ?? 0;
    }

#endregion Serialization and Deserialization
}

