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
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using GagSpeak.Services;
using GagSpeak.Hardcore;
using Glamourer.Api.Helpers;
using Glamourer.Api.Enums;
using Glamourer.Api.IpcSubscribers;

namespace GagSpeak.Interop;

public class GlamourerFunctions : IDisposable
{
    public static bool disableGlamChangeEvent { get; set; } = false;           // disables the glam change event
    public static bool finishedDrawingGlamChange { get; set; } = false;        // disables the glamourer
    private readonly    GagStorageManager               _gagStorageManager;     // the gag storage manager
    private readonly    RestraintSetManager             _restraintSetManager;   // the restraint set manager
    private readonly    HardcoreManager                 _hardcoreManager;       // the hardcore manager
    private readonly    CharacterHandler                _characterHandler;      // the character handler for managing player and whitelist info
    private readonly    GlamourerService                _Interop;               // the glamourer interop class
    private readonly    IFramework                      _framework;             // the framework for running tasks on the main thread
    private readonly    GagSpeakGlamourEvent            _gagSpeakGlamourEvent;  // for whenever glamourer changes
    private readonly    OnFrameworkService              _onFrameworkService;    // character data updates/helpers (framework based)
    private             CancellationTokenSource         _cts;
    public  readonly    EventSubscriber<nint, StateChangeType> _StateChangedSubscriber;// for listening to state changes

    public GlamourerFunctions(DalamudPluginInterface pi, GagStorageManager gagStorageManager, RestraintSetManager restraintSetManager,
    CharacterHandler characterHandler, GlamourerService GlamourerService, IFramework framework,
    GagSpeakGlamourEvent gagSpeakGlamourEvent, OnFrameworkService onFrameworkService, HardcoreManager hardcoreManager) {
        // initialize the glamourer interop class
        _gagStorageManager = gagStorageManager;
        _restraintSetManager = restraintSetManager;
        _hardcoreManager = hardcoreManager;
        _characterHandler = characterHandler;
        _Interop = GlamourerService;
        _framework = framework;
        _gagSpeakGlamourEvent = gagSpeakGlamourEvent;
        _onFrameworkService = onFrameworkService;
        // initialize delegate for the glamourer changed event
        _cts = new CancellationTokenSource(); // for handling gearset changes
        
        // also subscribe to the state changed event so we know whenever they try to change an outfit
        _StateChangedSubscriber = StateChangedWithType.Subscriber(pi, GlamourerChanged);
        _StateChangedSubscriber.Enable();
        _gagSpeakGlamourEvent.GlamourEventFired += GlamourEventFired;    // to know when we update any setting that should trigger a particular glamour refresh.
        _framework.Update += FrameworkUpdate;                            // to know when we should process the last glamour data
        GSLogger.LogType.Information($"[GlamourerService]: GlamourerFunctions initialized!");
    }

    public void Dispose() {
        GSLogger.LogType.Information($"[GlamourerService]: Disposing of GlamourerService");
        // unsubscribe from the state changed event
        _StateChangedSubscriber.Disable();
        _StateChangedSubscriber.Dispose();
        // unsub from our other events.
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
                GSLogger.LogType.Information($"[FrameworkUpdate] Re-Allowing Glamour Change Event");
            }
        } 
    }

    private void GlamourerChanged(nint address, StateChangeType type) {
        // make sure it is coming from our character, otherwise ignore.
        if (address != _onFrameworkService._address)
        {
            GSLogger.LogType.Verbose($"[GlamourerChanged]: Change not from Character, IGNORING");
            return;
        }
        
        // make sure we have wardrobe enabled, otherwise ignore
        if(!_characterHandler.playerChar._enableWardrobe)
        {
            GSLogger.LogType.Verbose($"[GlamourerChanged]: Wardrobe is disabled, so we wont be updating/applying any gag items");
            return;
        }
        
        // make sure we are not already processing a glamour event
        if(_gagSpeakGlamourEvent.IsGagSpeakGlamourEventExecuting || disableGlamChangeEvent)
        {
            GSLogger.LogType.Verbose($"[GlamourerChanged]: Blocked due to request variables");
            return;
        }
        
        // The StateChangeType is a type we want to perform a reapply on
        if(type == StateChangeType.Design
        || type == StateChangeType.Reapply
        || type == StateChangeType.Reset
        || type == StateChangeType.Equip
        || type == StateChangeType.Stain 
        || type == StateChangeType.Weapon)
        {
            GSLogger.LogType.Verbose($"[GlamourerChanged]: StateChangeType is {(StateChangeType)type}");
            _gagSpeakGlamourEvent.Invoke(UpdateType.RefreshAll);
        }
        else // it is not a type we care about, so ignore
        {
            GSLogger.LogType.Verbose($"[GlamourerChanged]: GlamourerChanged event was not a type we care about, so skipping (Type was: {(StateChangeType)type})");
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
            GSLogger.LogType.Information(" "+ e.UpdateType.ToString().ToUpper()+" GLAMOUR EVENT FIRED ");
            // conditionals:
            try{
                // condition 1 --> It was an update restraint set event. In this case, we should recall the restraint set applier
                if(e.UpdateType == UpdateType.UpdateRestraintSet && _characterHandler.playerChar._allowRestraintSetAutoEquip)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Restraint Set Update");
                    await ApplyRestrainSetToCachedCharacterData(); // correct
                    // if any of our gags are not none
                    if(_characterHandler.playerChar._allowItemAutoEquip && _characterHandler.playerChar._selectedGagTypes.Any(g => g != "None")) {
                        await ApplyGagItemsToCachedCharacterData();
                    }
                    // update our blindfold
                    if( _hardcoreManager.IsBlindfoldedForAny(out int enabledIdx, out string playerWhoBlindfoldedYou)) {
                        await EquipBlindfold(enabledIdx);
                    }
                }

                // condition 2 --> it was an disable restraint set event, we should revert back to automation, but then reapply the gags
                if(e.UpdateType == UpdateType.DisableRestraintSet && _characterHandler.playerChar._allowRestraintSetAutoEquip)
                {
                    try{ 
                        // now perform a revert based on our customization option
                        switch(_characterHandler.playerChar._revertStyle) {
                            case RevertStyle.ToGameOnly:
                                await _Interop.GlamourerRevertCharacter(_onFrameworkService._address);
                                break;
                            case RevertStyle.ToAutomationOnly:
                                await _Interop.GlamourerRevertCharacterToAutomation(_onFrameworkService._address);
                                break;
                            case RevertStyle.ToGameThenAutomation:
                                await _Interop.GlamourerRevertCharacter(_onFrameworkService._address);
                                await _Interop.GlamourerRevertCharacterToAutomation(_onFrameworkService._address);
                                break;
                        }
                        // dont know how to tell if it was successful, so we will just assume it was
                    }
                    catch (Exception) {
                        GSLogger.LogType.Error($"Error reverting glamourer to automation");
                    }
                    // now reapply the gags
                    if(_characterHandler.playerChar._allowItemAutoEquip) {
                        GSLogger.LogType.Debug($"[GlamourEventFired]: Reapplying gags");
                        await ApplyGagItemsToCachedCharacterData();
                        GSLogger.LogType.Debug($"[GlamourEventFired]: Reapplying blindfold");
                        if( _hardcoreManager.IsBlindfoldedForAny(out int enabledIdx, out string playerWhoBlindfoldedYou)) {
                            await EquipBlindfold(enabledIdx);
                        } else {
                            GSLogger.LogType.Debug($"[GlamourEventFired]: Player was not blindfolded, IGNORING");
                        }
                    }
                }

                // condition 3 --> it was an equip gag type event, we should take the gag type, and replace it with the item
                if(e.UpdateType == UpdateType.GagEquipped && _characterHandler.playerChar._allowItemAutoEquip)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Gag Equipped");
                    if(e.GagType != "None") {
                        await EquipWithSetItem(e.GagType, e.AssignerName);
                    }
                    // otherwise, do the same as the unequip function
                    else {
                        GSLogger.LogType.Debug($"[GlamourEventFired]: GagType is None, not setting item.");
                        // if it is none, then do the same as the unequip function.
                        var gagType = Enum.GetValues(typeof(GagList.GagType))
                                        .Cast<GagList.GagType>()
                                        .First(g => g.GetGagAlias() == e.GagType);
                        // unequip it
                        await _Interop.SetItemToCharacterAsync(
                                _onFrameworkService._address,
                                (Glamourer.Api.Enums.ApiEquipSlot)_gagStorageManager._gagEquipData[gagType]._slot,
                                ItemIdVars.NothingItem(_gagStorageManager._gagEquipData[gagType]._slot).Id.Id,
                                0,
                                0
                        );
                    }
                }
                
                // condition 4 --> it was an disable gag type event, we should take the gag type, and replace it with a nothing item
                if(e.UpdateType == UpdateType.GagUnEquipped && _characterHandler.playerChar._allowItemAutoEquip)
                {
                    // get the gagtype
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Gag UnEquipped");
                    var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().First(g => g.GetGagAlias() == e.GagType);
                    // this should replace it with nothing
                    await _Interop.SetItemToCharacterAsync(
                            _onFrameworkService._address,
                            (Glamourer.Api.Enums.ApiEquipSlot)_gagStorageManager._gagEquipData[gagType]._slot,
                            ItemIdVars.NothingItem(_gagStorageManager._gagEquipData[gagType]._slot).Id.Id,
                            0,
                            0
                    );
                    // reapply any restraints hiding under them, if any
                    await ApplyRestrainSetToCachedCharacterData();
                    // update blindfold
                    if( _hardcoreManager.IsBlindfoldedForAny(out int enabledIdx, out string playerWhoBlindfoldedYou)) {
                        await EquipBlindfold(enabledIdx);
                    } else {
                        GSLogger.LogType.Debug($"[GlamourEventFired]: Player was not blindfolded, IGNORING");
                    }
                }

                // condition 5 --> it was a gag refresh event, we should reapply all the gags
                if(e.UpdateType == UpdateType.UpdateGags)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Update Gags");
                    await ApplyGagItemsToCachedCharacterData();
                }

                // condition 6 --> it was a job change event, refresh all, but wait for the framework thread first
                if(e.UpdateType == UpdateType.JobChange)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Job Change");
                    await Task.Run(() => _onFrameworkService.RunOnFrameworkThread(UpdateCachedCharacterData));
                }

                // condition 7 --> it was a refresh all event, we should reapply all the gags and restraint sets
                if(e.UpdateType == UpdateType.RefreshAll || e.UpdateType == UpdateType.ZoneChange || e.UpdateType == UpdateType.Login)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Refresh All // Zone Change // Login // Job Change");
                    // apply all the gags and restraint sets
                    await Task.Run(() => _onFrameworkService.RunOnFrameworkThread(UpdateCachedCharacterData));
                }

                // condition 8 --> it was a safeword event, we should revert to the game, then to game and disable toys
                if(e.UpdateType == UpdateType.Safeword)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Safeword");
                    await _Interop.GlamourerRevertCharacter(_onFrameworkService._address);
                    // this might be blocking disable restraint sets so look into.
                }

                // condition 9 -- > it was a blindfold equipped event, we should apply the blindfold
                if(e.UpdateType == UpdateType.BlindfoldEquipped)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Blindfold Equipped");
                    // get the index of the person who equipped it onto you
                    if(AltCharHelpers.IsPlayerInWhitelist(e.AssignerName, out int whitelistCharIdx))
                    {
                        await EquipBlindfold(whitelistCharIdx);
                    } 
                    else
                    {
                        GSLogger.LogType.Debug($"[GlamourEventFired]: Assigner {e.AssignerName} is not on your whitelist, so not setting item.");
                    }
                }

                // condition 10 -- > it was a blindfold unequipped event, we should remove the blindfold
                if(e.UpdateType == UpdateType.BlindfoldUnEquipped)
                {
                    GSLogger.LogType.Debug($"[GlamourEventFired]: Processing Blindfold UnEquipped");
                    // get the index of the person who equipped it onto you
                    if(AltCharHelpers.IsPlayerInWhitelist(e.AssignerName, out int whitelistCharIdx))
                    {
                        await UnequipBlindfold(whitelistCharIdx);
                    } 
                    else
                    {
                        GSLogger.LogType.Debug($"[GlamourEventFired]: Assigner {e.AssignerName} is not on your whitelist, so not setting item.");
                    }
                }
            } catch (Exception ex) {
                GSLogger.LogType.Error($"[GlamourEventFired]: Error processing glamour event: {ex}");
            } finally {
                _gagSpeakGlamourEvent.IsGagSpeakGlamourEventExecuting = false;
                finishedDrawingGlamChange = true;
                semaphore.Release();
            }
        } else {
            GSLogger.LogType.Debug($"[GlamourEventFired]: Wardrobe is disabled, so we wont be updating/applying any gag items");
        }
    }
#region Task Methods for Glamour Updates
    public async Task EquipBlindfold(int idxOfBlindfold) {
        GSLogger.LogType.Debug($"[GlamourEventFired]: Found index {idxOfBlindfold} for blindfold");
        // attempt to equip the blindfold to the player
        await _Interop.SetItemToCharacterAsync(
            _onFrameworkService._address,
            (Glamourer.Api.Enums.ApiEquipSlot)_hardcoreManager._perPlayerConfigs[idxOfBlindfold]._blindfoldItem._slot,
            _hardcoreManager._perPlayerConfigs[idxOfBlindfold]._blindfoldItem._gameItem.Id.Id, // The _drawData._gameItem.Id.Id
            _hardcoreManager._perPlayerConfigs[idxOfBlindfold]._blindfoldItem._gameStain.Id, // The _drawData._gameStain.Id
            0
        );
    }

    public async Task UnequipBlindfold(int idxOfBlindfold) {
        GSLogger.LogType.Debug($"[GlamourEventFired]: Found index {idxOfBlindfold} for blindfold");
        // attempt to unequip the blindfold from the player
        await _Interop.SetItemToCharacterAsync(
            _onFrameworkService._address,
            (Glamourer.Api.Enums.ApiEquipSlot)_hardcoreManager._perPlayerConfigs[idxOfBlindfold]._blindfoldItem._slot,
            ItemIdVars.NothingItem(_hardcoreManager._perPlayerConfigs[idxOfBlindfold]._blindfoldItem._slot).Id.Id, // The _drawData._gameItem.Id.Id
            0,
            0
        );
    }
    /// <summary> Updates the raw glamourer customization data with our gag items and restraint sets, if applicable </summary>
    public async Task UpdateCachedCharacterData() {
        // for privacy reasons, we must first make sure that our options for allowing such things are enabled.
        if(_characterHandler.playerChar._allowRestraintSetAutoEquip) {
            await ApplyRestrainSetToCachedCharacterData();
        } else {
            GSLogger.LogType.Debug($"[GlamourerChanged]: Restraint Set Auto-Equip disabled, IGNORING");
        }

        if(_characterHandler.playerChar._allowItemAutoEquip) {
            await ApplyGagItemsToCachedCharacterData();
        } else {
            GSLogger.LogType.Debug($"[GlamourerChanged]: Item Auto-Equip disabled, IGNORING");
        }

        // try and get the blindfolded status to see if we are blindfolded
        if( _hardcoreManager.IsBlindfoldedForAny(out int enabledIdx, out string playerWhoBlindfoldedYou)) {
            await EquipBlindfold(enabledIdx);
        } else {
            GSLogger.LogType.Debug($"[GlamourerChanged]: Player was not blindfolded, IGNORING");
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
                            (Glamourer.Api.Enums.ApiEquipSlot)pair.Key, // the key (EquipSlot)
                            pair.Value._gameItem.Id.Id, // Set this slot to nothing (naked)
                            pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                            0));
                    } else {
                        // Because it was disabled, we will treat it as an overlay, ignoring it if it is a nothing item
                        if (!pair.Value._gameItem.Equals(ItemIdVars.NothingItem(pair.Value._slot))) {
                            // Apply the EquipDrawData
                            GSLogger.LogType.Debug($"[ApplyRestrainSetToData] Calling on Helmet Placement {pair.Key}!");
                            tasks.Add(_Interop.SetItemToCharacterAsync(
                                _onFrameworkService._address, 
                                (Glamourer.Api.Enums.ApiEquipSlot)pair.Key, // the key (EquipSlot)
                                pair.Value._gameItem.Id.Id, // The _drawData._gameItem.Id.Id
                                pair.Value._gameStain.Id, // The _drawData._gameStain.Id
                                0));
                        } else {
                            GSLogger.LogType.Debug($"[ApplyRestrainSetToData] Skipping over {pair.Key}!");
                        }
                    }
                }
                // early exit, we only want to apply one restraint set
                GSLogger.LogType.Debug($"[ApplyRestrainSetToData]: Applying Restraint Set to Cached Character Data");
                await Task.WhenAll(tasks);
                return;
            }
        }
        GSLogger.LogType.Debug($"[ApplyRestrainSetToData]: No restraint sets are enabled, skipping!");
    }
    
    /// <summary> Applies the gag items to the cached character data. </summary>
    public async Task ApplyGagItemsToCachedCharacterData() {
        GSLogger.LogType.Debug($"[ApplyGagItems]: Applying Gag Items to Cached Character Data");
        // temporary code until glamourer update's its IPC changes
        await EquipWithSetItem(_characterHandler.playerChar._selectedGagTypes[0], "self");
        await EquipWithSetItem(_characterHandler.playerChar._selectedGagTypes[1], "self");
        await EquipWithSetItem(_characterHandler.playerChar._selectedGagTypes[2], "self");
    }

    public async Task EquipWithSetItem(string gagName, string assignerName = "") {
        // if the gagName is none, then just dont set it and return
        if(gagName == "None") {
            GSLogger.LogType.Debug($"[OnGagEquippedEvent]: GagLayer Not Equipped.");
            return;
        }
        // Get the gagtype (enum) where it's alias matches the gagName
        var gagType = Enum.GetValues(typeof(GagList.GagType)).Cast<GagList.GagType>().First(g => g.GetGagAlias() == gagName);
        // See if the GagType is in our dictionary & that the gagName is not "None" (because .First() would make gagType==BallGag when gagName==None)
        if(_gagStorageManager._gagEquipData[gagType]._isEnabled == false) {
            GSLogger.LogType.Debug($"[Interop - SetItem]: GagType {gagName} is not enabled, so not setting item.");
            return;
        }
        // otherwise let's do the rest of the stuff
        if(assignerName == "self") _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, "self");
        if(assignerName != "self") _gagStorageManager.ChangeGagDrawDataWasEquippedBy(gagType, assignerName); 

        // see if assigner is valid
        if(ValidGagAssignerUser(gagName, _gagStorageManager._gagEquipData[gagType])) {
            try {
                await _Interop.SetItemToCharacterAsync(_onFrameworkService._address, 
                                                       (Glamourer.Api.Enums.ApiEquipSlot)_gagStorageManager._gagEquipData[gagType]._slot,
                                                       _gagStorageManager._gagEquipData[gagType]._gameItem.Id.Id,
                                                       _gagStorageManager._gagEquipData[gagType]._gameStain.Id,
                                                       0
                );
                GSLogger.LogType.Debug($"[Interop - SetItem]: Set item {_gagStorageManager._gagEquipData[gagType]._gameItem} to slot {_gagStorageManager._gagEquipData[gagType]._slot} for gag {gagName}");
            }
            catch (TargetInvocationException ex) {
                GSLogger.LogType.Error($"[Interop - SetItem]: Error setting item: {ex.InnerException}");
            } 
        } else {
            GSLogger.LogType.Debug($"[Interop - SetItem]: Assigner {assignerName} is not valid, so not setting item.");
        }
    }

    /// <summary> A Helper function that will return true if ItemAutoEquip should occur based on the assigner name, or if it shouldnt. </summary>
    public bool ValidGagAssignerUser(string gagName, EquipDrawData equipDrawData) {
        // next, we need to see if the gag is being equipped.
        if(equipDrawData._wasEquippedBy == "self")
        {
            GSLogger.LogType.Debug($"[ValidGagAssignerUser]: GagType {gagName} is being equipped by yourself, abiding by your config settings for Item Auto-Equip.");
            return true;
        }
        // if we hit here, it was an assignerName
        string tempAssignerName = equipDrawData._wasEquippedBy;
        var words = tempAssignerName.Split(' ');
        tempAssignerName = string.Join(" ", words.Take(2)); // should only take the first two words
        // if the name matches anyone in the Whitelist:
        if(AltCharHelpers.IsPlayerInWhitelist(tempAssignerName, out int whitelistCharIdx)) {
            // if the _yourStatusToThem is pet or slave, and the _theirStatusToYou is Mistress, then we can equip the gag.
            if(_characterHandler.whitelistChars[whitelistCharIdx].IsRoleLeanSubmissive(_characterHandler.whitelistChars[whitelistCharIdx]._yourStatusToThem) 
            && _characterHandler.whitelistChars[whitelistCharIdx].IsRoleLeanDominant(_characterHandler.whitelistChars[whitelistCharIdx]._theirStatusToYou))
            {
                GSLogger.LogType.Debug($"[ValidGagAssignerUser]: You are a pet/slave to the gag assigner, and {tempAssignerName} is your Mistress. Because this two way relationship is established, allowing Item Auto-Eqiup.");
                return true;
            } 
            else
            {
                GSLogger.LogType.Debug($"[ValidGagAssignerUser]: {tempAssignerName} is not someone you are a pet or slave to, nor are they defined as your Mistress. Thus, Item Auto-Equip being disabled for this gag.");
                return false;
            }
        }
        else
        {
            GSLogger.LogType.Debug($"[ValidGagAssignerUser]: GagType {gagName} is being equipped by {tempAssignerName}, but they are not on your whitelist, so we are not doing Item-AutoEquip.");
            return false;
        }
    }
#endregion Task Methods for Glamour Updates
}