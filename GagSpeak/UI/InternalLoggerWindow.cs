using System.Numerics;
using Dalamud.Interface.Windowing;
using GagSpeak.GSLogger;
using ImGuiNET;

namespace GagSpeak.UI;
public class InternalLoggerWindow : Window
{
    private readonly InternalLog _internalLog;
    public unsafe InternalLoggerWindow(InternalLog internallog) : base(GetLabel()) {
        _internalLog = internallog;

        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(200, 400),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };

        _internalLog.OnOpenWindowRequested += ToggleWindow;
    }

    private void ToggleWindow() {
        IsOpen = !IsOpen;
    }

    public override void Draw() {
        _internalLog.PrintImgui();
    }
    
    private static string GetLabel() => "InternalLoggerWindow###BlindfoldWindow";
}