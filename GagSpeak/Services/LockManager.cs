using System;                                       // Provides fundamental classes for base data types
using System.Collections.Generic;                   // Provides classes for defining generic collections
using GagSpeak.Services;                            // Contains service classes used in the GagSpeak application
using GagSpeak.Events;                              // Contains event classes used in the GagSpeak application
using GagSpeak.Data;                                // Contains data classes used in the GagSpeak application
using GagSpeak.UI.Helpers;                          // Contains chat classes used in the GagSpeak application
using Dalamud.Plugin.Services;                      // Contains service classes provided by the Dalamud plugin framework
using Dalamud.Game.Text.SeStringHandling.Payloads;  // Contains classes for handling special encoded (SeString) payloads in the Dalamud game
using Dalamud.Game.Text.SeStringHandling;           // Contains classes for handling special encoded (SeString) strings in the Dalamud game
using OtterGui.Classes;                             // Contains classes for managing the OtterGui framework

namespace GagSpeak;

/// <summary>
/// This class is used to handle command based lock interactions and UI based lock interactions.
/// </summary>
public class GagAndLockManager : IDisposable
{
    private readonly GagSpeakConfig     _config;            // for config options
    private readonly IChatGui           _clientChat;        // for chat messages
    private readonly IClientState       _clientState;       // for player payload
    private readonly TimerService       _timerService;      // for timers
    private readonly SafewordUsedEvent  _safewordUsedEvent; // for safeword event

    /// <summary>
    /// Initializes a new instance of the <see cref="GagAndLockManager"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak config.</param></item>
    /// <item><c>timerService</c><param name="timerService"> - The timer service.</param></item>
    /// <item><c>clientState</c><param name="clientState"> - The client state.</param></item>
    /// <item><c>safewordUsedEvent</c><param name="safewordUsedEvent"> - The safeword used event.</param></item>
    /// <item><c>clientChat</c><param name="clientChat"> - The client chat.</param></item>
    /// </list> </summary>
    public GagAndLockManager(GagSpeakConfig config, TimerService timerService, IClientState clientState,
    SafewordUsedEvent safewordUsedEvent , IChatGui clientChat) {
        _config = config;
        _clientChat = clientChat;
        _clientState = clientState;
        _timerService = timerService;
        _safewordUsedEvent = safewordUsedEvent;
        // subscribe to the safeword event
        _safewordUsedEvent.SafewordCommand += CleanupVariables;

        GagSpeak.Log.Debug("[GagAndLockManager] SERVICE CONSUTRCTOR INITIALIZED");
    }

    /// <summary> Unsubscribes from our subscribed event upon disposal </summary>
    public void Dispose() {
        _safewordUsedEvent.SafewordCommand -= CleanupVariables;
    }

    /// <summary>
    /// This method is used to handle gag applying command and UI presses
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>gagType</c><param name="gagType"> - The gag type.</param></item>
    /// </list> </summary>
    public void ApplyGag(int layerIndex, string gagType) {
        // apply the gag information to anywhere where it should be applied to within our code
        _config.selectedGagTypes[layerIndex] = gagType;
    }

    /// <summary>
    /// This method is used to handle individual gag removing command and UI presses
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void RemoveGag(int layerIndex) {
        // remove the gag information from anywhere where it should be removed from within our code
        _config.selectedGagTypes[layerIndex] = "None";
        _config.selectedGagPadlocks[layerIndex] = GagPadlocks.None;
        _config.selectedGagPadlocksPassword[layerIndex] = string.Empty;
        _config.selectedGagPadlocksAssigner[layerIndex] = "";
        _config._padlockIdentifier[layerIndex].ClearPasswords();
        _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, true, _config);
        // we dont worry about removing timers because if no lock, no timer.
    }

    /// <summary>
    /// This method is used to handle removing all of the gags through the command and UI interaction button
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void RemoveAllGags() {
        // remove all the gags from anywhere where it should be removed from within our code
        for(int i = 0; i < _config.selectedGagTypes.Count; i++) {
            RemoveGag(i);
        }
    }

    /// <summary>
    /// This method is used to handle the /gag lock command and UI interaction button. Acting as a toggle, this will call lock if unlocked, and unlock if locked.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    public void ToggleLock(int layerIndex) { // for the buttons
        if(_config._isLocked[layerIndex]) {
            Unlock(layerIndex); // call the base unlock function
        } else {
            Lock(layerIndex); // call the base lock function
        }
    }

    /// <summary>
    /// The base unlock function, used for detecting if a lock is capable of being unlocked or not based on passed in parameters.
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

    /// <summary>
    /// The base unlock function, used for detecting if a lock is capable of being unlocked or not based on passed in parameters.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The assigner name.</param></item>
    /// <item><c>password</c><param name="password"> - The password.</param></item>
    /// <item><c>targetName</c><param name="targetName"> - The target name.</param></item>
    /// </list> </summary>
    public void Unlock(int layerIndex, string assignerName, string password = "", string targetName = "", string YourPlayerName = "") { // for the buttons
        GagSpeak.Log.Debug($"[Padlock Manager Service]: We are unlocking our padlock.");
        // if what we use to try and unlock the padlock is valid, we can unlock it
        if(_config._padlockIdentifier[layerIndex].CheckPassword(_config, assignerName, targetName, password, YourPlayerName))
        {
            // unlock the padlock in all locations where we should be updating it as unlocked, clearing any stored lock information
            _config._isLocked[layerIndex] = false;
            _config._padlockIdentifier[layerIndex].ClearPasswords();
            _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, true, _config);
            _timerService.ClearIdentifierTimer(layerIndex);
            _config.Save();
        } else {
            // otherwise, we cannot unlock it
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Unlock was unsucessful.");
        }
    }

    /// <summary>
    /// The base lock function, used for detecting if a lock is capable of being locked or not based on passed in parameters.
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

    /// <summary>
    /// The augmented lock function, used for detecting if a lock is capable of being locked or not based on passed in parameters.
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
        // firstly, see if both our passwords are null, if it is true, it means this came from a button
        if(password1 == "" && password2 == "") {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: This Lock Request came from a button!");
            // if the padlock is valid, and has a valid password if it needs one, then we can lock
            if(_config._padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config._isLocked[layerIndex], _config,  assignerName, targetName, playerPayload.PlayerName)) {
                // if we reached this point it means our password was valid, so we can lock
                _config._isLocked[layerIndex] = true;
                _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, false, _config);
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
            if(_config._padlockIdentifier[layerIndex].SetAndValidate(_config, _config._padlockIdentifier[layerIndex]._padlockType.ToString(),
            password1, password2, assignerName, targetName)) {
                // if we reached this point it means our password was valid, so we can lock
                _config._isLocked[layerIndex] = true;
                _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, false, _config);
                StartTimerIfNecessary(layerIndex, _config, _timerService);
                _config.Save();
            } else {
                // otherwise, we cannot lock
                GagSpeak.Log.Debug($"[Padlock Manager Service]: LOCK -> Lock was unsucessful.");
                _config._padlockIdentifier[layerIndex].ClearPasswords();
            }
        }
    }

    /// <summary>
    /// This method is used start a timer if the padlock being locked on contains one, otherwise it shouldnt be called.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>padlockType</c><param name="padlockType"> - The padlock type.</param></item>
    /// </list> </summary>
    private void StartTimerIfNecessary(int layerIndex, GagSpeakConfig _config, TimerService _timerService) {
        GagSpeak.Log.Debug($"[Padlock Manager Service]: Checking if a starttimer is nessisary.");
        // just to double check this is actually a padlock with a timer
        if(_config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.FiveMinutesPadlock ||
        _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.TimerPasswordPadlock ||
        _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.MistressTimerPadlock)
        {   
            // assuming it is, start the timer
            GagSpeak.Log.Debug($"[Padlock Manager Service]: starttimer is nessisary, so setting it.");
            _timerService.StartTimer($"{_config._padlockIdentifier[layerIndex]._padlockType}_Identifier{layerIndex}", _config._padlockIdentifier[layerIndex]._storedTimer, 
            1000, () => { ActionOnTimeElapsed(layerIndex); }, _config.selectedGagPadLockTimer, layerIndex);
        }   // save the config
        _config.Save();
    }

    /// <summary>
    /// see if the padlock we have locked contains a timer within it
    /// <list type="bullet">
    /// <item><c>slot</c><param name="slot"> - The slot.</param></item>
    /// </list> </summary>
    public bool IsLockedWithTimer(int slot) { 
        // lets us know if our lock has a timer. (used in general tab)");
        var padlockType = _config._padlockIdentifier[slot]._padlockType;
        // if the padlock is locked, and it is a padlock with a timer, return true
        return _config._isLocked[slot] &&
        (padlockType == GagPadlocks.FiveMinutesPadlock || padlockType == GagPadlocks.TimerPasswordPadlock || padlockType == GagPadlocks.MistressTimerPadlock);
    }

    /// <summary>
    /// This method is used to handle the timer elapsed event, and is called when a timer elapses / finishes.
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// </list> </summary>
    private void ActionOnTimeElapsed(int layerIndex) { // the function to be used timer start actions
        GagSpeak.Log.Debug($"[Padlock Manager Service]: Timer elapsed! Unlocking from config, timerservice, and padlock identifers");
        // let the user know the timer elapsed, and unlock it
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Your " +
        $"{_config.selectedGagPadlocks[layerIndex]}'s expired and was removed!").AddItalicsOff().BuiltString);
        _config._isLocked[layerIndex] = false; // let the gag layer be accessible again
        _config._padlockIdentifier[layerIndex].ClearPasswords(); // reset all input and stored items to default values.
        _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, !_config._isLocked[layerIndex], _config);
    }

    /// <summary>
    /// If at any point we use /gagspeak safeword OUR_SAFEWORD, to clear all padlock and gag information and make it impossible for others to interact with you
    /// <list type="bullet">
    /// <item><c>sender</c><param name="sender"> - The sender.</param></item>
    /// <item><c>e</c><param name="e"> - The event arguments.</param></item>
    /// </list> </summary>
    private void CleanupVariables(object sender, SafewordCommandEventArgs e) {
        // clear EVERYTHING
        GagSpeak.Log.Debug("Safeword command invoked, and subscribed function called.");
        _config._isLocked = new List<bool> { false, false, false }; // reset is locked
        _config.LockDirectChatGarbler = false; // reset the garbler lock to be off
        _config.TimerData.Clear(); // reset the timer data
        _timerService.ClearIdentifierTimers(); // and the associated timers timerdata reflected
        _config._padlockIdentifier = new List<PadlockIdentifier> { // new blank padlockidentifiers
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
}