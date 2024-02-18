using System;
using System.IO;
using System.Threading.Tasks;
using Dalamud.Plugin;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace GagSpeak.ToyboxandPuppeteer;
public class SoundPlayer
{
    private readonly DalamudPluginInterface _pluginInterface;
    private          IWavePlayer waveOutDevice;
    private          AudioFileReader audioFileReader;
    private          LoopStream loopStream;
    private          SmbPitchShiftingSampleProvider pitchShifter;
    public  bool    isPlaying;
    private bool    isAdjustingVolume = false;
    private bool    isAdjustingPitch = false;
    private float   targetVolume = 0.01f;
    private float   targetPitch = 1.0f;
    private const float volumeChangeRate = 0.025f; // Adjust this value as needed
    private const float pitchChangeRate = 0.025f; // Adjust this value as needed

    public SoundPlayer(DalamudPluginInterface pluginInterface) {
        _pluginInterface = pluginInterface;
        // get the audio file
        string audioPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, "vibratorQuiet.wav");
        audioFileReader = new AudioFileReader(audioPath);
        loopStream = new LoopStream(audioFileReader);
        pitchShifter = new SmbPitchShiftingSampleProvider(loopStream.ToSampleProvider());
        var waveProvider = pitchShifter.ToWaveProvider16();
        waveOutDevice = new WaveOut { DesiredLatency = 80, NumberOfBuffers = 3 }; // 100ms latency, 2 buffers
        waveOutDevice.Init(waveProvider);
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

        string newAudioPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, audioPath);
        // Initialize new resources with the new audio file
        audioFileReader = new AudioFileReader(newAudioPath);
        loopStream = new LoopStream(audioFileReader);
        pitchShifter = new SmbPitchShiftingSampleProvider(loopStream.ToSampleProvider());
        var waveProvider = pitchShifter.ToWaveProvider16();
        waveOutDevice = new WaveOut { DesiredLatency = 80, NumberOfBuffers = 3 }; // 100ms latency, 2 buffers
        waveOutDevice.Init(waveProvider);

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