using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using System;
using System.Threading.Tasks;
using GagSpeak.Events;
using GagSpeak.CharacterData;

/// <summary> 
/// Stores all information about the player and helper functions for it here.
/// This object is constantly updated on each dalamud framework update.
/// </summary>
namespace GagSpeak.Services;

public class OnFrameworkService : IDisposable
{
    //============== Basic Class Assignment ===================//
    private readonly    IChatGui                        _chat;
    private readonly    IClientState                    _clientState;
    private readonly    ICondition                      _condition;
    private readonly    IFramework                      _framework;
    private readonly    IObjectTable                    _objectTable;
    private readonly    CharacterHandler                _characterHandler;
    //============= Personal Client Character Variables =================//
    public              IntPtr                          _address;                   // player address
    public              int                             _classJobId;                // player class job id
    private             bool                            _sentBetweenAreas = false;  // if we sent a between areas message
    private             ushort                          _lastZone = 0;              // last zone we were in

    public OnFrameworkService(IChatGui chat, IClientState clientState, ICondition condition, IFramework framework,
    IObjectTable objectTable, GagSpeakConfig config, CharacterHandler characterHandler) {
        _chat = chat;
        _clientState = clientState;
        _condition = condition;
        _framework = framework;
        _objectTable = objectTable;
        _characterHandler = characterHandler;
        // set variables that are unassigned
        _address = GetPlayerPointerAsync().GetAwaiter().GetResult();
        // subscribe to the framework update event
        _framework.Update += FrameworkOnUpdate;
        _clientState.Login += ClientStateOnLogin;
    }

    public void Dispose() {
        _clientState.Login -= ClientStateOnLogin;
        _framework.Update -= FrameworkOnUpdate;
    }

    private void ClientStateOnLogin() {
        _address = GetPlayerPointerAsync().GetAwaiter().GetResult();
        _classJobId = (int)(_clientState.LocalPlayer?.ClassJob.Id ?? 0);
        GagSpeak.Log.Debug($"[ClientStateOnLogin]  Player class job id: {_classJobId}");
    }


    /// <summary> The invokable framework function </summary>
    private void FrameworkOnUpdate(IFramework framework) => FrameworkOnUpdateInternal();

    /// <summary> The main framework update function. </summary>
    private unsafe void FrameworkOnUpdateInternal() {
        // if we are not logged in, or are dead, return
        if (_clientState.LocalPlayer?.IsDead ?? false) {
            GagSpeak.Log.Debug($"[FrameworkUpdate]  Player is dead, returning");
            return;
        }
        // If we are zoning, then we need to halt processing
        if (_condition[ConditionFlag.BetweenAreas] || _condition[ConditionFlag.BetweenAreas51]) {
            // log the zone
            var zone = _clientState.TerritoryType;
            // if it is different from our last zone, then we need to send a zone switch start message
            if (_lastZone != zone) {
                // set the last zone to the current zone
                _lastZone = zone;
                // if we are not already sent between area's then make sure we set it
                if (!_sentBetweenAreas) {
                    GagSpeak.Log.Debug($"[ZoneSwitch]  Zone switch/Gpose start");
                    _sentBetweenAreas = true;
                }
            }
            return;
        }
        // if we are between areas, but made it to this point, then it means we are back in the game
        if (_sentBetweenAreas) {
            GagSpeak.Log.Debug($"[ZoneSwitch]  Zone switch/Gpose end");
            // let user know on launch of their direct chat garbler is still enabled
            if (_characterHandler.playerChar._directChatGarblerActive && _characterHandler.playerChar._liveGarblerWarnOnZoneChange)
                _chat.PrintError("[Notice] Direct Chat Garbler is still enabled. A Friendly reminder incase you forgot <3");
            // update the between areas to false
            _sentBetweenAreas = false;
        }

    }

    /// <summary> Fetch a GameObject at spesified pointer </summary>
    public Dalamud.Game.ClientState.Objects.Types.GameObject? CreateGameObject(IntPtr reference) {
        EnsureIsOnFramework();
        return _objectTable.CreateObjectReference(reference);
    }

    /// <summary> Makes sure we are in dalamuds framework thread. </summary>
    public void EnsureIsOnFramework() {
        if (!_framework.IsInFrameworkUpdateThread)
            throw new InvalidOperationException("Can only be run on Framework");
    }

    /// <summary> Gets the pointer of _clientState.LocalPlayer?.Address. This is your PLAYER OBJECT ADDRESS. </summary>
    public IntPtr GetPlayerPointer() {
        EnsureIsOnFramework();
        return _clientState.LocalPlayer?.Address ?? IntPtr.Zero;
    }

    /// <summary> Gets the pointer of _clientState.LocalPlayer?.Address on the framework thread. This is your PLAYER OBJECT ADDRESS. </summary>
    public async Task<IntPtr> GetPlayerPointerAsync() {
        return await RunOnFrameworkThread(GetPlayerPointer).ConfigureAwait(false);
    }

    /// <summary> runs a task on the framework thread if not already done asyncronously </summary>
    public async Task RunOnFrameworkThread(Action action) {
        // If the current thread is not the framework thread
        if (!_framework.IsInFrameworkUpdateThread) {
            // Run the action on the framework thread
            await _framework.RunOnFrameworkThread(action).ConfigureAwait(false);
            // Wait until the current thread is no longer the framework thread
            while (_framework.IsInFrameworkUpdateThread) {
                await Task.Delay(1).ConfigureAwait(false);
            }
        } else {
            action(); // If the current thread is already the framework thread, run the action immediately
        }
    }

    /// <summary> runs a task with a return type on the framework thread, if not already done asyncronously </summary>
    public async Task<T> RunOnFrameworkThread<T>(Func<T> func) {
        if (!_framework.IsInFrameworkUpdateThread) {
            var result = await _framework.RunOnFrameworkThread(func).ConfigureAwait(false);
            while (_framework.IsInFrameworkUpdateThread) {
                await Task.Delay(1).ConfigureAwait(false);
            }
            return result;
        }
        return func.Invoke();
    }
}
