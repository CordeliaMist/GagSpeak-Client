using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;

using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using OtterGui;
using OtterGui.Classes;
using OtterGui.Log;
using OtterGui.Raii;
using OtterGui.Widgets;


namespace GagSpeak.UI.Helpers;


// originally this was passed through an EquipItem object/struct, but i am trying to convert this into a list of strings, initally passed in as a dictionary to the highest level class
public sealed class GagItemCombo : FilterComboCacheGagSpeak
{
    public readonly  string          Label; // our combo label
    private          string          _currentItem; // store our current item
    private          float           _innerWidth; // store our inner width of the menu

    public GagItemCombo(Dictionary<string,int> items, string slot, Logger log)
        : base(GetItems(items), log)
    {
        Label         = slot; // set our label
        _currentItem  = "Nothing";
        SearchByParts = true;
    }

    // draw our list
    protected override void DrawList(float width, float itemHeight)
    {
        base.DrawList(width, itemHeight);
        // if our new selection is not null and less than the last item in the dropdown
        if (NewSelection != null && Items.Count > NewSelection.Value)
            // then update our current selection to reflect that
            CurrentSelection = Items[NewSelection.Value];
    }


    // Function to update our current selection
    protected override int UpdateCurrentSelected(int currentSelected) {
        // if our current selection is equal to our current item
        if (CurrentSelection == _currentItem)
            // just return what we passed in
            return currentSelected;
        // otherwise it is a new selection so we should update it.

        CurrentSelectionIdx = Items.IndexOf(i => i == _currentItem);
        CurrentSelection = CurrentSelectionIdx >= 0 ? Items[CurrentSelectionIdx] : default;
        return base.UpdateCurrentSelected(CurrentSelectionIdx);
    }

    // main draw func
    public bool Draw(string previewName, float width, float innerWidth) {
        _innerWidth  = innerWidth;
        return Draw($"##{Label}", previewName, string.Empty, width, ImGui.GetTextLineHeightWithSpacing());
    }
    // find the filter width
    protected override float GetFilterWidth()
        => _innerWidth - 2 * ImGui.GetStyle().FramePadding.X;

    protected override bool DrawSelectable(int globalIdx, bool selected)
    {
        var name = Items[globalIdx];
        if (CurrentSelectionIdx == globalIdx)
        {
            CurrentSelectionIdx = -1;
            _currentItem        = name;
            CurrentSelection    = default;
        }

        ImGui.SameLine();
        var ret = ImGui.Selectable(name, selected);
        ImGui.SameLine();
        using var color = ImRaii.PushColor(ImGuiCol.Text, 0xFF808080);
        ImGuiUtil.RightAlign($"({name})");
        return ret;
    }

    protected override bool IsVisible(int globalIndex, LowerString filter)
        => base.IsVisible(globalIndex, filter);

    private static string GetLabel(string slot) {
        return slot switch
        {
            "LayerOneGagType" => "0",
            "LayerTwoGagType" => "1",
            "LayerThreeGagType" => "2",
            _                 => string.Empty,
        };
    }

    private static IReadOnlyList<string> GetItems(Dictionary<string,int> items)
    {
        // Get your items from _config.GagTypes or any appropriate source
        var itemlist = new List<string>(items.Keys); // Extracts keys from the provided dictionary
        return itemlist;
    }
}
