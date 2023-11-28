using System;
using System.Numerics;
using System.Collections.Generic;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;

namespace GagSpeak.UI.Tabs.HelpPageTab;

public enum HelpTabType {
    General,
    Whitelist,
    Settings,
    Commands,
    Precautions,
    Padlocks,
    Gags
}
/// <summary>
/// This class is used to handle the help page tab.
/// </summary>
public class HelpPageTab : Window, ITab
{
    Dictionary<string, Vector4> colorDict = new Dictionary<string, Vector4> {
        { "SafetyGreen", new Vector4(0.4f, 1.0f, 0.4f, 1.0f) }, // Light green for safety keywords
        { "VibrantRed", new Vector4(1.0f, 0.0f, 0.0f, 1.0f) }, // Vibrant red for high danger levels
        { "DarkRed", new Vector4(0.75f, 0.0f, 0.0f, 1.0f) }, // Dark red for various danger levels
        { "CautionOrange", new Vector4(1.0f, 0.5f, 0.0f, 1.0f) }, // Orange for caution
        { "SectionTitleDesc", new Vector4(0.816f,0.779f, 0.0f, 1)},
        { "SectionTitle", new Vector4(1.0f, 1.0f, 0.0f, 1.0f) },
        { "SectionBullet", new Vector4(0.0f,1.0f, 1.0f, 1) },
        { "FadedText" , new Vector4(1.0f,1.0f, 1.0f, 0.8f) },
        { "KeywordBlue",  new Vector4(0.639f, 0.951f, 1, 1.0f) }, // Light but not too vibrant blue for outlining keywords
        { "RosePink", new Vector4(1.0f, 0.0f, 0.5f, 1.0f) }, // Rose pink color
        { "ForestGreen", new Vector4(0.0f, 0.5f, 0.0f, 1.0f) }, // Dark forest green
        { "Purple", new Vector4(1f, 0.0f, 1f, 1.0f) }, // Light green for safety keywords
        { "FullWhite", new Vector4(1.0f, 1.0f, 1.0f, 1.0f) }, // Full white
    };

    public HelpTabType SelectedHelpTab { get; set; }
    private readonly GagSpeakConfig _config;
    private readonly UiBuilder _uiBuilder;
    public bool isShown { get; set; }
    public HelpPageTab(GagSpeakConfig config, UiBuilder uiBuilder)
    : base("HelpPagePopOut") {
        // Set the readonlys
        _config = config;
        _uiBuilder = uiBuilder;
        // determine if the pop out window is shown
        IsOpen = false;
        // set the window fields
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(600, 450),
            MaximumSize = ImGui.GetIO().DisplaySize,
        };
    }

    // Apply our lable for the tab
    public ReadOnlySpan<byte> Label
        => "Help"u8;

    /// <summary>
    /// This function is used to draw the content of the tab.
    /// </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;
        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("HelpPageChild")){
            DrawHeader();
            DrawHelpPage();
        }
    }

    /// <summary>
    /// implement window.draw to drawcontent
    /// </summary>
    public override void Draw()
        => DrawContent();

    /// <summary>
    /// drawing out the header
    /// </summary>
    private void DrawHeader()
        => WindowHeader.Draw("Plugin Information & Usage", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), OpenPopupButton());

    /// <summary>
    /// Draw the actual help page display. This should be neatly organized and colored
    /// </summary>
    private void DrawHelpPage() {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##HelpPagePanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw the content help tabs
        DrawHelpTabs();
    }

    /// <summary>
    /// Draw the help tabs
    /// </summary>
    public void DrawHelpTabs() {
        using var _ = ImRaii.PushId( "HelpTabList" );
        using var tabBar = ImRaii.TabBar( "HelpTabs" );
        if( !tabBar ) return;

        if (ImGui.BeginTabItem("General")) {
            SelectedHelpTab = HelpTabType.General;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Whitelist")) {
            SelectedHelpTab = HelpTabType.Whitelist;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Settings")) {
            SelectedHelpTab = HelpTabType.Settings;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Commands")) {
            SelectedHelpTab = HelpTabType.Commands;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Padlocks")) {
            SelectedHelpTab = HelpTabType.Padlocks;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Transparency")) {
            SelectedHelpTab = HelpTabType.Precautions;
            ImGui.EndTabItem();
        }
        // draw the selected tab
        switch (SelectedHelpTab) {
            case HelpTabType.General:
                DrawGeneralTab();
                break;
            case HelpTabType.Whitelist:
                DrawWhitelistTab();
                break;
            case HelpTabType.Settings:
                DrawSettingsTab();
                break;
            case HelpTabType.Commands:
                DrawCommandsTab();
                break;
            case HelpTabType.Padlocks:
                DrawPadlocksTab();
                break;
            case HelpTabType.Precautions:
                DrawTransparencyTab();
                break;
        }
    }

    void DrawGeneralTab() {
        ImGui.BeginChild("##generalTabInfo");
        {
            // draw out the general tab info
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"The Safeword Field");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Serves as the ultimate safety measure."); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // push the style for the faded color text
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]);
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Can be executed via "); ImGui.SameLine();
            ImGui.TextColored(colorDict["SafetyGreen"], "/gagspeak safeword YourSafeword"); ImGui.Spacing();
            // tell them what it disabled
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine(); 
            ImGui.TextWrapped("Disables the ability for any gags or padlocks to be put on you for 15min"); ImGui.Spacing();
            // tell them how it clears your gag and lock settings
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine(); 
            ImGui.TextWrapped("Clears all personal gag and lock settings");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // now for the role buttons
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Role Buttons");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.Text(" Used to define your lean in the BDSM dynamic."); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // push the style for the faded color text
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]);
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine(); 
            ImGui.TextWrapped("Triggered by pressing either of the buttons"); ImGui.Spacing();
            // tell them how it locks you into your role for 10 minutes
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Switching your role locks you into your new role for 10 minutes before you can toggle the buttons again"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SafetyGreen"], "ROLE  Dominant");
            ImGui.BulletText("Others cannot apply gags or padlocks to you.");
            ImGui.BulletText("Can "); ImGui.SameLine(); ImGui.TextColored(colorDict["KeywordBlue"], "send and recieve"); ImGui.SameLine();
            ImGui.Text("relation requests and info requests.");
            ImGui.BulletText("Gags and Padlocks can still be applied by yourself.");
            // colored text in bright red saying Submissive:
            ImGui.TextColored(colorDict["SafetyGreen"], "ROLE  Submissive");
            ImGui.BulletText("Others can apply gags and padlocks to you.");
            ImGui.BulletText("Can "); ImGui.SameLine(); ImGui.TextColored(colorDict["KeywordBlue"], "send and recieve"); ImGui.SameLine();
            ImGui.Text("relation requests and info requests.");
            ImGui.BulletText("Can be enslaved to a mistress.");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // now for the gag and padlock listings
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Gag and Padlock Listings");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Define what gags (and locks) you have on."); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // push the style for the faded color text
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]);
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("The 3 listings reflect how you can wear up to three gags at a time."); ImGui.Spacing();
            // describe the layers
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("The 3 layers are: "); ImGui.SameLine(); ImGui.TextColored(colorDict["SafetyGreen"], "Under Layer, Surface Layer, TopMost Layer"); ImGui.Spacing();
            // describe how the muffling strength is added together
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Muffling strength of the gags are added up if multiple are used."); ImGui.Spacing();
            // describe how to use the dropdowns
            ImGui.Spacing();ImGui.Spacing();
            ImGui.TextWrapped("Once a gag is selected from the dropdown, it will immediately be applied to you."); 
            ImGui.TextWrapped("After that gag is applied, you will be able to lock it using the second dropdown."); 
            ImGui.TextWrapped("The selected padlock type only applies once you hit the"); ImGui.SameLine(); ImGui.TextColored(colorDict["VibrantRed"], "Lock button");
            ImGui.Separator();
            ImGui.PopStyleColor(); // pop faded text style color
        }
        ImGui.EndChild(); // debugTabScrollRegion
    }
    // listing for the whitelist tab
    private void DrawWhitelistTab() {
        ImGui.BeginChild("##whitelistTabInfo");
        {
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"The Whitelist");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Displays the list of people in your GagSpeak whitelist.");
            ImGui.PopStyleColor(); // pop faded text style color
            
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color 
            // bullet points to define.
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("All buttons on this page are sent to the selected user in this list.");
            // tell them how it locks you into your role for 10 minutes
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Can Add/Remove players without without interactions being enabled.");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // define the player manager
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"The Player Manager");            
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Allows you to view more information on selected players.");
            ImGui.PopStyleColor(); // pop faded text style color

            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            // bullet points to define.

            // create a table with 10px padding on the inside of the cells
            using (var child2 = ImRaii.Table("WhitelistManagerTable", 2, ImGuiTableFlags.RowBg)) {
                // define the widths of each column
                ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Your Relation to themmm").X);
                ImGui.TableSetupColumn("information", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X-10);
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Interactions"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("A safeguard so you dont hit any buttons by accident."); ImGui.Spacing();
                ImGui.TableNextRow();ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Your Relation to them"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Displays the currently defined relationship you have towards the selected player in the whitelist."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Their Relation to you"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Displays the currently defined relationship this selected player on the whitelist has towards you ."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Commitment Length"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Displays the length of time player has had a relation with you."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Become Their Mistress"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Will send a message to the player, prompting a request. This request gives the player the option"+
                "to give concent on you becoming their mistress."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Become Their Pet"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Will send a message to the player, prompting a request. This request gives the player the option"+
                "to give concent on you becoming their pet."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Become Their Slave"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Will send a message to the player, prompting a request. This request gives the player the option"+
                "to give concent on you becoming their slave."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Show Profile"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Will open a new window displaying additional information about the selected player, including"+
                "their relationship to you, their lean, their homeworld, their garble level, and information about their gagtypes,"+
                "padlocks, time remaining if any, and assigners if any."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Remove Relation"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Severs the relation status on both ends with this player."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], "Add/Remove Player"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Adds player if they are within PHYSICAL DISTANCE of you. You can remove people from whitelist anywhere."); ImGui.Spacing();
            }
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // define the player interactions
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Player Interactions");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Allows you to interact with the selected player. There is no way to detect if any of these are sucessful."+
            "So request an info update if unsure.");
            ImGui.PopStyleColor(); // pop faded text style color

            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            using (var child2 = ImRaii.Table("WhitelistManagerTable", 2, ImGuiTableFlags.RowBg)) {
                // define the widths of each column
                ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Request Player Infos").X);
                ImGui.TableSetupColumn("information", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Apply Gag"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Sends a /gag apply message to the selected whitelist player based on dropdown selections."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Lock Gag"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Sends a /gag lock message to the selected whitelist player based on dropdown selections."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Unlock Gag"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Sends a /gag unlock message to the selected whitelist player based on dropdown selections."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Remove This Gag"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Sends a /gag remove message to the selected whitelist player based on dropdown selections."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Remove All Gags"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Sends a /gag removeall message to the selected whitelist player."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Toggle Live Garbler"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Can only be used if the person you are using it on is your slave, and you are their mistress." +
                "This will lock your slaves automatic live chat garbler on, leaving them saying muffled words without any choice to turn it off~."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SectionBullet"], " Request Player Info"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Asks the selected whitelisted player to provide you with updated information."); ImGui.Spacing();
            }
            ImGui.PopStyleColor(); // pop faded text style color
        }
        ImGui.EndChild();
    }
    // listing for the settings tab
    private void DrawSettingsTab() {
        // this table will be formatted much like the one in generaltabinfo, however this will go over the following title topics of gag configuration, directchargarblermode option, debug display, and the enabled channels       
        ImGui.BeginChild("##settingsTabInfo");
        {
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Gag Configuration");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Sets up the permissions for where you send your translated speech, and what channels it has permission to go to.");
            ImGui.PopStyleColor(); // pop faded text style color

            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color 
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Configures who is able to apply gags and locks onto you."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Defined by"); ImGui.SameLine(); ImGui.TextColored(colorDict["SafetyGreen"], "Only Friends"); ImGui.SameLine();
            ImGui.TextWrapped(","); ImGui.SameLine(); ImGui.TextColored(colorDict["SafetyGreen"], "Only Party Memebers"); ImGui.SameLine();
            ImGui.TextWrapped(", and"); ImGui.SameLine(); ImGui.TextColored(colorDict["SafetyGreen"], "Only Whitelist"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Logic for parameters are defined in an OR basis."); ImGui.Spacing();
            ImGui.TextColored(colorDict["CautionOrange"], "Example  Only Friends and Only Party Members:");
            ImGui.BulletText("People who are in your friend list can apply gags and locks to you.");
            ImGui.BulletText("People who are in your party can apply gags and locks to you.");
            ImGui.BulletText("People in your party who are not your friend can apply gags and locks to you.");
            ImGui.BulletText("People in your friend list but not in your party can apply gags and locks to you.");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color


            // define the directchargarblermode option
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Direct Chargarbler Mode");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped("Allows you to toggle the direct chargarbler mode on and off.");
            ImGui.PopStyleColor(); // pop faded text style color

            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("When enabled, any message you send to chat that is not a command is automatically translated."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If disabled, translations only can occur by typing your message after"); ImGui.SameLine(); ImGui.TextColored(colorDict["SafetyGreen"], "/gsm"); ImGui.SameLine();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // define the enabled channels
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Enabled Channels");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped("Allows you to toggle which channels you want to allow translated speech to occur in.");
            ImGui.PopStyleColor(); // pop faded text style color

            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Enabled Channels will sucessfully parse /gsm commands and Direct Chat Garbler Mode messages to them."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Disabled Channels will not parse /gsm commands and Direct Chat Garbler Mode messages to them."); ImGui.Spacing();
            ImGui.TextColored(colorDict["CautionOrange"], "Example Case  /say disabled & DirectChatGarblerMode enabled:");
            ImGui.BulletText("Any Messages sent to /say will not be translated.");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color            
        }
        ImGui.EndChild();
    }
    // listing for the commands tab
    private void DrawCommandsTab() {
        // this section is going to be formatted in mix of how the whitelist and general tab are formatted. I will begin by explaining how the commands work, mentioning how to bring up the help displays in client chat. Following that, I'll create a table that outlines the keywords used in the commands, and the spesifics of each. After that table, I will outline each general command, what it does, and give a list of all possible sub command formats and provide an example for each
        ImGui.BeginChild("##commandsTabInfo");
        {
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Commands Ovewview");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Made primarily made as an alternative way to automate the process of using them on people you may want to,"+
            " or for enhancing immersion via puppet master plugin in combination with other commands like /glamourer gear swaps.");
            ImGui.PopStyleColor(); // pop faded text style color

            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color 
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A more indepth guide to the usgage of the commands."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Made primarily made as an alternative way to using buttons "); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Made in mind with the idea of them being used with puppet master plugin or macros for kinky combinations."); ImGui.Spacing();
            ImGui.Separator();
            ImGui.PopStyleColor(); // pop faded text style color
            
            //  setup a table two columns wide, exactly like the whitelist one, it should be titled "Command Keywords"
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Primary Command Keywords & Their Purposes");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            using (var child2 = ImRaii.Table("CommandKeywordsTable", 2, ImGuiTableFlags.RowBg)) {
                // define the widths of each column
                ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Player Name").X);
                ImGui.TableSetupColumn("information", ImGuiTableColumnFlags.WidthStretch);
                // define the rows
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["VibrantRed"], "Layer"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Identifes the gag/padlock layer you're using the command on. (1, 2, or 3)"); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SafetyGreen"], "Gagtype"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Defines which type of gag is being applied."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["SafetyGreen"], "Padlock"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Defines which padlock type is locking a gag."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["Purple"], "Password"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("A password to define for certain padlocks."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["Purple"], "Password2"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("A secondary password only used in the TimerPasswordPadlock prompt."); ImGui.Spacing();
                ImGui.TableNextRow(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextColored(colorDict["KeywordBlue"], "Player Name"); ImGui.Spacing(); ImGui.TableNextColumn(); ImGui.Spacing();
                ImGui.TextWrapped("Player to use command on. Format: [ FirstName LastName@World ]"); ImGui.Spacing();
            }
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // describe the formats of the gag apply command
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Command: /gag");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Applies a gag to the defined player.");
            ImGui.PopStyleColor(); // pop faded text style color
            // display formats
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Use by itself to see the chat box help menu for /gag"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Format: /gag"); ImGui.SameLine();            
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine(); 
            ImGui.TextColored(colorDict["SafetyGreen"], "gagtype"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            // provide examples
            ImGui.TextColored(colorDict["CautionOrange"], "Command Example Uses:");
            ImGui.BulletText("/gag 1 Ball Gag | FirstName LastName@Exodus");
            ImGui.BulletText("/gag 2 Ring Gag | FirstName LastName@Exodus");
            ImGui.BulletText("/gag 3 Bit Gag | FirstName LastName@Exodus");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // describe the formats of the gag lock command
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Command: /gag lock");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Locks a gag on the defined player's gag. Formats are:");
            ImGui.PopStyleColor(); // pop faded text style color
            // display formats
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag lock"); ImGui.SameLine();
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine();
            ImGui.TextColored(colorDict["SafetyGreen"], "locktype"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag lock"); ImGui.SameLine();
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine();
            ImGui.TextColored(colorDict["SafetyGreen"], "locktype"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["Purple"], "password"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag lock"); ImGui.SameLine();
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine();
            ImGui.TextColored(colorDict["SafetyGreen"], "locktype"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["Purple"], "password"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["Purple"], "password2"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            // provide examples
            ImGui.TextColored(colorDict["CautionOrange"], "Command Example Uses:");
            ImGui.BulletText("/gag lock 1 MetalPadlock | Aella Kha@Mateus");
            ImGui.BulletText("/gag lock 2 CombinationPadlock | 2968 | Akiko Misaki@Exodus");
            ImGui.BulletText("/gag lock 1 PasswordPadlock | totalcontrol | Kaira Brulee@Balmung");
            ImGui.BulletText("/gag lock 3 TimerPasswordPadlock | gagslut | 40m20s | Zyra Chi@Exodus");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // describe the formats of the gag unlock command
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Command: /gag unlock");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Unlocks the padlock on the defined player's gag. Formats are:");
            ImGui.PopStyleColor(); // pop faded text style color
            // display formats
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag unlock"); ImGui.SameLine();
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag unlock"); ImGui.SameLine();
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["Purple"], "password"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            // provide examples
            ImGui.TextColored(colorDict["CautionOrange"], "Command Example Uses:");
            ImGui.BulletText("/gag unlock 1 | Jona Rosier@Maduin");
            ImGui.BulletText("/gag unlock 3 | 2968 | Akiko Viarl@Brynhildr");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // describe the formats of the gag remove command
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Command: /gag remove");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Removes the gag from the defined player.");
            ImGui.PopStyleColor(); // pop faded text style color
            // display formats
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag remove"); ImGui.SameLine();
            ImGui.TextColored(colorDict["VibrantRed"], "layer"); ImGui.SameLine();
            ImGui.TextColored(colorDict["FullWhite"], "|"); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            // provide examples
            ImGui.TextColored(colorDict["CautionOrange"], "Command Example Uses:");
            ImGui.BulletText("/gag remove 2 | Lilith Nasama@Zalera");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // describe the formats of the gag removeall command
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Command: /gag removeall");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped(" Removes all gags from the defined player.");
            ImGui.PopStyleColor(); // pop faded text style color
            // display formats
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("/gag removeall | "); ImGui.SameLine();
            ImGui.TextColored(colorDict["KeywordBlue"], "Player Name@Homeworld"); ImGui.Spacing();
            // provide examples
            ImGui.TextColored(colorDict["CautionOrange"], "Command Example Uses:");
            ImGui.BulletText("/gag removeall | Lilith Rosier@Malboro");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color
        }
        ImGui.EndChild();
    }
    // listing for the padlocks tab

    private void DrawPadlocksTab() {
        ImGui.BeginChild("##padlocksTabInfo");
        {
            // just kinda bullet list each gag type and what it does
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color


            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Metal Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("The basic padlock selection."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Requires no combination, password, or timer inputs to lock."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Can be unlocked by pressing unlock button"); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Combination Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A numeric combination padlock."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Combination must be exactly 4 digits in length, and supplied as the locks password"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("To unlock, player must enter the correct combination into the password field & press unlock."); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Password Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A alpha non-numeric password based padlock."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Password must be between 1 and 20 characters in length, and supplied as the locks password"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("To unlock, player must enter the correct password into the password field & press unlock."); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Five Minutes Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A padlock that can be unlocked after 5 minutes of being locked."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("as it will not be able to be unlocked until the timer is up.)"); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Timer Password Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A alpha non-numeric password based padlock with a timer."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Password must be between 1 and 20 characters in length, and supplied as the locks password"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Timer must be in the format: XXdXXhXXmXXs; for days, hours, minutes, seconds. With X == A Number. Can exclude or include any inbetween. (EX: 1h5s works)"); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Mistress Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A padlock that can be applied only by people defined as a mistress relation in your whitelist."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("This padlock can also only be unlocked by the mistress who assigned it."); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();

            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),"Mistress Timer Padlock");
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("A padlock that can be applied only by people defined as a mistress relation in your whitelist."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("This padlock can also only be unlocked by the mistress who assigned it."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("This padlock also has a timer, which can be set by the mistress who assigned it."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Timer must be in the format: XXdXXhXXmXXs; for days, hours, minutes, seconds. With X == A Number. Can exclude or include any inbetween. (EX: 1h5s works)"); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color
        }
        ImGui.EndChild();
    }

    // listing for the precautions tab
    private void DrawTransparencyTab() {
        ImGui.BeginChild("##precautionsTabInfo");
        {
            // this tab will be primarily used to warn the players about the dangers of using this plugin, and reassure them of all the safety checks put into place
            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),"Precautions To Be Aware Of & The Safety Checks In Place to Ensure your Safety");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["KeywordBlue"]); // push faded text style color
            ImGui.TextWrapped("This plugin, if you haven't already guessed, is not your regular translator plugin."+
            "\nThis plugin communicates serverside, not clientside. As such naturally people will worry about risks with getting"+
            " detected. Rest assured however, for I have put in numerous checks to ensure you have garenteed safety, and"+
            " wish to be fully transparent about it all.");
            ImGui.PopStyleColor(); // pop faded text style color

            // define the concerns
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "  "); ImGui.SameLine();
            ImGui.TextWrapped("Is it safe that actions with other people are serverside and not clientside?"); ImGui.Spacing();
            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "  "); ImGui.SameLine();
            ImGui.TextWrapped("How safe are the extracted messages from /gsm that are sent to the server?"); ImGui.Spacing();
            ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "  "); ImGui.SameLine();
            ImGui.TextWrapped("Is the DirectChatGarblerMode option safe to use or should I be worried?"); ImGui.Spacing();
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // address first question
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Is it safe that most actions with others are serverside?");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped("I put weeks into installing checks and throwing an error if any step goes wrong, so I would say you are in safe hands here.");
            ImGui.PopStyleColor(); // pop faded text style color
            // respond to concern            
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Yes, the messages are sent via tells, and every interaction that transmits info from one"+
            " player to another is formatted into a convencing roleplay tell message, making it look natural and playermade. Because it"+
            " is disguised this way, it could be easily brushed off as a misstell or accidental premature send of a message."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("In Short, be patient for my updates after every patch and you will be fine♥");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // address second question
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), "How safe is the sending of /gsm messages to the server?");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped("I garentee it. In fact, here's all the checks I've implemented to ensure your safety:");
            ImGui.PopStyleColor(); // pop faded text style color
            // respond to concern
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Once the message after /gsm is stored and requested to be sent to the server, I ensure that it does not"+
            " contain any special characters the server cant reconize. If it does, they are removed from the message."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Once the messages are sanatized and passed into the code that sends it off to the server, i first make"+
            " sure that it is not empty, or that it is not longer than the maximum allowed exepected message length. If it is, i throw"+
            " an error and will not send the message."); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("After the signature is verified, I then take the message and store it make sure the signature"+
            " still matches the sendChatToServer signature"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("Once the message is verified as a valid message to send, i then inject it into a chatmessagepayload,"+
            "which throws an error if it fails at any point during construction."); ImGui.Spacing();
            ImGui.TextColored(colorDict["VibrantRed"], "If ANY checks fail, the message isn't sent to server at all, keeping you safe.");            
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color

            // address third question
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f), "Is the DirectChatGarblerMode option safe to use?");
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["SectionTitleDesc"]); // push faded text style color
            ImGui.TextWrapped("I've implemented so many security checks and error handling that"+
            " I can confidently say it accounts for all edge cases and is safe now.\nHere are all the cases where it throws an exception:");
            ImGui.PopStyleColor(); // pop faded text style color
            // respond to concern
            ImGui.PushStyleColor(ImGuiCol.Text, colorDict["FadedText"]); // push faded text style color
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If it did not recieve a message to detour"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the message is empty"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the message is longer than the maximum allowed message length"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the message contains any special characters"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the detour signature is not correctly established"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the message has any error translating or is longer than maximum length after translation"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the message is empty after translation"); ImGui.Spacing();
            ImGui.TextColored(colorDict["SectionBullet"], "   "); ImGui.SameLine();
            ImGui.TextWrapped("If the translated message is unable to be placed into the original packet as the new modified message to be sent to the server"); ImGui.Spacing();
            ImGui.TextColored(colorDict["VibrantRed"], "If ANY checks fail, only the original message is sent to the server, keeping you safe.");
            ImGui.Spacing(); ImGui.Separator(); ImGui.Spacing();
            ImGui.PopStyleColor(); // pop faded text style color
        }
        ImGui.EndChild();
    }

    private WindowHeader.Button OpenPopupButton() {
        return new() {
            Description = "Toggle displaying help page as a seperate new window.",
            Icon = FontAwesomeIcon.Expand,
            OnClick = () => IsOpen = !IsOpen,
            Visible = true,
        };
    }
}