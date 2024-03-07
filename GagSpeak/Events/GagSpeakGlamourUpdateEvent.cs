using System;

namespace GagSpeak.Events;
/// <summary> This class is used to handle the gag item equipped event, and triggers every time it is fired. </summary>
public enum UpdateType {
    /// <summary> used when enabling a restraint set or updating it </summary>
    UpdateRestraintSet,

    /// <summary> used when disabling a restraint set </summary>
    DisableRestraintSet,

    /// <summary> used when equipping a gag </summary>
    GagEquipped,    

    /// <summary> used when removing a gag or having it reset to none </summary>
    GagUnEquipped,

    /// <summary> used when a players gag selection was changed and should be udpated </summary>
    UpdateGags,

    /// <summary> used when the blindfold is equipped </summary>
    BlindfoldEquipped,

    /// <summary> used when the blindfold is unequipped </summary>
    BlindfoldUnEquipped,
    
    /// <summary> used when updating all characters information in general </summary>
    RefreshAll,
    
    /// <summary> used when a player changes their job </summary>
    JobChange,
    
    /// <summary> used when a player logs in </summary>
    Login,
    
    /// <summary> triggered on a zone change to refresh state </summary>
    ZoneChange,

    /// <summary> for when a safeword is used </summary>
    Safeword,
}

public class GagSpeakGlamourEvent
{
    public delegate void GagSpeakGlamourEventHandler(object sender, GagSpeakGlamourEventArgs e); // define the event handler
    public event GagSpeakGlamourEventHandler? GlamourEventFired;                                 // define the event
    public bool IsGagSpeakGlamourEventExecuting { get; set; }                                    // indicate if the event is being executed

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke(UpdateType updateType, string gagType = "None", string assignerName = "", int setIndex = -1) {
        GSLogger.LogType.Debug($"[GagSpeakGlamourEvent] Invoked Type: {updateType} with gagtype: {gagType} from {assignerName} (Optional extra var: {setIndex})");
        IsGagSpeakGlamourEventExecuting = true;
        GlamourEventFired?.Invoke(this, new GagSpeakGlamourEventArgs(updateType, gagType, assignerName, setIndex));
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
    // contains the index of the set
    public int SetIndex { get; }

    public GagSpeakGlamourEventArgs(UpdateType updateType, string gagType = "None", string assignerName = "", int setIndex = -1) {
        UpdateType = updateType;
        GagType = gagType;
        AssignerName = assignerName;
        SetIndex = setIndex;
    }
}