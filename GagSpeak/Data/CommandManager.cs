using System;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using GagSpeak.UI;
using OtterGui.Classes;
using GagSpeak.Chat;
using GagSpeak.Chat.MsgEncoder;
using GagSpeak.Chat.MsgDecoder;
using GagSpeak.Chat.MsgResultLogic;
using GagSpeak.Chat.MsgDictionary;
using GagSpeak.Data;
using GagSpeak.Chat.Garbler;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XivCommon.Functions;
using ChatChannel = GagSpeak.Data.ChatChannel;
// practicing modular design
namespace GagSpeak.Services;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class CommandManager : IDisposable // Our main command list manager
{
    private const string MainCommandString = "/gagspeak"; // The primary command used for & displays
    private const string ActionsCommandString = "/gag"; // subcommand for more in-depth actions.
    private const string TranslateCommandString = "/gsm"; // convient subcommand for translating messages

    // Include our other classes
    private readonly MessageEncoder _gagMessages;
    private readonly ICommandManager _commands;
    private readonly MainWindow _mainWindow;
    private readonly HistoryWindow _historyWindow;
    private readonly HistoryService _historyService;
    private readonly IChatGui _chat;
    private readonly GagSpeakConfig _config;
    private readonly ChatManager _chatManager;
    private readonly IClientState _clientState;
    private RealChatInteraction _realChatInteraction;
    private readonly IFramework _framework; // framework from XIVClientStructs

    private readonly MessageGarbler _messageGarbler;
    // Constructor for the command manager
    public CommandManager(ICommandManager command, MainWindow mainwindow, HistoryWindow historywindow, HistoryService historyService,
        IChatGui chat, GagSpeakConfig config, ChatManager chatManager, IClientState clientState, IFramework framework, 
        RealChatInteraction realchatinteraction)
    {
        // set the private readonly's to the passed in data of the respective names
        _commands = command;
        _mainWindow = mainwindow;
        _historyWindow = historywindow;
        _chat = chat;
        _realChatInteraction = realchatinteraction;
        _config = config;
        _chatManager = chatManager;
        _clientState = clientState;
        _framework = framework;
        _gagMessages = new MessageEncoder();
        _messageGarbler = new MessageGarbler();
        _historyService = historyService;

        // Add handlers to the main commands
        _commands.AddHandler(MainCommandString, new CommandInfo(OnGagSpeak) {
            HelpMessage = "Toggles main UI when used without arguements. Use with 'help' or '?' to view sub-commands.",
            ShowInHelp = true
        });
        _commands.AddHandler(ActionsCommandString, new CommandInfo(OnGag) {
            HelpMessage = "Displays the list of GagSpeak commands. Use with 'help' or '?' for extended help.",
            ShowInHelp = true
        });

        _commands.AddHandler(TranslateCommandString, new CommandInfo(OnGSM) {
            HelpMessage = "Translates everything after /gsm into GagSpeak into currently selected chat type in the chat box.",
            ShowInHelp = true
        });
        // let user know on launch of their direct chat garbler is still enabled
        if (_config.DirectChatGarbler) {
            _chat.PrintError("Direct Chat Garbler is still enabled. If you don't want this on, remember to disable it!");
        }
    }

    // Dispose of the command manager
    public void Dispose()
    {
        // Remove the handlers from the main commands
        _commands.RemoveHandler(MainCommandString);
        _commands.RemoveHandler(ActionsCommandString);
        _commands.RemoveHandler(TranslateCommandString);
    }

    // Handler for the main gagspeak command
    private void OnGagSpeak(string command, string arguments)
    {
        var argumentList = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (argumentList.Length < 1) { _mainWindow.Toggle(); return; }

        var argument = argumentList.Length == 2 ? argumentList[1] : string.Empty; // Make arguement be everything after command
        switch(argumentList[0].ToLowerInvariant()) {
            case "safeword":
                Safeword(argument);        // when [/gagspeak safeword] is typed
                return;
            case "showlist":
                Showlist(argument);        // when [/gagspeak showlist] is typed
                return;
            case "setmode":
                Setmode(argument);         // when [/gagspeak setmode] is typed
                return;
            case "history":
                _historyWindow.Toggle();   // when [/gagspeak history] is typed
                return;
            case "":
                _mainWindow.Toggle(); // when [/gagspeak] is typed
                return;
            default:
                PrintHelpGagSpeak("help");// when no arguements are passed.
                return;
        };
    }

    private bool Safeword(string argument) { // Handler for the safeword subcommand
        if (string.IsNullOrWhiteSpace(argument)) { // If no safeword is provided
            _chat.Print("Please provide a safeword. Usage: /gagspeak safeword [your_safeword]"); return false; }
        if (_config.Safeword == argument) { // If the safeword is the same as the one we are trying to set
            _chat.Print("Safeword matched, deactivating all gags and locks"); 
            for (int layerIndex = 0; layerIndex < _config.selectedGagTypes.Count; layerIndex++) {
                _config.selectedGagTypes[layerIndex] = "None";
                _config.selectedGagPadlocks[layerIndex] = GagPadlocks.None;
                _config.selectedGagPadlocksPassword[layerIndex] = "";
                _config.selectedGagPadlocksAssigner[layerIndex] = "";
            }
        }

        return true;
    }

    private bool Showlist(string argument) { // Handler for the showlist subcommand
        if (string.IsNullOrWhiteSpace(argument)) { // If no argument is provided, tell them to spesify
            _chat.Print("Please specify what you want to see. Usage: /gagspeak showlist [padlocks/gags]"); return false; }
        var subCommand = argument.ToLower(); // set what we typed to lowercases to match checks
        if (subCommand == "padlocks") { // if we typed padlocks, show the padlocks list
            _chat.Print(new SeStringBuilder().AddYellow("Displaying the list of padlocks...").BuiltString);
            var padlockTypes = Enum.GetNames(typeof(GagPadlocks));
            foreach (var padlock in padlockTypes) { _chat.Print(new SeStringBuilder().AddBlue($"    》{padlock}").BuiltString); }
            return true;
        } else if (subCommand == "gags") {
            _chat.Print("Displaying gaglist will come soon! Need to make sure it is safe to print that much text first!");
            return true;
        } else {
            _chat.Print("Invalid argument. Usage: /gagspeak showlist [padlocks/gags]"); return false;
        }
    }

    private bool Setmode(string argument) { // Handler for the setmode subcommand
        if (string.IsNullOrWhiteSpace(argument)) { // If no argument is provided, tell them to spesify
            _chat.Print("Please specify the mode. Usage: /gagspeak setmode [dom/sub]"); return false; }
        var mode = argument.ToLower(); // set what we typed to lowercases to match checks
        if (mode == "dom") {
            _chat.Print(new SeStringBuilder().AddText("Your mode has been set to ").AddRed("Dom.").BuiltString); _config.InDomMode = true; _config.Save(); return true;
        } else if (mode == "sub") {
            _chat.Print(new SeStringBuilder().AddText("Your mode has been set to ").AddRed("Sub.").BuiltString); _config.InDomMode = false; _config.Save(); return true;
        } else {
            _chat.Print("Invalid mode. Usage: /gagspeak setmode [dom/sub]"); return false;
        }
    }

    // On the gag command
    private void OnGag(string command, string arguments) {
        var argumentList = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (argumentList.Length < 1) {
            PrintHelpGag("?");
            return;
        }

        var argument = argumentList.Length == 2 ? argumentList[1] : string.Empty;
        var _ = argumentList[0].ToLowerInvariant() switch
        {
            "1"         => GagApply(argumentList), // map to GagApply function
            "2"         => GagApply(argumentList), // map to GagApply function
            "3"         => GagApply(argumentList), // map to GagApply function
            "lock"      => GagLock(argument),      // map to GagLock function 
            "unlock"    => GagUnlock(argument),    // map to GagUnlock function
            "remove"    => GagRemove(argument),    // map to GagRemove function
            "removeall" => GagRemoveAll(argument), // map to GagRemoveAll function
            _           => PrintHelpGag("?"),      // if we didn't type help or ?, print the error
        };
    }

    // Handles:  /gag [layer] [gagtype] | [player target] 
    private bool GagApply(string[] argumentList) {
        // we are initially passed in the argument list seperated by spaces, but this isnt what we want, so rejoin them into one string
        string combinedArguments = string.Join(" ", argumentList);
        // now we need to break it down into parts again, but this time we want to split it by the | character
        string[] parts = combinedArguments.Split(" | ");
        // now we need to take everything else before the first | and split it by spaces again
        string argumentsBeforePipe = parts[0].Trim();
        string[] argumentsBeforePipeList = argumentsBeforePipe.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string targetPlayer = parts[1].Trim(); // Get the target player name after the pipe
        string gagType = string.Join(" ", argumentsBeforePipeList.Skip(1)); // get the gagtype
        string layer = argumentsBeforePipeList[0]; // get the layer

        // if our arguments are not valid, display help information
        if (! (_config.GagTypes.ContainsKey(gagType) && (layer == "1" || layer == "2" || layer == "3") && targetPlayer.Contains("@")) )
        {   // One of our parameters WAS invalid, so display to them the help.
            _chat.Print(new SeStringBuilder().AddRed("Invalid Arguments").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag ").AddYellow("layer ").AddGreen("gagtype").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            // define explination of the arguments
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Layer field must be either 1, 2, or 3, indicating the slot the gag is equipped to.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Gagtype field must be a valid gagtype. Use /gag showlist gags to see all valid gagtypes.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Player field must be a valid player name and homeworld. Example:").AddYellow("Sample Player@Bahamut.").BuiltString);
            return false; // maybe dont include this?
        }
        // we have passed in the correct arguments, so begin applying the logic.
        PlayerPayload playerPayload; // get player payload
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"{playerPayload.PlayerName} is Gagging {targetPlayer} with {gagType} on layer {layer}."); // log the action
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            // unique string for /gag apply == "over your mouth as the"
            _chatManager.SendRealMessage(_gagMessages.GagEncodedApplyMessage(playerPayload, targetPlayer, gagType, layer));
        }
        catch (Exception e) {
            GagSpeak.Log.Error($"Error sending chat message to player: {e.Message}");
            _chat.PrintError($"Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    // Handles: /gag lock [layer] [locktype] | [player target] && /gag lock [layer] [locktype] | [password] | [player target]
    private bool GagLock(string argument) { // arguement at this point = layer locktype | player target
        // step 1, split by " | " to get the components into the parts we need.
        string[] parts = argument.Split(" | ");
        // if our parts == 2, then we have no password, if our parts == 3, then we have a password. Set our vars now so we dont set them in both statements
        string targetplayer = string.Empty;
        string locktype = string.Empty;
        string password = string.Empty;
        string layer = string.Empty;
        if (parts.Length == 2) { // Condition, no password.
            GagSpeak.Log.Debug($"parts.Length == 2, parts[0] = {parts[0]}, parts[1] = {parts[1]}");
            targetplayer = parts[1].Trim(); // Get the target player name
            // take parts[0], which is [layer] [locktype], and split it so that layer == first word of parts[0] and locktype == the rest of[0]
            string[] layerAndLocktype = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // get the layer
            layer = layerAndLocktype[0];
            // get the locktype
            locktype = string.Join(" ", layerAndLocktype.Skip(1));
        } 
        else if (parts.Length == 3) { // oh my, we have a password
            GagSpeak.Log.Debug($"parts.Length == 3, parts[0] = {parts[0]}, parts[1] = {parts[1]}, parts[2] = {parts[2]}");
            targetplayer = parts[2].Trim(); // Get the target player name
            password = parts[1].Trim(); // Get the password
            // take parts[0], which is [layer] [locktype], and split it so that layer == first word of parts[0] and locktype == the rest of[0]
            string[] layerAndLocktype = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // get the layer
            layer = layerAndLocktype[0];
            // get the locktype
            locktype = string.Join(" ", layerAndLocktype.Skip(1));
            // verify password.
            if (IsInvalidPassword(locktype, password)) { return false; } // returns false if it is not a valid password
        }

        // if our arguments are not valid, display help information
        if (! (Enum.IsDefined(typeof(GagPadlocks), locktype) && (layer == "1" || layer == "2" || layer == "3") && targetplayer.Contains("@")) )
        {   // One of our parameters WAS invalid, so display to them the help.
            _chat.Print(new SeStringBuilder().AddRed("Invalid Arguments. Layer or Locktype or player name is incorrect").BuiltString);
            // display correct information
            if(parts.Length == 2) {
                _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag lock ").AddYellow("layer ").AddGreen("locktype").AddText(" | ")
                .AddBlue("player name@homeworld").BuiltString);
            } else if(parts.Length == 3) {
                _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag lock ").AddYellow("layer ").AddGreen("locktype").AddText(" | ")
                .AddPurple("password").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            } else {
                // default case.
                _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag lock ").AddYellow("layer ").AddGreen("locktype").AddText(" | ")
                .AddPurple("password").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            }
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Layer field must be either 1, 2, or 3, indicating the slot the lock is used on.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Locktype field must be a valid locktype. Use ").AddYellow("/gagspeak showlist padlocks ").AddText("to see all valid locktypes.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Password field must be a valid password for the associated locktype. To see spesifics, use ").AddYellow("/gagspeak showlist padlocks ").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Player field must be a valid player name and homeworld. Example: ").AddYellow("FirstName LastName@Bahamut.").BuiltString);
            return false;
        }

        // we have passed in the correct arguments, so begin applying the logic.
        PlayerPayload playerPayload; // get player payload
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"{playerPayload.PlayerName} is putting a {locktype} padlock on {targetplayer}'s layer {layer} gag with password {password}."); // log the action
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            if (parts.Length == 2) {
                // unique string for /gag lock == "from her pocket and uses it to lock your"
                _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetplayer, locktype, layer));
            } else if (parts.Length == 3) {
                // unique string for /gag lock password == "from her pocket and sets the combination password to"
                _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetplayer, locktype, layer, password));
            } else {
                _chat.PrintError("Something unexpected occured!");
                throw new Exception("Something unexpected occured!");
            }
        } catch (Exception e) {
            GagSpeak.Log.Error($"Error sending chat message to player: {e.Message}");
            _chat.PrintError($"Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    // Handles: /gag unlock [layer] | [player target] && /gag unlock [layer] | [password] | [player target]
    private bool GagUnlock(string argument) { // arguement at this point = layer | player target OR layer | password | player target
        // step 1, split by " | " to get the components into the parts we need.
        string[] parts = argument.Split(" | ");
        // if our parts == 2, then we have no password, if our parts == 3, then we have a password. Set our vars now so we dont set them in both statements
        string targetplayer = string.Empty;
        string password = string.Empty;
        string layer = string.Empty;
        if (parts.Length == 2) { // parts = [layer] | [player target]
            // Get the target player name
            targetplayer = parts[1].Trim();
            // get the layer
            layer = parts[0].Trim();
        } 
        else if (parts.Length == 3) { // parts = [layer] | [password] | [player target]
            targetplayer = parts[2].Trim(); // Get the target player name
            password = parts[1].Trim(); // Get the password
            layer = parts[0].Trim(); // get the layer
            // we dont need to check for validation, since that happens on the recieving end.
        }

        // if our arguments are not valid, display help information
        if (! ((layer == "1" || layer == "2" || layer == "3") && targetplayer.Contains("@")) )
        {   // One of our parameters WAS invalid, so display to them the help.
            _chat.Print(new SeStringBuilder().AddRed("Invalid Arguments").BuiltString);
            // do the same if else stuff we did in gaglock
            if(parts.Length == 2){
                _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag unlock ").AddYellow("layer").AddText(" | ").AddPurple("password")
                .AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            } else if (parts.Length == 3) {
                _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag unlock ").AddYellow("layer").AddText(" | ")
                .AddBlue("player name@homeworld").BuiltString);
            } else {
                // default case.
                _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag unlock ").AddYellow("layer").AddText(" | ").AddPurple("password")
                .AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            }
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Layer field must be either 1, 2, or 3, indicating the slot the lock is used on.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Password, if used must be a valid password for the associated locktype on the recieving end, otherwise this will do nothing.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Player field must be a valid player name and homeworld. Example: ").AddYellow("FirstName LastName@Bahamut.").BuiltString);
            return false;
        }
        // we have passed in the correct arguments, so begin applying the logic.
        PlayerPayload playerPayload; // get player payload
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"{playerPayload.PlayerName} is attempting to unlock {targetplayer}'s layer {layer} gag with password {password}."); // log the action
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            if (parts.Length == 2) {
                // unique string for /gag unlock == "reaches behind your neck, taking off the lock that was keeping your"
                _chatManager.SendRealMessage(_gagMessages.GagEncodedUnlockMessage(playerPayload, targetplayer, layer));
            } else if (parts.Length == 3) {
                // unique string for /gag unlock password == "reaches behind your neck and sets the password to"
                _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetplayer, layer, password));
            } else {
                _chat.PrintError("Something unexpected occured!");
                throw new Exception("Something unexpected occured!");
            }
        } catch (Exception e) {
            GagSpeak.Log.Error($"Error sending chat message to player: {e.Message}");
            _chat.PrintError($"Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    // Handles:  /gag remove [layer] | [player target]
    private bool GagRemove(string argument) { // arguement at this point = layer | player target
        // step 1, split by " | " to get the components into the parts we need.
        string[] parts = argument.Split(" | ");
        // if our parts == 2, then we have no password, if our parts == 3, then we have a password. Set our vars now so we dont set them in both statements
        string targetplayer = string.Empty;
        string layer = string.Empty;

        if (parts.Length == 2) { // we just need to make sure that we actually have valid arguements.
            targetplayer = parts[1].Trim(); // Get the password
            layer = parts[0].Trim(); // get the layer
        }
        // log all our variables how they are now
        GagSpeak.Log.Debug($"parts.Length == {parts.Length}, parts[0] = {parts[0]}");

        // if our arguments are not valid, display help information
        if (! ((layer == "1" || layer == "2" || layer == "3") && targetplayer.Contains("@")) )
        {   // One of our parameters WAS invalid, so display to them the help.
            _chat.Print(new SeStringBuilder().AddRed("Invalid Arguments").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag remove ").AddYellow("layer").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Layer field must be either 1, 2, or 3, indicating the slot the lock is used on.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Player field must be a valid player name and homeworld. Example: ").AddYellow("FirstName LastName@Bahamut.").BuiltString);
            return false;
        }
        // we have passed in the correct arguments, so begin applying the logic.
        PlayerPayload playerPayload; // get player payload
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"{playerPayload.PlayerName} is attempting to unlock {targetplayer}'s layer {layer} gag."); // log the action
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            // unique string for /gag remove == "reaches behind your neck and unfastens the buckle of your"
            _chatManager.SendRealMessage(_gagMessages.GagEncodedRemoveMessage(playerPayload, targetplayer, layer));
        } catch (Exception e) {
            _chat.PrintError($"Error sending chat message to player: {e.Message}");
            GagSpeak.Log.Error($"Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    // Handles /gag removeall | [player target]
    private bool GagRemoveAll(string argument) {
        string targetplayer = argument; // Get the playername
        // if our arguments are not valid, display help information
        targetplayer = targetplayer.Trim('|');
        targetplayer = targetplayer.Trim();

        if (!targetplayer.Contains("@"))
        {   // One of our parameters WAS invalid, so display to them the help.
            _chat.Print(new SeStringBuilder().AddRed("Invalid Arguments").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("Correct Usage is: /gag removeall").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "The Player field must be a valid player name and homeworld. Example: ").AddYellow("FirstName LastName@Bahamut.").BuiltString);
            return false;
        }
        // we have passed in the correct arguments, so begin applying the logic.
        PlayerPayload playerPayload; // get player payload
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id);
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"{playerPayload.PlayerName} is attempting removeall of {targetplayer}'s gags."); // log the action
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            // unique string for /gag remove == "reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more."
            _chatManager.SendRealMessage(_gagMessages.GagEncodedRemoveAllMessage(playerPayload, targetplayer));
        } catch (Exception e) {
            _chat.PrintError($"Error sending chat message to player: {e.Message}");
            GagSpeak.Log.Error($"Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    /// <summary>
    /// Verifies that the password is valid for the locktype.
    /// </summary>
    /// <param name="_locktype"></param>
    /// <param name="_password"></param>
    /// <returns></returns>
    private bool IsInvalidPassword(string _locktype, string _password) { // will return false if it password does not pass all condition checks
        if (Enum.TryParse(typeof(GagPadlocks), _locktype, out object parsedEnum)) // find the index of our locktype in enum
        {
            GagSpeak.Log.Debug($"Lock Type: {_locktype} | Password: {_password}");
            // make sure it is a padlock that allows a password
            int index = 0;
            index = (int)parsedEnum;
            GagSpeak.Log.Debug($"{index}");
            // if index == 0,1,4,6,7
            if (index == 0 || index == 1 || index == 4 || index == 6 || index == 7) { // it's a padlock that isnt meant to take any password
                _chat.Print(new SeStringBuilder().AddRed("Invalid Locktype").BuiltString);
                _chat.Print(new SeStringBuilder().AddText("The locktype you have provided does not allow a password. Please use a locktype that allows a password.").BuiltString);
                return true;
            } else if (index == 2) {
                if(!(_password.Length == 4 && _password.All(char.IsDigit))) { // its a combination padlock
                    _chat.Print(new SeStringBuilder().AddRed("Invalid Combination Lock").BuiltString);
                    _chat.Print(new SeStringBuilder().AddText("The password must be a 4 digit combination. EX: 0529 , 6921, ext..").BuiltString);
                    return true;
                }
            } else if ( index == 3 || index == 5) {
                GagSpeak.Log.Debug($"Doing a Password Check for {_locktype}");
                if (_password.Length > 30 || _password.Length == 0) { // if the password is longer than 30 characters, it is invalid.
                    _chat.Print(new SeStringBuilder().AddRed("Invalid Password").BuiltString);
                    _chat.Print(new SeStringBuilder().AddText("The password you have provided is too long. Passwords must be 30 characters or less.").BuiltString);
                    return true;
                }
            } else { // something unexpected occured.
                _chat.Print(new SeStringBuilder().AddRed("Something unexpected occured!").BuiltString);
                _chat.Print(new SeStringBuilder().AddText("You shouldnt be here!").BuiltString);
                return true;
            }
        }
        return false;
    }


    // On the gsm command
    private void OnGSM(string command, string arguments) {
        var argumentList = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (argumentList.Length < 1) {
            PrintHelpGSM("?");
            return;
        }
        // see if our currently selected channel in _config.CurrentChannel is in our list of enabled channels
        if (_config.Channels.Contains(ChatChannel.GetChatChannel())) {
            try {
                // Otherwise, what we have after should be a message to translate into GagSpeak
                GagSpeak.Log.Debug($"Translating message: {arguments}");
                var input = arguments; // get the text input
                var output = this._messageGarbler.GarbleMessage(arguments, _config.GarbleLevel);
                GagSpeak.Log.Debug($"Translated message in gagspeak: {output}");
                _realChatInteraction.SendMessage(output);
                _historyService.AddTranslation(new Translation(input, output));
            }
            catch (Exception e) {
                _chat.PrintError($"Error sending message to chatbox: {e.Message}");
                GagSpeak.Log.Error($"Error sending message to chatbox: {e.Message}");
            }
        } else {
            _chat.Print(new SeStringBuilder().AddRed("Invalid Channel").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("The channel you have selected is not enabled in the config. Please select a valid channel.").BuiltString);
            return;
        }
    }

    private bool PrintHelpGagSpeak(string argument) { // Primary help command
        // if we didn't type help or ?, print the error
        if (!string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) && argument != "?")
            _chat.Print(new SeStringBuilder().AddText("The given argument [ ").AddRed(argument, true).AddText(" ] is not valid.").BuiltString);
        
        // print header for help
        _chat.Print(new SeStringBuilder().AddYellow(" -- Arguments for /gagspeak --").BuiltString);
        // print command arguements
        _chat.Print(new SeStringBuilder().AddCommand("safeword", "Sets your safeword. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("showlist", "Displays the list of padlocks or gags. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("setmode", "Sets plugin mode to domme or sub. Has Cooldown time. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("history", "Displays the history window.").BuiltString);
        return true;
    }

    private bool PrintHelpGag(string argument) {
        // if we didn't type help or ?, print the error
        if (!string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) && argument != "?")
            _chat.Print(new SeStringBuilder().AddText("The given argument [ ").AddRed(argument, true).AddText(" ] is not valid.").BuiltString);

        // print header for help
        _chat.Print(new SeStringBuilder().AddYellow(" -- Default /gag Usage --").BuiltString);
        // print a chat message to explain the default definition of /gag [layer] [gagtype] | [player target] message
        _chat.Print(new SeStringBuilder().AddText("/gag ").AddRed("layer ").AddGreen("gagtype").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("    》").AddText("The Layer field defines which layer the gag is applied to.").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("    》").AddText("The Gagtype field defines which gag is applied to the layer.").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("    》").AddText("The Player field defines who the gag is applied to.").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("Example: ").AddItalics("/gag 1 Ball Gag | Sample Player@Bahamut").BuiltString);
        // print chat message to explain the arguments for /gag
        _chat.Print(new SeStringBuilder().AddYellow(" -- Accessible Submenu's for /gag --").BuiltString);   
        _chat.Print(new SeStringBuilder().AddCommand("/gag lock", "Locks a gag on target. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("/gag unlock", "Unlocks a gag on target. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("/gag remove", "Removes a gag from target. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("/gag removeall", "Removes all gags from target. Use without arguments for help.").BuiltString);
        return true;
    }

    // print the same help format but for /gs
    private bool PrintHelpGSM(string argument) {
        // if we didn't type help or ?, print the error
        if (!string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) && argument != "?")
            _chat.Print(new SeStringBuilder().AddText("The given argument [ ").AddRed(argument, true).AddText(" ] is not valid.").BuiltString);

        _chat.Print(new SeStringBuilder().AddYellow(" -- Default /gsm Usage --").BuiltString);
        // print a chat message to explain the default definition of /gs message
        _chat.Print(new SeStringBuilder().AddText("/gsm ").AddBlue("message").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("    》").AddBlue("message").AddText(" - contains everything after /gsm as a message. The message will be printed" +
            "out to the chat box in it's gagspoken format under the channel your chatbox is currently set to, if enabled in config.").BuiltString);
        return true;
    }
}

#pragma warning restore IDE1006
