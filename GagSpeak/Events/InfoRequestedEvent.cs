using System;

namespace GagSpeak.Events;

/// <summary>
/// This class is used to handle the info request event, triggered by the timerservice when both the
/// interactioncooldowntimer is less than 0 or not present AND the config.sendInfoName is not null or empty.
/// </summary>
public class InfoRequestEvent
{
    public delegate void InfoRequestEventHandler(object sender, InfoRequestEventArgs e);
    public event InfoRequestEventHandler? InfoRequest;

    public void Invoke() {
        GagSpeak.Log.Debug($"[InfoRequestEvent] Invoked");
        InfoRequest?.Invoke(this, new InfoRequestEventArgs());
    }
}

public class InfoRequestEventArgs : EventArgs
{
    // You can add properties here to pass additional information about the info request
}