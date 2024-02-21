using GagSpeak.CharacterData;
using GagSpeak.Events;

namespace GagSpeak.Hardcore;

public class ActionManager
{
    // This will fire an event whenever a event is triggered, sending you info on who assigned it and what set it was assigned to.
    private readonly RestraintSetToggleEvent    _restraintSetToggleEvent;
    // this will give you information about the playercharacter data, which you will need to get the current state of each hardcore property and its configuration
    private readonly CharacterHandler           _characterHandler;
    // stores logic for actions, detours and other things, can be split into more files
    public ActionManager(RestraintSetToggleEvent restraintSetToggleEvent, CharacterHandler characterHandler) {
        _restraintSetToggleEvent = restraintSetToggleEvent;
        _characterHandler = characterHandler;

        // subscribe to the event
        _restraintSetToggleEvent.SetToggled += OnRestraintSetToggled;
    }
    // helper functions and other general management functions can go here for appending and extracting information from the hardcore manager.

    // executed whenever the player toggles a restraint set
    private void OnRestraintSetToggled(object sender, RestraintSetToggleEventArgs e) {
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

            // our set is enabling, and it is valid, so now we should apply all the properties set to the restraintset related to action restrictions here.

            // we can get the permissions using the arguements like such:
                // e.SetIndex == Restraint Set Index
                // assignerIdx = whitelisted player who toggled it.
                // properties in the uniquePlayerperms[assignerIdx]_PROPERTY[e.SetIndex] are the hardcore permissions you have enabled for that player for that set.
            // example for restricting action restrictions 
            bool legsResrtainted = _characterHandler.playerChar._uniquePlayerPerms[assignerIdx]._legsRestraintedProperty[e.SetIndex];

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