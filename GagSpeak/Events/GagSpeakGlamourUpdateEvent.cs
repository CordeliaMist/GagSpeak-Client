using System;

namespace GagSpeak.Events;
/// <summary> This class is used to handle the gag item equipped event, and triggers every time it is fired. </summary>
public enum UpdateType {
    // Used for enabling a restraint set, or updating it
    UpdateRestraintSet,
    // used when disabling a restraint set
    DisableRestraintSet,
    // gag equipped
    GagEquipped,    
    // used when removing a gag or having it reset to none
    GagUnEquipped,
    // used for updating a characters information in general
    RefreshAll,
}

public class GagSpeakGlamourEvent
{
    public delegate void GagSpeakGlamourEventHandler(object sender, GagSpeakGlamourEventArgs e); // define the event handler
    public event GagSpeakGlamourEventHandler? GlamourEventFired;                                 // define the event
    public bool IsGagSpeakGlamourEventExecuting { get; set; }                                    // indicate if the event is being executed

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke(UpdateType updateType, string gagType = "None", string assignerName = "") {
        GagSpeak.Log.Debug($"[GagSpoeakGlamourEvent] Invoked Type: {updateType} with gagtype: {gagType} from {assignerName}");
        IsGagSpeakGlamourEventExecuting = true;
        GlamourEventFired?.Invoke(this, new GagSpeakGlamourEventArgs(updateType, gagType, assignerName));
    }
}

/// <summary>
/// This class is used to handle the gag item equipped event arguments
/// </summary>
public class GagSpeakGlamourEventArgs : EventArgs
{
    // contains the update type for the event
    public UpdateType UpdateType { get; }
    // contains the gag that is being removed, so we know what info to grab from our gag storage
    public string GagType { get; }
    // contains the name of the assigner
    public string AssignerName { get; }
    public GagSpeakGlamourEventArgs(UpdateType updateType, string gagType = "None", string assignerName = "") {
        UpdateType = updateType;
        GagType = gagType;
        AssignerName = assignerName;
    }
}