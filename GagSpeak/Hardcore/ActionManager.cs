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

namespace GagSpeak.Hardcore.Actions;
public unsafe class ActionManager : IDisposable
{
    private readonly HardcoreManager _hardcoreManager;
    private readonly IClientState _clientState;
    private readonly IPluginLog _log;
    private readonly IFramework _framework;
    private readonly IKeyState _keyState;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly ICondition _condition;
    private readonly IObjectTable _objectTable;
    private readonly RS_PropertyChangedEvent _rsPropertyChangedEvent;
    private FFXIVClientStructs.FFXIV.Client.Game.Control.Control* gameControl = FFXIVClientStructs.FFXIV.Client.Game.Control.Control.Instance(); // instance to have control over our walking
    delegate ref int GetRefValue(int vkCode); // virtual key code
    static GetRefValue getRefValue; // virtual key value
    internal delegate bool UseActionDelegate(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* am, ActionType type, uint acId, long target, uint a5, uint a6, uint a7, void* a8);
    internal Hook<UseActionDelegate> UseActionHook;

    public unsafe ActionManager(IClientState clientState, IPluginLog log, IFramework framework,
    IKeyState keyState, IGameInteropProvider interop, ICondition condition, IObjectTable gameObjects,
    HardcoreManager hardcoreManager, RS_PropertyChangedEvent RS_PropertyChangedEvent)
    {
        _hardcoreManager = hardcoreManager;
        _clientState = clientState;
        _log = log;
        _framework = framework;
        _keyState = keyState;
        _gameInteropProvider = interop;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;

        // subscribe
        _rsPropertyChangedEvent.SetChanged += OnRestraintSetPropertyChanged;
        _framework.Update += framework_Update;

        // set up a hook to fire every time the address signature is detected in our game.
        UseActionHook = _gameInteropProvider.HookFromAddress<UseActionDelegate>((nint)FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Addresses.UseAction.Value, UseActionDetour);
        UseActionHook.Enable();

        getRefValue = (GetRefValue)Delegate.CreateDelegate(typeof(GetRefValue), _keyState,
                    _keyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null));
        _condition = condition;
        _objectTable = gameObjects;

        GagSpeak.Log.Debug($"[Action Manager]: x {FFXIVClientStructs.FFXIV.Client.UI.AddonContentsFinder.Addresses.VTable}");
    }

    public void Dispose()
    {
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;
        _framework.Update -= framework_Update;
        // dispose of the hook
        UseActionHook.Disable();
        UseActionHook.Dispose();
    }

    private void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // if we have just turned off our force walk, allow us to move again
        if (e.PropertyType == HardcoreChangeType.ForcedWalk && e.ChangeType == RestraintSetChangeType.Disabled) {
            GagSpeak.Log.Debug($"[Action Manager]: Letting you run again");
            Task.Delay(200);
            Marshal.WriteByte((IntPtr)gameControl, 23163, 0x0);
        }
    }

    private void framework_Update(IFramework framework)
    {
        //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty]}");
        //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty56] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty56]}");
        //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty95] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty95]}");
        //_log.Debug($"_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty97] {_condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundToDuty97]}");
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

    // this should be replaces by the restraint set properties, which are invoked by  RS_PropertyChangedEvent.cs
    public enum AbilityRestrictionLevel
    {
        None,
        Hardcore,
        Minimal,
        Advanced,
        Spec
    }

    // tracking the role of job for the action detour
    public enum ActionRoles : byte
    {
        NonCombat = 0,
        Tank = 0x1,
        MeleeDps = 0x2,
        RangedDps = 0x3,
        Healer = 0x4
    }
}



// This class can likely be moved into its own seperate file, and used spesifically for detecting when the restraint set is toggled, and then updating all calls within action manager properties.

// theoretically, this whole class is redundant if we can just add the setToggled event to action manager then have it invoke a function which sets a variable "applyProperties" to true with a current active set, allowing
// the action manager to apply the properties to the player character. This would be a more efficient way of doing things, and would allow for a more modular approach to the action manager.
public class ActionManagerLogic
{
    // This will fire an event whenever a event is triggered, sending you info on who assigned it and what set it was assigned to.
    private readonly RS_ToggleEvent     _RS_ToggleEvent;
    // this will give you information about the playercharacter data, which you will need to get the current state of each hardcore property and its configuration
    private readonly CharacterHandler   _characterHandler;
    private readonly HardcoreManager    _hardcoreManager;
    // stores logic for actions, detours and other things, can be split into more files
    public ActionManagerLogic(RS_ToggleEvent RS_ToggleEvent, CharacterHandler characterHandler,
    HardcoreManager hardcoreManager) {
        _RS_ToggleEvent = RS_ToggleEvent;
        _characterHandler = characterHandler;
        _hardcoreManager = hardcoreManager;

        // subscribe to the event
        _RS_ToggleEvent.SetToggled += OnRestraintSetToggled;
    }
    // helper functions and other general management functions can go here for appending and extracting information from the hardcore manager.

    // executed whenever the player toggles a restraint set
    private void OnRestraintSetToggled(object sender, RS_ToggleEventArgs e) {
        // we should see if the set is enabled or disabled
        if(e.ToggleType == RestraintSetToggleType.Enabled) {
            // our set is enabling, and it is valid, so now we should apply all the properties set to the restraintset related to action restrictions here. 
            bool legsResrtainted = _hardcoreManager._forcedWalk;

            // you can apply any related to action restrictions  here for all properties that return true here

        } else {
            // our set is now disabled
            // if the assigner is self, disable all active properties for the restraint set, regardless of who it is.
            // (in other words, just put every Action restriction active in the movement manager, and turn it off.)
            // (this does not mean setting them to false, it means anything that is set to true, is what we should toggle the state of in the movement manager)

            // otherwise, get the list of Action restrictions that are active, and turn them off, just like we did when enabling them.

        }
    }
}