using GagSpeak.CharacterData;
using GagSpeak.Events;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GagSpeak.Wardrobe;
using Penumbra.GameData.Structs;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using GagSpeak.Services;
using GagSpeak.Gagsandlocks;

namespace GagSpeak.Hardcore.Actions;
public unsafe class GsActionManager : IDisposable
{
#region ClassIncludes
    private readonly IClientState _clientState;
    private readonly IFramework _framework;
    private readonly IKeyState _keyState;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly ICondition _condition;
    private readonly IObjectTable _objectTable;
    private readonly IDataManager _dataManager;
    private readonly GagSpeakConfig _config;
    private readonly HardcoreManager _hardcoreManager;
    private readonly RestraintSetManager _restraintSetManager;
    private readonly GagSpeakGlamourEvent _glamourEvent;
    private readonly RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private readonly RS_ToggleEvent _setToggleEvent;
    // for direct access inspection
    public FFXIVClientStructs.FFXIV.Client.Game.Control.Control* gameControl = FFXIVClientStructs.FFXIV.Client.Game.Control.Control.Instance(); // instance to have control over our walking
    // attempt to get the rapture hotbar module so we can modify the display of hotbar items
    public FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule* raptureHotarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();
    // for getting virtual key code access
    delegate ref int GetRefValue(int vkCode); // virtual key code
    static GetRefValue getRefValue; // virtual key value

    // hook creation for the action manager
    internal delegate bool UseActionDelegate(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
    internal Hook<UseActionDelegate> UseActionHook;
#endregion ClassIncludes
#region Attributes
    // any attributes here is needed
#endregion Attributes
    public unsafe GsActionManager(IClientState clientState, IFramework framework, GagSpeakGlamourEvent glamourEvent,
    IKeyState keyState, IGameInteropProvider interop, ICondition condition, IObjectTable gameObjects,
    RestraintSetManager restraintSetManager, HardcoreManager hardcoreManager, RS_PropertyChangedEvent RS_PropertyChangedEvent,
    IDataManager dataManager, RS_ToggleEvent setToggleEvent, GagSpeakConfig config)
    {
        // set our attributes
        _config = config;
        _hardcoreManager = hardcoreManager;
        _clientState = clientState;
        _glamourEvent = glamourEvent;
        _framework = framework;
        _keyState = keyState;
        _gameInteropProvider = interop;
        _dataManager = dataManager;
        _restraintSetManager = restraintSetManager;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;
        _setToggleEvent = setToggleEvent;
        // subscribe
        _glamourEvent.GlamourEventFired += JobChangeEventFired;
        _setToggleEvent.SetToggled += OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged += OnRestraintSetPropertyChanged;
        _framework.Update += framework_Update;

        // set up a hook to fire every time the address signature is detected in our game.

        UseActionHook = _gameInteropProvider.HookFromAddress<UseActionDelegate>((nint)FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Addresses.UseAction.Value, UseActionDetour);
        UseActionHook.Enable();

        getRefValue = (GetRefValue)Delegate.CreateDelegate(typeof(GetRefValue), _keyState,
                    _keyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null));
        _condition = condition;
        _objectTable = gameObjects;
        
        GagSpeak.Log.Debug($"[Action Manager] Rapture HotbarModule:    x {(ulong)raptureHotarModule:X})");
        GagSpeak.Log.Debug($"[Action Manager] ActionManager Game Ctrl: x {(ulong)gameControl:X})");
    }

    public void Dispose()
    {
        // unsub from events and stuff
        _setToggleEvent.SetToggled -= OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;
        _framework.Update -= framework_Update;
        // dispose of the hook
        UseActionHook.Disable();
        UseActionHook.Dispose();
    }

    // this doesnt account for controller hotbars, but it should be a good start for now.
    public (uint CommandId, HotbarSlotType CommandType)[] hotbarSkills = new (uint, HotbarSlotType)[10*12];

#region SlotManagment
    public void RestoreSavedSlots() {
        if(_clientState.LocalPlayer != null && _clientState.LocalPlayer.ClassJob != null && raptureHotarModule != null) {
            GagSpeak.Log.Debug($"[Action Manager]: Restoring saved slots");
            // restore all hotbar slots (DOES NOT WORK ON CROSS HOTBARS FROM WHAT I KNOW, USE AT OWN RISK)
            var baseSpan = raptureHotarModule->StandardHotBars; // the length of our hotbar count
            for(int i=0; i < baseSpan.Length; i++) {
                // get our hotbar row
                var hotbar = baseSpan.GetPointer(i);
                // if the hotbar is not null, we can get the slots data
                if (hotbar != null) {
                    // get the slots data...
                    raptureHotarModule->LoadSavedHotbar(_clientState.LocalPlayer.ClassJob.Id, (uint)i);
                }
            }
        } else {
            GagSpeak.Log.Debug($"[Action Manager]: Player is null, returning");
        }
    }

    private void UpdateSlots() {
        // before we get the hotbar rapture, let's 
        var baseSpan = raptureHotarModule->StandardHotBars; // the length of our hotbar count
        for(var i=0; i < baseSpan.Length; i++) {
            // get our hotbar row
            var hotbar = baseSpan.GetPointer(i);
            // if the hotbar is not null, we can get the slots data
            if (hotbar != null) {
                // get the slots data...
                for (var j = 0; j < 16; j++) {
                    var slot = hotbar->SlotsSpan.GetPointer(j);
                    if (slot == null) break;
                    // if the slot is not empty, get the command id
                    bool isAction = slot->CommandType == HotbarSlotType.Action || slot->CommandType == HotbarSlotType.GeneralAction;
                    // if it is a valid action, scan to see if the commandID is equyal to any of our banned actions
                    if (isAction && CurrentJobBannedActions.TryGetValue(slot->CommandId, out var props)) {
                        // see if any of the indexes in the array contain a AcReqPros
                        if(_hardcoreManager._rsProperties[ActivelyEnabledRestraintSetIdx]._legsRestraintedProperty
                        && props.Contains(AcReqProps.LegMovement)) {
                            // legs should be restrained, so remove any actions requireing leg movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 25788);
                        }
                        if(_hardcoreManager._rsProperties[ActivelyEnabledRestraintSetIdx]._armsRestraintedProperty
                        && props.Contains(AcReqProps.ArmMovement)) {
                            // arms should be restrained, so remove any actions requireing arm movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 25788);
                        }
                        if(_hardcoreManager._rsProperties[ActivelyEnabledRestraintSetIdx]._gaggedProperty
                        && props.Contains(AcReqProps.Speech)) {
                            // speech should be restrained, so remove any actions requireing speech
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 25788);
                        }
                        if(_hardcoreManager._rsProperties[ActivelyEnabledRestraintSetIdx]._blindfoldedProperty
                        && props.Contains(AcReqProps.Sight)) {
                            // sight should be restrained, so remove any actions requireing sight
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 25788);
                        }
                        if(_hardcoreManager._rsProperties[ActivelyEnabledRestraintSetIdx]._immobileProperty
                        && props.Contains(AcReqProps.Movement)) {
                            // immobile should be restrained, so remove any actions requireing movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 25788);
                        }
                    }
                }
            }
        }
    }

    // for updating our stored job list dictionary
    private void UpdateJobList() {
        // this will be called by the job changed event. When it does, we will update our job list with the new job.
        if(_clientState.LocalPlayer != null && _clientState.LocalPlayer.ClassJob != null) {
            GagSpeak.Log.Debug($"[Action Manager]: Updating job list");
            ActionData.GetJobActionProperties((JobType)_clientState.LocalPlayer.ClassJob.Id, out var bannedJobActions);
            CurrentJobBannedActions = bannedJobActions; // updated our job list

            // we should also update our indexes here
            // search through the restraint set manager, and see if the current restraint set is enabled, if they are, set that as the active index
            if(_restraintSetManager._restraintSets.Any(x => x._enabled)) {
                ActivelyEnabledRestraintSetIdx = _restraintSetManager._restraintSets.FindIndex(x => x._enabled);
            }
            // the index is now set..
        }
    }

    // the job listing with our respective action information to scan over the the updateSlots function
    public Dictionary<uint, AcReqProps[]> CurrentJobBannedActions = new Dictionary<uint, AcReqProps[]>(); // will be updated by the job change event
    public int ActivelyEnabledRestraintSetIdx = -1; // will be set to the index of whichever set get's enabled


#endregion SlotManagment
#region EventHandlers
    private void JobChangeEventFired(object sender, GagSpeakGlamourEventArgs e) {
        if(e.UpdateType == UpdateType.JobChange) {
            // update our job list
            UpdateJobList();
        }
    }

    private void OnRestraintSetToggled(object sender, RS_ToggleEventArgs e) {
        // we should see if the set is enabled or disabled
        if(e.ToggleType == RestraintSetToggleType.Enabled) {
            // if it is enabled, we should check if the assigner is in the whitelist
            ActivelyEnabledRestraintSetIdx = e.SetIndex;
            GagSpeak.Log.Debug($"[Action Manager]: Restraint set index {e.SetIndex} is now active");
        }
        // if the set is disabled, we need to restore our slots to their original state
        if(e.ToggleType == RestraintSetToggleType.Disabled) {
            // if the set is disabled, we need to restore our slots to their original state
            ActivelyEnabledRestraintSetIdx = -1;
            RestoreSavedSlots();
            GagSpeak.Log.Debug($"[Action Manager]: Restraint set index {e.SetIndex} is now disabled");
        }
    }

    private void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // if we have just turned off our force walk, allow us to move again
        if (e.PropertyType == HardcoreChangeType.ForcedWalk && e.ChangeType == RestraintSetChangeType.Disabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Letting you run again");
            System.Threading.Tasks.Task.Delay(200);
            Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
        }
        // if a restraint set property updates, do a refresh on our saveslots before updating them again
        if(e.PropertyType == HardcoreChangeType.RS_PropertyModified) {
            RestoreSavedSlots();
        }

        // if we are not being forced to walk, we need to go through all our action slots and store them to our memory
        if (e.PropertyType == HardcoreChangeType.ForcedWalk && e.ChangeType == RestraintSetChangeType.Enabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Forcing you to walk");
        }

        // forced follow enable
        if (e.PropertyType == HardcoreChangeType.ForcedFollow && e.ChangeType == RestraintSetChangeType.Enabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Forcing you to follow");
        }

        // if we are no longer being forced to follow, we need to restore our slots to their original state
        if (e.PropertyType == HardcoreChangeType.ForcedFollow && e.ChangeType == RestraintSetChangeType.Disabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Letting you run again");
            // restore all hotbar slots
            RestoreSavedSlots();
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
        && _config.AdminMode) {

            // create our current job list if we are scanning for the first time
            if(CurrentJobBannedActions.Count == 0) {
                UpdateJobList();
            }

            if(_hardcoreManager._forcedWalk) {
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
            // update our slots with our respective implied restrictions when forcedwalk is enabeled
            if(_hardcoreManager._forcedFollow) {
                
            }

            // obtain our current restraint set active index and 
            if(ActivelyEnabledRestraintSetIdx != -1 && _restraintSetManager._restraintSets[ActivelyEnabledRestraintSetIdx]._enabled) {
                UpdateSlots();
            }
            //FFXIVClientStructs.FFXIV.Client.UI.AddonSelectYesno

            /*var skip = false;
            if (!skip && _clientState != null && _clientState.LocalPlayer != null && _clientState.LocalPlayer.IsCasting)
            {
                //_keyState.SetRawValue(Dalamud.Game.ClientState.Keys.VirtualKey.ESCAPE, 1);
                //_keyState.SetRawValue(Dalamud.Game.ClientState.Keys.VirtualKey.ESCAPE, 0);
                var raw = _keyState.GetRawValue(VirtualKey.ESCAPE);
                getRefValue((int)VirtualKey.SPACE) = 3;
                skip = true;
                //getRefValue((int)VirtualKey.ESCAPE) = raw;
                _log.Debug($"[Action Manager]: aa");
                //_log.Debug($"[Action Manager]: {_clientState.LocalPlayer.CastActionId} {_clientState.LocalPlayer.CastActionType} {_clientState.LocalPlayer.CastTargetObjectId} {_clientState.LocalPlayer.ObjectId}");

            }else if (skip && !_clientState.LocalPlayer.IsCasting) {
                skip = false;
            }*/
        }
    }
#endregion Framework Updates
    private bool UseActionDetour(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8)
    {
        try
        {
            GagSpeak.Log.Debug($"[Action Manager]: {type} {acId} {target} {a5} {a6} {a7}");
            if (ActionType.Action == type && acId > 7) //!Abilities.general.ContainsKey(acId))
            {
                if (_clientState != null
                    && _clientState.LocalPlayer != null
                    && _clientState.LocalPlayer.ClassJob != null
                    && _clientState.LocalPlayer.ClassJob.GameData != null)
                {
                    var role = _clientState.LocalPlayer.ClassJob.GameData.Role;
                    // log the role
                    GagSpeak.Log.Debug($"[Action Manager]: Role: {role}");
                    // if (_config.BannedActionRoles.Contains((ActionRoles)role)) {
                    //     return false;
                    // }

                    // if (_config.canSelfCast && (ActionRoles)role == ActionRoles.Healer)
                    // {
                    //     //_log.Debug($"[Action Manager]: {_objectTable.FirstOrDefault(x => x.ObjectId.Equals(target))?.ObjectKind}");
                    //     if (_clientState.LocalPlayer.ObjectId == target /*|| _objectTable.FirstOrDefault(x => x.ObjectId.Equals((uint)target))?.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player*/)
                    //     {
                    //         return false;
                    //     }
                    // }
                    // log the action used
                    GagSpeak.Log.Debug($"[Action Manager]: {acId}");


                    // if (_config.AbilityRestrictionLevel > 0)
                    // {                            
                    //     switch ((ActionRoles)role)
                    //     {
                    //         case ActionRoles.NonCombat:
                    //             break;
                    //         case ActionRoles.Tank:
                    //             break;
                    //         case ActionRoles.MeleeDps:
                    //             break;
                    //         case ActionRoles.RangedDps:
                    //             break;
                    //         case ActionRoles.Healer:
                    //             switch (_config.AbilityRestrictionLevel)
                    //             {
                    //                 case AbilityRestrictionLevel.Hardcore:
                    //                     if (!Abilities.SpellsHardcore.Any(x => acId == x))
                    //                     {
                    //                         return false;
                    //                     }
                    //                     break;
                    //                 case AbilityRestrictionLevel.Minimal:
                    //                     if (!Abilities.SpellsMinimal.Any(x => acId == x))
                    //                     {
                    //                         return false;
                    //                     }
                    //                     break;
                    //                 case AbilityRestrictionLevel.Advanced:
                    //                     if (!Abilities.SpellsAdvanced.Any(x => acId == x))
                    //                     {
                    //                         return false;
                    //                     }
                    //                     break;
                    //                 case AbilityRestrictionLevel.Spec:
                    //                     if (!Abilities.SpellsSpec.Any(x => acId == x))
                    //                     {
                    //                         return false;
                    //                     }
                    //                     break;
                    //             }
                    //             break;
                    //     }
                    // }
                }
            }
        } catch (Exception e) {
            GagSpeak.Log.Error(e.ToString());
        }
        // return the original if we reach here
        var ret = UseActionHook.Original(am, type, acId, target, a5, a6, a7, a8);
        return ret;
    }
}