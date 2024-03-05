using System;

namespace GagSpeak.Events;

// Thrown every time the safeword is changed. Must be used before onSelectedIndexChanged.

/// <summary> This class is used to handle the safeword command event, and triggers every time it is fired. </summary>
public class SafewordUsedEvent 
{
    public delegate void SafewordCommandEventHandler(object sender, SafewordCommandEventArgs e); // define the event handler
    public event SafewordCommandEventHandler? SafewordCommand;                                    // define the event

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke() {
        GSLogger.LogType.Debug("[SafewordUsedEvent] Invoked");
        SafewordCommand?.Invoke(this, new SafewordCommandEventArgs());
    }
}

/// <summary>
/// This class is used to handle the safeword command event arguments, if any ever end up existing
/// </summary>
public class SafewordCommandEventArgs : EventArgs
{
    // You can add properties here to pass additional information about the safeword command
}