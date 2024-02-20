using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.Interop;
using ImGuiNET;
using Newtonsoft.Json;
using OtterGui;

namespace GagSpeak.Utility;
public class ListCopier
{
    private List<bool> _checkboxStates;
    public List<string> _items;

    public ListCopier(List<string> items)
    {
        _items = items;
        _checkboxStates = new List<bool>(new bool[_items.Count]);
    }

    public void UpdateListInfo(List<string> items) {
        _items = items;
        _checkboxStates = new List<bool>(new bool[_items.Count]);
    }

    public void DrawCopyButton(string popupId, string sucessText, string failText) {

        ImGui.SetNextWindowSize(new Vector2(200*ImGuiHelpers.GlobalScale, 210*ImGuiHelpers.GlobalScale)); // Set the size of the popup here
        if (ImGui.BeginPopup($"{popupId}")) {
            ImGuiUtil.DrawDisabledButton("Items to copy to clipboard", new Vector2(-1, 22*ImGuiHelpers.GlobalScale), "", true);
            using (var table2 = ImRaii.Table("copySelectionTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY,
            new Vector2(-1, 150*ImGuiHelpers.GlobalScale))) {
                if (!table2) { return; }

                ImGui.TableSetupColumn("Items To Copy", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("##Checkbox", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
                ImGui.TableNextRow();
                for (int i = 0; i < _items.Count; i++) {
                    ImGuiUtil.DrawFrameColumn(_items[i]);
                    ImGui.TableNextColumn();
                    bool checkboxState = _checkboxStates[i];
                    if (ImGui.Checkbox($"##checkbox{i}", ref checkboxState)) {
                        _checkboxStates[i] = checkboxState;
                    }
                }
            }
            ImGui.Separator();
            ImGui.Spacing();
            if(ImGui.Button("Copy Selected Items", new Vector2(180*ImGuiHelpers.GlobalScale, 0))) {
                List<string> selectedItems = new List<string>();
                for (int i = 0; i < _items.Count; i++) {
                    if (_checkboxStates[i]) {
                        selectedItems.Add(_items[i]);
                    }
                }
                CopyItemsList(selectedItems, sucessText, failText);
                _checkboxStates = new List<bool>(new bool[_items.Count]);
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

    private void CopyItemsList(List<string> listOfItemsToCopy, string sucessText, string failText) {
        try
        {
            if (listOfItemsToCopy.Count == 0) {
                GagSpeak.Log.Warning("No items to copy.");
                return;
            }
            string json = JsonConvert.SerializeObject(listOfItemsToCopy);
            var compressed = json.Compress(6);
            string base64 = Convert.ToBase64String(compressed);
            ImGui.SetClipboardText(base64);
            GagSpeak.Log.Debug($"{sucessText}");
        }
        catch (Exception ex) {
            GagSpeak.Log.Warning($"{ex.Message} {failText}");
        }
    }
}