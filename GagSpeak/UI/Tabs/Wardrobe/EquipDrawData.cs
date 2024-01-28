using Newtonsoft.Json.Linq;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;
using Newtonsoft.Json;

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

    /// <summary> Sets the IsEnabled for EquipDrawData.
    public void SetDrawDataIsEnabled(bool isEnabled) {
        _isEnabled = isEnabled;
    }

    /// <summary> Sets the WasEquippedBy for EquipDrawData.
    public void SetDrawDataEquippedBy(string drawDataEquippedBy) {
        _wasEquippedBy = drawDataEquippedBy;
    }

    /// <summary> Sets the Locked for EquipDrawData.
    public void SetDrawDataLocked(bool locked) {
        _locked = locked;
    }

    /// <summary> Sets the EquipSlot for EquipDrawData.
    /// <list type="bullet">
    /// <item><c>slot</c><param name="slot"> The slot to equip.</param></item>
    /// </list> </summary>
    public void SetDrawDataSlot(EquipSlot slot) {
        _slot = slot;
    }
    /// <summary> Sets the EquipItem for EquipDrawData.
    /// <list type="bullet">
    /// <item><c>gameItem</c><param name="gameItem"> The item to equip.</param></item>
    /// </list> </summary>
    public void SetDrawDataGameItem(EquipItem gameItem) {
        GagSpeak.Log.Debug($"[EquipDrawData] Changing equipment from {_gameItem} to {gameItem}");
        _gameItem = gameItem;
    }

    /// <summary> Sets the StainId for EquipDrawData.
    /// <list type="bullet">
    /// <item><c>gameStain</c><param name="gameStain"> The stain to equip.</param></item>
    /// </list> </summary>
    public void SetDrawDataGameStain(StainId gameStain) {
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

    // In EquipDrawData
    public JObject Serialize() {
        // Create a JsonSerializer with the EquipItemConverter
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new EquipItemConverter());
        // Serialize _gameItem and _gameStain as JObjects
        JObject gameItemObj = JObject.FromObject(_gameItem, serializer);
        JObject gameStainObj = JObject.FromObject(_gameStain, serializer);

        // Include gameItemObj and gameStainObj in the serialized object
        return new JObject() {
            ["IsEnabled"] = _isEnabled,
            ["WasEquippedBy"] = _wasEquippedBy,
            ["Locked"] = _locked,
            ["ActiveSlotListIdx"] = _activeSlotListIdx,
            ["Slot"] = _slot.ToString(),
            ["GameItem"] = gameItemObj,
            ["GameStain"] = gameStainObj
        };
    }
}