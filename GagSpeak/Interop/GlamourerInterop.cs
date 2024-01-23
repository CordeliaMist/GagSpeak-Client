using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Utility;
using GagSpeak.Data;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public sealed class GlamourerInterop
{    
    // most of this stuff is just temp until i can figure out something better
    private readonly DalamudPluginInterface _pluginInterface; // the plugin interface

    /// <summary> Initialize the IPC Subscriber callgates:  </summary>
    public readonly ICallGateSubscriber<(int, int)> _ApiVersions; // for getting the a_pluginInterface version of glamourer's A_pluginInterface

    ///////// IPC for getting the customizations
    public readonly ICallGateSubscriber<GameObject?, string>? _GetAllCustomizationFromCharacter; // for getting all of your customizations (self)
    //public readonly ICallGateSubscriber<string, string>? _GetAllCustomization; // for getting all customization from a particular actor (others)

    ///////// IPC for applying customizations/equipment to self/others
    public readonly ICallGateSubscriber<string, GameObject?, uint, object>? _ApplyAllLockCharacter; // for applying all to player
    //public readonly ICallGateSubscriber<string, string, uint, object>? _ApplyAllLock; // for applying all to an actor (others)
    public readonly ICallGateSubscriber<string, GameObject?, uint, object>? _ApplyOnlyEquipmentToCharacterLock; // for applying equipment to player
    public readonly ICallGateSubscriber<string, GameObject?, object>? _ApplyOnlyEquipmentToCharacter; // for applying equipment to player


    //////// IPC for the reverts & unlocks.
    public readonly ICallGateSubscriber<Character?, uint, object?> _RevertCharacterLock; // revert the lock on you (self)
    //public readonly ICallGateSubscriber<string, uint, object?> _RevertLock; // revert the lock on an actor (others)
    
    public readonly ICallGateSubscriber<Character?, uint, bool> _UnlockCharacter; // for unlocking your character (self)
    //public readonly ICallGateSubscriber<string, uint, bool> _Unlock; // for unlocking an actor (others)

    //////// IPC for Set item. NOTICE: the int from the end of the setItem subscribers are the enum state of the return value for getting the item.    
    public readonly ICallGateSubscriber<Character?, byte, ulong, byte, uint, int> _SetItem; // for setting an item on your character
    //private readonly ICallGateSubscriber<string, byte, ulong, byte, uint, int> _SetItemByActorName; // for setting an item on a particular actor


    //////// IPC for the state changed event
    public readonly ICallGateSubscriber<int, nint, Lazy<string>, object?> _StateChangedSubscriber;

    public readonly uint LockCode = 0x6D617265; // setting a lock code for our plugin
    private bool _Available = false; // defines if glamourer is currently interactable at all or not.

    public GlamourerInterop(DalamudPluginInterface pluginInterface) {
        _pluginInterface = pluginInterface; // initialize the plugin interface

        // initialize the IPC Subscriber callgates:
        _ApiVersions = _pluginInterface.GetIpcSubscriber<(int, int)>("Glamourer.ApiVersions");
        
        _GetAllCustomizationFromCharacter = _pluginInterface.GetIpcSubscriber<GameObject?, string>("Glamourer.GetAllCustomizationFromCharacter"); // Meant for you
        //_GetAllCustomization = _pluginInterface.GetIpcSubscriber<string, string>("Glamourer.GetAllCustomization"); // meant for others

        _ApplyAllLockCharacter = _pluginInterface.GetIpcSubscriber<string, GameObject?, uint, object>("Glamourer.ApplyAllToCharacterLock"); // Meant for you
        //_ApplyAllLock = _pluginInterface.GetIpcSubscriber<string, string, uint, object>("Glamourer.ApplyAllLock"); // meant for others
        _ApplyOnlyEquipmentToCharacterLock = _pluginInterface.GetIpcSubscriber<string, GameObject?, uint, object>("Glamourer.ApplyOnlyEquipmentToCharacterLock"); // Meant for you
        _ApplyOnlyEquipmentToCharacter = _pluginInterface.GetIpcSubscriber<string, GameObject?, object>("Glamourer.ApplyOnlyEquipmentToCharacter"); // Meant for you

        _RevertCharacterLock = _pluginInterface.GetIpcSubscriber<Character?, uint, object?>("Glamourer.RevertCharacterLock"); // meant for you
        //_RevertLock = _pluginInterface.GetIpcSubscriber<string, uint, object?>("Glamourer.RevertLock"); // meant for others

        _UnlockCharacter = _pluginInterface.GetIpcSubscriber<Character?, uint, bool>("Glamourer.Unlock"); // meant for you
        //_Unlock = _pluginInterface.GetIpcSubscriber<string, uint, bool>("Glamourer.UnlockName"); // meant for others

        _SetItem = _pluginInterface.GetIpcSubscriber<Character?, byte, ulong, byte, uint, int>("Glamourer.SetItem"); // meant for you
        //_SetItemByActorName = _pluginInterface.GetIpcSubscriber<string, byte, ulong, byte, uint, int>("Glamourer.SetItemByActorName"); // meant for others

        // also subscribe to the state changed event so we know whenever they try to change an outfit
        _StateChangedSubscriber = _pluginInterface.GetIpcSubscriber<int, nint, Lazy<string>, object?>("Glamourer.StateChanged");
    }


    public bool CheckGlamourerApi() {
        if(CheckGlamourerApiInternal()) {
            _Available = true;
        }
        return _Available;
    }
    /// <summary> An internal check to see if the glamourer API is active or not. Should be used prior to any IPC calls </summary>
    private bool CheckGlamourerApiInternal() {
        bool apiAvailable = false; // assume false at first
        try {
            // declare version of glamourer API
            var version = _ApiVersions.InvokeFunc();
            //GagSpeak.Log.Debug($"[GlamourerInterop]: Glamourer API Version: {version.Item1}.{version.Item2}");
            // once obtained, check if it matches the version of the currently installed glamourer from the plugin list
            bool versionValid = (_pluginInterface.InstalledPlugins
                .FirstOrDefault(p => string.Equals(p.InternalName, "Glamourer", StringComparison.OrdinalIgnoreCase))
                ?.Version ?? new Version(0, 0, 0, 0)) >= new Version(1, 0, 6, 1);
            // if the version is 0.1.0 or higher, and the version is valid, then we can assume the API is available.
            if (version.Item1 == 0 && version.Item2 >= 1 && versionValid) {
                apiAvailable = true;
            }
            // return our condition
            return apiAvailable;
        } catch {
            // if we error at any point just return false
            return apiAvailable;
        } finally {
            // regardless of the outcome, we will want to make sure that
            if (!apiAvailable) {
                GagSpeak.Log.Error($"[GlamourerInterop]: Glamourer inactive. All Wardrobe functionality will not work.");
            }
        }
    }    
}