
// event for controlling the restraint set listusing System;

using System;

namespace GagSpeak.Events;
/// <summary> Determines if the set is being enabled or disabled
public enum RestraintSetToggleType {
    Disabled,
    Enabled,
}

public class RS_ToggleEvent
{
    public delegate void RS_ToggleEventHandler(object sender, RS_ToggleEventArgs e); // define the event handler
    public event RS_ToggleEventHandler? SetToggled;                                 // define the event

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke(RestraintSetToggleType updateType, int setIndex, string assignerName) { // setindex can be size for integrity checks
        // remake this for the list update type
        GSLogger.LogType.Debug($"[RS_ToggleEventEvent] Restraint set index {setIndex} toggled to: {updateType} by {assignerName}");
        SetToggled?.Invoke(this, new RS_ToggleEventArgs(updateType, setIndex, assignerName));
    }
}

public class RS_ToggleEventArgs : EventArgs
{
    // contains the update type for the event
    public RestraintSetToggleType ToggleType { get; }
    // contains the index of the restraint set
    public int SetIndex { get; }
    // contains the name of the assigner
    public string AssignerName { get; }
    public RS_ToggleEventArgs(RestraintSetToggleType toggleType, int setIndex, string assignerName) {
        ToggleType = toggleType;
        SetIndex = setIndex;
        AssignerName = assignerName;
    }
}