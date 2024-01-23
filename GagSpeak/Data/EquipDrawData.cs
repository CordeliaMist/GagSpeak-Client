using System;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;

namespace GagSpeak.Data;

public class EquipDrawData
{
    public bool                 _isEnabled;
    public bool                 _locked;
    public int                  _activeSlotListIdx;
    public EquipSlot            _slot;
    public EquipItem            _gameItem;
    public StainId              _gameStain;

    public EquipDrawData(EquipItem gameItem) {
        _isEnabled = false; // by default, dont enabled these
        _locked = false; // these arent locked by default, unless the gag is locked.
        _slot = EquipSlot.Head; // default slot, and the go-to equip slot for a gag
        _gameItem = gameItem; // default to nothing
        _gameStain = 0; // default to no stain
        _activeSlotListIdx = 0; // make active slot indexlist 0 aka helmet.
    }

    public void SetSlot(EquipSlot slot) {
        _slot = slot;
    }
    public void SetGameItem(EquipItem gameItem) {
        GagSpeak.Log.Debug($"[EquipDrawData] Changing equipment from {_gameItem} to {gameItem}");
        _gameItem = gameItem;
    }

    public void ResetGameItem() {
        _gameItem = ItemIdVars.NothingItem(_slot);
    }

    public void SetGameStain(StainId gameStain) {
        _gameStain = gameStain;
    }

    public void ResetGameStain() {
        _gameStain = 0;
    }
}