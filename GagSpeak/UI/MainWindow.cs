using Dalamud.Game.Text;
using Dalamud.Plugin;
using ImGuiNET;
using OtterGui.Custom;
using OtterGui.Widgets;
using Dalamud.Interface.Colors;
using System.Linq;
using System;
using System.Diagnostics;
using Num = System.Numerics;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface.Utility;


// Practicing Modular Design
using GagSpeak.Events;
using GagSpeak.UI.Tabs;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;

namespace GagSpeak.UI;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class MainWindow : Window //, IDisposable
{
    public enum TabType 
    {
        None            = -1,
        General         = 0, // Where you select your gags and safewords and lock types. Put into own tab for future proofing beauty spam
        Whitelist       = 1, // Where you can append peoples names to a whitelist, which is used to account for permissions on command usage.
        ConfigSettings  = 2 // Where you can change the plugin settings, such as debug mode, and other things.
    }

    // Private readonly variables here, fill in rest later. (or rather take out)
    private readonly GagSpeakConfig  _config; // Might just be used for debug stuff, may be able to remove.
    // private readonly TabSelected    _event;
    private readonly ITab[]         _tabs;

    // Readonly variables for the tabs
    public readonly GeneralTab          General;
    public readonly WhitelistTab        Whitelist;
    public readonly ConfigSettingsTab   ConfigSettings;
    public TabType SelectTab = TabType.None; // What tab is selected?
    public MainWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, GeneralTab general,
        WhitelistTab whitelist, ConfigSettingsTab configsettings): base(GetLabel())
    {
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;

        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(700, 675),     // Minimum size of the window
            MaximumSize = ImGui.GetIO().DisplaySize, // Maximum size of the window
        };

        // set the private readonly's to the passed in data of the respective names
        General = general;
        Whitelist = whitelist;
        ConfigSettings = configsettings;
        // Below are the stuff besides the tabs that are passed through
        //_event     = @event;
        _config    = config;
        // the tabs to be displayed
        _tabs = new ITab[]
        {
            general,
            whitelist,
            configsettings,
        };
        // lets subscribe to the event so we can write custom code once this event is raised.
       // _event.Subscribe(OnTabSelected, TabSelected.Priority.MainWindow);
    }

    // Prepare to draw the sick window thing
    public override void PreDraw() {
        // Before we draw, lets lock the window in place, just so until its finished being drawn, we dont have anyone dragging shit everywhere.
        // It also helps to make sure the very modular precent based widths we will configure function properly.
        Flags = _config.LockMainWindow
            ? Flags | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize
            : Flags & ~(ImGuiWindowFlags.NoMove |ImGuiWindowFlags.NoResize);
    }

    // The function called when the mainwindow must be DISPOSED OF
    // public void Dispose()
    //     => _event.Unsubscribe(OnTabSelected); // Best to unsubscribe to events we subscribed to after disposal!


    // Now that we are THE READY. Yis. Let us, LE DRAW (This should be called by the windowManager)
    public override void Draw() {
        // get our cursors Y position and store it into YPOS
        var yPos = ImGui.GetCursorPosY();

        // Lets begin drawing the top bar of the UI, which will contain all of the tabs, And an optional few buttons to trigger additional pop-out windows maybe.
        /// FORMAT: [string label]              - The label for the tab bar (its identification tag)
        /// [ImGuiTabBarFlags flags]            - Any flags we want to include in the design of the tab bar
        /// [ReadOnlySpan<byte> selectTab]      - The tab that we are currently on.
        /// [out ReadOnlySpan<byte> currentTab] - the current tab selected we are on, read in byte span format
        /// [Action buttons]                    - UNSURE ATM
        /// [params ITab[] tabs]                - the list of tabs that will will want to have displayed
        if (TabBar.Draw("##tabs", ImGuiTabBarFlags.None, ToLabel(SelectTab), out var currentTab, () => { }, _tabs)) {
            SelectTab           = TabType.None; // set the selected tab to none
            _config.SelectedTab = FromLabel(currentTab); // set the config selected tab to the current tab
            //_config.Save(); // FIND OUT HOW TO USE SaveConfig(); ACROSS CLASSES LATER.
        }

        // We want to display the save & close, and the donation buttons on the topright, so lets draw those as well.
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - 10 * ImGui.GetFrameHeight(), yPos - ImGuiHelpers.GlobalScale));
        // Can use basic stuff for now, but if you want to look into locking buttons, reference glamourer's button / checkbox code.
        if (ImGui.Button("Save and Close Config")) {
            // SaveConfig(); <-- Make this actually save and close config.
        }

        // This appears to use a completely seperate style of ImGui drawing, look into further later.

        // In that same line...
        ImGui.SameLine();
        // Configure the style for our next button
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        ImGui.Text(" ");
        ImGui.SameLine();

        // And now have that button be for the Ko-Fi Link
        if (ImGui.Button("Tip Cordy for her hard work!")) {
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
            _                       => ReadOnlySpan<byte>.Empty, // This label confuses me a bit. I think it is just a blank label?
        };


    // Function to determine which tab we are going from when switching tabs
    private TabType FromLabel(ReadOnlySpan<byte> label) {
        // @formatter:off
        if (label == General.Label)     return TabType.General;
        if (label == Whitelist.Label)    return TabType.Whitelist;
        if (label == ConfigSettings.Label)   return TabType.ConfigSettings;
        // @formatter:on
        return TabType.None;
    }

    // Cordy Note: General Support Group buttons, not nessisary for overall design
    // / <summary> Draw the support button group on the right-hand side of the window. </summary>
    // public static void DrawSupportButtons(Changelog changelog)
    // {
    //     var width = ImGui.CalcTextSize("Join Discord for Support").X + ImGui.GetStyle().FramePadding.X * 2;
    //     var xPos  = ImGui.GetWindowWidth() - width;
    //     // Respect the scroll bar width.
    //     if (ImGui.GetScrollMaxY() > 0)
    //         xPos -= ImGui.GetStyle().ScrollbarSize + ImGui.GetStyle().FramePadding.X;

    //     ImGui.SetCursorPos(new Vector2(xPos, 0));
    //     CustomGui.DrawDiscordButton("Sample Text", width);

    //     ImGui.SetCursorPos(new Vector2(xPos, ImGui.GetFrameHeightWithSpacing()));
    //     CustomGui.DrawGuideButton(Glamourer.Messager, width);

    //     ImGui.SetCursorPos(new Vector2(xPos, 2 * ImGui.GetFrameHeightWithSpacing()));
    //     if (ImGui.Button("Show Changelogs", new Vector2(width, 0)))
    //         changelog.ForceOpen = true;
    // }


    // this coordinates what to execute once the mouse clicks on a different tab than the one we are on.

    // Still stuck figuring out what the Style? _ is for, but I think it is just a blank style?
    // private void OnTabSelected(TabType type, Style? _)
    //     => SelectTab = type;

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeak###GagSpeakMainWindow";
}

#pragma warning restore IDE1006