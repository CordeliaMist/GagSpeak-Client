using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using GagSpeak.Services;
using GagSpeak.Events;
using GagSpeak.Data;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;

namespace GagSpeak;
public class GagAndLockManager : IDisposable
{
    private readonly GagSpeakConfig _config;
    private readonly IChatGui _clientChat;
    private readonly IClientState _clientState;
    private readonly TimerService _timerService;
    private readonly SafewordUsedEvent _safewordUsedEvent;

    // constructor
    public GagAndLockManager(GagSpeakConfig config, TimerService timerService, IClientState clientState,
    SafewordUsedEvent safewordUsedEvent, IChatGui clientChat) {
        _config = config;
        _clientChat = clientChat;
        _clientState = clientState;
        _timerService = timerService;
        _safewordUsedEvent = safewordUsedEvent;
        // sub to events
        _safewordUsedEvent.SafewordCommand += CleanupVariables;
    }

    // dispose 
    public void Dispose() {
        _safewordUsedEvent.SafewordCommand -= CleanupVariables;
    }

    // Method to apply a gag
    public void ApplyGag(int layerIndex, string gagType) {
        _config.selectedGagTypes[layerIndex] = gagType;
    }

    // Method to remove a gag
    public void RemoveGag(int layerIndex) {
        _config.selectedGagTypes[layerIndex] = "None";
        _config.selectedGagPadlocks[layerIndex] = GagPadlocks.None;
        _config.selectedGagPadlocksPassword[layerIndex] = string.Empty;
        _config.selectedGagPadlocksAssigner[layerIndex] = "";
        _config._padlockIdentifier[layerIndex].ClearPasswords();
        _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, true, _config);
        // we dont worry about removing timers because if no lock, no timer.
    }

    // Method to remove all gags
    public void RemoveAllGags() {
        for(int i = 0; i < _config.selectedGagTypes.Count; i++) {
            RemoveGag(i);
        }
    }

    public void ToggleLock(int layerIndex) { // for the buttons
        if(_config._isLocked[layerIndex]) {
            Unlock(layerIndex); // button unlock
        } else {
            Lock(layerIndex); // button lock
        }
    }

    public void Unlock(int layerIndex) {
        PlayerPayload playerPayload = GetPlayerPayload();
        if(playerPayload != null) { // if the payload returned not null, we can use it
            Unlock(layerIndex, playerPayload.PlayerName, null, playerPayload.PlayerName);
        } else {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Player payload is null, so we are using the default name.");
            Unlock(layerIndex, null);
        }
    }
    // we only need to overload with 1 password, as the timerpassword doesnt need the time to unlock
    public void Unlock(int layerIndex, string assignerName, string password = null, string targetName = null) { // for the buttons
        GagSpeak.Log.Debug($"[Padlock Manager Service]: We are unlocking our padlock.");
        if(_config._padlockIdentifier[layerIndex].CheckPassword(_config, assignerName, targetName, password))
        {
            _config._isLocked[layerIndex] = false;
            _config._padlockIdentifier[layerIndex].ClearPasswords();
            _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, true, _config);
            _timerService.ClearIdentifierTimer(layerIndex);
            _config.Save();
        } else {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Unlock was unsucessful.");
        }
    }

    // only will be triggered by buttons
    public void Lock(int layerIndex) {
        PlayerPayload playerPayload = GetPlayerPayload();
        if(playerPayload != null) { // if the payload returned not null, we can use it
            // string[] nameParts = playerPayload.PlayerName.Split(' ');
            // string playerName = nameParts[0] + " " + nameParts[1];
            Lock(layerIndex, playerPayload.PlayerName, null, null, playerPayload.PlayerName);
        } else {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Player payload is null, so we are using the default name.");
            Lock(layerIndex, null);
        }
    }
    // occurs when recieving incoming messages during the msg result logic. AKA the sender is your assigner, target is you
    public void Lock(int layerIndex, string assignerName, string password1 = null, string password2 = null, string targetName = null) {
        // firstly, see if both our passwords are null, if it is true, it means this came from a button
        if(password1 == null && password2 == null) {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: This Lock Request came from a button!");
            // perform the button lock sequence
            if(_config._padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config._isLocked[layerIndex], _config,  assignerName, targetName)) {
                _config._isLocked[layerIndex] = true;
                _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, false, _config);
                StartTimerIfNecessary(layerIndex, _config, _timerService);
                _config.Save();
            } else {
                GagSpeak.Log.Debug($"[Padlock Manager Service]: LOCK -> Lock was unsucessful.");
            }
        }
        // otherwise, it means this came from a command, so we need to check if the passwords are valid
        else {
            // we will need to setandvalidate, over just validate
            GagSpeak.Log.Debug($"[Padlock Manager Service]: This Lock Request came from a command!");
            if(_config._padlockIdentifier[layerIndex].SetAndValidate(_config, _config._padlockIdentifier[layerIndex]._padlockType.ToString(),
            password1, password2, assignerName, targetName))
            {
                // if we reached this point it means our password was valid, so we can lock
                _config._isLocked[layerIndex] = true;
                _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, false, _config);
                StartTimerIfNecessary(layerIndex, _config, _timerService);
                _config.Save();
            } else {
                GagSpeak.Log.Debug($"[Padlock Manager Service]: LOCK -> Lock was unsucessful.");
                _config._padlockIdentifier[layerIndex].ClearPasswords();
            }
        }
    }

    private void StartTimerIfNecessary(int layerIndex, GagSpeakConfig _config, TimerService _timerService) {
        GagSpeak.Log.Debug($"[Padlock Manager Service]: Checking if a starttimer is nessisary.");
        if(_config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.FiveMinutesPadlock ||
        _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.TimerPasswordPadlock ||
        _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.MistressTimerPadlock)
        {   // start the timer
            GagSpeak.Log.Debug($"[Padlock Manager Service]: starttimer is nessisary, so setting it.");
            _timerService.StartTimer($"{_config._padlockIdentifier[layerIndex]._padlockType}_Identifier{layerIndex}", _config._padlockIdentifier[layerIndex]._storedTimer, 
            1000, () => { ActionOnTimeElapsed(layerIndex); }, _config.selectedGagPadLockTimer, layerIndex);
        }   // save the config
        _config.Save();
    }

    public bool IsLockedWithTimer(int slot) { // lets us know if our lock has a timer. (used in general tab)");
        var padlockType = _config._padlockIdentifier[slot]._padlockType;
        return _config._isLocked[slot] &&
        (padlockType == GagPadlocks.FiveMinutesPadlock || padlockType == GagPadlocks.TimerPasswordPadlock || padlockType == GagPadlocks.MistressTimerPadlock);
    }

    private void ActionOnTimeElapsed(int layerIndex) { // the function to be used timer start actions
        GagSpeak.Log.Debug($"[Padlock Manager Service]: Timer elapsed! Unlocking from config, timerservice, and padlock identifers");
        _clientChat.Print(new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Your " +
        $"{_config.selectedGagPadlocks[layerIndex]}'s expired and was removed!").AddItalicsOff().BuiltString);
        _config._isLocked[layerIndex] = false; // let the gag layer be accessible again
        _config._padlockIdentifier[layerIndex].ClearPasswords(); // reset all input and stored items to default values.
        _config._padlockIdentifier[layerIndex].UpdateConfigPadlockInfo(layerIndex, !_config._isLocked[layerIndex], _config);
    }

    // cleanup variables upon safeword
    private void CleanupVariables(object sender, SafewordCommandEventArgs e) {
        _config._isLocked = new List<bool> { false, false, false }; // reset is locked
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
        // some dummy code to manually invoke the index change handler because im stupid.
        _config.selectedGagTypes[0] = _config.selectedGagTypes[0];
    }

    public PlayerPayload GetPlayerPayload() { // gets the player payload
        try { 
            return new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
        }
        catch {
            GagSpeak.Log.Debug("[MsgResultLogic]: Failed to get player payload, returning null");
            return null;
        }
    }
}