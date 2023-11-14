using System.Collections.Generic;
using System.Linq;
using System;

namespace GagSpeak.Data;
// a struct to hold information on whitelisted players.
public class WhitelistCharData {
    public string name; // get the character name
    public string homeworld; // get the characters world (dont know how to get this for now)
    public string relationshipStatus; // get the relationship status of the character to you
    public bool isDomMode; // is the character in dom mode?
    public int garbleLevel; // get the garble level of the character
    public string PendingRelationshipRequest; // get the pending relationship request, if any
    public DateTimeOffset timeOfCommitment; // how long has your commitment with this player lasted? (data stores time of commitment start)
    public bool lockedLiveChatGarbler { get; set; } // is the live chat garbler locked?
    public List<string> selectedGagTypes { get; set; } // What gag types are selected?
    public List<GagPadlocks> selectedGagPadlocks { get; set; } // which padlocks are equipped currently?
    public List<string> selectedGagPadlocksPassword { get; set; } // password lock on padlocks, if any
    public List<DateTimeOffset> selectedGagPadlocksTimer { get; set; } // stores time each padlock was assigned.
    public List<string> selectedGagPadlocksAssigner { get; set; } // who assigned the padlocks, if any
    // Constructor for the struct
    public WhitelistCharData(string _name, string _homeworld, string _relationshipStatus)
    {
        this.name = _name;
        this.homeworld = _homeworld;
        this.relationshipStatus = _relationshipStatus;
        this.lockedLiveChatGarbler = false;
        this.PendingRelationshipRequest = "None";
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
        // set default values for selectedGagPadLockTimer
        if (this.selectedGagPadlocksTimer == null || !this.selectedGagPadlocksTimer.Any() || this.selectedGagPadlocksTimer.Count > 3) {
            this.selectedGagPadlocksTimer = new List<DateTimeOffset> { DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now };}
    }

    public void SetTimeOfCommitment() {
        this.timeOfCommitment = DateTimeOffset.Now;
    }

    // function to get the commitment duration
    public string GetCommitmentDuration() {
        if (this.timeOfCommitment == default(DateTimeOffset))
            return ""; // Display nothing if commitment time is not set
            
        TimeSpan duration = DateTimeOffset.Now - this.timeOfCommitment; // get the duration
        // Display the duration in days, hours, minutes
        return $"{duration.Days} days, {duration.Hours} hours, {duration.Minutes} minutes";
    }
}

