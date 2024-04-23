using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Plugin;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GagSpeak.ToyboxandPuppeteer;
public class SoundPlayer
{
    private readonly    DalamudPluginInterface _pluginInterface;
    private             IWavePlayer waveOutDevice;
    private             AudioFileReader audioFileReader;
    private             LoopStream loopStream;
    private             SmbPitchShiftingSampleProvider pitchShifter;
    public  bool        isPlaying;
    private bool        isAdjustingVolume = false;
    private bool        isAdjustingPitch = false;
    private float       targetVolume = 0.01f;
    private float       targetPitch = 1.0f;
    private const float volumeChangeRate = 0.025f; // Adjust this value as needed
    private const float pitchChangeRate = 0.025f; // Adjust this value as needed
    // List of device names
    public List<string> DeviceNames { get; private set; }
    public int          ActiveDeviceId = 0; 
    // variable is here for the sake of tracking with the list variable, since the index on a list doesnt go negative, this is off by 1

    #pragma warning disable CS8618 // we dont need to worry about this since it happens in the startup function
    public SoundPlayer(DalamudPluginInterface pluginInterface) {
        _pluginInterface = pluginInterface;
        // setup blank list for device names
        DeviceNames = new List<string> { "Using Default Device!" };
        // get the audio file
        StartupNAudioProvider("vibratorQuiet.wav");
    }
    #pragma warning restore CS8618

    protected void StartupNAudioProvider(string audioPathInput, bool isDeviceChange = false) {
        // get the audio file
        string audioPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, audioPathInput);
        audioFileReader = new AudioFileReader(audioPath);
        loopStream = new LoopStream(audioFileReader);
        pitchShifter = new SmbPitchShiftingSampleProvider(loopStream.ToSampleProvider());
        var waveProvider = pitchShifter.ToWaveProvider16();
        // if device change is false, dont input the ID
        if(isDeviceChange == false)
        {
            waveOutDevice = new WaveOutEvent { DesiredLatency = 80, NumberOfBuffers = 3 }; // 100ms latency, 2 buffers, device id -1 expected.
        }
        else
        {
            waveOutDevice = new WaveOutEvent { DeviceNumber = ActiveDeviceId-1, DesiredLatency = 80, NumberOfBuffers = 3 };
        }
        // now try and initalize it
        try {
            // if devicechange is false, assume we are initializing
            GSLogger.LogType.Information($"Detected Device Count: {WaveOut.DeviceCount}");
            // see what device is currently selected
            for(int i = 0; i< WaveOut.DeviceCount; i++){
                var capabilities = WaveOut.GetCapabilities(i);
                GSLogger.LogType.Information($"Device {i}: {capabilities.ProductName}");
                DeviceNames.Add(capabilities.ProductName); // add the device name to the list
            }

            waveOutDevice.Init(waveProvider);
            GSLogger.LogType.Information("SoundPlayer sucessfully setup with NAudio");
        }
        catch (NAudio.MmException ex)
        {
            if (ex.Result == NAudio.MmResult.BadDeviceId)
            {
                // Handle the exception, e.g. show a message to the user
                GSLogger.LogType.Error("Bad Default Device ID. Attempting manual assignment.");

                // attempt to do it manually
                for(int i = 0; i< WaveOut.DeviceCount; i++){
                    try {
                        var capabilities = WaveOut.GetCapabilities(i);
                        GSLogger.LogType.Information($"Device {i}: {capabilities.ProductName}\n"+
                        $" --- Supports Playback Control: {capabilities.SupportsPlaybackRateControl}");

                        waveOutDevice = new WaveOutEvent { DeviceNumber = i, DesiredLatency = 80, NumberOfBuffers = 3 };
                        waveOutDevice.Init(waveProvider);
                        GSLogger.LogType.Information("SoundPlayer successfully setup with NAudio for device " + i);
                        // if we reach here, the device is valid and we can break the loop
                        ActiveDeviceId = i+1;
                        break;
                    }
                    catch (NAudio.MmException ex2)
                    {
                        if (ex2.Result == NAudio.MmResult.BadDeviceId)
                        {
                            // Handle the exception, e.g. show a message to the user
                            GSLogger.LogType.Error($"Bad Device ID for device {i}, trying next device.");
                        }
                        else
                        {
                            GSLogger.LogType.Error("Unknown NAudio Exception: " + ex2.Message);
                            throw;
                        }
                    }
                }
            }
            else
            {
                GSLogger.LogType.Error("Unknown NAudio Exception: " + ex.Message);
                throw;
            }
        }
    }

    public void SwitchDevice(int deviceId)
    {
        if (deviceId < 0 || deviceId >= WaveOut.DeviceCount)
        {
            GSLogger.LogType.Error($"Invalid device ID: {deviceId}");
            return;
        }

        bool wasActiveBeforeChange = isPlaying;
        if (isPlaying)
        {
            waveOutDevice.Stop();
        }

        waveOutDevice.Dispose();

        // change the activeDeviceId
        ActiveDeviceId = deviceId;

        waveOutDevice = new WaveOutEvent { DeviceNumber = deviceId-1, DesiredLatency = 80, NumberOfBuffers = 3 };
        waveOutDevice.Init(pitchShifter.ToWaveProvider16());
        GSLogger.LogType.Information($"Switched to device {deviceId}: {DeviceNames[deviceId]}");

        if (wasActiveBeforeChange)
        {
            waveOutDevice.Play();
        }
    }

    public void ChangeAudioPath(string audioPath) {
        // Stop the current audio
        bool wasActiveBeforeChange = isPlaying;
        if(isPlaying) {
            waveOutDevice.Stop();
        }
        // Dispose of the current resources
        waveOutDevice.Dispose();
        audioFileReader.Dispose();
        loopStream.Dispose();

        StartupNAudioProvider(audioPath);

        // If the audio was playing before, start it again
        if(wasActiveBeforeChange) {
            waveOutDevice.Play();
        }
    }

    public void Play() {
        isPlaying = true;
        audioFileReader.Volume = 0f;
        waveOutDevice.Play();
    }

    public void Stop() {
        isPlaying = false;
        waveOutDevice.Stop();
    }

    public async void SetVolume(float intensity) {
        targetVolume = intensity;
        targetPitch = 1.0f + intensity*.5f;

        // If the volume or pitch is already being adjusted, don't start another adjustment
        if (isAdjustingVolume || isAdjustingPitch) return;

        isAdjustingVolume = true;
        isAdjustingPitch = true;

        while (Math.Abs(audioFileReader.Volume - targetVolume) > volumeChangeRate || Math.Abs(pitchShifter.PitchFactor - targetPitch) > pitchChangeRate) {
            if (audioFileReader.Volume < targetVolume) {
                audioFileReader.Volume += volumeChangeRate;
            } else if (audioFileReader.Volume > targetVolume) {
                audioFileReader.Volume -= volumeChangeRate;
            }

            if (pitchShifter.PitchFactor < targetPitch) {
                pitchShifter.PitchFactor += pitchChangeRate;
            } else if (pitchShifter.PitchFactor > targetPitch) {
                pitchShifter.PitchFactor -= pitchChangeRate;
            }

            await Task.Delay(20); // Adjust this delay as needed
        }

        // Once the volume and pitch are close enough to the target, set them directly
        audioFileReader.Volume = targetVolume;
        pitchShifter.PitchFactor = targetPitch;
        isAdjustingVolume = false;
        isAdjustingPitch = false;
    }

    public void Dispose() {
        // Stop the audio and dispose of the resources
        waveOutDevice.Stop();
        waveOutDevice.Dispose();
        audioFileReader.Dispose();
        loopStream.Dispose();
    }
}