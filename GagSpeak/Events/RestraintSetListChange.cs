
// event for controlling the restraint set listusing System;

using System;

namespace GagSpeak.Events;
public enum ListUpdateType {
    AddedRestraintSet,
    ReplacedRestraintSet,
    RemovedRestraintSet,
    SizeIntegrityCheck,
    NameChanged,
}

public class RS_ListChanged
{
    public delegate void RestraintSetListChangedHandler(object sender, RestraintSetListChangedArgs e); // define the event handler
    public event RestraintSetListChangedHandler? SetListModified;                                 // define the event

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke(ListUpdateType updateType, int setIndex) { // setindex can be size for integrity checks
        // remake this for the list update type
        GSLogger.LogType.Debug($"[RestraintSetListChangedEvent] Invoked Type: {updateType} with set index {setIndex}");
        SetListModified?.Invoke(this, new RestraintSetListChangedArgs(updateType, setIndex));
    }
}

public class RestraintSetListChangedArgs : EventArgs
{
    // contains the update type for the event
    public ListUpdateType UpdateType { get; }
    // contains the index of the restraint set
    public int SetIndex { get; }
    public RestraintSetListChangedArgs(ListUpdateType updateType, int setIndex) {
        UpdateType = updateType;
        SetIndex = setIndex;
    }
}