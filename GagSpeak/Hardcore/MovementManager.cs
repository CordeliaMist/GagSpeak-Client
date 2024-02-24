using System;
using System.Runtime.InteropServices;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Events;

namespace GagSpeak.Hardcore.Movement;
public class MovementManager : IDisposable
{
    // This will fire an event whenever a event is triggered, sending you info on who assigned it and what set it was assigned to.
    private readonly    RS_ToggleEvent      _rsToggleEvent;
    // this will give you information about the playercharacter data, which you will need to get the current state of each hardcore property and its configuration
    private readonly    CharacterHandler    _characterHandler;
    private readonly    HardcoreManager     _hardcoreManager;
    private readonly    ICondition          _condition;
    private readonly    RS_PropertyChangedEvent _rsPropertyChangedEvent;
    // for having the movement memory -- was originally private static, revert back if it causes issues.
    private static      MoveMemory          _moveMemory;
    public static readonly int[] _blockedKeys = new int[] { 321, 322, 323, 324, 325, 326 };
    
    // the list of keys that are blocked while movement is disabled. Req. to be static, must be set here.
    public MovementManager(RS_ToggleEvent RS_ToggleEvent, CharacterHandler characterHandler, ICondition condition,
    MoveMemory moveMemory, HardcoreManager hardcoreManager, RS_PropertyChangedEvent RS_PropertyChangedEvent) {
        _rsToggleEvent = RS_ToggleEvent;
        _characterHandler = characterHandler;
        _condition = condition;
        _moveMemory = moveMemory;
        _hardcoreManager = hardcoreManager;
        _rsPropertyChangedEvent = RS_PropertyChangedEvent;

        // subscribe to the event
        _rsToggleEvent.SetToggled += OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged += OnRestraintSetPropertyChanged;
    }

    public void Dispose() {
        // unsubscribe from the event
        _rsToggleEvent.SetToggled -= OnRestraintSetToggled;
        _rsPropertyChangedEvent.SetChanged -= OnRestraintSetPropertyChanged;
    }


    // helper functions and other general management functions can go here for appending and extracting information from the hardcore manager.
    private unsafe void EnableMoving() {
        GagSpeak.Log.Debug($"Enabling moving, cnt {_moveMemory.ForceDisableMovement}");
        // disable our hooks, we dont need to track them anymore.
        _moveMemory.DisableHooks();
        // if (_moveMemory.ForceDisableMovement > 0) {
        //     _moveMemory.ForceDisableMovement--;
        // }
    }

    private void DisableMoving() {
        GagSpeak.Log.Debug($"Disabling moving, cnt {_moveMemory.ForceDisableMovement}");
        // reinable our hooks
        _moveMemory.EnableHooks();
        // _moveMemory.ForceDisableMovement++;
    }

    private void OnRestraintSetPropertyChanged(object sender, RS_PropertyChangedEventArgs e) {
        // enable movement if this satisfies
        if(e.PropertyType == HardcoreChangeType.MovementDisabled && e.ChangeType == RestraintSetChangeType.Disabled) {
            EnableMoving();
        }
        // disable movement if this satisfies
        if(e.PropertyType == HardcoreChangeType.MovementDisabled && e.ChangeType == RestraintSetChangeType.Enabled) {
            DisableMoving();
        }
        
    }



    // executed whenever the player toggles a restraint set
    private void OnRestraintSetToggled(object sender, RS_ToggleEventArgs e) {
        // we should see if the set is enabled or disabled
        if(e.ToggleType == RestraintSetToggleType.Enabled) {
            // if it is enabled, we should check if the assigner is in the whitelist
            // you can get the current whitelist index like this:
            int assignerIdx = -1;
            // we dont need to validate if assigner is in whitelist because we already did it in the result logic.
            assignerIdx = _characterHandler.GetWhitelistIndex(e.AssignerName);
            // Only continue if the set is valid index
            if(assignerIdx == -1) {
                GagSpeak.Log.Debug($"[MovementManager] Assigner {e.AssignerName} is not in the whitelist, aborting");
                return; // early escape 
            }

            // our set is enabling, and it is valid, so now we should apply all the properties set to the restraint set related to movement here.

            // we can get the permissions using the arguements like such:
                // e.SetIndex == Restraint Set Index
                // assignerIdx = whitelisted player who toggled it.
                // properties in the uniquePlayerperms[assignerIdx]_PROPERTY[e.SetIndex] are the hardcore permissions you have enabled for that player for that set.
            // example for restricting leg movement.
            bool weightedDown = _hardcoreManager._rsProperties[e.SetIndex]._weightyProperty;

            // you can apply any movement logic here for all properties that return true here

        } else {
            // our set is now disabled
            // if the assigner is self, disable all active properties for the restraint set, regardless of who it is.
            // (in other words, just put every movement restriction active in the movement manager, and turn it off.)
            // (this does not mean setting them to false, it means anything that is set to true, is what we should toggle the state of in the movement manager)

            // otherwise, get the list of movement restrictions that are active, and turn them off, just like we did when enabling them.

        }
    }
}