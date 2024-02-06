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

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class ToyboxOverviewPanel
{
    private readonly    PlugService _plugService; // for getting the plug service
    private readonly    FontService _fontService; // for getting the font
    private readonly    ToyboxPatternTable _patternTable; // for getting the pattern table
    private readonly    CharacterHandler _charHandler; // for getting the whitelist
    private readonly    PatternHandler _patternCollection; // for getting the patterns
    private             Stopwatch _recordingStopwatch; // tracking how long a pattern was executing for
    private             bool _playingPattern = false; // for checking if we are recording
    public ToyboxOverviewPanel(FontService fontService, CharacterHandler characterHandler,
    PatternHandler patternCollection, PlugService plugService, ToyboxPatternTable patternTable) {
        _fontService = fontService;
        _charHandler = characterHandler;
        _patternCollection = patternCollection;
        _plugService = plugService;
        _patternTable = patternTable;

        if(_plugService == null) {
            throw new ArgumentNullException(nameof(plugService));
        }

        _recordingStopwatch = new Stopwatch();

    }

    public void Draw() {
        using var child = ImRaii.Child("ToyboxOverviewPanelChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child) { return;}
        DrawToyboxOverviewPanel();
    }

    private void DrawToyboxOverviewPanel() {
        // create two columns
        var width = ImGui.GetContentRegionAvail().X;
        ImGui.Columns(2, "PermissionSetters and Connection Buttons", false);
        ImGui.SetColumnWidth(0, width*0.8f);
        // draw out the global settings
        ImGui.AlignTextToFramePadding();
        CharacterSavingCheckbox("Intensity Control", 
        "Determines if people are allowed to adjust the intensity of your vibe while connected and active.",
        _charHandler.playerChar._allowIntensityControl, v => _charHandler.playerChar._allowIntensityControl = v, _charHandler);
        ImGui.SameLine();
        CharacterSavingCheckbox("Toybox UI Locking",
        "Determines if people are allowed to lock the toybox UI. Tiers 3+ Can override this setting.",
        _charHandler.playerChar._allowToyboxLocking, v => _charHandler.playerChar._allowToyboxLocking = v, _charHandler);
        // draw out the individual settings
        CharacterSavingCheckbox("Change Toy State",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} is able to enable / disable your toy",
        _charHandler.playerChar._allowChangingToyState[_charHandler.activeListIdx], v => _charHandler.playerChar._allowChangingToyState[_charHandler.activeListIdx] = v,
        _charHandler);
        ImGui.SameLine();
        CharacterSavingCheckbox("Executing Patterns",
        $"If {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]} is able to execute your stored patterns on your toy",
        _charHandler.playerChar._allowUsingPatterns[_charHandler.activeListIdx], v => _charHandler.playerChar._allowUsingPatterns[_charHandler.activeListIdx] = v,
        _charHandler);
        // go over to the next column
        ImGui.NextColumn();
        // get the remaining width
        var width2 = ImGui.GetContentRegionAvail().X;
        // draw out the connect button
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        if(ImGuiUtil.DrawDisabledButton("Connect", new Vector2(width2, 20*ImGuiHelpers.GlobalScale),
        "Attempts to connect to the Intiface server", _plugService.IsClientConnected())){
            // attempt to connect to the server
            _plugService.ConnectToServerAsync();
            if(_plugService.IsClientConnected()) {
                GagSpeak.Log.Debug("[Toybox Overview Panel] Successfully connected to the Intiface server");
            } else {
                GagSpeak.Log.Debug("[Toybox Overview Panel] Failed to connect to the Intiface server");
            }
        }
        // and disconnect button
        if(ImGuiUtil.DrawDisabledButton("Disconnect", new Vector2(width2, 20*ImGuiHelpers.GlobalScale),
        "disconnects from the Intiface server", !_plugService.IsClientConnected())) {
            // attempt to disconnect from the server
            _plugService.DisconnectAsync();
            if(!_plugService.IsClientConnected()) {
                GagSpeak.Log.Debug("[Toybox Overview Panel] Successfully disconnected from the Intiface server");
            } else {
                GagSpeak.Log.Debug("[Toybox Overview Panel] Failed to disconnect from the Intiface server");
            }
        }
        // pop off the colors we pushed
        ImGui.PopStyleColor(3);
        // go to the next row
        ImGui.TableNextColumn();
        ImGui.Columns(1);
        // draw the separator
        ImGui.Separator();
        // now we can draw out a table 
        using (var InfoPatTable = ImRaii.Table("InfoAndPatterns", 2, ImGuiTableFlags.None, new Vector2(width, -ImGui.GetFrameHeight()))) {
            if (!InfoPatTable) { return; }
            // Create the headers for the table
            ImGui.TableSetupColumn("Plug and Pattern Info", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("PatternList", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("PatternListingsmmmmm").X);
            // go to the next row
            // check if the device exists
            try{
                // and print the current plug name
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                var yPos = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(yPos - 5*ImGuiHelpers.GlobalScale);
                ImGui.PushFont(_fontService.UidFont);
                if(!_plugService.HasConnectedDevice()) { 
                    ImGui.Text("No Device Connected!");
                }
                else 
                {
                    // draw out the name of the device
                    #pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if(_plugService._activeDevice.DisplayName.IsNullOrEmpty()) {
                        ImGui.Text($"{_plugService._activeDevice.Name} Connected");
                    }
                    else {
                        ImGui.Text($"{_plugService._activeDevice.DisplayName} Connected");
                    }
                    #pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                ImGui.PopFont();
                // print all the juicy into to figure out how this service works
                // Print ButtplugClient details
                ImGui.Text($"Client Name: {_plugService._client.Name}");
                ImGui.Text($"Connected: {_plugService._client.Connected}");
                if(_plugService.IsClientConnected() && _plugService.HasConnectedDevice()) {
                    ImGui.Text($"Devices: {string.Join(", ", _plugService._client.Devices.Select(d => d.Name))}");
                } else {
                    ImGui.Text($"Devices: No Devices Connected");
                }
                // Print ButtplugClientDevice details if a device is connected
                if (_plugService._activeDevice != null)
                {
                    ImGui.Text($"Device Index: {_plugService._activeDevice.Index}");
                    ImGui.Text($"Device Name: {_plugService._activeDevice.Name}");
                    ImGui.Text($"Device Display Name: {_plugService._activeDevice.DisplayName}");
                    ImGui.Text($"Message Timing Gap: {_plugService._activeDevice.MessageTimingGap}");
                    ImGui.Text($"ActiveToy's Step Size: {_plugService._stepSize}");
                    ImGui.Text($"Has Battery: {_plugService._activeDevice.HasBattery}");
                    ImGui.TextWrapped($"Vibrate Attributes: {string.Join(", ", _plugService._activeDevice.VibrateAttributes.Select(a => a.ActuatorType.ToString()))}");
                    ImGui.Text($"Oscillate Attributes: {string.Join(", ", _plugService._activeDevice.OscillateAttributes.Select(a => a.ActuatorType.ToString()))}");
                    ImGui.Text($"Rotate Attributes: {string.Join(", ", _plugService._activeDevice.RotateAttributes.Select(a => a.ActuatorType.ToString()))}");
                    ImGui.Text($"Linear Attributes: {string.Join(", ", _plugService._activeDevice.LinearAttributes.Select(a => a.ActuatorType.ToString()))}");
                }
            } catch (Exception ex) {
                GagSpeak.Log.Error($"[Toybox Overview Panel] Error in Async: {ex.ToString()}");
            }

            // go to the next row
            ImGui.TableNextColumn();
            // draw the pattern list

        } // table ends here
    }


    private void CharacterSavingCheckbox(string label, string tooltip, bool current, Action<bool> setter, CharacterHandler _charHandler) {
        using var id  = ImRaii.PushId(label);
        var       tmp = current;
        if (ImGui.Checkbox(string.Empty, ref tmp) && tmp != current) {
            setter(tmp);
            _charHandler.Save();
        }
        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker(label, tooltip);
    }
}
