using GagSpeak.Events;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GagSpeak.Wardrobe;
using GagSpeak.Services;

namespace GagSpeak.Hardcore.Actions;
public unsafe class GsActionManager : IDisposable
{
#region ClassIncludes
    private readonly IClientState _clientState;
    private readonly IFramework _framework;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly IDataManager _dataManager;
    private readonly GagSpeakConfig _config;
    private readonly HardcoreManager _hcManager;
    private readonly HotbarLocker _hotbarLocker;
    private readonly RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private readonly RS_ToggleEvent _setToggleEvent;
    private readonly OnFrameworkService _onFrameworkService;
    private readonly GagSpeakGlamourEvent _glamourEvent;
    private readonly InitializationManager _manager;
    // for direct access inspection

    public Control* gameControl = Control.Instance(); // instance to have control over our walking
    // attempt to get the rapture hotbar module so we can modify the display of hotbar items
    public RaptureHotbarModule* raptureHotarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();
    // hook creation for the action manager
    internal delegate bool UseActionDelegate(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
    internal Hook<UseActionDelegate> UseActionHook;
#endregion ClassIncludes
#region Attributes
    public Dictionary<uint, AcReqProps[]> CurrentJobBannedActions = new Dictionary<uint, AcReqProps[]>(); // stores the current job actions
    public Dictionary<int, Tuple<float, DateTime>> CooldownList = new Dictionary<int, Tuple<float, DateTime>>(); // stores the recast timers for each action

#endregion Attributes
    public unsafe GsActionManager(IClientState clientState, IFramework framework, IGameInteropProvider interop,
    HardcoreManager hardcoreManager, RS_PropertyChangedEvent RS_PropertyChangedEvent,
    IDataManager dataManager, RS_ToggleEvent setToggleEvent, GagSpeakConfig config, HotbarLocker hotbarLocker,
    InitializationManager manager, GagSpeakGlamourEvent glamourEvent, OnFrameworkService onFrameworkService)
    {
        _clientState = clientState;
        _framework = framework;
        _gameInteropProvider = interop;
        _hcManager = hardcoreManager;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;
        _dataManager = dataManager;
        _setToggleEvent = setToggleEvent;
        _onFrameworkService = onFrameworkService;
        _config = config;
        _hotbarLocker = hotbarLocker;
        _manager = manager;
        _glamourEvent = glamourEvent;
        // set up a hook to fire every time the address signature is detected in our game.
        UseActionHook = _gameInteropProvider.HookFromAddress<UseActionDelegate>((nint)ActionManager.Addresses.UseAction.Value, UseActionDetour);
        UseActionHook.Enable();

        _setToggleEvent.SetToggled += OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged += OnRestraintSetPropertyChanged;
        _manager.MovementManagerInitialized += OnMovementManagerInitialized;
        // set that we are ready to true
        _manager._actionManagerReadyForEvent.SetResult(true);
    }

    public void Dispose() {
        // unsub from events and stuff
        _manager.MovementManagerInitialized -= OnMovementManagerInitialized;
        _setToggleEvent.SetToggled -= OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;
        _framework.Update -= framework_Update;
        // set lock to visable again
        _hotbarLocker.SetHotbarLockState(false);
        // dispose of the hook
        if (UseActionHook != null) {
            if (UseActionHook.IsEnabled) {
                UseActionHook.Disable();
            }
            UseActionHook.Dispose();
            UseActionHook = null!;
        }
    }

    private void EnableProperties(int setIndex, int idxOfAssigner) {
        // if the index isnt out of range, then apply the settings
        if(setIndex != -1) {
            // apply stimulation modifier, if any
            _hcManager.ApplyMultipler();
            // activate hotbar lock, if we have any properties enabled
            if(_config.hardcoreMode && _hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[setIndex].AnyPropertyTrue()) {
                _hotbarLocker.SetHotbarLockState(true);
            }
        }
    }

    private void DisableProperties() {
        // reset multiplier
        _hcManager.StimulationMultipler = 1.0;
        // we should also restore hotbar slots
        RestoreSavedSlots();
        // we should also unlock hotbar lock
        _hotbarLocker.SetHotbarLockState(false);
    }
#region SlotManagment
    public void RestoreSavedSlots() {
        if(_clientState.LocalPlayer != null && _clientState.LocalPlayer.ClassJob != null && raptureHotarModule != null) {
            GSLogger.LogType.Debug($"[Action Manager]: Restoring saved slots");
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
            GSLogger.LogType.Debug($"[Action Manager]: Player is null, returning");
        }
    }

    // fired on framework tick while a set is active
    private void UpdateSlots(int idxOfAssignerOfSet, int idxOfSet) {
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
                        if(_hcManager._perPlayerConfigs[idxOfAssignerOfSet]._rsProperties[idxOfSet]._gaggedProperty && props.Contains(AcReqProps.Speech)) {
                            // speech should be restrained, so remove any actions requireing speech
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 2886);
                            continue;
                        }
                        if(_hcManager._perPlayerConfigs[idxOfAssignerOfSet]._rsProperties[idxOfSet]._blindfoldedProperty && props.Contains(AcReqProps.Sight)) {
                            // sight should be restrained, so remove any actions requireing sight
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 99);
                            continue;
                        }
                        if(_hcManager._perPlayerConfigs[idxOfAssignerOfSet]._rsProperties[idxOfSet]._weightyProperty && props.Contains(AcReqProps.Weighted)) {
                            // weighted should be restrained, so remove any actions requireing weight
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 151);
                            continue;
                        }
                        if(_hcManager._perPlayerConfigs[idxOfAssignerOfSet]._rsProperties[idxOfSet]._immobileProperty && props.Contains(AcReqProps.Movement)) {
                            // immobile should be restrained, so remove any actions requireing movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 2883);
                            continue;
                        }
                        if(_hcManager._perPlayerConfigs[idxOfAssignerOfSet]._rsProperties[idxOfSet]._legsRestraintedProperty && props.Contains(AcReqProps.LegMovement)) {
                            // legs should be restrained, so remove any actions requireing leg movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 55);
                            continue;
                        }
                        if(_hcManager._perPlayerConfigs[idxOfAssignerOfSet]._rsProperties[idxOfSet]._armsRestraintedProperty && props.Contains(AcReqProps.ArmMovement)) {
                            // arms should be restrained, so remove any actions requireing arm movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 68);
                            continue;
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
            GSLogger.LogType.Debug($"[Action Manager]: Updating job list");
            ActionData.GetJobActionProperties((JobType)_clientState.LocalPlayer.ClassJob.Id, out var bannedJobActions);
            CurrentJobBannedActions = bannedJobActions; // updated our job list
            // only do this if we are logged in
            if (_clientState.IsLoggedIn 
            &&  _clientState.LocalPlayer != null
            && _clientState.LocalPlayer.Address != IntPtr.Zero
            && raptureHotarModule->StandardHotBars != null) {
                // if we are logged in, we should also restore our hotbar slots to the saved state over the live state
                GenerateCooldowns();
            }
        } else {
            GSLogger.LogType.Debug($"[Action Manager]: Player is null, returning");
        }
    }

    private void GenerateCooldowns() {
        // if our current dictionary is not empty, empty it
        if(CooldownList.Count > 0) {
            CooldownList.Clear();
        }
        // get the current job actions
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
                    if (slot->CommandType == HotbarSlotType.Action) {
                        // we will want to add the tuple for each slot, the tuple should contain the cooldown group
                        var adjustedId = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetAdjustedActionId(slot->CommandId);
                        // get the cooldown group
                        var cooldownGroup = -1;
                        var action = _dataManager.Excel.GetSheet<Lumina.Excel.GeneratedSheets.Action>()!.GetRow(adjustedId);
                        if (action == null) { break; }
                        // there is a minus one offset for actions, while general actions do not have them.
                        cooldownGroup = action.CooldownGroup -1;
                        // get recast time
                        var recastTime = ActionManager.GetAdjustedRecastTime(ActionType.Action, adjustedId);
                        recastTime = (int)(recastTime * _hcManager.StimulationMultipler);
                        // if it is an action or general action, append it
                        GSLogger.LogType.Verbose($"[Action Manager]: SlotID {slot->CommandId} Cooldown group {cooldownGroup} with recast time {recastTime}");
                        if (!CooldownList.ContainsKey(cooldownGroup)) {
                            CooldownList.Add(cooldownGroup, new Tuple<float, DateTime>(recastTime, DateTime.MinValue));
                        }
                    }
                }
            }                   
        }
    }

#endregion SlotManagment
#region EventHandlers
    private void OnMovementManagerInitialized() {
        GSLogger.LogType.Information(" Completing Action Manager Initialization ");
        // if we are ready to initialize the actions, we should update our job list
        UpdateJobList();
        // see if we should enable the sets incase we load this prior to the restraint set manager loading.
        if(_hcManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int idxOfAssigner)) {
            GSLogger.LogType.Debug($"[Action Manager]: Restraint set index {enabledIdx}, activated by {assignerOfSet}, is now active");            
            EnableProperties(enabledIdx, idxOfAssigner);
        } 
        else {
            GSLogger.LogType.Debug($"[Action Manager]: No restraint sets are active");
        }
        _framework.Update += framework_Update;
        // invoke the actionManagerFinished method
        _manager.CompleteStep(InitializationSteps.ActionManagerInitialized);
    }

    private void OnRestraintSetToggled(object sender, RS_ToggleEventArgs e) {
        // when restraint set is active, change the actively enabled restraint set index so that
        // we know which set is active when we try applying affects to the hotbar
        if(e.ToggleType == RestraintSetToggleType.Enabled) {
            // dont need to do this but it gives us the variables without us needing to use characterHandler, i know its messy
            if(_hcManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int idxOfAssigner)) {
                EnableProperties(e.SetIndex, idxOfAssigner);
                GSLogger.LogType.Debug($"[Action Manager]: Restraint set index {e.SetIndex} is now active");
            }
        }
        // if we are disabling the restraint set, we should restore our hotbar slots to the saved state over the live state
        if(e.ToggleType == RestraintSetToggleType.Disabled) {
            DisableProperties();
            GSLogger.LogType.Debug($"[Action Manager]: Restraint set index {e.SetIndex} is now disabled");
        }
    }

    private void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // refresh hotbars whenever property is turned off
        switch(e.PropertyType) {
            // cases that cause us to update our saved slots
            case HardcoreChangeType.LegsRestraint:
            case HardcoreChangeType.ArmsRestraint:
            case HardcoreChangeType.Gagged:
            case HardcoreChangeType.Blindfolded:
            case HardcoreChangeType.Immobile:
            case HardcoreChangeType.Weighty:
            {
                RestoreSavedSlots();
                break;
            }
            // cases that cause us to update our stored recast timers
            case HardcoreChangeType.LightStimulation:
            case HardcoreChangeType.MildStimulation:
            case HardcoreChangeType.HeavyStimulation:
            {
                GenerateCooldowns();
                break; // woot
            }
        }
    }
#endregion EventHandlers
#region Framework Updates
    private void framework_Update(IFramework framework) => FrameworkOnUpdateInternal();
    private unsafe void FrameworkOnUpdateInternal() {
        // make sure we only do checks when we are properly logged in and have a character loaded
        if (_clientState.LocalPlayer?.IsDead ?? false) {
            return;
        }

        if (AllowFrameworkHardcoreUpdates()) {
            // if the class job is different than the one stored, then we have a class job change (CRITICAL TO UPDATING PROPERLY)
            if (_clientState.LocalPlayer!.ClassJob.Id != _onFrameworkService._classJobId) {
                // update the stored class job
                _onFrameworkService._classJobId = _clientState.LocalPlayer.ClassJob.Id;
                // invoke jobChangedEvent to call the job changed glamour event
                _glamourEvent.Invoke(UpdateType.JobChange);
                // regenerate our slots
                UpdateJobList();
                RestoreSavedSlots();
                return;
            }

            // see if any set is enabled
            if(_hcManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int idxOfAssigner)) {
                // otherwise, check to see if any property of that enabled set is true for the person who assigned it
                if(_hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[enabledIdx].AnyPropertyTrue())
                    UpdateSlots(idxOfAssigner, enabledIdx);
            }
        }
    }
#endregion Framework Updates
    private bool UseActionDetour(ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8) {
        try {
            GSLogger.LogType.Verbose($"[Action Manager]: UseActionDetour called {acId} {type}");

            // if we are allowing hardcore updates / in hardcore mode
            if (AllowFrameworkHardcoreUpdates())
            {
                // check to see if the action is a teleport / return action, and if so to cancel it if the player is
                if(_hcManager.IsForcedToStayForAny(out int forcedFollowAssignerIdx, out string forcedFollowAssigner)) {
                    // check if we are trying to hit teleport or return from hotbars /  menus
                    if(type == ActionType.GeneralAction && (acId == 7 || acId == 8)) {
                        GSLogger.LogType.Verbose($"[Action Manager]: You are currently locked away, canceling teleport/return execution");
                        return false;
                    }
                    // if we somehow managed to start executing it, then stop that too
                    if(type == ActionType.Action && (acId == 5 || acId == 6 || acId == 11408)) {
                        GSLogger.LogType.Verbose($"[Action Manager]: You are currently locked away, canceling teleport/return execution");
                        return false;
                    }

                    // if it is self destrcut, 
                }

                // check to see if any our of sets is currently active 
                if(_hcManager.IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int idxOfAssigner)) {
                    // because they are, lets see if the light, mild, or heavy stimulation is active
                    if(_hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[enabledIdx]._lightStimulationProperty
                    || _hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[enabledIdx]._mildStimulationProperty
                    || _hcManager._perPlayerConfigs[idxOfAssigner]._rsProperties[enabledIdx]._heavyStimulationProperty) {
                        // then let's check our action ID's to apply the modified cooldown timers
                        if (ActionType.Action == type && acId > 7) {
                            // Debug current metrics of action
                            var recastTime = ActionManager.GetAdjustedRecastTime(type, acId);
                            var adjustedId = am->GetAdjustedActionId(acId);
                            var recastGroup = am->GetRecastGroup((int)type, adjustedId);
                            if (CooldownList.ContainsKey(recastGroup)) {
                                // GSLogger.LogType.Debug($"[Action Manager]: GROUP FOUND - Recast Time: {recastTime} | Cast Group: {recastGroup}");
                                var cooldownData = CooldownList[recastGroup];
                                // if we are beyond our recast time from the last time used, allow the execution
                                if (DateTime.Now >= cooldownData.Item2.AddMilliseconds(cooldownData.Item1)) {
                                    // Update the last execution time before execution
                                    GSLogger.LogType.Verbose($"[Action Manager]: ACTION COOLDOWN FINISHED");
                                    CooldownList[recastGroup] = new Tuple<float, DateTime>(cooldownData.Item1, DateTime.Now);
                                } else {
                                    GSLogger.LogType.Verbose($"[Action Manager]: ACTION COOLDOWN NOT FINISHED");
                                    return false; // Do not execute the action
                                }
                            } else {
                                GSLogger.LogType.Debug($"[Action Manager]: GROUP NOT FOUND");
                            }
                        }
                    }
                }
            }
        } catch (Exception e) {
            GSLogger.LogType.Error(e.ToString());
        }
        // return the original if we reach here
        var ret = UseActionHook.Original(am, type, acId, target, a5, a6, a7, a8);
        // invoke the action used event
        return ret;
    }

    private bool AllowFrameworkHardcoreUpdates() {
        return (
           _clientState.IsLoggedIn                          // we must be logged in
        && _clientState.LocalPlayer != null                 // our character must not be null
        && _clientState.LocalPlayer.Address != IntPtr.Zero  // our address must be valid
        && _config.hardcoreMode                             // we are in hardcore mode
        );                                                  // we must have an active set enabled.
    }
}