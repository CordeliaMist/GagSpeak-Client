using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Utility;
using GagSpeak.Data;
using GagSpeak.Services;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;
using GagSpeak.Utility;
using GagSpeak.CharacterData;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public sealed class GlamourerService
{
    private readonly DalamudPluginInterface _pluginInterface; // the plugin interface
    private readonly ClientUserInfo _clientUserInfo; // the game framework utility
    
    /// <summary> Initialize the IPC Subscriber callgates:  </summary>
    public readonly ICallGateSubscriber<(int, int)> _ApiVersions; // Gets Glamourer's API version.
    public readonly ICallGateSubscriber<GameObject?, string>? _GetAllCustomizationFromCharacter; // get all customization from YOUR player character
    public readonly ICallGateSubscriber<string, GameObject?, uint, object>? _ApplyAllToCharacterLock; // applies ALL (customization & gear) to player character, then locks it
    public readonly ICallGateSubscriber<string, GameObject?, uint, object>? _ApplyOnlyEquipmentToCharacterLock; // apply equipment to character, then lock
    public readonly ICallGateSubscriber<string, GameObject?, object>? _ApplyOnlyEquipmentToCharacter; // for applying equipment to player
    public readonly ICallGateSubscriber<GameObject?, uint, bool> _UnlockCharacter; // Unlocks yourself, allowing you to edit again.
    public readonly ICallGateSubscriber<GameObject?, object> _RevertCharacter; // Unlocks yourself, and reverts you back to base game state
    public readonly ICallGateSubscriber<GameObject?, uint, object?> _RevertCharacterLock; // Unlocks yourself, and reverts you back to base game state
    public readonly ICallGateSubscriber<GameObject?, uint, bool> _RevertToAutomationCharacter; // reverts your character to the automation design state of that job.
    public readonly ICallGateSubscriber<GameObject?, byte, ulong, byte, uint, int> _SetItem; // sets an item to your character
    public readonly ICallGateSubscriber<int, nint, Lazy<string>, object?> _StateChangedSubscriber;

    public readonly uint LockCode = 0x6D617265; // setting a lock code for our plugin
    private bool _Available = false; // defines if glamourer is currently interactable at all or not.

    public GlamourerService(DalamudPluginInterface pluginInterface, ClientUserInfo ClientUserInfo) {
        _pluginInterface = pluginInterface; // initialize the plugin interface
        _clientUserInfo = ClientUserInfo; // initialize the game framework utility
        // API callgate
        _ApiVersions = _pluginInterface.GetIpcSubscriber<(int, int)>("Glamourer.ApiVersions");
        // customization callgates
        _GetAllCustomizationFromCharacter = _pluginInterface.GetIpcSubscriber<GameObject?, string>("Glamourer.GetAllCustomizationFromCharacter");
        // apply callgates
        _ApplyAllToCharacterLock = _pluginInterface.GetIpcSubscriber<string, GameObject?, uint, object>("Glamourer.ApplyAllToCharacterLock");
        _ApplyOnlyEquipmentToCharacterLock = _pluginInterface.GetIpcSubscriber<string, GameObject?, uint, object>("Glamourer.ApplyOnlyEquipmentToCharacterLock");
        _ApplyOnlyEquipmentToCharacter = _pluginInterface.GetIpcSubscriber<string, GameObject?, object>("Glamourer.ApplyOnlyEquipmentToCharacter"); // Meant for you
        // unlock & revert callgates
        _UnlockCharacter = _pluginInterface.GetIpcSubscriber<GameObject?, uint, bool>("Glamourer.Unlock");
        _RevertCharacter = _pluginInterface.GetIpcSubscriber<GameObject?, object>("Glamourer.RevertCharacter");
        _RevertCharacterLock = _pluginInterface.GetIpcSubscriber<GameObject?, uint, object?>("Glamourer.RevertCharacterLock");
        _RevertToAutomationCharacter = _pluginInterface.GetIpcSubscriber<GameObject?, uint, bool>("Glamourer.RevertToAutomationCharacter");
        // set item callgate
        _SetItem = _pluginInterface.GetIpcSubscriber<GameObject?, byte, ulong, byte, uint, int>("Glamourer.SetItem"); 
        // also subscribe to the state changed event so we know whenever they try to change an outfit
        _StateChangedSubscriber = _pluginInterface.GetIpcSubscriber<int, nint, Lazy<string>, object?>("Glamourer.StateChanged");
    }

    /// <summary> Checks if Glamourer is active and installed. </summary>
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
            var version = _ApiVersions.InvokeFunc();
            // once obtained, check if it matches the version of the currently installed glamourer from the plugin list
            bool versionValid = (_pluginInterface.InstalledPlugins
                .FirstOrDefault(p => string.Equals(p.InternalName, "Glamourer", StringComparison.OrdinalIgnoreCase))
                ?.Version ?? new Version(0, 0, 0, 0)) >= new Version(1, 0, 6, 1);
            // if the version is 0.1.0 or higher, and the version is valid, then we can assume the API is available.
            if (version.Item1 == 0 && version.Item2 >= 1 && versionValid) {
                apiAvailable = true;
            }
            return apiAvailable;
        } catch {
            return apiAvailable;
        } finally {
            if (!apiAvailable) {
                GagSpeak.Log.Error($"[GlamourerService]: Glamourer inactive. All Wardrobe functionality will not work.");
            }
        }
    }

    /// <summary> ========== BEGIN OUR IPC CALL MANAGEMENT UNDER ASYNC TASKS ========== </summary>
    
    /// <summary> Apply all customizations to the character. </summary>
    public async Task ApplyAllToCharacterAsync(string? customization, IntPtr character) {
        // If our customization is empty, glamourer is not enabled, or we are zoning, do not process this request.
        if (!CheckGlamourerApi() || string.IsNullOrEmpty(customization)) return;
        try {
            await _clientUserInfo.RunOnFrameworkThread(() => {
                // set the game object to the character
                var gameObj = _clientUserInfo.CreateGameObject(character);
                // if the game object is the character, then get the customization for it.
                if (gameObj is Character c) {
                    GagSpeak.Log.Verbose("[ApplyAllAsyncIntrop] Calling on IPC: GlamourerApplyAll");
                    _ApplyOnlyEquipmentToCharacter!.InvokeAction(customization, c); // can modify to be the lock later.
                }
            }).ConfigureAwait(false);
        } catch (Exception) {
            GagSpeak.Log.Debug("[ApplyAllAsyncIntrop] Failed to apply Glamourer data");
        } 
    }

    /// <summary> Apply only equipment to the character. </summary>
    public async Task<string> GetCharacterCustomizationAsync(IntPtr character) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return string.Empty;
        try {
            // await for us to be running on the framework thread. Once we are:
            return await _clientUserInfo.RunOnFrameworkThread(() => {
                // set the game object to the character
                var gameObj = _clientUserInfo.CreateGameObject(character);
                // if the game object is the character, then get the customization for it.
                if (gameObj is Character c) {
                    GagSpeak.Log.Verbose("[GetCharacterCustomizationAsync] Calling on IPC: GlamourerGetAllCustomizationFromCharacter");
                    return _GetAllCustomizationFromCharacter!.InvokeFunc(c);
                }
                // otherwise, just return an empty string.
                return string.Empty;
            }).ConfigureAwait(false);
        } catch {
            // if at any point this errors, return an empty string as well.
            return string.Empty;
        }
    }

    public async Task SetItemToCharacterAsync(IntPtr character, byte slot, ulong item, byte dye, uint variant) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return;
        try {
            // await for us to be running on the framework thread. Once we are:
            await _clientUserInfo.RunOnFrameworkThread(() => {
                // set the game object to the character
                var gameObj = _clientUserInfo.CreateGameObject(character);
                // if the game object is the character, then get the customization for it.
                if (gameObj is Character c) {
                    GagSpeak.Log.Verbose("[GetCharacterCustomizationAsync] Calling on IPC: GlamourSetItemToCharacter");
                    _SetItem!.InvokeFunc(c, slot, item, dye, variant);
                }
                // otherwise, just return an empty string.
                return;
            }).ConfigureAwait(false);
        } catch {
            // if at any point this errors, return an empty string as well.
            return;
        }
    }

    public async Task GlamourerRevertCharacter(IntPtr character) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return;
        try
        {
            // we spesifically DONT want to wait for character to finish drawing because we want to revert before an automation is applied
            await _clientUserInfo.RunOnFrameworkThread(() => {
                try
                {
                    // set the game object to the character
                    var gameObj = _clientUserInfo.CreateGameObject(character);
                    // if the game object is the character, then get the customization for it.
                    if (gameObj is Character c) {
                    //logger.LogDebug("[{appid}] Calling On IPC: GlamourerUnlockName", applicationId);
                    //_glamourerUnlock.InvokeFunc(name, LockCode);
                    //logger.LogDebug("[{appid}] Calling On IPC: GlamourerRevert", applicationId);
                        _RevertCharacter.InvokeAction(c);
                    }
                }
                catch (Exception ex)
                {
                    GagSpeak.Log.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            GagSpeak.Log.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
        }
    }

    public async Task GlamourerRevertCharacterToAutomation(IntPtr character) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return;
        try
        {
            // we spesifically DONT want to wait for character to finish drawing because we want to revert before an automation is applied
            await _clientUserInfo.RunOnFrameworkThread(() => {
                try
                {
                    // set the game object to the character
                    var gameObj = _clientUserInfo.CreateGameObject(character);
                    // if the game object is the character, then get the customization for it.
                    if (gameObj is Character c) {
                    //logger.LogDebug("[{appid}] Calling On IPC: GlamourerUnlockName", applicationId);
                    //_glamourerUnlock.InvokeFunc(name, LockCode);
                    //logger.LogDebug("[{appid}] Calling On IPC: GlamourerRevert", applicationId);
                        _RevertToAutomationCharacter.InvokeFunc(c, 1337);
                    }
                }
                catch (Exception ex)
                {
                    GagSpeak.Log.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            GagSpeak.Log.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
        }
    }
}