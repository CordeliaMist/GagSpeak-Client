using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Utility;
using System;

// using Penumbra.A_pluginInterface.Enums;
// using Penumbra.A_pluginInterface.Helpers;
using System.Collections.Concurrent;
using System.Text;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public sealed class GlamourerInterop
{
    private readonly DalamudPluginInterface _pluginInterface; // the plugin interface

    /// <summary> Initialize the IPC Subscriber callgates:  </summary>
    private readonly ICallGateSubscriber<(int, int)> _glamourerApiVersions; // for getting the a_pluginInterface version of glamourer's A_pluginInterface

    ///////// IPC for getting the customizations
    private readonly ICallGateSubscriber<GameObject?, string>? _glamourerGetAllCustomizationFromCharacter; // for getting all of your customizations (self)
    //private readonly ICallGateSubscriber<string, string>? _glamourerGetAllCustomization; // for getting all customization from a particular actor (others)

    ///////// IPC for applying customizations/equipment to self/others
    private readonly ICallGateSubscriber<string, GameObject?, uint, object>? _glamourerApplyAllLockCharacter; // for applying all to player
    //private readonly ICallGateSubscriber<string, string, uint, object>? _glamourerApplyAllLock; // for applying all to an actor (others)
    private readonly ICallGateSubscriber<string, GameObject?, uint, object>? _glamourerApplyOnlyEquipmentToCharacterLock; // for applying equipment to player


    //////// IPC for the reverts & unlocks.
    private readonly ICallGateSubscriber<Character?, uint, object?> _glamourerRevertCharacterLock; // revert the lock on you (self)
    //private readonly ICallGateSubscriber<string, uint, object?> _glamourerRevertLock; // revert the lock on an actor (others)
    
    private readonly ICallGateSubscriber<Character?, uint, bool> _glamourerUnlockCharacter; // for unlocking your character (self)
    //private readonly ICallGateSubscriber<string, uint, bool> _glamourerUnlock; // for unlocking an actor (others)

    //////// IPC for Set item. NOTICE: the int from the end of the setItem subscribers are the enum state of the return value for getting the item.    
    private readonly ICallGateSubscriber<Character?, byte, ulong, byte, uint, int> _glamourerSetItem; // for setting an item on your character
    //private readonly ICallGateSubscriber<string, byte, ulong, byte, uint, int> _glamourerSetItemByActorName; // for setting an item on a particular actor

    // setting a lock code for our plugin
    private readonly uint LockCode = 0x6D617265;

    public GlamourerInterop(DalamudPluginInterface pluginInterface) {
        _pluginInterface = pluginInterface; // initialize the plugin interface

        // initialize the IPC Subscriber callgates:
        _glamourerApiVersions = _pluginInterface.GetIpcSubscriber<(int, int)>("Glamourer.A_pluginInterfaceVersions");
        
        _glamourerGetAllCustomizationFromCharacter = _pluginInterface.GetIpcSubscriber<GameObject?, string>("Glamourer.GetAllCustomizationFromCharacter"); // Meant for you
        //_glamourerGetAllCustomization = _pluginInterface.GetIpcSubscriber<string, string>("Glamourer.GetAllCustomization"); // meant for others

        _glamourerApplyAllLockCharacter = _pluginInterface.GetIpcSubscriber<string, GameObject?, uint, object>("Glamourer.ApplyAllToCharacterLock"); // Meant for you
        //_glamourerApplyAllLock = _pluginInterface.GetIpcSubscriber<string, string, uint, object>("Glamourer.ApplyAllLock"); // meant for others
        _glamourerApplyOnlyEquipmentToCharacterLock = _pluginInterface.GetIpcSubscriber<string, GameObject?, uint, object>("Glamourer.ApplyOnlyEquipmentToCharacterLock"); // Meant for you


        _glamourerRevertCharacterLock = _pluginInterface.GetIpcSubscriber<Character?, uint, object?>("Glamourer.RevertCharacterLock"); // meant for you
        //_glamourerRevertLock = _pluginInterface.GetIpcSubscriber<string, uint, object?>("Glamourer.RevertLock"); // meant for others

        _glamourerUnlockCharacter = _pluginInterface.GetIpcSubscriber<Character?, uint, bool>("Glamourer.Unlock"); // meant for you
        //_glamourerUnlock = _pluginInterface.GetIpcSubscriber<string, uint, bool>("Glamourer.UnlockName"); // meant for others

        _glamourerSetItem = _pluginInterface.GetIpcSubscriber<Character?, byte, ulong, byte, uint, int>("Glamourer.SetItem"); // meant for you
        //_glamourerSetItemByActorName = _pluginInterface.GetIpcSubscriber<string, byte, ulong, byte, uint, int>("Glamourer.SetItemByActorName"); // meant for others

        // also subscribe to the state changed event so we know whenever they try to change an outfit
        _pluginInterface.GetIpcSubscriber<int, nint, Lazy<string>, object?>("Glamourer.StateChanged").Subscribe((type, address, customize) => GlamourerChanged(address));

        GagSpeak.Log.Debug($"[GlamourerInterop]: GlamourerInterop initialized!");
    }

    private void GlamourerChanged(nint address) {
        GagSpeak.Log.Debug($"[GlamourerInterop]: GlamourerChanged Event triggered!: {address}");
        // Mediator.Publish(new GlamourerChangedMessage(address));
    }

    // i really dont know wtf im doing with my life right now lol.


    /*
        METHOD ONE: IF we are telling the plugin to equip a certain item whenever the gag is equipped, we should:
            1. Have a UI element that allows user to select the item slot & ID of the item they want to equip.
            (or just get all customization and extract the slots contents from there)
            2. Store this item to a storage in the config or something as an extra attribute for that gag. (could also be in the json for gaglist)
            3. whenever the gag is equipped, the SETITEM IPC will be called.
            4. everytime GLAMOURER'S STATECHANGED IPC is triggered, we check to see if that slot is the same. if it is not, set the item again to keep it on.
            5. When the gag is removed use GLAMOURER'S REVERTCHARACTER or REVERTOAUTOMATIONCHARACTER IPC to revert the changes.
    */

    /*
        METHOD TWO: If we are telling the plugin to lock someone's restraints, we should:
            1. append an additional button for mistress's and slaves called "lock restraints". When pressed, this will:
            2. call the APPLYALLCHARACTERLOCK IPC to lock the players current state.
            (this could be replaced with a looped applyEquipmentLOCK if we want it to be versitile across characters)
            3. scan for the statechanged event.
            4. When they are let free of restraints, revert to the automation of that players class, or just flat out revert them.
    */

        
}