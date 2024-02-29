using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace GagSpeak.UI;
public class BlindfoldWindow : Window, IDisposable
{
    private IDalamudTextureWrap     textureWrap;
    private readonly    UiBuilder   _uiBuilder;
    private readonly    IFramework  _framework;
    private readonly    IGameGui    _gameGui;
    private readonly    IClientState _clientState;
    private             Stopwatch   stopwatch = new Stopwatch();

    public unsafe BlindfoldWindow(UiBuilder uiBuilder, DalamudPluginInterface pluginInterface,
    IFramework framework, IGameGui gameGui, IClientState clientState) : base(GetLabel(),
    ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMouseInputs | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove |
    ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking) {
        _uiBuilder = uiBuilder;
        _framework = framework;
        _gameGui = gameGui;
        _clientState = clientState;
        // determine if the pop out window is shown
        IsOpen = false;
        
        _uiBuilder.DisableUserUiHide = true;

        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "BlindfoldLace_Sensual.png");
        textureWrap = _uiBuilder.LoadImage(imagePath);
    }

    public void Dispose() {
        textureWrap.Dispose();
    }

    public override unsafe void PreDraw()
    {
        // get the size of the screen and set it
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
    }

    public override void Draw() {
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
        // get the window size
        var windowSize = ImGui.GetWindowSize();
        // Draw the image in sections, skipping the cut-out rectangle
        ImGui.Image(textureWrap.ImGuiHandle, windowSize); // Bottom left section

    
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar(2);
        base.PostDraw();

    }

    private static string GetLabel() => "BlindfoldWindow###BlindfoldWindow";
}