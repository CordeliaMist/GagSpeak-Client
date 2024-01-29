using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Utility;
using GagSpeak.Events;
using GagSpeak.Data;
using GagSpeak.Services;

namespace GagSpeak.UI.ComboListings;

/// <summary> This class is used to handle the gag type filter combo box. </summary>
public sealed class GagTypeFilterCombo 
{
    private GagSpeakConfig          _config;            // the config for the plugin
    private GagService              _gagService;        // the gag service
    private string                  _comboSearchText;   // the search text for the combo box
    private List<Gag>               _gagTypes;          // the gag types
    private bool                    isDummy = false;    // used to distinguish between general tab appliers, and whitelist ones

    /// <summary>
    /// Initializes a new instance of the <see cref="GagTypeFilterCombo"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>gagTypes</c><param name="gagTypes"> - The gag types.</param></item>
    /// </list> </summary>
    public GagTypeFilterCombo(GagService gagService, GagSpeakConfig config) {
        _comboSearchText = string.Empty;
        _gagService = gagService;
        _config = config;
        // set the gagtypes temp dictionary
        _gagTypes = _gagService._gagTypes;
    }

    /// <summary>
    /// This function draws ImGui's Combo list, but with a search filter. (and for Dictionary<string,int>)
    /// <list type="bullet">
    /// <item><c>ID</c><param name="ID"> - The list of items to display in the combo box</param></item>
    /// <item><c>label</c><param name="label"> - The label to display outside the combo box</param></item>
    /// <item><c>layerindex</c><param name="layerIndex"> - a list where the stored selection from the list is saved</param></item>
    /// </list>
    /// </summary>
    public void Draw(int ID, ref string label, ObservableList<string> listing, int layerIndex, int width) {
        // distinguish between general tab appliers, and whitelist ones
        if(label == "Dummy") { 
            label = listing[layerIndex];
            isDummy = true;
        }
        try
        {
            ImGui.SetNextItemWidth(width);
            using( var gagTypeCombo = ImRaii.Combo($"##{ID}_Type", label, ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) { 
                if( gagTypeCombo ) { // Assign it an ID if combo is sucessful.
                    // add the popup state
                    using var id = ImRaii.PushId($"##{ID}_Type"); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(width); // Set filter length to full
                    if( ImGui.InputTextWithHint("##filter", "Filter...", ref _comboSearchText, 255 ) ) { // Draw filter bar
                        // If the search bar is empty, display all the types from the strings in contentList, otherwise, display only search matches
                        _gagTypes = string.IsNullOrEmpty(_comboSearchText) ? (
                            _gagService._gagTypes
                        ) : (
                            _gagService._gagTypes.Where(gag => gag._gagName.ToLower().Contains(_comboSearchText.ToLower())).ToList()
                        );
                    }
                    // Now that we have our results, so draw the childs
                    var       height = ImGui.GetTextLineHeightWithSpacing() * 12 - ImGui.GetFrameHeight() - ImGui.GetStyle().WindowPadding.Y;
                    using var child = ImRaii.Child("Child", new Vector2( width, 200),true);
                    using var indent = ImRaii.PushIndent(ImGuiHelpers.GlobalScale);

                    // draw list
                    foreach( var item in _gagTypes ) { // We will draw out one selectable for each item.
                        if( ImGui.Selectable( item._gagName, item._gagName == listing[layerIndex] ) ) { // If our item is selected, set it and break
                            if(isDummy) { 
                                listing[layerIndex] = item._gagName;
                            } // update data (if for generaltab)
                            label = item._gagName; // update label
                            _comboSearchText = string.Empty;
                            _gagTypes = _gagService._gagTypes;
                            ImGui.CloseCurrentPopup();
                            _config.Save();
                            return;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            GagSpeak.Log.Debug(e.ToString());
        }
    }
}