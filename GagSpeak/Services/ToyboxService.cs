using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Buttplug;
using Buttplug.Client;
using Buttplug.Client.Connectors;
using Buttplug.Client.Connectors.WebsocketConnector;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using GagSpeak.Utility;
// Thank you to Ryuuki for most of this code. I have modified it to fit the needs of GagSpeak

// Big Warning: This is a very NSFW service. It is a service that connects to a toybox server and controls a lovense device.
// This service is not to be used in any way that is not consensual. It is also not to be used in any way that is not legal.
namespace GagSpeak.Services;
public class PlugService : IDisposable
{
    // migrate any direct calls to functions to make this private later
    private readonly CharacterHandler _characterHandler; // for getting the whitelist
    private readonly IChatGui _chatGui; // for sending messages to the chat
    private readonly ActiveDeviceChangedEvent _activeDeviceChangedEvent;
    public ButtplugClient client;
    public ButtplugWebsocketConnector connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345"));
    public ButtplugClientDevice? activeDevice; // our connected device
    // our plugServiceAttributes
    public int deviceIndex;         // know our actively selected device index in _client.Devices[]
    public bool anyDeviceConnected; // to know if any device is connected without needing to interact with server
    public bool isScanning;         // to know if we are scanning
    public double stepInterval;         // know the step size of our active device
    public int stepCount;           // know the step count of our active device
    public double batteryLevel;     // know the battery level of our active device
    public PlugService(CharacterHandler characterHandler, IChatGui chatGui,
    ActiveDeviceChangedEvent activeDeviceChangedEvent) {
        _characterHandler = characterHandler;
        _activeDeviceChangedEvent = activeDeviceChangedEvent;
        _chatGui = chatGui;
        // initially assume we have no connected devices
        anyDeviceConnected = false;
        isScanning = false;
        deviceIndex = -1;
        stepCount = 0;
        batteryLevel = 0;
        ////// STEP ONE /////////
        // connect to client
        client = new ButtplugClient("GagSpeak");
        ////// STEP TWO /////////
        // once the client connects, it will ask server for list of devices.
        // we will want to make sure that we check to see if we have them, meaning we will need to set up events.
        // in order to cover all conditions, we should set up all of our events so we can recieve them from the server
        // (Side note: it is fine that our connector is defined up top, because it isnt used until we do ConnectASync)
        client.DeviceAdded += OnDeviceAdded;
        client.DeviceRemoved += OnDeviceRemoved;
        client.ScanningFinished += OnScanningFinished;
        client.ServerDisconnect += OnServerDisconnect;
        ////// STEP THREE /////////
        // Now that our events are set up, we can connect to server (and any other event subscribers)
        _activeDeviceChangedEvent.ActiveDeviceChanged += OnActiveDeviceChanged;
    }

    // when this service is disposed, we need to be sure to dispose of the client and the connector
    public void Dispose() {
        client.DisconnectAsync();
        client.Dispose();
        connector.Dispose();
        // dispose of our events
        client.DeviceAdded -= OnDeviceAdded;
        client.DeviceRemoved -= OnDeviceRemoved;
        client.ScanningFinished -= OnScanningFinished;
        client.ServerDisconnect -= OnServerDisconnect;
    }

#region Event Handlers
    /// <summary Fired every time a device is added to the client </summary>
    private void OnDeviceAdded(object? sender, DeviceAddedEventArgs e) {
        GagSpeak.Log.Debug($"[Toybox Service][Event Handler] Device Added: {e.Device.Name}");
        if(client.Devices.Count() > 0 && anyDeviceConnected == false) {
            activeDevice = client.Devices.First();
            GetstepIntervalForActiveDevice();
            GetBatteryLevelForActiveDevice();
            anyDeviceConnected = true;
        }
        if(anyDeviceConnected) {
            activeDevice = client.Devices.First();
            GetstepIntervalForActiveDevice();
            GetBatteryLevelForActiveDevice();
        }
    }

    /// <summary Fired every time a device is removed from the client </summary>
    private void OnDeviceRemoved(object? sender, DeviceRemovedEventArgs e) {
        GagSpeak.Log.Debug($"[Toybox Service][Event Handler] Device Removed: {e.Device.Name}");
        if(HasConnectedDevice() == false) {
            anyDeviceConnected = false;
            deviceIndex = -1;
        }
    }

    /// <summary Fired when scanning for devices is finished </summary>
    private void OnScanningFinished(object? sender, EventArgs e) {
        GagSpeak.Log.Debug("[Toybox Service][Event Handler] Scanning Finished");
        isScanning = false;
    }

    /// <summary Fired when the server disconnects </summary>
    private void OnServerDisconnect(object? sender, EventArgs e) {
        // reset all of our variables so we dont display any data requiring one
        anyDeviceConnected = false;
        isScanning = false;
        deviceIndex = -1;
        activeDevice = null;
        stepCount = 0;
        batteryLevel = 0;
        GagSpeak.Log.Debug("[Toybox Service][Event Handler] Server Disconnected");
    }

    /// <summary Fired when the active device is changed </summary>
    private void OnActiveDeviceChanged(object? sender, ActiveDeviceChangedEventArgs e) {
        GagSpeak.Log.Debug($"[Toybox Service][Event Handler] Active Device Index Changed to: {e.DeviceIndex}");
        // first make sure this index is within valid bounds of our client devices
        if (e.DeviceIndex >= 0 && e.DeviceIndex < client.Devices.Count()) {
            // if it is, set our device index to the new index
            deviceIndex = e.DeviceIndex;
            // set our active device to the new device
            activeDevice = client.Devices.ElementAt(deviceIndex);
            // get the step size for the new device
            GetstepIntervalForActiveDevice();
            GetBatteryLevelForActiveDevice();
        } else {
            GagSpeak.Log.Error($"[Toybox Service][Event Handler] Active Device Index {e.DeviceIndex} out of bounds, not updating.");
        }
    }

#endregion Event Handlers

#region Client Functions

    /// <summary> Connect to the server asynchronously </summary>
    /// <returns>True if we are connected, false if not</returns>
    public bool IsClientConnected() => client.Connected;
    

    /// <summary> See's if our ActiveDevice is null or not </summary>
    /// <returns>True if the ActiveDevice is not null, false if it is null</returns>
    public bool IsActiveDeviceNotNull() => activeDevice != null;

    /// <summary> Sees if we have any connected devices on the server. </summary>
    /// <returns>True if we have a connected device, false if we do not.</returns>
    public bool HasConnectedDevice() => client.Connected && client.Devices.Any() ? true : false;

    /// <summary> Connect to the server asynchronously </summary>
    public async void ConnectToServerAsync() {
        try {
            if (!client.Connected) {
                GagSpeak.Log.Debug("[Toybox Service] Attempting to connect to the Intiface server, client was not initially connected");
                await client.ConnectAsync(connector);
                // after we wait for the connector to process, check if the connection is there
                if(client.Connected) {
                    // 1. see if there are already devices connected
                    anyDeviceConnected = HasConnectedDevice();
                    // 2. If anyDeviceConnected is true, set our active device to the first device in the list
                    if(anyDeviceConnected) {
                        // set our active device to the first device in the list
                        activeDevice = client.Devices.First();
                        GetstepIntervalForActiveDevice();
                        GetBatteryLevelForActiveDevice();
                        // we should also set our device index to 0
                        deviceIndex = 0;
                        // activate the vibe
                        if(_characterHandler.playerChar._isToyActive) {
                            GagSpeak.Log.Debug($"[Toybox Service] Active Device: {activeDevice.Name}, is enabled! Vibrating with intensity: {(byte)((_characterHandler.playerChar._intensityLevel/(double)stepCount)*100)}");
                            await ToyboxVibrateAsync((byte)((_characterHandler.playerChar._intensityLevel/(double)stepCount)*100), 100);
                        }
                    }
                    // if we meet here, it's fine, it just means we are connected and dont yet have anything to display.
                    // So we will wait until a device is added to set anyDeviceConnected to true
                    UIHelpers.LogAndPrintGagSpeakMessage("Successfully connected to the Intiface server!", _chatGui, "[GagSpeak Toybox]");
                } else {
                    DisconnectAsync();
                    UIHelpers.LogAndPrintGagSpeakMessage("Timed out while attempting to connect to Intiface Server!", _chatGui, "[GagSpeak Toybox]");
                }
            } else {
                UIHelpers.LogAndPrintGagSpeakMessage("No Need to connect to the Intiface server, client was already connected!", _chatGui, "[GagSpeak Toybox]");
            }
        } 
        catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in ConnectToServerAsync: {ex.Message.ToString()}");
            UIHelpers.LogAndPrintGagSpeakMessage("Error while connectiong to Intiface Server! Make sure you have disabled any other plugins "+
            $"that connect with Intiface before connecting, or make sure you have Intiface running.", _chatGui, "[GagSpeak Toybox]");
        }
    }

    /// <summary> Disconnect from the server asynchronously </summary>
    public async void DisconnectAsync() {
        // when called, attempt to:
        try
        {
            // see if we are connected to the server. If we are, then disconnect from the server
            if (client.Connected) {
                await client.DisconnectAsync();
                if(client.Connected == false) {
                    UIHelpers.LogAndPrintGagSpeakMessage("Successfully disconnected from the Intiface server!", _chatGui, "[GagSpeak Toybox]");
                }
            }
            // once it is disconnected, dispose of the client and the connector
            connector.Dispose();
            connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345"));
        }
        // if at any point we fail here, throw an exception
        catch (Exception ex) {
            // log the exception
            GagSpeak.Log.Error($"[Toybox Service] Error while disconnecting from Async: {ex.ToString()}");
        }
    }

    /// <summary> Start scanning for devices asynchronously </summary>
    public async Task StartScanForDevicesAsync() {
        UIHelpers.LogAndPrintGagSpeakMessage("Now scanning for devices, you may attempt to connect a device now", _chatGui, "[GagSpeak Toybox]");
        try {
            if (client.Connected) {
                isScanning = true;
                await client.StartScanningAsync();
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in ScanForDevicesAsync: {ex.ToString()}");
        }
    }

    /// <summary> Stop scanning for devices asynchronously </summary>
    public async Task StopScanForDevicesAsync() {
        UIHelpers.LogAndPrintGagSpeakMessage("Halting the scan for new devices to add", _chatGui, "[GagSpeak Toybox]");
        try {
            if (client.Connected) {
                await client.StopScanningAsync();
                if (isScanning) {
                    isScanning = false;
                }
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in StopScanForDevicesAsync: {ex.ToString()}");
        }
    }

    /// <summary> Changes active Device and invokes . </summary>

#endregion Client Functions
#region Toy Functions

    /// <summary> Get's the devices step size. </summary>
    /// <returns>The step size of the device</returns>
    public void GetstepIntervalForActiveDevice() {
        try {
            if (client.Connected && activeDevice != null) {
                if (activeDevice.VibrateAttributes.Count > 0) {
                    stepCount = (int)activeDevice.VibrateAttributes.First().StepCount;
                    stepInterval = 1.0 / stepCount;
                }
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in setting step size: {ex.ToString()}");
        }
    }

    public async void GetBatteryLevelForActiveDevice() {
        try {
            if (client.Connected && activeDevice != null) {
                if (activeDevice.HasBattery) {
                    // try get to get the battery level
                    try{
                        batteryLevel = await activeDevice.BatteryAsync();
                    }
                    catch (Exception ex) {
                        GagSpeak.Log.Error($"[Toybox Service] Error in getting battery level: {ex.ToString()}");
                    }
                }
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in getting battery level: {ex.ToString()}");
        }
    }


    // Vibrate the device for a set amount of time and strength
    public async Task ToyboxVibrateAsync(byte intensity, int msUntilTaskComplete = 100) {
        // when this is recieved, attempt to:
        try
        {
            // must recalbulate the strength to be between 0 and 1. Here before going in
            double strength = intensity / 100.0;
            // round the strength to the nearest step
            strength = Math.Round(strength / stepInterval) * stepInterval;
            // log it
            // GagSpeak.Log.Debug($"[Toybox Service] Rounded Step: {strength}");
            // send it
            if (anyDeviceConnected && deviceIndex >= 0 && msUntilTaskComplete > 0) {
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                await activeDevice.VibrateAsync(strength); // the strength to move to from previous strength level
                // wait for the set amount of seconds
                await Task.Delay(msUntilTaskComplete);
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
            } else {
                GagSpeak.Log.Error("[Toybox Service] No device connected or device index is out of bounds, cannot vibrate.");
            
            }
        }
        // if at any point we fail here, throw an exception
        catch (Exception ex) {
            // log the exception
            GagSpeak.Log.Error($"[Toybox Service] Error while vibrating: {ex.ToString()}");
        }
    }
#endregion Toy Functions
}

