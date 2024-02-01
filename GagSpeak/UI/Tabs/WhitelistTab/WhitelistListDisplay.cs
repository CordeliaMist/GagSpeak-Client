using System.Linq;
using ImGuiNET;
using GagSpeak.CharacterData;
using GagSpeak.Utility;

namespace GagSpeak.UI.Tabs.WhitelistTab;

/// <summary> This class is used to handle the whitelist tab. </summary>
public class WhitelistListDisplay
{
    private readonly    GagSpeakConfig  _config;                // store the config for the whitelist
    
    public WhitelistListDisplay(GagSpeakConfig config) {
        _config = config;
    }

    public void Draw(ref int currentWhitelistItem)
    {
        // make sure our whitelist has content.
        if(_config.whitelist.Count == 0) {
            WhitelistHelpers.AddNewWhitelistItem("None", "None", "None", _config);
            _config.Save();
        }
        
        // Create the listbox for the whitelist
        ImGui.SetNextItemWidth(ImGui.GetContentRegionMax().X-5);
        string[] whitelistNames = _config.whitelist.Select(entry => entry._name).ToArray();
        ImGui.ListBox("##whitelist", ref currentWhitelistItem, whitelistNames, whitelistNames.Length, 10);
    }
}