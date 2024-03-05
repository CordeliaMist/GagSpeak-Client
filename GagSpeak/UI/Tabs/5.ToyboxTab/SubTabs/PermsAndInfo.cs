using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using Dalamud.Utility;
using GagSpeak.Utility;
using System;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using OtterGui;
using GagSpeak.Events;
using System.Linq;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class PermsAndInfoSubtab : IDisposable
{
    private readonly    GagSpeakConfig              _config;
    private readonly    CharacterHandler            _charHandler;
    private readonly    IClientState                _client;
    private readonly    PlugService                 _plugService;
    private readonly    SoundPlayer                 _soundPlayer;
    private readonly    PatternHandler              _patternCollection;
    private readonly    FontService                 _fontService; // for getting the font
    private readonly    ActiveDeviceChangedEvent    _activeDeviceChangedEvent; // for getting the active device
    private             int? _tempSliderValue; // for storing the slider value
    private             string? _tempPortValue; // for storing the port value
    private             int _simulatedVibeType; // quiet or loud or none?

    public PermsAndInfoSubtab(GagSpeakConfig config, CharacterHandler charHandler, SoundPlayer soundPlayer,
    PlugService plugService, PatternHandler patternCollection, FontService fontService, IClientState client,
    ActiveDeviceChangedEvent activeDeviceChangedEvent) {
        _config = config;
        _charHandler = charHandler;
        _client = client;
        _plugService = plugService;
        _soundPlayer = soundPlayer;
        _patternCollection = patternCollection;
        _fontService = fontService;
        _activeDeviceChangedEvent = activeDeviceChangedEvent;
        // setup values
        _tempSliderValue = 0;
        _tempPortValue = _config.intifacePortValue != null ? _config.intifacePortValue : "ws://localhost:12345";
        _simulatedVibeType = 0;

        // start the sound player if it is not already started
        if(!_soundPlayer.isPlaying && _charHandler.playerChar._usingSimulatedVibe && _charHandler.playerChar._isToyActive) {
            _soundPlayer.Play();
        }
    }

    public void Dispose() {
        // stop the sound player if it is playing
        if(_soundPlayer.isPlaying) {
            _soundPlayer.Stop();
        }
        _soundPlayer.Dispose();
    }

    public void Draw() {
        using var child = ImRaii.Child("##ToyboxPermsOverviewChild", new Vector2(ImGui.GetContentRegionAvail().X, -ImGuiHelpers.GlobalScale*81), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
        // pop the zero spacing style var for everything inside
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        var name = $"{_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}";
        var yourName = "";
        if(_client.LocalPlayer == null) { yourName = "You"; }
        else { yourName = $"{_client.LocalPlayer.Name.ToString().Split(' ')[0]}"; }
        // show header
        ImGui.PushFont(_fontService.UidFont);
        try{
            ImGuiUtil.Center($"Toy Info & Permissions for {name}");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
            $"This determines what you are allowing {name} to be able to enable for you.\n"+
            "You are NOT controlling what to do to them.");
            }
        } finally { ImGui.PopFont(); }
        ImGui.Separator();
        // in the first section, draw out the permissions
        // draw the permission buttons
        UIHelpers.CheckboxNoConfig("State",
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
        UIHelpers.CheckboxNoConfig("Triggers",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} can execute triggers involving their name",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._allowUsingTriggers,
        v => _charHandler.ToggleAllowToyboxTriggers(_charHandler.activeListIdx)
        );
        ImGui.Separator();
        // begin table
        using (var table = ImRaii.Table("Info And Perms Toybox", 2, ImGuiTableFlags.None)) {
            if (!table) { return; }
            // set up columns
            var width = ImGui.GetContentRegionAvail().X;
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthFixed, width - 125*ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("VibratorSlider", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextColumn();
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            // now we need to draw out the server setup and connectivity
            var portValue  = _tempPortValue ?? _config.intifacePortValue;
            // if port value is empty, set it to the default
            if(portValue.IsNullOrEmpty()) {
                portValue = "ws://localhost:12345";
            }
            ImGui.SetNextItemWidth(250*ImGuiHelpers.GlobalScale);
            if (ImGui.InputTextWithHint($"##Connector Address", "Sets the intiface server address.", ref portValue, 128, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                _tempPortValue = portValue;
            }
            if (ImGui.IsItemDeactivatedAfterEdit()) {
                _config.SetIntifacePortValue(portValue);
                _tempPortValue = null;
            }
            if( ImGui.IsItemHovered()) {
                ImGui.SetTooltip("The address of the intiface server to connect to.\n"+
                "The default address is ws://localhost:12345\n"+
                "Clear input field to automatically reset it to this.");
            }

            // depending on what we select here, the options below it will change
            var text = _charHandler.playerChar._usingSimulatedVibe ? "Using Simulated Vibrator" : "Using Lovenese / Connected Toy";
            if(ImGuiUtil.DrawDisabledButton(text, new Vector2(250*ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight()),
            _charHandler.playerChar._usingSimulatedVibe ? "You are using a simulated vibrator.\nClick to switch to using a lovense Toy"
            : "You are using a lovense / connected toy.\nClick to switch to using a simulated vibrator", false))
            {
                _charHandler.ToggleUsingSimulatedVibe();
            }
            
            // draw out remaining buttons based on the type of toy we are using
            if(_charHandler.playerChar._usingSimulatedVibe) {
                DrawSimulatedVibeSettings();
            }
            else {
                DrawConnectedToySettings();
            }

            // update the sound player if the simulated vibe type has changed
            if((!_charHandler.playerChar._isToyActive || !_charHandler.playerChar._usingSimulatedVibe) && _soundPlayer.isPlaying) {
                _soundPlayer.Stop();
            }
            // conditions required to play the simulated vibe
            if(_charHandler.playerChar._usingSimulatedVibe && _charHandler.playerChar._isToyActive && !_soundPlayer.isPlaying) {
                _soundPlayer.Play();
                _soundPlayer.SetVolume((float)(_charHandler.playerChar._intensityLevel/(double)(_plugService.stepCount == 0 ? 20 : _plugService.stepCount)));
            }

            // display the connected toy info here:
            ImGui.Separator();
            // draw out the info about the currently active device
            try{
                ImGui.PushFont(_fontService.UidFont);
                if(!_plugService.anyDeviceConnected) { 
                    ImGui.Text("No Device Connected!");
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                    ImGui.TextWrapped("If any device is connected, its information will be shown here");
                    ImGui.PopStyleColor();
                }
                else {
                    #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if(_plugService.activeDevice.DisplayName.IsNullOrEmpty()) {
                        ImGui.Text($"{_plugService.activeDevice.Name} Connected");
                    }
                    else {
                        ImGui.Text($"{_plugService.activeDevice.DisplayName} Connected");
                    }
                    #pragma warning restore CS8602 // Dereference of a possibly null reference.
                    if (_plugService.activeDevice != null) {
                        string displayName = string.IsNullOrEmpty(_plugService.activeDevice.DisplayName) ? _plugService.activeDevice.Name : _plugService.activeDevice.DisplayName;
                        ImGui.Text($"Name: {displayName}");
                        ImGui.Text($"Step Size: {100/(100*_plugService.stepInterval)}");
                        ImGui.Text($"Step Interval: {_plugService.stepInterval}");
                        ImGui.Text($"Battery Level: {(int)(_plugService.batteryLevel*100)}%%");
                    }
                }
            } catch (System.Exception e) {
                GSLogger.LogType.Debug($"{e.Message} Error drawing the setup and info subtab");
            } finally {
                ImGui.PopFont();
            }
            // go to the next column
            ImGui.TableNextColumn();
            // draw out the vibe intensity slider
            width = ImGui.GetContentRegionAvail().X;
            DrawVibeIntensitySlider(width);
        }
        // pop the spacing style so our main window can use still be merged together
        ImGui.PopStyleVar();
    }

    private void DrawVibeIntensitySlider(float width) {
        // push the style colors
        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.602f, 0.283f, 0.448f, 0.540f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.602f, 0.283f, 0.448f, 0.640f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new Vector4(0.602f, 0.283f, 0.448f, 0.740f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, new Vector4(0.955f, .289f, .687f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, new Vector4(0.964f, 0.392f, 0.765f, 1.0f));
        if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.BeginDisabled(); }
        try{
            // draw out the slider here
            width = 75*ImGuiHelpers.GlobalScale;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 25*ImGuiHelpers.GlobalScale);
            int maxVal = _plugService.stepCount == 0 ? 20 : _plugService.stepCount;
            int intensityResult = _charHandler.playerChar._intensityLevel;
            if(ImGui.VSliderInt("##VertSliderToy", new Vector2(width,ImGui.GetContentRegionAvail().Y), ref intensityResult, 0, maxVal)) {
                //  (byte)(intensityResult*_plugService.stepCount); formats it back into the same value stored by patterns for fast calculations
                _charHandler.UpdateIntensityLevel(intensityResult);
                //GSLogger.LogType.Debug($"[Toybox Overview Panel] Intensity Level: {_charHandler.playerChar._intensityLevel}");
                // update the intensity on our device if it is set to active
                if(_tempSliderValue != intensityResult) {
                    _tempSliderValue = intensityResult;
                    // send the intensity to the device, if it is active
                    if(_charHandler.playerChar._isToyActive && _plugService.activeDevice != null) {
                        _ = _plugService.ToyboxVibrateAsync((byte)((intensityResult/(double)maxVal)*100), 20);
                    }
                    // update our simulated toy, if active
                    if(_charHandler.playerChar._isToyActive && _charHandler.playerChar._usingSimulatedVibe) {
                        _soundPlayer.SetVolume((float)(intensityResult/(double)maxVal));
                    }
                }
            }
        } finally {
            ImGui.PopStyleColor(5);
            if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.EndDisabled(); }
        }
    }

    private void DrawSimulatedVibeSettings() {
        // Draw out a combo that stores the options of each of the checkboxes above to select the simulated vibe sound type
    
        ImGui.SetNextItemWidth(250*ImGuiHelpers.GlobalScale);
        if(ImGui.Combo("##SimulatedVibeType", ref _simulatedVibeType, new string[] { "Play Quiet Sounding Vibrator", "Play Loud Sounding Vibrator" }, 2)) {
            // set the vibe type based on the selection
            if(_simulatedVibeType == 0) {
                SwitchToQuietVibe();
            } else {
                GSLogger.LogType.Debug($"[Toybox Overview Panel] Switching to Loud Vibe");
                SwitchToLoudVibe();
            }
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Select the type of simulated vibrator sound to play when the intensity is adjusted."); }
    }

    private void SwitchToQuietVibe() {
        _simulatedVibeType = 0;
        _soundPlayer.ChangeAudioPath("vibratorQuiet.wav");
        _soundPlayer.SetVolume((float)(_charHandler.playerChar._intensityLevel/(double)(_plugService.stepCount == 0 ? 20 : _plugService.stepCount)));
    }

    private void SwitchToLoudVibe() {
        _simulatedVibeType = 1;
        _soundPlayer.ChangeAudioPath("vibrator.wav");
        _soundPlayer.SetVolume((float)(_charHandler.playerChar._intensityLevel/(double)(_plugService.stepCount == 0 ? 20 : _plugService.stepCount)));
    }

    private void DrawConnectedToySettings() {
        // we are drawing out the lovense toy settings, so add the dropdown the for the active toy selection
        try{
            // because it is possible for the service to fail, everything should be in a trycatch loop here
            ImGui.SetNextItemWidth(250*ImGuiHelpers.GlobalScale);
            if (_plugService.client.Devices.Any()) {
                // if our device list is not empty but our device index is not set, set it to the first device
                if(_plugService.deviceIndex == -1) {
                    _plugService.deviceIndex = 0;
                }
                // draw the combo
                if (ImGui.Combo("##DeviceList", ref _plugService.deviceIndex,
                _plugService.client.Devices.Select(d => d.Name).ToArray(), _plugService.client.Devices.Count())) {
                    // if the combo list is changed, we should update the active device
                    _activeDeviceChangedEvent.Invoke(_plugService.deviceIndex);
                }
            } else {
                var _simulatedVibeType2 = 0;
                ImGui.Combo("##SimulatedVibeType", ref _simulatedVibeType2, new string[] { "No Device Currently Connected!" }, 1);
            }
            // next row, we need to draw out the two buttons for starting and stopping scanning
            var buttonWidth = new Vector2(124*ImGuiHelpers.GlobalScale, ImGui.GetFrameHeight());
            if (ImGuiUtil.DrawDisabledButton("Start Scanning", buttonWidth, "Begins scanning for any toys looking to connect to client",
            _plugService.isScanning || !_plugService.IsClientConnected())) {
                // begin scanning for devices
                _ = _plugService.StartScanForDevicesAsync().ContinueWith(t => 
                {
                    if (t.Exception != null) {
                        GSLogger.LogType.Error($"[Toybox Overview Panel] Error starting scan for devices: {t.Exception}");
                    }
                });
            }
            ImGui.SameLine();
            if (ImGuiUtil.DrawDisabledButton("Stop Scanning", buttonWidth, "Stops scanning for any toys looking to connect to client",
            !_plugService.isScanning || !_plugService.IsClientConnected())) {
                // stop scanning for devices
                _ = _plugService.StopScanForDevicesAsync().ContinueWith(t => 
                {
                    if (t.Exception != null) {
                        GSLogger.LogType.Error($"[Toybox Overview Panel] Error stopping scan for devices: {t.Exception}");
                    }
                });
            }
        } catch (Exception ex) {
            GSLogger.LogType.Error($"[Toybox Overview Panel] Error generating a list from connected devices {ex.ToString()}");
        }
    }
}