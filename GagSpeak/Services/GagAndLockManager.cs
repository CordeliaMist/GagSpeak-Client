using System;                                       // Provides fundamental classes for base data types
using System.Collections.Generic;                   // Provides classes for defining generic collections
using GagSpeak.Services;                            // Contains service classes used in the GagSpeak application
using GagSpeak.Events;                              // Contains event classes used in the GagSpeak application
using GagSpeak.Data;                                // Contains data classes used in the GagSpeak application
using GagSpeak.UI.Helpers;                          // Contains chat classes used in the GagSpeak application
using Dalamud.Plugin.Services;                      // Contains service classes provided by the Dalamud plugin framework
using Dalamud.Game.Text.SeStringHandling.Payloads;  // Contains classes for handling special encoded (SeString) payloads in the Dalamud game
using Dalamud.Game.Text.SeStringHandling;           // Contains classes for handling special encoded (SeString) strings in the Dalamud game
using OtterGui.Classes;
using System.Linq;
using GagSpeak.Wardrobe;                             // Contains classes for managing the OtterGui framework

namespace GagSpeak;

/// <summary>
/// This class is used to handle command based lock interactions and UI based lock interactions.
/// </summary>
public class GagAndLockManager : IDisposable
{
    private readonly GagSpeakConfig         _config;                // for config options
    private readonly GagStorageManager      _gagStorageManager;     // for gag storage
    private readonly RestraintSetManager    _restraintSetManager;   // for restraint set management
    private readonly IChatGui               _clientChat;            // for chat messages
    private readonly IClientState           _clientState;           // for player payload
    private readonly TimerService           _timerService;          // for timers
    private readonly SafewordUsedEvent      _safewordUsedEvent;     // for safeword event
    private readonly ItemAutoEquipEvent     _gagItemEquippedEvent;  // for gag item equipped event

    /// <summary> Initializes a new instance of the <see cref="GagAndLockManager"/> class. </summary>
    public GagAndLockManager(GagSpeakConfig config, IChatGui clientChat, TimerService timerService,
    GagStorageManager gagStorageManager, IClientState clientState, SafewordUsedEvent safewordUsedEvent,
    ItemAutoEquipEvent _itemAutoEquipEvent, RestraintSetManager restraintSetManager) {
        _config = config;
        _clientChat = clientChat;
        _clientState = clientState;
        _timerService = timerService;
        _safewordUsedEvent = safewordUsedEvent;
        _gagItemEquippedEvent = _itemAutoEquipEvent;
        _gagStorageManager = gagStorageManager;
        _restraintSetManager = restraintSetManager;
        // subscribe to the safeword event
        _safewordUsedEvent.SafewordCommand += CleanupVariables;
    }

    /// <summary> Unsubscribes from our subscribed event upon disposal </summary>
    public void Dispose() {
        _safewordUsedEvent.SafewordCommand -= CleanupVariables;
    }

#region GeneralGagAndLockFunctions
    /// <summary> This method is used to handle gag applying command and UI presses
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>gagType</c><param name="gagType"> - The gag type.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The assigner name, if any.</param></item>
    /// </list> </summary>
    public void ApplyGag(int layerIndex, string gagType, string assignerName = "self") {
        // apply the gag information to anywhere where it should be applied to within our code
        _config.selectedGagTypes[layerIndex] = gagType;
        
        // Trigger the event letting our wardrobe manager know a gag is equipped
        _gagItemEquippedEvent.Invoke(gagType, assignerName);
    }

    /// <summary> This method is used to handle individual gag removing command and UI presses
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void RemoveGag(int layerIndex) {
        // get gagtype before clear
        var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().FirstOrDefault(gt => gt.GetGagAlias() == _config.selectedGagTypes[layerIndex]);
        // remove the gag information from anywhere where it should be removed from within our code
        _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, "");
        _config.selectedGagTypes[layerIndex] = "None";
        _config.selectedGagPadlocks[layerIndex] = LockableType.None;
        _config.selectedGagPadlocksPassword[layerIndex] = string.Empty;
        _config.selectedGagPadlocksAssigner[layerIndex] = "";
        _config.padlockIdentifier[layerIndex].ClearPasswords();
        _config.padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, true, _config);
        // we dont worry about removing timers because if no lock, no timer.
    }

    /// <summary> This method is used to handle removing all of the gags through the command and UI interaction button
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void RemoveAllGags() {
        // remove all the gags from anywhere where it should be removed from within our code
        for(int i = 0; i < _config.selectedGagTypes.Count; i++) {
            RemoveGag(i);
        }
    }

    /// <summary> This method is used to handle the /gag lock command and UI interaction button. Acting as a toggle, this will call lock if unlocked, and unlock if locked.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void ToggleLock(int layerIndex) { // for the buttons
        if(_config.isLocked[layerIndex]) {
            Unlock(layerIndex); // call the base unlock function
        } else {
            Lock(layerIndex); // call the base lock function
        }
    }

    /// <summary> The base unlock function, used for detecting if a lock is capable of being unlocked or not based on passed in parameters.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void Unlock(int layerIndex) {
        // see if we can get the player payload
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // if we can, use it, otherwise, use the default name
        if(playerPayload != null) {
            Unlock(layerIndex, playerPayload.PlayerName, "", playerPayload.PlayerName);
        } else {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Player payload is null, so we are using the default name.");
            Unlock(layerIndex, "");
        }
    }

    /// <summary> The base unlock function, used for detecting if a lock is capable of being unlocked or not based on passed in parameters.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The assigner name.</param></item>
    /// <item><c>password</c><param name="password"> - The password.</param></item>
    /// <item><c>targetName</c><param name="targetName"> - The target name.</param></item>
    /// </list> </summary>
    public void Unlock(int layerIndex, string assignerName, string password = "", string targetName = "", string YourPlayerName = "") { // for the buttons
        GagSpeak.Log.Debug($"[Padlock Manager Service]: We are unlocking our padlock.");
        // if what we use to try and unlock the padlock is valid, we can unlock it
        if(_config.padlockIdentifier[layerIndex].CheckPassword(_config, assignerName, targetName, password, YourPlayerName))
        {
            // unlock the padlock in all locations where we should be updating it as unlocked, clearing any stored lock information
            _config.isLocked[layerIndex] = false;
            _config.padlockIdentifier[layerIndex].ClearPasswords();
            _config.padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, true, _config);
            _timerService.ClearIdentifierTimer(layerIndex);
            _config.Save();
        } else {
            // otherwise, we cannot unlock it
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Unlock was unsucessful.");
        }
    }

    /// <summary> The base lock function, used for detecting if a lock is capable of being locked or not based on passed in parameters.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void Lock(int layerIndex) {
        // see if we can get the player payload
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // if the payload returned not null, we can use it
        if(playerPayload != null) {
            Lock(layerIndex, playerPayload.PlayerName, "", "", playerPayload.PlayerName);
        } else {
            // otherwise, we use the default name
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Player payload is null, so we are using the default name.");
            Lock(layerIndex, "");
        }
    }

    /// <summary> The augmented lock function, used for detecting if a lock is capable of being locked or not based on passed in parameters.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The assigner name.</param></item>
    /// <item><c>password1</c><param name="password1"> - The password1.</param></item>
    /// <item><c>password2</c><param name="password2"> - The password2.</param></item>
    /// <item><c>targetName</c><param name="targetName"> - The target name.</param></item>
    /// </list> </summary>
    public void Lock(int layerIndex, string assignerName, string password1 = "", string password2 = "", string targetName = "") {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        GagSpeak.Log.Debug($"[Padlock Manager Service]: targetName: {targetName}");
        // firstly, see if both our passwords are null, if it is true, it means this came from a button
        if(password1 == "" && password2 == "") {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: This Lock Request came from a button!");
            // if the padlock is valid, and has a valid password if it needs one, then we can lock
            if(_config.padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config.isLocked[layerIndex], _config,  assignerName, targetName, playerPayload.PlayerName)) {
                // if we reached this point it means our password was valid, so we can lock
                _config.isLocked[layerIndex] = true;
                _config.padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, false, _config);
                StartTimerIfNecessary(layerIndex, _config, _timerService);
                _config.Save();
            } else {
                // otherwise, we cannot lock
                GagSpeak.Log.Debug($"[Padlock Manager Service]: LOCK -> Lock was unsucessful.");
            }
        }
        // otherwise, it means we came from a command
        else {
            // we will need to setandvalidate, over just validate
            GagSpeak.Log.Debug($"[Padlock Manager Service]: This Lock Request came from a command!");
            if(_config.padlockIdentifier[layerIndex].SetAndValidate(_config, _config.padlockIdentifier[layerIndex]._padlockType.ToString(),
            password1, password2, assignerName, targetName)) {
                // if we reached this point it means our password was valid, so we can lock
                _config.isLocked[layerIndex] = true;
                _config.padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, false, _config);
                StartTimerIfNecessary(layerIndex, _config, _timerService);
                _config.Save();
            } else {
                // otherwise, we cannot lock
                GagSpeak.Log.Debug($"[Padlock Manager Service]: LOCK -> Lock was unsucessful.");
                _config.padlockIdentifier[layerIndex].ClearPasswords();
            }
        }
    }

    /// <summary> This method is used start a timer if the padlock being locked on contains one, otherwise it shouldnt be called.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>padlockType</c><param name="padlockType"> - The padlock type.</param></item>
    /// </list> </summary>
    private void StartTimerIfNecessary(int layerIndex, GagSpeakConfig _config, TimerService _timerService) {
        GagSpeak.Log.Debug($"[Padlock Manager Service]: Checking if a starttimer is nessisary.");
        // just to double check this is actually a padlock with a timer
        if(_config.padlockIdentifier[layerIndex]._padlockType == LockableType.FiveMinutesPadlock ||
        _config.padlockIdentifier[layerIndex]._padlockType == LockableType.TimerPasswordPadlock ||
        _config.padlockIdentifier[layerIndex]._padlockType == LockableType.MistressTimerPadlock)
        {   
            // assuming it is, start the timer
            GagSpeak.Log.Debug($"[Padlock Manager Service]: starttimer is nessisary, so setting it.");
            _timerService.StartTimer($"{_config.padlockIdentifier[layerIndex]._padlockType}_Identifier{layerIndex}", _config.padlockIdentifier[layerIndex]._storedTimer, 
            1000, () => { ActionOnTimeElapsed(layerIndex); }, _config.selectedGagPadLockTimer, layerIndex);
            _config.Save();
        } 
    }

    /// <summary> see if the padlock we have locked contains a timer within it
    /// <list type="bullet">
    /// <item><c>slot</c><param name="slot"> - The slot.</param></item>
    /// </list> </summary>
    public bool IsLockedWithTimer(int slot) { 
        // lets us know if our lock has a timer. (used in general tab)");
        var padlockType = _config.padlockIdentifier[slot]._padlockType;
        // if the padlock is locked, and it is a padlock with a timer, return true
        return _config.isLocked[slot] &&
        (padlockType == LockableType.FiveMinutesPadlock || padlockType == LockableType.TimerPasswordPadlock || padlockType == LockableType.MistressTimerPadlock);
    }

    /// <summary> This method is used to handle the timer elapsed event, and is called when a timer elapses / finishes.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    private void ActionOnTimeElapsed(int layerIndex) { // the function to be used timer start actions
        GagSpeak.Log.Debug($"[Padlock Manager Service]: Timer elapsed! Unlocking from config, timerservice, and padlock identifers");
        // let the user know the timer elapsed, and unlock it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Your " +
        $"{_config.selectedGagPadlocks[layerIndex]}'s expired and was removed!").AddItalicsOff().BuiltString);
        _config.isLocked[layerIndex] = false; // let the gag layer be accessible again
        _config.padlockIdentifier[layerIndex].ClearPasswords(); // reset all input and stored items to default values.
        _config.padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, !_config.isLocked[layerIndex], _config);
    }

    /// <summary> If at any point we use /gagspeak safeword OUR_SAFEWORD, to clear all padlock and gag information and make it impossible for others to interact with you
    /// <list type="bullet">
    /// <item><c>sender</c><param name="sender"> - The sender.</param></item>
    /// <item><c>e</c><param name="e"> - The event arguments.</param></item>
    /// </list> </summary>
    private void CleanupVariables(object sender, SafewordCommandEventArgs e) {
        // clear EVERYTHING
        GagSpeak.Log.Debug("Safeword command invoked, and subscribed function called.");
        _config.isLocked = new List<bool> { false, false, false }; // reset is locked
        _config.LockDirectChatGarbler = false; // reset the garbler lock to be off
        _config.timerData.Clear(); // reset the timer data
        _timerService.ClearIdentifierTimers(); // and the associated timers timerdata reflected
        _config.padlockIdentifier = new List<PadlockIdentifier> { // new blank padlockidentifiers
            new PadlockIdentifier(),
            new PadlockIdentifier(),
            new PadlockIdentifier()
        };
        // reset the timers to current time
        _config.selectedGagPadLockTimer = new List<DateTimeOffset> {
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            DateTimeOffset.Now
        };
        // some dummy code to manually invoke the index change handler because im stupid and idk how to trigger events within an event trigger
        _config.selectedGagTypes[0] = _config.selectedGagTypes[0];
    }
#endregion GeneralGagAndLockFunctions

#region Wardrobe Functions
    /// <summary> This method locks the restraint set on your player by assigner name, for a set time, on a spesific set.
    /// <list type="bullet">
    /// <item><c>restraintSetName</c><param name="restraintSetName"> - The restraint set name.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The assigner name.</param></item>
    /// <item><c>timerDuration</c><param name="timerDuration"> - The timer duration.</param></item>
    /// <item><c>targetName</c><param name="targetName"> - The target name.</param></item>
    /// </list> </summary>
    public bool LockRestraintSet(string restraintSetName, string assignerName, string timerDuration = "", string targetName = "") {
        // get the restraint set from the list where its name property is equal. Must search with a iterative approach
        int setIndex = _restraintSetManager._restraintSets.FindIndex(set => set._name == restraintSetName);
        // if the restraint set is null, we cannot lock it
        if(setIndex < 0) {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: LockRestraintSet -> Restraint set does not exist, so we cannot lock it.");
            return false;
        }
        if(_restraintSetManager._restraintSets[setIndex]._enabled == false) {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: LockRestraintSet -> Restraint set is not enabled, so we cannot lock it.");
            return false;
        }
        // if the restraint set is locked, we cannot lock it
        if(_restraintSetManager._restraintSets[setIndex]._locked == true) {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: LockRestraintSet -> Restraint set is already locked, so we cannot lock it.");
            return false;
        }
        // otherwise, we can lock it
        GagSpeak.Log.Debug($"[Padlock Manager Service]: LockRestraintSet -> Restraint set is not locked, so we can lock it.");
        // if the timer duration is valid, we can lock it
        DateTimeOffset endTime = UIHelpers.GetEndTime(timerDuration);
        _restraintSetManager.ChangeRestraintSetNewLockEndTime(setIndex, endTime);
        _restraintSetManager.LockRestraintSet(setIndex, assignerName);
        // start the timer for the lock
        _timerService.StartTimer($"RestraintSet_{_restraintSetManager._restraintSets[setIndex]._name}",
            timerDuration, 1000, () =>
            {
                _restraintSetManager.TryUnlockRestraintSet(setIndex, assignerName); // attempts to lock it
            }
        );
        return true;
    }

    /// <summary> This method unlocks the restraint set on your player by assigner name
    /// <list type="bullet">
    /// <item><c>restraintSetName</c><param name="restraintSetName"> - The restraint set name.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The assigner name.</param></item>
    /// </list> </summary>
    public bool UnlockRestraintSet(string restraintSetName, string assignerName) {
        // get the restraint set from the list where its name property is equal. Must search with a iterative approach
        int setIndex = _restraintSetManager._restraintSets.FindIndex(set => set._name == restraintSetName);
        // if the restraint set is null, we cannot unlock it
        if(setIndex < 0) {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: UnlockRestraintSet -> Restraint set does not exist, so we cannot unlock it.");
            return false;
        }
        // if the restraint set is not locked, we cannot unlock it
        if(_restraintSetManager._restraintSets[setIndex]._locked == false) {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: {restraintSetName} is not locked, so we cannot unlock it.");
            return false;
        }
        if(_restraintSetManager._restraintSets[setIndex]._wasLockedBy != assignerName && _restraintSetManager._restraintSets[setIndex]._wasLockedBy != "self") {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: {restraintSetName} was locked by someone else!");
            return false;
        }
        // otherwise, we can unlock it
        GagSpeak.Log.Debug($"[Padlock Manager Service]: UnlockRestraintSet -> Restraint set is locked, so we can attempt to unlock it.");
        // unlock the restraint set
        if(_restraintSetManager.TryUnlockRestraintSet(setIndex, assignerName)) {
            _timerService.ClearRestraintSetTimer();
        }
        
        return true;
    }
#endregion Wardrobe Functions
}