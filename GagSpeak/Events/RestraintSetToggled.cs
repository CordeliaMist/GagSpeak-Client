
// event for controlling the restraint set listusing System;

using System;

namespace GagSpeak.Events;
/// <summary> Determines if the set is being enabled or disabled
public enum RestraintSetToggleType {
    Enabled,
    Disabled,
}

public class RestraintSetToggleEvent
{
    public delegate void RestraintSetToggleEventHandler(object sender, RestraintSetToggleEventArgs e); // define the event handler
    public event RestraintSetToggleEventHandler? SetToggled;                                 // define the event

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke(RestraintSetToggleType updateType, int setIndex, string assignerName) { // setindex can be size for integrity checks
        // remake this for the list update type
        GagSpeak.Log.Debug($"[RestraintSetToggleEventEvent] Restraint set index {setIndex} toggled to: {updateType} by {assignerName}");
        SetToggled?.Invoke(this, new RestraintSetToggleEventArgs(updateType, setIndex, assignerName));
    }
}

public class RestraintSetToggleEventArgs : EventArgs
{
    // contains the update type for the event
    public RestraintSetToggleType ToggleType { get; }
    // contains the index of the restraint set
    public int SetIndex { get; }
    // contains the name of the assigner
    public string AssignerName { get; }
    public RestraintSetToggleEventArgs(RestraintSetToggleType toggleType, int setIndex, string assignerName) {
        ToggleType = toggleType;
        SetIndex = setIndex;
        AssignerName = assignerName;
    }
}