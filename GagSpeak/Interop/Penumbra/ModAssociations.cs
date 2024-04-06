using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;

namespace GagSpeak.Interop.Penumbra;
public class ModAssociations : IDisposable
{
    private readonly PenumbraService          _penumbra;
    private readonly RestraintSetManager      _manager;
    private readonly ModCombo                 _modCombo;
    private readonly IClientState             _clientState;
    private readonly RS_ToggleEvent           _rsToggleEvent;

    public ModAssociations(PenumbraService penumbra, RestraintSetManager manager,
    RS_ToggleEvent rsToggleEvent, IClientState clientState)
    {
        _penumbra = penumbra;
        _manager  = manager;
        _modCombo = new ModCombo(penumbra, GagSpeak.Log);
        _rsToggleEvent = rsToggleEvent;
        _clientState = clientState;

        _rsToggleEvent.SetToggled += ApplyModsOnSetToggle;
    }

    public void Dispose() {
        _rsToggleEvent.SetToggled -= ApplyModsOnSetToggle;
    }

    private void ApplyModsOnSetToggle(object sender, RS_ToggleEventArgs e) {
        // if the set is being enabled, we should toggle on the mods
        if(_clientState.IsLoggedIn && _clientState.LocalContentId != 0) {
            if (e.ToggleType == RestraintSetToggleType.Enabled) {
                foreach (var (mod, settings, disableWhenInactive, redrawAfterToggle) in _manager._restraintSets[e.SetIndex]._associatedMods) {
                    _penumbra.SetMod(mod, settings, true, disableWhenInactive, redrawAfterToggle);
                }
            }
            // otherwise, we should toggle off the mods
            else {
                foreach (var (mod, settings, disableWhenInactive, redrawAfterToggle) in _manager._restraintSets[e.SetIndex]._associatedMods) {
                    _penumbra.SetMod(mod, settings, false, disableWhenInactive, redrawAfterToggle);
                }
            }
        }
    }

    // main draw function for the mod associations table
    public void Draw() {
        DrawTable();
    }

    // draw the table for constructing the associated mods.
    private void DrawTable() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.3f, ImGui.GetStyle().CellPadding.Y));
        using var table = ImRaii.Table("Mods", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY);
        if (!table) { return; }

        ImGui.TableSetupColumn("##Delete",      ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("Mods to enable with this Set",       ImGuiTableColumnFlags.WidthStretch);        
        ImGui.TableSetupColumn("Toggle",         ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Toggle").X);
        ImGui.TableSetupColumn("##Redraw",        ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
        ImGui.TableSetupColumn("##Update",      ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());             // update to reflect what is in
        ImGui.TableHeadersRow();

        Mod? removedMod = null;
        (Mod mod, ModSettings settings, bool disableWhenInactive, bool redrawAfterToggle)? updatedMod = null;

        foreach (var ((mod, settings, disableWhenInactive, redrawAfterToggle), idx) in _manager._restraintSets[_manager._selectedIdx]._associatedMods.WithIndex()) {
            using var id = ImRaii.PushId(idx);
            DrawAssociatedModRow(mod, settings, disableWhenInactive, redrawAfterToggle, out var removedModTmp, out var updatedModTmp);
            if (removedModTmp.HasValue) {
                removedMod = removedModTmp;
            }
            if (updatedModTmp.HasValue) {
                updatedMod = updatedModTmp;
            }
        }

        DrawNewModRow();

        if (removedMod.HasValue) {
            _manager.RemoveMod(_manager._selectedIdx, removedMod.Value);
        }
        
        if (updatedMod.HasValue) {
            _manager.UpdateMod(_manager._selectedIdx, updatedMod.Value.mod, updatedMod.Value.settings, updatedMod.Value.disableWhenInactive, updatedMod.Value.redrawAfterToggle);
        }
    }

    private void DrawAssociatedModRow(Mod mod, ModSettings settings, bool disableWhenInactive, bool redrawAfterToggle, out Mod? removedMod, out (Mod, ModSettings, bool, bool)? updatedMod) {
        removedMod = null;
        updatedMod = null;
        // get the index of this mod
        var currentModIndex = _manager._restraintSets[_manager._selectedIdx]._associatedMods.FindIndex(x => x.mod == mod);
        if (currentModIndex == -1) {
            // Handle the case where the mod is not found in the list
            return;
        }
        ImGui.TableNextColumn();
        // delete icon
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
        "Delete this mod from associations", !ImGui.GetIO().KeyShift, true)) {
            removedMod = mod;
        }
        
        // the name of the appended mod
        ImGui.TableNextColumn();
        ImGui.Selectable($"{mod.Name}##name");
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"Mod to be enabled when restraint set it turned on.\n{mod.Name}"); }
        // if we should enable or disable this mod list (all buttons should sync)
        
        ImGui.TableNextColumn();
        // get the current mod we are looking at
        var currentMod = _manager._restraintSets[_manager._selectedIdx]._associatedMods[currentModIndex];
        // set icon and help text
        var iconText = currentMod.disableWhenInactive ? FontAwesomeIcon.Check : FontAwesomeIcon.Times;
        var helpText = currentMod.disableWhenInactive ? "Mods are disabled when set is disabled" : "Mods will stay enabled after set is turned off";
        if (ImGuiUtil.DrawDisabledButton(iconText.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()),
        helpText, false, true)) {
            updatedMod = (mod, settings, !disableWhenInactive, redrawAfterToggle);
        }

        ImGui.TableNextColumn();
        // redraw button
        var iconText2 = currentMod.redrawAfterToggle ? FontAwesomeIcon.Redo : FontAwesomeIcon.None;
        var helpText2 = currentMod.redrawAfterToggle ? "Redraws self after set toggle (nessisary for VFX/Animation Mods)" : "Do not redraw when set is toggled (uses fast redraw)";
        if (ImGuiUtil.DrawDisabledButton(iconText2.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()),
        helpText2, false, true)) {
            updatedMod = (mod, settings, disableWhenInactive, !redrawAfterToggle);
        }

        // button to update the status the mod from penumbra
        ImGui.TableNextColumn();
        ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Search.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
        "Inspect current mod status", false, true);
        if (ImGui.IsItemHovered()) {
            var (_, newSettings) = _penumbra.GetMods().FirstOrDefault(m => m.Mod == mod);
            if (ImGui.IsItemClicked()) {
                updatedMod = (mod, newSettings, disableWhenInactive, redrawAfterToggle);
            }
            
            using var style = ImRaii.PushStyle(ImGuiStyleVar.PopupBorderSize, 2 * ImGuiHelpers.GlobalScale);
            using var tt = ImRaii.Tooltip();
            ImGui.Separator();
            var namesDifferent = mod.Name != mod.DirectoryName;
            ImGui.Dummy(new Vector2(300 * ImGuiHelpers.GlobalScale, 0));
            using (ImRaii.Group()) {
                if (namesDifferent)
                    ImGui.TextUnformatted("Directory Name");
                ImGui.TextUnformatted("Enabled");
                ImGui.TextUnformatted("Priority");
                ModCombo.DrawSettingsLeft(newSettings);
            }

            ImGui.SameLine(Math.Max(ImGui.GetItemRectSize().X + 3 * ImGui.GetStyle().ItemSpacing.X, 150 * ImGuiHelpers.GlobalScale));
            using (ImRaii.Group()) {
                if (namesDifferent)
                    ImGui.TextUnformatted(mod.DirectoryName);
                ImGui.TextUnformatted(newSettings.Enabled.ToString());
                ImGui.TextUnformatted(newSettings.Priority.ToString());
                ModCombo.DrawSettingsRight(newSettings);
            }
        }
    }
    
    private void DrawNewModRow()
    {
        var currentName = _modCombo.CurrentSelection.Mod.Name;
        ImGui.TableNextColumn();
        var tt = currentName.IsNullOrEmpty()
            ? "Please select a mod first."
            : _manager._restraintSets[_manager._selectedIdx]._associatedMods.Any(x => x.mod == _modCombo.CurrentSelection.Mod)
                ? "The design already contains an association with the selected mod."
                : string.Empty;

        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetFrameHeight()), tt, tt.Length > 0,
                true))
            _manager.AddMod(_manager._selectedIdx, _modCombo.CurrentSelection.Mod, _modCombo.CurrentSelection.Settings);
        ImGui.TableNextColumn();
        _modCombo.Draw("##new", currentName.IsNullOrEmpty() ? "Select new Mod..." : currentName, string.Empty,
            ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight());
    }
}