using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using ImPlotNET;
using OtterGui;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.WorkshopTab;
public class WorkshopTab : ITab, IDisposable
{
    private readonly    IDalamudTextureWrap _spinningArrowTextureWrap;    // for loading images
    private readonly    IDalamudTextureWrap _floatingDotTextureWrap;    // for loading images
    private readonly    UiBuilder   _uiBuilder;             // for loading images
    private readonly    FontService _fontService;
    private readonly    PlugService _plugService;
    private readonly    WorkshopMediator _mediator;
    private             SavePatternWindow _SavePatternWindow;
    private readonly    CharacterHandler _characterHandler;
    private readonly    SoundPlayer _soundPlayer;
    private TimerRecorder _timerRecorder;
    private TimerRecorder _storedRecordedData;
    private List<double> recordedPositions = new List<double>();  // The recorded Y positions of the circle, this is used for the realtime feedback, temporary
    private List<double> tempStoredLoopPositions = new List<double>();  // Records AND STORES information about the recorded Y value of the circle, temporary
    //private List<byte> storedRecordedPositions = new List<byte>();  // Records AND STORES information about the recorded Y value of the circle, perminant
    private bool isDragging;
    public bool isLooping = false;
    public bool isFloating = false;
    private double[] circlePos = new double[2];  // The circle's position
    #region Attributes
    public float xAxisLimit = 40;
    public float yAxisLimitLower = 0;
    public float yAxisLimitUpper = 100;
    public double[] positions = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };  // The positions of the ticks
    public string[] labels = { "0%", "", "", "", "", "", "", "", "", "", "100%" };  // The labels of the ticks
    #endregion Attributes

    public ReadOnlySpan<byte> Label => "Workshop"u8; // apply the tab label

    public WorkshopTab(FontService fontService, UiBuilder uiBuilder, SavePatternWindow savePatternWindow, SoundPlayer soundPlayer,
    DalamudPluginInterface pluginInterface, PlugService plugService, WorkshopMediator mediator, CharacterHandler characterHandler) {
        _fontService = fontService;
        _uiBuilder = uiBuilder;
        _plugService = plugService;
        _mediator = mediator;
        _soundPlayer = soundPlayer;
        _SavePatternWindow = savePatternWindow;
        _characterHandler = characterHandler;
        
        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "arrows-spin.png");
        var imagePath2 = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "circle-dot.png");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        var IconImage2 = _uiBuilder.LoadImage(imagePath2);
        _spinningArrowTextureWrap = _uiBuilder.LoadImage(imagePath);
        _floatingDotTextureWrap = _uiBuilder.LoadImage(imagePath2);

        isDragging = false;
        // Create a new stopwatch
        _mediator.recordingStopwatch = new Stopwatch();
        // create a timer for realtime feedback display. This data is disposed of automatically after 300 entries (15s of data)
        _timerRecorder = new TimerRecorder(10, AddCirclePositionToBuffer);
        // create a timer for storing the recorded data. This timer is saved, and disposed of upon plugin restart or a new pattern start
        _storedRecordedData = new TimerRecorder(20, RecordData);
    }

    public void Dispose() {
        // stop the sound player if it is playing
        if(_soundPlayer.isPlaying) {
            _soundPlayer.Stop();
        }
        _soundPlayer.Dispose();
        _timerRecorder.Dispose();
    }

    public void DrawContent() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(0, 0))
                                .Push(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
        using var color = ImRaii.PushColor(ImGuiCol.ChildBg, ColorId.LovenseDragButtonBGAlt.Value());
        using var child = ImRaii.Child("##ToyboxWorkshopPanelChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return;}
        if(!_mediator.isRecording && _mediator.finishedRecording) { ImGui.BeginDisabled(); }
        try{
            // Draw the waveform
            float[] xs = Enumerable.Range(0, recordedPositions.Count).Select(i => (float)i).ToArray();  // x-values
            float[] ys = recordedPositions.Select(pos => (float)pos).ToArray();  // y-values
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
            if(ImPlot.BeginPlot("##Waveform", new System.Numerics.Vector2(width, 125), ImPlotFlags.NoBoxSelect | ImPlotFlags.NoMenus
            | ImPlotFlags.NoLegend | ImPlotFlags.NoFrame)) {
                ImPlot.SetupAxes("X Label", "Y Label", 
                    ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks | ImPlotAxisFlags.NoHighlight,
                    ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks);
                if (xs.Length > 0 || ys.Length > 0) {
                    ImPlot.PlotLine("Recorded Positions", ref xs[0], ref ys[0], xs.Length);
                }
                ImPlot.EndPlot();
            }
            // clear the styles
            ImPlot.PopStyleColor(2);
            // shift up again
            xPos = ImGui.GetCursorPosX();
            yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(yPos - ImGuiHelpers.GlobalScale * 13);
            ImGui.Separator();
            yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(yPos - ImGuiHelpers.GlobalScale);

            using (var table2 = ImRaii.Table("ThePatternCreationTable", 2, ImGuiTableFlags.NoPadInnerX |  ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.BordersV)) {
                if (!table2) { return; } // make sure our table was made
                ImGui.TableSetupColumn("InteractivePatternDrawer",  ImGuiTableColumnFlags.WidthStretch);                  
                ImGui.TableSetupColumn("InteractionButtons",        ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Time Remainingmmm..").X);
                ImGui.TableNextColumn();
                // create styles for the next plot
                ImPlot.PushStyleColor(ImPlotCol.PlotBg, ColorId.LovenseDragButtonBG.Value());
                // Draw the first row of the table
                // Draw a thin line with a timer to show the current position of the circle
                width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y + ImGui.GetTextLineHeight() + ImGuiHelpers.GlobalScale * 5;
                // go to the next line and draw the grid we can move out thing in
                yPos = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(yPos - ImGui.GetTextLineHeight());
                ImPlot.SetNextAxesLimits(- 50, + 50, -10, 110, ImPlotCond.Always);
                var PreviousPos = circlePos[1]; // store the Y position
                if (ImPlot.BeginPlot("##Box", new System.Numerics.Vector2(width+ImGui.GetTextLineHeight(), height), ImPlotFlags.NoBoxSelect | ImPlotFlags.NoLegend | ImPlotFlags.NoFrame)) {
                    ImPlot.SetupAxes("X Label", "Y Label", 
                        ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks | ImPlotAxisFlags.NoMenus | ImPlotAxisFlags.NoHighlight,
                        ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoMenus | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoHighlight);
                    ImPlot.SetupAxisTicks(ImAxis.Y1, ref positions[0], 11, labels);
                    ImPlot.DragPoint(0, ref circlePos[0], ref circlePos[1], ColorId.LushPinkButton.Value(), 20, ImPlotDragToolFlags.NoCursors);
                    
                    // if the mouse button is released, while we are looping and dragging turn dragging off
                    if (isDragging && ImGui.IsMouseReleased(ImGuiMouseButton.Left)) {
                        isDragging = false;
                        loopIndex = 0; // reset the index
                        GSLogger.LogType.Debug("Dragging Period Ended!");
                    }
                    // if our mouse is down...
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Left)) {

                        // if we are not yet marked as dragging, and the positions are different, then mark that we are dragging
                        if (!isDragging && PreviousPos != circlePos[1]) {
                            isDragging = true;
                            tempStoredLoopPositions = new List<double>(); // start a new list
                            loopIndex = 0; // reset the index
                            GSLogger.LogType.Debug("Dragging Period Started!");
                        }
                    }
                    // account for floating and boundry crossing
                    AccountForFloating();
                    // end the plot
                    ImPlot.EndPlot();
                }
                // pop the styles
                ImPlot.PopStyleColor();
                ImGui.TableNextColumn();
                // create another table inside here
                DrawSideButtonsTable();
                // now we can draw the buttons
                width = ImGui.GetContentRegionAvail().X;
                // Draw the buttons for recording and stopping the recording
                if(!_mediator.isRecording) {
                    if(ImGuiUtil.DrawDisabledButton("Start Recording##StartRecordingButton", new Vector2(width, -1), string.Empty, _mediator.isRecording)) {
                        _mediator.isRecording = !_mediator.isRecording;
                        StartRecording();
                    }
                } else {
                    if(ImGuiUtil.DrawDisabledButton("Stop Recording##StopRecordingButton", new Vector2(width, -1), string.Empty, !_mediator.isRecording)) {
                        _mediator.isRecording = !_mediator.isRecording;
                        StopRecording();
                        // for saving a pattern after it is finished recording
                        if(!_mediator.isRecording && _mediator.finishedRecording) {
                            // Get the size and position of the main window
                            Vector2 mainWindowSize = ImGui.GetWindowSize();
                            Vector2 mainWindowPos = ImGui.GetWindowPos();
                            // Calculate the center of the main window
                            Vector2 center = mainWindowPos + mainWindowSize / 2.0f;
                            // Get the size of the SavePatternWindow
                            Vector2 savePatternWindowSize = new Vector2(200, 100); // You need to implement GetSize method in SavePatternWindow
                            // Calculate the position of the SavePatternWindow so that it's centered relative to the main window
                            Vector2 savePatternWindowPos = center - savePatternWindowSize / 2.0f;
                            // Set the position of the SavePatternWindow
                            ImGui.SetNextWindowPos(savePatternWindowPos);
                            _SavePatternWindow.Toggle();
                        }
                    }
                }
            }
        } catch (Exception e) {
            GSLogger.LogType.Error($"{e} Error drawing the toybox workshop subtab");
        } finally {
            if(!_mediator.isRecording && _mediator.finishedRecording) { ImGui.EndDisabled(); }
            color.Dispose();
            color.Pop();
        }
    }

#region Helper sub-drawFunctions
    public void DrawSideButtonsTable() {
        // push our styles
        using var styleColor = ImRaii.PushColor(ImGuiCol.Button, new Vector4(.2f,.2f,.2f,.2f))
            .Push(ImGuiCol.ButtonHovered, new Vector4(.3f,.3f,.3f,.4f))
            .Push(ImGuiCol.ButtonActive, ColorId.LushPinkButton.Value());
        using var styleVar = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 40);
        // push the table 
        using (var table3 = ImRaii.Table("ThePatternCreationButtonsTable", 1, ImGuiTableFlags.None, new Vector2(-1,-ImGui.GetTextLineHeight()*2))) {
            if (!table3) { return; } // make sure our table was made
            // Create the headers for the table
            ImGui.TableSetupColumn("TheButtonColumn",  ImGuiTableColumnFlags.WidthStretch);                  
            ImGui.TableNextColumn();
            // Draw the first row of the table
            // Draw the current time
            var width = ImGui.GetContentRegionAvail().X;
            ImGui.SetNextItemWidth(width);
            ImGui.AlignTextToFramePadding();
            ImGui.PushFont(_fontService.UidFont);
            if(_mediator.isRecording) {
                ImGuiUtil.Center($"{_mediator.recordingStopwatch.Elapsed.ToString(@"mm\:ss")}");
            } else {
                // we should move down the same ammount that cell would have printed
                var yPos2 = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(yPos2 + ImGui.GetTextLineHeight());
            }
            ImGui.PopFont();
            // Draw out the looping button
            ImGui.NewLine();
            var xPos = ImGui.GetCursorPosX();
            var yPos = ImGui.GetCursorPosY();
            try{
                ImGui.SetCursorPosX(xPos + (ImGui.GetContentRegionAvail().X - 90*ImGuiHelpers.GlobalScale )/ 2*ImGuiHelpers.GlobalScale);
                if(ImGuiUtil.DrawDisabledButton($"##ThePatternLoopingButton", new Vector2(90*ImGuiHelpers.GlobalScale, 90*ImGuiHelpers.GlobalScale),
                "Keeps the circle from falling back to the ground", false)) {
                    isLooping = !isLooping;
                    if(isFloating) { isFloating = false; }
                }
                ImGui.SetCursorPos(new Vector2(xPos + (ImGui.GetContentRegionAvail().X - 80*ImGuiHelpers.GlobalScale )/ 2*ImGuiHelpers.GlobalScale, yPos+ 5f*ImGuiHelpers.GlobalScale));
                // now go back overtop the button and draw an image
                Vector4 buttonColor = isLooping ? ColorId.LushPinkButton.Value() : ColorId.SideButton.Value();
                ImGui.Image(_spinningArrowTextureWrap.ImGuiHandle, new Vector2(80*ImGuiHelpers.GlobalScale, 80*ImGuiHelpers.GlobalScale),
                Vector2.Zero, Vector2.One, buttonColor);
            } catch (Exception e) {
                GSLogger.LogType.Error($"{e} Error drawing the image button");
            }
            ImGui.NewLine();
            ImGui.NewLine();
            try{
                xPos = ImGui.GetCursorPosX();
                yPos = ImGui.GetCursorPosY();
                ImGui.SetCursorPosX(xPos + (ImGui.GetContentRegionAvail().X - 90*ImGuiHelpers.GlobalScale )/ 2*ImGuiHelpers.GlobalScale);
                if(ImGuiUtil.DrawDisabledButton($"##FloatButtonTriggerForPattern", new Vector2(90*ImGuiHelpers.GlobalScale, 90*ImGuiHelpers.GlobalScale), 
                "Keeps the circle from falling back to the ground", false)) {
                    isFloating = !isFloating;
                    if(isLooping) { isLooping = false; }
                }
                ImGui.SetCursorPos(new Vector2(xPos + (ImGui.GetContentRegionAvail().X - 80*ImGuiHelpers.GlobalScale )/ 2*ImGuiHelpers.GlobalScale, yPos+ 5f*ImGuiHelpers.GlobalScale));
                // now go back overtop the button and draw an image
                Vector4 buttonColor = isFloating ? ColorId.LushPinkButton.Value() : ColorId.SideButton.Value();
                ImGui.Image(_floatingDotTextureWrap.ImGuiHandle, new Vector2(80*ImGuiHelpers.GlobalScale, 80*ImGuiHelpers.GlobalScale),
                Vector2.Zero, Vector2.One, buttonColor);
            } catch (Exception e) {
                GSLogger.LogType.Error($"{e} Error drawing the image button");
            }
        }
        // pop the styles
        styleColor.Pop();
        styleVar.Pop();
    }
#endregion Helper sub-drawFunctions

#region Helper Fuctions
    // When active, the circle will not fall back to the 0 coordinate on the Y axis of the plot, and remain where it is
    public void AccountForFloating(){
        // Check if the circle's position is beyond the axis limit and set it to the limit if it is
        if (circlePos[0] > xAxisLimit) { circlePos[0] = xAxisLimit; }
        if (circlePos[0] < -xAxisLimit){ circlePos[0] = -xAxisLimit;}
        if (circlePos[1] > yAxisLimitUpper) { circlePos[1] = yAxisLimitUpper;}
        if (circlePos[1] < yAxisLimitLower) {circlePos[1] = yAxisLimitLower;}
        // if the isfloating is not active and we have let go of the circle, drop it.
        if(isFloating == false && isDragging == false){
            // drop the circle by 10
            if(circlePos[1] < 10) {
                circlePos[1] = 0;
            } else {
                circlePos[1] -= 10;
            }
        }
    }

    public void StartRecording() {
        _mediator.storedRecordedPositions.Clear();
        _timerRecorder.Start();
        _storedRecordedData.Start();
        _mediator.recordingStopwatch.Start();  // Start the stopwatch
        _mediator.isRecording = true;
        // start up the sound audio
        _soundPlayer.Play();
    }

    public void StopRecording() {
        _timerRecorder.Stop();
        _storedRecordedData.Stop();
        recordedPositions.Clear();
        tempStoredLoopPositions.Clear();
        // handle mediators
        _mediator.recordingStopwatch.Stop();  // Stop the stopwatch
        _mediator.isRecording = false;
        _mediator.finishedRecording = true;
        // send a command to switch the vibe back down to 0
        _ = _plugService.ToyboxVibrateAsync((byte)((_characterHandler.playerChar._intensityLevel/(double)_plugService.stepCount)*100), 10);
        // stop the sound audio
        GSLogger.LogType.Debug($"Stopping the sound audio {(float)(_characterHandler.playerChar._intensityLevel/(double)_plugService.stepCount)*100}");
        if(_characterHandler.playerChar._usingSimulatedVibe) {
            var size = _plugService.stepCount == 0 ? 20 : _plugService.stepCount;
            _soundPlayer.SetVolume((float)(_characterHandler.playerChar._intensityLevel/(double)size));
        }
    }

    // Then, in your main rendering loop:
    private int loopIndex; // the position in the temp stored loop data we are in
    // fired every 10ms
    private void AddCirclePositionToBuffer(object? sender, ElapsedEventArgs e) {
        // Limit the number of recorded positions to 1000 (add proper cap later)
        if (recordedPositions.Count > 1000) {
            // replace the list with a new list, making the first 200 elements the last 200 elements of the previous list
            recordedPositions = recordedPositions.GetRange(recordedPositions.Count - 200, 200);
        }
        // if we are not looping
        if(!isLooping) {
            // GSLogger.LogType.Debug("Not Looping!");
            // just add to the default
            recordedPositions.Add(circlePos[1]);
            return;
        }
        // if we are looping, and we are not yet dragging, (and we dont have any stored data yet)
        if(isLooping && !isDragging && tempStoredLoopPositions.Count == 0) {
            // this means we are storing looped data, but not yet dragging, so still log the original permissions
            // GSLogger.LogType.Debug("Looping, but not dragging!");
            recordedPositions.Add(circlePos[1]);
            return;
        }
        // if we are looping and dragging, then we need to store the data to both the tempRealTime and and the tempRecorded
        if(isLooping && isDragging) {
            // GSLogger.LogType.Debug("Looping and dragging!");
            // if we are dragging, and we are not yet looping, then we need to store the data to the tempRealTime
            recordedPositions.Add(circlePos[1]);
            tempStoredLoopPositions.Add(circlePos[1]);
            return;
        }
        // if we are marked as looping, but we are no longer dragging, and our tempstorage has data, then add that instead and increase the index
        if(isLooping && !isDragging && tempStoredLoopPositions.Count > 0) {
            // GSLogger.LogType.Debug("Looping, but not dragging, and we have data!");
            // if we are not dragging, and we have data, then we need to add the data from the temp storage
            recordedPositions.Add(tempStoredLoopPositions[loopIndex]);
            loopIndex++;
            // if we have reached the end of the loop, reset the index
            if(loopIndex >= tempStoredLoopPositions.Count) {
                loopIndex = 0;
            }
            return;
        }
    }
    private void RecordData(object? sender, ElapsedEventArgs e) {
        if(isLooping && !isDragging && tempStoredLoopPositions.Count > 0) {
            // GSLogger.LogType.Debug("Looping, but not dragging, and we have data!");
            // if we are not dragging, and we have data, then we need to add the data from the temp storage
            _mediator.storedRecordedPositions.Add((byte)Math.Round(tempStoredLoopPositions[loopIndex]));
        } else {
            // GSLogger.LogType.Debug("Not Looping!");
            // just add to the default
            _mediator.storedRecordedPositions.Add((byte)Math.Round(circlePos[1]));
        }
        // if we reached passed our "capped limit", start removing the data at the beginning
        if (_mediator.storedRecordedPositions.Count > 270000) {  // Limit the number of recorded positions to 1000
            GSLogger.LogType.Debug("Capped the stored data, stopping recording!");
            StopRecording();
        }
        if(_plugService.HasConnectedDevice() && _plugService.IsClientConnected() && _plugService.anyDeviceConnected) {
            if(isLooping && !isDragging && tempStoredLoopPositions.Count > 0) {
                //GSLogger.LogType.Debug($"{(byte)Math.Round(tempStoredLoopPositions[loopIndex])}");
                _ = _plugService.ToyboxVibrateAsync((byte)Math.Round(tempStoredLoopPositions[loopIndex]), 10);
            } else {
                //GSLogger.LogType.Debug($"{(byte)Math.Round(circlePos[1])}");
                _ = _plugService.ToyboxVibrateAsync((byte)Math.Round(circlePos[1]), 10);
            }
        }
        // record to simulated vibe if on
        if(_characterHandler.playerChar._usingSimulatedVibe) {
            if(isLooping && !isDragging && tempStoredLoopPositions.Count > 0) {
                _soundPlayer.SetVolume((float)(tempStoredLoopPositions[loopIndex]/100));
            } else {
                _soundPlayer.SetVolume((float)(circlePos[1]/100));
            }
        }

    }
#endregion Helper Fuctions
}
