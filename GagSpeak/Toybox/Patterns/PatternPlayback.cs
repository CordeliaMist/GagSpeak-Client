using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Math;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using ImPlotNET;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class PatternPlayback : IDisposable
{
    private readonly PlugService        _plugService;
    private readonly CharacterHandler   _characterHandler;
    private readonly SoundPlayer        _soundPlayer;
    private PatternData                 _tempStoredPattern;
    private TimerRecorder               _timerRecorder;
    public Stopwatch                    _recordingStopwatch;
    private List<byte>                  storedRecordedPositions = new List<byte>(); // the stored pattern data to playback
    private double[]                    currentPos = new double[2];  // The plotted points position on the wavelength graph
    public bool                         _isPlaybackActive;  // Whether the playback is active

    public PatternPlayback(PlugService plugService, CharacterHandler characterHandler, SoundPlayer soundPlayer) {
        _plugService = plugService;
        _characterHandler = characterHandler;
        _soundPlayer = soundPlayer;
        _isPlaybackActive = false;
        _playbackIndex = 0;
        // empty pattern
        _tempStoredPattern = new PatternData();
        // Create a new stopwatch
        _recordingStopwatch = new Stopwatch();
        // create a timer for realtime feedback display. This data is disposed of automatically after 300 entries (15s of data)
        _timerRecorder = new TimerRecorder(20, ReadVibePosFromBuffer);
    }

    public void Dispose() {
        // stop the sound player if it is playing
        if(_soundPlayer.isPlaying) {
            _soundPlayer.Stop();
        }
        _soundPlayer.Dispose();
        // dispose of the timers
        _timerRecorder.Dispose();
        _recordingStopwatch.Stop();
        _recordingStopwatch.Reset();
    }

public void Draw() {
    using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(0, 0)).Push(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
    using var child = ImRaii.Child("##PatternPlaybackChild", new Vector2(ImGui.GetContentRegionAvail().X, 80), true, ImGuiWindowFlags.NoScrollbar);
    if (!child) { return;}
    try{
        // Draw the waveform
        float[] xs;  // x-values
        float[] ys;  // y-values
        // if we are playing back
        if (_isPlaybackActive) {
            int start = Math.Max(0, _playbackIndex - 150);
            int count = Math.Min(150, _playbackIndex - start + 1);
            int buffer = 150 - count; // The number of extra values to display at the end


            xs = Enumerable.Range(-buffer, count + buffer).Select(i => (float)i).ToArray();
            ys = storedRecordedPositions.Skip(storedRecordedPositions.Count - buffer).Take(buffer)
                .Concat(storedRecordedPositions.Skip(start).Take(count))
                .Select(pos => (float)pos).ToArray();

            // Transform the x-values so that the latest position appears at x=0
            for (int i = 0; i < xs.Length; i++) {
                xs[i] -= _playbackIndex;
            }
        } else {
            xs = new float[0];
            ys = new float[0];
        }
        float latestX = xs.Length > 0 ? xs[xs.Length - 1] : 0; // The latest x-value
        // Transform the x-values so that the latest position appears at x=0
        for (int i = 0; i < xs.Length; i++) {
            xs[i] -= latestX;
        }

        // get the xpos so we can draw it back a bit to span the whole width
        var xPos = ImGui.GetCursorPosX();
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPos - ImGuiHelpers.GlobalScale * 10, yPos - ImGuiHelpers.GlobalScale * 10));
        var width = ImGui.GetContentRegionAvail().X + ImGuiHelpers.GlobalScale * 10;
        // set up the color map for our plots.
        ImPlot.PushStyleColor(ImPlotCol.Line, ColorId.LushPinkLine.Value());
        ImPlot.PushStyleColor(ImPlotCol.PlotBg, ColorId.LovenseScrollingBG.Value());
        // draw the waveform
        ImPlot.SetNextAxesLimits(- 150, 0, -5, 110, ImPlotCond.Always);
        if(ImPlot.BeginPlot("##Waveform", new System.Numerics.Vector2(width, 100), ImPlotFlags.NoBoxSelect | ImPlotFlags.NoMenus
        | ImPlotFlags.NoLegend | ImPlotFlags.NoFrame)) {
            ImPlot.SetupAxes("X Label", "Y Label", 
                ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks | ImPlotAxisFlags.NoHighlight,
                ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks);
            if (xs.Length > 0 || ys.Length > 0) {
                ImPlot.PlotLine("Recorded Positions", ref xs[0], ref ys[0], xs.Length);
            }
            ImPlot.EndPlot();
        }
        ImPlot.PopStyleColor(2);
    } catch (Exception e) {
        GSLogger.LogType.Error($"{e} Error drawing the toybox workshop subtab");
    }
}
#region Helper Fuctions
    // When active, the circle will not fall back to the 0 coordinate on the Y axis of the plot, and remain where it is
    public void StartPlayback(PatternData pattern, int IdxToPlay) {
        GSLogger.LogType.Debug($"Starting playback of pattern {pattern._name}");
        // set the playback index to the start
        _playbackIndex = 0;
        // set the stored pattern index we are using to playback here
        // (shoudld point to same place in memopry according to c# logic)
        _tempStoredPattern = pattern;
        // set the data to active and store the pattern data
        storedRecordedPositions = _tempStoredPattern._patternData;
        _tempStoredPattern._isActive = true;
        _isPlaybackActive = true;
        // Initialize the volume levels
        if(_characterHandler.playerChar._usingSimulatedVibe) {
            InitializeVolumeLevels(storedRecordedPositions);
        }
        // start our timers
        _recordingStopwatch.Start();
        _timerRecorder.Start();
        // start our simulated sound if active
        if(_characterHandler.playerChar._usingSimulatedVibe) {
            _soundPlayer.Play();
        }
    }

    public void InitializeVolumeLevels(List<byte> intensityPattern) {
        volumeLevels.Clear();
        foreach (var intensity in intensityPattern) {
            // Assuming intensity is a value between 0 and 100
            float volume = intensity / 100f;
            volumeLevels.Add(volume);
        }
    }
    private List<float> volumeLevels = new List<float>();

    public void StopPlayback() {
        GSLogger.LogType.Debug($"Stopping playback of pattern {_tempStoredPattern._name}");
        // clear the local variables
        _isPlaybackActive = false;
        _tempStoredPattern._isActive = false;
        _playbackIndex = 0;
        // clear the temp stored reference data, replacing it with a blank one
        _tempStoredPattern = new PatternData();
        // reset the timers
        _timerRecorder.Stop();
        _recordingStopwatch.Stop();
        _recordingStopwatch.Reset();
        // stop the simulated sound if active
        if(_characterHandler.playerChar._usingSimulatedVibe) {
            var value = _plugService.stepCount == 0 ? 20 : _plugService.stepCount;
            _soundPlayer.SetVolume((float)(_characterHandler.playerChar._intensityLevel/(double)value));
            volumeLevels.Clear();
        }
        // reset vibe to normal levels
        if(_plugService.HasConnectedDevice() && _plugService.IsClientConnected() && _plugService.anyDeviceConnected) {
            if(_characterHandler.playerChar._isToyActive) {
                _ = _plugService.ToyboxVibrateAsync((byte)((_characterHandler.playerChar._intensityLevel/(double)_plugService.stepCount)*100), 10);
            } else {
                _ = _plugService.ToyboxVibrateAsync(0, 10);
            }
        }

    }

    private int _playbackIndex;  // The current index of the playback
    private void ReadVibePosFromBuffer(object? sender, ElapsedEventArgs e) {
        // If we're playing back the stored positions
        if (_isPlaybackActive) {
            // If we've reached the end of the stored positions, stop playback
            if (_playbackIndex >= storedRecordedPositions.Count) {
                // first see if our current pattern is set to loop, if it is, then restart the playback
                if (_tempStoredPattern._loop) {
                    _playbackIndex = 0;
                    _recordingStopwatch.Restart();
                    return;
                } else {
                    StopPlayback();
                    return;
                }
            }
            //GSLogger.LogType.Debug($"Playing back position {_playbackIndex} with data {storedRecordedPositions[_playbackIndex]}");
            // Convert the current stored position to a float and store it in currentPos
            currentPos[1] = storedRecordedPositions[_playbackIndex];

            // Send the vibration command to the device
            if(_plugService.HasConnectedDevice() && _plugService.IsClientConnected() && _plugService.anyDeviceConnected) {
                if(_characterHandler.playerChar._isToyActive) {
                    _ = _plugService.ToyboxVibrateAsync(storedRecordedPositions[_playbackIndex], 10);
                } else {
                    _ = _plugService.ToyboxVibrateAsync(0, 10);
                }
            }
            // update volume to vibeSim if we are using it
            if(_characterHandler.playerChar._usingSimulatedVibe) {
                if (_playbackIndex < volumeLevels.Count) {
                    float volume = volumeLevels[_playbackIndex];
                    _soundPlayer.SetVolume(volume);
                }
            }
            _playbackIndex++;
        }
    }
#endregion Helper Fuctions
}
