using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.CharacterData;
using Dalamud.Interface;
using OtterGui;
using System.Linq;
using GagSpeak.ToyboxandPuppeteer;
using System.Collections.Generic;
using Dalamud.Interface.Utility;

namespace GagSpeak.UI.Tabs.PuppeteerTab;
public partial class PuppeteerAliasTable {

    private readonly    CharacterHandler            _characterHandler;
    private             Dictionary<int, string>     _tempAliasTexts;
    private             Dictionary<int, string>     _tempAliasCommands;
    private             AliasTrigger                _tempNewAlias;
    public PuppeteerAliasTable(CharacterHandler characterHandler) {
        _characterHandler = characterHandler;
        _tempNewAlias = new AliasTrigger();
        _tempAliasTexts = new Dictionary<int, string>();
        _tempAliasCommands = new Dictionary<int, string>();
    }

    public void Draw() {
        var _ = ImRaii.Group();
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw out the table
        DrawAliasListTable();
    }

    // store a temp list of items to replace;
    public List<int> itemsToRemove = new List<int>();

    private void DrawAliasListTable() {
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.3f, ImGui.GetStyle().CellPadding.Y)); // Modify the X padding
        using (var table = ImRaii.Table("UniqueAliasListCreator", 2, ImGuiTableFlags.RowBg)) {
        if (!table) { return; }
            // draw the header row
            ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableSetupColumn("Alias Text Input / Output", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableSetupColumn("Use##IsEnabled", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableHeadersRow();


            // Replace this with your actual data
            foreach (var (aliasTrigger, idx) in _characterHandler.playerChar._triggerAliases.ElementAt(_characterHandler.activeListIdx)
                                                                                    ._aliasTriggers.Select((value, index) => (value, index)))
            {
                using var id = ImRaii.PushId(idx);
                bool shouldRemove = DrawAssociatedModRow(aliasTrigger, idx);
                if(shouldRemove) {
                    itemsToRemove.Add(idx);
                }
            }
            // after we get what must be removed, remove them before we draw
            foreach (var item in itemsToRemove) {
                _characterHandler.RemoveAliasEntry(item);
            }
            if(itemsToRemove.Count > 0) {
                itemsToRemove.Clear();
            }
            DrawNewModRow();
        }
        // remove the style var
        ImGui.PopStyleVar();
    }

    private bool DrawAssociatedModRow(AliasTrigger aliasTrigger, int idx) {
        bool shouldRemove = false;
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
        "Delete this alias from associations\nHold SHIFT in order to delete.", !ImGui.GetIO().KeyShift, true)) {
            shouldRemove = true;
        }
        bool isEnabled = aliasTrigger._enabled;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGuiHelpers.GlobalScale);
        if (ImGui.Checkbox($"##isEnabled{idx}", ref isEnabled)) {
            _characterHandler.UpdateAliasEntryEnabled(idx, isEnabled); // Use the helper function to update the alias entry enabled status
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"Whether or not the alias is enabled for use."); }
        ImGui.TableNextColumn();
        string aliasText = _tempAliasTexts.ContainsKey(idx) ? _tempAliasTexts[idx] : aliasTrigger._inputCommand;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint($"##aliasText{idx}", "Alias Input phrase goes here...", ref aliasText, 64)) {
            _tempAliasTexts[idx] = aliasText; // Update the alias entry input
        }
        if(ImGui.IsItemDeactivatedAfterEdit()) {
            _characterHandler.UpdateAliasEntryInput(idx, aliasText);
            _tempAliasTexts.Remove(idx);
        }
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGuiHelpers.GlobalScale);
        string command = _tempAliasCommands.ContainsKey(idx) ? _tempAliasCommands[idx] : aliasTrigger._outputCommand;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint($"##command{idx}", "Alias Output phrase goes here...", ref command, 300)) // If the input text is modified
            _tempAliasCommands[idx] = command; // Update the alias entry output
        if(ImGui.IsItemDeactivatedAfterEdit()) {
            _characterHandler.UpdateAliasEntryOutput(idx, command);
            _tempAliasCommands.Remove(idx);
        }
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + .5f*ImGuiHelpers.GlobalScale);

        return shouldRemove;
    }

    private void DrawNewModRow() {
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetFrameHeight()), "", false, true)){
            _characterHandler.AddNewAliasEntry(_tempNewAlias); // Use the helper function to add the new alias entry
            _tempNewAlias = new AliasTrigger();
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"Add the alias configuration to the list."); }
        bool newAliasEnabled = _tempNewAlias._enabled;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGuiHelpers.GlobalScale);
        if (ImGui.Checkbox("##newAliasEnabled", ref newAliasEnabled)) {
            _tempNewAlias._enabled = newAliasEnabled; // Update the new alias entry enabled status
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"Whether or not the alias is enabled for use."); }
        
        ImGui.TableNextColumn();
        string newAliasText = _tempNewAlias._inputCommand;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputTextWithHint("##newAliasText", "Alias Input...", ref newAliasText, 50)){
            _tempNewAlias._inputCommand = newAliasText; // Update the new alias entry input
        }
        if(ImGui.IsItemHovered()) { 
            ImGui.SetTooltip($"When {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]} "+
            "says this as a part of the command for you to execute after your trigger phrase,\n"+
            "You will replace it with the alias output command before executing it.");
        }
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        string newAliasCommand = _tempNewAlias._outputCommand;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - ImGuiHelpers.GlobalScale);
        if (ImGui.InputTextWithHint("##newAliasCommand","Output Command to execute when alias input is read...", ref newAliasCommand, 200)) {
            _tempNewAlias._outputCommand = newAliasCommand; // Update the new alias entry output
        }
        if(ImGui.IsItemHovered()) { 
            ImGui.SetTooltip($"What you will replace the alias input command with if "+
            "it is included in the command to execute after your trigger phrase.");
        }
    }
}