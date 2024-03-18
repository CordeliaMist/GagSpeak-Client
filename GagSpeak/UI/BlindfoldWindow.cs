using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Timers;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;

namespace GagSpeak.UI;

public enum AnimType { ActivateWindow, DeactivateWindow, None }

public enum BlindfoldType { Light, Sensual }

public class BlindfoldWindow : Window, IDisposable
{
    private DalamudPluginInterface  _pi;
    private IDalamudTextureWrap     textureWrap;
    private UiBuilder               _uiBuilder;
    private TimerRecorder           _timerRecorder;
    private Stopwatch               stopwatch = new Stopwatch();
    private float alpha = 0.0f; // Alpha channel for the image
    private float imageAlpha = 0.0f; // Alpha channel for the image
    private Vector2 position = new Vector2(0, -ImGui.GetIO().DisplaySize.Y); // Position of the image, start from top off the screen
    public AnimType AnimationProgress = AnimType.ActivateWindow; // Whether the image is currently animating
    public bool isShowing = false; // Whether the image is currently showing
    float progress = 0.0f;
    float easedProgress = 0.0f;
    float startY = -ImGui.GetIO().DisplaySize.Y;
    float midY = 0.2f * ImGui.GetIO().DisplaySize.Y;

    public unsafe BlindfoldWindow(UiBuilder uiBuilder, DalamudPluginInterface pluginInterface) : base(GetLabel(),
    ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove |
    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoNavFocus) {
        _uiBuilder = uiBuilder;
        _pi = pluginInterface;
        // determine if the pop out window is shown
        IsOpen = false;
        // make it not close on escape
        RespectCloseHotkey = false;
        // make it not close on ui hide
        _uiBuilder.DisableUserUiHide = true;

        // Load the image
        var imagePath = Path.Combine(_pi.AssemblyLocation.Directory?.FullName!, "BlindfoldLace_Sensual.png");
        textureWrap = _uiBuilder.LoadImage(imagePath);
        // set the stopwatch to send an elapsed time event after 2 seconds then stop
        _timerRecorder = new TimerRecorder(2000, ToggleWindow);
    }

    public void Dispose() {
        _timerRecorder.Dispose();
    }

    public override unsafe void PreDraw() {
        ImGui.SetNextWindowPos(Vector2.Zero); // start at top left of the screen
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size); // draw across the whole screen
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero); // set the padding to 0
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f); // set the border size to 0
    }

    public void ChangeBlindfoldType(BlindfoldType type) {
        var imagePath = "";
        if(type == BlindfoldType.Light) {
            imagePath = Path.Combine(_pi.AssemblyLocation.Directory?.FullName!, "Blindfold_Light.png");
            textureWrap?.Dispose(); // Dispose the old image to free resources
            textureWrap = _uiBuilder.LoadImage(imagePath); // Load the new image
        } else {
            imagePath = Path.Combine(_pi.AssemblyLocation.Directory?.FullName!, "BlindfoldLace_Sensual.png");
            textureWrap?.Dispose(); // Dispose the old image to free resources
            textureWrap = _uiBuilder.LoadImage(imagePath); // Load the new image
        }
    }

    public void ToggleWindow(object? sender, ElapsedEventArgs e) {
        if (IsOpen && !isShowing) {
            this.Toggle();
            _timerRecorder.Stop();
        } else {
            // just stop 
            AnimationProgress = AnimType.None;
            _timerRecorder.Stop();
        }
    }

    public void ActivateWindow() {
        GSLogger.LogType.Debug($"BlindfoldWindow: Activating window");
        // if an active timer is running
        if (_timerRecorder.IsRunning) {
            // we were trying to deactivate the window, so stop the timer and turn off the window
            GSLogger.LogType.Debug($"BlindfoldWindow: Timer is running, stopping it");
            _timerRecorder.Stop();
            this.Toggle();
        }
        // now turn it back on and reset all variables
        this.Toggle();
        alpha = 0.0f; // Alpha channel for the image
        imageAlpha = 0.0f; // Alpha channel for the image
        position = new Vector2(0, -ImGui.GetIO().DisplaySize.Y); // Position of the image, start from top off the screen
        progress = 0.0f;
        easedProgress = 0.0f;
        startY = -ImGui.GetIO().DisplaySize.Y;
        midY = 0.2f * ImGui.GetIO().DisplaySize.Y;
        AnimationProgress = AnimType.ActivateWindow;
        isShowing = true;
        // Start the stopwatch when the window starts showing
        _timerRecorder.Start();

    }

    public void DeactivateWindow() {
        // if an active timer is running
        if (_timerRecorder.IsRunning) {
            // we were trying to deactivate the window, so stop the timer and turn off the window
            _timerRecorder.Stop();
        }
        // start the timer to deactivate the window
        _timerRecorder.Start();
        AnimationProgress = AnimType.DeactivateWindow;
        alpha = 1.0f;
        imageAlpha = 1.0f;
        isShowing = false;
    }

    public override void Draw() {
        // force focus the window
        if (!ImGui.IsWindowFocused()) {
            if (!stopwatch.IsRunning) {
                stopwatch.Start();
            }
            if (stopwatch.ElapsedMilliseconds >= 100) {
                ImGui.SetWindowFocus();
                stopwatch.Reset();
            }
        } else {
            stopwatch.Reset();
        }

        if(AnimationProgress != AnimType.None) {
            // see if we are playing the actionation animation
            if(AnimationProgress == AnimType.ActivateWindow) {
                progress = (float)_timerRecorder.Elapsed.TotalMilliseconds / 2000.0f; // 2.0f is the total duration of the animation in seconds
                progress = Math.Min(progress, 1.0f); // Ensure progress does not exceed 1.0f
                // Use a sine function for the easing
                startY = -ImGui.GetIO().DisplaySize.Y;
                midY = 0.1f * ImGui.GetIO().DisplaySize.Y;
                if (progress < 0.7f) {
                    alpha = (1 - (float)Math.Pow(1 - (progress / 0.7f), 1.5)) / 0.7f;
                    // First 80% of the animation: ease out quint from startY to midY
                    easedProgress = 1 - (float)Math.Pow(1 - (progress / 0.7f), 1.5);
                    position.Y = startY + (midY - startY) * easedProgress;
                } else {
                    // Last 20% of the animation: ease in from midY to 0
                    easedProgress = 1 - (float)Math.Cos(((progress - 0.7f) / 0.3f) * Math.PI / 2);
                    position.Y = midY + (0 - midY) * easedProgress;
                }
                // If the animation is finished, stop the stopwatch and reset alpha
                if (progress >= 1.0f) {
                    AnimationProgress = AnimType.None;
                }
                imageAlpha = Math.Min(alpha, 1.0f); // Ensure the image stays at full opacity once it reaches it
            }
            // or if its the deactionation one
            else if(AnimationProgress == AnimType.DeactivateWindow) {
                // Calculate the progress of the animation based on the elapsed time
                progress = (float)_timerRecorder.Elapsed.TotalMilliseconds / 2000.0f; // 2.0f is the total duration of the animation in seconds
                progress = Math.Min(progress, 1.0f); // Ensure progress does not exceed 1.0f
                // Use a sine function for the easing
                startY = -ImGui.GetIO().DisplaySize.Y;
                midY = 0.1f * ImGui.GetIO().DisplaySize.Y;
                // Reverse the animation
                if (progress < 0.3f) {
                    // First 30% of the animation: ease in from 0 to midY
                    easedProgress = (float)Math.Sin((progress / 0.3f) * Math.PI / 2);
                    position.Y = midY * easedProgress;
                } else {
                    alpha = (progress - 0.3f) / 0.7f;
                    // Last 70% of the animation: ease out quint from midY to startY
                    easedProgress = (float)Math.Pow((progress - 0.3f) / 0.7f, 1.5);
                    position.Y = midY + (startY - midY) * easedProgress;
                }
                // If the animation is finished, stop the stopwatch and reset alpha
                if (progress >= 1.0f) {
                    AnimationProgress = AnimType.None;
                }
                imageAlpha = 1 - (alpha == 1 ? 0 : alpha); // Ensure the image stays at full opacity once it reaches it
            }
        } else {
            position.Y = isShowing ? 0 : startY;
        }
        // Set the window position
        ImGui.SetWindowPos(position);
        // get the window size
        var windowSize = ImGui.GetWindowSize();
        // Draw the image with the updated alpha value
        ImGui.Image(textureWrap.ImGuiHandle, windowSize, Vector2.Zero, Vector2.One, new Vector4(1.0f, 1.0f, 1.0f, imageAlpha));
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(2);
        base.PostDraw();
    }

    private static string GetLabel() => "BlindfoldWindow###BlindfoldWindow";
}