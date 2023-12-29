using System.Numerics;
using System;
using System.IO;
using System.Linq;
using Dalamud.Interface.Utility;
using Dalamud.Plugin;
using Dalamud.Interface.Internal;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using GagSpeak.Events;
using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.Data;

namespace GagSpeak.UI.GagListings;
/// <summary> This class is used to draw the gag listings. </summary>
public class GagListingsDrawer : IDisposable
{
    IDalamudTextureWrap textureWrap1; IDalamudTextureWrap textureWrap2; IDalamudTextureWrap textureWrap3; // for image display
    IDalamudTextureWrap textureWrap4; IDalamudTextureWrap textureWrap5; IDalamudTextureWrap textureWrap6; // for image display
    private             DalamudPluginInterface  _pluginInterface;               // used to get the plugin interface
    private             GagAndLockManager       _lockManager;                   // used to get the lock manager
    private             GagService              _gagService;                    // used to get the gag service
    private             TimerService            _timerService;                  // used to get the timer service
    private readonly    GagSpeakConfig          _config;                        // used to get the config
    private             float                   _requiredComboWidthUnscaled;    // used to determine the required width of the combo
    private             float                   _requiredComboWidth;            // used to determine the width of the combo
    private             string                  _buttonLabel = "";              // used to display the button label
    public              bool[]                  _adjustDisp;                    // used to adjust the display of the password field
    private             Vector2                 _iconSize;                      // size of the icon
    private             float                   _comboLength;                   // length of the combo
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GagListingsDrawer"/> class...
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>dalamudPluginInterface</c><param name="dalamudPluginInterface"> - The Dalamud plugin interface.</param></item>
    /// <item><c>timerService</c><param name="timerService"> - The timer service.</param></item>
    /// <item><c>lockManager</c><param name="lockManager"> - The lock manager.</param></item>
    /// </list> </summary>
    public GagListingsDrawer(GagSpeakConfig config, DalamudPluginInterface dalamudPluginInterface, 
    TimerService timerService, GagAndLockManager lockManager, GagService gagService)
    {
        _config = config;
        _pluginInterface = dalamudPluginInterface;
        _timerService = timerService;
        _lockManager = lockManager;
        _gagService = gagService;
        // draw textures for the gag and padlock listings //
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth\\{config.selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth\\{config.selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth\\{config.selectedGagTypes[2]}.png"));
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks\\{config.selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks\\{config.selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks\\{config.selectedGagPadlocks[2].ToString()}.png"));
        // initialize the adjust display
        _adjustDisp = new bool[] {false, false, false};
        // Subscribe to the events
        _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
        _config.selectedGagPadlocks.ItemChanged += OnSelectedTypesChanged;
    }

    /// <summary> Disposes of the <see cref="GagListingsDrawer"/> subscribed events, unsubscribing them. </summary>
    public void Dispose() {
        _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
        _config.selectedGagPadlocks.ItemChanged -= OnSelectedTypesChanged;
    }

    /// <summary> prepare the gag listing drawer by setting its width for the icon and combo. </summary>
    public void PrepareGagListDrawing() {
        // Draw out the content size of our icon
        _iconSize = new Vector2(2 * ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y);
        // Determine the size of our comboLength
        _comboLength = 280 * ImGuiHelpers.GlobalScale;
        // if the required combo with is unscaled
        if (_requiredComboWidthUnscaled == 0)
            _requiredComboWidthUnscaled = _gagService._gagTypes.Max(gag => ImGui.CalcTextSize(gag._gagName).X) / ImGuiHelpers.GlobalScale;
        // get the scaled combo width
        _requiredComboWidth = _requiredComboWidthUnscaled * ImGuiHelpers.GlobalScale;
    }

    /// <summary> 
    /// Draw the actual gag listing, this is the main function that is called to draw the gag listing.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>gagTypeFilterCombo</c><param name="gagTypeFilterCombo"> - The gag type filter combo.</param></item>
    /// <item><c>gagLockFilterCombo</c><param name="gagLockFilterCombo"> - The gag lock filter combo.</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>displayLabel</c><param name="displayLabel"> - The display label.</param></item>
    /// <item><c>width</c><param name="width"> - The width.</param></item>
    /// </list> </summary>
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
            // Draw the combos
            if (DrawGagTypeItemCombo(ID, config, layerIndex, _config._isLocked[layerIndex], width, _gagTypeFilterCombo)) {}
            // Adjust the width of the padlock dropdown to 3/4 of the original width
            int newWidth = (int)(width * 0.75f);
            if (DrawGagLockItemCombo(ID, config, layerIndex, _config._isLocked[layerIndex], newWidth, _gagLockFilterCombo)) {}
            // end our disabled fields, if any, here
            if(_config._isLocked[layerIndex]) { ImGui.EndDisabled(); } // end the disabled part here, if it was disabled
            
            // get the type of button label that will display
            _buttonLabel = _config._isLocked[layerIndex] ? "Unlock" : "Lock"; // we want to display unlock button if we are currently locked
            ImGui.SameLine();
            if (ImGui.Button(_buttonLabel, new Vector2(-1, 0))) {
                _lockManager.ToggleLock(layerIndex);
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
                _config.displaytext[layerIndex] = _timerService.GetRemainingTimeForPadlock(layerIndex);
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

    /// <summary>
    /// If at any point we have changed to a new item in the gag or padlock listing, we should update our image display.
    /// <list type="bullet">
    /// <item><c>sender</c><param name="sender"> - The sender.</param></item>
    /// <item><c>e</c><param name="e"> - The event arguments.</param></item>
    /// </list> </summary>
    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        // update the texture wraps
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth/{_config.selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth/{_config.selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth/{_config.selectedGagTypes[2]}.png"));
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks/{_config.selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks/{_config.selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks/{_config.selectedGagPadlocks[2].ToString()}.png"));
    }   

    /// <summary>
    /// Draw the combo listing of the gag types
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>gagTypeFilterCombo</c><param name="gagTypeFilterCombo"> - The gag type filter combo.</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>locked</c><param name="locked"> - The locked.</param></item>
    /// <item><c>width</c><param name="width"> - The width.</param></item>
    /// <item><c>gagtypecombo</c><param name="gagtypecombo"> - The gag type combo.</param></item>
    /// </list> </summary>
    /// <returns> True if it succeeds, false if it fails. </returns>
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
                config.selectedGagTypes[layerIndex] = _gagService._gagTypes.First()._gagName; // to the first option, none
                _config.Save();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }

    /// <summary>
    /// Draw the combo listing of the gag locks
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>gagLockFilterCombo</c><param name="gagLockFilterCombo"> - The gag lock filter combo.</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>locked</c><param name="locked"> - The locked.</param></item>
    /// <item><c>width</c><param name="width"> - The width.</param></item>
    /// <item><c>gaglockcombo</c><param name="gaglockcombo"> - The gag lock combo.</param></item>
    /// </list> </summary>
    /// <returns> True if it succeeds, false if it fails. </returns>
    public bool DrawGagLockItemCombo(int ID, GagSpeakConfig config, int layerIndex, bool locked, int width, GagLockFilterCombo gaglockcombo) {
        var combo = gaglockcombo; // get the combo
        // if we left click and it is unlocked, open it
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Enum");
        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        combo.Draw(ID, config.selectedGagPadlocks, layerIndex, width);
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

    /// <summary>
    /// Draw the combo listing of the gag types for the whitelisted character
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>gagTypeFilterCombo</c><param name="gagTypeFilterCombo"> - The gag type filter combo.</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>locked</c><param name="locked"> - The locked.</param></item>
    /// <item><c>width</c><param name="width"> - The width.</param></item>
    /// <item><c>gagtypecombo</c><param name="gagtypecombo"> - The gag type combo.</param></item>
    /// </list> </summary>
    /// <returns> True if it succeeds, false if it fails. </returns>
    public bool DrawGagTypeItemCombo(int ID,  WhitelistCharData charData, ref string gagLabel, int layerIndex, bool locked, int width, GagTypeFilterCombo gagtypecombo) {
        var combo = gagtypecombo; // get the combo
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Type");
        using var disabled = ImRaii.Disabled(locked);
        combo.Draw(ID, ref gagLabel, charData.selectedGagTypes, layerIndex, width);
        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                gagLabel = _gagService._gagTypes.First()._gagName;
                _config.Save();
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }

    /// <summary>
    /// Draw the combo listing of the gag locks for the whitelisted character
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>gagLockFilterCombo</c><param name="gagLockFilterCombo"> - The gag lock filter combo.</param></item>
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index.</param></item>
    /// <item><c>locked</c><param name="locked"> - The locked.</param></item>
    /// <item><c>width</c><param name="width"> - The width.</param></item>
    /// <item><c>gaglockcombo</c><param name="gaglockcombo"> - The gag lock combo.</param></item>
    /// </list> </summary>
    /// <returns> True if it succeeds, false if it fails. </returns>
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