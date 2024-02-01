using System;
using GagSpeak.Data;

namespace GagSpeak.CharacterData;


/// <summary> A class to hold the data for the whitelist character </summary>
public class WhitelistedCharacterInfo : CharacterInfoBase
{
    public string           _name { get; set; }                             // get the character name
    public string           _homeworld { get; set; }                        // get the characters world (dont know how to get this for now)
    public RoleLean         _yourStatusToThem { get; set; }                 // who you are to them
    public RoleLean         _theirStatusToYou { get; set; }                 // who they are to you
    public RoleLean         _pendingRelationRequestFromYou { get; set; }    // displays the current dyanmic request sent by you to this player
    public RoleLean         _pendingRelationRequestFromPlayer { get; set; } // displays the current dynamic request from this player to you
    public DateTimeOffset   _timeOfCommitment { get; set; }                 // how long has your commitment lasted?
    ////////////////////////////////////////////////// PROTECTED FIELDS ////////////////////////////////////////////////////
    public  bool            _grantExtendedLockTimes { get; set; } = false;  // [TIER 2] without this enabled, no locked times can exceed 12 hours
    public  string          _triggerPhraseForPuppeteer { get; set; } = "";  // [TIER 0] what is this persons trigger phrase for you?
    
    /// <summary> Initializes a new instance of the <see cref="WhitelistCharData"/> class. </summary>
    public WhitelistedCharacterInfo(string name, string homeworld, string relationshipStatus) {
        _name = name;
        _homeworld = homeworld;
        _yourStatusToThem = (RoleLean) Enum.Parse(typeof(RoleLean), relationshipStatus, true);
        _theirStatusToYou = RoleLean.None;
        _pendingRelationRequestFromPlayer = RoleLean.None;
        _pendingRelationRequestFromYou = RoleLean.None;
    }
#region General Interactions
    /// <summary> get the string format of the roleleanEnum </summary>
    public string GetRoleLeanString(RoleLean role) {
        return role.ToString();
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
        // If a two way dyanamic is note yet established, then our tier is 0.
        if (_yourStatusToThem == RoleLean.None || _theirStatusToYou == RoleLean.None) {
            return DynamicTier.Tier0;
        }
        // TIER 1 == dynamic of PET/SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        if (_yourStatusToThem == RoleLean.Mistress || _yourStatusToThem == RoleLean.Master || _yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Pet || _theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier1;
        }}
        // TIER 2 == dynamic of SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        else if (_yourStatusToThem == RoleLean.Mistress || _yourStatusToThem == RoleLean.Master || _yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier2;
        }}
        // TIER 3 == dynamic of SLAVE/ABSOLUTE-SLAVE with OWNER.
        else if (_yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier3;
        }}
        // TIER 4 == dynamic of ABSOLUTE-SLAVE with OWNER.
        else if (_yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier4;
        }}
        // we should never make it here, but if we do, set the dynamic to 0 anyways
        return DynamicTier.Tier0;
    }
#endregion State Fetching / Setting
}

