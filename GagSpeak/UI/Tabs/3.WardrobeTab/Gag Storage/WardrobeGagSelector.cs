using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using System.Linq;
using Dalamud.Interface.Utility;
using GagSpeak.Wardrobe;
using GagSpeak.Gagsandlocks;

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class GagStorageSelector
{
    private readonly    GagStorageManager   _gagStorageManager;     // for getting the gag storage manager

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public GagStorageSelector(GagStorageManager gagStorageManager)
    {
        _gagStorageManager = gagStorageManager;
    }

    public void Draw(float width) {
        // create a group for our homebrew listbox
        using var group = ImRaii.Group();
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing)
            .Push(ImGuiStyleVar.FrameRounding, 0); // and make them recantuclar instead of rounded buttons
        // make sure our next item is directly below our header
        DrawGagSelector(width);
        style.Pop();
    }

    private void DrawGagSelector(float width) {
        // create our selector child within, we wont want to give it any padding so we give off the illusion of the listbox being the same size as the combo
        // using var color = ImRaii.PushColor(ImGuiCol.ChildBg, new Vector4(0xFE / 255f, 0x73 / 255f, 0xBE / 255f, 0xEF / 255f));
        using var child = ImRaii.Child("##GagSelector", new Vector2(width * ImGuiHelpers.GlobalScale, -1), true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return; } // make sure our child was made
        
        // push the 

        foreach(var gag in Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().Where(gag => gag != GagList.GagType.None)) {
            if(ImGui.Selectable(gag.GetGagAlias(), _gagStorageManager._selectedGag == gag)) {
                // Only update the selected GagIndex in our gagstorage manager if it's different from the currently selected gag
                if (_gagStorageManager._selectedGag != gag) {
                    _gagStorageManager.SetSelectedIdx(gag);
                }
            }
        }
    }
}