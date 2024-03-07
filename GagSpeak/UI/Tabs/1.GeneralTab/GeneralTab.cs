using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui;
using OtterGui.Widgets;
using GagSpeak.Services;
using Dalamud.Interface.Utility;

using GagSpeak.Gagsandlocks;
using GagSpeak.CharacterData;
using GagSpeak.UI.Equipment;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace GagSpeak.UI.Tabs.GeneralTab;
/// <summary> This class is used to handle the general tab for the GagSpeak plugin. </summary>
public class GeneralTab : ITab, IDisposable
{
    private readonly    GagSpeakConfig          _config;                    // the config for the plugin
    private readonly    GagSpeakChangelog       _changelog;
    private readonly    CharacterHandler        _characterHandler;          // the character handler for the plugin
    private readonly    TimerService            _timerService;              // the timer service for the plugin
    private readonly    GagListingsDrawer       _gagListingsDrawer;         // the drawer for the gag listings
    private readonly    GagAndLockManager       _lockManager;               // the lock manager for the plugin
    private readonly    GagService              _gagService;                // the gag manager for the plugin
    private             GagTypeFilterCombo[]    _gagTypeFilterCombo;        // create an array of item combos
    private             GagLockFilterCombo[]    _gagLockFilterCombo;        // create an array of item combos
    private             string?                 _tempSafeword;              // for initializing a temporary safeword for the text input field
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralTab"/> class.
    /// </summary> 
    public GeneralTab(GagListingsDrawer gagListingsDrawer, GagSpeakConfig config, GagSpeakChangelog changelog,
    CharacterHandler characterHandler, TimerService timerService, GagAndLockManager lockManager, GagService gagService) {
        _config = config;
        _changelog = changelog;
        _characterHandler = characterHandler;
        _timerService = timerService;
        _gagListingsDrawer = gagListingsDrawer;
        _lockManager = lockManager;
        _gagService = gagService;

        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_gagService),
            new GagTypeFilterCombo(_gagService),
            new GagTypeFilterCombo(_gagService)
        };
        // draw out our gagpadlock filter combo listings
        _gagLockFilterCombo = new GagLockFilterCombo[] {
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config)
        };

        // Subscribe to timer events
        _timerService.RemainingTimeChanged += OnRemainingTimeChanged;
    }

    // store our current safeword
    public string _currentSafeword = string.Empty;
    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label => "General"u8;

    /// <summary>
    /// This function is called when the tab is disposed.
    /// </summary>
    public void Dispose() { 
        // Unsubscribe from timer events
        _timerService.RemainingTimeChanged -= OnRemainingTimeChanged;
    }

    /// <summary>
    /// This Function draws the content for the window of the General Tab
    /// </summary>
    public void DrawContent() {
        // Definitely need to refine the ImGui code here, but this is a good start.
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;

        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("GeneralTabChild")) {
            DrawHeader();
            DrawGeneral();
        }
    }

    /// <summary>
    /// This function draws the header for the window of the General Tab
    /// </summary>
    private void DrawHeader()
        => WindowHeader.Draw("Gag Selections / Inspector", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, ImGui.GetContentRegionAvail().X-ImGuiHelpers.GlobalScale);


    private void DrawGeneral() {
        // let's start by drawing the outline for the container
        using var child = ImRaii.Child("GeneralTabPanel", -Vector2.One, true, ImGuiWindowFlags.NoScrollbar);
        // Let's create a table in this panel
        var xPos = ImGui.GetCursorPosX();
        var yPos = ImGui.GetCursorPosY();
        using (var table = ImRaii.Table("Main Declarations", 3)) {
            if(!table) { return; } // make sure our table was made
            // Identify our columns.
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Set Safewordm").X);
            ImGui.TableSetupColumn("Data", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("mmmmmmmmmmmmmmmmmm").X);
            ImGui.TableSetupColumn("Cooldowns", ImGuiTableColumnFlags.WidthStretch);

            // draw our our first row
            ImGuiUtil.DrawFrameColumn("Set Safeword");
            ImGui.TableNextColumn();
            // if the safeword was used, disable the section and show cooldown message
            if(_characterHandler.playerChar._safewordUsed) { ImGui.BeginDisabled(); }

            // add variables for the safeword stuff
            var width = new Vector2(-1, 0);
            var safeword  = _tempSafeword ?? _characterHandler.playerChar._safeword; // temp storage to hold until we de-select the text input
            ImGui.SetNextItemWidth(width.X);
            if (ImGui.InputText("##Safeword", ref safeword, 30, ImGuiInputTextFlags.None))
                _tempSafeword = safeword;
            if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                _characterHandler.playerChar._safeword = safeword;
                _tempSafeword = null;
            }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip("Can be triggered with /safeword (your safeword)\n[obviously without the ()]"); }
            // draw the cooldown timer
            if(_characterHandler.playerChar._safewordUsed) { ImGui.EndDisabled(); }
            ImGui.TableNextColumn();
            if(_characterHandler.playerChar._safewordUsed) {
                ImGui.Text($"CD:{_timerService.remainingTimes.GetValueOrDefault("SafewordUsed", "N/A")}");
            }
        } 
        // if we used our safeword
        if(_characterHandler.playerChar._safewordUsed) {
            ImGui.SameLine();
            // create a timer that executes whenever you use the safeword command. This blocks all actions for the next 5m
            ImGui.Text($"Safeword Used! Disabling All Actions! CD: {_timerService.remainingTimes.GetValueOrDefault("SafewordUsed", "N/A")}");
        }

        // disable this interactability if our safeword is on cooldown
        if(_characterHandler.playerChar._safewordUsed) { ImGui.BeginDisabled(); }
        ImGui.NewLine();
        // Now let's draw our 3 gag appliers
        _gagListingsDrawer.PrepareGagListDrawing(); // prepare our listings
        // draw our 3 gag listings
        int i = 0;
        foreach(var slot in Enumerable.Range(0, 3)) {
            _gagListingsDrawer.DrawGagAndLockListing(slot, _config, _gagTypeFilterCombo[slot], _gagLockFilterCombo[slot], slot, $"Gag Slot {slot + 1}");
            // display timer here.
            if(_lockManager.IsLockedWithTimer(slot)) {
                ImGui.TextColored(new Vector4(1,1,0,0.5f), _timerService.GetRemainingTimeForPadlock(slot));
            }
            i++;
            if(i<3) { ImGui.NewLine(); }
        }

        // end of disabled stuff
        if(_characterHandler.playerChar._safewordUsed) { ImGui.EndDisabled(); }
    
        // let people know which gags are not working
        ImGui.Text("These Gags dont work yet! If you have any IRL, contact me to help fill in the data!");
        ImGui.TextColored(new Vector4(0,1,0,1), "Bit Gag Padded, Bone Gag, Chopstick Gag, Dental Gag, Hook Gag,\n"+
                                                "Inflatable Hood, Plug Gag,Sensory Deprivation Hood, Spider Gag.");

        // Draw the changelog
        // before we go down, lets draw the changelog button on the top right
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - 5.25f * ImGui.GetFrameHeight(), yPos + ImGuiHelpers.GlobalScale));
        xPos = ImGui.GetCursorPosX();
        yPos = ImGui.GetCursorPosY();
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        ImGui.Text(" ");
        ImGui.SameLine();
        if (ImGui.Button("Changelog", new Vector2(5f * ImGui.GetFrameHeight(), ImGui.GetFrameHeight()))) {
            // force open the changelog here
            _changelog.Changelog.ForceOpen = true;
        }
        ImGui.SetCursorPos(new Vector2(xPos, yPos+30));
        ImGui.Text(" ");
        ImGui.SameLine();
        if (ImGui.Button("Short YT Guides", new Vector2(5f * ImGui.GetFrameHeight(), ImGui.GetFrameHeight()))) {
            ImGui.SetTooltip( "Only if you want to though!");
            Process.Start(new ProcessStartInfo {FileName = "https://www.youtube.com/playlist?list=PLGzKipCtkx7EAyk1k5gRFG8ZyKB0FMTR3", UseShellExecute = true});
        }
        ImGui.SetCursorPos(new Vector2(xPos, yPos+60));
        ImGui.Text(" ");
        ImGui.SameLine();
        if (ImGui.Button("Toss a Thanks!♥", new Vector2(5f * ImGui.GetFrameHeight(), ImGui.GetFrameHeight()))) {
            ImGui.SetTooltip( "Only if you want to though!");
            Process.Start(new ProcessStartInfo {FileName = "https://ko-fi.com/cordeliamist", UseShellExecute = true});
        }
        // pop off the colors we pushed
        ImGui.PopStyleColor(3);
    }

    /// <summary>
    /// outputs the remaining time on timers each time the millisecond elapsed time passes.
    /// <list type="bullet">
    /// <item><c>timerName</c><param name="timerName"> - The name of the timer.</param></item>
    /// <item><c>remainingTime</c><param name="remainingTime"> - The remaining time for the timer.</param></item>
    /// </list> </summary>
    private void OnRemainingTimeChanged(string timerName, TimeSpan remainingTime)
    {
        // update display of remaining time
        if(timerName == "RoleSwitchCooldown") {
            // Update the remaining time in the dictionary
            _timerService.remainingTimes[timerName] = $"{remainingTime.Minutes}m{remainingTime.Seconds}s";
        }
        if(timerName == "SafewordUsed") {
            // Update the remaining time in the dictionary
            _timerService.remainingTimes[timerName] = $"{remainingTime.Minutes}m{remainingTime.Seconds}s";
        }
        // update timer padlocks
        for (int i = 0; i < 3; i++) {
            if (timerName == $"{Padlocks.FiveMinutesPadlock}_Identifier{i}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Minutes} Minutes, {remainingTime.Seconds}Seconds";
            } else if (timerName == $"{Padlocks.TimerPasswordPadlock}_Identifier{i}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Hours} Hours, {remainingTime.Minutes} Minutes, {remainingTime.Seconds} Seconds";
            } else if (timerName == $"{Padlocks.MistressTimerPadlock}_Identifier{i}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Days} Days, {remainingTime.Hours} Hours, {remainingTime.Minutes} Minutes, {remainingTime.Seconds} Seconds";
            }
        }
    }
}