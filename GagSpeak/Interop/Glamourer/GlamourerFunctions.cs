using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using GagSpeak.Events;
using GagSpeak.Utility;
using GagSpeak.Wardrobe;
using GagSpeak.CharacterData;
using GagSpeak.Gagsandlocks;
using GagSpeak.UI.Equipment;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using GagSpeak.Services;

namespace GagSpeak.Interop;

/// <summary>
/// Create a sealed class for our interop manager.
/// </summary>

public class GlamourerFunctions : IDisposable
{
    public static bool disableGlamChangeEvent { get; set; } = false;           // disables the glam change event
    public static bool finishedDrawingGlamChange { get; set; } = false;        // disables the glamourer
    private readonly    GagSpeakConfig                  _config;                // the plugin interface
    private readonly    GagStorageManager               _gagStorageManager;     // the gag storage manager
    private readonly    RestraintSetManager             _restraintSetManager;   // the restraint set manager
    private readonly    CharacterHandler                _characterHandler;      // the character handler for managing player and whitelist info
    private readonly    GlamourerService                _Interop;               // the glamourer interop class
    private readonly    IFramework                      _framework;             // the framework for running tasks on the main thread
    private readonly    GagSpeakGlamourEvent            _gagSpeakGlamourEvent;  // for whenever glamourer changes
    private readonly    OnFrameworkService              _onFrameworkService;      // character data updates/helpers (framework based)
    private             Action<StateChangeType, nint, Lazy<string>> _ChangedDelegate;       // delgate for  GlamourChanged, so it is subscribed and disposed properly
    private             CancellationTokenSource         _cts;

    public GlamourerFunctions(GagSpeakConfig gagSpeakConfig, GagStorageManager gagStorageManager, RestraintSetManager restraintSetManager,
    CharacterHandler characterHandler, GlamourerService GlamourerService, IFramework framework, GagSpeakGlamourEvent gagSpeakGlamourEvent,
    OnFrameworkService onFrameworkService) {
        // initialize the glamourer interop class
        _config = gagSpeakConfig;
        _gagStorageManager = gagStorageManager;
        _restraintSetManager = restraintSetManager;
        _characterHandler = characterHandler;
        _Interop = GlamourerService;
        _framework = framework;
        _gagSpeakGlamourEvent = gagSpeakGlamourEvent;
        _onFrameworkService = onFrameworkService;
        // initialize delegate for the glamourer changed event
        _ChangedDelegate = (type, address, customize) => GlamourerChanged(type, address);
        _cts = new CancellationTokenSource(); // for handling gearset changes
        
        _Interop._StateChangedSubscriber.Subscribe(_ChangedDelegate);    // to know when glamourer state changes
        _gagSpeakGlamourEvent.GlamourEventFired += GlamourEventFired;    // to know when we update any setting that should trigger a particular glamour refresh.
        _framework.Update += FrameworkUpdate;                            // to know when we should process the last glamour data
        
        GagSpeak.Log.Debug($"[GlamourerService]: GlamourerFunctions initialized!");
    }

    public void Dispose() {
        GagSpeak.Log.Debug($"[GlamourerService]: Disposing of GlamourerService");
        _Interop._StateChangedSubscriber.Unsubscribe(_ChangedDelegate);
        _gagSpeakGlamourEvent.GlamourEventFired -= GlamourEventFired;
        _framework.Update -= FrameworkUpdate;
    }

    public void FrameworkUpdate(IFramework framework) {
        // if we have finiahed drawing
        if(finishedDrawingGlamChange){
            // and we have disabled the glamour change event still
            if(disableGlamChangeEvent) {
                // make sure to turn that off and reset it
                finishedDrawingGlamChange = false;
                disableGlamChangeEvent = false;
                GagSpeak.Log.Debug($"[FrameworkUpdate] Re-Allowing Glamour Change Event");
            }
        } 
    }

    // we must account for certain situations, and perform an early exit return or access those functions if they are true.
    // another slim

    private void GlamourerChanged(StateChangeType type, nint address) {
        // just know the type and address
        if (address != _onFrameworkService._address) {
            GagSpeak.Log.Verbose($"[GlamourerChanged]: Change not from Character, IGNORING"); return;
        }
        
        // make sure we have wardrobe enabled
        if(!_characterHandler.playerChar._enableWardrobe) {
            GagSpeak.Log.Verbose($"[GlamourerChanged]: Wardrobe is disabled, so we wont be updating/applying any gag items");
            return;
        }
        
        // make sure we are not already processing a glamour event
        if(_gagSpeakGlamourEvent.IsGagSpeakGlamourEventExecuting || disableGlamChangeEvent) {
            GagSpeak.Log.Verbose($"[GlamourerChanged]: Blocked due to request variables");
            return;
        }
        
        // if it is a design change, then we should reapply the gags and restraint sets
        if(type == StateChangeType.Design || type == StateChangeType.Reapply ||  type == StateChangeType.Reset) {
            GagSpeak.Log.Verbose($"[GlamourerChanged]: StateChangeType is Design, Re-Applying any Gags or restraint sets configured if conditions are satisfied");
            // process the latest glamourerData and append our alterations
            _gagSpeakGlamourEvent.Invoke(UpdateType.RefreshAll);
            return;
        }
        
        // CONDITION FIVE: The StateChangeType is an equip or Stain, meaning that the player changed classes in game, or gearsets, causing multiple events to trigger
        if(type == StateChangeType.Equip || type == StateChangeType.Stain || type == StateChangeType.Weapon) {
            var enumType = (StateChangeType)type;  
            GagSpeak.Log.Verbose($"[GlamourerChanged]: StateChangeType is {enumType}");
            _gagSpeakGlamourEvent.Invoke(UpdateType.RefreshAll);
        } else {
            var enumType = (StateChangeType)type;  
            GagSpeak.Log.Verbose($"[GlamourerChanged]: GlamourerChangedEvent was not equipmenttype, stain, or weapon; but rather {enumType}");
        }
    }

    private static  SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    public async void GlamourEventFired(object sender, GagSpeakGlamourEventArgs e) {
        // Otherwise, fire the events!
        _cts.Cancel();
        await semaphore.WaitAsync();
        disableGlamChangeEvent = true;
        // only execute if our wardrobe is enabled
        if(_characterHandler.playerChar._enableWardrobe) {
            GagSpeak.Log.Debug($"================= [ "+ e.UpdateType.ToString().ToUpper()+" GLAMOUR EVENT FIRED ] ====================");
            // conditionals:
            // condition 1 --> It was an update restraint set event. In this case, we should recall the restraint set applier
            try{
                if(e.UpdateType == UpdateType.UpdateRestraintSet && _characterHandler.playerChar._allowRestraintSetAutoEquip) {
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Restraint Set Update");
                    await ApplyRestrainSetToCachedCharacterData(); // correct
                    // if any of our gags are not none
                    if(_characterHandler.playerChar._allowItemAutoEquip && _characterHandler.playerChar._selectedGagTypes.Any(g => g != "None")) {
                        await ApplyGagItemsToCachedCharacterData();
                    }
                }
                // condition 2 --> it was an disable restraint set event, we should revert back to automation, but then reapply the gags
                if(e.UpdateType == UpdateType.DisableRestraintSet && _characterHandler.playerChar._allowRestraintSetAutoEquip) {
                    try{
                        GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Restraint Set Disable, reverting to automation");
                        // revert based on our setting
                        if(_characterHandler.playerChar._revertStyle == RevertStyle.ToAutomationOnly)
                        {
                            // if we want to just revert to automation, then do just that.
                            await _Interop.GlamourerRevertCharacterToAutomation(_onFrameworkService._address);
                        }
                        if(_characterHandler.playerChar._revertStyle == RevertStyle.ToGameOnly)
                        {
                            // if we want to always revert to game, then do just that
                            await _Interop.GlamourerRevertCharacter(_onFrameworkService._address);
                        }
                        if(_characterHandler.playerChar._revertStyle == RevertStyle.ToGameThenAutomation)
                        {
                            // finally, if we want to revert to the game, then to any automation for this class after, then do just that
                            await _Interop.GlamourerRevertCharacter(_onFrameworkService._address);
                            await _Interop.GlamourerRevertCharacterToAutomation(_onFrameworkService._address);
                        }
                        // dont know how to tell if it was successful, so we will just assume it was
                    } catch (Exception) {
                        GagSpeak.Log.Error($"Error reverting glamourer to automation");
                    }
                    // now reapply the gags
                    if(_characterHandler.playerChar._allowItemAutoEquip) {
                        await ApplyGagItemsToCachedCharacterData();
                    }
                }
                if(e.UpdateType == UpdateType.GagEquipped && _characterHandler.playerChar._allowItemAutoEquip) {
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Gag Equipped");
                    if(e.GagType != "None") {
                        await EquipWithSetItem(e.GagType, e.AssignerName);
                    }
                    // otherwise, do the same as the unequip function
                    else {
                        GagSpeak.Log.Debug($"[GlamourEventFired]: GagType is None, not setting item.");
                        // if it is none, then do the same as the unequip function.
                        var gagType = Enum.GetValues(typeof(GagList.GagType))
                                        .Cast<GagList.GagType>()
                                        .First(g => g.GetGagAlias() == e.GagType);
                        // unequip it
                        await _Interop.SetItemToCharacterAsync(
                                _onFrameworkService._address,
                                Convert.ToByte(_gagStorageManager._gagEquipData[gagType]._slot),
                                ItemIdVars.NothingItem(_gagStorageManager._gagEquipData[gagType]._slot).Id.Id,
                                0,
                                0
                        );
                    }
                }
                // condition 4 --> it was an disable gag type event, we should take the gag type, and replace it with a nothing item
                if(e.UpdateType == UpdateType.GagUnEquipped && _characterHandler.playerChar._allowItemAutoEquip) {
                    // get the gagtype
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Gag UnEquipped");
                    var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().First(g => g.GetGagAlias() == e.GagType);
                    // this should replace it with nothing
                    await _Interop.SetItemToCharacterAsync(
                            _onFrameworkService._address,
                            Convert.ToByte(_gagStorageManager._gagEquipData[gagType]._slot),
                            ItemIdVars.NothingItem(_gagStorageManager._gagEquipData[gagType]._slot).Id.Id,
                            0,
                            0
                    );
                    // reapply any restraints hiding under them, if any
                    await ApplyRestrainSetToCachedCharacterData();
                }
                // condition 5 --> it was a gag refresh event, we should reapply all the gags
                if(e.UpdateType == UpdateType.UpdateGags) {
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Update Gags");
                    await ApplyGagItemsToCachedCharacterData();
                }

                // condition 6 --> it was a job change event, refresh all, but wait for the framework thread first
                if(e.UpdateType == UpdateType.JobChange) {
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Job Change");
                    await Task.Run(() => _onFrameworkService.RunOnFrameworkThread(UpdateCachedCharacterData));
                }

                // condition 7 --> it was a refresh all event, we should reapply all the gags and restraint sets
                if(e.UpdateType == UpdateType.RefreshAll || e.UpdateType == UpdateType.ZoneChange || e.UpdateType == UpdateType.Login) {
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Refresh All // Zone Change // Login // Job Change");
                    await Task.Run(() => _onFrameworkService.RunOnFrameworkThread(UpdateCachedCharacterData));
                }
                // condition 8 --> it was a safeword event, we should revert to the game, then to game and disable toys
                if(e.UpdateType == UpdateType.Safeword) {
                    GagSpeak.Log.Debug($"[GlamourEventFired]: Processing Safeword");
                    await _Interop.GlamourerRevertCharacter(_onFrameworkService._address);
                    // disable all toys
                }
            } catch (Exception ex) {
                GagSpeak.Log.Error($"[GlamourEventFired]: Error processing glamour event: {ex.Message}");
            } finally {
                _gagSpeakGlamourEvent.IsGagSpeakGlamourEventExecuting = false;
                finishedDrawingGlamChange = true;
                semaphore.Release();
            }
        } else {
            GagSpeak.Log.Debug($"[GlamourEventFired]: Wardrobe is disabled, so we wont be updating/applying any gag items");
        }
    }

    /// <summary> Updates the raw glamourer customization data with our gag items and restraint sets, if applicable </summary>
    public async Task UpdateCachedCharacterData() {
        // for privacy reasons, we must first make sure that our options for allowing such things are enabled.
        if(_characterHandler.playerChar._allowRestraintSetAutoEquip) {
            await ApplyRestrainSetToCachedCharacterData();
        } else {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Restraint Set Auto-Equip disabled, IGNORING");
        }

        if(_characterHandler.playerChar._allowItemAutoEquip) {
            await ApplyGagItemsToCachedCharacterData();
        } else {
            GagSpeak.Log.Debug($"[GlamourerChanged]: Item Auto-Equip disabled, IGNORING");
        }
    }

    /// <summary> Applies the only enabled restraint set to your character on update trigger. </summary>
    public async Task ApplyRestrainSetToCachedCharacterData() { // dummy placeholder line
        // Find the restraint set with the matching name
        List<Task> tasks = new List<Task>();
        foreach (var restraintSet in _restraintSetManager._restraintSets) {
            // If the restraint set is enabled
            if (restraintSet._enabled) {
                // Iterate over each EquipDrawData in the restraint set
                foreach (var pair in restraintSet._drawData) {
                    // see if the item is enabled or not (controls it's visibility)
                    if(pair.Value._isEnabled) {
                        // because it is enabled, we will still apply nothing items
                        tasks.Add(_Interop.SetItemToCharacterAsync(
                            _onFrameworkService._address, 
                            Convert.ToByte(pair.Key), // the key (EquipSlot)
                            pair.Value._gameItem.Id.Id, // Set this slot to nothing (naked)
                            pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                            0));
                    } else {
                        // Because it was disabled, we will treat it as an overlay, ignoring it if it is a nothing item
                        if (!pair.Value._gameItem.Equals(ItemIdVars.NothingItem(pair.Value._slot))) {
                            // Apply the EquipDrawData
                            GagSpeak.Log.Debug($"[ApplyRestrainSetToData] Calling on Helmet Placement {pair.Key}!");
                            tasks.Add(_Interop.SetItemToCharacterAsync(
                                _onFrameworkService._address, 
                                Convert.ToByte(pair.Key), // the key (EquipSlot)
                                pair.Value._gameItem.Id.Id, // The _drawData._gameItem.Id.Id
                                pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                                0));
                        } else {
                            GagSpeak.Log.Debug($"[ApplyRestrainSetToData] Skipping over {pair.Key}!");
                        }
                    }
                }
                // early exit, we only want to apply one restraint set
                GagSpeak.Log.Debug($"[ApplyRestrainSetToData]: Applying Restraint Set to Cached Character Data");
                await Task.WhenAll(tasks);
                return;
            }
        }
        GagSpeak.Log.Debug($"[ApplyRestrainSetToData]: No restraint sets are enabled, skipping!");
    }
    
    /// <summary> Applies the gag items to the cached character data. </summary>
    public async Task ApplyGagItemsToCachedCharacterData() {
        GagSpeak.Log.Debug($"[ApplyGagItems]: Applying Gag Items to Cached Character Data");
        // temporary code until glamourer update's its IPC changes
        await EquipWithSetItem(_characterHandler.playerChar._selectedGagTypes[0], "self");
        await EquipWithSetItem(_characterHandler.playerChar._selectedGagTypes[1], "self");
        await EquipWithSetItem(_characterHandler.playerChar._selectedGagTypes[2], "self");
    }

    public async Task EquipWithSetItem(string gagName, string assignerName = "") {
        // if the gagName is none, then just dont set it and return
        if(gagName == "None") {
            GagSpeak.Log.Debug($"[OnGagEquippedEvent]: GagLayer Not Equipped.");
            return;
        }
        // Get the gagtype (enum) where it's alias matches the gagName
        var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().First(g => g.GetGagAlias() == gagName);
        // See if the GagType is in our dictionary & that the gagName is not "None" (because .First() would make gagType==BallGag when gagName==None)
        if(_gagStorageManager._gagEquipData[gagType]._isEnabled == false) {
            GagSpeak.Log.Debug($"[Interop - SetItem]: GagType {gagName} is not enabled, so not setting item.");
            return;
        }
        // otherwise let's do the rest of the stuff
        if(assignerName == "self") _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, "self");
        if(assignerName != "self") _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, assignerName); 

        // see if assigner is valid
        if(ValidGagAssignerUser(gagName, _gagStorageManager._gagEquipData[gagType])) {
            try {
                await _Interop.SetItemToCharacterAsync(_onFrameworkService._address, 
                                                       Convert.ToByte(_gagStorageManager._gagEquipData[gagType]._slot),
                                                       _gagStorageManager._gagEquipData[gagType]._gameItem.Id.Id,
                                                       _gagStorageManager._gagEquipData[gagType]._gameStain.Id,
                                                       0
                );
                GagSpeak.Log.Debug($"[Interop - SetItem]: Set item {_gagStorageManager._gagEquipData[gagType]._gameItem} to slot {_gagStorageManager._gagEquipData[gagType]._slot} for gag {gagName}");
            }
            catch (TargetInvocationException ex) {
                GagSpeak.Log.Error($"[Interop - SetItem]: Error setting item: {ex.InnerException}");
            } 
        } else {
            GagSpeak.Log.Debug($"[Interop - SetItem]: Assigner {assignerName} is not valid, so not setting item.");
        }
    }

    /// <summary> A Helper function that will return true if ItemAutoEquip should occur based on the assigner name, or if it shouldnt. </summary>
    public bool ValidGagAssignerUser(string gagName, EquipDrawData equipDrawData) {
        // next, we need to see if the gag is being equipped.
        if(equipDrawData._wasEquippedBy == "self")
        {
            GagSpeak.Log.Debug($"[ValidGagAssignerUser]: GagType {gagName} is being equipped by yourself, abiding by your config settings for Item Auto-Equip.");
            return true;
        }
        // if we hit here, it was an assignerName
        string tempAssignerName = equipDrawData._wasEquippedBy;
        var words = tempAssignerName.Split(' ');
        tempAssignerName = string.Join(" ", words.Take(2)); // should only take the first two words
        // if the name matches anyone in the Whitelist:
        if(_characterHandler.whitelistChars.Any(w => w._name == tempAssignerName)) {
            // if the _yourStatusToThem is pet or slave, and the _theirStatusToYou is Mistress, then we can equip the gag.
            int playerIdx = _characterHandler.GetWhitelistIndex(tempAssignerName);
            if(_characterHandler.whitelistChars[playerIdx].IsRoleLeanSubmissive(_characterHandler.whitelistChars[playerIdx]._yourStatusToThem) 
            && _characterHandler.whitelistChars[playerIdx].IsRoleLeanDominant(_characterHandler.whitelistChars[playerIdx]._theirStatusToYou))
            {
                GagSpeak.Log.Debug($"[ValidGagAssignerUser]: You are a pet/slave to the gag assigner, and {tempAssignerName} is your Mistress. Because this two way relationship is established, allowing Item Auto-Eqiup.");
                return true;
            } 
            else
            {
                GagSpeak.Log.Debug($"[ValidGagAssignerUser]: {tempAssignerName} is not someone you are a pet or slave to, nor are they defined as your Mistress. Thus, Item Auto-Equip being disabled for this gag.");
                return false;
            }
        }
        else
        {
            GagSpeak.Log.Debug($"[ValidGagAssignerUser]: GagType {gagName} is being equipped by {tempAssignerName}, but they are not on your whitelist, so we are not doing Item-AutoEquip.");
            return false;
        }
    }
}