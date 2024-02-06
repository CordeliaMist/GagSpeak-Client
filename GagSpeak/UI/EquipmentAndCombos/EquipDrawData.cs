using Newtonsoft.Json.Linq;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;
using Newtonsoft.Json;
using System;
using System.Linq;
using GagSpeak.Utility;

namespace GagSpeak.UI.Equipment;
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
        // Find the index of the slot in the EqdpSlots list
        int activeSlotIndex = EquipSlotExtensions.EqdpSlots.Select((s, i) => new { s, i })
                                    .FirstOrDefault(x => x.s == slot)?.i ?? -1;
        // Check if the slot was found in the list
        if (activeSlotIndex != -1) {
            // Set the active slot index
            _activeSlotListIdx = activeSlotIndex;
        } else {
            // Handle the case where the slot was not found in the list
            Console.WriteLine($"EquipSlot {slot} not found in EqdpSlots list.");
        }
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
    public void ResetDrawDataGameItem() {
        _gameItem = ItemIdVars.NothingItem(_slot);
    }

    /// <summary> Resets the gameStain to nothing. </summary>
    public void ResetDrawDataGameStain() {
        _gameStain = 0;
    }

    // In EquipDrawData
    public JObject Serialize() {
        // Create a JsonSerializer with the EquipItemConverter
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new EquipItemConverter());
        // Serialize _gameItem and _gameStain as JObjects
        JObject gameItemObj = JObject.FromObject(_gameItem, serializer);

        // Include gameItemObj and gameStainObj in the serialized object
        return new JObject() {
            ["IsEnabled"] = _isEnabled,
            ["WasEquippedBy"] = _wasEquippedBy,
            ["Locked"] = _locked,
            ["ActiveSlotListIdx"] = _activeSlotListIdx,
            ["Slot"] = _slot.ToString(),
            ["GameItem"] = gameItemObj,
            ["GameStain"] = _gameStain.ToString(),
        };
    }

    public void Deserialize(JObject jsonObject) {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        _isEnabled = jsonObject["IsEnabled"]?.Value<bool>() ?? false;
        _wasEquippedBy = jsonObject["WasEquippedBy"]?.Value<string>() ?? string.Empty;
        _locked = jsonObject["Locked"]?.Value<bool>() ?? false;
        _activeSlotListIdx = jsonObject["ActiveSlotListIdx"]?.Value<int>() ?? 0;
        _slot = (EquipSlot)Enum.Parse(typeof(EquipSlot), jsonObject["Slot"]?.Value<string>() ?? string.Empty);

        var serializer = new JsonSerializer();
        serializer.Converters.Add(new EquipItemConverter());
        _gameItem = jsonObject["GameItem"] != null ? jsonObject["GameItem"].ToObject<EquipItem>(serializer) : new EquipItem();
        // Parse the StainId
        if (byte.TryParse(jsonObject["GameStain"]?.Value<string>(), out var stainIdByte)) {
            _gameStain = new StainId(stainIdByte);
        } else {
            // Handle the error, e.g., log a message or throw an exception
            Console.WriteLine($"Invalid StainId value: {jsonObject["GameStain"]?.Value<string>()}. Must be a valid byte value.");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}