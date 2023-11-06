using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using OtterGui;
using OtterGui.Raii;
﻿using Dalamud.Game.Text;
using Dalamud.Plugin;
using System.Diagnostics;
using Num = System.Numerics;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using OtterGui.Widgets;
using Dalamud.Interface;

namespace GagSpeak.UI.Tabs.HelpPageTab;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class HelpPageTab : ITab
{
    // Begin by appending the readonlys and privates
    private readonly GagSpeakConfig _config;
    private readonly UiBuilder _uiBuilder;

    public HelpPageTab(GagSpeakConfig config, UiBuilder uiBuilder)
    {
        // Set the readonlys
        _config = config;
        _uiBuilder = uiBuilder;
    }

    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label
        => "HelpPage"u8;

    /// <summary>
    /// This Function draws the content for the window of the ConfigSettings Tab
    /// </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;


        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("HelpPageChild"))
        {
            DrawHeader();
            DrawHelpPage();
        }
    }

    private void DrawHeader()
        => WindowHeader.Draw("Plugin Information & Usage", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    // Draw the actual config settings
    private void DrawHelpPage() {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##HelpPagePanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // Overview page, display to the user a brief description of how the plugin functions
        ImGui.Text("Overview:");
        ImGui.Text("This Dalamud Plugin is used to enhance role-playing by simulating speech when gagged.");
        ImGui.Text("The plugin allows you to equip up to 3 gags, each with their own garble level. The");
        ImGui.Text("garble level determines how much your speech is restricted.");
        ImGui.Text("Players can send muffled messages by typing /gsm before their message.");
        ImGui.Separator();
        ImGui.Text("GENERAL TAB:");
        ImGui.Text("The General Tab provides a place to set your safeword, which currently has no");
        ImGui.Text("functional use. In the future, using /gagspeak (your safeword) will automatically");
        ImGui.Text("remove all gags and locks from your player, regardless of your status.");
        ImGui.Text("Setting your mode to dominant or submissive currently has limited functionality.");
        ImGui.Text("Currently, setting your mode to dominant prevents you from being gagged by others.");
        ImGui.Separator();
        ImGui.Text("GAG LISTINGS:");
        ImGui.Text("The Gag Listings section allows you to be gagged with up to 3 different kinds of gags.");
        ImGui.Text("Below each Gagtype dropdown is a second dropdown. This dropdown handles the type of");
        ImGui.Text("Padlock that can lock its associated gag. Once a gag has an associated lock, it");
        ImGui.Text("cannot be removed.");
        ImGui.Text("Each gag you wear has a garble level, determining how much your speech is restricted.");
        ImGui.Text("You can check this under the debug display option in the config and settings tab.");
        ImGui.NewLine();
        ImGui.Text("You can automate equipping gags via: /gag [layer] [gagtype] | [playername]@[world].");
        ImGui.Text("Example Use: /gag 1 Ball Gag | FirstName LastName@Balmung.");
        ImGui.NewLine();
        ImGui.Text("You can lock worn gags with: /gag lock [layer] [gagtype] | [playername]@[world].");
        ImGui.NewLine();
        ImGui.Text("For password lock types: /gag lock [layer] [gagtype] [password] | [playername]@[world].");
        ImGui.Separator();
        ImGui.Text("WHITELIST:");
        ImGui.Text("Players can be added to the whitelist by being within range of another player they");
        ImGui.Text("want to add in-game, and selecting 'Add Player.' Anyone on this whitelist will be");
        ImGui.Text("put under the player filter whitelist in the config settings tab. The whitelist");
        ImGui.Text("will have features in the future allowing you to see their last updated information");
        ImGui.Text("or current mistress, among other information.");
        ImGui.Separator();
        ImGui.Text("SETTINGS:");
        ImGui.Text("This is where you can set up where your muffled sentences are displayed.");
        ImGui.Text("You can use enabled channel filters, allowing muffled sentences to be");
        ImGui.Text("displayed in any checked-off channels. Additionally, you can use the Only Friends");
        ImGui.Text("option to only allow friends to gag you, or the Only Party Members option to only");
        ImGui.Text("allow party members to gag you. The Only Whitelist option allows you to only allow");
        ImGui.Text("whitelisted players to gag you. You can also enable debug mode to see information.");
        ImGui.Separator();
        ImGui.Text("AUTOMATION:");
        ImGui.Text("Use /gag, /gsm, or /gagspeak without arguments for more information on these commands.");

        ImGui.Text("Overview:");
        ImGui.Text("This Dalamud Plugin is used to enhance role-playing by simulating speech when gagged.");
        ImGui.Text("The plugin allows you to equip up to 3 gags, each with their own garble level. The ");
        ImGui.Text("garble level determines how much your speech is restricted.");
        ImGui.Text("Players can send muffled messages by typing /gsm before their message");
        ImGui.Separator();
        ImGui.Text("GENERAL TAB:");
        ImGui.Text("The General Tab provides a place to set your safeword, which currently has no ");
        ImGui.Text("functional use. In the future, using /gagspeak (your safeword) will automatically");
        ImGui.Text("remove all gags and locks from your player, regardless of your status.");
        ImGui.Text("Setting your mode to dominant or submissive currently has limited functionality.");
        ImGui.Text("Currently, setting your mode to dominant prevents you from being gagged by others.");
        ImGui.Separator();
        ImGui.Text("GAG LISTINGS:");
        ImGui.Text("The Gag Listings section allows you to be gagged with up to 3 different kinds of gags.");
        ImGui.Text("Below Each Gagtype dropdown is a second dropdown. This dropdown handles the type of");
        ImGui.Text("Padlock that can lock its associated gag. Once a gag has an associated lock, it");
        ImGui.Text("cannot be removed.");
        ImGui.Text("Each gag you wear has a garble level, determining how much your speech is restricted.");
        ImGui.Text("much your speech is restricted. You can check this under the debug display option");
        ImGui.Text("in the config and settings tab.");
        ImGui.NewLine();
        ImGui.Text("You can automate equipping gags via: /gag [layer] [gagtype] | [playername]@[world]");
        ImGui.Text("Example Use: /gag 1 Ball Gag | FirstName LastName@Balmung");
        ImGui.NewLine();
        ImGui.Text("You can lock worn gags with: /gag lock [layer] [gagtype] | [playername]@[world]");
        ImGui.NewLine();
        ImGui.Text("For password lock types: /gag lock [layer] [gagtype] [password] | [playername]@[world]");
        ImGui.Separator();
        ImGui.Text("WHITELIST:");
        ImGui.Text("Players can be added to the whitelist by being within range of another player they");
        ImGui.Text("want to add in-game, and selecting 'Add Player.' Anyone on this whitelist will be");
        ImGui.Text("put under the player filter whitelist in the config settings tab. The whitelist");
        ImGui.Text("will have features in the future allowing you to see their last updated information");
        ImGui.Text("or current mistress, among other information.");
        ImGui.Separator();
        ImGui.Text("SETTINGS:");
        ImGui.Text("This is where you can setup where your muffled sentences are displayed too.");
        ImGui.Text("You can use enabled channel filters, which only allows muffled sentences to be");
        ImGui.Text("displayed in any checked off channels. Additionally, you can use the Only Friends");
        ImGui.Text("option to only allow friends to gag you, or the Only Party Members option to only");
        ImGui.Text("allow party members to gag you. The Only Whitelist option allows you to only allow");
        ImGui.Text("whitelisted players to gag you. You can also enable debug mode to see information.");
        ImGui.Separator();
        ImGui.Text("AUTOMATION:");
        ImGui.Text("Use /gag, /gsm, or /gagspeak without arguments for more information on these commands.");
    }
}

#pragma warning restore IDE1006