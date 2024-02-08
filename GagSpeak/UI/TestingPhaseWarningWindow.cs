﻿using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using GagSpeak.Services;
using OtterGui;
using Dalamud.Interface.Utility.Raii;

namespace GagSpeak.UI;
// probably can remove this later, atm it is literally just used for the debug window
public class TestingPhaseWarningWindow : Window //, IDisposable
{
    private readonly MainWindow _ui;
    private readonly FontService _fontService;
    private readonly GagSpeakConfig _config;
    public TestingPhaseWarningWindow(FontService fontService, MainWindow mainWindow, GagSpeakConfig gagSpeakConfig) : base(GetLabel()) {
        _fontService = fontService;
        _ui = mainWindow;
        _config = gagSpeakConfig;
        Size = new Vector2(800, 550);
        // add flags that allow you to move, but not resize the window, also disable collapsible
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar;

        // open it
        Toggle();
    }

    public override void Draw() {
        using var style = ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(1, 0, 0, 0.8f))
                            .Push(ImGuiCol.Button, new Vector4(.4f, .4f, .4f, .6f));
        using (var child = ImRaii.Child("##TestingPhaseWarningWindowChild", new Vector2(-1,-1), true,
        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove )) {
            if (!child) { return; }

            if (ImGui.IsWindowAppearing()) { ImGui.SetKeyboardFocusHere(0); }
            ImGui.PushFont(_fontService.UidFont);
            ImGuiUtil.Center("THIS IS A TESTING PHASE UPDATE");
            ImGui.NewLine();
            ImGuiUtil.Center("This is a testing phase update!");
            ImGui.NewLine();
            ImGuiUtil.Center("Any feature in here could cause you to crash!");
            ImGui.NewLine();
            ImGuiUtil.Center("If you are not testing with me directly,");
            ImGuiUtil.Center("I HIGHLY ADVISE NOT USING THIS UNTIL THE TESTING PHASE IS OVER");
            ImGuiUtil.Center("(Because if something goes wrong i cant help you fix it)");
            ImGui.NewLine();
            ImGuiUtil.Center("Thank you for understanding.");
            ImGuiUtil.Center("I'llremove this window and post in CK discord when its released");
            var widthAdjust = ImGui.GetContentRegionAvail().X * 0.15f;
            var buttonWidth = ImGui.GetContentRegionAvail().X * 0.7f;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + widthAdjust);
            if (ImGui.Button("I'm Testing with the Dev", new Vector2(buttonWidth, 50))) {
                Toggle();
                _config.TestingPhaseDisableMode = false;
                _config.Save();
            }
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + widthAdjust);
            if(ImGui.Button("Ok, I Understand (Close all GagSpeak Windows)", new Vector2(buttonWidth, 50))) {
                Toggle();
                // if our main window is open close it
                if(_ui.IsOpen)
                    _ui.Toggle();
                // enable the testing phrase disable mode
                _config.TestingPhaseDisableMode = true;
                _config.Save();
            }

            ImGui.PopFont();
        }
    }
    private static string GetLabel() => "TestingPhaseWarningWindow###TestingPhaseWarningWindow";
}
