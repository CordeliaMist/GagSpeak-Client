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
// Thank you to Ryuuki for most of this code. I have modified it to fit the needs of GagSpeak

// Big Warning: This is a very NSFW service. It is a service that connects to a toybox server and controls a lovense device.
// This service is not to be used in any way that is not consensual. It is also not to be used in any way that is not legal.
namespace GagSpeak.Services;
public class PlugService : IDisposable
{
    // migrate any direct calls to functions to make this private later
    private readonly    CharacterHandler _characterHandler; // for getting the whitelist
    public ButtplugClient _client = new ButtplugClient("GagSpeak");
    public ButtplugWebsocketConnector _connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345"));
    public ButtplugClientDevice? _activeDevice; // our connected device
    // our plugServiceAttributes
    public int _deviceIndex;
    public double _stepSize;
    public PlugService(CharacterHandler characterHandler) {
        _characterHandler = characterHandler;

        // Add events later.

        // _client.DeviceAdded += OnDeviceAdded;
        // _client.DeviceRemoved += OnDeviceRemoved;
        // _client.ErrorReceived += OnErrorReceived;
        // _client.ScanningFinished += OnScanningFinished;
        // _client.ServerDisconnect += OnServerDisconnect;
        AttemptDelayedConnection();
    }

    // when this service is disposed, we need to be sure to dispose of the client and the connector
    public void Dispose() {
        _client.DisconnectAsync();
        _client.Dispose();
        _connector.Dispose();
    }

    private async void AttemptDelayedConnection() {
        await Task.Delay(7000);
        ConnectToServerAsync();
    }

    /// <summary> Connect to the server asynchronously </summary>
    /// <returns>True if we are connected, false if not</returns>
    public bool IsClientConnected() {
        return _client.Connected;
    }

    /// <summary> See's if our ActiveDevice is null or not </summary>
    /// <returns>True if the ActiveDevice is not null, false if it is null</returns>
    public bool IsActiveDeviceNotNull() {
        return _activeDevice != null;
    }


    // Connect to the server asynchronously
    public async void ConnectToServerAsync() {
        try {
            if (!_client.Connected) {
                GagSpeak.Log.Debug("[Toybox Service] Attempting to connect to the Intiface server, client was not initially connected");
                await _client.ConnectAsync(_connector);
                if(_client.Connected) {
                    GagSpeak.Log.Debug("[Toybox Service] Successfully connected to the Intiface server");
                } else {
                    DisconnectAsync();
                    GagSpeak.Log.Debug("[Toybox Service] Timed out while attempting to connect to Intiface Server");
                }
            } else {
                GagSpeak.Log.Debug("[Toybox Service] No Need to connect to the Intiface server, client was already connected");
            }
        } 
        catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in ConnectToServerAsync: {ex.ToString()}");
        }
    }

    /// <summary> Sees if we have any connected devices on the server. </summary>
    /// <returns>True if we have a connected device, false if we do not.</returns>
    public bool HasConnectedDevice() {
        try {
            // see if we have a device at any point
            if (_client.Connected) {
                _activeDevice = _client.Devices.FirstOrDefault();
                if (_activeDevice != null) {
                    GetStepSizeForActiveDevice();
                    return true;
                }
                else {
                    return false;
                }
            }
            return false;
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in HasConnectedDevice: {ex.ToString()}");
            return false;
        }
    }

    /// <summary> Get's the devices step size. </summary>
    /// <returns>The step size of the device</returns>
    public void GetStepSizeForActiveDevice() {
        try {
            if (_client.Connected && _activeDevice != null) {
                if (_activeDevice.VibrateAttributes.Count > 0) {
                    _stepSize = 1.0 / _activeDevice.VibrateAttributes.First().StepCount;
                    _characterHandler.playerChar._activeToyStepSize = _stepSize;
                }
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Service] Error in setting step size: {ex.ToString()}");
        }
    }

    // Disconnect from the server Asyncronously
    public async void DisconnectAsync() {
        // when called, attempt to:
        try
        {
            // see if we are connected to the server. If we are, then disconnect from the server
            if (_client.Connected) {
                await _client.DisconnectAsync();
            }
            // once it is disconnected, dispose of the client and the connector
            _connector.Dispose();
            _connector = new ButtplugWebsocketConnector(new Uri("ws://localhost:12345"));
        }
        // if at any point we fail here, throw an exception
        catch (Exception ex) {
            // log the exception
            GagSpeak.Log.Error($"[Toybox Service] Error while disconnecting from Async: {ex.ToString()}");
        }
    }

    // Vibrate the device for a set amount of time and strength
    public async void Vibrate(int seconds, double strength) {
        // when this is recieved, attempt to:
        try
        {
            // if the strength is not between 0 and 1, log the information
            if (strength < 0 || strength > 1) {
                GagSpeak.Log.Information($"Strength must be between 0 and 1: {strength}");
            }
            // if we are connected to the server and the device is not null and the seconds are greater than 0
            if (_client.Connected && _activeDevice != null && seconds > 0) {
                // vibrate the device with the strength
                await _activeDevice.VibrateAsync(strength);
                // wait for the set amount of seconds
                Thread.Sleep(seconds * 1000);
                // stop the device from vibrating
                await _activeDevice.Stop();
            }
        }
        // if at any point we fail here, throw an exception
        catch (Exception ex) {
            // log the exception
            GagSpeak.Log.Error($"[Toybox Service] Error while vibrating: {ex.ToString()}");
        }
    }
}

