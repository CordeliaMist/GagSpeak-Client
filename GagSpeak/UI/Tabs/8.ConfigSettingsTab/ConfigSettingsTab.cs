using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using GagSpeak.ChatMessages;
using GagSpeak.Services;
using GagSpeak.Utility;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.CharacterData;
using Dalamud.Interface.Utility;
using OtterGui;

namespace GagSpeak.UI.Tabs.ConfigSettingsTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class ConfigSettingsTab : ITab
{
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    CharacterHandler                _characterHandler;      // for getting the whitelist
    private readonly    FontService                     _fontService;           // for getting the font
    private             Dictionary<string, string[]>    _languages;             // the dictionary of languages & dialects 
    private             string[]                        _currentDialects;       // the array of language names
    private             string                          _activeDialect;         // the dialect selected
    private             string?                         _globalTriggerPhrase;        // the language selected
    private             string                          _tempPassword;          // the temp password for the safeword

    /// <summary> Initializes a new instance of the <see cref="ConfigSettingsTab"/> class. <summary>
    public ConfigSettingsTab(GagSpeakConfig config, CharacterHandler characterHandler,
    GagListingsDrawer gagListingsDrawer, GagService gagService, FontService fontService) {
        _config = config;
        _characterHandler = characterHandler;
        _fontService = fontService;
        // load the dropdown info
        _languages = new Dictionary<string, string[]> {
            { "English", new string[] { "US", "UK" } },
            { "Spanish", new string[] { "Spain", "Mexico" } },
            { "French", new string[] { "France", "Quebec" } },
            { "Japanese", new string[] { "Japan" } }
        };
        // set password to blank
        _tempPassword = "";

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
        => WindowHeader.Draw("Configuration & Settings", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0);

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawConfigSettings() {
        // Lets start by drawing the child.
        var width = ImGui.GetContentRegionAvail().X;
        using var child = ImRaii.Child("##SettingsPanel", new Vector2(width, -1), true, ImGuiWindowFlags.NoScrollbar);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        
        // set up our columns for the printing
        using (var table = ImRaii.Table("ConfigColumns", 2, ImGuiTableFlags.SizingFixedFit)) {
            if (!table) { return; }
            // draw out the table
            ImGui.TableSetupColumn("ConfigColumn1", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale*275);
            ImGui.TableSetupColumn("ConfigColumn2", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var yPos = ImGui.GetCursorPosY();
            ImGui.SetCursorPosY(yPos - 2*ImGuiHelpers.GlobalScale);
            ///////////////////////////// HARDCORE MODE /////////////////////////////\
            if(_config.hardcoreMode) {ImGui.BeginDisabled();}
            try{
                var tmp = _config.hardcoreMode;
                if (ImGui.Checkbox("##Hardcore Mode", ref tmp) && tmp != _config.hardcoreMode) {
                    // open the popup
                    ImGui.OpenPopup("Hardcore Warning");
                }
                ImGui.SameLine();
                ImGuiUtil.LabeledHelpMarker("Hardcore Mode", "CAN ONLY BE TURNED OFF WITH A SAFEWORD, USE WITH CAUTION\n"+
                "Enabling removes ability to toggle any options in whitelist tab once two-way commitment is made.");

                ImGui.SetNextWindowSize(new Vector2(750, 375));
                if (ImGui.BeginPopup("Hardcore Warning")) {
                    ImGui.PushFont(_fontService.UidFont);
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                    try{
                        ImGuiUtil.Center("READ THIS WARNING BEFORE HITTING CONFIRM");
                        ImGui.Separator();
                    } finally {
                        ImGui.PopStyleColor();
                        ImGui.PopFont();
                    }
                    ImGui.Spacing();
                    ImGui.PushFont(_fontService.UidFont);
                    try{
                        ImGuiUtil.Center("Hardcore mode, once enabled, CAN ONLY BE DISABLED WITH A SAFEWORD.");
                        ImGui.Spacing();
                        ImGuiUtil.Center("Your settings for someone in whitelist become locked after a two-way commitment.");
                        ImGui.Spacing();
                        ImGuiUtil.Center("Ensure you're comfortable with your dynamic tiers before enabling.");
                        ImGui.Spacing();
                        ImGuiUtil.Center("In the hardcore tab, you can customize settings for each player.");
                        ImGui.Spacing();
                        ImGuiUtil.Center("No modifications can be made in the hardcore tab by anyone not in your whitelist.");
                        ImGui.Spacing();
                        ImGuiUtil.Center("You cannot trigger anything in the hardcore tab yourself.");
                        ImGui.Spacing();
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                        ImGui.Separator();
                        ImGuiUtil.Center("Surrender your Movement, Speech, Hotbar, Vision, Recast Speed, & Dialogue Options?...");
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGuiHelpers.GlobalScale*700/2-ImGuiHelpers.GlobalScale*350/2));
                        if (ImGui.Button("CONFIRM", new Vector2(ImGuiHelpers.GlobalScale*350, 0))) {
                            _config.SetHardcoreMode(!_config.hardcoreMode);
                            ImGui.CloseCurrentPopup();
                        }
                    } finally {
                        ImGui.PopStyleColor();
                        ImGui.PopFont();
                    }
                    ImGui.EndPopup();
                }
            } finally {
                if(_config.hardcoreMode) {ImGui.EndDisabled();}
            }
            ///////////////////////////// COMMAND SETTINGS /////////////////////////////
            // should we allow commands from friends not in whitelist?
            UIHelpers.CheckboxNoConfig("Commands From Friends", 
                "Commands & Interactions from other players are only recieved by GagSpeak if in your Friend List.",
                _characterHandler.playerChar._doCmdsFromFriends,
                v => _characterHandler.ToggleCmdFromFriends()
            );
            // should we allow commands from party members not in whitelist?
            UIHelpers.CheckboxNoConfig("Commands From Party",
                "Commands & Interactions from other players are only recieved by GagSpeak if in the Party List.",
                _characterHandler.playerChar._doCmdsFromParty,
                v => _characterHandler.ToggleCmdFromParty()
            );
            // Direct chat garbler, is it enabled?
            if(_characterHandler.playerChar._directChatGarblerLocked) {ImGui.BeginDisabled();}
            try
            {
                UIHelpers.CheckboxNoConfig("Direct Chat Garbler",
                    "AUTOMATICALLY Translate any NON-COMMAND chat message to gagspeak.\n\n"+
                    ">> This will ONLY occur in any of the checked off channels under ENABLED CHANNELS below.\n\n"+
                    ">> This is Serverside, just like /gsm.",
                    _characterHandler.playerChar._directChatGarblerActive, 
                    v => _characterHandler.ToggleDirectChatGarbler()
                );
            } finally {
                if(_characterHandler.playerChar._directChatGarblerLocked) {ImGui.EndDisabled();}
            }
            // should we enable the direct chat garbler every time we change zones?
            UIHelpers.CheckboxNoConfig("Garbler Zone Warnings",
                "Sends a warning to your chat every time you switch zones when direct chat garbler is enabled.\n",
                _characterHandler.playerChar._liveGarblerWarnOnZoneChange,
                v => _characterHandler.ToggleZoneWarnings()
            );
            UIHelpers.CheckboxNoConfig("Auto-Open UI",
                "Opens the GagSpeak UI on login or plugin enable automatically",
                _config.UiOpenOnEnable,
                v => _config.SetUiOpenOnEnable(!_config.UiOpenOnEnable)
            );
        

            ///////////////////////////// WARDROBE SETTINGS /////////////////////////////
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
            ImGui.Text("Wardrobe Settings:");
            UIHelpers.CheckboxNoConfig("Enable Wardrobe",
                "Must be enabled for anything in the Kink Wardrobe component of GagSpeak to function.",
                _characterHandler.playerChar._enableWardrobe,
                v => _characterHandler.ToggleEnableWardrobe()
            );

            UIHelpers.CheckboxNoConfig("Gag Items Auto-Equip",
                "If this option is enabled, anyone in your whitelist that applies a gag to you, applies the GagStorage item as well.\n"+
                ">> This does not happen regardless if that item is disabled in your GagStorage.",
                _characterHandler.playerChar._allowItemAutoEquip,
                v => _characterHandler.ToggleGagItemAutoEquip()
            );

            UIHelpers.CheckboxNoConfig("Restraint Sets Auto-Equip",
                "If enabled, anyone in your whitelist, any restraint set that is enabled will auto equip.\n"+
                ">> You can manually disable this in the wardrobe tab so they only auto equip when you want them to.",
                _characterHandler.playerChar._allowRestraintSetAutoEquip,
                v => _characterHandler.ToggleRestraintSetAutoEquip()
            );

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X*.75f);
            RevertStyle prevRevertStyle = _characterHandler.playerChar._revertStyle; // to only execute code to update data once it is changed
            if (ImGui.BeginCombo("##RevertStyle", GagSpeakConfig.GetRevertStyleAlias(_characterHandler.playerChar._revertStyle), ImGuiComboFlags.NoArrowButton)) {
                foreach (RevertStyle style in Enum.GetValues(typeof(RevertStyle))) {
                    bool isSelected = (_characterHandler.playerChar._revertStyle == style);
                    if (ImGui.Selectable(GagSpeakConfig.GetRevertStyleAlias(style), isSelected)) {
                        _characterHandler.SetRevertStyle(style);
                        GSLogger.LogType.Debug($"[ConfigSettingsTab] RevertStyle changed to: {GagSpeakConfig.GetRevertStyleAlias(_characterHandler.playerChar._revertStyle)}");
                    }
                    if (isSelected) {
                        ImGui.SetItemDefaultFocus();
                    }
                }
                ImGui.EndCombo();
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Select how you want your attire to revert when a restraint set is removed.");
            }
            ///////////////////////////// PUPPETEER SETTINGS /////////////////////////////
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
            ImGui.Text("Puppeteer Settings:");
            UIHelpers.CheckboxNoConfig("Enable Puppeteer",
                "Allows the use of the Puppeteer Module of GagSpeak.\n"+
                ">> This will allow anyone in your whitelist to use the Puppeteer Module on you who is allowed.",
                _characterHandler.playerChar._allowPuppeteer,
                v => _characterHandler.TogglePuppeteer()
            );
            var result = _globalTriggerPhrase ?? _characterHandler.playerChar._globalTriggerPhrase;
            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*230);
            if (ImGui.InputTextWithHint("##GlobalTriggerPhrase", "Global Trigger Phrase (Hover me!)", ref result, 64, ImGuiInputTextFlags.EnterReturnsTrue)) {
                _globalTriggerPhrase = result;
            }
            if(ImGui.IsItemDeactivatedAfterEdit()) {
                _characterHandler.SetGlobalTriggerPhrase(result);
                _globalTriggerPhrase = null;
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Set the trigger phrase that is accessable to EVERYONE. (Leave blank to disable)");
            }
            // draw out the global permissions
            var checkbox1Value = _characterHandler.playerChar._globalAllowSitRequests;
            var checkbox2Value = _characterHandler.playerChar._globalAllowMotionRequests;
            var checkbox3Value = _characterHandler.playerChar._globalAllowAllCommands;
            UIHelpers.CheckboxNoConfig("Sit",
                "Allows sit commands for EVERYONE who uses the global trigger phrase",
                checkbox1Value,
                v => _characterHandler.SetGlobalAllowSitRequests(!_characterHandler.playerChar._globalAllowSitRequests)
            );
            ImGui.SameLine();
            UIHelpers.CheckboxNoConfig("Motion",
                "Allows motion commands for EVERYONE who uses the global trigger phrase",
                checkbox2Value,
                v => _characterHandler.SetGlobalAllowMotionRequests(!_characterHandler.playerChar._globalAllowMotionRequests)
            );
            ImGui.SameLine();
            UIHelpers.CheckboxNoConfig("All",
                "Allows all commands for EVERYONE who uses the global trigger phrase",
                checkbox3Value,
                v => _characterHandler.SetGlobalAllowAllCommands(!_characterHandler.playerChar._globalAllowAllCommands)
            );


            ///////////////////////////// TOYBOX SETTINGS /////////////////////////////
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
            ImGui.Text("Toybox Settings:");
            UIHelpers.CheckboxNoConfig("Enable Toybox", 
                "Allows the use of the Toybox Module of GagSpeak.\n"+
                ">> This will allow anyone in your whitelist to use the Toybox Module on you who is allowed.",
                _characterHandler.playerChar._enableToybox,
                v => _characterHandler.ToggleEnableToybox()
            );


            // channel listings
            ImGui.TableNextColumn();
            // Show Debug Menu when Debug logging is enabled
            if(_characterHandler.playerChar._directChatGarblerLocked == true) {ImGui.BeginDisabled();}
            try
            {
                yPos = ImGui.GetCursorPosY();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("GagSpeak Channels:");
                if(ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Every selected channel from here becomes a channel that your direct chat garbler works in.");
                }
                ImGui.SameLine();
                // Create the language dropdown
                ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*65);
                string prevLang = _config.language; // to only execute code to update data once it is changed
                if (ImGui.BeginCombo("##Language", _config.language, ImGuiComboFlags.NoArrowButton)) {
                    foreach (var language in _languages.Keys.ToArray()) {
                        bool isSelected = (_config.language == language);
                        if (ImGui.Selectable(language, isSelected)) {
                            _config.language = language;
                        }
                        if (isSelected) {
                            ImGui.SetItemDefaultFocus();
                        }
                    }
                    ImGui.EndCombo();
                }
                if(ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Select the language you want to use for GagSpeak.");
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
                ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*55);
                string[] dialects = _languages[_config.language];
                string prevDialect = _activeDialect; // to only execute code to update data once it is changed
                if (ImGui.BeginCombo("##Dialect", _activeDialect, ImGuiComboFlags.NoArrowButton)) {
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
                if(ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Select the Dialect you want to use for GagSpeak.");
                }
                //update if changed
                if (prevDialect != _activeDialect) { // set the dialect to the newly selected dialect once it is changed
                    SetConfigDialectFromDialect(_activeDialect);
                    _config.Save();
                }

                // display the channels
                var i = 0;
                foreach (var e in ChatChannel.GetOrderedChannels()) {
                    // See if it is already enabled by default
                    var enabled = _config.ChannelsGagSpeak.Contains(e);
                    // Create a new line after every 4 columns
                    if (i != 0 && (i==4 || i==7 || i==11 || i==15 || i == 19)) {
                        ImGui.NewLine();
                        //i = 0;
                    }
                    // Move to the next row if it is LS1 or CWLS1
                    if (e is ChatChannel.ChatChannels.LS1 or ChatChannel.ChatChannels.CWL1)
                        ImGui.Separator();

                    if (ImGui.Checkbox($"{e}", ref enabled)) {
                        // See If the UIHelpers.Checkbox is clicked, If not, add to the list of enabled channels, otherwise, remove it.
                        if (enabled) _config.ChannelsGagSpeak.Add(e);
                        else _config.ChannelsGagSpeak.Remove(e);
                        _config.Save();
                    }

                    ImGui.SameLine();
                    i++;
                }
            } finally {
                if(_characterHandler.playerChar._directChatGarblerLocked == true) {ImGui.EndDisabled();}
            }

            ImGui.NewLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Puppeteer Channels:");
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Every selected channel from here becomes a channel that you will pick up your trigger word from.\n"+
                "The Global Puppeteer trigger works in all channels, and cannot be configured.");
            }
            var j = 0;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
            foreach (var f in ChatChannel.GetOrderedChannels()) {
                // See if it is already enabled by default
                var enabledPuppet = _config.ChannelsPuppeteer.Contains(f);
                // Create a new line after every 4 columns
                if (j != 0 && (j==4 || j==7 || j==11 || j==15 || j == 19)) {
                    ImGui.NewLine();
                    //i = 0;
                }
                // Move to the next row if it is LS1 or CWLS1
                if (f is ChatChannel.ChatChannels.LS1 or ChatChannel.ChatChannels.CWL1)
                    ImGui.Separator();

                if (ImGui.Checkbox($"{f}##{f}_puppeteer", ref enabledPuppet)) {
                    // See If the UIHelpers.Checkbox is clicked, If not, add to the list of enabled channels, otherwise, remove it.
                    if (enabledPuppet) _config.ChannelsPuppeteer.Add(f);
                    else _config.ChannelsPuppeteer.Remove(f);
                    _config.Save();
                }

                ImGui.SameLine();
                j++;
            }
            ImGui.PopStyleVar();
            // admin key password field
            ImGui.NewLine();
            ImGui.Spacing();
            var password  = _tempPassword; // temp storage to hold until we de-select the text input
            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*125);
            if (ImGui.InputTextWithHint($"BetaTester: {_config.AdminMode}##TestingPassKey", "TestingPassKey", ref password, 128, ImGuiInputTextFlags.None))
                _tempPassword = password;
            if (ImGui.IsItemDeactivatedAfterEdit()) { // will only update our safeword once we click away from the safeword bar
                var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                var base64Password = System.Convert.ToBase64String(passwordBytes);
                if(base64Password == "QmV0YSBUZXN0aW5nIEtleSBGb3IgVHJpZ2dlciBUZXN0aW5nIFdvdyBTbyBjb29sIG9tZyBoZWhl") {
                    _config.AdminMode = true;
                } else {
                    _config.AdminMode = false;
                }
                _tempPassword = "";
            }
        }
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
