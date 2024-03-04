namespace GagSpeak.Interop;

/// <summary> the type of statechange provided by glamourerIPC </summary>
public enum StateChangeType {
    /// <summary> A characters saved state had the model id changed. This means everything may have changed. Data is the old model id and the new model id. [(uint, uint)] </summary>
    Model,

    /// <summary> A characters saved state had multiple customization values changed. TData is the old customize array and the applied changes. [(Customize, CustomizeFlag)] </summary>
    EntireCustomize,

    /// <summary> A characters saved state had a customization value changed. Data is the old value, the new value and the type. [(CustomizeValue, CustomizeValue, CustomizeIndex)]. </summary>
    Customize,

    /// <summary> A characters saved state had an equipment piece changed. Data is the old value, the new value and the slot [(EquipItem, EquipItem, EquipSlot)]. </summary>
    Equip,

    /// <summary> A characters saved state had its weapons changed. Data is the old mainhand, the old offhand, the new mainhand and the new offhand [(EquipItem, EquipItem, EquipItem, EquipItem)]. </summary>
    Weapon,

    /// <summary> A characters saved state had a stain changed. Data is the old stain id, the new stain id and the slot [(StainId, StainId, EquipSlot)]. </summary>
    Stain,

    /// <summary> A characters saved state had a crest visibility changed. Data is the old crest visibility, the new crest visibility and the slot [(bool, bool, EquipSlot)]. </summary>
    Crest,

    /// <summary> A characters saved state had its customize parameter changed. Data is the old value, the new value and the type [(CustomizeParameterValue, CustomizeParameterValue, CustomizeParameterFlag)]. </summary>
    Parameter,

    /// <summary> A characters saved state had a material color table value changed. Data is the old value, the new value and the index [(Vector3, Vector3, MaterialValueIndex)] or just the index for resets. </summary>
    MaterialValue,

    /// <summary> A characters saved state had a design applied. This means everything may have changed. Data is the applied design. [DesignBase] </summary>
    Design,

    /// <summary> A characters saved state had its state reset to its game values. This means everything may have changed. Data is null. </summary>
    Reset,

    /// <summary> A characters saved state had a meta toggle changed. Data is the old stain id, the new stain id and the slot [(StainId, StainId, EquipSlot)]. </summary>
    Other,

    /// <summary> A characters state was reapplied. Data is null. </summary>
    Reapply,
}