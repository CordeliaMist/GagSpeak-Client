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
using Penumbra.GameData.Interop;
using Penumbra.GameData.Structs;
using Penumbra.Api.IpcSubscribers;

namespace GagSpeak.Interop.Penumbra;

// using CurrentSettings = ValueTuple<PenumbraApiEc, (bool, int, IDictionary<string, IList<string>>, bool)?>;

public readonly record struct Mod(string Name, string DirectoryName) : IComparable<Mod> {
    public int CompareTo(Mod other) {
        var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
        if (nameComparison != 0)
            return nameComparison;

        return string.Compare(DirectoryName, other.DirectoryName, StringComparison.Ordinal);
    }
}

// gets the settings for the mod, including all details about it.
public readonly record struct ModSettings(Dictionary<string, List<string>> Settings, int Priority, bool Enabled) {
    public ModSettings() : this(new Dictionary<string, List<string>>(), 0, false) { }

    public static ModSettings Empty
        => new();
}

// the penumbra service that we will use to interact with penumbra
public unsafe class PenumbraService : IDisposable
{
    public const int RequiredPenumbraBreakingVersion = 5;
    public const int RequiredPenumbraFeatureVersion  = 0;
    private readonly DalamudPluginInterface                                 _pluginInterface;
    private readonly EventSubscriber<ChangedItemType, uint>                 _tooltipSubscriber;
    private readonly EventSubscriber<MouseButton, ChangedItemType, uint>    _clickSubscriber;
    private          RedrawObject?          _redrawSubscriber;    // when a target redraws
    private          GetModList?            _getMods;             // gets the mod list for our table
    private          GetCollection?         _currentCollection;   // gets the current collection of our character (0)
    private          GetCurrentModSettings? _getCurrentSettings;  // we shouldnt need this nessisarily  
    private          TrySetMod?             _setMod;              // set the mod to be enabled or disabled
    private          TrySetModPriority?     _setModPriority;      // change the mod priority while active to that it overrides other things

    private readonly IDisposable _initializedEvent;
    private readonly IDisposable _disposedEvent;
    private readonly OnFrameworkService _frameworkService;


    public bool     Available    { get; private set; }
    public int      CurrentMajor { get; private set; }
    public int      CurrentMinor { get; private set; }
    public DateTime AttachTime   { get; private set; }

    public PenumbraService(DalamudPluginInterface pi, OnFrameworkService frameworkService) {
        _frameworkService = frameworkService;
        _pluginInterface       = pi;
        _initializedEvent      = Initialized.Subscriber(pi, Reattach);
        _disposedEvent         = Disposed.Subscriber(pi, Unattach);
        _tooltipSubscriber     = ChangedItemTooltip.Subscriber(pi);
        _clickSubscriber       = ChangedItemClicked.Subscriber(pi);
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
            var allMods    = _getMods!.Invoke();
            var collection = _currentCollection!.Invoke(ApiCollectionType.Current);
            return allMods
                .Select(m => (m.Key, m.Value, _getCurrentSettings!.Invoke(collection!.Value.Id, m.Key)))
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

    public (Guid Id, string Name) CurrentCollection
        => Available ? _currentCollection!.Invoke(ApiCollectionType.Current)!.Value : (Guid.Empty, "<Unavailable>"); // gets the current collection type

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
            var collection = _currentCollection!.Invoke(ApiCollectionType.Current)!.Value.Id;
            // create error code, assume success
            PenumbraApiEc errorCode = PenumbraApiEc.Success;
            // now, if the newsetstate is true, we should enable the mod
            if(newSetState == true) {
                // enable the mod
                errorCode = _setMod!.Invoke(collection, mod.DirectoryName, true, mod.Name);
                // get the recieved message
                switch (errorCode) {
                    case PenumbraApiEc.ModMissing:        return $"The mod {mod.Name} [{mod.DirectoryName}] could not be found.";
                    case PenumbraApiEc.CollectionMissing: return $"The collection {collection} could not be found.";
                }
                // after this, raise the priority to 99
                errorCode = _setModPriority!.Invoke(collection, mod.DirectoryName, settings.Priority+50, mod.Name);
                Debug.Assert(errorCode is PenumbraApiEc.Success or PenumbraApiEc.NothingChanged, "Setting Priority should not be able to fail.");
            }
            // otherwise, we are attempting to disable the mod
            else {
                // disable the mod, but ONLY if disabledMods is true
                if(disableMods == true) {
                    errorCode = _setMod!.Invoke(collection, mod.DirectoryName, false, mod.Name);
                    // get the recieved message
                    switch (errorCode) {
                        case PenumbraApiEc.ModMissing:        return $"The mod {mod.Name} [{mod.DirectoryName}] could not be found.";
                        case PenumbraApiEc.CollectionMissing: return $"The collection {collection} could not be found.";
                    }
                }
                // regardless of if that was on or not, we want to reset it back to their original priority
                errorCode = _setModPriority!.Invoke(collection, mod.DirectoryName, settings.Priority, mod.Name);
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
        _redrawSubscriber!.Invoke(playerCharObj.ObjectIndex, settings);

    }

    /// <summary> Reattach to the currently running Penumbra IPC provider. Unattaches before if necessary. </summary>
    public void Reattach() {
        try {
            // unattach from the current penumbra
            Unattach();

            AttachTime = DateTime.UtcNow;
            try
            {
                (CurrentMajor, CurrentMinor) = new global::Penumbra.Api.IpcSubscribers.ApiVersion(_pluginInterface).Invoke();
            }
            catch
            {
                try
                {
                    (CurrentMajor, CurrentMinor) = new global::Penumbra.Api.IpcSubscribers.Legacy.ApiVersions(_pluginInterface).Invoke();
                }
                catch
                {
                    CurrentMajor = 0;
                    CurrentMinor = 0;
                    throw;
                }
            }
            // if its broken, dont reattach
            if (CurrentMajor != RequiredPenumbraBreakingVersion || CurrentMinor < RequiredPenumbraFeatureVersion)
            {
                throw new Exception(
                    $"Invalid Version {CurrentMajor}.{CurrentMinor:D4}, required major Version {RequiredPenumbraBreakingVersion} with feature greater or equal to {RequiredPenumbraFeatureVersion}.");
            }
            // attach to the penumbra
            _tooltipSubscriber.Enable();
            _clickSubscriber.Enable();
            _redrawSubscriber   = new RedrawObject(_pluginInterface);
            _getMods            = new GetModList(_pluginInterface);
            _currentCollection  = new GetCollection(_pluginInterface);
            _getCurrentSettings = new GetCurrentModSettings(_pluginInterface);
            _setMod             = new TrySetMod(_pluginInterface);
            _setModPriority     = new TrySetModPriority(_pluginInterface);
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