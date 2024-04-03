using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;
using Condition = Dalamud.Game.ClientState.Conditions.ConditionFlag;
using XivControl = FFXIVClientStructs.FFXIV.Client.Game.Control;
using Dalamud.Plugin.Services;
using GagSpeak.Events;
using GagSpeak.Utility;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using GagSpeak.CharacterData;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Collections.Generic;
using GagSpeak.Services;

namespace GagSpeak.Hardcore.Movement;
public class MovementManager : IDisposable
{
    private readonly    GagSpeakConfig          _config;
    private readonly    HardcoreManager         _hcManager;
    private readonly    ICondition              _condition;
    private readonly    IClientState            _clientState;
    private readonly    IFramework              _framework;
    private readonly    IKeyState               _keyState;
    private readonly    CharacterHandler        _charaManager;
    private readonly    IObjectTable            _objectTable;   // object table
    private readonly    OptionPromptListeners   _autoDialogSelect;
    private readonly    OnFrameworkService      _onFrameworkService;
    private readonly    RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private readonly    InitializationManager    _manager;
    // for having the movement memory -- was originally private static, revert back if it causes issues.
    private             MoveController          _MoveController;
    // for controlling walking speed, follow movement manager, and sitting/standing.
    public unsafe       GameCameraManager*      cameraManager = GameCameraManager.Instance(); // for the camera manager object
    public unsafe       XivControl.Control*     gameControl = XivControl.Control.Instance(); // instance to have control over our walking
    // get the keystate ref values
    delegate ref        int                     GetRefValue(int vkCode);
    private static      GetRefValue?            getRefValue;
    private             bool                    WasCancelled = false; // if true, we have cancelled any movement keys

    // the list of keys that are blocked while movement is disabled. Req. to be static, must be set here.
    public MovementManager(ICondition condition, IKeyState keyState, MoveController MoveController,
    HardcoreManager hardcoreManager, IObjectTable objectTable, RS_PropertyChangedEvent RS_PropertyChangedEvent,
    IFramework framework, OnFrameworkService onFrameworkService, IClientState clientState,
    GagSpeakConfig config, InitializationManager manager, CharacterHandler characterManager,
    OptionPromptListeners autoDialogSelect) {
        _config = config;
        _condition = condition;
        _charaManager = characterManager;
        _clientState = clientState;
        _MoveController = MoveController;
        _framework = framework;
        _objectTable = objectTable;
        _keyState = keyState;
        _onFrameworkService = onFrameworkService;
        _hcManager = hardcoreManager;
        _autoDialogSelect = autoDialogSelect;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;
        _manager = manager;
        
        // attempt to set the value safely
        GenericHelpers.Safe(delegate {
            getRefValue = (GetRefValue)Delegate.CreateDelegate(typeof(GetRefValue), _keyState, 
                            _keyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null)!);
        });

        // run an async task that will await to apply affects until we are logged in, after which it will fire the restraint effect logic
        Task.Run(async () => {
            if(!IsPlayerLoggedIn()) {
                GSLogger.LogType.Debug($"[RestraintSetManager] Waiting for login to complete before activating restraint set");
                while (!_clientState.IsLoggedIn || _clientState.LocalPlayer == null || _clientState.LocalPlayer.Address == IntPtr.Zero && _clientState.LocalContentId == 0) {
                    await Task.Delay(2000); // Wait for 1 second before checking the login status again
                }
            }
            // if we are being forced to follow by anyone
            if(_hcManager.IsForcedFollowingForAny(out int enabledFollowIdx, out string playerWhoForceFollowedYou)) {
                _hcManager.HandleForcedFollow(enabledFollowIdx, _hcManager._perPlayerConfigs[enabledFollowIdx]._forcedFollow);
            }
            // if we are blindfolded by anyone, we should apply that as well
            if(_hcManager.IsBlindfoldedForAny(out int enabledBlindfoldIdx, out string playerWhoBlindfoldedYou)) {
                await _hcManager.HandleBlindfoldLogic(enabledBlindfoldIdx, _hcManager._perPlayerConfigs[enabledBlindfoldIdx]._blindfolded, playerWhoBlindfoldedYou);
            }
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
        // enable movement
        _MoveController.CompletelyEnableMovement();
        ResetCancelledMoveKeys();
    }

    private bool IsPlayerLoggedIn() => _clientState.IsLoggedIn && _clientState.LocalPlayer != null && _clientState.LocalPlayer.Address != IntPtr.Zero;
#region EventHandlers
    // this will be invoked when the hardcore manager is iniitalized. Only then will we finish enabling the rest of our information for the movement manager.
    private void OnHardcoreManagerInitialized() {
        // if the hardcore manager is initialized, we should set the movement controller
        GSLogger.LogType.Information(" Completing Movement Manager Initialization ");
        // start the framework update cycle
        _framework.Update += framework_Update;
        // start the action manager update cycle
        _manager.CompleteStep(InitializationSteps.MovementManagerInitialized);
    }
    private unsafe void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // let us go back into non-rp mode once weighted is disabled
        if (e.PropertyType == HardcoreChangeType.Weighty && e.ChangeType == RestraintSetChangeType.Disabled) {
            GSLogger.LogType.Debug($"[Action Manager]: Letting you run again");
            System.Threading.Tasks.Task.Delay(200);
            Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
        }

        if(e.PropertyType == HardcoreChangeType.ForcedFollow && e.ChangeType == RestraintSetChangeType.Disabled) {
            if(!_hcManager._perPlayerConfigs.Any(x => x._forcedSit) && !_hcManager._perPlayerConfigs.Any(x => x._forcedFollow)) {
                _MoveController.CompletelyEnableMovement();
                ResetCancelledMoveKeys();
            }
        }
        if(e.PropertyType == HardcoreChangeType.ForcedSit && e.ChangeType == RestraintSetChangeType.Disabled) {
            if(!_hcManager._perPlayerConfigs.Any(x => x._forcedSit) && !_hcManager._perPlayerConfigs.Any(x => x._forcedFollow)) {
                _MoveController.CompletelyEnableMovement();
                ResetCancelledMoveKeys();
            }
        }
    }

#endregion EventHandlers
#region Framework Updates
    private void framework_Update(IFramework framework) => OnFrameworkInternal();
    private unsafe void OnFrameworkInternal() {
        // make sure we only do checks when we are properly logged in and have a character loaded
        if (_clientState.LocalPlayer?.IsDead ?? false || _onFrameworkService._sentBetweenAreas) {
            return;
        }

        // if we are able to update our hardcore effects
        if (AllowFrameworkHardcoreUpdates()) {
            // and we are in a valid condition to do so (ignore this for now unless we get crash reports)
            //if(InConditionToApplyEffects())

            // If the player is being forced to sit, we want to completely immobilize them
            var sitting = isForcedSitting();
            var following = isForcedFollowing();
            var immobile = isImmobile();
            if(sitting || following || immobile) {
                HandleMovementPrevention(following, sitting, immobile);
            }
            else {
                _MoveController.CompletelyEnableMovement();
                ResetCancelledMoveKeys();
            }
            
            // if any conditions that would affect your walking state are active, then force walking to occur
            HandleWalkingState(); 

            // if player is in forced follow state, we need to track their position so we can auto turn it off if they are standing still for 6 seconds
            if(_hcManager.IsForcedFollowingForAny(out int enabledFollowIdx, out string playerWhoForceFollowedYou)) {
                // if the player is not moving...
                if(_clientState.LocalPlayer!.Position != _hcManager.LastPosition) {
                    _hcManager.LastMovementTime = DateTimeOffset.Now;           // reset timer
                    _hcManager.LastPosition = _clientState.LocalPlayer.Position;// update last position
                } 
                // otherwise, they are not moving, so check if the timer has gone past 6000ms
                else {
                    if((DateTimeOffset.Now - _hcManager.LastMovementTime).TotalMilliseconds > 6000) {
                        // if they have, then we need to force them to move again
                        var index = _hcManager._perPlayerConfigs.FindIndex(x => x._forcedFollow);
                        // set the forced follow to false
                        _hcManager.SetForcedFollow(index, false);
                        GSLogger.LogType.Debug($"[MovementManager]: Player has been standing still for too long, forcing them to move again");
                    }
                }
            }
            
            // if we are allowing forced to stay from anyone (you dont need to have the option locked on, just the allowance one) then enable the hooks
            if(EnableOptionPromptHooks()) {
                // enable the hooks for the option prompts
                _autoDialogSelect.Enable(); 
                // while they are active, if we are not in a dialog prompt option, scan to see if we are by an estate enterance
                if (_hcManager.IsForcedToStayForAny(out int enabledIdx, out string playerWhoForceStayedYou) 
                && _condition[Condition.OccupiedInQuestEvent] == false
                && _onFrameworkService._sentBetweenAreas == false)
                {
                    // grab all the event object nodes (door interactions)
                    List<Dalamud.Game.ClientState.Objects.Types.GameObject>? nodes = _objectTable.Where(x => x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.EventObj && GetTargetDistance(x) < 3.5f).ToList();
                    foreach (var obj in nodes) {
                        // only follow through with the node named "Entrance"
                        if(obj.Name.TextValue == "Entrance") {
                            TargetSystem.Instance()->InteractWithObject((GameObject*)(nint)obj.Address, false);
                        }
                    }
                }

            }
            // otherwise, disable the hooks if it is inactive
            else {
                _autoDialogSelect.Disable(); // disable the hooks for prompt selection
            }

            // if we are blindfoled and have forcedfirstperson to true, force first person
            if(_hcManager.IsBlindfoldedForAny(out int enabledBlindfoldIdx, out string playerWhoBlindfoldedYou)
            && _hcManager._perPlayerConfigs[enabledBlindfoldIdx]._forceLockFirstPerson)
            {
                if(cameraManager != null && cameraManager->Camera != null
                && cameraManager->Camera->Mode != (int)CameraControlMode.FirstPerson)
                {
                    // force first person
                    cameraManager->Camera->Mode = (int)CameraControlMode.FirstPerson;
                }
            }
        }
    }


    // Helper functions for minimizing the content in the framework update code section above
    public float GetTargetDistance(Dalamud.Game.ClientState.Objects.Types.GameObject target) {
        Vector2 position = new(target.Position.X, target.Position.Z);
        Vector2 selfPosition = new(_clientState.LocalPlayer!.Position.X, _clientState.LocalPlayer.Position.Z);
        return Math.Max(0, Vector2.Distance(position, selfPosition) - target.HitboxRadius - _clientState.LocalPlayer.HitboxRadius);
    }

    public unsafe void TryInteract(GameObject* baseObj) {
        if (baseObj->GetIsTargetable())
            TargetSystem.Instance()->InteractWithObject(baseObj, true);
    }

    private bool isForcedSitting() => _hcManager._perPlayerConfigs.Any(x => x._forcedSit);
    private bool isForcedFollowing() => _hcManager._perPlayerConfigs.Any(x => x._forcedFollow);
    private bool isImmobile() {
        if(_hcManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int idxOfAssigner)) {
            return _hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[enabledIdx]._immobileProperty;
        }
        return false;
    }
    private bool EnableOptionPromptHooks() => _hcManager._perPlayerConfigs.Any(x => x._allowForcedToStay);

    private bool AllowFrameworkHardcoreUpdates() {
        return (
           _clientState.IsLoggedIn                          // we must be logged in
        && _clientState.LocalPlayer != null                 // our character must not be null
        && _clientState.LocalPlayer.Address != IntPtr.Zero  // our address must be valid
        && _config.hardcoreMode                             // we are in hardcore mode
        );                                                  // we must have an active set enabled.
    }

    // handles the walking state
    private unsafe void HandleWalkingState() {
        if(_hcManager.IsForcedFollowingForAny(out int enabledFollowIdx, out string playerWhoForceFollowedYou) // if anyone is making us forcefollow
        || (_hcManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int idxOfAssigner)   // OR we have any set enabled
                && enabledIdx != -1                                                                           // with a valid set idx
                && _hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[enabledIdx]._weightyProperty))   // that has their weighty property enabled
        {
            // get the byte that sees if the player is walking
            uint isWalking = Marshal.ReadByte((IntPtr)gameControl, 23163);
            // and if they are not, force it.
            if (isWalking == 0) {
                Marshal.WriteByte((IntPtr)gameControl, 23163, 0x1);
            }
        }
    }

    // handle the prevention of our movenent.
    private void HandleMovementPrevention(bool following, bool sitting, bool immobile) {
        if(sitting) {
            _MoveController.CompletelyDisableMovement(true, true); // set pointer and turn off mouse and disable emotes
        }
        else if(immobile) {
            _MoveController.CompletelyDisableMovement(true, true); // set pointer but dont turn off mouse
        }
        // otherwise if we are forced to follow
        else if(following) {
            // in this case, we want to maske sure to block players keys and force them to legacy mode.
            if(GameConfig.UiControl.GetBool("MoveMode") == false) {
                GameConfig.UiControl.Set("MoveMode", (int)MovementMode.Legacy);
            }
            // dont set pointer, but disable mouse
            _MoveController.CompletelyDisableMovement(false, true); // disable mouse
        }
        // otherwise, we should re-enable the mouse blocking and immobilization traits
        else {
            _MoveController.CompletelyEnableMovement(); // re-enable both
        }
        // cancel our set keys such as auto run ext, immobilization skips previous two and falls under this
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
                GSLogger.LogType.Verbose($"Cancelling key {x}");
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
                if (GenericHelpers.IsKeyPressed((Keys)x)) {
                    SetKeyState(x, 3);
                }
            });
        }
    }

    // set the key state (if you start crashing when using this you probably have a fucked up getrefvalue)
    private static void SetKeyState(VirtualKey key, int state) => getRefValue!((int)key) = state;
#endregion Framework Updates
}