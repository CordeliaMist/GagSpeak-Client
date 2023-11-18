using System.Collections.Generic;
using System;

namespace GagSpeak.Events;
// Thrown every time the safeword is changed. Must be used before onSelectedIndexChanged.
// Or it will cause a crash loop

// This event will fire when the safeword command is executed
public class SafewordUsedEvent 
{
    public delegate void SafewordCommandEventHandler(object sender, SafewordCommandEventArgs e); // define the event handler
    public event SafewordCommandEventHandler SafewordCommand; // define the event

    public void Invoke() { // method to manually trigger the event
        SafewordCommand?.Invoke(this, new SafewordCommandEventArgs());
    }
}

// define the event args
public class SafewordCommandEventArgs : EventArgs
{
    // You can add properties here to pass additional information about the safeword command
}