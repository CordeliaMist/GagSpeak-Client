using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using System.Collections.Generic;
using System.Linq;

namespace GagSpeak
{
    public unsafe partial class GagSpeak
    {
        /// <summary>
        /// Function: Draw Filter Combo (string, int) [May make this generic later]
        /// Purpose: Draws the filter combo box based on spesified parameters
        /// CONTENT LIST - the list of items to display in the combo box
        /// LABEL - the label to display above the combo box
        /// SELECTED TYPES - the list of currently selected types to display in the combo box
        /// LAYER INDEX - the index of the gagtype list to know which type it displays
        /// </summary>
        public void DrawFilterCombo(Dictionary<string, int> contentList, string label, List<string> selectedTypes, int layerIndex) {
            // create an empty string for the search text
            var ComboSearchText = string.Empty;
            // Create the combo
            using( var gagTypeOneCombo = ImRaii.Combo( label, selectedTypes[layerIndex], ImGuiComboFlags.PopupAlignLeft | ImGuiComboFlags.HeightLargest)) {
                // Assign it an ID if combo is sucessful.
                if( gagTypeOneCombo ) {
                    using var id = ImRaii.PushId( label ); // Push an ID for the combo box (based on label / name)
                    ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X); // Set filter length to full
                    
                    // Draw filter bar
                    if( ImGui.InputTextWithHint("##filter", "Filter...", ref ComboSearchText, 255 ) ) {
                        // Query: if the search bar is empty, display all the gag types, otherwise, display only search matches
                        contentList = string.IsNullOrEmpty(ComboSearchText) ? (
                            Configuration.GagTypes
                        ) : (
                            Configuration.GagTypes.Where(x=>x.Key.ToLower().Contains(ComboSearchText.ToLower())).ToDictionary(x=>x.Key, x=>x.Value)
                        );
                    }
                    
                    // Now that we have our results, be sure to draw them! (does this so filter list remains visible)
                    using var child = ImRaii.Child( "Child", new Vector2( ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X, 200),true);
                    // We will draw out one listing for each item.
                    foreach( var item in contentList.Keys ) {
                        // If our item is selected, set it and break
                        if( ImGui.Selectable( item, item == selectedTypes[layerIndex] ) ) {
                            selectedTypes[layerIndex] = item;
                            SaveConfig(); // also update the config so we can see.
                            break;
                        }
                    }
                }
            }
            // Finished with the combo, now we can close out of the function.
        }
        // Out of function here!
    }
}