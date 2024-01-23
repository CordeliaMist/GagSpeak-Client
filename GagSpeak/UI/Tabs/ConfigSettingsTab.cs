using System;
using System.Numerics;
using System.IO;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using Dalamud.Plugin;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using GagSpeak.Data;
using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.ComboListings;
using System.Collections.Generic;
using System.Linq;

namespace GagSpeak.UI.Tabs.ConfigSettingsTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class ConfigSettingsTab : ITab
{
    private readonly    IDalamudTextureWrap             _dalamudTextureWrap;    // for loading images
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    UiBuilder                       _uiBuilder;             // for loading images
    private             Dictionary<string, string[]>    _languages;             // the dictionary of languages & dialects 
    private             string[]                        _currentDialects;       // the array of language names
    private             string                          _activeDialect;         // the dialect selected

    /// <summary> Initializes a new instance of the <see cref="ConfigSettingsTab"/> class. <summary>
    public ConfigSettingsTab(GagSpeakConfig config, UiBuilder uiBuilder, DalamudPluginInterface pluginInterface,
    GagListingsDrawer gagListingsDrawer, GagService gagService) {
        _config = config;
        _uiBuilder = uiBuilder;
        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "iconUI.png");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        // sets the icon
        _dalamudTextureWrap = IconImage;
        // load the dropdown info
        _languages = new Dictionary<string, string[]> {
            { "English", new string[] { "US", "UK" } },
            { "Spanish", new string[] { "Spain", "Mexico" } },
            { "French", new string[] { "France", "Quebec" } },
            { "Japanese", new string[] { "Japan" } }
        };

        _currentDialects = _languages[_config.language]; // put all dialects into an array
        _activeDialect = GetDialectFromConfigDialect();  // set the active dialect to the one in the config
    }

    public ReadOnlySpan<byte> Label => "Settings"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;
        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("SettingsChild")) {
            DrawHeader();
            DrawConfigSettings();
        }
    }

    /// <summary> This Function draws the header for the ConfigSettings Tab </summary>
    private void DrawHeader()
        => WindowHeader.Draw("Configuration & Settings", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawConfigSettings() {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##SettingsPanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        ImGui.Columns(2,"ConfigColumns", false);
        ImGui.SetColumnWidth(0, ImGui.GetWindowWidth() / 2 - 10);
        // UIHelpers.Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        UIHelpers.Checkbox("Only Friends", "Only processes process /gag (target) commands from others if they are on your friend list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.friendsOnly, v => _config.friendsOnly = v, _config);
        // UIHelpers.Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        UIHelpers.Checkbox("Only Party Members", "Only processes /gag (target) commands from others if they are in your current party list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.partyOnly, v => _config.partyOnly = v, _config);
        // UIHelpers.Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        UIHelpers.Checkbox("Only Whitelist", "Only processes /gag (target) commands from others if they are in your whitelist.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.whitelistOnly, v => _config.whitelistOnly = v, _config);
        // only allow UIHelpers.Checkbox interaction iif _config.LockDirectChatGarbler is false
        if(_config.LockDirectChatGarbler == true) {ImGui.BeginDisabled();}
        UIHelpers.Checkbox("Direct Chat Garbler", "Automatically translate chat messages in all enabled channels to gagspeak automatically. "+
            "(WITHOUT commands).\nThis does make use of chat to server interception. Even though now it is ensured safe, always turn this OFF "+
            "after any patch or game update, until the plug curator says it's safe", _config.DirectChatGarbler, v => _config.DirectChatGarbler = v, _config);
        if(_config.LockDirectChatGarbler == true) {ImGui.EndDisabled();}
        // UiHelpers.Checkbox will dictate if any of the wardrobe component of gagspeak is enabled or not.
        UIHelpers.Checkbox("Enable Wardrobe", "Allows GagSpeak to execute any components of the plugin that interact with "+
            "glamourer. Including Restraint locking & Item Auto-Equip.\n PLEASE NOTE: Even if enabled, only someone who you have a pet "+
            "or slave relation to can use these features on you.", _config.enableWardrobe, v => _config.enableWardrobe = v, _config);
        // UiHelpers.Checkbox will dictate if Item Auto-Equip has permission to activate (will likely render enableWardrobe absolete).
        UIHelpers.Checkbox("Allow Item Auto-Equip", "Overrides all wardrobe gag items Auto-Equip setting, Acting as an Override for a fast "+
        "and quick disable for your privacy needs.", _config.allowItemAutoEquip, v => _config.allowItemAutoEquip = v, _config);
        // UiHelpers.Checkbox will dictate if Anyone who is your Mistress that you are a slave to, will have access to lock any of your restraint sets on.
        UIHelpers.Checkbox("Allow Restraint Locking [WIP]", "Anyone who is your Mistress that you are a slave to, will have access to lock your "+
            "restraint set on you.", _config.allowRestraintLocking, v => _config.allowRestraintLocking = v, _config);
        // UiHelpers.Checkbox will dictate if Anyone who is your Mistress that you are a slave to, will have full control over your restraint sets & locking/unlocking
        UIHelpers.Checkbox("Allow Restraint Control [WIP]", "Anyone who is your Mistress that you are a slave to, will have full control over your "+
            "restraint sets & locking/unlocking.", _config.surrenderRestraintControl, v => _config.surrenderRestraintControl = v, _config);
        
        // Create the language dropdown
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X/2);
        string prevLang = _config.language; // to only execute code to update data once it is changed
        if (ImGui.BeginCombo("##Language", _config.language)) {
            foreach (var language in _languages.Keys.ToArray()) {
                bool isSelected = (_config.language == language);
                if (ImGui.Selectable(language, isSelected)) {
                    _config.language = language;
                    GagSpeak.Log.Debug($"[ConfigSettingsTab] Language changed to: {_config.language}");
                }
                if (isSelected) {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        //update if changed 
        if (prevLang != _config.language) { // set the language to the newly selected language once it is changed
            _currentDialects = _languages[_config.language]; // update the dialects for the new language
            _activeDialect = _currentDialects[0]; // set the active dialect to the first dialect of the new language
            SetConfigDialectFromDialect(_activeDialect);
            _config.Save();
        }
        ImGui.SameLine(); 
        // Create the dialect dropdown
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X-25);
        string[] dialects = _languages[_config.language];
        string prevDialect = _activeDialect; // to only execute code to update data once it is changed
        if (ImGui.BeginCombo("##Dialect", _activeDialect)) {
            foreach (var dialect in dialects) {
                bool isSelected = (_activeDialect == dialect);
                if (ImGui.Selectable(dialect, isSelected)) {
                    _activeDialect = dialect;
                }
                if (isSelected) {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        //update if changed
        if (prevDialect != _activeDialect) { // set the dialect to the newly selected dialect once it is changed
            SetConfigDialectFromDialect(_activeDialect);
            _config.Save();
        }

        // channel listings
        ImGui.NextColumn();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10);
        // you might normally want to embed resources and load them from the manifest stream
        ImGui.Image(_dalamudTextureWrap.ImGuiHandle, new Vector2(_dalamudTextureWrap.Width, _dalamudTextureWrap.Height));
        ImGui.Columns(1);

        // Show Debug Menu when Debug logging is enabled
        if(_config.LockDirectChatGarbler == true) {ImGui.BeginDisabled();}
        ImGui.Text("Enabled Channels:"); ImGui.Separator();
        var i = 0;
        foreach (var e in ChatChannel.GetOrderedChannels()) {
            // See if it is already enabled by default
            var enabled = _config.Channels.Contains(e);
            // Create a new line after every 4 columns
            if (i != 0 && (i==5 || i==9 || i==13 || i==17 || i == 21)) {
                ImGui.NewLine();
                //i = 0;
            }
            // Move to the next row if it is LS1 or CWLS1
            if (e is ChatChannel.ChatChannels.LS1 or ChatChannel.ChatChannels.CWL1)
                ImGui.Separator();

            if (ImGui.Checkbox($"{e}", ref enabled)) {
                // See If the UIHelpers.Checkbox is clicked, If not, add to the list of enabled channels, otherwise, remove it.
                if (enabled) _config.Channels.Add(e);
                else _config.Channels.Remove(e);
            }

            ImGui.SameLine();
            i++;
        }
        // Set the columns back to 1 now and space over to next section
        ImGui.Columns(1);
        ImGui.PopStyleVar();
        if(_config.LockDirectChatGarbler == true) {ImGui.EndDisabled();}
    }

    /// <summary>
    /// Used to restore the dropdown to the selection from the config
    /// </summary>
    private string GetDialectFromConfigDialect() {
        switch (_config.languageDialect) {
            case "IPA_US": return "US";
            case "IPA_UK": return "UK";
            case "IPA_FRENCH": return "France";
            case "IPA_QUEBEC": return "Quebec";
            case "IPA_JAPAN": return "Japan";
            case "IPA_SPAIN": return "Spain";
            case "IPA_MEXICO": return "Mexico";
            default: return "US";
        }
    }

    /// <summary>
    /// Sets the config dialect from dialect string selected by the dropdown.
    /// </summary>
    private void SetConfigDialectFromDialect(string dialect) {
        switch (dialect) {
            case "US": _config.languageDialect = "IPA_US"; break;
            case "UK": _config.languageDialect = "IPA_UK"; break;
            case "France": _config.languageDialect = "IPA_FRENCH"; break;
            case "Quebec": _config.languageDialect = "IPA_QUEBEC"; break;
            case "Japan": _config.languageDialect = "IPA_JAPAN"; break;
            case "Spain": _config.languageDialect = "IPA_SPAIN"; break;
            case "Mexico": _config.languageDialect = "IPA_MEXICO"; break;
            default: _config.languageDialect = "IPA_US"; break;
        }
    }
}
