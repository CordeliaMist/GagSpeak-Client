using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Utility;
using OtterGui.Classes;

namespace GagSpeak.UI.Tabs.WhitelistTab;

public static class InfoSendAndRequestHelpers {
#region RequestInfoHelpers
    public static void RequestInfoFromPlayer(int listIdx, CharacterHandler characterHandler, ChatMessages.ChatManager chatManager,
    ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui)
    {    
        GagSpeak.Log.Debug($"[WhitelistTab]: Requesting information from player: {characterHandler.whitelistChars[listIdx]._name}");
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        if (!characterHandler.IsIndexWithinBounds(listIdx)) { 
            GagSpeak.Log.Error($"[WhitelistTab]: Error, Index out of bounds for requesting information from player: {characterHandler.whitelistChars[listIdx]._name}");
            return;
        }
        // print to chat that you sent the request

        chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Sending information request to " +
            $"{characterHandler.whitelistChars[listIdx]._name}, please wait...").AddItalicsOff().BuiltString);
        // send the message
        string targetPlayer = characterHandler.whitelistChars[listIdx]._name + "@" + characterHandler.whitelistChars[listIdx]._homeworld;
        chatManager.SendRealMessage(gagMessages.EncodeRequestInfoMessage(playerPayload, targetPlayer));
    }

	/// <summary>  Controls logic for sending the first chunk of your info the player that requested it from you. 
    /// <para> It makes sure it is something that is allowed based on your current information about the whitelisted player, then if allowable,
    /// sends them the encoded message automatically, serving as a shortcut to needing to type out the commands. </para> </summary>
    public static void SendInfoToPlayer(CharacterHandler characterHandler, ChatMessages.ChatManager chatManager, 
    ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui, GagSpeakConfig config, string senderName)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        try
        {
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{senderName}] "+
                "with your details(1/3)").AddItalicsOff().BuiltString);
            //send the message
            chatManager.SendRealMessage(gagMessages.HandleProvideInfoPartOne(playerPayload, senderName, characterHandler));
        }
        catch (Exception e)
        {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Error: {e}").AddItalicsOff().BuiltString);
        }
    }

    /// <summary>  Controls logic for sending the second chunk of your info the player that requested it from you. </summary>
    public static void SendInfoToPlayer2(CharacterHandler characterHandler, ChatMessages.ChatManager chatManager,
    ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui, GagSpeakConfig config, string senderName)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        try
        {
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{senderName}] "+
                "with your details(2/3)").AddItalicsOff().BuiltString);
            //send the message
            chatManager.SendRealMessage(gagMessages.HandleProvideInfoPartTwo(playerPayload, senderName, characterHandler));
        }
        catch (Exception e)
        {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Error: {e}").AddItalicsOff().BuiltString);
        }
    }

    /// <summary>  Controls logic for sending the third chunk of your info the player that requested it from you. </summary>
    public static void SendInfoToPlayer3(CharacterHandler characterHandler, ChatMessages.ChatManager chatManager,
    ChatMessages.MessageTransfer.MessageEncoder gagMessages, IClientState clientState, IChatGui chatGui, GagSpeakConfig config, string senderName)
    {
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(clientState, out playerPayload);
        try
        {
            // print to chat that you sent the request
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating whitelisted player [{senderName}] "+
                "with your details(3/3)").AddItalicsOff().BuiltString);
            //send the message
            chatManager.SendRealMessage(gagMessages.HandleProvideInfoPartThree(playerPayload, senderName, characterHandler));
        }
        catch (Exception e)
        {
            chatGui.Print(
                new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddRed($"Error: {e}").AddItalicsOff().BuiltString);
        }
    }
}
#endregion RequestInfoHelpers