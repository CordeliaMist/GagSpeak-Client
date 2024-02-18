using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using OtterGui;
using GagSpeak.CharacterData;
using System.Collections.Generic;

namespace GagSpeak.UI.Tabs.HelpPageTab;
/// <summary> This class is used to handle the Toybox Tab. </summary>
public class HelpPageTab : ITab
{
    private readonly    CharacterHandler                _characterHandler;      // for getting the character handler
    private readonly    TutorialWindow                  _tutorialWindow;        // for getting the tutorial window
    private             bool                            _leftTabSelected;       // for getting the left tab selected
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

    public HelpPageTab(CharacterHandler characterHandler, TutorialWindow tutorialWindow) {
        _characterHandler = characterHandler;
        _tutorialWindow = tutorialWindow;
    }

    public ReadOnlySpan<byte> Label => "Help"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the Toybox Tab </summary>
    public void DrawContent() {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        DrawShelfSelection();
        if(_leftTabSelected) {
            DrawTransparencyTab();
        }
        else {
            DrawPadlocksTab();
        }
    }

    /// <summary> Draws out the subtabs for the toybox tab </summary>
    private void DrawShelfSelection() {
        // make our buttons look like selection tabs
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonSize = new Vector2(ImGui.GetContentRegionAvail().X / 3, ImGui.GetFrameHeight());
        // draw out the buttons subtabs of our toybox
        if (ImGuiUtil.DrawDisabledButton("Padlocks", buttonSize, "Shows your toybox's connection status, and settings for each whitelisted player", !_leftTabSelected))
            _leftTabSelected = false;
        ImGui.SameLine();
        if(ImGuiUtil.DrawDisabledButton("Open Visual Tutorial", buttonSize, "A wonderful visual tutorial, your welcome♥", false)) {
            _tutorialWindow.Toggle();
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton("Transparency", buttonSize, "A savable pattern creator, for all your fun needs~", _leftTabSelected))
            _leftTabSelected = true;
        style.Pop();
    }

    //draw the children tabs
#region Padlocks Tab
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
#endregion Padlocks Tab
#region Precautions Tab
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
#endregion Precautions Tab
}