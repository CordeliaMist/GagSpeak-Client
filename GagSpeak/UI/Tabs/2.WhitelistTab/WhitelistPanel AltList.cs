using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui;
using System.Linq;
using Dalamud.Interface;
using System.Collections.Generic;
using Dalamud.Interface.Utility;
using GagSpeak.Utility;


namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {
    private List<int> itemsToRemove = new List<int>();

    public void DrawAltList(ref bool _interactions)
    {
        // draw out the panel
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw out the collapsible tabs and their bodies
        ImGui.PushFont(_fontService.UidFont);
        try { ImGuiUtil.Center($"{AltCharHelpers.FetchOriginalName(_characterHandler.activeListIdx).Split(' ')[0]}'s Alt Character List"); }
        finally { ImGui.PopFont(); }
        
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.3f, ImGui.GetStyle().CellPadding.Y)); // Modify the X padding
        using (var table = ImRaii.Table("AltListTable", 4, ImGuiTableFlags.RowBg)) {
            if (!table) { return; }

            // draw the header row
            ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableSetupColumn("Character Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Homeworld", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Name To Use", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Name To Use").X + ImGuiHelpers.GlobalScale * 5);
            ImGui.TableHeadersRow();

            // Replace this with your actual data source
            foreach (var (altCharacter, idx) in _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAW.Select((value, index) => (value, index))) {
                using var id = ImRaii.PushId(idx);
                bool shouldRemove = DrawAltCharacterRow(altCharacter, idx);
                if(shouldRemove) {
                    itemsToRemove.Add(idx);
                }
            }

            // after we get what must be removed, remove them before we draw
            foreach (var item in itemsToRemove) {
                _characterHandler.whitelistChars[_characterHandler.activeListIdx].RemoveAltCharacter(item);
                _characterHandler.Save();
                GSLogger.LogType.Information($"Removed alt character at index {item}");
            }
            // i broke things yay
            if(itemsToRemove.Count > 0) {
                itemsToRemove.Clear();
            }
        }
        // remove the style var
        ImGui.PopStyleVar(2);
    }

    private bool DrawAltCharacterRow((string _name, string _homeworld) altCharacter, int idx) {
        bool shouldRemove = false;
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
        "Unregister this player as an alt of this character.\nHold SHIFT in order to delete.", idx == 0 || !ImGui.GetIO().KeyShift, true)) {
            shouldRemove = true;
        }
        ImGui.TableNextColumn();
        ImGui.Text(altCharacter._name);
        ImGui.TableNextColumn();
        ImGui.Text(altCharacter._homeworld);
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess == idx ? FontAwesomeIcon.Check.ToIconString() : FontAwesomeIcon.Times.ToIconString(),
        new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight()), "Select this name to be used when processing interactions.", false, true))
        {
            _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess =
                _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess == idx ? 0 : idx;
            
            _characterHandler.Save();
        }

        return shouldRemove;
    }
}