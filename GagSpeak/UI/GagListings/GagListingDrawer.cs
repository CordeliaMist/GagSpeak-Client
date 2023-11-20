using System.Numerics;
using ImGuiNET;
using System.Linq;
using OtterGui;
using OtterGui.Raii;
using Dalamud.Interface.Utility;
using System;
using GagSpeak.UI.Helpers;
using GagSpeak.Data;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Internal;
using System.Collections.Generic;

using GagSpeak.Events;
using GagSpeak.Services;
using Microsoft.VisualBasic.FileIO;
using Dalamud.Game.Config;


#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
namespace GagSpeak.UI.GagListings;
public class GagListingsDrawer : IDisposable
{
    IDalamudTextureWrap textureWrap1; // for image display
    IDalamudTextureWrap textureWrap2; // for image display
    IDalamudTextureWrap textureWrap3; // for image display
    IDalamudTextureWrap textureWrap4; // for image display
    IDalamudTextureWrap textureWrap5; // for image display
    IDalamudTextureWrap textureWrap6; // for image display

    private DalamudPluginInterface _pluginInterface;
    private TimerService _timerService;
    private readonly SafewordUsedEvent _safewordUsedEvent;
    private const float DefaultWidth = 280; // set the default width
    private readonly GagSpeakConfig _config;    
    private float _requiredComboWidthUnscaled;
    private float _requiredComboWidth;
    private string _buttonLabel = "";
    private string _passwordField = "";
    public bool[] _adjustDisp; // used to adjust the display of the password field
    
    public GagListingsDrawer(GagSpeakConfig config, DalamudPluginInterface dalamudPluginInterface, 
    TimerService timerService, SafewordUsedEvent safewordUsedEvent) // Constructor
    {
        _config = config;
        //update interface
        _pluginInterface = dalamudPluginInterface;
        _timerService = timerService;
        _safewordUsedEvent = safewordUsedEvent;

        // draw textures for the gag list
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagTypes[2]}.png"));
        // draw textures for the padlock list
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagPadlocks[2].ToString()}.png"));

        _adjustDisp = new bool[] {false, false, false};
        // Subscribe to the events
        _safewordUsedEvent.SafewordCommand += CleanupVariables;
        _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
        _config.selectedGagPadlocks.ItemChanged += OnSelectedTypesChanged;
    }

    private Vector2 _iconSize; // size
    private float _comboLength;

    public void Dispose() {
        _safewordUsedEvent.SafewordCommand -= CleanupVariables;
        _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
        _config.selectedGagPadlocks.ItemChanged -= OnSelectedTypesChanged;
    }

    // This function just prepares our styleformat for the drawing
    public void PrepareGagListDrawing() {
        // Draw out the content size of our icon
        _iconSize = new Vector2(2 * ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y);
        // Determine the size of our comboLength
        _comboLength = DefaultWidth * ImGuiHelpers.GlobalScale;
        // if the required combo with is unscaled
        if (_requiredComboWidthUnscaled == 0)
            _requiredComboWidthUnscaled = _config.GagTypes.Keys.Max(key => ImGui.CalcTextSize(key).X) / ImGuiHelpers.GlobalScale;
        // get the scaled combo width
        _requiredComboWidth = _requiredComboWidthUnscaled * ImGuiHelpers.GlobalScale;
    }

    // Draw the listings
    public void DrawGagAndLockListing(int ID, GagSpeakConfig config, GagTypeFilterCombo _gagTypeFilterCombo, GagLockFilterCombo _gagLockFilterCombo,
    int layerIndex, string displayLabel, int width) {
        // if we are locked, set the locked to true
        if(_config._isLocked[layerIndex]) {
            ImGui.BeginDisabled();
        }
        // push our styles
        using var    id = ImRaii.PushId($"{ID}_listing"); // push the ID
        var     spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y }; // push spacing
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing); // push style
    
        // draw our icon thingy
        // Setup our table
        ImGui.Columns(3,"Gag Listing", false);
        ImGui.SetColumnWidth(0, 85);
        try {
            switch(layerIndex){
                case 0:
                    ImGui.Image(textureWrap1.ImGuiHandle, new Vector2(80, 80));
                    break;
                case 1:
                    ImGui.Image(textureWrap2.ImGuiHandle, new Vector2(80, 80));
                    break;
                case 2:
                    ImGui.Image(textureWrap3.ImGuiHandle, new Vector2(80, 80));
                    break;
            }
        }
        catch (Exception e) {
            GagSpeak.Log.Error($"Failed to draw icon for slot {layerIndex} with gag type {config.selectedGagTypes[layerIndex]}");
            GagSpeak.Log.Error(e.ToString());
        }
          
        ImGui.NextColumn(); ImGui.SetColumnWidth(1, width+10); // Set the desired widths);
        // create a group for the 2 dropdowns and icon
        using (var group = ImRaii.Group()) {
            if(!_adjustDisp[layerIndex]){ // inch our way down half the distance of a newline
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetFrameHeight() / 1.4f);
            }
            
            // draw the combo boxes
            if (DrawGagTypeItemCombo(ID, config, layerIndex, _config._isLocked[layerIndex], width, _gagTypeFilterCombo)) {
                // do stuff
            }
            
            // Adjust the width of the padlock dropdown to 3/4 of the original width
            int newWidth = (int)(width * 0.75f);
            // draw the padlock dropdown
            if (DrawGagLockItemCombo(ID, config, layerIndex, _config._isLocked[layerIndex], newWidth, _gagLockFilterCombo, _config._padlockIdentifier[layerIndex])) {
                // do stuff
            }
            // end our disabled fields, if any, here
            if(_config._isLocked[layerIndex]) {
                ImGui.EndDisabled();
            }
            
            // get the type of button label that will display
            _buttonLabel = _config._isLocked[layerIndex] ? "Unlock" : "Lock"; // we want to display unlock button if we are currently locked
            ImGui.SameLine();
            if (ImGui.Button(_buttonLabel, new Vector2(-1, 0))) {
                // our button has been clicked while we were locked, so we are attempting to unlock
                if(_config._isLocked[layerIndex]) {
                    // attempt to validate the password when the button is pressed.
                    if(_config._padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config._isLocked[layerIndex])) {
                        // see if the passwords compare after they are valid
                        if(_config._padlockIdentifier[layerIndex].CheckPassword()) {
                            // at this point, our password is valid, so we can successfully unlock the padlock
                            _config._isLocked[layerIndex] = false;
                            _adjustDisp[layerIndex] = false;
                            GagSpeak.Log.Debug($"Padlock on slot {layerIndex} has been unlocked.");
                            _config._padlockIdentifier[layerIndex].ClearPasswords();
                            _config._padlockIdentifier[layerIndex].UpdateConfigPadlockPasswordInfo(layerIndex, !_config._isLocked[layerIndex], _config);
                            config.Save();
                        } 
                        else {     
                            GagSpeak.Log.Debug($"Password for Padlock is incorrect, try again.");
                        }
                    } 
                    else        
                        GagSpeak.Log.Debug($"Password for Padlock is incorrect, try again.");
                } 
                // our button has been clicked while we were unlocked, so attempt to lock
                else {
                    // attempt to validate the password when the button is pressed.
                    if(_config._padlockIdentifier[layerIndex].ValidatePadlockPasswords(_config._isLocked[layerIndex])) {
                        // at this point, our password is valid, so we can sucessfully lock the padlock
                        _config._padlockIdentifier[layerIndex].UpdateConfigPadlockPasswordInfo(layerIndex, _config._isLocked[layerIndex], _config);
                        _config._isLocked[layerIndex] = true;
                        GagSpeak.Log.Debug($"[GagListDrawer]: Padlock on slot {layerIndex} is now locked.");

                        if(_config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.FiveMinutesPadlock ||
                        _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.TimerPasswordPadlock ||
                        _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.MistressTimerPadlock) {
                            // create the timer.
                            _timerService.StartTimer($"{_config._padlockIdentifier[layerIndex]._padlockType}_Identifier{layerIndex}", _config._padlockIdentifier[layerIndex]._storedTimer, 
                            1000, () => {
                                // when the timer elapses, unlock padlock and clear data
                                _config._isLocked[layerIndex] = false;
                                _config._padlockIdentifier[layerIndex].ClearPasswords();
                                _config._padlockIdentifier[layerIndex].UpdateConfigPadlockPasswordInfo(layerIndex, !_config._isLocked[layerIndex], _config);
                            }, _config.selectedGagPadLockTimer, layerIndex);
                        }
                        config.Save();
                    }
                    else        
                        GagSpeak.Log.Debug($"Password for Padlock is incorrect, try again.");
                }
            }
            // Display the password fields based on the selected padlock type
            if(_config._padlockIdentifier[layerIndex].DisplayPasswordField(_config._padlockIdentifier[layerIndex]._padlockType)) {
                _adjustDisp[layerIndex] = true;
            } else {
                _adjustDisp[layerIndex] = false;
            }
            // display the remaining time if we have a timer for this and we are locked
            if(_config._isLocked[layerIndex] && 
            (_config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.FiveMinutesPadlock ||
            _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.MistressTimerPadlock ||
            _config._padlockIdentifier[layerIndex]._padlockType == GagPadlocks.TimerPasswordPadlock)) {
                _config.displaytext[layerIndex] = $"{_timerService.remainingTimes.GetValueOrDefault($"{_config._padlockIdentifier[layerIndex]._padlockType.ToString()}_Identifier{layerIndex}", "N/A")}]";
            }
        }
        ImGui.NextColumn();
        ImGui.SetColumnWidth(2, 80);
        if(config.selectedGagPadlocks[layerIndex] != GagPadlocks.None) {
            if(layerIndex==0) { ImGui.Image(textureWrap4.ImGuiHandle, new Vector2(80, 80)); }
            if(layerIndex==1) { ImGui.Image(textureWrap5.ImGuiHandle, new Vector2(80, 80)); }
            if(layerIndex==2) { ImGui.Image(textureWrap6.ImGuiHandle, new Vector2(80, 80)); }
        }
        // end our table
        ImGui.Columns(1);
    }

    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        GagSpeak.Log.Debug($"Item at index {e.Index} changed from '{e.OldValue}' to '{e.NewValue}'");
        // update the texture wraps
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagTypes[2]}.png"));
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagPadlocks[2].ToString()}.png"));
    }
    //
    // cleanup variables upon safeword
    private void CleanupVariables(object sender, SafewordCommandEventArgs e) {
        _passwordField = "";
        _config._isLocked = new List<bool> { false, false, false }; // reset is locked
        _adjustDisp = new bool[] {false, false, false}; // reset displays
        _config.TimerData.Clear(); // reset the timer data
        _timerService.ClearIdentifierTimers(); // and the associated timers timerdata reflected
        _config._padlockIdentifier = new List<PadlockIdentifier> { // new blank padlockidentifiers
            new PadlockIdentifier(),
            new PadlockIdentifier(),
            new PadlockIdentifier()
        };
        // reset the timers to current time
        _config.selectedGagPadLockTimer = new List<DateTimeOffset> {
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            DateTimeOffset.Now
        };


        // print out all of the padlock identifiers gagtypes, the drawers _config._isLocked, and the drawers _adjustDisp
        GagSpeak.Log.Debug($"PadlockIdentifier[0] = {_config._padlockIdentifier[0]._padlockType.ToString()}, _config._isLocked[0] = {_config._isLocked[0]}, _adjustDisp[0] = {_adjustDisp[0]}");
        GagSpeak.Log.Debug($"PadlockIdentifier[1] = {_config._padlockIdentifier[1]._padlockType.ToString()}, _config._isLocked[1] = {_config._isLocked[1]}, _adjustDisp[1] = {_adjustDisp[1]}");
        GagSpeak.Log.Debug($"PadlockIdentifier[2] = {_config._padlockIdentifier[2]._padlockType.ToString()}, _config._isLocked[2] = {_config._isLocked[2]}, _adjustDisp[2] = {_adjustDisp[2]}");
        // some dummy code to manually invoke the index change handler because im stupid.
        _config.selectedGagTypes[0] = _config.selectedGagTypes[0];
    }
    
    // update timer display

    // draw the gag item combo
    public bool DrawGagTypeItemCombo(int ID, GagSpeakConfig config, int layerIndex, bool locked, int width, GagTypeFilterCombo gagtypecombo) {
        var combo = gagtypecombo; // get the combo
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Type");
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        var dummy = "Dummy"; // used as filler for combos that dont need labels
        combo.Draw(ID, ref dummy, config.selectedGagTypes, layerIndex, width);

        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                config.selectedGagTypes[layerIndex] = _config.GagTypes.Keys.First(); // to the first option, none
                _config.Save();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }

    public bool DrawGagLockItemCombo(int ID, GagSpeakConfig config, int layerIndex, bool locked, int width, GagLockFilterCombo gaglockcombo, PadlockIdentifier selected) {
        var combo = gaglockcombo; // get the combo
        // if we left click and it is unlocked, open it
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Enum");
        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        combo.Draw(ID, config.selectedGagPadlocks, layerIndex, width, selected);
        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                config.selectedGagPadlocks[layerIndex]= GagPadlocks.None; // to the first option, none
                config.selectedGagPadlocksPassword[layerIndex] = "";
                config.selectedGagPadlocksAssigner[layerIndex] = "";
                config.Save();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }





    // for the whitelist page
    public bool DrawGagTypeItemCombo(int ID,  WhitelistCharData charData, ref string gagLabel, int layerIndex, bool locked, int width, GagTypeFilterCombo gagtypecombo) {
        var combo = gagtypecombo; // get the combo
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Type");
        using var disabled = ImRaii.Disabled(locked);
        combo.Draw(ID, ref gagLabel, charData.selectedGagTypes, layerIndex, width);
        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                gagLabel = _config.GagTypes.Keys.First();
                _config.Save();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }
    public bool DrawGagLockItemCombo(int ID, WhitelistCharData charData, ref string lockLabel, int layerIndex, bool locked, int width, GagLockFilterCombo gaglockcombo) {
        // This code is a shadow copy of the function above, used for accepting WhitelistCharData as a type
        var combo = gaglockcombo;
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Enum");
        using var disabled = ImRaii.Disabled(locked);
        combo.Draw(ID, ref lockLabel, charData.selectedGagPadlocks, layerIndex, width);
        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                lockLabel = "None";
                _config.Save();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }
}
#pragma warning restore IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention 