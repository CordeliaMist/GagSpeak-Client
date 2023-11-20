using Dalamud.Plugin;
using ImGuiNET;
using OtterGui.Widgets;
using System;
using System.Diagnostics;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface.Utility;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;
using GagSpeak.UI.Tabs.HelpPageTab;

namespace GagSpeak.UI;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class MainWindow : Window
{
    public enum TabType 
    {
        None            = -1,
        General         = 0, // Where you select your gags and safewords and lock types. Put into own tab for future proofing beauty spam
        Whitelist       = 1, // Where you can append peoples names to a whitelist, which is used to account for permissions on command usage.
        ConfigSettings  = 2, // Where you can change the plugin settings, such as debug mode, and other things.
        HelpPage        = 3 // Where you can find information on how to use the plugin, and how to get support.
    }

    // Private readonly variables here, fill in rest later. (or rather take out)
    private readonly GagSpeakConfig  _config; // Might just be used for debug stuff, may be able to remove.
    // private readonly TabSelected    _event;
    private readonly ITab[]         _tabs;

    // Readonly variables for the tabs
    public readonly GeneralTab          General;
    public readonly WhitelistTab        Whitelist;
    public readonly ConfigSettingsTab   ConfigSettings;
    public readonly HelpPageTab         HelpPage;
    public TabType SelectTab = TabType.None; // What tab is selected?
    public MainWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, GeneralTab general,
        WhitelistTab whitelist, ConfigSettingsTab configsettings, HelpPageTab helpPageTab): base(GetLabel())
    {
        // let the user know if their direct chat garlber is still enabled upon launch
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;

        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(500, 520),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };

        // set the private readonly's to the passed in data of the respective names
        General = general;
        Whitelist = whitelist;
        ConfigSettings = configsettings;
        HelpPage = helpPageTab;
        
        // Below are the stuff besides the tabs that are passed through
        //_event     = @event;
        _config    = config;
        // the tabs to be displayed
        _tabs = new ITab[]
        {
            general,
            whitelist,
            configsettings,
            helpPageTab
        };
    }

    // Prepare to draw the sick window thing
    public override void PreDraw() {
        // Before we draw, lets lock the window in place, just so until its finished being drawn, we dont have anyone dragging shit everywhere.
        // It also helps to make sure the very modular precent based widths we will configure function properly.
        Flags |= ImGuiWindowFlags.NoResize;   //<--- UNCOMMMENT THIS ONCE READY FOR RELEASE 
    }

    public override void Draw() {
        // get our cursors Y position and store it into YPOS
        var yPos = ImGui.GetCursorPosY();

        if (TabBar.Draw("##tabs", ImGuiTabBarFlags.None, ToLabel(SelectTab), out var currentTab, () => { }, _tabs)) {
            SelectTab           = TabType.None; // set the selected tab to none
            _config.SelectedTab = FromLabel(currentTab); // set the config selected tab to the current tab
            _config.Save(); // FIND OUT HOW TO USE SaveConfig(); ACROSS CLASSES LATER.
        }

        // We want to display the save & close, and the donation buttons on the topright, so lets draw those as well.
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - 10f * ImGui.GetFrameHeight(), yPos - ImGuiHelpers.GlobalScale));
        // Can use basic stuff for now, but if you want to look into locking buttons, reference glamourer's button / checkbox code.
        if (ImGui.Button("Save & Close")) {
            Toggle();
            _config.Save();
        }

        // In that same line...
        ImGui.SameLine();
        // Configure the style for our next button
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        ImGui.Text(" ");
        ImGui.SameLine();

        // And now have that button be for the Ko-Fi Link
        if (ImGui.Button("Toss Cordy a thanks!")) {
            ImGui.SetTooltip( "Only if you want to though!");
            Process.Start(new ProcessStartInfo {FileName = "https://ko-fi.com/cordeliamist", UseShellExecute = true});
        }

        ImGui.PopStyleColor(3);
    }

    // Function to determine which label we are going to when switching tabs
    private ReadOnlySpan<byte> ToLabel(TabType type)
        => type switch // we do this via a switch statement
        {
            TabType.General         => General.Label,
            TabType.Whitelist       => Whitelist.Label,
            TabType.ConfigSettings  => ConfigSettings.Label,
            TabType.HelpPage        => HelpPage.Label,
            _                       => ReadOnlySpan<byte>.Empty, // This label confuses me a bit. I think it is just a blank label?
        };


    // Function to determine which tab we are going from when switching tabs
    private TabType FromLabel(ReadOnlySpan<byte> label) {
        // @formatter:off
        if (label == General.Label)     return TabType.General;
        if (label == Whitelist.Label)    return TabType.Whitelist;
        if (label == ConfigSettings.Label)   return TabType.ConfigSettings;
        if (label == HelpPage.Label)     return TabType.HelpPage;
        // @formatter:on
        return TabType.None;
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeak###GagSpeakMainWindow";
}

#pragma warning restore IDE1006