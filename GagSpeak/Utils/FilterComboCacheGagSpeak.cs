using System;
using ImGuiNET;
using OtterGui.Log;
using System.Collections.Generic;
using OtterGui.Classes;
using System.Linq;

namespace GagSpeak.UI.Helpers;

public abstract class FilterComboCacheGagSpeak : FilterComboBaseGagSpeak
{
    public string? CurrentSelection { get; protected set; }
    private readonly ICachingList<string> _items;
    protected int CurrentSelectionIdx = -1;

    protected FilterComboCacheGagSpeak(IEnumerable<string> items, Logger log)
        : base(new TemporaryList<string>(items), false, log)
    {
        CurrentSelection = default;
        _items = (ICachingList<string>)Items;
    }

    protected FilterComboCacheGagSpeak(Func<IReadOnlyList<string>> generator, Logger log)
        : base(new LazyList<string>(generator), false, log)
    {
        CurrentSelection = default;
        _items           = (ICachingList<string>)Items;
    }

    // clean up the excess list
    protected override void Cleanup()
        => _items.ClearList();

    // draw the list
    protected override void DrawList(float width, float itemHeight) {
        base.DrawList(width, itemHeight);
        if (NewSelection != null && Items.Count > NewSelection.Value)
            CurrentSelection = Items[NewSelection.Value];
    }

    public bool Draw(string label, string preview, string tooltip, float previewWidth, float itemHeight, ImGuiComboFlags flags = ImGuiComboFlags.None)
        => Draw(label, preview, tooltip, ref CurrentSelectionIdx, previewWidth, itemHeight, flags);
}
