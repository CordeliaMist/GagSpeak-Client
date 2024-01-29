using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui;
using OtterGui.Widgets;
using GagSpeak.UI.ComboListings;
using GagSpeak.Services;
using GagSpeak.Data;
using GagSpeak.Wardrobe;

namespace GagSpeak.UI.Tabs.GeneralTab;
/// <summary> This class is used to handle the general tab for the GagSpeak plugin. </summary>
public class GeneralTab : ITab, IDisposable
{
    private readonly GagSpeakConfig         _config;                    // the config for the plugin
    private readonly TimerService           _timerService;              // the timer service for the plugin
    private readonly GagListingsDrawer      _gagListingsDrawer;         // the drawer for the gag listings
    private readonly GagAndLockManager      _lockManager;               // the lock manager for the plugin
    private readonly GagService             _gagService;                // the gag manager for the plugin
    private          GagTypeFilterCombo[]   _gagTypeFilterCombo;        // create an array of item combos
    private          GagLockFilterCombo[]   _gagLockFilterCombo;        // create an array of item combos
    private          bool?                  _inDomMode;                 // lets us know if we are in dom mode or not
    private          string?                _tempSafeword;              // for initializing a temporary safeword for the text input field
    private          bool                   modeButtonsDisabled = false;// lets us know if the mode buttons are disabled or not
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneralTab"/> class.
    /// </summary>
    public GeneralTab(GagListingsDrawer gagListingsDrawer, GagSpeakConfig config,
    TimerService timerService, GagAndLockManager lockManager, GagService gagService) {
        _config = config;
        _timerService = timerService;
        _gagListingsDrawer = gagListingsDrawer;
        _lockManager = lockManager;
        _gagService = gagService;

        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_gagService, _config),
            new GagTypeFilterCombo(_gagService, _config),
            new GagTypeFilterCombo(_gagService, _config)
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
        => WindowHeader.Draw("Gag Selections / Inspector", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    /// <summary>
    /// This function draws the general tab contents
    /// </summary>
    private void DrawGeneral() {
        // let's start by drawing the outline for the container
        using var child = ImRaii.Child("GeneralTabPanel", -Vector2.One, true);
        // Let's create a table in this panel
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
            if(_config.SafewordUsed) { ImGui.BeginDisabled(); }

            // add variables for the safeword stuff
            var width = new Vector2(-1, 0);
            var safeword  = _tempSafeword ?? _config.Safeword; // temp storage to hold until we de-select the text input
            ImGui.SetNextItemWidth(width.X);
            if (ImGui.InputText("##Safeword", ref safeword, 30, ImGuiInputTextFlags.None))
                _tempSafeword = safeword;
            if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                _config.Safeword = safeword;
                _tempSafeword = null;
            }
            // draw the cooldown timer
            if(_config.SafewordUsed) { ImGui.EndDisabled(); }
            ImGui.TableNextColumn();
            if(_config.SafewordUsed) {
                ImGui.Text($"Safeword Cooldown: {_timerService.remainingTimes.GetValueOrDefault("SafewordUsed", "N/A")}");
            }
            // draw our our second row
            var mode = _inDomMode ?? _config.InDomMode;
            ImGuiUtil.DrawFrameColumn("Mode Selector");
            ImGui.TableNextColumn();
            width = new Vector2(ImGui.GetContentRegionAvail().X-10,0);
            // draw out our two buttons to set the modes. When the button labeled sub is pressed, it will switch isDomMode to false, and lock the interactability of the sub button.
            // when the button labeled dom is pressed, it will switch isDomMode to true, and lock the interactability of the dom button.
            if(modeButtonsDisabled) {
                ImGui.BeginDisabled();
            }
            if (mode == true) {
                // User is in Dom mode
                if(!modeButtonsDisabled) {ImGui.BeginDisabled();}
                if (ImGui.Button("Dominant", width/2)) {
                    // Dom mode is already active, do nothing or display a message
                }
                if(!modeButtonsDisabled) {ImGui.EndDisabled();}
                ImGui.SameLine();
                if (ImGui.Button("Submissive", width/2)) {
                    _inDomMode = false; // Switch to Sub mode
                    _config.InDomMode = false;
                    modeButtonsDisabled = true;
                    _timerService.StartTimer("RoleSwitchCooldown", "10m", 1000, () => DisableModeButtons());
                    _config.Save();
                }
            } else {
                // User is in Sub mode
                if (ImGui.Button("Dominant", width/2)) {
                    _inDomMode = true; // Switch to Dom mode
                    _config.InDomMode = true;
                    modeButtonsDisabled = true;
                    _timerService.StartTimer("RoleSwitchCooldown", "10m", 1000, () => DisableModeButtons());
                    _config.Save();
                }
                if(!modeButtonsDisabled) {ImGui.BeginDisabled();}
                ImGui.SameLine();
                if (ImGui.Button("Submissive", width/2)) {
                    // do nothing
                }
                if(!modeButtonsDisabled) {ImGui.EndDisabled();}
            }
            ImGui.TableNextColumn();
            if(modeButtonsDisabled) {
                ImGui.EndDisabled();
                ImGui.Text($"[{(_config.InDomMode? "Dom" : "Sub")}] Swap Cooldown: {_timerService.remainingTimes.GetValueOrDefault("RoleSwitchCooldown", "N/A")}");
            }
        } 
        // if we used our safeword
        if(_config.SafewordUsed) {
            ImGui.SameLine();
            // create a timer that executes whenever you use the safeword command. This blocks all actions for the next 5m
            ImGui.Text($"Safeword Used! Disabling All Actions! CD: {_timerService.remainingTimes.GetValueOrDefault("SafewordUsed", "N/A")}");
        }

        // disable this interactability if our safeword is on cooldown
        if(_config.SafewordUsed) { ImGui.BeginDisabled(); }
        
        // Now let's draw our 3 gag appliers
        _gagListingsDrawer.PrepareGagListDrawing(); // prepare our listings
        int width2 = (int)(ImGui.GetContentRegionAvail().X / 2);
        // draw our 3 gag listings
        int i = 0;
        foreach(var slot in Enumerable.Range(0, 3)) {
            _gagListingsDrawer.DrawGagAndLockListing(slot, _config, _gagTypeFilterCombo[slot], _gagLockFilterCombo[slot], slot, $"Gag Slot {slot + 1}", width2);
            // display timer here.
            if(_lockManager.IsLockedWithTimer(slot)) {
                ImGui.TextColored(new Vector4(1,1,0,0.5f), _timerService.GetRemainingTimeForPadlock(slot));
            }
            i++;
            if(i<3) { ImGui.NewLine(); }
        }

        // end of disabled stuff
        if(_config.SafewordUsed) { ImGui.EndDisabled(); }
    
        // let people know which gags are not working
        ImGui.Text("These Gags dont work yet! If you have any IRL, contact me to help fill in the data!");
        ImGui.TextColored(new Vector4(0,1,0,1), "Bit Gag Padded, Bone Gag, Bone Gag XL, Chopstick Gag, Dental Gag,\n"+
                                                "Harness Panel Gag, Hook Gag, Inflatable Hood, Latex/Leather Hoods, Plug Gag\n"+
                                                "Pump Gag, Sensory Deprivation Hood, Spider Gag, Tenticle Gag.");
    }

    /// <summary> This function disables the mode buttons after the cooldown is over. </summary>
    private void DisableModeButtons() {
        modeButtonsDisabled = false;
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
            if (timerName == $"{LockableType.FiveMinutesPadlock}_Identifier{i}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Minutes} Minutes, {remainingTime.Seconds}Seconds";
            } else if (timerName == $"{LockableType.TimerPasswordPadlock}_Identifier{i}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Hours} Hours, {remainingTime.Minutes} Minutes, {remainingTime.Seconds} Seconds";
            } else if (timerName == $"{LockableType.MistressTimerPadlock}_Identifier{i}") {
                _timerService.remainingTimes[timerName] = $"Time Remaining: {remainingTime.Days} Days, {remainingTime.Hours} Hours, {remainingTime.Minutes} Minutes, {remainingTime.Seconds} Seconds";
            }
        }
    }
}