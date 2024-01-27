using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;

namespace GagSpeak.Data;
public class EquipDrawData
{
    public bool                 _isEnabled;
    public string               _wasEquippedBy;
    public bool                 _locked;
    public int                  _activeSlotListIdx;
    public EquipSlot            _slot;
    public EquipItem            _gameItem;
    public StainId              _gameStain;

    public EquipDrawData(EquipItem gameItem) {
        _isEnabled = false; // by default, dont enabled these
        _wasEquippedBy = ""; // by default, no one equipped these
        _locked = false; // these arent locked by default, unless the gag is locked.
        _slot = EquipSlot.Head; // default slot, and the go-to equip slot for a gag
        _gameItem = gameItem; // default to nothing
        _gameStain = 0; // default to no stain
        _activeSlotListIdx = 0; // make active slot indexlist 0 aka helmet.
    }

    /// <summary> Sets the EquipSlot for EquipDrawData.
    /// <list type="bullet">
    /// <item><c>slot</c><param name="slot"> The slot to equip.</param></item>
    /// </list> </summary>
    public void SetSlot(EquipSlot slot) {
        _slot = slot;
    }
    /// <summary> Sets the EquipItem for EquipDrawData.
    /// <list type="bullet">
    /// <item><c>gameItem</c><param name="gameItem"> The item to equip.</param></item>
    /// </list> </summary>
    public void SetGameItem(EquipItem gameItem) {
        GagSpeak.Log.Debug($"[EquipDrawData] Changing equipment from {_gameItem} to {gameItem}");
        _gameItem = gameItem;
    }

    /// <summary> Sets the StainId for EquipDrawData.
    /// <list type="bullet">
    /// <item><c>gameStain</c><param name="gameStain"> The stain to equip.</param></item>
    /// </list> </summary>
    public void SetGameStain(StainId gameStain) {
        _gameStain = gameStain;
    }

    /// <summary> Resets the gameItem to nothing. </summary>
    public void ResetGameItem() {
        _gameItem = ItemIdVars.NothingItem(_slot);
    }

    /// <summary> Resets the gameStain to nothing. </summary>
    public void ResetGameStain() {
        _gameStain = 0;
    }
}