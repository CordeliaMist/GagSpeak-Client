using System;

namespace GagSpeak.Events;

public class InteractOrPermButtonEvent
{
    public delegate void InteractOrPermButtonEventHandler(object sender, InteractOrPermButtonEventArgs e);
    public event InteractOrPermButtonEventHandler? ButtonPressed;

    public void Invoke() {
        GagSpeak.Log.Debug($"[InteractOrPermButtonEvent] Invoked");
        ButtonPressed?.Invoke(this, new InteractOrPermButtonEventArgs());
    }
}

public class InteractOrPermButtonEventArgs : EventArgs
{
    // You can add properties here to pass additional information about the info request
}