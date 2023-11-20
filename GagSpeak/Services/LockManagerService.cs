using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using GagSpeak.Services;
using GagSpeak.Events;
using GagSpeak.Data;

namespace GagSpeak;
public class LockManager : IDisposable
{
    private readonly GagSpeakConfig _config;
    private readonly TimerService _timerService;
    private readonly SafewordUsedEvent _safewordUsedEvent;

    // constructor
    public LockManager(GagSpeakConfig config, TimerService timerService, SafewordUsedEvent safewordUsedEvent) {
        _config = config;
        _timerService = timerService;
        _safewordUsedEvent = safewordUsedEvent;

        // sub to events
        _safewordUsedEvent.SafewordCommand += CleanupVariables;
    }

    // dispose 
    public void Dispose() {
        _safewordUsedEvent.SafewordCommand -= CleanupVariables;
    }

    public void ToggleLock(int layerIndex) {
        GagSpeak.Log.Debug($"[Padlock Manager Service]: We are toggling our padlock.");
        if(_config._isLocked[layerIndex]) {
            Unlock(layerIndex);
        } else {
            Lock(layerIndex);
        }
    }

    private void Unlock(int layerIndex) {
        GagSpeak.Log.Debug($"[Padlock Manager Service]: We are unlocking our padlock.");
        if(_config._padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config._isLocked[layerIndex]) && _config._padlockIdentifier[layerIndex].CheckPassword()) {
            _config._isLocked[layerIndex] = false;
            _config._padlockIdentifier[layerIndex].ClearPasswords();
            _config._padlockIdentifier[layerIndex].UpdateConfigPadlockPasswordInfo(layerIndex, true, _config);
            _config.Save();
        } else {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Password for Padlock is incorrect.");
        }
    }

    private void Lock(int layerIndex) {
        GagSpeak.Log.Debug($"[Padlock Manager Service]: We are locking our padlock.");
        if(_config._padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config._isLocked[layerIndex])) {
            _config._isLocked[layerIndex] = true;
            _config._padlockIdentifier[layerIndex].UpdateConfigPadlockPasswordInfo(layerIndex, false, _config);
            StartTimerIfNecessary(layerIndex, _config, _timerService);
            _config.Save();
        } else {
            GagSpeak.Log.Debug($"[Padlock Manager Service]: Password for Padlock is incorrect.");
        }
    }

    private void StartTimerIfNecessary(int layerIndex, GagSpeakConfig _config, TimerService _timerService) {
        if(_config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.FiveMinutesPadlock ||
           _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.TimerPasswordPadlock ||
           _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.MistressTimerPadlock) {
            _timerService.StartTimer($"{_config._padlockIdentifier[layerIndex]._padlockType}_Identifier{layerIndex}", _config._padlockIdentifier[layerIndex]._storedTimer, 
            1000, () => {
                _config._isLocked[layerIndex] = false;
                _config._padlockIdentifier[layerIndex].ClearPasswords();
                _config._padlockIdentifier[layerIndex].UpdateConfigPadlockPasswordInfo(layerIndex, !_config._isLocked[layerIndex], _config);
            }, _config.selectedGagPadLockTimer, layerIndex);
        }
        _config.Save();
    }

    public bool IsLockedWithTimer(int slot) {
        var padlockType = _config._padlockIdentifier[slot]._padlockType;
        return _config._isLocked[slot] && (padlockType == GagPadlocks.FiveMinutesPadlock || 
                                            padlockType == GagPadlocks.TimerPasswordPadlock || 
                                            padlockType == GagPadlocks.MistressTimerPadlock);
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
}