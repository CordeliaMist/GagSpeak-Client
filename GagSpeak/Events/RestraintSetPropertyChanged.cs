using System;
namespace GagSpeak.Events;
public enum HardcoreChangeType {
    RS_PropertyModified,
    // LegsRestraint,
    // ArmsRestraint,
    // Gagged,
    // Blindfolded,
    // Immobile,
    // Weighty,
    // LightStimulation,
    // MildStimulation,
    // HeavyStimulation, 
    ForcedWalk,
    ForcedSit,
    ForcedFollow,
    MovementDisabled,
    Safeword,
}
/// <summary>
/// The type of change that was made to the restraint
/// </summary>
public enum RestraintSetChangeType {
    Enabled,
    Disabled,
}

public class RS_PropertyChangedEvent
{
    public delegate void RS_PropertyChangedHandler(object sender, RS_PropertyChangedEventArgs e);
    public event RS_PropertyChangedHandler? SetChanged;                                 
    public void Invoke(HardcoreChangeType propertyType, RestraintSetChangeType changeType) {
        // remake this for the list update type
        GagSpeak.Log.Debug($"[RS_PropertyChangedEvent] Property {propertyType} was {changeType}");
        SetChanged?.Invoke(this, new RS_PropertyChangedEventArgs(propertyType, changeType));
    }
}

public class RS_PropertyChangedEventArgs : EventArgs
{
    // property that was changed
    public HardcoreChangeType PropertyType { get; }
    // change type
    public RestraintSetChangeType ChangeType { get; }
    public RS_PropertyChangedEventArgs(HardcoreChangeType propertyType, RestraintSetChangeType changeType) { 
        PropertyType = propertyType;
        ChangeType = changeType;
    }
}