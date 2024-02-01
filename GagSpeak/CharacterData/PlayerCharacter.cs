using System.Collections.Generic;

namespace GagSpeak.CharacterData;

// Settings set by the player & not visable to whitelisted players (REQUESTING INFO SENDS THEM INFO FROM WHITELISTCHARDATA & INFO BASE)
public class PlayerCharacterInfo : CharacterInfoBase
{
    //////////////////////////////////////// PREFERENCES FOR NON-WHITELISTED PLAYERS  ///////////////////////////////////////
    public  string      _safeword { get; set; } = "safeword";               // What is the safeword?
    public  bool        _doCmdsFromFriends { get; set; } = false;           // gives anyone on your friendlist access to use GagSpeak commands on you
    public  bool        _doCmdsFromParty { get; set; } = false;             // gives anyone in your party access to use GagSpeak commands on you
    public  bool        _liveGarblerWarnOnZoneChange { get; set; } = false; // enables or disables the live garbler warning on zone change
    ///////////////////////////////////////////// WARDROBE COMPONENT SETTINGS  /////////////////////////////////////////////
    public  bool        _allowItemAutoEquip { get; set; } = false;          // lets player set if anything in the GagStorage compartment will function
    public  bool        _allowRestraintSetAutoEquip { get; set; } = false;  // lets player set if anything in the Restraintset compartment will function
    ///////////////////////////////////////////// GAGSPEAK PUPPETEER SETTINGS  /////////////////////////////////////////////
    public  bool        _allowPuppeteer { get; set; } = false;              // lets the player set if puppeteer will function
    ////////////////////////////////////////////////// PROTECTED FIELDS ////////////////////////////////////////////////////
    public  List<bool>  _grantExtendedLockTimes { get; set; } = [];         // [TIER 2] Each idx reflect the idx of player in whitelist. Should be updated with whitelist
    public  List<string>_triggerPhraseForPuppeteer { get; set; } = [];      // Each idx reflect the idx of player in whitelist. Should be updated with whitelist
    ///////////////////////////////////////////// FUTURE MODULES CAN GO HERE /////////////////////////////////////////////
}