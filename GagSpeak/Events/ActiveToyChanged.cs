using System;

namespace GagSpeak.Events;
public class ActiveDeviceChangedEvent
{
    public delegate void ActiveDeviceChangedEventHandler(object sender, ActiveDeviceChangedEventArgs e);
    public event ActiveDeviceChangedEventHandler? ActiveDeviceChanged;
    public void Invoke(int deviceIndex) {
        ActiveDeviceChanged?.Invoke(this, new ActiveDeviceChangedEventArgs(deviceIndex));
    }
}

public class ActiveDeviceChangedEventArgs : EventArgs
{
    public int DeviceIndex { get; }

    public ActiveDeviceChangedEventArgs(int deviceIndex)
    {
        DeviceIndex = deviceIndex;
    }
}