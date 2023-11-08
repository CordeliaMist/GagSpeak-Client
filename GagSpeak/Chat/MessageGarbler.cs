using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;
using OtterGui.Classes;
using OtterGui.Log;
using GagSpeak.Chat;
using Lumina;
using GagSpeak.Services;

// practicing modular design
namespace GagSpeak.Chat.Garbler;

// The component that will handle the translation of the chat message
public class MessageGarbler
{
    // The method that will translate our messages
    public string GarbleMessage(string beginString, int garbleLevel ) {
        // First we need to get the garble level from the config
        int level = garbleLevel;
        // Then we need to set the end string to null
        string endString = "";

        if (level == 0) { return beginString; } // incase we dont have any gags on
        
        // Then we need to set the begin string to lowercase
        beginString = beginString.ToLower();
        // Then we need to loop through the begin string and start garbling it until it's done!
        for (var ind = 0; ind < beginString.Length; ind++) {
            // General conditions is that if the garble level is above a certain threshold,
            // After this, all other lower conditions would also apply (i think?)
            char currentChar = beginString[ind];
            if (level >= 20)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkvbywgpfucdlhr".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (level >= 16)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkvbywgpf".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (level >= 12)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkv".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (level >= 8)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else { endString += "m"; }
            }
            else if (level >= 7)
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
            else if (level >= 6)
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
            else if (level >= 5)
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
            else if (level >= 4)
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
            else if (level >= 3)
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
            else if (level >= 2)
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
            else if (level >= 1)
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
        }
        return endString;
    }
}