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
using GagSpeak.Hardcore.Movement;
using ImGuiNET;
using FFXIVClientStructs.Interop.Attributes;

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
    private readonly HotbarLocker _hotbarLocker;
    private readonly GagSpeakGlamourEvent _glamourEvent;
    private readonly RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private readonly RS_ToggleEvent _setToggleEvent;
    // for direct access inspection
    public FFXIVClientStructs.FFXIV.Client.Game.Control.Control* gameControl = FFXIVClientStructs.FFXIV.Client.Game.Control.Control.Instance(); // instance to have control over our walking
    // attempt to get the rapture hotbar module so we can modify the display of hotbar items
    public FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule* raptureHotarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();
    // hook creation for the action manager
    internal delegate bool UseActionDelegate(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
    internal Hook<UseActionDelegate> UseActionHook;
#endregion ClassIncludes
#region Attributes
    public Dictionary<uint, AcReqProps[]> CurrentJobBannedActions; // will be updated by the job change event, also used to cancel actions
    private Dictionary<int, Tuple<float, DateTime>> CooldownList;
    private double multiplier;
    public bool AnyPropertiesEnabled;
#endregion Attributes
    public unsafe GsActionManager(IClientState clientState, IFramework framework, GagSpeakGlamourEvent glamourEvent,
    IKeyState keyState, IGameInteropProvider interop, ICondition condition, IObjectTable gameObjects,
    RestraintSetManager restraintSetManager, HardcoreManager hardcoreManager, RS_PropertyChangedEvent RS_PropertyChangedEvent,
    IDataManager dataManager, RS_ToggleEvent setToggleEvent, GagSpeakConfig config, HotbarLocker hotbarLocker)
    {
        // set our attributes
        _config = config;
        _hardcoreManager = hardcoreManager;
        _clientState = clientState;
        _glamourEvent = glamourEvent;
        _framework = framework;
        _keyState = keyState;
        _hotbarLocker = hotbarLocker;
        _gameInteropProvider = interop;
        _dataManager = dataManager;
        _condition = condition;
        _objectTable = gameObjects;
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

        // set the attributes
        CurrentJobBannedActions = new Dictionary<uint, AcReqProps[]>();
        CooldownList = new Dictionary<int, Tuple<float, DateTime>>();
        multiplier = 1.0;
        AnyPropertiesEnabled = false;

        // see if we should enable the sets incase we load this prior to the restraint set manager loading.
        if(_restraintSetManager._restraintSets.Any(x => x._enabled)){
            // if any of our sets are enabled, we should call our function to start enabling the properties
            // get the index of the enabled set
            var index = _restraintSetManager._restraintSets.FindIndex(x => x._enabled);
            EnableProperties(index);
        }

    }

    public void Dispose() {
        // unsub from events and stuff
        _glamourEvent.GlamourEventFired -= JobChangeEventFired;
        _setToggleEvent.SetToggled -= OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;
        _framework.Update -= framework_Update;
        // dispose of the hook
        if (UseActionHook != null) {
            if (UseActionHook.IsEnabled) {
                UseActionHook.Disable();
            }
            UseActionHook.Dispose();
            UseActionHook = null;
        }
    }
#region Properties
    private void EnableProperties(int index) {
        _hardcoreManager.ActiveSetIdxEnabled = index;
        // set properties to true
        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled].AnyPropertyTrue()) {
            AnyPropertiesEnabled = true;
            // apply stimulation modifier
            _hardcoreManager.ApplyMultipler();
            // activate hotbar lock
            _hotbarLocker.SetHotbarLockState(true);
        }
    }

    private void DisableProperties() {
        // set the active set to -1
        _hardcoreManager.ActiveSetIdxEnabled = -1;
        // reset multiplier
        _hardcoreManager.StimulationMultipler = 1.0;
        // if the set is disabled, regardless of if we have any properties or not enabled the set is inactive.
        // because of this we should set the AnyPropertiesEnabled to false
        AnyPropertiesEnabled = false;
        // we should also restore hotbar slots
        RestoreSavedSlots();
        // we should also unlock hotbar lock
        _hotbarLocker.SetHotbarLockState(false);
    }
#endregion Properties

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

    // fired on framework tick while a set is active
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
                        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._gaggedProperty
                        && props.Contains(AcReqProps.Speech)) {
                            // speech should be restrained, so remove any actions requireing speech
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 2886);
                            continue;
                        }
                        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._blindfoldedProperty
                        && props.Contains(AcReqProps.Sight)) {
                            // sight should be restrained, so remove any actions requireing sight
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 99);
                            continue;
                        }
                        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._weightyProperty
                        && props.Contains(AcReqProps.Weighted)) {
                            // weighted should be restrained, so remove any actions requireing weight
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 151);
                            continue;
                        }
                        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._immobileProperty
                        && props.Contains(AcReqProps.Movement)) {
                            // immobile should be restrained, so remove any actions requireing movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 2883);
                            continue;
                        }
                        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._legsRestraintedProperty
                        && props.Contains(AcReqProps.LegMovement)) {
                            // legs should be restrained, so remove any actions requireing leg movement
                            slot->Set(raptureHotarModule->UiModule, HotbarSlotType.Action, 55);
                            continue;
                        }
                        if(_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._armsRestraintedProperty
                        && props.Contains(AcReqProps.ArmMovement)) {
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
            GagSpeak.Log.Debug($"[Action Manager]: Updating job list");
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
            GagSpeak.Log.Debug($"[Action Manager]: Player is null, returning");
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
                        recastTime = (int)(recastTime * _hardcoreManager.StimulationMultipler);
                        // if it is an action or general action, append it
                        GagSpeak.Log.Verbose($"[Action Manager]: SlotID {slot->CommandId} Cooldown group {cooldownGroup} with recast time {recastTime}");
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
    private void JobChangeEventFired(object sender, GagSpeakGlamourEventArgs e) {
        // update our stored job list so we know which job actions are banned for our current class
        if(e.UpdateType == UpdateType.JobChange) {
            UpdateJobList();
        }
    }
    private void OnRestraintSetToggled(object sender, RS_ToggleEventArgs e) {
        // when restraint set is active, change the actively enabled restraint set index so that
        // we know which set is active when we try applying affects to the hotbar
        if(e.ToggleType == RestraintSetToggleType.Enabled) {
            EnableProperties(e.SetIndex);
            GagSpeak.Log.Debug($"[Action Manager]: Restraint set index {e.SetIndex} is now active");
        }
        // if we are disabling the restraint set, we should restore our hotbar slots to the saved state over the live state
        if(e.ToggleType == RestraintSetToggleType.Disabled) {
            DisableProperties();
            GagSpeak.Log.Debug($"[Action Manager]: Restraint set index {e.SetIndex} is now disabled");
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
                break;
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
        if (_clientState.IsLoggedIn 
        &&  _clientState.LocalPlayer != null
        && _clientState.LocalPlayer.Address != IntPtr.Zero
        && CurrentJobBannedActions != null
        && _config.AdminMode)
        {
            // create our current job list if we are scanning for the first time (This is just so we dont crash and get a million errors)
            if(CurrentJobBannedActions.Count == 0) { UpdateJobList(); }

            // obtain our current restraint set active index and 
            if(_hardcoreManager.ActiveSetIdxEnabled != -1 && AnyPropertiesEnabled) {
                // update our slots with our respective implied restrictions when forcedwalk is enabeled
                UpdateSlots();
            }
        }
    }
#endregion Framework Updates
    private bool UseActionDetour(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8) {
        try
        {
            if (_clientState.IsLoggedIn &&  _clientState.LocalPlayer != null && _clientState.LocalPlayer.Address != IntPtr.Zero
            && CurrentJobBannedActions != null && _config.AdminMode)
            {
                // if we are in hardcore mode, and we have an active set enabled, and we have any property enabled
                if(_hardcoreManager.ActiveSetIdxEnabled != -1 &&
                (_hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._lightStimulationProperty
                || _hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._mildStimulationProperty
                || _hardcoreManager._rsProperties[_hardcoreManager.ActiveSetIdxEnabled]._heavyStimulationProperty))
                {
                    if (ActionType.Action == type && acId > 7) {
                        // Debug current metrics of action
                        var recastTime = ActionManager.GetAdjustedRecastTime(type, acId);
                        var adjustedId = am->GetAdjustedActionId(acId);
                        var recastGroup = am->GetRecastGroup((int)type, adjustedId);
                        if (CooldownList.ContainsKey(recastGroup)) {
                            GagSpeak.Log.Debug($"[Action Manager]: GROUP FOUND - Recast Time: {recastTime} | Cast Group: {recastGroup}");
                            var cooldownData = CooldownList[recastGroup];
                            // if we are beyond our recast time from the last time used, allow the execution
                            if (DateTime.Now >= cooldownData.Item2.AddMilliseconds(cooldownData.Item1)) {
                                // Update the last execution time before execution
                                GagSpeak.Log.Debug($"[Action Manager]: ACTION COOLDOWN FINISHED");
                                CooldownList[recastGroup] = new Tuple<float, DateTime>(cooldownData.Item1, DateTime.Now);
                            } else {
                                GagSpeak.Log.Debug($"[Action Manager]: ACTION COOLDOWN NOT FINISHED");
                                return false; // Do not execute the action
                            }
                        } else {
                            GagSpeak.Log.Debug($"[Action Manager]: GROUP NOT FOUND");
                        }
                    }
                }
            }
        } catch (Exception e) {
            GagSpeak.Log.Error(e.ToString());
        }
        // return the original if we reach here
        var ret = UseActionHook.Original(am, type, acId, target, a5, a6, a7, a8);
        // invoke the action used event
        return ret;
    }
}