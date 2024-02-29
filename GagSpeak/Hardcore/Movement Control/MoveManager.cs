using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;
using Condition = Dalamud.Game.ClientState.Conditions.ConditionFlag;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using XivControl = FFXIVClientStructs.FFXIV.Client.Game.Control;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using GagSpeak.Utility;
using PInvoke;
using WinKeys = System.Windows.Forms.Keys;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
namespace GagSpeak.Hardcore.Movement;
public unsafe class MovementManager : IDisposable
{
    private readonly    GagSpeakConfig          _config;
    private readonly    HardcoreManager         _hcManager;
    private readonly    ICondition              _condition;
    private readonly    IClientState            _clientState;
    private readonly    IFramework              _framework;
    private readonly    IKeyState               _keyState;
    private readonly    RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private readonly    InitializationManager    _manager;
    // for having the movement memory -- was originally private static, revert back if it causes issues.
    private             MoveController      _MoveController;
    // for controlling walking speed, follow movement manager, and sitting/standing.
    public              XivControl.Control*     gameControl = XivControl.Control.Instance(); // instance to have control over our walking
    public              AgentMap*               agentMap = AgentMap.Instance(); // instance to have control over our walking
    // get the keystate ref values
    delegate ref        int                     GetRefValue(int vkCode);
    private static      GetRefValue             getRefValue;
    private             bool                    WasCancelled = false; // if true, we have cancelled any movement keys

    // the list of keys that are blocked while movement is disabled. Req. to be static, must be set here.
    public unsafe MovementManager(ICondition condition, IKeyState keyState,
    MoveController MoveController, HardcoreManager hardcoreManager,
    RS_PropertyChangedEvent RS_PropertyChangedEvent, IFramework framework, 
    IClientState clientState, GagSpeakConfig config, InitializationManager manager) {
        _config = config;
        _condition = condition;
        _clientState = clientState;
        _MoveController = MoveController;
        _framework = framework;
        _keyState = keyState;
        _hcManager = hardcoreManager;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;
        _manager = manager;
        // attempt to set the value safely
        HcHelpers.Safe(delegate {
            getRefValue = (GetRefValue)Delegate.CreateDelegate(typeof(GetRefValue), _keyState,
                        _keyState.GetType().GetMethod("GetRefValue",
                        BindingFlags.NonPublic | BindingFlags.Instance,
                        null, new Type[] { typeof(int) }, null));
        });

        // subscribe to the event
        _rsPropertyChangedEvent.SetChanged += OnRestraintSetPropertyChanged;
        // set that we are ready to true
        _manager.HardcoreManagerInitialized += OnHardcoreManagerInitialized;
        _manager._OrdersReadyForEvent.SetResult(true);
    }

    public void Dispose() {
        // unsubscribe from the event
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;
        _framework.Update -= framework_Update;
        _manager.HardcoreManagerInitialized -= OnHardcoreManagerInitialized;
    }
#region EventHandlers
    // this will be invoked when the hardcore manager is iniitalized. Only then will we finish enabling the rest of our information for the movement manager.
    private void OnHardcoreManagerInitialized() {
        // if the hardcore manager is initialized, we should set the movement controller
        GagSpeak.Log.Debug("======================== [ Completing Movement Manager Initialization ] ========================");

        // start the framework update cycle
        _framework.Update += framework_Update;
        // start the action manager update cycle
        _manager.CompleteStep(InitializationSteps.MovementManagerInitialized);
    }
    private void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // let us go back into non-rp mode once weighted is disabled
        if (e.PropertyType == HardcoreChangeType.Weighty && e.ChangeType == RestraintSetChangeType.Disabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Letting you run again");
            System.Threading.Tasks.Task.Delay(200);
            Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
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

        // if we are able to update our hardcore effects
        if (AllowFrameworkHardcoreUpdates()) {
            // and we are in a valid condition to do so (ignore this for now unless we get crash reports)
            //if(InConditionToApplyEffects()) {}

            // if the player is either ordered to sit, follow, or be immobile, we should handle movement prevention
            if(_hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedSit
            || _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedFollow
            || _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._rsProperties[_hcManager.ActiveHCsetIdx]._immobileProperty)
            {
                // if any of these are true, we should handle movement prevention
                HandleMovementPrevention();
            }
            // otherwise, we should enable movement and any blocked virtual keys
            else {
                _MoveController.EnableMouseMoving();
                ResetCancelledMoveKeys();
            }

            // if any conditions that would affect your walking state are active, then force walking to occur
            HandleWalkingState(); // if it is ever disabled, we will re-enable walking via event handling, but shouldnt do it in framework update or we will force people to perminantly run

            // if the player if forced to follow, we need to start a timer whenever they stop moving..
            // if the timer remains false for a certain amount of time, then we need to force them to move again
            if(_hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedFollow && agentMap != null) {
                // if the player is not moving...
                GagSpeak.Log.Debug($"[MovementManager]: Checking if player is moving");
                uint IsPlayerMoving = Marshal.ReadByte((IntPtr)agentMap, 23080);
                if(IsPlayerMoving == 1) {
                    // then we need to reset the timer
                    _lastMovementTime = DateTimeOffset.Now;
                } else {
                    // otherwise, we should check if the player has been standing still for 5000ms.
                    if((DateTimeOffset.Now - _lastMovementTime).TotalMilliseconds > 5000) {
                        // if they have, then we need to force them to move again
                        _hcManager.SetForcedFollow(_hcManager.ActivePlayerCfgListIdx, false);
                        GagSpeak.Log.Debug($"[MovementManager]: Player has been standing still for too long, forcing them to move again");
                    }
                }
            }
        }
    }

    private bool AllowFrameworkHardcoreUpdates() {
        return (
           _clientState.IsLoggedIn                          // we must be logged in
        && _clientState.LocalPlayer != null                 // our character must not be null
        && _clientState.LocalPlayer.Address != IntPtr.Zero  // our address must be valid
        && _config.AdminMode                                // we are in hardcore mode
        && _hcManager.ActivePlayerCfgListIdx != -1          // we must have an active player config
        && _hcManager.ActiveHCsetIdx != -1                  // we must have an active set enabled. 
        );                              // we must have an active set enabled.
    }
    // checks if we are in a proper condition to apply effects
    private bool InConditionToApplyEffects() {
        // if any of these are true, we should not apply effects
        var ret = _condition[Condition.Mounted] || _condition[Condition.BoundByDuty] || _condition[Condition.InCombat] ||
        _condition[Condition.BoundByDuty56] || _condition[Condition.BoundByDuty95] || _condition[Condition.BoundToDuty97];
        // so return the opposite
        return !ret;
    }


    // handles the walking state
    private void HandleWalkingState() {
        if(_hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedFollow
        || (_hcManager.ActiveHCsetIdx != -1  && _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._rsProperties[_hcManager.ActiveHCsetIdx]._weightyProperty))
        {
            uint isWalking = Marshal.ReadByte((IntPtr)gameControl, 23163);
            // force walking
            if (isWalking == 0) {
                Marshal.WriteByte((IntPtr)gameControl, 23163, 0x1);
            }
        }
    }

    // handle the prevention of our movenent.
    private void HandleMovementPrevention() {
        // if we are either forced to sit or forced to walk
        if(_hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedSit
        || _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedFollow) {
            // we should block mouse movement
            _MoveController.DisableMouseMoving();
        }
        // otherwise, we should allow mouse movement
        else {
            _MoveController.EnableMouseMoving();
        }
        // regardless, if any movement prevention is present, we should cancel any movement keys
        CancelMoveKeys();
    }

    private void CancelMoveKeys() {
        _config.MoveKeys.Each(x => {
            // the action to execute for each of our moved keys
            if (_keyState.GetRawValue(x) != 0) {
                // if the value is set to execute, cancel it.
                _keyState.SetRawValue(x, 0);
                // set was canceled to true
                WasCancelled = true;
                GagSpeak.Log.Verbose($"Cancelling key {x}");
            }
        });
    }

    private void ResetCancelledMoveKeys() {
        // if we had any keys canceled
        if (WasCancelled) {
            // set was cancelled back to false
            WasCancelled = false;
            // and restore the state of the virtual keys
            _config.MoveKeys.Each(x => {
                // the action to execute for each key
                if (HcHelpers.IsKeyPressed((Keys)x)) {
                    SetKeyState(x, 3);
                    GagSpeak.Log.Debug($"Reenabling key {x}");
                }
            });
        }
    }

    // set the key state
    private static void SetKeyState(VirtualKey key, int state) => getRefValue((int)key) = state;
    private DateTimeOffset _lastMovementTime = DateTimeOffset.Now;
#endregion Framework Updates
}