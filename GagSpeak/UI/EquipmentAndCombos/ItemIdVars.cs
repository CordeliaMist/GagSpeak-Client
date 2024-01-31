
using System;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;

namespace GagSpeak.Data;
public static class ItemIdVars {
    public static ItemId NothingId(EquipSlot slot) // used
        => uint.MaxValue - 128 - (uint)slot.ToSlot();

    public static ItemId SmallclothesId(EquipSlot slot) // unused
        => uint.MaxValue - 256 - (uint)slot.ToSlot();

    public static ItemId NothingId(FullEquipType type) // unused
        => uint.MaxValue - 384 - (uint)type;

    public static EquipItem NothingItem(EquipSlot slot) // used
        => new("Nothing", NothingId(slot), 0, 0, 0, 0, slot.ToEquipType(), 0, 0, 0);

    public static EquipItem NothingItem(FullEquipType type) // likely unused
        => new("Nothing", NothingId(type), 0, 0, 0, 0, type, 0, 0, 0);

    public static EquipItem SmallClothesItem(EquipSlot slot) // used
        => new("Smallclothes (NPC)", SmallclothesId(slot), 0, 9903, 0, 1, slot.ToEquipType(), 0, 0, 0);

}