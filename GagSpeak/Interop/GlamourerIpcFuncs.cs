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
using GagSpeak.Utility;
using Penumbra.GameData.Actors;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using Penumbra.Api;
using Penumbra.Api.Enums;
using Character = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using Penumbra.GameData.Structs;
using System.Timers;
using GagSpeak.Wardrobe;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>
public class GlamourerIpcFuncs : IDisposable
{
    private readonly    GagSpeakConfig                  _config;                // the plugin interface
    private readonly    GagStorageManager               _gagStorageManager;     // the gag storage manager
    private readonly    RestraintSetManager             _restraintSetManager;   // the restraint set manager
    private readonly    IClientState                    _clientState;           // the client state to get player character
    private readonly    GlamourerInterop                _Interop;               // the glamourer interop class
    private readonly    ItemAutoEquipEvent              _itemAutoEquipEvent;    // the item auto equip event class
    private readonly    JobChangedEvent                 _jobChangedEvent;       // for whenever we change jobs
    private readonly    DalamudPluginInterface          _pluginInterface;       // plugin interface general shit
    private readonly    CharaDataHelpers                _charaDataHelpers;      // character data updates/helpers (framework based)
    public              IObjectTable                    _objectTable;           // the object table for defining gameobject types
    private             Action<int, nint, Lazy<string>> _ChangedDelegate;       // delgate for  GlamourChanged, so it is subscribed and disposed properly
    private             bool                            _disableGlamChangeEvent;// disable the glamour changed event
    private             string?                         _lastCustomizationData; // store the last customization data
    private             CancellationTokenSource         _cts;

    public GlamourerIpcFuncs(GagSpeakConfig gagSpeakConfig, IClientState clientState, IObjectTable objectTable, RestraintSetManager restraintSetManager,
    CharaDataHelpers charaDataHelpers, GlamourerInterop glamourerInterop, ItemAutoEquipEvent itemAutoEquipEvent,
    DalamudPluginInterface pluginInterface, JobChangedEvent jobChangedEvent, GagStorageManager gagStorageManager) {
        // initialize the glamourer interop class
        _config = gagSpeakConfig;
        _clientState = clientState;
        _Interop = glamourerInterop;
        _itemAutoEquipEvent = itemAutoEquipEvent;
        _pluginInterface = pluginInterface;
        _objectTable = objectTable;
        _charaDataHelpers = charaDataHelpers;
        _jobChangedEvent = jobChangedEvent;
        _gagStorageManager = gagStorageManager;
        _restraintSetManager = restraintSetManager;
        // initialize delegate for the glamourer changed event
        _ChangedDelegate = (type, address, customize) => GlamourerChanged(type, address);
        // _throttleTimerHandler = (sender, e) => ThrottleTimerElapsed(e);

        _disableGlamChangeEvent = false;
        _lastCustomizationData = "";
        _cts = new CancellationTokenSource(); // for handling gearset changes
        
        //_Interop._StateChangedSubscriber.Subscribe(_ChangedDelegate);   // to know when glamourer state changes
        _Interop._StateChangedSubscriber.Subscribe(_ChangedDelegate);   // to know when glamourer state changes
        _itemAutoEquipEvent.GagItemEquipped += OnGagEquippedEvent;      // common sense
        _jobChangedEvent.JobChanged += SwitchedJobsEvent;                  // to know when the model is finished drawing
    
        GagSpeak.Log.Debug($"[GlamourerInterop]: GlamourerIpcFuncs initialized!");
        _ = Task.Run(WaitForPlayerToLoad); // wait for player load, then get the player object
    }

    public void Dispose() {
        GagSpeak.Log.Debug($"[GlamourerInterop]: Disposing of GlamourerInterop");
        _Interop._StateChangedSubscriber.Unsubscribe(_ChangedDelegate);
        _itemAutoEquipEvent.GagItemEquipped -= OnGagEquippedEvent;
        _jobChangedEvent.JobChanged -= SwitchedJobsEvent;

    }

    private async Task WaitForPlayerToLoad() {
        try {
            while (!await _charaDataHelpers.GetIsPlayerPresentAsync().ConfigureAwait(false)) {
                await Task.Delay(100).ConfigureAwait(false);
            }
            // player is loaded, so we can now get the player object
            GagSpeak.Log.Debug($"[CLIENTSTATE-LOGIN EVENT]: Character logged in! Caching data!");
            string _lastCustomizationData = await _Interop.GetCharacterCustomizationAsync(_charaDataHelpers.Address);
            GagSpeak.Log.Debug($"[GlamourerChanged]:  from GetAllCustomization: {_lastCustomizationData}");
            // deserialize the customization data
            await Task.Run(() => DeserializeCustomizationData(_lastCustomizationData));
        }
        catch (Exception ex) {
            GagSpeak.Log.Error($"[WaitForPlayerToLoad]: Error waiting for player to load: {ex}");
        }
    }


    // we must account for certain situations, and perform an early exit return or access those functions if they are true.
   private void GlamourerChanged(int type, nint address) {
        // Will be unessisary until after glamourer does its stateChanged update and more inclusion with IPC
        // CONDITION ONE: Make sure it is from the local player, and not another player
        if (address != (_charaDataHelpers.Address) || _config.InDomMode) {
            GagSpeak.Log.Verbose($"[GlamourerChanged]: Change not from Character / In Dom Mode, IGNORING");
            return;
        }
        // CONDITION TWO: _config.EnableWardrobe is false, meaning we shouldnt process any of these
        if(!_config.enableWardrobe) {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Wardrobe is disabled, so we wont be updating/applying any gag items");
            return;
        }
        // CONDITION THREE: we were just executing an ItemAuto-Equip event, so we dont need to worry about it
        if(_itemAutoEquipEvent.IsItemAutoEquipEventExecuting == true || _disableGlamChangeEvent == true) {
            GagSpeak.Log.Verbose($"[GlamourerChanged]: Blocked due to request variables");
            return;
        }
        // CONDITION FOUR: The StateChangeType is a design. Meaning the player changed to a different look via glamourer
        if(type == (int)StateChangeType.Design) {
            GagSpeak.Log.Debug($"[GlamourerChanged]: StateChangeType is Design, Re-Applying any Gags or restraint sets configured if conditions are satisfied");
            // process the latest glamourerData and append our alterations
            try {
                // Cancel the previous task
                _cts.Cancel();
                GagSpeak.Log.Debug($"============================ [ GLAMOUR CHANGED RESULT: DESIGN ] ============================");
                ProcessLastGlamourData();
                return;
            } catch (Exception ex) {
                GagSpeak.Log.Error($"[GlamourerChanged]: Error processing last glamour data: {ex}");
            }
        }
        // CONDITION FIVE: The StateChangeType is an equip or Stain, meaning that the player changed classes in game, or gearsets, causing multiple events to trigger
        if(type == (int)StateChangeType.Equip || type == (int)StateChangeType.Stain || type == (int)StateChangeType.Weapon) {
            var enumType = (StateChangeType)type;  
            GagSpeak.Log.Verbose($"[GlamourerChanged]: StateChangeType is {enumType}");

            // Cancel the previous task
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            // Start a new task with a delay
            Task.Run(async () => {
                try {
                    await Task.Delay(200, _cts.Token); // Wait for 200 ms
                    GagSpeak.Log.Debug($"============================ [ GLAMOUR CHANGED RESULT: GEARSET CHANGE / {enumType} CHANGE ] ============================");
                    ProcessLastGlamourData();
                } catch (TaskCanceledException) {
                    // Task was cancelled, do nothing
                }
            }, _cts.Token);
        } else {
            var enumType = (StateChangeType)type;  
            GagSpeak.Log.Verbose($"[GlamourerChanged]: GlamourerChangedEvent was not equipmenttype, stain, or weapon; but rather {enumType}");
        }
    }
    
    private void ProcessLastGlamourDataTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e) {
        GagSpeak.Log.Debug($"[ThrottleTimerElapsed]: Throttle timer elapsed, processing last glamour data!");
        Task.Run(() => _charaDataHelpers.RunOnFrameworkThread(ProcessLastGlamourData));
    }

    private async void SwitchedJobsEvent(object sender, JobChangedEventArgs e) {
        // Supossedly, all glamourchanged events will process before this fires, so we should be good.
        //await _charaDataHelpers.WaitWhileCharacterIsDrawing();
        GagSpeak.Log.Debug($"[SwitchedJobsEvent]: Character finished drawing, applying gag updates!");
        // will work for now because glamourer IPC is not updated
        _cts.Cancel();
        _disableGlamChangeEvent = true;
        GagSpeak.Log.Debug($"============================ [ GLAMOUR CHANGED RESULT: JOB CHANGE ] ============================");
        await UpdateCachedCharacterData(_lastCustomizationData);
        _disableGlamChangeEvent = false;
    }


    /// <summary> Processes glamourer data, injects info from wardrobe configurations, then sends back to glamourer. </summary>
    private async void ProcessLastGlamourData() {
        // set the item auto-equip to true, so we dont get stuck in a loop
        try {
            _disableGlamChangeEvent = true;
            string _lastCustomizationData = await _Interop.GetCharacterCustomizationAsync(_charaDataHelpers.Address);
            GagSpeak.Log.Debug($"[GlamourerChanged]:  from GetAllCustomization: {_lastCustomizationData}");
            // deserialize the customization data
            await Task.Run(() => DeserializeCustomizationData(_lastCustomizationData));      
            // update the customization with our info
            await UpdateCachedCharacterData(_lastCustomizationData);
            // serialize and send back off the data
            // await Task.Run(() => SerilizationAndApplyCustomizationData());
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[ProcessLastGlamourData]: Error processing last glamour data: {ex}");
        } finally {
            // make sure we set this off once we are done.  
            await Task.Run(() => {
                // sleep this task for a bit so we dont cause a loop
                Thread.Sleep(1000);
                GagSpeak.Log.Debug($"[GlamourerChanged]: re-allowing GlamourChangedEvent");
                _disableGlamChangeEvent = false;
            });
        }
    }

    /// <summary> Deserialize the customization data from the IPC call into JSON format so we can update our cached character class
    /// <list type="bullet">
    /// <item><c>customizationData</c><param name="customizationData">String containing base64 info of customization data</param></item>
    /// </list> </summary>
    public void DeserializeCustomizationData(string? customizationData) {
        #pragma warning disable CS8604 , CS8601 // Possible null reference argument.
        try {
            var bytes = Convert.FromBase64String(customizationData);
            var version = bytes[0];
            version = bytes.DecompressToString(out var decompressed);
            GagSpeak.Log.Debug($"[GlamourerChanged]: Decoded string: {decompressed}");
            _config.cachedCharacterData = JsonConvert.DeserializeObject<GlamourerCharacterData>(decompressed);
        }
        catch (Exception ex) {
            GagSpeak.Log.Error($"[DeserializeCustomizationData]: Error deserializing customization: {ex}");
        }
        #pragma warning restore CS8604 , CS8601 // Possible null reference argument.
    }

    /// <summary> Updates the raw glamourer customization data with our gag items and restraint sets, if applicable
    /// <list type="bullet">
    /// <item><c>customizationData</c><param name="customizationData">String containing base64 info of customization data</param></item>
    /// </list> </summary>
    public async Task UpdateCachedCharacterData(string? customizationData) {
        // next, see if we are allowed to apply restraint sets
        await ApplyRestrainSetToCachedCharacterData();
        
        // for privacy reasons, we must first make sure that our options for allowing such things are enabled.
        if(_config.allowItemAutoEquip) {
            await ApplyGagItemsToCachedCharacterData();
        } else {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Item Auto-Equip is disabled, IGNORING");
        }
    }

    /// <summary> Serialize the customization data back into base64 format, then send it back to glamourer to apply. </summary>
    public void SerilizationAndApplyCustomizationData() {
        // now that the cache is updated and we have player object, we need to serialize and send back off the data.
        try {
            string? json = JsonConvert.SerializeObject(_config.cachedCharacterData);
            GagSpeak.Log.Debug($"[GlamourerChanged]: Serilized json: {json}");
            var compressed = json.Compress(6);
            string base64 = System.Convert.ToBase64String(compressed);
            GagSpeak.Log.Debug($"[GlamourerChanged]: Sending back off the data: {base64}");
            
            Task.Run(async () => {
                await _Interop.ApplyAllToCharacterAsync(base64, _charaDataHelpers.Address);
            });
        }
        catch (Exception ex) {
            GagSpeak.Log.Error($"[SerilizationAndApplyCustomizationData]: Error serilizing and applying customization: {ex}");
        }
    }

    /// <summary> Applies the gag items to the cached character data. </summary>
    public async Task ApplyGagItemsToCachedCharacterData() {
        GagSpeak.Log.Debug($"[ApplyGagItems]: Applying Gag Items to Cached Character Data");
        
        // temporary code until glamourer update's its IPC changes
        await EquipWithSetItem(_config.selectedGagTypes[0], "self");
        await EquipWithSetItem(_config.selectedGagTypes[1], "self");
        await EquipWithSetItem(_config.selectedGagTypes[2], "self");
        
    /*      UNTIL GLAMOURER IPC CAN TAKE GRABBING CUSTOMIZATION CORRECTLY, USE THE FORMAT ABOVE

        // Iterate over each selected gag type
        var i = 1;
        foreach (var gagName in _config.selectedGagTypes) {
            // skip if the gag is none (aka no gag
            if(gagName == "None") {
                GagSpeak.Log.Debug($"[ApplyGagItems]: GagType is None for layer {i}, not setting item.");
                i++;
                continue;
            }

            // Otherwise, find the gag type with the matching alias name
            var gagType = Enum.GetValues(typeof(GagList.GagType))
                .Cast<GagList.GagType>()
                .FirstOrDefault(gt => gt.GetGagAlias() == gagName);
            // If the gag type was found and it exists in the dictionary
            if (_config.gagEquipData.TryGetValue(gagType, out var equipData)) {
                // If the equip data is enabled & and the current gag user is a valid one, then assign it
                if (equipData._isEnabled && ValidGagAssignerUser(gagName, equipData)) {
                    GagSpeak.Log.Debug($"[ApplyGagItems]: Item Auto-Equip for {gagType} is enabled, setting item {equipData._gameItem} : {equipData._gameItem.ItemId.Id} to slot {equipData._slot}");
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
            } else {
                GagSpeak.Log.Error($"[ApplyGagItems]: GagType {gagName} does not exist in the dictionary!");
            }
            i++; // increment I to reflect the layer
        }
    */
    }

    /// <summary> Applies the only enabled restraint set to your character on update trigger. </summary>
    public async Task ApplyRestrainSetToCachedCharacterData() { // dummy placeholder line
        GagSpeak.Log.Debug($"[ApplyRestrainSetToData]: Applying Restraint Set to Cached Character Data");
        // Find the restraint set with the matching name
        foreach (var restraintSet in _restraintSetManager._restraintSets) {
            // If the restraint set is enabled
            if (restraintSet._enabled) {
                // Iterate over each EquipDrawData in the restraint set
                foreach (var pair in restraintSet._drawData) {
                    // If the EquipDrawData is not a nothing item
                    if (!pair.Value._gameItem.Equals(ItemIdVars.NothingItem(pair.Value._slot))) {
                        // Apply the EquipDrawData
                        await _Interop.SetItemToCharacterAsync(
                            _charaDataHelpers.Address, 
                            Convert.ToByte(pair.Key), // the key (EquipSlot)
                            pair.Value._gameItem.Id.Id, // The _drawData._gameItem.Id.Id
                            pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                            0);
                    } else {
                        GagSpeak.Log.Debug($"[ApplyRestrainSetToData]: EquipDrawData for {pair.Key} is a nothing item, skipping!");
                    }
                }
                // early exit, we only want to apply one restraint set
                return;
            }
        }
        GagSpeak.Log.Debug($"[ApplyRestrainSetToData]: No restraint sets are enabled, skipping!");
    }

    public async void OnGagEquippedEvent(object sender, ItemAutoEquipEventArgs e) {
        try {
            // know if we can even do anything anyways
            if(!(_config.enableWardrobe && _config.allowItemAutoEquip)) {
                GagSpeak.Log.Debug($"[OnGagEquippedEvent]: ItemAutoEquip Permissions not granted. Not setting item.");
                return;
            }
            // otherwise, equip the set item
            await EquipWithSetItem(e.GagType, e.AssignerName);
        }
        catch (Exception ex) {
            GagSpeak.Log.Error($"[OnGagEquippedEvent]: Error in OnGagEquippedEvent: {ex}");
        }
        finally {
            // release ItemAutoEquipping once done.
            _itemAutoEquipEvent.IsItemAutoEquipEventExecuting = false;
        }
    }

    public async Task EquipWithSetItem(string gagName, string assignerName = "") {
        // if the gagName is none, then just dont set it and return
        if(gagName == "None") {
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: GagType is None, not setting item.");
            return;
        }
        // Get the gagtype (enum) where it's alias matches the gagName
        var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().FirstOrDefault(g => g.GetGagAlias() == gagName);
        // See if the GagType is in our dictionary & that the gagName is not "None" (because .FirstOrDefault() would make gagType==BallGag when gagName==None)
        if(_gagStorageManager._gagEquipData[gagType]._isEnabled == false) {
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: GagType {gagName} is not enabled, so not setting item.");
            return;
        }
        // otherwise let's do the rest of the stuff
        if(assignerName == "self") _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, "self");
        if(assignerName != "self") _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, assignerName); 

        // see if assigner is valid
        if(ValidGagAssignerUser(gagName, _gagStorageManager._gagEquipData[gagType])) {
            try {
                await _Interop.SetItemToCharacterAsync(_charaDataHelpers.Address, 
                                                       Convert.ToByte(_gagStorageManager._gagEquipData[gagType]._slot),
                                                       _gagStorageManager._gagEquipData[gagType]._gameItem.Id.Id,
                                                       _gagStorageManager._gagEquipData[gagType]._gameStain.Id,
                                                       0
                );
                GagSpeak.Log.Debug($"[OnGagEquippedEvent]: Set item {_gagStorageManager._gagEquipData[gagType]._gameItem} to slot {_gagStorageManager._gagEquipData[gagType]._slot} for gag {gagName}");
            }
            catch (TargetInvocationException ex) {
                GagSpeak.Log.Error($"[OnGagEquippedEvent]: Error setting item: {ex.InnerException}");
            } 
        } else {
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: Assigner {assignerName} is not valid, so not setting item.");
        }
    }

    /// <summary> A Helper function that will return true if ItemAutoEquip should occur based on the assigner name, or if it shouldnt. </summary>
    public bool ValidGagAssignerUser(string gagName, EquipDrawData equipDrawData) {
        // next, we need to see if the gag is being equipped.
        if(equipDrawData._wasEquippedBy == "self") {
            GagSpeak.Log.Debug($"[ValidGagAssignerUser]: GagType {gagName} is being equipped by yourself, abiding by your config settings for Item Auto-Equip.");
            return true;
        }
        // if we hit here, it was an assignerName
        string tempAssignerName = equipDrawData._wasEquippedBy;
        var words = tempAssignerName.Split(' ');
        tempAssignerName = string.Join(" ", words.Take(2)); // should only take the first two words
        // if the name matches anyone in the Whitelist:
        if(_config.Whitelist.Any(w => w.name == tempAssignerName)) {
            // if the relationshipStatus is pet or slave, and the relationshipStatusToYou is Mistress, then we can equip the gag.
            var whitelistCharData = _config.Whitelist.FirstOrDefault(w => w.name == tempAssignerName);
            if(whitelistCharData?.relationshipStatus == "pet" || whitelistCharData?.relationshipStatus == "slave" && whitelistCharData?.relationshipStatusToYou == "Mistress") {
                GagSpeak.Log.Debug($"[ValidGagAssignerUser]: You are a pet/slave to the gag assigner, and {tempAssignerName} is your Mistress. Because this two way relationship is established, allowing Item Auto-Eqiup.");
                return true;
            } 
            else {
                GagSpeak.Log.Debug($"[ValidGagAssignerUser]: {tempAssignerName} is not someone you are a pet or slave to, nor are they defined as your Mistress. Thus, Item Auto-Equip being disabled for this gag.");
                return false;
            }
        }
        else {
            GagSpeak.Log.Debug($"[ValidGagAssignerUser]: GagType {gagName} is being equipped by {tempAssignerName}, but they are not on your whitelist, so we are not doing Item-AutoEquip.");
            return false;
        }
    }
}