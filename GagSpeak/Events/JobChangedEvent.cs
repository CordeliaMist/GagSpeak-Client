using System;

namespace GagSpeak.Events;
public class JobChangedEvent
{
    public delegate void JobChangedEventHandler(object sender, JobChangedEventArgs e);
    public event JobChangedEventHandler? JobChanged;
    public void Invoke() {
        JobChanged?.Invoke(this, new JobChangedEventArgs());
    }
}

public class JobChangedEventArgs : EventArgs
{

}