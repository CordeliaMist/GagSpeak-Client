using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using GagSpeak.Events;
using GagSpeak.Services;
using System.Runtime.InteropServices;
using Penumbra.GameData.Enums;
using System.Reflection.Metadata.Ecma335;
using Penumbra.String;

/// <summary> 
/// Stores all information about the player and helper functions for it here.
/// This object is constantly updated on each dalamud framework update.
/// </summary>
namespace GagSpeak.CharacterData;

/// <summary> Draw condition of our object character in the scene </summary>
public enum DrawCondition {
    None,
    DrawObjectZero,
    RenderFlags,
    ModelInSlotLoaded,
    ModelFilesInSlotLoaded
}

public class ClientUserInfo : IDisposable
{
    //============== Basic Class Assignment ===================//
    private readonly IChatGui _chat;
    private readonly IClientState _clientState;
    private readonly ICondition _condition;
    private readonly IDataManager _gameData;
    private readonly IFramework _framework;
    private readonly IObjectTable _objectTable;
    private readonly GagSpeakConfig _config;
    private readonly CharacterHandler _characterDataManager;
    private readonly JobChangedEvent _jobChangedEvent;
    private CancellationTokenSource? _clearCts = new(); // used for clearing the character data
    //============= Personal Variable Assignment =================//
    public IntPtr Address { get; private set; } // player address
    public IntPtr DrawObjectAddress { get; set; } // player draw object address
    private uint? ClassJobId = 0;  // ID of current job
    public string Name { get; private set; } = string.Empty; // player name
    public byte RaceId { get; private set; } // player race
    public byte Gender { get; private set; } // player gender
    public byte TribeId { get; private set; } // player tribe
    private byte[] CustomizeData { get; set; } = new byte[26]; // player customization data
    private byte[] EquipSlotData { get; set; } = new byte[40]; // player equip slot data
    private ushort[] MainHandData { get; set; } = new ushort[3]; // player main hand data
    private ushort[] OffHandData { get; set; } = new ushort[3]; // player off hand data
    //============== Basic Variable Assignment For Helpers ===================//
    public bool _haltUpdateProcessing = false; // if we can update our character data each framework tick
    public bool _jobChanged = false; // for knowing if our job changed
    public  bool _canCheckForDrawing = true; // can we check for drawing?
    public int _ptrNullCounter = 0; // counter for how many times our pointer has been null
    public bool IsAnythingDrawing { get; private set; } = false; // lets us know if anything is drawing at all.
    private DateTime _delayedFrameworkUpdateCheck = DateTime.Now; // keeps track of how delayed our framework update is
    private string _lastGlobalBlockPlayer = string.Empty; // player name of the last global block
    private string _lastGlobalBlockReason = string.Empty; // reason for the last global block
    private bool _sentBetweenAreas = false; // if we sent a between areas message
    private ushort _lastZone = 0;
    private DateTime _lastLoggedTime = DateTime.Now; // last time we logged a message
    public bool IsZoning => _condition[ConditionFlag.BetweenAreas] || _condition[ConditionFlag.BetweenAreas51]; // if we are zoning
    public Lazy<Dictionary<ushort, string>> WorldData { get; private set; } // contains world data if we ever need it at any point

    public ClientUserInfo(IChatGui chat, IClientState clientState, ICondition condition, IDataManager gameData,
    IFramework framework, IObjectTable objectTable, GagSpeakConfig config, CharacterHandler characterDataManager,
    JobChangedEvent jobChangedEvent) {
        _chat = chat;
        _clientState = clientState;
        _condition = condition;
        _gameData = gameData;
        _framework = framework;
        _objectTable = objectTable;
        _config = config;
        _characterDataManager = characterDataManager;
        _jobChangedEvent = jobChangedEvent;
        // set variables that are unassigned
        Address = GetPlayerPointerAsync().GetAwaiter().GetResult();
        WorldData = new(() => {
            return gameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>(Dalamud.ClientLanguage.English)!
                .Where(w => w.IsPublic && !w.Name.RawData.IsEmpty)
                .ToDictionary(w => (ushort)w.RowId, w => w.Name.ToString());
        });

        // subscribe to the framework update event
        _framework.Update += FrameworkOnUpdate;
    }

    public void Dispose() {
        _framework.Update -= FrameworkOnUpdate;

    }

#region FrameworkUpdateFuncs
    /// <summary> The invokable framework function </summary>
    private void FrameworkOnUpdate(IFramework framework) => FrameworkOnUpdateInternal();

    /// <summary> 
    /// The main framework update function. 
    /// </summary>
    private unsafe void FrameworkOnUpdateInternal() {
        // if we are not logged in, or are dead, return
        if (_clientState.LocalPlayer?.IsDead ?? false) return;
        
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
            // early escape
            return;
        }


        // if we are between areas, but made it to this point, then it means we are back in the game
        if (_sentBetweenAreas) {
            GagSpeak.Log.Debug($"[ZoneSwitch]  Zone switch/Gpose end");
            // let user know on launch of their direct chat garbler is still enabled
            if (_characterDataManager.playerChar._directChatGarblerActive)
                _chat.PrintError("[Notice] Direct Chat Garbler is still enabled. A Friendly reminder encase you forgot <3");
            // update the between areas to false
            _sentBetweenAreas = false;
        }


        // Otherwise, reset the IsAnythingDrawing bool for the next framework update
        IsAnythingDrawing = false;
        // if we can check for drawing, check for drawing
        if(_canCheckForDrawing)
            CheckCharacterForDrawing();
        // check and update our object data
        try {
            CheckAndUpdateObject();
        } catch (Exception ex) {
            GagSpeak.Log.Error($"[FrameworkUpdate] Error during framework update of {ex}");
        }  
        

        // if we are not drawing anything and we have a global block player, reset the global block player (not sure what this does yet)
        if (!IsAnythingDrawing && !string.IsNullOrEmpty(_lastGlobalBlockPlayer)) {
            GagSpeak.Log.Debug($"Global draw block: END => {_lastGlobalBlockPlayer}");
            _lastGlobalBlockPlayer = string.Empty;
            _lastGlobalBlockReason = string.Empty;
            // if we have finished redrawing, then we can check for drawing again
            if(_config.finishedDrawingGlamChange){
                GagSpeak.Log.Debug($"[FrameworkUpdate] GagSpeak Glamour Updates Finished");
                if(_config.disableGlamChangeEvent) {
                    GagSpeak.Log.Debug($"[FrameworkUpdate] Disabling Glamour Change Event");
                    var lastLoggedOccurance = DateTime.Now;
                    if ((lastLoggedOccurance - _lastLoggedTime).TotalMilliseconds < 100) {
                        GagSpeak.Log.Debug($"[FrameworkUpdate] Last logged occurance was less than 100ms ago, returning");
                        _lastLoggedTime = lastLoggedOccurance;
                        _config.finishedDrawingGlamChange = false;
                        _config.disableGlamChangeEvent = false;
                        _jobChangedEvent.Invoke();
                    } else {
                        _lastLoggedTime = lastLoggedOccurance;
                        _config.finishedDrawingGlamChange = false;
                        _config.disableGlamChangeEvent = false;
                    }
                }
            } 
        }


        // if our job is changed, invoke a redraw
        if (_jobChanged) {
            _jobChangedEvent.Invoke();
        }
        

        // if the current time is less than the delayed framework update check + 1 second, return
        if (DateTime.Now < _delayedFrameworkUpdateCheck.AddSeconds(1)) return;
        // update the delayed framework update check to the current time
        _delayedFrameworkUpdateCheck = DateTime.Now;
    }
#endregion FrameworkUpdateFuncs
//=====================================================================================================================
//========================================== Object Helper Functions ==================================================
//=====================================================================================================================
#region ObjectHelperFuncs
    /// <summary> Waits for us to finish drawing. Note that this may cause crashes, so be sure to remove it if it does </summary>
    public async Task WaitWhileCharacterIsDrawing(int timeOut = 5000, CancellationToken? ct = null) {
        if (!_clientState.IsLoggedIn) return;
        GagSpeak.Log.Debug($"[CharaDataHelper]  Waiting for character to finish drawing");
        const int tick = 250;   // framework thread tick rate
        int curWaitTime = 0;    // current time since redraw began
        try {
            // while the cancellation token is not cancelled & wait time < timeout, wait
            while ((!ct?.IsCancellationRequested ?? true)
            && curWaitTime < timeOut
            && await IsBeingDrawnRunOnFrameworkAsync().ConfigureAwait(false)) {
                GagSpeak.Log.Debug($"[CharaDataHelper]  Waiting for character to finish drawing");
                // add 250 to the current wait time
                curWaitTime += tick;
                await Task.Delay(tick).ConfigureAwait(true);
            }
            // log that we finished drawing
            GagSpeak.Log.Debug($"[CharaDataHelper]  Finished drawing after {curWaitTime}ms");
        } catch (NullReferenceException ex) {
            GagSpeak.Log.Warning($"[CharaDataHelper] Error accessing player pointer, they do not exist anymore {ex}");
        } catch (AccessViolationException ex) {
            GagSpeak.Log.Warning($"[CharaDataHelper] Error accessing player pointer, they do not exist anymore {ex}");
        }
    }

    /// <summary> Returns if we are being drawn on the framework thread
    public async Task<bool> IsBeingDrawnRunOnFrameworkAsync() {
        return await RunOnFrameworkThread(IsBeingDrawn).ConfigureAwait(false);
    }

    /// <summary> Fetch a GameObject at spesified pointer </summary>
    public Dalamud.Game.ClientState.Objects.Types.GameObject? CreateGameObject(IntPtr reference) {
        EnsureIsOnFramework();
        return _objectTable.CreateObjectReference(reference);
    }

    /// <summary> Fetch a GameObject at spesified pointer on the framework thread) </summary>
    public async Task<Dalamud.Game.ClientState.Objects.Types.GameObject?> CreateGameObjectAsync(IntPtr reference) {
        return await RunOnFrameworkThread(() => _objectTable.CreateObjectReference(reference)).ConfigureAwait(false);
    }

    /// <summary> Fetch a draw object unsafely with curPtr </summary>
    private unsafe IntPtr GetDrawObjUnsafe(nint curPtr) {
        return (IntPtr)((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)curPtr)->DrawObject;
    }
    /// <summary> Makes sure we are in dalamuds framework thread. </summary>
    public void EnsureIsOnFramework() {
        if (!_framework.IsInFrameworkUpdateThread)
            throw new InvalidOperationException("Can only be run on Framework");
    }

    /// <summary> Get Character type Object from table </summary>
    public Dalamud.Game.ClientState.Objects.Types.Character? GetCharacterFromObjectTable() {
        EnsureIsOnFramework();
        var objTableObj = _objectTable[0]; // we will always be index 0.
        if (objTableObj!.ObjectKind != Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player) return null;
        return (Dalamud.Game.ClientState.Objects.Types.Character)objTableObj;
    }

    /// <summary> Sees if _clientState.LocalPlayer is not null and a valid player. </summary>
    public bool GetIsPlayerPresent() {
        EnsureIsOnFramework();
        return _clientState.LocalPlayer != null && _clientState.LocalPlayer.IsValid();
    }

    /// <summary> Sees if _clientState.LocalPlayer is not null and a valid player on the framework thread </summary>
    public async Task<bool> GetIsPlayerPresentAsync() {
        return await RunOnFrameworkThread(GetIsPlayerPresent).ConfigureAwait(false);
    }

    /// <summary> Gets the class Dalamud.Game.ClientState.Objects.SubKinds.PlayerCharacter </summary>
    public PlayerCharacter GetPlayerCharacter() {
        EnsureIsOnFramework();
        return _clientState.LocalPlayer!;
    }

    /// <summary> Gets the name of your PlayerCharacter. </summary>
    public string GetPlayerName() {
        EnsureIsOnFramework();
        return _clientState.LocalPlayer?.Name.ToString() ?? "";
    }

    /// <summary> Gets the name of your PlayerCharacter on the frameworkthread </summary>
    public async Task<string> GetPlayerNameAsync() {
        return await RunOnFrameworkThread(GetPlayerName).ConfigureAwait(false);
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

    /// <summary> Gets the ID of the home world of your character. </summary>
    public uint GetHomeWorldId() {
        EnsureIsOnFramework();
        return _clientState.LocalPlayer!.HomeWorld.Id;
    }

    /// <summary> Gets the ID of the current world of your character. </summary>
    public uint GetWorldId() {
        EnsureIsOnFramework();
        return _clientState.LocalPlayer!.CurrentWorld.Id;
    }

    /// <summary> return ID of characters home world on the framework thread </summary>
    public async Task<uint> GetHomeWorldIdAsync() {
        return await RunOnFrameworkThread(GetHomeWorldId).ConfigureAwait(false);
    }

    /// <summary> get ID your characters home world ID on the framework thread </summary>
    public async Task<uint> GetWorldIdAsync() {
        return await RunOnFrameworkThread(GetWorldId).ConfigureAwait(false);
    }

    /// <summary> See if the address of an object is currently present in the object table </summary>
    public unsafe bool IsGameObjectPresent(IntPtr key) {
        return _objectTable.Any(f => f.Address == key);
    }

    /// <summary> Check if a gameobject is present in the scene </summary>
    public bool IsObjectPresent(Dalamud.Game.ClientState.Objects.Types.GameObject? obj) {
        EnsureIsOnFramework();
        return obj != null && obj.IsValid();
    }

    /// <summary> Check if a gameobject is present in the scene from the framework thread </summary>
    public async Task<bool> IsObjectPresentAsync(Dalamud.Game.ClientState.Objects.Types.GameObject? obj) {
        return await RunOnFrameworkThread(() => IsObjectPresent(obj)).ConfigureAwait(false);
    }

    /// <summary> Invalidate our character object, setting all references to it to null / zero </summary>
    public void Invalidate() {
        Address = IntPtr.Zero;
        DrawObjectAddress = IntPtr.Zero;
        // Allows object to be updated again, can be added later,
        // Basically deals with things like zone switching and cutscene viewing
        _haltUpdateProcessing = false;
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
#endregion ObjectHelperFuncs
#region ObjectChangeChecks
    /// <summary> Clear the player object data after a delay </summary>
    private async Task ClearAsync(CancellationToken token) {
        GagSpeak.Log.Debug($"[Clear Async]  Running Clear Task");
        await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
        GagSpeak.Log.Debug($"[Clear Async]  Sending ClearCachedForObjectMessage");
        // null the cancellation token source
        _clearCts = null;
    }

    /// <summary> Return true or false based on if we are being drawn or not. </summary>
    private bool IsBeingDrawn() {
        // get the current pointer of the address
        var curPtr = Address;
        GagSpeak.Log.Information($"[CharaObjectHandler] IsBeingDrawn, CurPtr: {curPtr}, {this}, {curPtr.ToString()}");
        // if our pointer is zero, and we have been deemed null less than two framework ticks, return true
        if (curPtr == IntPtr.Zero && _ptrNullCounter < 2) {
            GagSpeak.Log.Information($"[CharaObjectHandler] IsBeingDrawn, CurPtr is ZERO, counter is {_ptrNullCounter}");
            _ptrNullCounter++;
            return true;
        }
        // if our current pointer is zero still, then throw an exception
        if (curPtr == IntPtr.Zero) {
            GagSpeak.Log.Information($"[CharaObjectHandler] IsBeingDrawn, CurPtr is ZERO, returning");
            Invalidate();
            throw new ArgumentNullException($"CurPtr for {this} turned ZERO");
        }
        // if we arent zero, AKA logged in, see if anything is drawing. If it is, return true
        if (IsAnythingDrawing) {
            return true;
        }

        // if we are not, then get the draw object pointer for our character object
        var drawObj = GetDrawObjUnsafe(curPtr);
        GagSpeak.Log.Information($"[CharaObjectHandler] IsBeingDrawn, DrawObjPtr: {drawObj.ToString()}");
        // Get the draw condition of the object and return it as a boolean for if it is of type none or not
        var isDrawn = IsBeingDrawnUnsafe(drawObj, curPtr);
        GagSpeak.Log.Information($"[CharaObjectHandler] IsBeingDrawn, Condition: {isDrawn}");
        return isDrawn != DrawCondition.None;
    }

    /// <summary> Unsafe call to know if object is being drawn or not. Only called from the safe method, so should be good </summary>
    private unsafe DrawCondition IsBeingDrawnUnsafe(IntPtr drawObj, IntPtr curPtr) {
        // create a boolean to know if the draw object is zero
        var drawObjZero = drawObj == IntPtr.Zero;
        // if it is, return DrawObjectZero
        if (drawObjZero) return DrawCondition.DrawObjectZero;
        // otherwise, get the object kind and its render flags
        var renderFlags = (((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)curPtr)->RenderFlags) != 0x0;
        if (renderFlags) return DrawCondition.RenderFlags;
        // it will always be us, so we can get the model type
        var modelInSlotLoaded = (((CharacterBase*)drawObj)->HasModelInSlotLoaded != 0);
        if (modelInSlotLoaded) return DrawCondition.ModelInSlotLoaded;
        // if it is not, then get the model files in slot loaded
        var modelFilesInSlotLoaded = (((CharacterBase*)drawObj)->HasModelFilesInSlotLoaded != 0);
        if (modelFilesInSlotLoaded) return DrawCondition.ModelFilesInSlotLoaded;
        return DrawCondition.None;
    }

    /// <summary> 
    /// Compare player's customization data to new data & return true if anything was changed
    /// The process and return conditions for this is the same in the equipment data
    /// </summary>
    private unsafe bool CompareAndUpdateCustomizeData(byte* customizeData) {
        bool hasChanges = false; 
        // loop through our customization data
        for (int i = 0; i < CustomizeData.Length; i++) {
            // read the byte at the address of the customization data and store it in a variable
            var data = Marshal.ReadByte((IntPtr)customizeData, i);
            // if the data is different than the data stored, then we have a change
            if (CustomizeData[i] != data) {
                GagSpeak.Log.Debug($"[CompareAndUpdateCustomize]  has changes! {EquipSlotData[i]} to {data}");
                EquipSlotData[i] = data;
                CustomizeData[i] = data;
                hasChanges = true;
            }
        }
        return hasChanges;
    }

    /// <summary> (CharacterCustomizationCompatMAYBE?) : 
    /// Compare player's customization data to new data & return true if anything was changed
    /// The process and return conditions for this is the same in the equipment data
    /// </summary>
    private unsafe bool CompareAndUpdateEquipByteData(EquipmentModelId equipSlotData) {
        byte* equipSlotDataByteFriendly = (byte*)&equipSlotData;
        return CompareAndUpdateEquipByteData(equipSlotDataByteFriendly);
    }

    /// <summary> 
    /// Compare player's customization data to new data & return true if anything was changed
    /// The process and return conditions for this is the same in the equipment data
    /// </summary>
    private unsafe bool CompareAndUpdateEquipByteData(byte* equipSlotData) {
        bool hasChanges = false;
        for (int i = 0; i < EquipSlotData.Length; i++) {
            var data = Marshal.ReadByte((IntPtr)equipSlotData, i);
            if (EquipSlotData[i] != data) {
                GagSpeak.Log.Debug($"[CompareAndUpdateEquip]  has changes! {EquipSlotData[i]} to {data}");
                EquipSlotData[i] = data;
                hasChanges = true;
            }
        }
        return hasChanges;
    }

    /// <summary> Compare player's main hand data to new data & return true if anything was changed </summary>
    private unsafe bool CompareAndUpdateMainHand(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Weapon* weapon) {
        if ((nint)weapon == nint.Zero) return false;
        bool hasChanges = false;
        hasChanges |= weapon->ModelSetId != MainHandData[0];
        MainHandData[0] = weapon->ModelSetId;
        hasChanges |= weapon->Variant != MainHandData[1];
        MainHandData[1] = weapon->Variant;
        hasChanges |= weapon->SecondaryId != MainHandData[2];
        MainHandData[2] = weapon->SecondaryId;
        if(hasChanges) {
            GagSpeak.Log.Debug($"[CompareAndUpdateMainHand]  || Changed from modelsetid: "+
            $"{weapon->ModelSetId} to {MainHandData[0]} || variant: {weapon->Variant} to "+
            $"{MainHandData[1]} || secondaryid: {weapon->SecondaryId} to {MainHandData[2]}");
        }
        return hasChanges;
    }

    /// <summary> Compare player's off hand data to new data & return true if anything was changed </summary>
    private unsafe bool CompareAndUpdateOffHand(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Weapon* weapon) {
        if ((nint)weapon == nint.Zero) return false;
        bool hasChanges = false;
        hasChanges |= weapon->ModelSetId != OffHandData[0];
        OffHandData[0] = weapon->ModelSetId;
        hasChanges |= weapon->Variant != OffHandData[1];
        OffHandData[1] = weapon->Variant;
        hasChanges |= weapon->SecondaryId != OffHandData[2];
        OffHandData[2] = weapon->SecondaryId;
        if(hasChanges) {
            GagSpeak.Log.Debug($"[CompareAndUpdateOffHand]  || Changed from modelsetid: "+
            $"{weapon->ModelSetId} to {MainHandData[0]} || variant: {weapon->Variant} to "+
            $"{MainHandData[1]} || secondaryid: {weapon->SecondaryId} to {MainHandData[2]}");
        }
        return hasChanges;
    }
#endregion ObjectChangeChecks
#region ObjectUpdateFuncs
    /// <summary> Check our player object and update its variables on each framework thread </summary>
private unsafe void CheckAndUpdateObject() {
        // store our previous address and draw object address
        var prevAddr = Address;
        var prevDrawObj = DrawObjectAddress;
        // get a new address and store it
        Address = GetPlayerPointerAsync().GetAwaiter().GetResult();
        // if the address is not zero, then we can get the draw object address
        if (Address != IntPtr.Zero) {
            // get the drawobject address and store it
            _ptrNullCounter = 0;
            var drawObjAddr = (IntPtr)((FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)Address)->DrawObject;
            DrawObjectAddress = drawObjAddr;
        } else {
            // otherwise, we have an invalid address, so we set the draw object address to zero
            DrawObjectAddress = IntPtr.Zero;
        }

        // if we asked to halt processing, then stop updating
        if (_haltUpdateProcessing) return;

        // determine if the drawobject address and object address have changed
        bool drawObjDiff = DrawObjectAddress != prevDrawObj;
        bool addrDiff = Address != prevAddr;

        // if the new Address and DrawObject address are not zero,
        // then we passed the validation checks above and can update the object
        if (Address != IntPtr.Zero && DrawObjectAddress != IntPtr.Zero) {
            // if our cancel token is not null, then we cancel it and dispose it
            if (_clearCts != null) {
                GagSpeak.Log.Debug($"[CheckAndUpdateObject] Cancelling Clear Task {this}");
                _clearCts.Cancel();
                _clearCts.Dispose();
                _clearCts = null;
            }
            // create a chara variable with the character's address
            var chara = (Character*)Address;
            // get the name of the character
            var name = new ByteString(chara->GameObject.Name).ToString();
            // see if they had a name change
            bool nameChange = !string.Equals(name, Name, StringComparison.Ordinal);
            if (nameChange) {
                // if they did, update the name
                Name = name;
            }
            // assume there is no difference in the equipment.
            bool equipDiff = false;
            
            // if the draw object address is a character base and the model type is human, then we can get the class job
            if (((DrawObject*)DrawObjectAddress)->Object.GetObjectType() == FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType.CharacterBase
            && ((CharacterBase*)DrawObjectAddress)->GetModelType() == CharacterBase.ModelType.Human) {
                // get the class job of the character
                var classJob = chara->CharacterData.ClassJob;
                // if the class job is different than the one stored, then we have a class job change (CRITICAL TO UPDATING PROPERLY)
                if (classJob != ClassJobId) {
                    GagSpeak.Log.Information($"[CHARA HANDLER UPDATE] classjob changed from {ClassJobId} to {classJob}");
                    // update the stored class job
                    ClassJobId = classJob;
                    // invoke jobChangedEvent to call the glamourerRevert
                    _jobChanged = true;
                } else {
                    _jobChanged = false;
                }
            }
        }
        /*          This is not nessisary unless glamourer's IPC can fix

                // compare and update the equip byte data at the drawObject address for the head slot spesifically. (idk why?)
                equipDiff = CompareAndUpdateEquipByteData((byte*)&((Human*)DrawObjectAddress)->Head);
                // store refernce variables for the data in the main hand and off hand
                ref var mh = ref chara->DrawWeapon(WeaponSlot.MainHand);
                ref var oh = ref chara->DrawWeapon(WeaponSlot.OffHand);
                // compare and update the main hand and off hand data if they are different.
                equipDiff |= CompareAndUpdateMainHand((Weapon*)mh.DrawObject);
                equipDiff |= CompareAndUpdateOffHand((Weapon*)oh.DrawObject);
            } // Otherwise, if the drawObject is not a character base or the model type is not human, then we can get the equip byte data from the game object 
            else {
                equipDiff = CompareAndUpdateEquipByteData((byte*)&chara->DrawHead);
                if (equipDiff)
                    GagSpeak.Log.Information($"[CheckAndUpdateObject] checking  equip data as game draw obj, result: {equipDiff}");
            }

            // assume there is no difference in the customization data (not that it matters for us anyways)
            bool customizeDiff = false;
            // if the draw object address is a character base and the model type is human, then we can get the customization data
            if (((DrawObject*)DrawObjectAddress)->Object.GetObjectType() == FFXIVClientStructs.FFXIV.Client.Graphics.Scene.ObjectType.CharacterBase
            && ((CharacterBase*)DrawObjectAddress)->GetModelType() == CharacterBase.ModelType.Human) {
                // get the customization  (this will become very irrelavent later.)
                var gender = ((Human*)DrawObjectAddress)->Customize.Sex;
                var raceId = ((Human*)DrawObjectAddress)->Customize.Race;
                var tribeId = ((Human*)DrawObjectAddress)->Customize.Clan;
                // if our gender, race, or tribe was updated, then we must update our stored variables
                if (gender != Gender || raceId != RaceId || tribeId != TribeId) {
                    Gender = gender;
                    RaceId = raceId;
                    TribeId = tribeId;
                }
                // iterate through all the customization data and update it if any of it is different
                customizeDiff = CompareAndUpdateCustomizeData(((Human*)DrawObjectAddress)->Customize.Data);
            } else {
                // if our character is not a human, then we can get the customization data from the game object instead
                customizeDiff = CompareAndUpdateCustomizeData(chara->DrawCustomizeData);
                // if there was any customization data difference, log it
                if (customizeDiff)
                   GagSpeak.Log.Information($"[CheckAndUpdateObject] Checking  customize data from game obj, result: {equipDiff}");
            }
            // if we had any different of any type, let the log know
            if (addrDiff || drawObjDiff || equipDiff || customizeDiff || nameChange) {
                GagSpeak.Log.Information($"[CheckAndUpdateObject]  At least onne object changed this framework tick");
            }
        }
        */
        // otherwise, if either of our addresses were different, we will need to clear and generate a new cancellation token
        else if (addrDiff || drawObjDiff) {
            _clearCts?.Cancel();
            _clearCts?.Dispose();
            // create a new cancellation token source
            _clearCts = new();
            // create a new task that will run the clear async method
            var token = _clearCts.Token;
            _ = Task.Run(() => ClearAsync(token), token);
        }
    }
    
    /// <summary> Lets us know that our character is beginning to draw new changes, whenver our drawobject address updates </summary>
    private unsafe void CheckCharacterForDrawing() {
        // if we are not logged in, return
        if (!_clientState.IsLoggedIn || _clientState.LocalPlayer == null) return;
        // if we arent the player character, dont bother checking
        PlayerCharacter p = _clientState.LocalPlayer ?? throw new NullReferenceException("LocalPlayer is null");
        // if we are currently marked as not drawing anything, then look to see if we are.
        if (!IsAnythingDrawing) {
            // get the current pointer of the address
            var gameObj = (GameObject*)p.Address;
            // get the draw object pointer for our character object
            var drawObj = gameObj->DrawObject;
            // define our name and assume we are not drawing
            var playerName = p.Name.ToString();
            bool isDrawing = false;
            bool isDrawingChanged = false;
            // do various checks to see if we satisfy conditions to be drawing something.
            if ((nint)drawObj != IntPtr.Zero) {
                isDrawing = gameObj->RenderFlags == 0b100000000000;
                if (!isDrawing) {
                    isDrawing = ((CharacterBase*)drawObj)->HasModelInSlotLoaded != 0;
                    if (!isDrawing) {
                        isDrawing = ((CharacterBase*)drawObj)->HasModelFilesInSlotLoaded != 0;
                        if (isDrawing && !string.Equals(_lastGlobalBlockPlayer, playerName, StringComparison.Ordinal)
                        && !string.Equals(_lastGlobalBlockReason, "HasModelFilesInSlotLoaded", StringComparison.Ordinal)) {
                            _lastGlobalBlockPlayer = playerName;
                            _lastGlobalBlockReason = "HasModelFilesInSlotLoaded";
                            isDrawingChanged = true;
                        }
                    } else {
                        if (!string.Equals(_lastGlobalBlockPlayer, playerName, StringComparison.Ordinal)
                        && !string.Equals(_lastGlobalBlockReason, "HasModelInSlotLoaded", StringComparison.Ordinal)) {
                            _lastGlobalBlockPlayer = playerName;
                            _lastGlobalBlockReason = "HasModelInSlotLoaded";
                            isDrawingChanged = true;
                        }
                    }
                } else {
                    if (!string.Equals(_lastGlobalBlockPlayer, playerName, StringComparison.Ordinal)
                    && !string.Equals(_lastGlobalBlockReason, "RenderFlags", StringComparison.Ordinal)) {
                        _lastGlobalBlockPlayer = playerName;
                        _lastGlobalBlockReason = "RenderFlags";
                        isDrawingChanged = true;
                    }
                }
            }
            // if we are drawing, log it
            if (isDrawingChanged) {
                GagSpeak.Log.Debug($"Global draw block: START => {playerName} ({_lastGlobalBlockReason})");
            }
            // update IsAnythingDrawing variable
            IsAnythingDrawing |= isDrawing;
        }
    }
}
#endregion ObjectUpdateFuncs