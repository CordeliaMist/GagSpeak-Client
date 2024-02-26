using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Events;

namespace GagSpeak.Hardcore.Movement;
public unsafe class MovementManager : IDisposable
{
    // This will fire an event whenever a event is triggered, sending you info on who assigned it and what set it was assigned to.
    private readonly    RS_ToggleEvent      _rsToggleEvent;
    // this will give you information about the playercharacter data, which you will need to get the current state of each hardcore property and its configuration
    private readonly    CharacterHandler    _characterHandler;
    private readonly    GagSpeakConfig      _config;
    private readonly    HardcoreManager     _hardcoreManager;
    private readonly    ICondition          _condition;
    private readonly    IClientState        _clientState;
    private readonly    IFramework          _framework;
    private readonly    RS_PropertyChangedEvent _rsPropertyChangedEvent;
    // for having the movement memory -- was originally private static, revert back if it causes issues.
    private static      MoveMemory          _moveMemory;
    public static readonly int[] _blockedKeys = new int[] { 321, 322, 323, 324, 325, 326 };
    // for controlling walking speed
    public FFXIVClientStructs.FFXIV.Client.Game.Control.Control* gameControl = FFXIVClientStructs.FFXIV.Client.Game.Control.Control.Instance(); // instance to have control over our walking

    
    // the list of keys that are blocked while movement is disabled. Req. to be static, must be set here.
    public unsafe MovementManager(RS_ToggleEvent RS_ToggleEvent, CharacterHandler characterHandler, ICondition condition,
    MoveMemory moveMemory, HardcoreManager hardcoreManager, RS_PropertyChangedEvent RS_PropertyChangedEvent,
    IFramework framework, IClientState clientState, GagSpeakConfig config) {
        _rsToggleEvent = RS_ToggleEvent;
        _config = config;
        _characterHandler = characterHandler;
        _condition = condition;
        _clientState = clientState;
        _moveMemory = moveMemory;
        _framework = framework;
        _hardcoreManager = hardcoreManager;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;

        // subscribe to the event
        _rsToggleEvent.SetToggled += OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged += OnRestraintSetPropertyChanged;
        _framework.Update += framework_Update;
    }

    public void Dispose() {
        // unsubscribe from the event
        _rsToggleEvent.SetToggled -= OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;

        // if we are locked, unlock us
        if (_moveMemory.ForceDisableMovement > 0) {
            EnableMoving();
        }
    }


    // helper functions and other general management functions can go here for appending and extracting information from the hardcore manager.
    private unsafe void EnableMoving() {
        GagSpeak.Log.Debug($"Enabling moving, cnt {_moveMemory.ForceDisableMovement}");
        // disable our hooks, we dont need to track them anymore.
        _moveMemory.DisableHooks();
        if (_moveMemory.ForceDisableMovement > 0) {
            _moveMemory.ForceDisableMovement--;
        }
    }

    private void DisableMoving() {
        GagSpeak.Log.Debug($"Disabling moving, cnt {_moveMemory.ForceDisableMovement}");
        // reinable our hooks
        _moveMemory.EnableHooks();
        _moveMemory.ForceDisableMovement++;
    }

#region EventHandlers
    private void JobChangeEventFired(object sender, GagSpeakGlamourEventArgs e) {

    }

    private void OnRestraintSetToggled(object sender, RS_ToggleEventArgs e) {
    }

    private void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // let us go back into non-rp mode once weighted is disabled
        if (e.PropertyType == HardcoreChangeType.Weighty && e.ChangeType == RestraintSetChangeType.Disabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Letting you run again");
            System.Threading.Tasks.Task.Delay(200);
            Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
        }
        // roundabout way of saying "If any other options are already active, there is no need to activate it again
        if(RestraintSetChangeType.Enabled == e.ChangeType) {
            switch(e.PropertyType) {
                case HardcoreChangeType.Immobile:
                case HardcoreChangeType.ForcedSit:
                case HardcoreChangeType.ForcedFollow: {
                    if(_hardcoreManager._forcedFollow || _hardcoreManager._forcedSit ||
                    (_hardcoreManager.ActiveSetIdxEnabled != -1  && _hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._weightyProperty))
                    {
                        // if any of these are already active, dont worry about activating movement more, so return
                        return;
                    }
                    // otherwise, disable movement
                    else {
                        DisableMoving();
                    }
                }
                break;
            }
        }
        // roundabout way of saying "If any other options are already active, then we shouldnt be able to deactive them"
        if(RestraintSetChangeType.Disabled == e.ChangeType) {
            switch(e.PropertyType) {
                case HardcoreChangeType.Immobile:
                case HardcoreChangeType.ForcedSit:
                case HardcoreChangeType.ForcedFollow: {
                    if(_hardcoreManager._forcedFollow || _hardcoreManager._forcedSit ||
                    (_hardcoreManager.ActiveSetIdxEnabled != -1  && _hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._weightyProperty))
                    {
                        // if any of these are already active, dont worry about activating movement more, so return
                        return;
                    }
                    // otherwise, enable movement
                    else {
                        EnableMoving();
                    }
                }
                break;
            }
        }
    }

#endregion EventHandlers
#region Framework Updates
    private void framework_Update(IFramework framework) {
        // make sure we only do checks when we are properly logged in and have a character loaded
        if (_clientState.LocalPlayer?.IsDead ?? false) {
            GagSpeak.Log.Debug($"[FrameworkUpdate]  Player is dead, returning");
            return;
        }
        if (_clientState.IsLoggedIn 
        &&  _clientState.LocalPlayer != null
        && _clientState.LocalPlayer.Address != IntPtr.Zero
        && _config.AdminMode)
        {
            // if any conditions that would affect your walking state are active, then force walking to occur
            if(_hardcoreManager._forcedFollow
            || (_hardcoreManager.ActiveSetIdxEnabled != -1  && _hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._weightyProperty))
            {
                uint isWalking = Marshal.ReadByte((IntPtr)gameControl, 23163);
                if (_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Mounted] || 
                    _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] || 
                    _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] ||
                    _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty56] ||
                    _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty95] ||
                    _condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundToDuty97])
                {
                    GagSpeak.Log.Debug($"[Action Manager]: {isWalking}");
                    if (isWalking == 1) {
                        // let them run again if they are in combat, mounted, or bound by duty
                        Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
                    }
                }
                else if (isWalking == 0) {
                    Marshal.WriteByte((IntPtr)gameControl, 23163, 0x1);
                }
            }

            // other statements here.
        }
    }
#endregion Framework Updates
}