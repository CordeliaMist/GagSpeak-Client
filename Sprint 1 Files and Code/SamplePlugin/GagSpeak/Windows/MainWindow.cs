using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Internal.Windows.Settings.Widgets;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using Microsoft.VisualBasic;

namespace Gagger.Windows;


public class MainWindow : Window, IDisposable
{
    private readonly TextureWrap settingsImage;
    private Plugin Plugin;

    private string textIn = "Enter your text here.";
    private string textOut ="Garbled text will appear here.";
    public MainWindow(Plugin plugin, TextureWrap Image) : base(
        "Text Garbler", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse|ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.settingsImage = Image;
        this.Plugin = plugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        /* if (ImGui.BeginTable("table1", 2))
        {
            ImGui.TableSetColumnIndex(1);
            if (ImGui.ImageButton(this.settingsImage.ImGuiHandle, new Vector2(this.settingsImage.Width, this.settingsImage.Height)))
            {
                this.Plugin.DrawConfigUI();
            }
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.InputTextMultiline("input",ref textIn,500, new Vector2(400.0f,250.0f),ImGuiInputTextFlags.CtrlEnterForNewLine);
            ImGui.TableSetColumnIndex(1);
            if(ImGui.Button("GAG ME!"))
            {
                textOut = garbleFunction(textIn);
            }
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.InputTextMultiline("input",ref textOut,500, new Vector2(400.0f,250.0f),ImGuiInputTextFlags.ReadOnly);
            ImGui.EndTable();
        } */

        if (ImGui.ImageButton(this.settingsImage.ImGuiHandle, new Vector2(this.settingsImage.Width, this.settingsImage.Height)))
            {
                this.Plugin.DrawConfigUI();
            }
        if(ImGui.InputTextMultiline("input",ref textIn,500, new Vector2(400.0f,250.0f),ImGuiInputTextFlags.CtrlEnterForNewLine));
        if(ImGui.Button("GAG ME!"))
        {
            textOut = garbleFunction(textIn);
        }
        ImGui.InputTextMultiline("output",ref textOut,500, new Vector2(400.0f,250.0f));

    }

    private static string garbleFunction(string input)
        {
        string beginString = input;
        string endString = string.Empty;
        beginString = beginString.ToLower();
        for (int ind = 0; ind < beginString.Length; ind++)
        {
            char currentChar = beginString[ind];
            if (Configuration.strength >= 20)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkvbywgpfucdlhr".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 16)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkvbywgpf".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 12)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkv".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 8)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 7)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if (currentChar == 'b') { endString += currentChar; }
                else if ("aeiouy".Contains(currentChar)) { endString += "e"; }
                else if ("jklr".Contains(currentChar)) { endString += "a"; }
                else if ("szh".Contains(currentChar)) { endString += "h"; }
                else if ("dfgnmwtcqxpv".Contains(currentChar)) { endString += "m"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 6)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("aeiouyt".Contains(currentChar)) { endString += "e"; }
                else if ("jklrw".Contains(currentChar)) { endString += "a"; }
                else if ("szh".Contains(currentChar)) { endString += "h"; }
                else if ("dfgnm".Contains(currentChar)) { endString += "m"; }
                else if ("cqx".Contains(currentChar)) { endString += "k"; }
                else if ("bpv".Contains(currentChar)) { endString += "f"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 5)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("eiouyt".Contains(currentChar)) { endString += "e"; }
                else if ("jlrwa".Contains(currentChar)) { endString += "a"; }
                else if ("szh".Contains(currentChar)) { endString += "h"; }
                else if ("dfgm".Contains(currentChar)) { endString += "m"; }
                else if ("cqxk".Contains(currentChar)) { endString += "k"; }
                else if ("bpv".Contains(currentChar)) { endString += "f"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 4)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("vbct".Contains(currentChar)) { endString += "e"; }
                else if ("wyjlr".Contains(currentChar)) { endString += "a"; }
                else if ("sz".Contains(currentChar)) { endString += "h"; }
                else if ("df".Contains(currentChar)) { endString += "m"; }
                else if ("qkx".Contains(currentChar)) { endString += "k"; }
                else if (currentChar == 'p') { endString += "f"; }
                else if (currentChar == 'g') { endString += "n"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 3)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("vbct".Contains(currentChar)) { endString += "e"; }
                else if ("wyjlr".Contains(currentChar)) { endString += "a"; }
                else if ("sz".Contains(currentChar)) { endString += "s"; }
                else if ("qkx".Contains(currentChar)) { endString += "k"; }
                else if (currentChar == 'd') { endString += "m"; }
                else if (currentChar == 'p') { endString += "f"; }
                else if (currentChar == 'g') { endString += "h"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 2)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("ct".Contains(currentChar)) { endString += "e"; }
                else if ("jlr".Contains(currentChar)) { endString += "a"; }
                else if ("qkx".Contains(currentChar)) { endString += "k"; }
                else if ("dmg".Contains(currentChar)) { endString += "m"; }
                else if (currentChar == 's') { endString += "z"; }
                else if (currentChar == 'z') { endString += "s"; }
                else if (currentChar == 'f') { endString += "h"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 1)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("jlr".Contains(currentChar)) { endString += "a"; }
                else if ("cqkx".Contains(currentChar)) { endString += "k"; }
                else if ("dmg".Contains(currentChar)) { endString += "m"; }
                else if (currentChar == 't') { endString += "e"; }
                else if (currentChar == 'z') { endString += "s"; }
                else if (currentChar == 'f') { endString += "h"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength == 0)
            {
                endString += currentChar;
            }
        }
        return endString;
    }
}