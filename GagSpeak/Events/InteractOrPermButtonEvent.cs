using System;

namespace GagSpeak.Events;

public class InteractOrPermButtonEvent
{
    public delegate void InteractOrPermButtonEventHandler(object sender, InteractOrPermButtonEventArgs e);
    public event InteractOrPermButtonEventHandler? ButtonPressed;

    public void Invoke(int seconds) {
        GSLogger.LogType.Debug($"[InteractOrPermButtonEvent] Invoked");
        ButtonPressed?.Invoke(this, new InteractOrPermButtonEventArgs(seconds));
    }
}

public class InteractOrPermButtonEventArgs : EventArgs
{
    public int Seconds { get; }

    public InteractOrPermButtonEventArgs(int seconds)
    {
        Seconds = seconds;
    }
}