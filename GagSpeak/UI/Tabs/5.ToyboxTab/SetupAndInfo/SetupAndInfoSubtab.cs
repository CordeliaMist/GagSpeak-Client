using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using Dalamud.Utility;
using GagSpeak.Utility;
using System;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class SetupAndInfoSubtab : IDisposable
{
    private readonly    GagSpeakConfig      _config;
    private readonly    CharacterHandler    _charHandler;
    private readonly    PlugService         _plugService;
    private readonly    SoundPlayer         _soundPlayer;
    private readonly    PatternHandler      _patternCollection;
    private readonly    FontService _fontService; // for getting the font
    private             int? _tempSliderValue; // for storing the slider value
    private             string? _tempUri; // for storing the uri value
    private             bool _simulatedVibeType; // quiet or loud or none?

    public SetupAndInfoSubtab(GagSpeakConfig config, CharacterHandler charHandler, SoundPlayer soundPlayer,
    PlugService plugService, PatternHandler patternCollection, FontService fontService) {
        _config = config;
        _charHandler = charHandler;
        _plugService = plugService;
        _soundPlayer = soundPlayer;
        _patternCollection = patternCollection;
        _fontService = fontService;
        // setup values
        _tempSliderValue = 0;
        _tempUri = _config.intifaceUri != null ? _config.intifaceUri : "ws://localhost:12345";
        _simulatedVibeType = true;

        // start the sound player if it is not already started
        if(!_soundPlayer.isPlaying && _charHandler.playerChar._usingSimulatedVibe) {
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
        // allow the user to setup a custom uri for the intiface server
        var yPos = ImGui.GetCursorPosY();
        var xPos = ImGui.GetCursorPosX();
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Intiface Server Uri: ");
        ImGui.SameLine();
        // store the input text boxes trigger phrase
        var uri  = _tempUri ?? _config.intifaceUri;
        ImGui.SetNextItemWidth(100*ImGuiHelpers.GlobalScale);
        if (ImGui.InputText($"##Intiface Server Uri", ref uri, 10, ImGuiInputTextFlags.EnterReturnsTrue))
            _tempUri = uri;
        // will only update our safeword once we click away or enter is pressed
        if (ImGui.IsItemDeactivatedAfterEdit()) {
            _config.SetIntifaceUri(uri);
            _tempUri = null;
        }
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Select the intiface server you want to connect.\nws://localhost:12345 is the default used.");
        }
        // draw out the option for if player wants to use the simulated toy
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Simulated Toy: ");
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("",
        "play a simulated vibrator sound to your client when the toy would be otherwise active.",
        _charHandler.playerChar._usingSimulatedVibe,
        v => _charHandler.ToggleUsingSimulatedVibe()
        );
        ImGui.SetCursorPos(new Vector2(
            xPos+ImGui.GetContentRegionAvail().X-110*ImGuiHelpers.GlobalScale,
            yPos));
        xPos = ImGui.GetCursorPosX();
        yPos = ImGui.GetCursorPosY();
        // ask if they want to use the quiet vibe or the loud vibe
        UIHelpers.CheckboxNoConfig("Quiet Vibe",
        "play a quieter vibrator sound to your client when the toy would be otherwise active.",
        _charHandler.playerChar._usingSimulatedVibe && _simulatedVibeType,
        v => SwitchToQuietVibe()
        );
        ImGui.SetCursorPos(new Vector2(xPos, yPos+ImGui.GetFrameHeightWithSpacing()));
        // ask if they want the louder 
        UIHelpers.CheckboxNoConfig("Loud Vibe",
        "play a louder vibrator sound to your client when the toy would be otherwise active.",
        _charHandler.playerChar._usingSimulatedVibe && !_simulatedVibeType,
        v => SwitchToLoudVibe()
        );
        // update the sound player if the simulated vibe type has changed
        if(!_charHandler.playerChar._usingSimulatedVibe && _soundPlayer.isPlaying) {
            _soundPlayer.Stop();
        }
        if(_charHandler.playerChar._usingSimulatedVibe && !_soundPlayer.isPlaying) {
            _soundPlayer.Play();
        }
        ImGui.Separator();
        // draw out the info about the currently active device
        var width = ImGui.GetContentRegionAvail().X;
        ImGui.Columns(2, "ToyInfo", false);
        ImGui.SetColumnWidth(0, width*0.75f);
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
                if (_plugService.activeDevice != null)
                {
                    
                    ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale, ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale));
                    ImGui.Text($"Name: {_plugService.activeDevice.Name}");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                    ImGui.Text($"Disp. Name: {_plugService.activeDevice.DisplayName}");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                    ImGui.Text($"Step Size: {100/(100*_plugService.stepInterval)}");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                    ImGui.Text($"Step Interval: {_plugService.stepInterval}");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                    ImGui.Text($"Battery Level: {(int)(_plugService.batteryLevel*100)}%%");
                }
            }
        } catch (System.Exception e) {
            GagSpeak.Log.Debug($"{e.Message} Error drawing the setup and info subtab");
        } finally {
            ImGui.PopFont();
        }
        ImGui.NextColumn();
        // draw out the slider here
        width = width*0.2f;
        int maxVal = _plugService.stepCount == 0 ? 20 : _plugService.stepCount;
        int intensityResult = _charHandler.playerChar._intensityLevel;
        if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.BeginDisabled(); }
        if(ImGui.VSliderInt("##VertSliderToy", new Vector2(width,ImGuiHelpers.GlobalScale*220), ref intensityResult, 0, maxVal)) {
            //  (byte)(intensityResult*_plugService.stepCount); formats it back into the same value stored by patterns for fast calculations
            _charHandler.UpdateIntensityLevel(intensityResult);
            //GagSpeak.Log.Debug($"[Toybox Overview Panel] Intensity Level: {_charHandler.playerChar._intensityLevel}");
            // update the intensity on our device if it is set to active
            if(_tempSliderValue != intensityResult) {
                _tempSliderValue = intensityResult;
                // send the intensity to the device, if it is active
                if(_charHandler.playerChar._isToyActive && _plugService.activeDevice != null) {
                    _ = _plugService.ToyboxVibrateAsync((byte)((intensityResult/(double)maxVal)*100), 20);
                }
                // update our simulated toy, if active
                if(_charHandler.playerChar._usingSimulatedVibe) {
                    _soundPlayer.SetVolume((float)(intensityResult/(double)maxVal));
                }
            }
        }
        if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.EndDisabled(); }
        ImGui.Columns(1);
    }

    private void SwitchToQuietVibe() {
        _simulatedVibeType = true;
        _soundPlayer.ChangeAudioPath("vibratorQuiet.wav");
    }

    private void SwitchToLoudVibe() {
        _simulatedVibeType = false;
        _soundPlayer.ChangeAudioPath("vibrator.wav");
    }
}
