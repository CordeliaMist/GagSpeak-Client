using System;
using System.Linq;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using Dalamud.Utility;
using GagSpeak.Events;
using GagSpeak.Gagsandlocks;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class ToyboxOverviewPanel
{
    private readonly    PlugService _plugService; // for getting the plug service
    private readonly    FontService _fontService; // for getting the font
    private readonly    ToyboxPatternTable _patternTable; // for getting the pattern table
    private readonly    PatternPlayback _patternPlayback; // for getting the pattern playback
    private readonly    CharacterHandler _charHandler; // for getting the whitelist
    private readonly    PatternHandler _patternCollection; // for getting the patterns
    private             Stopwatch _recordingStopwatch; // tracking how long a pattern was executing for
    private readonly    ActiveDeviceChangedEvent _activeDeviceChangedEvent; // for getting the active device
    private             bool _playingPattern; // for checking if we are recording
    private             int? _tempSliderValue;
    public ToyboxOverviewPanel(FontService fontService, CharacterHandler characterHandler, PlugService plugService, PatternPlayback patternPlayback,
    ActiveDeviceChangedEvent activeDeviceChangedEvent, PatternHandler patternCollection, ToyboxPatternTable patternTable) {
        _fontService = fontService;
        _charHandler = characterHandler;
        _patternCollection = patternCollection;
        _plugService = plugService;
        _patternTable = patternTable;
        _activeDeviceChangedEvent = activeDeviceChangedEvent;
        _patternPlayback = patternPlayback;

        _playingPattern = false;
        _tempSliderValue = 0;

        if(_plugService == null) {
            throw new ArgumentNullException(nameof(plugService));
        }

        _recordingStopwatch = new Stopwatch();
    }

    public void Draw() {
        using (_ = ImRaii.Group()) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
            DrawToyboxOverviewPanel();
            DrawToyboxButtonRow();
        }
    }

    private void DrawToyboxOverviewPanel() {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing)
                            .Push(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        using var child = ImRaii.Child("ToyboxOverviewPanelChild", new Vector2(ImGui.GetContentRegionAvail().X, -ImGui.GetFrameHeight()-ImGuiHelpers.GlobalScale), true);
        if (!child) { return;}
        var xPos = ImGui.GetCursorPosX();
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 5*ImGuiHelpers.GlobalScale);
        // create two columns
        var width = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("Disconnectedm").X;
        ImGui.Columns(2, "PermissionSetters and Connection Buttons", false);
        ImGui.SetColumnWidth(0, width);
        // draw out the global settings
        ImGui.AlignTextToFramePadding();
        UIHelpers.CheckboxNoConfig("Intensity Control", 
        "Determines if people are allowed to adjust the intensity of your vibe while connected and active.",
        _charHandler.playerChar._allowIntensityControl[_charHandler.activeListIdx],
        v => _charHandler.ToggleAllowIntensityControl(_charHandler.activeListIdx)
        );
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("Toybox UI Locking",
        "Determines if people are allowed to lock the toybox UI. Tiers 3+ Can override this setting.",
        _charHandler.playerChar._allowToyboxLocking,
        v => _charHandler.ToggleToyboxUILocking()
        );
        // draw out the individual settings
        UIHelpers.CheckboxNoConfig("Change Toy State",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} is able to enable / disable your toy",
        _charHandler.playerChar._allowChangingToyState[_charHandler.activeListIdx],
        v => _charHandler.ToggleChangeToyState(_charHandler.activeListIdx)
        );
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("Executing Patterns",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} is able to execute your stored patterns on your toy",
        _charHandler.playerChar._allowUsingPatterns[_charHandler.activeListIdx],
        v => _charHandler.ToggleAllowPatternExecution(_charHandler.activeListIdx)
        );
        // go over to the next column
        ImGui.NextColumn();
        // get the remaining width
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 2*ImGuiHelpers.GlobalScale);
        var width2 = ImGui.GetContentRegionAvail().X;
        // draw out the connect button
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if(ImGuiUtil.DrawDisabledButton("Connect", new Vector2(width2, 20*ImGuiHelpers.GlobalScale),
        "Attempts to connect to the Intiface server", _plugService.IsClientConnected())){
            // attempt to connect to the server
            _plugService.ConnectToServerAsync();
        }
        // and disconnect button
        if(ImGuiUtil.DrawDisabledButton("Disconnect", new Vector2(width2, 20*ImGuiHelpers.GlobalScale),
        "disconnects from the Intiface server", !_plugService.IsClientConnected())) {
            // attempt to disconnect from the server
            _plugService.DisconnectAsync();
        }
        // pop off the colors we pushed
        ImGui.PopStyleColor(3);
        // go to the next row
        ImGui.TableNextColumn();
        ImGui.Columns(1);
        // draw the separator
        ImGui.Separator();
        // now we can draw out a table 
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos - 4*ImGuiHelpers.GlobalScale);
        using (var InfoPatTable = ImRaii.Table("InfoAndPatterns", 2, ImGuiTableFlags.NoPadInnerX | ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.BordersV, new Vector2(-1, -90))) {
            if (!InfoPatTable) { return; }
            // Create the headers for the table;
            ImGui.TableSetupColumn("Plug and Pattern Info", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("PatternList", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("PatternListingsmmmmmmm").X);
            // and print the current plug name
            ImGui.TableNextColumn();
            // within this cell, restore the padding
            // check if the device exists
            try{
                xPos = ImGui.GetCursorPosX();
                yPos = ImGui.GetCursorPosY();
                ImGui.SetCursorPos(new Vector2(xPos+5*ImGuiHelpers.GlobalScale, yPos - 5*ImGuiHelpers.GlobalScale));
                if(!_plugService.anyDeviceConnected) { 
                    DisplayText("No Device Connected!");
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 110*ImGuiHelpers.GlobalScale);
                }
                else {
                    #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if(_plugService.activeDevice.DisplayName.IsNullOrEmpty()) {
                        DisplayText($"{_plugService.activeDevice.Name} Connected");
                    }
                    else {
                        DisplayText($"{_plugService.activeDevice.DisplayName} Connected");
                    }
                    #pragma warning restore CS8602 // Dereference of a possibly null reference.
                    // print all the juicy info about your currently active toy
                    width = ImGui.GetContentRegionAvail().X;
                    ImGui.Columns(2, "ToyInfo", false);
                    ImGui.SetColumnWidth(0, width*0.7f);
                    if (_plugService.activeDevice != null)
                    {
                        
                        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale, ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale));
                        ImGui.PushStyleColor(ImGuiCol.Text, lushPinkLine);
                        ImGui.Text($"Name: {_plugService.activeDevice.Name}");
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                        ImGui.Text($"Display Name: {_plugService.activeDevice.DisplayName}");
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                        ImGui.Text($"Message Timing Gap: {_plugService.activeDevice.MessageTimingGap}");
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                        ImGui.Text($"Step Size: {100/(100*_plugService.stepInterval)}");
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                        ImGui.Text($"Step Interval: {_plugService.stepInterval}");
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 3*ImGuiHelpers.GlobalScale);
                        ImGui.Text($"Battery?: {_plugService.activeDevice.HasBattery}");
                        ImGui.PopStyleColor();
                    }
                    ImGui.NextColumn();
                    // draw out the slider here
                    width = width*0.25f;
                    int maxVal = _plugService.stepCount;
                    int intensityResult = _charHandler.playerChar._intensityLevel;
                    if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.BeginDisabled(); }
                    if(ImGui.VSliderInt("##VertSliderToy", new Vector2(width,ImGuiHelpers.GlobalScale*120), ref intensityResult, 0, maxVal)) {
                        //  (byte)(intensityResult*_plugService.stepCount); formats it back into the same value stored by patterns for fast calculations
                        _charHandler.playerChar._intensityLevel = intensityResult;
                        //GagSpeak.Log.Debug($"[Toybox Overview Panel] Intensity Level: {_charHandler.playerChar._intensityLevel}");
                        // update the intensity on our device if it is set to active
                        if(_plugService.activeDevice != null && _tempSliderValue != intensityResult) {
                            _tempSliderValue = intensityResult;
                            // send the intensity to the device
                            _ = _plugService.ToyboxVibrateAsync((byte)((intensityResult/(double)maxVal)*100), 20);
                        }
                    }
                    if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.EndDisabled(); }
                    ImGui.Columns(1);
                }
                // draw info of selected Pattern, if one is selected
                if (_patternCollection._activePatternIndex >=0) {
                    if(!_plugService.anyDeviceConnected) {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGuiHelpers.GlobalScale);
                    } else {
                        ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX(), ImGui.GetCursorPosY() - 15*ImGuiHelpers.GlobalScale));
                    }
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5*ImGuiHelpers.GlobalScale);
                    DisplayText($"{_patternCollection._patterns[_patternCollection._activePatternIndex]._name} : ID {_patternCollection._activePatternIndex}");
                    // display the description, the duration, and if it is running stuff
                    ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + 5*ImGuiHelpers.GlobalScale, ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale));
                    ImGui.Text($"{_patternCollection._patterns[_patternCollection._activePatternIndex]._description}");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5*ImGuiHelpers.GlobalScale);
                    ImGui.Text($"Length: {_patternCollection._patterns[_patternCollection._activePatternIndex]._duration}");
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5*ImGuiHelpers.GlobalScale);
                    string isRunningText = _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive ? "Running" : "Not Running";
                    ImGui.Text($"{isRunningText}");
                    // now draw out the button for starting/stopping a pattern, and a checkbox for if we should loop it or not
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 5*ImGuiHelpers.GlobalScale);
                    width = ImGui.GetContentRegionAvail().X;
                    if (ImGui.Button("Start / Stop", new Vector2(width/3, 22*ImGuiHelpers.GlobalScale))) {
                        if (!_patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) {
                            _patternCollection.ExecutePatternProper();
                        } else {
                            _patternCollection.StopPattern();
                        }
                    }
                    ImGui.SameLine();
                    bool loop = _patternCollection._patterns[_patternCollection._activePatternIndex]._loop;
                    if (ImGui.Checkbox("Loop", ref loop)) {
                        _patternCollection._patterns[_patternCollection._activePatternIndex]._loop = loop;
                    }
                    ImGui.SameLine();
                    // display the current running time from the patternplayback class
                    ImGuiUtil.Center($"{_patternPlayback._recordingStopwatch.Elapsed.ToString(@"mm\:ss")}");
                }
            } catch (Exception ex) {
                GagSpeak.Log.Error($"[Toybox Overview Panel] Error in Async: {ex.ToString()}");
            }
            // go to the next row
            ImGui.TableNextColumn();
            if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.BeginDisabled(); }
            // draw the pattern list
            _patternTable.Draw();
            if(_patternCollection.GetActiveIdx() != -1 && _patternCollection._patterns[_patternCollection._activePatternIndex]._isActive) { ImGui.EndDisabled(); }
        } // table ends here
        style.Pop(2);
        // draw the pattern playback sampler here
        _patternPlayback.Draw();
    }
    public Vector4 lushPinkLine = new Vector4(.806f, .102f, .407f, 1);
    public Vector4 lushPinkButton = new Vector4(1, .051f, .462f, 1);

    private void DisplayText(string text) {
        ImGui.PushFont(_fontService.UidFont);
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{text}");
        ImGui.PopFont();
    }

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
}
