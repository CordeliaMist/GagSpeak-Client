using System.Collections.Generic;
using System.Linq;
using System;
using GagSpeak.Events;
using GagSpeak.Data;

namespace GagSpeak.CharacterData;

/// <summary>
/// General settings that are both visable to client and whitelisted players, and modifyable, given correct access.
/// <para> Access is granted by the following (Note for all these conditions, the one with edit permissions is the dominant): </para>
/// <list type="bullet">
///     <item>[ TIER 0 ] A readonly field, for displaying information </item>
///     <item>[ TIER 1 ] Modifiable by a dynamic of PET/SLAVE/ABSOLUTE-SLAVE -- MISTRESS|MASTER/OWNER. </item>
///     <item>[ TIER 2 ] Modifiable by a dynamic of SLAVE/ABSOLUTE-SLAVE -- MISTRESS|MASTER/OWNER. </item>
///     <item>[ TIER 3 ] Modifiable by a dynamic of SLAVE/ABSOLUTE-SLAVE -- OWNER. </item>
///     <item>[ TIER 4 ] Modifiable by a dynamic of ABSOLUTE-SLAVE -- OWNER. </item>
/// </list>
/// </summary>
public class CharacterInfoBase
{
    //////////////////////////////////////// GENERAL STATUS VISIBILITY  ///////////////////////////////////////
    public  bool                _safewordUsed { get; set; } = false;                // [TIER 0] Has the safeword been used?
    public  bool                _grantExtendedLockTimes { get; set; } = false;      // [TIER 2] without this enabled, no locked times can exceed 12 hours
    /////////////////////////////////////////// CHAT GARBLER STATES  //////////////////////////////////////////
    public  bool                _directChatGarblerActive { get; set; } = false;     // [TIER 4] Is direct chat garbler enabled?
    public  bool                _directChatGarblerLocked { get; set; } = false;     // [TIER 3] Is live chat garbler enabled?
    ////////////////////////////////////////// GAG AND LOCK VARIABLES //////////////////////////////////////////
    public  WatchList<string>   _selectedGagTypes { get; set; }                     // [TIER 0] What gag types are selected?
    public  WatchList<Padlocks> _selectedGagPadlocks { get; set; }                  // [TIER 0] which padlocks are equipped currently?
    public  List<string>        _selectedGagPadlockPassword { get; set; }           // [TIER 0] password lock on padlocks, if any
    public  List<DateTimeOffset>_selectedGagPadlockTimer { get; set; }              // [TIER 0] stores time when the padlock will be unlocked.
    public  List<string>        _selectedGagPadlockAssigner { get; set; }           // [TIER 0] name of who assigned the padlocks
    ///////////////////////////////////////// WARDROBE COMPONENT SETTINGS  //////////////////////////////////////
    public  bool                _enableWardrobe { get; set; } = false;              // [TIER 0] enables / disables all wardrobe functionality
    public  bool                _lockGagStorageUiOnGagLock { get; set; } = false;   // [TIER 1] locks storage ui when gag is locked (player cant disable auto-eqiup from gag storage)
    public  bool                _enableRestraintSets { get; set; }= false;          // [TIER 2] allows the dom to enable spesific restraint sets by name
    public  bool                _restraintSetLocking { get; set; } = false;         // [TIER 1] enables / disables all restraint set locking
    //////////////////////////////////////// GAGSPEAK PUPPETEER SETTINGS  //////////////////////////////////////
    public  string              _triggerPhraseForPuppeteer { get; set; } = "";      // [TIER 0] what is the trigger phrase for puppeteer to use?
    public  bool                _allowSitRequests { get; set; } = false;            // [TIER 1] if granting them access to sit commands
    public  bool                _allowMotionRequests { get; set; } = false;         // [TIER 2] if granting them motion based access
    public  bool                _allowAllCommands { get; set; } = false;            // [TIER 4] if granting them access to all commands (harmful & gagspeak ones)
    /////////////////////////////////////////// TOYBOX MODULE SETTINGS ////////////////////////////////////////
    public  bool                _enableToybox { get; set; } = false;                // [TIER 4] if granting them access to toybox
    public  bool                _allowToyboxToggle { get; set; } = false;           // [TIER 1] if they can start and stop your toy from vibrating
    public  bool                _allowIntensityControl { get; set; } = false;       // [TIER 0] if they can control the vibes intensity and speed
    public  int                 _intensityLevel { get; set; } = 0;                  // [TIER 2] the current intensity level of the toy [1-10]
    public  bool                _canUseStoredPatterns { get; set; } = false;        // [TIER 2] Access to execute stored patterns for your vibe.
    public  bool                _allowToyboxLocking { get; set; } = false;          // [TIER 4] if granting them access to lock toybox
    ///////////////////////////////////////// FUTURE MODULES CAN GO HERE ////////////////////////////////////////


    public CharacterInfoBase() {
        // create new variables for the lists
        if (_selectedGagTypes == null || !_selectedGagTypes.Any() || _selectedGagTypes.Count > 3) {
            _selectedGagTypes = new WatchList<string> { "None", "None", "None" };}
        // Set default values for selectedGagPadlocks
        if (_selectedGagPadlocks == null || !_selectedGagPadlocks.Any() || _selectedGagPadlocks.Count > 3) {
            _selectedGagPadlocks = new WatchList<Padlocks> { Padlocks.None, Padlocks.None, Padlocks.None };}
        // set default values for selectedGagPadlocksPassword
        if (_selectedGagPadlockPassword == null || !_selectedGagPadlockPassword.Any() || _selectedGagPadlockPassword.Count > 3) {
            _selectedGagPadlockPassword = new List<string> { "", "", "" };}
        // set default values for selectedGagPadLockTimer
        if (_selectedGagPadlockTimer == null || !_selectedGagPadlockTimer.Any() || _selectedGagPadlockTimer.Count > 3) {
            _selectedGagPadlockTimer = new List<DateTimeOffset> { DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now };}
        // set default values for selectedGagPadlocksAssigner
        if (_selectedGagPadlockAssigner == null || !_selectedGagPadlockAssigner.Any() || _selectedGagPadlockAssigner.Count > 3) {
            _selectedGagPadlockAssigner = new List<string> { "", "", "" };}
    }
}