using System;
using Newtonsoft.Json;
using Penumbra.GameData.Structs;

namespace GagSpeak;
// had to make this to get a workaround for the fact that the json readwrite doesnt read from private readonly structs, and equipItem is a private readonly struct
public class EquipItemConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanConvert(Type objectType) {
        if(objectType == typeof(EquipItem)) {
            GagSpeak.Log.Debug($"[EquipItemConverter] Can convert {objectType}");
            return true;
        } else {
            return false;
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        GagSpeak.Log.Debug($"[EquipItemConverter] Reading JSON for {objectType}");
        var surrogate = serializer.Deserialize<EquipItemSurrogate>(reader);
        return (EquipItem)surrogate;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        GagSpeak.Log.Debug($"[EquipItemConverter] Writing JSON for {value}");
        var surrogate = (EquipItemSurrogate)(EquipItem)value;
        serializer.Serialize(writer, surrogate);
    }
}