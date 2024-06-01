using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Glamourer.Api.Enums;
using Glamourer.Api.IpcSubscribers;
using GagSpeak.Services;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public sealed class GlamourerService : IDisposable
{
    private readonly DalamudPluginInterface _pluginInterface; // the plugin interface
    private readonly IClientState _clientState; // the client state utility
    private readonly OnFrameworkService _OnFrameworkService; // the game framework utility
    
    /// <summary> Initialize the IPC Subscriber callgates:  </summary>
    private readonly ApiVersion _ApiVersion; // the API version of glamourer
    private readonly ApplyState? _ApplyOnlyEquipment; // for applying equipment to player
    private readonly SetItem _SetItem; // for setting an item to the character (not once by default, must setup with flags)
    private readonly RevertState _RevertCharacter; // for reverting the character
    private readonly RevertToAutomation _RevertToAutomation; // for reverting the character to automation
    private bool _Available = false; // defines if glamourer is currently interactable at all or not.

    public GlamourerService(DalamudPluginInterface pluginInterface, OnFrameworkService OnFrameworkService, IClientState clientState) {
        _pluginInterface = pluginInterface; // initialize the plugin interface
        _OnFrameworkService = OnFrameworkService; // initialize the game framework utility
        _clientState = clientState; // initialize the client state utility

        _ApiVersion = new ApiVersion(_pluginInterface); // get the API version
        _ApplyOnlyEquipment = new ApplyState(_pluginInterface); // get the apply only equipment callgate
        _SetItem = new SetItem(_pluginInterface); // get the set item callgate
        _RevertCharacter = new RevertState(_pluginInterface); // get the revert character callgate
        _RevertToAutomation = new RevertToAutomation(_pluginInterface); // get the revert to automation callgate
    }

    public void Dispose() {
        // revert our character back to the base game state
        if(_clientState.LocalPlayer != null && _clientState.LocalPlayer.Address != IntPtr.Zero) {
            Task.Run(()=>GlamourerRevertCharacterToAutomation(_clientState.LocalPlayer.Address));
        }
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
            var version = _ApiVersion.Invoke();
            // once obtained, check if it matches the version of the currently installed glamourer from the plugin list
            bool versionValid = (_pluginInterface.InstalledPlugins
                .FirstOrDefault(p => string.Equals(p.InternalName, "Glamourer", StringComparison.OrdinalIgnoreCase))
                ?.Version ?? new Version(0, 0, 0, 0)) >= new Version(1, 2, 1, 4);
            if (version is { Major: 1, Minor: >= 1 } && versionValid)
            {
                apiAvailable = true;
            }
            return apiAvailable;
        } catch {
            return apiAvailable;
        } finally {
            if (!apiAvailable) {
                GSLogger.LogType.Error($"[GlamourerService]: Glamourer inactive. All Wardrobe functionality will not work.");
            }
        }
    }

    /// <summary> ========== BEGIN OUR IPC CALL MANAGEMENT UNDER ASYNC TASKS ========== </summary>
    
    // /// <summary> Apply all customizations to the character. </summary>
    /*
    public async Task ApplyAllToCharacterAsync(string? customization, IntPtr character) {
        // If our customization is empty, glamourer is not enabled, or we are zoning, do not process this request.
        if (!CheckGlamourerApi() || string.IsNullOrEmpty(customization)) return;
        try {
            await _OnFrameworkService.RunOnFrameworkThread(() => {
                // set the game object to the character
                var gameObj = _OnFrameworkService.CreateGameObject(character);
                // if the game object is the character, then get the customization for it.
                if (gameObj is Character c) {
                    GSLogger.LogType.Verbose("[ApplyAllAsyncIntrop] Calling on IPC: GlamourerApplyAll");
                    _ApplyOnlyEquipment!.Invoke(customization, c.ObjectIndex, 0, Glamourer.Api.Enums.ApplyFlag.Equipment);
                }
            }).ConfigureAwait(false);
        } catch (Exception) {
            GSLogger.LogType.Debug("[ApplyAllAsyncIntrop] Failed to apply Glamourer data");
        } 
    }
    */

    // /// <summary> Apply only equipment to the character. </summary>
    /*
    public async Task<string> GetCharacterCustomizationAsync(IntPtr character) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return string.Empty;
        try {
            // await for us to be running on the framework thread. Once we are:
            return await _OnFrameworkService.RunOnFrameworkThread(() => {
                // set the game object to the character
                var gameObj = _OnFrameworkService.CreateGameObject(character);
                // if the game object is the character, then get the customization for it.
                if (gameObj is Character c) {
                    GSLogger.LogType.Verbose("[GetCharacterCustomizationAsync] Calling on IPC: GlamourerGetAllCustomizationFromCharacter");
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
    */

    public async Task SetItemToCharacterAsync(IntPtr character, Glamourer.Api.Enums.ApiEquipSlot slot, ulong item, byte dye, uint variant) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return;
        try {
            // await for us to be running on the framework thread. Once we are:
            await _OnFrameworkService.RunOnFrameworkThread(() => {
                // set the game object to the character
                var gameObj = _OnFrameworkService.CreateGameObject(character);
                // if the game object is the character, then get the customization for it.
                if (gameObj is Character c) {
                    _SetItem!.Invoke(c.ObjectIndex, slot, item, dye, 1337);
                }
            }).ConfigureAwait(true);
        } catch(Exception ex) {
            // if at any point this errors, return an empty string as well.
            GSLogger.LogType.Warning($"[SetItemOnceToCharacterAsync] Failed to set item to character with slot {slot}, item {item}, dye {dye}, and key {variant}, {ex}");
            return;
        }
    }

    public async Task GlamourerRevertCharacterToAutomation(IntPtr character) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return;
        try
        {
            // we spesifically DONT want to wait for character to finish drawing because we want to revert before an automation is applied
            await _OnFrameworkService.RunOnFrameworkThread(async () => {
                try
                {
                    // set the game object to the character
                    var gameObj = _OnFrameworkService.CreateGameObject(character);
                    // if the game object is the character, then get the customization for it.
                    if (gameObj is Character c)
                    {
                        GSLogger.LogType.Verbose("[GlamourRevertIPC] Calling on IPC: GlamourerRevertToAutomationCharacter");
                        GlamourerApiEc result = _RevertToAutomation.Invoke(c.ObjectIndex);
                        GSLogger.LogType.Verbose($"[GlamourRevertIPC] Revert to automation result: {result}");
                        // if it doesnt return success, revert to game instead
                        if(result != GlamourerApiEc.Success)
                        {
                            GSLogger.LogType.Warning($"[GlamourRevertIPC] Revert to automation failed, reverting to game instead");
                            await GlamourerRevertCharacter(character);
                        }
                    }
                }
                catch (Exception ex)
                {
                    GSLogger.LogType.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            GSLogger.LogType.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
        }
    }

    public async Task GlamourerRevertCharacter(IntPtr character) {
        // if the glamourerApi is not active, then return an empty string for the customization
        if (!CheckGlamourerApi()) return;
        try
        {
            // we spesifically DONT want to wait for character to finish drawing because we want to revert before an automation is applied
            await _OnFrameworkService.RunOnFrameworkThread(() => {
                try
                {
                    // set the game object to the character
                    var gameObj = _OnFrameworkService.CreateGameObject(character);
                    // if the game object is the character, then get the customization for it.
                    if (gameObj is Character c) {
                        _RevertCharacter.Invoke(c.ObjectIndex);
                    }
                }
                catch (Exception ex)
                {
                    GSLogger.LogType.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
                }
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            GSLogger.LogType.Warning($"[GlamourRevertIPC] Error during GlamourerRevert: {ex}");
        }
    }

}