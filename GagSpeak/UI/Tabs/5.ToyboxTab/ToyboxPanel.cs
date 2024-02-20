using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using GagSpeak.Events;
using System.Diagnostics;
using Dalamud.Interface;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public enum ToyboxSubTab {
    Setup,
    Patterns,
    Triggers
}

public class ToyboxPanel
{
    private readonly    PlugService _plugService; // for getting the plug service
    private readonly    PatternPlayback _patternPlayback; // for getting the pattern playback
    private readonly    CharacterHandler _charHandler; // for getting the whitelist
    private readonly    SetupAndInfoSubtab _setupAndInfoSubtab; // for getting the setup and info subtab
    private readonly    PatternSubtab _patternSubtab; // for getting the pattern subtab
    private readonly    TriggersSubtab _triggerSubtab; // for getting the trigger subtab
    private readonly    ActiveDeviceChangedEvent _activeDeviceChangedEvent; // for getting the active device
    private             ToyboxSubTab _activeToyboxSubTab;
    public ToyboxPanel(FontService fontService, CharacterHandler characterHandler, PlugService plugService,
    PatternPlayback patternPlayback, ActiveDeviceChangedEvent activeDeviceChangedEvent, ToyboxPatternTable patternTable,
    SetupAndInfoSubtab setupAndInfoSubtab, PatternSubtab patternSubtab, TriggersSubtab triggerSubtab) {
        _charHandler = characterHandler;
        _plugService = plugService;
        _patternPlayback = patternPlayback;
        _setupAndInfoSubtab = setupAndInfoSubtab;
        _patternSubtab = patternSubtab;
        _triggerSubtab = triggerSubtab;
        _activeDeviceChangedEvent = activeDeviceChangedEvent;
        _activeToyboxSubTab = ToyboxSubTab.Setup;
        // should never occur but safeguarding
        if(_plugService == null) {
            throw new ArgumentNullException(nameof(plugService));
        }
    }

    public void Draw() {
        using (_ = ImRaii.Group()) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            DrawToyboxPanelHeader();
            DrawToyboxPanel();
            // draw the pattern playback sampler here
            _patternPlayback.Draw();
            DrawToyboxButtonRow();
        }
    }

    // draw the header
    private void DrawToyboxPanelHeader() {
        WindowHeader.Draw($"Toybox Setup for {_charHandler.whitelistChars[_charHandler.activeListIdx]._name}",
        0, ImGui.GetColorU32(ImGuiCol.FrameBg), 1, 0,
        DeviceStateButton(_charHandler.playerChar._isToyActive),
        ConnectionButton(_plugService.IsClientConnected()),
        GetIntifaceButton());
        // Set the size of the next window (the popup)
        ImGui.SetNextWindowSize(new Vector2(350, 100));
        if (ImGui.BeginPopup("IntifacePopup")) {
            ImGuiUtil.Center("Either click to watch a quick CK guide on the install & setup");
            ImGuiUtil.Center("Or go straight to the releases page for the download");
            if (ImGui.Button("Watch Quick Install Guide", new Vector2(-1, 0))) {
                Process.Start(new ProcessStartInfo {FileName = "https://www.youtube.com/watch?v=h3JBYiwtAXg&list=PLGzKipCtkx7EAyk1k5gRFG8ZyKB0FMTR3&index=6", UseShellExecute = true});
                ImGui.CloseCurrentPopup();
            }
            if (ImGui.Button("Go To Site Directly", new Vector2(-1, 0))) {
                Process.Start(new ProcessStartInfo {FileName = "https://github.com/intiface/intiface-central/releases/", UseShellExecute = true});
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawToyboxPanel() {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using var child = ImRaii.Child("ToyboxPanelChild", new Vector2(ImGui.GetContentRegionAvail().X, -ImGui.GetFrameHeight()-80*ImGuiHelpers.GlobalScale), true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return;}
        // draw the permission buttons
        UIHelpers.CheckboxNoConfig("Toy State",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} can turn your toys on/off",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._allowChangingToyState,
        v => _charHandler.ToggleChangeToyState(_charHandler.activeListIdx)
        );
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("Intensity", 
        $"If  {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} can adjust your active toys intensity.",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._allowIntensityControl,
        v => _charHandler.ToggleAllowIntensityControl(_charHandler.activeListIdx)
        );
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("Patterns",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} can execute patterns to your toys",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._allowUsingPatterns,
        v => _charHandler.ToggleAllowPatternExecution(_charHandler.activeListIdx)
        );
        ImGui.SameLine();
        // UIHelpers.CheckboxNoConfig("Triggers",
        // $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} can execute triggers involving their name",
        // _charHandler.playerChar._allowUsingTriggers[_charHandler.activeListIdx],
        // v => _charHandler.ToggleAllowTriggerExecution(_charHandler.activeListIdx)
        // );
        // draw the separator
        ImGui.NewLine();
        // draw out the subtabs
        ToyboxSubtabs();
        // draw the body for the selected tab
        DrawBody();
        // pop our styles and draw the pattern playback here.
        style.Pop();
    }
#region TabSelection
    public void ToyboxSubtabs() {
        using var _ = ImRaii.PushId( "ToyboxSubTabs" );
        using var tabBar = ImRaii.TabBar( "ToyboxSubTabs" );
        if( !tabBar ) return;

        if (ImGui.BeginTabItem("Setup & Info")) {
            _activeToyboxSubTab = ToyboxSubTab.Setup;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Patterns")) {
            _activeToyboxSubTab = ToyboxSubTab.Patterns;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Triggers")) {
            _activeToyboxSubTab = ToyboxSubTab.Triggers;
            ImGui.EndTabItem();
        }
    }

    public void DrawBody() {
        // determine which permissions we will draw out
        switch (_activeToyboxSubTab) {
            case ToyboxSubTab.Setup:
                _setupAndInfoSubtab.Draw();
                break;
            case ToyboxSubTab.Patterns:
                _patternSubtab.Draw();
                break;
            case ToyboxSubTab.Triggers:
                _triggerSubtab.Draw();
                break;
        }
    }
#endregion TabSelection
#region HeaderButtons
    private WindowHeader.Button DeviceStateButton(bool deviceState)
        => deviceState
            ? new WindowHeader.Button {
                Description = "Toy is On",
                Icon = FontAwesomeIcon.ToggleOn,
                OnClick = () => {
                    _charHandler.ToggleToyState();
                    // see what the new state is, and update the vibe accordingly
                    if(_charHandler.playerChar._isToyActive) {
                        _ = _plugService.ToyboxVibrateAsync((byte)((_charHandler.playerChar._intensityLevel/(double)_plugService.stepCount)*100), 20);
                    } else {
                        _ = _plugService.ToyboxVibrateAsync(0, 20);
                    }
                },
                Width = ImGui.GetFrameHeightWithSpacing() + 3*ImGuiHelpers.GlobalScale,
                Visible = true,
                Disabled = false,
            }
            : new WindowHeader.Button {
                Description = "Toy is Off",
                Icon = FontAwesomeIcon.ToggleOff,
                OnClick = () => {
                    _charHandler.ToggleToyState();
                    // see what the new state is, and update the vibe accordingly
                    if(_charHandler.playerChar._isToyActive) {
                        _ = _plugService.ToyboxVibrateAsync((byte)((_charHandler.playerChar._intensityLevel/(double)_plugService.stepCount)*100), 20);
                    } else {
                        _ = _plugService.ToyboxVibrateAsync(0, 20);
                    }
                },
                Width = ImGui.GetFrameHeightWithSpacing() + 3*ImGuiHelpers.GlobalScale,
                Visible = true,
                Disabled = false,
            };

    private WindowHeader.Button ConnectionButton(bool isConnected)
        => isConnected
            ? new WindowHeader.Button {
                Description = "Disconnect to Intiface",
                Icon = FontAwesomeIcon.Unlink,
                OnClick = () => _plugService.DisconnectAsync(),
                Width = ImGui.GetFrameHeightWithSpacing() + 2*ImGuiHelpers.GlobalScale,
                Visible = true,
                Disabled = false,
            }
            : new WindowHeader.Button {
                Description = "Connect to Intiface",
                Icon = FontAwesomeIcon.Link,
                OnClick = () => _plugService.ConnectToServerAsync(),
                Width = ImGui.GetFrameHeightWithSpacing() + 2*ImGuiHelpers.GlobalScale,
                Visible = true,
                Disabled = false,
            };

    private WindowHeader.Button GetIntifaceButton()
        => new WindowHeader.Button {
            Description = "Opens the Intiface website.",
            Icon = FontAwesomeIcon.Globe, // Replace with the appropriate icon
            OnClick = () => ImGui.OpenPopup("IntifacePopup"),
            Visible = true,
            Disabled = false,
        };
#endregion HeaderButtons
#region ToyboxButtons
    private void DrawToyboxButtonRow() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, -1);
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, -1);
        var xPos = ImGui.GetCursorPosX();
        // we want to make a combo dropdown of our plug services connected devices.
        // to do so, first see if we have any devices.
        // Get the size of the combo box
        Vector2 comboBoxSize = new Vector2(buttonWidth.X, -1);
        try{
            // if we do have connected devices, draw that combo list here
            ImGui.SetNextItemWidth(buttonWidth2.X);
            if(ImGui.Combo("##DeviceList", ref _plugService.deviceIndex,
            _plugService.client.Devices.Select(d => d.Name).ToArray(), _plugService.client.Devices.Count())) {
                // if the combo list is changed, we should update the active device
                _activeDeviceChangedEvent.Invoke(_plugService.deviceIndex);
            }
            comboBoxSize = ImGui.GetItemRectSize();
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[Toybox Overview Panel] Error generating a list from connected devices {ex.ToString()}");
        }
        ImGui.SameLine();
        // draw the start scanning button
        if (ImGuiUtil.DrawDisabledButton("Start Scanning", new Vector2(buttonWidth.X, comboBoxSize.Y), "Begins scanning for any toys looking to connect to client",
        _plugService.isScanning || !_plugService.IsClientConnected())) {
            // begin scanning for devices
            _ = _plugService.StartScanForDevicesAsync().ContinueWith(t => 
            {
                if (t.Exception != null) {
                    GagSpeak.Log.Error($"[Toybox Overview Panel] Error starting scan for devices: {t.Exception}");
                }
            });
        }
        ImGui.SameLine();
        // draw the stop scanning button
        if (ImGuiUtil.DrawDisabledButton("Stop Scanning", new Vector2(buttonWidth.X, comboBoxSize.Y), "Stops scanning for any toys looking to connect to client",
        !_plugService.isScanning || !_plugService.IsClientConnected())) {
            // stop scanning for devices
            _ = _plugService.StopScanForDevicesAsync().ContinueWith(t => 
            {
                if (t.Exception != null) {
                    GagSpeak.Log.Error($"[Toybox Overview Panel] Error stopping scan for devices: {t.Exception}");
                }
            });
        }
    }
#endregion ToyboxButtons
}
