using System;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game;
using GagSpeak.Services;
using System.Collections.Generic;
using GagSpeak.Utility;
using GagSpeak.Chat.Garbler;
using Dalamud.Plugin.Services;

// I swear to god, if any contributors even attempt to tinker with this file, I will swat you over the head. DO NOT DO IT.

// Signatures located and adopted from sourcecode:
// https://github.com/Caraxi/SimpleTweaksPlugin/blob/6a9e32489d75b63d1915d90720f45f5d75366a87/Tweaks/CommandAlias.cs
// Check for any potential sig changes or changes to code each update and altar accordingly
#pragma warning disable IDE1006
namespace GagSpeak.Chat;
public unsafe class ChatInputProcessor : IDisposable {
    private readonly GagSpeakConfig _config; // for config options
    private readonly HistoryService _historyService; // for history service
    private readonly MessageGarbler _messageGarbler; // for message garbler
    public virtual bool Ready { get; protected set; } // see if ready
    public virtual bool Enabled { get; protected set; } // set if enabled
    private nint processChatInputAddress;
    private unsafe delegate byte ProcessChatInputDelegate(nint uiModule, byte** message, nint a3);
    private HookWrapper<ProcessChatInputDelegate> processChatInputHook = null!; // should be storing the chat message
    public static List<IHookWrapper> HookList = new();

    // equivalent to its "startup" functionality from simpletweaks.
    internal ChatInputProcessor(ISigScanner scanner, IGameInteropProvider interop, GagSpeakConfig config, HistoryService historyService) {
        // initialize interopfromattributes
        _config = config;
        _historyService = historyService;
        _messageGarbler = new MessageGarbler();
        interop.InitializeFromAttributes(this);
        // try to get the chatinput address
        try {
            // try to get the chatinput address
            processChatInputAddress = scanner.ScanText("E8 ?? ?? ?? ?? FE 86 ?? ?? ?? ?? C7 86 ?? ?? ?? ?? ?? ?? ?? ??");
            Ready = true;
            GagSpeak.Log.Debug($"ChatInputProcessor: Found chat input address at {processChatInputAddress:X}");
        } catch (Exception e) {
            // log it if we fail
            GagSpeak.Log.Debug($"{e} : Failed to find chat input address");
        }

        // if we get here, it means we can enable our scanner, because we found the address
        // but just as a failsafe, if we aren't ready, abort and return.
        if (!Ready) {
             GagSpeak.Log.Debug("Why are we here?");
            return;
        }
        //set up our hooks
        // first setup a temp storage to yoink from
        try {
            GagSpeak.Log.Debug("ChatInputProcessor: Setting up hooks");
            var h = interop.HookFromAddress(processChatInputAddress, new ProcessChatInputDelegate(ProcessChatInputDetour));
            GagSpeak.Log.Debug("ChatInputProcessor: Setting up hook wrapper");
            var wh = new HookWrapper<ProcessChatInputDelegate>(h); // make it a hook wrapper
            HookList.Add(wh); // add it to the hook list
            processChatInputHook = wh; // set the hook to the hook wrapper
            processChatInputHook?.Enable(); // enable the hook
            GagSpeak.Log.Debug("ChatInputProcessor: Hook setup complete");
            Enabled = true; // set enabled to true
        }
        catch (Exception e) {
            GagSpeak.Log.Error($"{e} : Failed to setup hooks");
        }
    }

    //next up processing the input
    private unsafe byte ProcessChatInputDetour(nint uiModule, byte** message, nint a3) {
        GagSpeak.Log.Debug("ChatInputProcessor: Processing chat input detour");
        // try the following
        try {
            var bc = 0; // bc = ??? (possibly bit count)
            for (var i=0; i <=500; i++) { // making sure command / message is within 500 characters
                if (*(*message + i) != 0) continue; // if the message is empty, break
                bc = i; // increment bc
                break;
            }
            if(bc < 2 || bc > 500) {
                // if we satsify this condition it means our message is an invalid message so disregard it
                return processChatInputHook.Original(uiModule, message, a3); // just send the message as invalid or whatever
            }
            
            var inputString = Encoding.UTF8.GetString(*message, bc);
            GagSpeak.Log.Debug($"Message: {inputString}"); // see our message
            // first let's make sure its not a command
            if (inputString.StartsWith("/")) {
                // if it is isn't a command, we can just return the original message
                GagSpeak.Log.Debug("ChatInputProcessor: Message is a command, returning original message");
                return processChatInputHook.Original(uiModule, message, a3);
            }

            // if our current channel is in our list of enabled channels AND we have enabled direct chat translation...
            if ( _config.Channels.Contains(Data.ChatChannel.GetChatChannel()) && (_config.DirectChatGarbler == true) ) {
                // if we satisfy this condition, it means we can try to attempt modifying the message.
                GagSpeak.Log.Debug($"ChatInputDetour: Attempting to modify message!");
                // we can try to attempt modifying the message.
                try {
                    GagSpeak.Log.Debug($"ChatInputDetour: input Message -> {inputString}");
                    // create the output translated text
                    var output = _messageGarbler.GarbleMessage(inputString, _config.GarbleLevel);
                    GagSpeak.Log.Debug($"ChatInputDetour: translated Message -> {output}");
                    _historyService.AddTranslation(new Translation(inputString, output));
                    // create the new string
                    var newStr = output;
                    // if our new string is less than or equal to 500 characters, we can alias it
                    if (newStr.Length <= 500) {
                        // log the sucessful alias
                        GagSpeak.Log.Debug($"Aliasing Message: {inputString} -> {newStr}");
                        // encode the new string
                        var bytes = Encoding.UTF8.GetBytes(newStr);
                        // allocate the memory
                        var mem1 = Marshal.AllocHGlobal(400);
                        var mem2 = Marshal.AllocHGlobal(bytes.Length + 30);
                        // copy and write the new memory into the allocated memory
                        Marshal.Copy(bytes, 0, mem2, bytes.Length);
                        Marshal.WriteByte(mem2 + bytes.Length, 0);
                        Marshal.WriteInt64(mem1, mem2.ToInt64());
                        Marshal.WriteInt64(mem1 + 8, 64);
                        Marshal.WriteInt64(mem1 + 8 + 8, bytes.Length + 1);
                        Marshal.WriteInt64(mem1 + 8 + 8 + 8, 0);
                        // properly send off the new message by setting it to r at the right pointer
                        var r = processChatInputHook.Original(uiModule, (byte**) mem1.ToPointer(), a3);
                        // free up the memory we used for assigning
                        Marshal.FreeHGlobal(mem1);
                        Marshal.FreeHGlobal(mem2);
                        // return the result of the alias
                        return r;
                    }
                    // if we reached this point, it means our message was longer than 500 character, inform the user!
                    GagSpeak.Log.Error("Message after translation was just too long!");
                    return 0; // fucking ABORT!
                }
                catch (Exception e) { // if at any point we fail here, throw an exception.
                    GagSpeak.Log.Error($"Error sending message to chatbox: {e.Message}");
                }
            }
        } 
        catch (Exception e) { // cant ever have enough safety!
            GagSpeak.Log.Error($"Error sending message to chatbox (secondary): {e.Message}");
        }
        // return the original message untranslated
        return processChatInputHook.Original(uiModule, message, a3);
    }
    // method to disable the hook
    protected void Disable() {
        processChatInputHook?.Disable();
        Enabled = false;
    }
    // method to dispose of the hook, self explanitory
    public void Dispose() {
        if (!Ready) return;

        foreach (var hook in HookList) {
            hook?.Disable();
            hook?.Dispose();
        }
        HookList.Clear();

        processChatInputHook?.Disable();
        processChatInputHook?.Dispose();
        Ready = false;
        Enabled = false;
    }
}
