using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using OtterGui.Classes;
using XivCommon.Functions;
using System.Threading.Tasks;
using GagSpeak.Events;
using GagSpeak.UI;
using GagSpeak.Utility;
using GagSpeak.Wardrobe;
using GagSpeak.Interop;
using GagSpeak.Services;
using GagSpeak.ChatMessages.MessageTransfer;
using System.Runtime.CompilerServices;
using GagSpeak.Gagsandlocks;
using GagSpeak.CharacterData;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.Hardcore.Movement;
using GagSpeak.Hardcore.Actions;
using GagSpeak.Hardcore;
using ImGuiNET;

namespace GagSpeak.ChatMessages;

/// <summary> Handles all of the commands that are used in the plugin. </summary>
public class CommandManager : IDisposable // Our main command list manager
{
    private const string MainCommandString      = "/gagspeak"; // The primary command used for & displays
    private const string ActionsCommandString   = "/gag"; // subcommand for more in-depth actions.
    private const string TranslateCommandString = "/gsm"; // convient subcommand for translating messages
    private const string SafewordCommandString  = "/safeword"; // subcommand for safeword
    private readonly    MessageEncoder          _gagMessages;
    private readonly    ICommandManager         _commands;
    private readonly    MainWindow              _mainWindow;
    private readonly    DebugWindow             _debugWindow;
    private readonly    IChatGui                _chat;
    private readonly    GagSpeakConfig          _config;
    private readonly    OnChatMsgManager             _chatManager;
    private readonly    IClientState            _clientState;
    private             RealChatInteraction     _realChatInteraction;
    private readonly    TimerService            _timerService;
    private readonly    GagService              _gagService;
    private readonly    GagGarbleManager        _gagManager;
    private readonly    GagStorageManager       _gagStorageManager;
    private readonly    RestraintSetManager     _restriantSetManager;
    private readonly    HardcoreManager         _hardcoreManager;
    private readonly    GlamourerService        _glamourerInterop;
    private readonly    CharacterHandler        _characterHandler;
    private readonly    SafewordUsedEvent       _safewordCommandEvent;
    private readonly    GagSpeakGlamourEvent    _glamourEvent;
    // Constructor for the command manager
    public CommandManager(ICommandManager command, MainWindow mainwindow, DebugWindow debugWindow,
    GagSpeakGlamourEvent glamourEvent, RestraintSetManager restraintSetManager, HardcoreManager hardcoreManager,
    IChatGui chat, GagSpeakConfig config, OnChatMsgManager chatManager, IClientState clientState,
    GlamourerService GlamourerService, GagService gagService, CharacterHandler characterHandler,
    GagGarbleManager GagGarbleManager, RealChatInteraction realchatinteraction, TimerService timerService,
    SafewordUsedEvent safewordCommandEvent, MessageEncoder messageEncoder, GagStorageManager gagStorageManager)
    {
        // set the private readonly's to the passed in data of the respective names
        _commands = command;
        _hardcoreManager = hardcoreManager;
        _glamourEvent = glamourEvent;
        _mainWindow = mainwindow;
        _debugWindow = debugWindow;
        _chat = chat;
        _realChatInteraction = realchatinteraction;
        _config = config;
        _chatManager = chatManager;
        _clientState = clientState;
        _gagMessages = messageEncoder;
        _gagService = gagService;
        _gagManager = GagGarbleManager;
        _timerService = timerService;
        _safewordCommandEvent = safewordCommandEvent;
        _gagStorageManager = gagStorageManager;
        _restriantSetManager = restraintSetManager;
        _glamourerInterop = GlamourerService;
        _characterHandler = characterHandler;

        // Add handlers to the main commands
        _commands.AddHandler(MainCommandString, new CommandInfo(OnGagSpeak) {
            HelpMessage = "Toggles main UI when used without arguements. Use with 'help' or '?' to view sub-commands.",
            ShowInHelp = true
        });
        _commands.AddHandler(ActionsCommandString, new CommandInfo(OnGag) {
            HelpMessage = "All commands for gag interactions fall under this.",
            ShowInHelp = true
        });
        _commands.AddHandler(TranslateCommandString, new CommandInfo(OnGSM) {
            HelpMessage = "Translates everything after /gsm into GagSpeak into currently selected chat type in the chat box.",
            ShowInHelp = true
        });
        _commands.AddHandler(SafewordCommandString, new CommandInfo(OnSafeword) {
            HelpMessage = "revert all settings to false and disable any active components. For emergency uses.",
            ShowInHelp = true
        });

        // let user know on launch of their direct chat garbler is still enabled
        if (_characterHandler.playerChar._directChatGarblerActive) {
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
        _commands.RemoveHandler(SafewordCommandString);
    }

#region GagSpeak Help
    // Handler for the main gagspeak command
    private void OnGagSpeak(string command, string arguments)
    {
        var argumentList = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (argumentList.Length < 1) { 
            _mainWindow.Toggle();
            return;
        }

        var argument = argumentList.Length == 2 ? argumentList[1] : string.Empty; // Make arguement be everything after command
        switch(argumentList[0].ToLowerInvariant()) {
            case "restraintset":
                RestraintSet(argument); // when [/gagspeak restraintset] is typed
                return;
            case "showlist":
                Showlist(argument);        // when [/gagspeak showlist] is typed
                return;
            case "debug":
                _debugWindow.Toggle();     // when [/gagspeak debug] is typed
                return;
            case "":
                _mainWindow.Toggle(); // when [/gagspeak] is typed
                return;
            default:
                PrintHelpGagSpeak("help");// when no arguements are passed.
                return;
        };
    }

    private void OnSafeword(string command, string argument) { // Handler for the safeword subcommand
        if (string.IsNullOrWhiteSpace(argument)) { // If no safeword is provided
            _chat.Print("Please provide a safeword. Usage: /gagspeak safeword [your_safeword]"); 
            return;
        }

        // If the safeword is the same as the one we are trying to set and there is no "SafewordUsed" timer
        if (_characterHandler.playerChar._safeword == argument) {
            // see if the safeword is on cooldown
            if (!_timerService.timers.ContainsKey("SafewordUsed")) {
                GagSpeak.Log.Debug($"[Command Manager]: Safeword matched, and is off cooldown, deactivating all gags and locks");
                _chat.Print("Safeword matched, and is off cooldown, deactivating all gags and locks");
                // Disable the ObserveList so we dont trigger the safeword event
                _characterHandler.playerChar._selectedGagTypes.IsSafewordCommandExecuting = true;
                _characterHandler.playerChar._selectedGagPadlocks.IsSafewordCommandExecuting = true;
                // remove all data
                for (int layerIndex = 0; layerIndex < 3; layerIndex++) {
                    _characterHandler.SetPlayerGagType(layerIndex, "None", true, "self");
                    _characterHandler.SetPlayerGagPadlock(layerIndex, Padlocks.None);
                    _characterHandler.SetPlayerGagPadlockPassword(layerIndex, "");
                    _characterHandler.SetPlayerGagPadlockAssigner(layerIndex, "");
                }
                _config.SetHardcoreMode(false);
                _gagStorageManager.ResetEverythingDueToSafeword();
                _restriantSetManager.ResetEverythingDueToSafeword();
                _hardcoreManager.ResetEverythingDueToSafeword();
                
                _timerService.ClearRestraintSetTimer();
                _glamourEvent.Invoke(UpdateType.Safeword); // revert to game state
                try{
                    IntPtr playerAddress = _clientState.LocalPlayer!.Address;
                    Task.Run(async () => await _glamourerInterop.GlamourerRevertCharacterToAutomation(playerAddress));
                } catch (Exception e) {
                    GagSpeak.Log.Error($"Error reverting glamourer to automation: {e.Message}");
                    _chat.PrintError($"Error reverting glamourer to automation upon safeword usage: {e.Message}");
                }
                // Re-enable the ObserveList so we can trigger the safeword event
                _characterHandler.playerChar._selectedGagTypes.IsSafewordCommandExecuting = false;
                _characterHandler.playerChar._selectedGagPadlocks.IsSafewordCommandExecuting = false;
                // Fire the safeword command event
                GagSpeak.Log.Debug($"[Command Manager]: Firing Invoke from CommandManager");
                _safewordCommandEvent.Invoke();
                // fire the safewordUsed bool to true so that we set the cooldown
                _characterHandler.SetSafewordUsed(true);
                var cooldownTime = _config.AdminMode ? "2s" : "10m";
                _timerService.StartTimer("SafewordUsed", cooldownTime, 1000, () => _characterHandler.SetSafewordUsed(false));
            }
            // otherwise inform the user that the cooldown for safeword being used is still present
            else {
                GagSpeak.Log.Debug($"[Command Manager]: Safeword matched, but the usage is still on cooldown");
                _chat.Print("Safeword matched, but the usage is still on cooldown");
            }
        } else { // if the safeword is not the same as the one we are trying to set
            GagSpeak.Log.Debug($"[Command Manager]: Safeword did not match");
            _chat.Print("Safeword did not match!");
        }
        return;
    }

    private bool Showlist(string argument) { // Handler for the showlist subcommand
        if (string.IsNullOrWhiteSpace(argument)) { // If no argument is provided, tell them to spesify
            _chat.Print("Please specify what you want to see. Usage: /gagspeak showlist [padlocks/gags]"); return false; }
        var subCommand = argument.ToLower(); // set what we typed to lowercases to match checks
        if (subCommand == "padlocks") { // if we typed padlocks, show the padlocks list
            _chat.Print(new SeStringBuilder().AddYellow("Displaying the list of padlocks...").BuiltString);
            var padlockTypes = Enum.GetNames(typeof(Padlocks));
            foreach (var padlock in padlockTypes) { _chat.Print(new SeStringBuilder().AddBlue($"    》{padlock}").BuiltString); }
            return true;
        } else if (subCommand == "gags") {
            _chat.Print("List of Gags is very long, to view them easily, view the list in the Wardrobe Gag Storage Compartment!");
            return true;
        } else {
            _chat.Print("Invalid argument. Usage: /gagspeak showlist [padlocks/gags]"); return false;
        }
    }
#endregion GagSpeak Help

#region GagHelp
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
        if (! (_gagService._gagTypes.Any(gag => gag._gagName == gagType) && (layer == "1" || layer == "2" || layer == "3") && targetPlayer.Contains("@")) )
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
        !(Enum.IsDefined(typeof(Padlocks), locktype) && (layer == "1" || layer == "2" || layer == "3") && targetplayer.Contains("@")) )
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
#endregion GagHelp

#region Wardrobe Help
    // Handles: /restraintset lock [Restraint Set Name] | [Timer] | [player target] 
    private bool RestraintSetLock(string argument) { // arguement at this point = setname | timer | player targetlayer locktype | player target
        if(argument == string.Empty) { // if the argument is empty, display help information
            RestraintSetLockHelpText();
            return false;
        }
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // step 1, split by " | " to get the components into the parts we need.
        string[] parts = argument.Split(" | ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // get the parts
        string restraintSetName = parts[0].Trim();
        string timer = parts[1].Trim();
        string targetPlayer = parts[2].Trim();

        // if our arguments are not valid, display help information
        if(!(ValidateWhitelistedPlayer(playerPayload.PlayerName) && ValidateTimer(timer))) {
            RestraintSetLockHelpText();
            return false;
        }

        // we have passed in the correct arguments, so begin applying the logic.
        try{ // try to store the information about the player to the payload, if we fail, throw an exception
            // If sucessful, print our debug messages so we make sure we are sending the correct information
            GagSpeak.Log.Debug($"[Command Manager]: /restraintset lock command sucessful, sending off to Message Encoder.");  
            _chatManager.SendRealMessage(_gagMessages.EncodeWardrobeRestraintSetLock(playerPayload, restraintSetName, timer, targetPlayer));
        } catch (Exception e) {
            GagSpeak.Log.Error($"[Command Manager]: Error sending chat message to player: {e.Message}");
            _chat.PrintError($"[GagSpeak] Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    private void RestraintSetLockHelpText() {
        _chat.Print(new SeStringBuilder().AddRed("Invalid format / arguements for locking restraint set. Correct Format is:").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("  /restraintset lock ").AddYellow("restraint Set Name ").AddText(" | ").AddGreen("timer duration").AddText(" | ").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
        _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
            "restraint set name must be a valid restraint set name in the target players restraint list.").BuiltString);
        _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
            "timer duration must be a valid timer format").BuiltString);
        _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
            "Player must have format: ").AddYellow("FirstName LastName@World.").BuiltString);
    }


    // Handles: /restraintset unlock [Restraint Set Name] | [player target] 
    private bool RestraintSetUnlock(string argument) { // arguement at this point = setname | timer | player targetlayer locktype | player target
        // if the arguement string is not empty, proceed
        if (string.IsNullOrWhiteSpace(argument)) {
            RestraintSetUnlockHelpText();
            return false;
        }
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        // step 1, split by " | " to get the components into the parts we need.
        string[] parts = argument.Split(" | ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // get the parts
        string restraintSetName = parts[0].Trim();
        string targetPlayer = parts[1].Trim();

        // if our arguments are not valid, display help information
        if (!ValidateWhitelistedPlayer(playerPayload.PlayerName) || parts.Length != 2) {
            RestraintSetUnlockHelpText();
            return false;
        }
        try{ 
            GagSpeak.Log.Debug($"[Command Manager]: /restraintset unlock command now sending off to Message Encoder.");  
            _chatManager.SendRealMessage(_gagMessages.EncodeWardrobeRestraintSetUnlock(playerPayload, targetPlayer, restraintSetName));
        } catch (Exception e) {
            GagSpeak.Log.Error($"[Command Manager]: Error sending chat message to player: {e.Message}");
            _chat.PrintError($"[GagSpeak] Error sending chat message to player: {e.Message}");
            return false;
        }
        return true; // sucessful!
    }

    private void RestraintSetUnlockHelpText() {
        _chat.Print(new SeStringBuilder().AddRed("Invalid command format or player name. Correct Format is:").BuiltString);
        _chat.Print(new SeStringBuilder().AddText("  /restraintset unlock ").AddYellow("restraint Set Name").AddText(" | ").AddBlue("player name@homeworld").BuiltString);
        _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
            "restraint set name must be a valid restraint set name in the target players restraint list.").BuiltString);
        _chat.Print(new SeStringBuilder().AddBlue("    》").AddText(
            "Player must have format: ").AddYellow("FirstName LastName@World.").BuiltString);
    }

#endregion Wardrobe Help

#region Helper Functions
    /// <summary>
    /// Verifies that the password is valid for the locktype.
    /// </summary>
    /// <param name="_locktype"></param>
    /// <param name="_password"></param>
    /// <returns></returns>
    private bool IsInvalidPassword(string _locktype, string _password, string playername, string _password2) { // will return false if it password does not pass all condition checks
        bool ret = false;
        
        if (Enum.TryParse(typeof(Padlocks), _locktype, out object? parsedEnum)) {
            switch (parsedEnum) {
                case Padlocks.None:
                    return false;
                case Padlocks.MetalPadlock:
                    ret = !(_password == string.Empty && _password2 == string.Empty);
                    return ret;
                case Padlocks.CombinationPadlock:
                    ret = !(ValidateCombination(_password) && _password2 == string.Empty && _password != string.Empty);
                    return ret;
                case Padlocks.PasswordPadlock:
                    ret = !(ValidatePassword(_password) && _password2 == string.Empty && _password != string.Empty);
                    return ret;
                case Padlocks.FiveMinutesPadlock:
                    ret = !(_password2 == string.Empty && _password == string.Empty);
                    return ret;
                case Padlocks.TimerPasswordPadlock:
                    ret = !(ValidatePassword(_password) && ValidateTimer(_password2));
                    return ret;
                case Padlocks.MistressPadlock:
                    ret = !(ValidateMistress(playername) && _password == string.Empty && _password2 == string.Empty);
                    return ret;
                case Padlocks.MistressTimerPadlock:
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

    private bool ValidateWhitelistedPlayer(string playerName) {
        PlayerPayload playerPayload; // get the current player info
        if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
        else { throw new Exception("Player is null!");}
        if (playerName == playerPayload.PlayerName) { 
            GagSpeak.Log.Debug("[Command Manager]: Player is self, returning true");
            return true;
        }
        if (_characterHandler.whitelistChars.Any(w => playerName.Contains(w._name))) { 
            GagSpeak.Log.Debug("[Command Manager]: Player is whitelisted, returning true");
            return true;
        }
        GagSpeak.Log.Debug("[Command Manager]: Player is not whitelisted, returning false");
        return false;
        
    }
    private bool ValidateMistress(string playerName) {
        PlayerPayload playerPayload; // get the current player info
        if(_clientState.LocalPlayer != null) { playerPayload = new PlayerPayload(_clientState.LocalPlayer.Name.TextValue, _clientState.LocalPlayer.HomeWorld.Id); }
        else { throw new Exception("Player is null!");}
        if (playerName == playerPayload.PlayerName) { return true;}
        if (_characterHandler.whitelistChars.Any(w => playerName.Contains(w._name) && w.IsRoleLeanDominant(w._yourStatusToThem))) { return true;}
        return false;
    }
#endregion Helper Functions
    // On the gsm command
    private void OnGSM(string command, string arguments) {
        var argumentList = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (argumentList.Length < 1) {
            PrintHelpGSM("?");
            return;
        }
        // see if our currently selected channel in _config.CurrentChannel is in our list of enabled channels
        if (_config.ChannelsGagSpeak.Contains(ChatChannel.GetChatChannel())) {
            try {
                // Otherwise, what we have after should be a message to translate into GagSpeak
                var input = arguments; // get the text input
                var output = this._gagManager.ProcessMessage(arguments);
                _realChatInteraction.SendMessage(output);
            }
            catch (Exception e) {
                _chat.PrintError($"[GagSpeak] Error sending message to chatbox: {e.Message}");
                GagSpeak.Log.Error($"[Command Manager]: Error sending message to chatbox: {e.Message}");
            }
        } else {
            _chat.Print(new SeStringBuilder().AddRed("Invalid Channel").BuiltString);
            _chat.Print(new SeStringBuilder().AddText("[GagSpeak] The channel the message was sent to is not enabled in configuration options! Aborting Message ♥").BuiltString);
            return;
        }
    }

    private void RestraintSet(string arguments)
    {
        var argumentList = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (argumentList.Length < 1) {
            PrintHelpWardrobe("?");
            return;
        }
        var argument = argumentList.Length == 2 ? argumentList[1] : string.Empty; // Make arguement be everything after command
        switch(argumentList[0].ToLowerInvariant()) {
            case "lock":
                RestraintSetLock(argument);        // when [/gagspeak safeword] is typed
                return;
            case "unlock":
                RestraintSetUnlock(argument);        // when [/gagspeak showlist] is typed
                return;
            default:
                PrintHelpWardrobe("help");// when no arguements are passed.
                return;
        };
    }

    private bool PrintHelpGagSpeak(string argument) { // Primary help command
        // if we didn't type help or ?, print the error
        if (!string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) && argument != "?")
            _chat.Print(new SeStringBuilder().AddText("The given argument [ ").AddRed(argument, true).AddText(" ] is not valid.").BuiltString);
        
        // print header for help
        _chat.Print(new SeStringBuilder().AddYellow(" -- Arguments for /gagspeak --").BuiltString);
        // print command arguements
        _chat.Print(new SeStringBuilder().AddCommand("showlist", "Displays the list of padlocks or gags. Use without arguments for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("restraintset", "Prints help for restraint set commands. Use alone for help.").BuiltString);
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

    // print the same help format but for /gs
    private bool PrintHelpWardrobe(string argument) {
        // if we didn't type help or ?, print the error
        if (!string.Equals(argument, "help", StringComparison.OrdinalIgnoreCase) && argument != "?")
            _chat.Print(new SeStringBuilder().AddText("The given argument [ ").AddRed(argument, true).AddText(" ] is not valid.").BuiltString);
        // print command help
        _chat.Print(new SeStringBuilder().AddYellow(" -- Default /restraintset Usage --").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("/restraintset lock", "Locks a restraintset on target. Use alone for help.").BuiltString);
        _chat.Print(new SeStringBuilder().AddCommand("/restraintset unlock", "Unlocks a restraintset on target. Use alone for help.").BuiltString);
        return true;
    }
}
