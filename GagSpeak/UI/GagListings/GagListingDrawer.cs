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

using GagSpeak.Events;


// Practicing Modular Design
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

    private const float DefaultWidth = 280; // set the default width
    private readonly GagSpeakConfig _config;    
    private float _requiredComboWidthUnscaled;
    private float _requiredComboWidth;
    private string _passwordField = "";
    
    // I believe this dictates the data for the stain list, swap out for padlock list probably
    
    public GagListingsDrawer(GagSpeakConfig config, DalamudPluginInterface dalamudPluginInterface) // Constructor
    {
        _config = config;
        //update interface
        _pluginInterface = dalamudPluginInterface;

        // draw textures for the gag list
        textureWrap1 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagTypes[0]}.png"));
        textureWrap2 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagTypes[1]}.png"));
        textureWrap3 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagTypes[2]}.png"));
        // draw textures for the padlock list
        textureWrap4 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagPadlocks[0].ToString()}.png"));
        textureWrap5 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagPadlocks[1].ToString()}.png"));
        textureWrap6 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{config.selectedGagPadlocks[2].ToString()}.png"));
    
        _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
        _config.selectedGagPadlocks.ItemChanged += OnSelectedTypesChanged;
    }

    private Vector2 _iconSize;
    private float _comboLength;

    public void Dispose() {
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
    public void DrawGagAndLockListing(int ID, GagSpeakConfig config, GagTypeFilterCombo _gagTypeFilterCombo,
    GagLockFilterCombo _gagLockFilterCombo, int layerIndex, string displayLabel, bool locked, int width1, int width2) {
        // Get our boolean on if we are locked or not
        bool isPadlockEquipped = config.selectedGagPadlocks[layerIndex] != GagPadlocks.None;
        // if we are locked, set the locked to true
        if(isPadlockEquipped)
            ImGui.BeginDisabled();
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
          
        ImGui.NextColumn();
        ImGui.SetColumnWidth(1, width1+10); // Set the desired widths);
        // create a group for the 2 dropdowns and icon
        using (var group = ImRaii.Group()) {
            if(!isPadlockEquipped) { // inch our way down half the distance of a newline
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetFrameHeight() / 1.4f);
            }
            // draw the combo boxes
            GagPadlocks selected = GagPadlocks.None;
            if (DrawGagTypeItemCombo(ID, config, layerIndex, locked, width1, _gagTypeFilterCombo)) {}
            if (DrawGagLockItemCombo(ID, config, layerIndex, locked, width2, _gagLockFilterCombo, ref selected)) {
                if(isPadlockEquipped) {
                    ImGui.EndDisabled();
                    // going to do a lot of checks here for what field to display
                    
                    var password  = _passwordField; // temp storage to hold until we de-select the text input
                    ImGui.SetNextItemWidth(width1);
                    if (ImGui.InputText("##Password_Input", ref password, 30, ImGuiInputTextFlags.None))
                        _passwordField = password;
                    if (ImGui.IsItemDeactivatedAfterEdit() || ImGui.IsKeyPressed(ImGuiKey.Enter)) { // will only update our safeword once we click away from the safeword bar
                        // reset the password field
                        _passwordField = "";
                        // check if the password we have entered matches the password we have saved
                        if(config.selectedGagPadlocksPassword[layerIndex] == password) {
                            // the password we have entered does match the password we have saved, so disable the lock and print status to debug
                            config.selectedGagPadlocksPassword[layerIndex] = ""; // clear the password
                            config.selectedGagPadlocks[layerIndex] = GagPadlocks.None; // clear the padlock
                            GagSpeak.Log.Debug($"Padlock on slot {layerIndex} has been unlocked & lock removed.");
                        } else {
                            // the password we have entered does not match the password we have saved, so print status to debug
                            GagSpeak.Log.Debug($"Password for Padlock is incorrect, try again.");
                        }
                    }
                }
            }
        }
        ImGui.NextColumn();
        ImGui.SetColumnWidth(2, 80);
        if(config.selectedGagPadlocks[layerIndex] != GagPadlocks.None) {
            if(layerIndex==0) { ImGui.Image(textureWrap4.ImGuiHandle, new Vector2(80, 80)); return;}
            if(layerIndex==1) { ImGui.Image(textureWrap5.ImGuiHandle, new Vector2(80, 80)); return;}
            if(layerIndex==2) { ImGui.Image(textureWrap6.ImGuiHandle, new Vector2(80, 80)); return;}
        }
        // end our table
        ImGui.Columns(1);
    }

    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        GagSpeak.Log.Debug($"Item at index {e.Index} changed from '{e.OldValue}' to '{e.NewValue}'");
            if(e.Index==0) // gagtyped
                textureWrap1 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagTypes[0]}.png"));
            if(e.Index==1)
                textureWrap2 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagTypes[1]}.png"));
            if(e.Index==2)
                textureWrap3 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagTypes[2]}.png"));
            if(e.Index==0) // passwords
                textureWrap4 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagPadlocks[0].ToString()}.png"));
            if(e.Index==1)
                textureWrap5 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagPadlocks[1].ToString()}.png"));
            if(e.Index==2)
                textureWrap6 = _pluginInterface.UiBuilder.LoadImage(Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, $"{_config.selectedGagPadlocks[2].ToString()}.png"));
    }
    
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
    public bool DrawGagLockItemCombo(int ID, GagSpeakConfig config, int layerIndex, bool locked, int width, GagLockFilterCombo gaglockcombo, ref GagPadlocks selected) {
        var combo = gaglockcombo; // get the combo
        // if we left click and it is unlocked, open it
        if (ImGui.IsItemClicked() && !locked)
            UIHelpers.OpenCombo($"{ID}_Enum");
        // using the var disabled, disable this if it is locked.
        using var disabled = ImRaii.Disabled(locked);
        // draw the thing
        combo.Draw(ID, config.selectedGagPadlocks, layerIndex, width, ref selected);
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