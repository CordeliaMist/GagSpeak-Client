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
using GagSpeak.Utility;
using GagSpeak.Wardrobe;
using GagSpeak.Gagsandlocks;
using GagSpeak.CharacterData;
using GagSpeak.UI.Equipment;

namespace GagSpeak.UI.Tabs.GeneralTab;
/// <summary> This class is used to draw the gag listings. </summary>
public class GagListingsDrawer : IDisposable
{
    IDalamudTextureWrap textureWrap1; IDalamudTextureWrap textureWrap2; IDalamudTextureWrap textureWrap3; // for image display
    IDalamudTextureWrap textureWrap4; IDalamudTextureWrap textureWrap5; IDalamudTextureWrap textureWrap6; // for image display
    private             DalamudPluginInterface  _pluginInterface;               // used to get the plugin interface
    private readonly    GagSpeakConfig          _config;                        // used to get the config
    private             GagAndLockManager       _lockManager;                   // used to get the lock manager
    private readonly    GagStorageManager       _gagStorageManager;             // used to get the gag storage manager
    private readonly    CharacterHandler        _characterHandler;              // used to get the character handler
    private             GagService              _gagService;                    // used to get the gag service
    private             TimerService            _timerService;                  // used to get the timer service
    private readonly    ItemAutoEquipEvent      _itemAutoEquipEvent;            // used to get the item auto equip event
    private             float                   _requiredComboWidthUnscaled;    // used to determine the required width of the combo
    private             float                   _requiredComboWidth;            // used to determine the width of the combo
    private             string                  _buttonLabel = "";              // used to display the button label
    public              bool[]                  _adjustDisp;                    // used to adjust the display of the password field
    private             Vector2                 _iconSize;                      // size of the icon
    private             float                   _comboLength;                   // length of the combo
    
    public GagListingsDrawer(DalamudPluginInterface dalamudPluginInterface, GagSpeakConfig config, GagStorageManager gagStorageManager,
    GagAndLockManager lockManager, CharacterHandler characterHandler, GagService gagService, TimerService timerService,
    ItemAutoEquipEvent itemAutoEquipEvent) {
        _pluginInterface = dalamudPluginInterface;
        _config = config;
        _characterHandler = characterHandler;
        _timerService = timerService;
        _lockManager = lockManager;
        _gagService = gagService;
        _itemAutoEquipEvent = itemAutoEquipEvent;
        _gagStorageManager = gagStorageManager;
        // draw textures for the gag and padlock listings //
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth\\{_characterHandler.playerChar._selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth\\{_characterHandler.playerChar._selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth\\{_characterHandler.playerChar._selectedGagTypes[2]}.png"));
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks\\{_characterHandler.playerChar._selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks\\{_characterHandler.playerChar._selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks\\{_characterHandler.playerChar._selectedGagPadlocks[2].ToString()}.png"));
        // initialize the adjust display
        _adjustDisp = new bool[] {false, false, false};
        // Subscribe to the events
        _characterHandler.playerChar._selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
        _characterHandler.playerChar._selectedGagPadlocks.ItemChanged += OnSelectedTypesChanged;
    }

    /// <summary> Disposes of the <see cref="GagListingsDrawer"/> subscribed events, unsubscribing them. </summary>
    public void Dispose() {
        _characterHandler.playerChar._selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
        _characterHandler.playerChar._selectedGagPadlocks.ItemChanged -= OnSelectedTypesChanged;
    }

    /// <summary> prepare the gag listing drawer by setting its width for the icon and combo. </summary>
    public void PrepareGagListDrawing() {
        // Draw out the content size of our icon
        _iconSize = new Vector2(2 * ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y);
        // Determine the size of our comboLength
        _comboLength = 280 * ImGuiHelpers.GlobalScale;
        // if the required combo with is unscaled
        if (_requiredComboWidthUnscaled == 0)
            try{
                // get the scaled combo width
                _requiredComboWidthUnscaled = _gagService._gagTypes.Max(gag => ImGui.CalcTextSize(gag._gagName).X) / ImGuiHelpers.GlobalScale;
            }
            catch (Exception e) {
                GagSpeak.Log.Error($"Failed to calculate the required combo width for the gag listing drawer. Size of gagtypes was {_gagService._gagTypes.Count}");
                GagSpeak.Log.Error(e.ToString());
            }
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
        if(_config.isLocked[layerIndex]) {
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
            GagSpeak.Log.Error($"Failed to draw icon for slot {layerIndex} with gag type {_characterHandler.playerChar._selectedGagTypes[layerIndex]}");
            GagSpeak.Log.Error(e.ToString());
        }
          
        ImGui.NextColumn(); ImGui.SetColumnWidth(1, width+10); // Set the desired widths);
        // create a group for the 2 dropdowns and icon
        using (var group = ImRaii.Group()) {
            if(!_adjustDisp[layerIndex]){ // inch our way down half the distance of a newline
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetFrameHeight() / 1.4f);
            }
            // Draw the combos
            if (DrawGagTypeItemCombo(ID, layerIndex, _config.isLocked[layerIndex], width, _gagTypeFilterCombo)) {}
            // Adjust the width of the padlock dropdown to 3/4 of the original width
            int newWidth = (int)(width * 0.75f);
            if (DrawGagLockItemCombo(ID, layerIndex, _config.isLocked[layerIndex], newWidth, _gagLockFilterCombo)) {}
            // end our disabled fields, if any, here
            if(_config.isLocked[layerIndex]) { ImGui.EndDisabled(); } // end the disabled part here, if it was disabled
            
            // get the type of button label that will display
            _buttonLabel = _config.isLocked[layerIndex] ? "Unlock" : "Lock"; // we want to display unlock button if we are currently locked
            ImGui.SameLine();
            if (ImGui.Button(_buttonLabel, new Vector2(-1, 0))) {
                _lockManager.ToggleLock(layerIndex);
            }
            // Display the password fields based on the selected padlock type
            if(_config.padlockIdentifier[layerIndex].DisplayPasswordField(_config.padlockIdentifier[layerIndex]._padlockType)) {
                _adjustDisp[layerIndex] = true;
            } else {
                _adjustDisp[layerIndex] = false;
            }
            // display the remaining time if we have a timer for this and we are locked
            if(_config.isLocked[layerIndex] && 
            (_config.padlockIdentifier[layerIndex]._padlockType == Padlocks.FiveMinutesPadlock ||
            _config.padlockIdentifier[layerIndex]._padlockType == Padlocks.MistressTimerPadlock ||
            _config.padlockIdentifier[layerIndex]._padlockType == Padlocks.TimerPasswordPadlock)) {
                _config.displaytext[layerIndex] = _timerService.GetRemainingTimeForPadlock(layerIndex);
            }
        }
        ImGui.NextColumn();
        ImGui.SetColumnWidth(2, 80);
        if(_characterHandler.playerChar._selectedGagPadlocks[layerIndex] != Padlocks.None) {
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
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth/{_characterHandler.playerChar._selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth/{_characterHandler.playerChar._selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"ItemMouth/{_characterHandler.playerChar._selectedGagTypes[2]}.png"));
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks/{_characterHandler.playerChar._selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks/{_characterHandler.playerChar._selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(
            Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"Padlocks/{_characterHandler.playerChar._selectedGagPadlocks[2].ToString()}.png"));
    }   

    /// <summary> FOR THE PLAYER DRAWER </summary>
    public bool DrawGagTypeItemCombo(int ID, int layerIndex, bool locked, int width, GagTypeFilterCombo gagtypecombo) {
        var combo = gagtypecombo; // get the combo
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Type");
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        var prevItem = _characterHandler.playerChar._selectedGagTypes[layerIndex]; // get the previous item
        combo.Draw(ID, _characterHandler, layerIndex, width);
        
        if(prevItem != _characterHandler.playerChar._selectedGagTypes[layerIndex]) { // if we have changed the item, update the image
            GagSpeak.Log.Debug($"=======================[ Padlock Manager : GAG CHANGE ]======================");
            GagSpeak.Log.Debug($"[GagListingsDrawer]: Firing itemAuto-Equip event for gag {_characterHandler.playerChar._selectedGagTypes[layerIndex]}");
            _characterHandler.Quicksave();
            _itemAutoEquipEvent.Invoke(_characterHandler.playerChar._selectedGagTypes[layerIndex], "self");
        }

        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                // get gagtype before clear
                var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().FirstOrDefault(gt => gt.GetGagAlias() == _characterHandler.playerChar._selectedGagTypes[layerIndex]);
                // clear the gag item from the selectedGagTypes list, resetting it to none
                _characterHandler.SetPlayerGagType(layerIndex, _gagService._gagTypes.First()._gagName);
                // reset the _wasEquippedBy to empty
                _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, "");
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }

    /// <summary> FOR THE PLAYER DRAWER </summary>
    public bool DrawGagLockItemCombo(int ID, int layerIndex, bool locked, int width, GagLockFilterCombo gaglockcombo) {
        var combo = gaglockcombo; // get the combo
        // if we left click and it is unlocked, open it
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Enum");
        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        combo.Draw(ID, _characterHandler, layerIndex, width);
        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                _characterHandler.SetPlayerGagPadlock(layerIndex, Padlocks.None); // to the first option, none
                _characterHandler.SetPlayerGagPadlockPassword(layerIndex, ""); // clear the password
                _characterHandler.SetPlayerGagPadlockAssigner(layerIndex, ""); // clear the assigner
                
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }

    /// <summary> FOR THE WHITELIST DRAWER </summary>
    public bool DrawGagTypeItemCombo(int ID, int whitelistIdx, ref string gagLabel, int layerIndex, bool locked, int width, GagTypeFilterCombo gagtypecombo) {
        var combo = gagtypecombo; // get the combo
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Type");
        using var disabled = ImRaii.Disabled(locked);
        combo.Draw(ID, ref gagLabel, _characterHandler, whitelistIdx, layerIndex, width);
        if (!locked) { // if we right click on it, clear the selection
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
                gagLabel = _gagService._gagTypes.First()._gagName;
            }
            ImGuiUtil.HoverTooltip("Right-click to clear.");
        }
        return true;
    }

    /// <summary> FOR THE WHITELIST DRAWER </summary>
    public bool DrawGagLockItemCombo(int ID, int whitelistIdx, ref string lockLabel, int layerIndex, int width, GagLockFilterCombo gaglockcombo) {
        // This code is a shadow copy of the function above, used for accepting WhitelistCharData as a type
        var combo = gaglockcombo;
        if (ImGui.IsItemClicked())
            UIHelpers.OpenCombo($"{ID}_Enum");
        combo.Draw(ID, ref lockLabel, _characterHandler, whitelistIdx, layerIndex, width);
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            lockLabel = "None";
        }
        ImGuiUtil.HoverTooltip("Right-click to clear.");
        return true;
    }
}