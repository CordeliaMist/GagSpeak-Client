using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Internal.Notifications;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using OtterGui.Log;
using GagSpeak.Data;
using GagSpeak.Events;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using GagSpeak.Services;
using Penumbra.GameData.Actors;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using Penumbra.Api;
using Penumbra.Api.Enums;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public sealed class GlamourerIpcFuncs : IDisposable
{
    // define deligate variables here
    private Action<int, nint, Lazy<string>> _ChangedDelegate;
    private readonly GagSpeakConfig _config; // the plugin interface
    private readonly IClientState _clientState; // the client state to get player character
    private readonly GlamourerInterop _Interop; // the glamourer interop class
    private readonly ItemAutoEquipEvent _itemAutoEquipEvent; // the item auto equip event class
    private readonly DalamudPluginInterface _pluginInterface;

    // help with timers
    private System.Timers.Timer? throttleTimer; // Timer to check if the throttle duration has passed
    private Tuple<int, nint, Lazy<string>>? lastGlamourData; // Store data from the last call
    private bool disableGlamourChangedEvent = false; // disable the glamour changed event

    public GlamourerIpcFuncs(GagSpeakConfig gagSpeakConfig, IClientState clientState,
    GlamourerInterop glamourerInterop, ItemAutoEquipEvent itemAutoEquipEvent, DalamudPluginInterface pluginInterface) {
        // initialize the glamourer interop class
        _config = gagSpeakConfig;
        _clientState = clientState;
        _Interop = glamourerInterop;
        _itemAutoEquipEvent = itemAutoEquipEvent;
        _pluginInterface = pluginInterface;
        _ChangedDelegate = (type, address, customize) => GlamourerChanged(type, address, customize);
        
        // subscribe to any events or state changes here
        _clientState.Login += ClientState_Login;                        // login event, to know when player logs in with character
        _Interop._StateChangedSubscriber.Subscribe(_ChangedDelegate);   // to know when glamourer state changes
        _itemAutoEquipEvent.GagItemEquipped += OnGagEquippedEvent;      // to know when a gag item is equipped / updated on the UI / player
    
        GagSpeak.Log.Debug($"[GlamourerInterop]: GlamourerIpcFuncs initialized!");
    }

    public void Dispose() {
        GagSpeak.Log.Debug($"[GlamourerInterop]: Disposing of GlamourerInterop");
        // unsubscribe from any events or state changes here
        _clientState.Login -= ClientState_Login;
        _Interop._StateChangedSubscriber.Unsubscribe(_ChangedDelegate);
        _itemAutoEquipEvent.GagItemEquipped -= OnGagEquippedEvent;

        // Dispose of the throttle timer
        if (throttleTimer != null) {
            throttleTimer.Stop();
            throttleTimer.Elapsed -= ProcessLastGlamourData;
            throttleTimer.Dispose();
            throttleTimer = null;
        }
    }

    public void ClientState_Login() {
        // whenever a player logs in, we will want to initialize their character customization data
        GagSpeak.Log.Debug($"[CLIENTSTATE-LOGIN EVENT]: Character logged in! Caching character customization data!");
        string customizationData = _Interop._GetAllCustomizationFromCharacter?.InvokeFunc(_clientState.LocalPlayer) ?? "";
        if(customizationData == "") {
            GagSpeak.Log.Error($"[CLIENTSTATE-LOGIN EVENT]: Customization data was empty, you will need to change an item on you to get it properly!");
            return;
        }  
        else {
            // update the cached character customization data with character customization data
            UpdateCachedCharacterCustomizationData(new Lazy<string>(() => customizationData));
            // maybe make all gags and restraints apply here if possible later, idk
        }
    }

    // we must account for certain situations, and perform an early exit return or access those functions if they are true.
    private void GlamourerChanged(int type, nint address, Lazy<string> customize) {
        if (_clientState.LocalPlayer == null) { throw new InvalidOperationException("LocalPlayer is null.");} // deals with possible null ref
        // CONDITION ONE: Make sure it is from the local player, and not another player
        if (address != _clientState.LocalPlayer.Address) {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Change not from Character, IGNORING");
            return;
        }
        // Store the data from the current call
        lastGlamourData = new Tuple<int, nint, Lazy<string>>(type, address, customize);

        // CONDITION FOUR: _config.EnableWardrobe is false, meaning we shouldnt process any of these
        if(!_config.enableWardrobe) {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Wardrobe is disabled, so we wont be updating/applying any gag items");
            return;
        }

        // CONDITION TWO: we were just executing an ItemAuto-Equip event, so we dont need to worry about it
        if(_itemAutoEquipEvent.IsItemAutoEquipEventExecuting == true) {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Currently Processing an Item Auto-Equip, or using Apply Glamourer IPC IGNORING");
            return;
        }

        // CONDITION THREE: The StateChangeType is a design. Meaning the player changed to a different look via glamourer
        if(type == (int)StateChangeType.Design) {
            GagSpeak.Log.Debug($"[GlamourerChanged]: StateChangeType is Design, Re-Applying any Gags or restraint sets configured if conditions are satisfied");
            return;
        }
        
        // CONDITION FOUR: The StateChangeType is an equip or Stain, meaning that the player changed classes in game, or gearsets, causing multiple events to trigger
        if(type == (int)StateChangeType.Equip || type == (int)StateChangeType.Stain || type == (int)StateChangeType.Weapon) {
            // Start or restart the throttle timer
            if (throttleTimer == null) {
                throttleTimer = new System.Timers.Timer(200); // Set the interval to 200 ms
                throttleTimer.Elapsed += ProcessLastGlamourData;
                throttleTimer.AutoReset = false; // Prevent the timer from restarting automatically
            }
            throttleTimer.Stop();
            throttleTimer.Start();
            return;
        } else {
            var enumType = (StateChangeType)type;  
            GagSpeak.Log.Debug($"[GlamourerChanged]: GlamourerChangedEvent was not equipmenttype, stain, or weapon; but rather {enumType}");
        }
    }

    // This function is invoked to process the last stored data
    private void ProcessLastGlamourData(object? sender, System.Timers.ElapsedEventArgs e) {
        #pragma warning disable CS8602 // lastGlamourData is never not null at this point
        var enumType = (StateChangeType)lastGlamourData.Item1;
        nint address = lastGlamourData.Item2;
        Lazy<string> customize = lastGlamourData.Item3;
        GagSpeak.Log.Debug($"[GlamourerChanged]: GlamourerChanged Event with type [{enumType}] and address {address} and character customization");
        // disable any other incoming changes
        _itemAutoEquipEvent.IsItemAutoEquipEventExecuting = true;
        // deserialize the customization data
        DeserializeCustomizationData(customize);
        
        // update the customization with our info
        UpdateCachedCharacterCustomizationData(customize);
        
        //SerilizationAndApplyCustomizationData(); // disabled because glamourerAPI sucks ass
        // Reset lastGlamourData to null or any default value after processing
        lastGlamourData = null;

        #pragma warning restore CS8602
    }

    public void UpdateCachedCharacterCustomizationData(Lazy<string> customizationData) {
        try {
            // for privacy reasons, we must first make sure that our options for allowing such things are enabled.
            if(_config.allowItemAutoEquip) {
                ApplyGagItemsToCachedCharacterData();
            } else {
                GagSpeak.Log.Debug($"[GlamourerChanged]: Item Auto-Equip is disabled, IGNORING");
            }

            // next, see if we are allowed to apply restraint sets
            if(_config.allowRestraintLocking) {
                ApplyRestrainSetToCachedCharacterData();
            } else {
                GagSpeak.Log.Debug($"[GlamourerChanged]: Restraint Sets are disabled, IGNORING");
            }
        }
        catch (Exception ex) {
            
            GagSpeak.Log.Error($"[CustomizationGrab]: Error getting customization: {ex}");
        }
    }

    public void DeserializeCustomizationData(Lazy<string> customizationData) {
        try {
            // dont accept any more changed events until this is done
            disableGlamourChangedEvent = true;
            // attempt to send it back off
            string customizationValue = customizationData.Value;
            GagSpeak.Log.Debug($"[GlamourerChanged]: Got customization value: {customizationValue}");
            // Decode the Base64 string
            var bytes = Convert.FromBase64String(customizationValue);
            var version = bytes[0];
            version = bytes.DecompressToString(out var decompressed);
            GagSpeak.Log.Debug($"[GlamourerChanged]: Decoded string: {decompressed}");
            _config.cachedCharacterData = JsonConvert.DeserializeObject<GlamourerCharacterData>(decompressed);
        }
        catch (Exception ex) {
            GagSpeak.Log.Error($"[DeserializeCustomizationData]: Error deserializing customization: {ex}");
        }
    }

    public void SerilizationAndApplyCustomizationData() {
        // now that the cache is updated, we need to serialize and send back off the data.
        try {
        string? json = JsonConvert.SerializeObject(_config.cachedCharacterData);
        GagSpeak.Log.Debug($"[GlamourerChanged]: Serilized json: {json}");
        var compressed = json.Compress(6);
        string base64 = System.Convert.ToBase64String(compressed);
        GagSpeak.Log.Debug($"[GlamourerChanged]: Sending back off the data: {base64}");
        _Interop._ApplyOnlyEquipmentToCharacter?.InvokeAction(base64, _clientState.LocalPlayer);
        }
        catch (Exception ex) {
            GagSpeak.Log.Error($"[SerilizationAndApplyCustomizationData]: Error serilizing and applying customization: {ex}");
        }
    }


    // Apply gag items to the cached customization
    public void ApplyGagItemsToCachedCharacterData() {
        GagSpeak.Log.Debug($"[GlamourerChanged]: Applying Gag Items to Cached Character Data");

        /*      Code for whenever the glamourer API is fixed / working properly
        // Iterate over each selected gag type
        foreach (var gagName in _config.selectedGagTypes) {
            // Find the gag type with the matching alias name
            var gagType = Enum.GetValues(typeof(GagList.GagType))
                .Cast<GagList.GagType>()
                .FirstOrDefault(gt => gt.GetGagAlias() == gagName);

            // If the gag type was found and it exists in the dictionary
            if (_config.gagEquipData.TryGetValue(gagType, out var equipData)) {
                // If the equip data is enabled
                if (equipData._isEnabled) {
                    // update the customization data with the correct slots.
                    switch (equipData._slot) {
                        case EquipSlot.Head:
                            _config.cachedCharacterData.Equipment.Head.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Head.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Body:
                            _config.cachedCharacterData.Equipment.Body.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Body.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Hands:
                            _config.cachedCharacterData.Equipment.Hands.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Hands.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Legs:
                            _config.cachedCharacterData.Equipment.Legs.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Legs.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Feet:
                            _config.cachedCharacterData.Equipment.Feet.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Feet.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Ears:
                            _config.cachedCharacterData.Equipment.Ears.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Ears.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Neck:
                            _config.cachedCharacterData.Equipment.Neck.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Neck.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.Wrists:
                            _config.cachedCharacterData.Equipment.Wrists.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.Wrists.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.RFinger:
                            _config.cachedCharacterData.Equipment.RFinger.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.RFinger.Stain = equipData._gameStain.Id;
                            break;
                        case EquipSlot.LFinger:
                            _config.cachedCharacterData.Equipment.LFinger.ItemId = equipData._gameItem.ItemId.Id;
                            _config.cachedCharacterData.Equipment.LFinger.Stain = equipData._gameStain.Id;
                            break;
                        default:
                            GagSpeak.Log.Error($"[ApplyGagItems]: Invalid slot {equipData._slot} for gag {gagType}");
                            break;
                    }
                } else {
                    GagSpeak.Log.Debug($"[ApplyGagItems]: Item Auto-Equip for {gagType} is not enabled, skipping Auto-Equip!");
                } // at this point, we have altared the customization in the cached character data @ the index
            }
        } // At this point, we have modified all gag slots where possible
        */

        // this is a temp bullshit solution until glamourer API is fixed
        Task.Run(() => {
            Thread.Sleep(10);
            EquipGagWithSetItem(_config.selectedGagTypes[0]);
            Thread.Sleep(10);
            EquipGagWithSetItem(_config.selectedGagTypes[1]);
            Thread.Sleep(10);
            EquipGagWithSetItem(_config.selectedGagTypes[2]);
            GagSpeak.Log.Debug($"[GlamourerChanged]: Finished applying gag items to cached character data");     
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: ITEMAUTOEQUIPEVENTEXECUTING IS NOW FALSE");
            _itemAutoEquipEvent.IsItemAutoEquipEventExecuting = false;    
        });

    }

    public void ApplyRestrainSetToCachedCharacterData() {
        GagSpeak.Log.Debug($"[GlamourerChanged]: Applying Restraint Set to Cached Character Data");
    }

    /// <summary> This event will fire whenever a gag is equipped, from the gagspeak UI or from another players whitelist interaction buttons or from commands.
    /// <para> This is ONLY used for appending a new update to setitem to apply a gag, and NOT to update gags on changed glamour updates. This is handled seperately. </para>
    /// </summary>
    public void OnGagEquippedEvent(object sender, ItemAutoEquipEventArgs e) {
        try {
            // know if we can even do anything anyways
            if(!(_Interop.CheckGlamourerApi() && _config.enableWardrobe)) { GagSpeak.Log.Debug($"[OnGagEquippedEvent]: Glamourer is not available, or wardrobe is disabled. Not setting item."); return; }
            // know if we are already processing an item auto equip event
            EquipGagWithSetItem(e.GagType);
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[OnGagEquippedEvent]: Error in OnGagEquippedEvent: {ex}");
        } finally {
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: ITEMAUTOEQUIPEVENTEXECUTING IS NOW FALSE");
            _itemAutoEquipEvent.IsItemAutoEquipEventExecuting = false;
        }
    }

    public void EquipGagWithSetItem(string gagName) {
        var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().FirstOrDefault(g => g.GetGagAlias() == gagName);
        // make sure that the gagtype exists in the dictionary
        if(_config.gagEquipData.ContainsKey(gagType) && gagName != "None") {
            // then get the equipDrawData from the dictionary
            var equipDrawData = _config.gagEquipData[gagType];
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: Invoking function with: {_clientState.LocalPlayer}, {Convert.ToByte(equipDrawData._slot)}, {equipDrawData._gameItem.Id.Id}, {equipDrawData._gameStain.Id}, 0");
            // then set the item
            try {
                _Interop._SetItem.InvokeFunc( _clientState.LocalPlayer, Convert.ToByte(equipDrawData._slot), equipDrawData._gameItem.Id.Id, equipDrawData._gameStain.Id, 0 );
                GagSpeak.Log.Debug($"[OnGagEquippedEvent]: Set item {equipDrawData._gameItem} to slot {equipDrawData._slot} for gag {gagName}");
            }
            catch (TargetInvocationException ex) {
                GagSpeak.Log.Error($"[OnGagEquippedEvent]: Error setting item: {ex.InnerException}");
            } 
        }
        else {
            GagSpeak.Log.Error($"[OnGagEquippedEvent]: GagType {gagName} does not exist in the dictionary!");
        }
    }
}