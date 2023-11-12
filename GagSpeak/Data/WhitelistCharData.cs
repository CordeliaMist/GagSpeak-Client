using System.Collections.Generic;
using System.Linq;

namespace GagSpeak.Data;
// a struct to hold information on whitelisted players.
public struct WhitelistCharData {
    public string name; // get the character name
    public string homeworld; // get the characters world (dont know how to get this for now)
    public string relationshipStatus; // get the relationship status of the character to you
    // relation status can be "Mistress", "Pet", "Slave", or "None"
    public int commitmentDuration; // how long has your commitment with this player lasted?
    public bool lockedLiveChatGarbler { get; set; } // is the live chat garbler locked?
    public List<string> selectedGagTypes { get; set; } // What gag types are selected?
    public List<GagPadlocks> selectedGagPadlocks { get; set; } // which padlocks are equipped currently?
    public List<string> selectedGagPadlocksPassword { get; set; } // password lock on padlocks, if any
    public List<string> selectedGagPadlocksAssigner { get; set; } // who assigned the padlocks, if any
    // Constructor for the struct
    public WhitelistCharData(string _name, string _homeworld, string _relationshipStatus)
    {
        this.name = _name;
        this.homeworld = _homeworld;
        this.relationshipStatus = _relationshipStatus;
        this.commitmentDuration = 0;
        this.lockedLiveChatGarbler = false;
        // Make sure we aren't getting any duplicates
        if (this.selectedGagTypes == null || !this.selectedGagTypes.Any() || this.selectedGagTypes.Count > 3) {
            this.selectedGagTypes = new List<string> { "None", "None", "None" };}
        // Set default values for selectedGagPadlocks
        if (this.selectedGagPadlocks == null || !this.selectedGagPadlocks.Any() || this.selectedGagPadlocks.Count > 3) {
            this.selectedGagPadlocks = new List<GagPadlocks> { GagPadlocks.None, GagPadlocks.None, GagPadlocks.None };}
        // set default values for selectedGagPadlocksPassword
        if (this.selectedGagPadlocksPassword == null || !this.selectedGagPadlocksPassword.Any() || this.selectedGagPadlocksPassword.Count > 3) {
            this.selectedGagPadlocksPassword = new List<string> { "", "", "" };}
        // set default values for selectedGagPadlocksAssigner
        if (this.selectedGagPadlocksAssigner == null || !this.selectedGagPadlocksAssigner.Any() || this.selectedGagPadlocksAssigner.Count > 3) {
            this.selectedGagPadlocksAssigner = new List<string> { "", "", "" };}
    }
}

