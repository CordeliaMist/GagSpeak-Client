using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using GagSpeak.Services;
using Penumbra.Api;
using Penumbra.Api.Enums;
using Penumbra.Api.Helpers;

namespace GagSpeak.Interop.Penumbra;

using CurrentSettings = ValueTuple<PenumbraApiEc, (bool, int, IDictionary<string, IList<string>>, bool)?>;

public readonly record struct Mod(string Name, string DirectoryName) : IComparable<Mod> {
    public int CompareTo(Mod other) {
        var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0)
            return nameComparison;

        return string.Compare(DirectoryName, other.DirectoryName, StringComparison.Ordinal);
    }
}

// gets the settings for the mod, including all details about it.
public readonly record struct ModSettings(IDictionary<string, IList<string>> Settings, int Priority, bool Enabled) {
    public ModSettings() : this(new Dictionary<string, IList<string>>(), 0, false) { }

    public static ModSettings Empty
        => new();
}

// the penumbra service that we will use to interact with penumbra
public unsafe class PenumbraService : IDisposable
{
    public const int RequiredPenumbraBreakingVersion = 4;
    public const int RequiredPenumbraFeatureVersion  = 15;
    private readonly DalamudPluginInterface                                         _pluginInterface;
    private readonly EventSubscriber<ChangedItemType, uint>                         _tooltipSubscriber;
    private readonly EventSubscriber<MouseButton, ChangedItemType, uint>            _clickSubscriber;
    private          ActionSubscriber<GameObject, RedrawType>                       _redrawSubscriber;    // when a target redraws
    private          FuncSubscriber<IList<(string, string)>>                        _getMods;             // gets the mod list for our table
    private          FuncSubscriber<ApiCollectionType, string>                      _currentCollection;   // gets the current collection of our character (0)
    private          FuncSubscriber<string, string, string, bool, CurrentSettings>  _getCurrentSettings;  // we shouldnt need this nessisarily  
    private          FuncSubscriber<string, string, string, bool, PenumbraApiEc>    _setMod;              // set the mod to be enabled or disabled
    private          FuncSubscriber<string, string, string, int, PenumbraApiEc>     _setModPriority;      // change the mod priority while active to that it overrides other things

    private readonly EventSubscriber _initializedEvent;
    private readonly EventSubscriber _disposedEvent;

    public bool Available { get; private set; }

    private readonly OnFrameworkService _frameworkService;
    public PenumbraService(DalamudPluginInterface pi, OnFrameworkService frameworkService) {
        _frameworkService = frameworkService;
        _pluginInterface       = pi;
        _initializedEvent      = Ipc.Initialized.Subscriber(pi, Reattach);
        _disposedEvent         = Ipc.Disposed.Subscriber(pi, Unattach);
        _tooltipSubscriber     = Ipc.ChangedItemTooltip.Subscriber(pi);
        _clickSubscriber       = Ipc.ChangedItemClick.Subscriber(pi);
        Reattach();
    }

    public event Action<MouseButton, ChangedItemType, uint> Click {
        add => _clickSubscriber.Event += value;
        remove => _clickSubscriber.Event -= value;
    }

    public event Action<ChangedItemType, uint> Tooltip {
        add => _tooltipSubscriber.Event += value;
        remove => _tooltipSubscriber.Event -= value;
    }

    // for our get mod list for the table
    public IReadOnlyList<(Mod Mod, ModSettings Settings)> GetMods() {
        if (!Available)
            return Array.Empty<(Mod Mod, ModSettings Settings)>();

        try{
            var allMods    = _getMods.Invoke();
            var collection = _currentCollection.Invoke(ApiCollectionType.Current);
            return allMods
                .Select(m => (m.Item1, m.Item2, _getCurrentSettings.Invoke(collection, m.Item1, m.Item2, true)))
                .Where(t => t.Item3.Item1 is PenumbraApiEc.Success)
                .Select(t => (new Mod(t.Item2, t.Item1),
                    !t.Item3.Item2.HasValue
                        ? ModSettings.Empty
                        : new ModSettings(t.Item3.Item2!.Value.Item3, t.Item3.Item2!.Value.Item2, t.Item3.Item2!.Value.Item1)))
                .OrderByDescending(p => p.Item2.Enabled)
                .ThenBy(p => p.Item1.Name)
                .ThenBy(p => p.Item1.DirectoryName)
                .ThenByDescending(p => p.Item2.Priority)
                .ToList();
        }
        catch (Exception ex) {
            GSLogger.LogType.Error($"Error fetching mods from Penumbra:\n{ex}");
            return Array.Empty<(Mod Mod, ModSettings Settings)>();
        }
    }

    public string CurrentCollection => Available ? _currentCollection.Invoke(ApiCollectionType.Current) : "<Unavailable>"; // gets the current collection type

    /// <summary>
    /// Try to set all mod settings as desired. Only sets when the mod should be enabled.
    /// If it is disabled, ignore all other settings.
    /// </summary>
    public string SetMod(Mod mod, ModSettings settings, bool newSetState, bool disableMods, bool redrawSelf) {
        if (!Available)
            return "Penumbra is not available.";

        var sb = new StringBuilder(); 
        try {
            // get the collection of our character
            var collection = _currentCollection.Invoke(ApiCollectionType.Current);
            // create error code, assume success
            PenumbraApiEc errorCode = PenumbraApiEc.Success;
            // now, if the newsetstate is true, we should enable the mod
            if(newSetState == true) {
                // enable the mod
                errorCode = _setMod.Invoke(collection, mod.DirectoryName, mod.Name, true);
                // get the recieved message
                switch (errorCode) {
                    case PenumbraApiEc.ModMissing:        return $"The mod {mod.Name} [{mod.DirectoryName}] could not be found.";
                    case PenumbraApiEc.CollectionMissing: return $"The collection {collection} could not be found.";
                }
                // after this, raise the priority to 99
                errorCode = _setModPriority.Invoke(collection, mod.DirectoryName, mod.Name, settings.Priority+50);
                Debug.Assert(errorCode is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged, "Setting Priority should not be able to fail.");
            }
            // otherwise, we are attempting to disable the mod
            else {
                // disable the mod, but ONLY if disabledMods is true
                if(disableMods == true) {
                    errorCode = _setMod.Invoke(collection, mod.DirectoryName, mod.Name, false);
                    // get the recieved message
                    switch (errorCode) {
                        case PenumbraApiEc.ModMissing:        return $"The mod {mod.Name} [{mod.DirectoryName}] could not be found.";
                        case PenumbraApiEc.CollectionMissing: return $"The collection {collection} could not be found.";
                    }
                }
                // regardless of if that was on or not, we want to reset it back to their original priority
                errorCode = _setModPriority.Invoke(collection, mod.DirectoryName, mod.Name, settings.Priority);
                Debug.Assert(errorCode is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged, "Setting Priority should not be able to fail.");
            }

            // finally, if we asked to redraw after toggle, redraw
            if(redrawSelf == true) {
                // redraw the object
                RedrawObject(_frameworkService._address, RedrawType.Redraw);
            }

            // get the recieved message
            switch (errorCode) {
                case PenumbraApiEc.ModMissing:        return $"The mod {mod.Name} [{mod.DirectoryName}] could not be found.";
                case PenumbraApiEc.CollectionMissing: return $"The collection {collection} could not be found.";
            }
            // return the invoke message code now built as an SE string
            return sb.ToString();
        }
        catch (Exception ex) {
            return sb.AppendLine(ex.Message).ToString();
        }
    }

    /// <summary> Try to redraw the given actor. </summary>
    public void RedrawObject(IntPtr playerCharObjPtr, RedrawType settings) {
        GameObject playerCharObj = _frameworkService.CreateGameObject(playerCharObjPtr) ?? throw new InvalidOperationException("Player object not found.");
        _redrawSubscriber.Invoke(playerCharObj, settings);

    }

    /// <summary> Reattach to the currently running Penumbra IPC provider. Unattaches before if necessary. </summary>
    public void Reattach() {
        try {
            // unattach from the current penumbra
            Unattach();
            // get the version of the penumbra
            var (breaking, feature) = Ipc.ApiVersions.Subscriber(_pluginInterface).Invoke();
            if (breaking != RequiredPenumbraBreakingVersion || feature < RequiredPenumbraFeatureVersion) {
                throw new Exception(
                    $"Invalid Version {breaking}.{feature:D4}, required major Version {RequiredPenumbraBreakingVersion} with feature greater or equal to {RequiredPenumbraFeatureVersion}.");
            }
            // attach to the penumbra
            _tooltipSubscriber.Enable();
            _clickSubscriber.Enable();
            _redrawSubscriber   = Ipc.RedrawObject.Subscriber(_pluginInterface);
            _getMods            = Ipc.GetMods.Subscriber(_pluginInterface);
            _currentCollection  = Ipc.GetCollectionForType.Subscriber(_pluginInterface);
            _getCurrentSettings = Ipc.GetCurrentModSettings.Subscriber(_pluginInterface);
            _setMod             = Ipc.TrySetMod.Subscriber(_pluginInterface);
            _setModPriority     = Ipc.TrySetModPriority.Subscriber(_pluginInterface);
            Available           = true;
            // _penumbraReloaded.Invoke();
            GSLogger.LogType.Debug("Glamourer attached to Penumbra.");
        }
        catch (Exception e) {
            GSLogger.LogType.Debug($"Could not attach to Penumbra:\n{e}");
        }
    }

    /// <summary> Unattach from the currently running Penumbra IPC provider. </summary>
    public void Unattach() {
        _tooltipSubscriber.Disable();
        _clickSubscriber.Disable();
        if (Available) {
            Available = false;
            GSLogger.LogType.Debug("Glamourer detached from Penumbra.");
        }
    }

    public void Dispose() {
        Unattach();
        _tooltipSubscriber.Dispose();
        _clickSubscriber.Dispose();
        _initializedEvent.Dispose();
        _disposedEvent.Dispose();
    }
}