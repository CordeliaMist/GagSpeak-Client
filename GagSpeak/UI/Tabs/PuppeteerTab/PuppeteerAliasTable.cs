using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.CharacterData;
using Dalamud.Interface;
using OtterGui;
using System.Linq;
using GagSpeak.ToyboxandPuppeteer;
using System.Collections.Generic;

namespace GagSpeak.UI.Tabs.PuppeteerTab;
public partial class PuppeteerAliasTable {

    private readonly    CharacterHandler            _characterHandler;
    private Dictionary<int, string> _tempAliasTexts = new Dictionary<int, string>();
    private Dictionary<int, string> _tempAliasCommands = new Dictionary<int, string>();

    private             AliasTrigger                _tempNewAlias;
    public PuppeteerAliasTable(CharacterHandler characterHandler) {
        _characterHandler = characterHandler;
        _tempNewAlias = new AliasTrigger();
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

        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.2f, ImGui.GetStyle().CellPadding.Y)); // Modify the X padding
        using (var table = ImRaii.Table("UniqueAliasListCreator", 4, ImGuiTableFlags.RowBg)) {
        if (!table) { return; }
            // draw the header row
            ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableSetupColumn("Alias Text Input", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("ThisIsALongNamemmmmm").X);
            ImGui.TableSetupColumn("Use##IsEnabled", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableSetupColumn("   Replacement text to execute instead", ImGuiTableColumnFlags.WidthStretch);
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
            itemsToRemove.Clear();
            DrawNewModRow();
        }
        // remove the style var
        ImGui.PopStyleVar();
    }

    private bool DrawAssociatedModRow(AliasTrigger aliasTrigger, int idx) {
        bool shouldRemove = false;
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
                "Delete this alias from associations", false, true))
            shouldRemove = true;

        ImGui.TableNextColumn();
        string aliasText = _tempAliasTexts.ContainsKey(idx) ? _tempAliasTexts[idx] : aliasTrigger._inputCommand;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText($"##aliasText{idx}", ref aliasText, 50)) {
            _tempAliasTexts[idx] = aliasText; // Update the alias entry input
        }
        if(ImGui.IsItemDeactivatedAfterEdit()) {
            _characterHandler.UpdateAliasEntryInput(idx, aliasText);
            _tempAliasTexts.Remove(idx);
        }
        ImGui.TableNextColumn();
        bool isEnabled = aliasTrigger._enabled;
        if (ImGui.Checkbox($"##isEnabled{idx}", ref isEnabled)) {
            _characterHandler.UpdateAliasEntryEnabled(idx, isEnabled); // Use the helper function to update the alias entry enabled status
        }

        ImGui.TableNextColumn();
        string command = _tempAliasCommands.ContainsKey(idx) ? _tempAliasCommands[idx] : aliasTrigger._outputCommand;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText($"##command{idx}", ref command, 200)) // If the input text is modified
            _tempAliasCommands[idx] = command; // Update the alias entry output
        if(ImGui.IsItemDeactivatedAfterEdit()) {
            _characterHandler.UpdateAliasEntryOutput(idx, command);
            _tempAliasCommands.Remove(idx);
        }

        return shouldRemove;
    }

    private void DrawNewModRow() {
        ImGui.TableNextColumn();
        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetFrameHeight()), "", false, true)){
            _characterHandler.AddNewAliasEntry(_tempNewAlias); // Use the helper function to add the new alias entry
            _tempNewAlias = new AliasTrigger();
        }
        ImGui.TableNextColumn();
        string newAliasText = _tempNewAlias._inputCommand;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText("##newAliasText", ref newAliasText, 50)){
            _tempNewAlias._inputCommand = newAliasText; // Update the new alias entry input
        }
        ImGui.TableNextColumn();
        bool newAliasEnabled = _tempNewAlias._enabled;
        if (ImGui.Checkbox("##newAliasEnabled", ref newAliasEnabled)) {
            _tempNewAlias._enabled = newAliasEnabled; // Update the new alias entry enabled status
        }
        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        string newAliasCommand = _tempNewAlias._outputCommand;
        if (ImGui.InputText("##newAliasCommand", ref newAliasCommand, 200)) {
            _tempNewAlias._outputCommand = newAliasCommand; // Update the new alias entry output
        }
    }
}