using System;

namespace GagSpeak.Events;
/// <summary> This class is used to handle the gag item equipped event, and triggers every time it is fired. </summary>
public class ItemAutoEquipEvent
{
    public delegate void ItemAutoEquipEventHandler(object sender, ItemAutoEquipEventArgs e); // define the event handler
    public event ItemAutoEquipEventHandler? GagItemEquipped;                                    // define the event
    public bool IsItemAutoEquipEventExecuting { get; set; }                                     // indicate if the event is being executed

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke(string gagType, string assignerName) {
        GagSpeak.Log.Debug("[GagItemEquippedEventHandler] Invoked with gagtype: " + gagType + " from " + assignerName);
        IsItemAutoEquipEventExecuting = true;
        GagItemEquipped?.Invoke(this, new ItemAutoEquipEventArgs(gagType, assignerName));
    }
}

/// <summary>
/// This class is used to handle the gag item equipped event arguments
/// </summary>
public class ItemAutoEquipEventArgs : EventArgs
{
    public string GagType { get; }
    public string AssignerName { get; }
    public ItemAutoEquipEventArgs(string gagType, string assignerName) {
        GagType = gagType;
        AssignerName = assignerName;
    }
}