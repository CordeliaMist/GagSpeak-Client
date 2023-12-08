using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterGui.Classes;
using XivCommon.Functions;
using ChatChannel = GagSpeak.Data.ChatChannel;
using GagSpeak.Chat;
using GagSpeak.Chat.MsgEncoder;
using GagSpeak.Data;
using GagSpeak.Events;
using GagSpeak.Chat.Garbler;
using GagSpeak.UI;
using GagSpeak.UI.Helpers;

namespace GagSpeak.Services;

/// <summary>
/// The command manager for the plugin. Handles all of the commands that are used in the plugin.
/// </summary>
public class CommandManager : IDisposable // Our main command list manager
{
    private const string MainCommandString = "/gagspeak"; // The primary command used for & displays
    private const string ActionsCommandString = "/gag"; // subcommand for more in-depth actions.
    private const string TranslateCommandString = "/gsm"; // convient subcommand for translating messages
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
    private readonly IFramework _framework; 
    private readonly TimerService _timerService;
    private readonly GagManager _gagManager;
    private readonly SafewordUsedEvent _safewordCommandEvent;

    // Constructor for the command manager
    public CommandManager(ICommandManager command, MainWindow mainwindow, HistoryWindow historywindow, HistoryService historyService,
    IChatGui chat, GagSpeakConfig config, ChatManager chatManager, IClientState clientState, IFramework framework, GagManager gagManager, 
    RealChatInteraction realchatinteraction, TimerService timerService, SafewordUsedEvent safewordCommandEvent, MessageEncoder messageEncoder)
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
        _gagMessages = messageEncoder;
        _gagManager = gagManager;
        _historyService = historyService;
        _timerService = timerService;
        _safewordCommandEvent = safewordCommandEvent;

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
            
            // Disable the ObserveList so we dont trigger the safeword event
            _config.selectedGagTypes.IsSafewordCommandExecuting = true;
            _config.selectedGagPadlocks.IsSafewordCommandExecuting = true;
            
            for (int layerIndex = 0; layerIndex < _config.selectedGagTypes.Count; layerIndex++) {
                _config.selectedGagTypes[layerIndex] = "None";
                _config.selectedGagPadlocks[layerIndex] = GagPadlocks.None;
                _config.selectedGagPadlocksPassword[layerIndex] = "";
                _config.selectedGagPadlocksAssigner[layerIndex] = "";
            }

            // Re-enable the ObserveList
            _config.selectedGagTypes.IsSafewordCommandExecuting = false;
            _config.selectedGagPadlocks.IsSafewordCommandExecuting = false;

            // Fire the safeword command event 
            _safewordCommandEvent.Invoke();

            // fire the safewordUsed bool to true so that we set the cooldown
            _config.SafewordUsed = true;
            _timerService.StartTimer("SafewordUsed", "15s", 1000, () => _config.SafewordUsed = false);
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
        if (! (GagAndLockTypes.GagTypes.ContainsKey(gagType) && (layer == "1" || layer == "2" || layer == "3") && targetPlayer.Contains("@")) )
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
            if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
            else { throw new Exception("Player is null!");}
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"[Command Manager]: /gag apply command sucessful, sending off to Message Encoder.");
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

    // Handles: /gag lock [layer] [locktype] | [player target] 
    //       && /gag lock [layer] [locktype] | [password] | [player target]
    //       && /gag lock [layer] [locktype] | [password] | [timer] | [player target]
    private bool GagLock(string argument) { // arguement at this point = layer locktype | player target
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // step 1, split by " | " to get the components into the parts we need.
        string[] parts = argument.Split(" | ");
        // if our parts == 2, then we have no password, if our parts == 3, then we have a password. Set our vars now so we dont set them in both statements
        string targetplayer = string.Empty;
        string locktype = string.Empty;
        string password = string.Empty;
        string timer = string.Empty;
        string layer = string.Empty;
        if (parts.Length == 2) { // Condition, no password.
            targetplayer = parts[1].Trim(); // Get the target player name
            // take parts[0], which is [layer] [locktype], and split it so that layer == first word of parts[0] and locktype == the rest of[0]
            string[] layerAndLocktype = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // get the layer
            layer = layerAndLocktype[0];
            // get the locktype
            locktype = string.Join(" ", layerAndLocktype.Skip(1));
        } 
        else if (parts.Length == 3) { // oh my, we have a password
            targetplayer = parts[2].Trim(); // Get the target player name
            password = parts[1].Trim(); // Get the password
            // take parts[0], which is [layer] [locktype], and split it so that layer == first word of parts[0] and locktype == the rest of[0]
            string[] layerAndLocktype = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // get the layer
            layer = layerAndLocktype[0];
            // get the locktype
            locktype = string.Join(" ", layerAndLocktype.Skip(1));
            // verify password.
        }
        else if (parts.Length == 4) {
            targetplayer = parts[3].Trim(); // Get the target player name
            timer = parts[2].Trim(); // Get the password
            password = parts[1].Trim(); // Get the password
            // take parts[0], which is [layer] [locktype], and split it so that layer == first word of parts[0] and locktype == the rest of[0]
            string[] layerAndLocktype = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // get the layer
            layer = layerAndLocktype[0];
            // get the locktype
            locktype = string.Join(" ", layerAndLocktype.Skip(1));
            // verify password.
        }

        // if our arguments are not valid, display help information
        if (IsInvalidPassword(locktype, password, playerPayload.PlayerName, timer) ||
        !(Enum.IsDefined(typeof(GagPadlocks), locktype) && (layer == "1" || layer == "2" || layer == "3") && targetplayer.Contains("@")) )
        {   // One of our parameters WAS invalid, so display to them the help.
            _chat.Print(new SeStringBuilder().AddRed("Invalid format/arguements. Format can be any of the following:").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("  /gag lock ").AddYellow("layer ").AddGreen("locktype").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("  /gag lock ").AddYellow("layer ").AddGreen("locktype").AddText(" | ").AddPurple("password").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("  /gag lock ").AddYellow("layer ").AddGreen("locktype").AddText(" | ").AddPurple("password").AddText(" | ").AddPurple("password2").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "Layer must be either 1, 2, or 3.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "Locktype must be locktype from ").AddYellow("/gagspeak showlist padlocks ").AddText(".").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "Player must have format: ").AddYellow("FirstName LastName@World.").BuiltString);
            return false;
        }

        // we have passed in the correct arguments, so begin applying the logic.
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"[Command Manager]: /gag lock command sucessful, sending off to Message Encoder.");  
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            if (parts.Length == 2) {
                _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetplayer, locktype, layer));
            } else if (parts.Length == 3) {
                _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetplayer, locktype, layer, password));
            } else if (parts.Length == 4) {
                _chatManager.SendRealMessage(_gagMessages.GagEncodedLockMessage(playerPayload, targetplayer, locktype, layer, password, timer));
            } else {
                _chat.PrintError("[GagSpeak] Something unexpected occured!");
                throw new Exception("[Command Manager]: Something unexpected occured!");
            }
        } catch (Exception e) {
            GagSpeak.Log.Error($"[Command Manager]: Error sending chat message to player: {e.Message}");
            _chat.PrintError($"[GagSpeak] Error sending chat message to player: {e.Message}");
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
            _chat.Print(new SeStringBuilder().AddRed("Invalid format/arguements. Format can be any of the following:").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("  /gag unlock ").AddYellow("layer ").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("  /gag unlock ").AddYellow("layer ").AddText(" | ").AddPurple("password").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "Layer must be either 1, 2, or 3.").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "Password must satisfy password conditions of lock types from ").AddYellow("/gagspeak showlist padlocks ").AddText(".").BuiltString);
            _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
                "Player must have format: ").AddYellow("FirstName LastName@World.").BuiltString);
            return false;
        }
        // we have passed in the correct arguments, so begin applying the logic.
        PlayerPayload playerPayload; // get player payload
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
            else { throw new Exception("Player is null!");}
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"[Command Manager]: /gag unlock command sucessful, sending off to Message Encoder.");
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            if (parts.Length == 2) {
                // unique string for /gag unlock == "reaches behind your neck, taking off the lock that was keeping your"
                _chatManager.SendRealMessage(_gagMessages.GagEncodedUnlockMessage(playerPayload, targetplayer, layer));
            } else if (parts.Length == 3) {
                // unique string for /gag unlock password == "reaches behind your neck and sets the password to"
                _chatManager.SendRealMessage(_gagMessages.GagEncodedUnlockMessage(playerPayload, targetplayer, layer, password));
            } else {
                _chat.PrintError("[GagSpeak] Something unexpected occured!");
                throw new Exception("[Command Manager]: Something unexpected occured!");
            }
        } catch (Exception e) {
            GagSpeak.Log.Error($"[Command Manager]: Error sending chat message to player: {e.Message}");
            _chat.PrintError($"[GagSpeak]: Error sending chat message to player: {e.Message}");
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
            if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
            else { throw new Exception("Player is null!");}
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"[Command Manager]: /gag remove command extracted sucessfully, sending off to Message Encoder.");
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            if (layer == "1") { layer = "first"; } else if (layer == "2") { layer = "second"; } else if (layer == "3") { layer = "third"; }
            // unique string for /gag remove == "reaches behind your neck and unfastens the buckle of your"
            _chatManager.SendRealMessage(_gagMessages.GagEncodedRemoveMessage(playerPayload, targetplayer, layer));
        } catch (Exception e) {
            _chat.PrintError($"[GagSpeak] Error sending chat message to player: {e.Message}");
            GagSpeak.Log.Error($"[Command Manager]: Error sending chat message to player: {e.Message}");
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
            if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
            else { throw new Exception("Player is null!");}
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"[Command Manager]: /gag removeall command extracted sucessfully, sending off to Message Encoder.");
            // SENDING INCODED MESSAGE TO PLAYER DISGUISED AS A NORMAL TEXT MESSAGE
            // unique string for /gag remove == "reaches behind your neck and unbuckles all of your gagstraps, allowing you to speak freely once more."
            _chatManager.SendRealMessage(_gagMessages.GagEncodedRemoveAllMessage(playerPayload, targetplayer));
        } catch (Exception e) {
            _chat.PrintError($"[GagSpeak] Error sending chat message to player: {e.Message}");
            GagSpeak.Log.Error($"[Command Manager]: Error sending chat message to player: {e.Message}");
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
    private bool IsInvalidPassword(string _locktype, string _password, string playername, string _password2) { // will return false if it password does not pass all condition checks
        bool ret = false;
        if (Enum.TryParse(typeof(GagPadlocks), _locktype, out object? parsedEnum)) {
            switch (parsedEnum) {
                case GagPadlocks.None:
                    return false;
                case GagPadlocks.MetalPadlock:
                    ret = !(_password == string.Empty && _password2 == string.Empty);
                    return ret;
                case GagPadlocks.CombinationPadlock:
                    ret = !(ValidateCombination(_password) && _password2 == string.Empty && _password != string.Empty);
                    return ret;
                case GagPadlocks.PasswordPadlock:
                    ret = !(ValidatePassword(_password) && _password2 == string.Empty && _password != string.Empty);
                    return ret;
                case GagPadlocks.FiveMinutesPadlock:
                    ret = !(_password2 == string.Empty && _password == string.Empty);
                    return ret;
                case GagPadlocks.TimerPasswordPadlock:
                    ret = !(ValidatePassword(_password) && ValidateTimer(_password2));
                    return ret;
                case GagPadlocks.MistressPadlock:
                    ret = !(ValidateMistress(playername) && _password == string.Empty && _password2 == string.Empty);
                    return ret;
                case GagPadlocks.MistressTimerPadlock:
                    ret = !(ValidateMistress(playername) && ValidateTimer(_password) && _password2 == string.Empty);
                    return ret;
                default:
                    return true;
            }
        }
        return false;
    }

    private bool ValidatePassword(string _inputPassword) { // Passwords must be less than 20 characters and cannot contain spaces
        if(!string.IsNullOrWhiteSpace(_inputPassword) && _inputPassword.Length <= 20 && !_inputPassword.Contains(" ")) {
            return true;
        } else {
            _chat.Print(new SeStringBuilder().AddRed("[GagSpeak] Invalid Password").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("[GagSpeak] The password you have provided is too long. Passwords must be 30 characters or less.").BuiltString);
            return false;
        }
    }
    private bool ValidateCombination(string _inputCombination) { // Combinations must be 4 digits
        if(int.TryParse(_inputCombination, out _) && _inputCombination.Length == 4) {
            return true;
        } else {
            _chat.Print(new SeStringBuilder().AddRed("[GagSpeak] Invalid Combination Lock").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("[GagSpeak] The password must be a 4 digit combination. EX: 0529 , 6921, ext..").BuiltString);
            return false;
        }
    }
    private bool ValidateTimer(string _inputTimer) {
        GagSpeak.Log.Debug($"[Command Manager]: Validating timer: {_inputTimer}");
        if(_inputTimer == string.Empty) { return false; } // if we have no timer, return false
        // Timers must be in the format of 00h00m00s
        var match = Regex.Match(_inputTimer, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
        return match.Success;
    }
    private bool ValidateMistress(string playerName) {
        PlayerPayload playerPayload; // get the current player info
        if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
        else { throw new Exception("Player is null!");}
        if (playerName == playerPayload.PlayerName) { return true;}
        if (_config.Whitelist.Any(w => playerName.Contains(w.name) && w.relationshipStatus == "Mistress")) { return true;}
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
                var input = arguments; // get the text input
                var output = this._gagManager.ProcessMessage(arguments);
                _realChatInteraction.SendMessage(output);
                _historyService.AddTranslation(new Translation(input, output));
            }
            catch (Exception e) {
                _chat.PrintError($"[GagSpeak] Error sending message to chatbox: {e.Message}");
                GagSpeak.Log.Error($"[Command Manager]: Error sending message to chatbox: {e.Message}");
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
