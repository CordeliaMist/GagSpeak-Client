using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Keys;
using Condition = Dalamud.Game.ClientState.Conditions.ConditionFlag;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using XivControl = FFXIVClientStructs.FFXIV.Client.Game.Control;
using Dalamud.Plugin.Services;
using GagSpeak.Events;
using GagSpeak.Utility;
using PInvoke;
using WinKeys = System.Windows.Forms.Keys;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using GagSpeak.Gagsandlocks;
using GagSpeak.CharacterData;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

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
    private readonly    RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private readonly    InitializationManager    _manager;
    // for having the movement memory -- was originally private static, revert back if it causes issues.
    private             MoveController          _MoveController;
    // for controlling walking speed, follow movement manager, and sitting/standing.
    public unsafe       XivControl.Control*     gameControl = XivControl.Control.Instance(); // instance to have control over our walking
    public unsafe       AgentMap*               agentMap = AgentMap.Instance(); // instance to have control over our walking

    // get the keystate ref values
    delegate ref        int                     GetRefValue(int vkCode);
    private static      GetRefValue             getRefValue;
    private             bool                    WasCancelled = false; // if true, we have cancelled any movement keys
    private static      MovementMode            CameraMode = MovementMode.Standard; // camera mode fetched from movement mode

    // the list of keys that are blocked while movement is disabled. Req. to be static, must be set here.
    public MovementManager(ICondition condition, IKeyState keyState,
    MoveController MoveController, HardcoreManager hardcoreManager,
    RS_PropertyChangedEvent RS_PropertyChangedEvent, IFramework framework, 
    IClientState clientState, GagSpeakConfig config, InitializationManager manager,
    CharacterHandler characterManager) {
        _config = config;
        _condition = condition;
        _charaManager = characterManager;
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
                            _keyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null));
        });

        // run an async task that will await to apply affects until we are logged in, after which it will fire the restraint effect logic
        Task.Run(async () => {
            if(!IsPlayerLoggedIn()) {
                GagSpeak.Log.Debug($"[RestraintSetManager] Waiting for login to complete before activating restraint set");
                while (!_clientState.IsLoggedIn || _clientState.LocalPlayer == null || _clientState.LocalPlayer.Address == IntPtr.Zero) {
                    await Task.Delay(3000); // Wait for 1 second before checking the login status again
                }
            }
            int idx = _hcManager._perPlayerConfigs.FindIndex(x => x._forcedFollow == true);
            if(idx != -1) { 
                _hcManager.HandleForcedFollow(idx, _hcManager._perPlayerConfigs[idx]._forcedFollow);
            }
            idx = _hcManager._perPlayerConfigs.FindIndex(x => x._blindfolded == true);
            if(idx != -1) {
                _hcManager.HandleBlindfoldLogic(idx, _hcManager._perPlayerConfigs[idx]._blindfolded, _charaManager.whitelistChars[idx]._name);
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
        GagSpeak.Log.Debug("======================== [ Completing Movement Manager Initialization ] ========================");

        // start the framework update cycle
        _framework.Update += framework_Update;
        // start the action manager update cycle
        _manager.CompleteStep(InitializationSteps.MovementManagerInitialized);
    }
    private unsafe void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // let us go back into non-rp mode once weighted is disabled
        if (e.PropertyType == HardcoreChangeType.Weighty && e.ChangeType == RestraintSetChangeType.Disabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Letting you run again");
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
        if (_clientState.LocalPlayer?.IsDead ?? false) {
            GagSpeak.Log.Debug($"[FrameworkUpdate]  Player is dead, returning");
            return;
        }

        // if we are able to update our hardcore effects
        if (AllowFrameworkHardcoreUpdates()) {
            // and we are in a valid condition to do so (ignore this for now unless we get crash reports)
            //if(InConditionToApplyEffects()) {}

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
            HandleWalkingState(); // if it is ever disabled, we will re-enable walking via event handling, but shouldnt do it in framework update or we will force people to perminantly run

            // if the player if forced to follow, we need to start a timer whenever they stop moving..
            // if the timer remains false for a certain amount of time, then we need to force them to move again
            if(_hcManager._perPlayerConfigs.Any(x => x._forcedFollow) && agentMap != null) {
                // if the player is not moving...
                uint IsPlayerMoving = Marshal.ReadByte((IntPtr)agentMap, 23080);
                //GagSpeak.Log.Debug($"[MovementManager]: IsPlayerMoving: {IsPlayerMoving}");
                if(IsPlayerMoving == 1) {
                    // then we need to reset the timer
                    _hcManager.LastMovementTime = DateTimeOffset.Now;
                } else {
                    // otherwise, we should check if the player has been standing still for 5000ms.
                    if((DateTimeOffset.Now - _hcManager.LastMovementTime).TotalMilliseconds > 15000) {
                        // if they have, then we need to force them to move again
                        // find the index in the hardcore manager that has their forced follow set to true
                        var index = _hcManager._perPlayerConfigs.FindIndex(x => x._forcedFollow);
                        // set the forced follow to false
                        _hcManager.SetForcedFollow(index, false);
                        GagSpeak.Log.Debug($"[MovementManager]: Player has been standing still for too long, forcing them to move again");
                    }
                }
            }

            // handle blinfolded camera action
            if(_hcManager._perPlayerConfigs.Any(x => x._forcedSit)) {
                var localChar = (Character*)(_clientState.LocalPlayer?.Address ?? IntPtr.Zero);
                EmoteController* controller = &(localChar->EmoteController);
                uint val = Marshal.ReadByte((IntPtr)controller, 20);
                uint val2 = Marshal.ReadByte((IntPtr)controller, 33);
                if((val == 50 && val2 == 0) || (val == 98 && val2 == 2) || (val == 117 && val2 == 3)) {
                    Marshal.WriteByte((IntPtr)controller, 20, 97);
                    Marshal.WriteByte((IntPtr)controller, 33, 1);
                }
            }
        }
    }

    public unsafe EmoteController* GetEmoteController() {
        XivControl.Control* controlInstance = XivControl.Control.Instance();
        if (controlInstance == null)
        {
            throw new InvalidOperationException("Control instance is null.");
        }

        BattleChara* localPlayer = controlInstance->LocalPlayer;
        if (localPlayer == null)
        {
            throw new InvalidOperationException("Local player is null.");
        }

        Character character = localPlayer->Character;

        EmoteController emoteController = character.EmoteController;

        return &emoteController;
    }

    private bool isForcedSitting() => _hcManager._perPlayerConfigs.Any(x => x._forcedSit);
    private bool isForcedFollowing() => _hcManager._perPlayerConfigs.Any(x => x._forcedFollow);
    private bool isImmobile() => ImmobileActiveAndValid();
    private bool ImmobileActiveAndValid() {
        return _hcManager.ActiveHCsetIdx != -1
            && _hcManager.ActivePlayerCfgListIdx != -1
            && _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._rsProperties[_hcManager.ActiveHCsetIdx]._immobileProperty;
    }

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
        if(_hcManager._perPlayerConfigs.Any(x => x._forcedFollow)
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
    private void HandleMovementPrevention(bool following, bool sitting, bool immobile) {
        if(sitting) {
            _MoveController.CompletelyDisableMovement(true, true, false); // set pointer and turn off mouse and disable emotes
        }
        else if(immobile) {
            _MoveController.CompletelyDisableMovement(true, false, true); // set pointer but dont turn off mouse
        }
        // otherwise if we are forced to follow
        else if(following) {
            // in this case, we want to maske sure to block players keys and force them to legacy mode.
            if(GameConfig.UiControl.GetBool("MoveMode") == false) {
                GameConfig.UiControl.Set("MoveMode", (int)MovementMode.Legacy);
            }
            // dont set pointer, but disable mouse
            _MoveController.CompletelyDisableMovement(false, true, true); // disable mouse and emotes
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
                }
            });
        }
    }

    // set the key state
    private static void SetKeyState(VirtualKey key, int state) => getRefValue((int)key) = state;
#endregion Framework Updates
}