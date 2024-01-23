using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui.Widgets;
using Penumbra.GameData.DataContainers;
using Penumbra.GameData.Structs;

namespace GagSpeak.UI.ComboListings;
public sealed class StainColorCombo(float _comboWidth, DictStain _stains)
    : FilterComboColors(_comboWidth, CreateFunc(_stains), GagSpeak.Log)
{
    protected override bool DrawSelectable(int globalIdx, bool selected)
    {
        var       buttonWidth = ImGui.GetContentRegionAvail().X;
        var       totalWidth  = ImGui.GetContentRegionMax().X;
        using var style       = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(buttonWidth / 2 / totalWidth, 0.5f));

        return base.DrawSelectable(globalIdx, selected);
    }

    private static Func<IReadOnlyList<KeyValuePair<byte, (string Name, uint Color, bool Gloss)>>> CreateFunc(DictStain stains)
        => () => stains.Select(kvp => kvp)
            .Prepend(new KeyValuePair<StainId, Stain>(Stain.None.RowIndex, Stain.None)).Select(kvp
                => new KeyValuePair<byte, (string, uint, bool)>(kvp.Key.Id, (kvp.Value.Name, kvp.Value.RgbaColor, kvp.Value.Gloss))).ToList();
}