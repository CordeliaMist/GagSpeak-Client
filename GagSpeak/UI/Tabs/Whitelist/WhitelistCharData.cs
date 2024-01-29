using System.Collections.Generic; // for lists
using System.Linq;                // for lists
using System;                     // for basic C# types
using GagSpeak.Events;           // for the observable list

namespace GagSpeak.Data;

/// <summary> A class to hold the data for the whitelist character </summary>
public class WhitelistCharData {
    
    public string                       name;                                     // get the character name
    public string                       homeworld;                                // get the characters world (dont know how to get this for now)
    public string                       relationshipStatus;                       // who you are to them
    public string                       relationshipStatusToYou;                  // who they are to you
    public bool                         isDomMode;                                // is the character in dom mode?
    public int                          garbleLevel;                              // get the garble level of the character
    public string                       PendingRelationRequestFromYou;            // Once approved by the other player, the relationshipStatus variable is updated
    public string                       PendingRelationRequestFromPlayer;         // Stores a request from another player. Stored in relationshipStatusToYou if approved
    public DateTimeOffset               timeOfCommitment;                         // how long has your commitment lasted?
    public bool                         lockedLiveChatGarbler { get; set; }       // is the live chat garbler locked?
    public ObservableList<string>       selectedGagTypes { get; set; }            // What gag types are selected?
    public ObservableList<LockableType>  selectedGagPadlocks { get; set; }         // which padlocks are equipped currently?
    public List<string>                 selectedGagPadlocksPassword { get; set; } // password lock on padlocks, if any
    public List<DateTimeOffset>         selectedGagPadlocksTimer { get; set; }    // stores the timespan left until unlock of the player.
    public List<string>                 selectedGagPadlocksAssigner { get; set; } // who assigned the padlocks, if any
    
    /// <summary> Initializes a new instance of the <see cref="WhitelistCharData"/> class. </summary>
    public WhitelistCharData(string _name, string _homeworld, string _relationshipStatus)
    {
        this.name = _name;
        this.homeworld = _homeworld;
        this.relationshipStatus = _relationshipStatus;
        this.relationshipStatusToYou = "None";
        this.lockedLiveChatGarbler = false;
        this.PendingRelationRequestFromPlayer = ""; // remember, this keeps track of people wanting to declare relations with you
        this.PendingRelationRequestFromYou = ""; // and this keeps track of the relation requests you issue out to others for message feedback
        // Make sure we aren't getting any duplicates
        if (this.selectedGagTypes == null || !this.selectedGagTypes.Any() || this.selectedGagTypes.Count > 3) {
            this.selectedGagTypes = new ObservableList<string> { "None", "None", "None" };}
        // Set default values for selectedGagPadlocks
        if (this.selectedGagPadlocks == null || !this.selectedGagPadlocks.Any() || this.selectedGagPadlocks.Count > 3) {
            this.selectedGagPadlocks = new ObservableList<LockableType> { LockableType.None, LockableType.None, LockableType.None };}
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

    /// <summary> Sets the time of commitment </summary>
    public void SetTimeOfCommitment() {
        this.timeOfCommitment = DateTimeOffset.Now;
    }

    /// <summary> 
    /// gets the time of commitment 
    /// </summary>
    /// <returns>The time of commitment.</returns>
    public string GetCommitmentDuration() {
        if (this.timeOfCommitment == default(DateTimeOffset))
            return ""; // Display nothing if commitment time is not set
        TimeSpan duration = DateTimeOffset.Now - this.timeOfCommitment; // Get the duration
        int days = duration.Days % 30;
        // Display the duration in the desired format
        return $"{days}d, {duration.Hours}h, {duration.Minutes}m, {duration.Seconds}s";
    }

    /// <summary>
    /// gets the duration left on a timed padlock type
    /// <list type="bullet">
    /// <item><c>index</c><param name="index"> - The index of the padlock.</param></item>
    /// </list> </summary>
    /// <returns>The duration left on the padlock.</returns>
    public string GetPadlockTimerDurationLeft(int index) {
        TimeSpan duration = this.selectedGagPadlocksTimer[index] - DateTimeOffset.Now; // Get the duration
        if (duration < TimeSpan.Zero) {
            // check if the padlock type was a type with a timer, and if so, set the other stuff to none
            if (this.selectedGagPadlocks[index] == LockableType.FiveMinutesPadlock || this.selectedGagPadlocks[index] == LockableType.MistressTimerPadlock
            || this.selectedGagPadlocks[index] == LockableType.TimerPasswordPadlock) {
                this.selectedGagPadlocks[index] = LockableType.None;
                this.selectedGagPadlocksPassword[index] = "";
                this.selectedGagPadlocksAssigner[index] = "";
            }
            return "";
            
        }
        // Display the duration in the desired format
        return $"{duration.Hours}h, {duration.Minutes}m, {duration.Seconds}s";
    }
}
