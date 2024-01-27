




using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using GagSpeak.Data;

namespace GagSpeak.Wardrobe;

public class RestraintSet //: IDisposable
{
    private string _name = "New Restraint Set"; // lets you define the name of the set
    private string _description = "This is a new restraint set!"; // lets you define the description of the set
    private bool _enabled = false; // lets you define if the set is enabled
    private bool _locked = false; // lets you define if the set is locked
    public Dictionary<EquipSlot, EquipDrawData> _drawData; // stores the equipment draw data for the set

    public RestraintSet() {
        // create the new dictionaries
        _drawData = new Dictionary<EquipSlot, EquipDrawData>();

        foreach (var slot in EquipSlotExtensions.EqdpSlots) {
            _drawData[slot] = new EquipDrawData(ItemIdVars.NothingItem(slot));
        }
    }

    public string GetName() => _name;

    public string GetDescription() => _description;

    public void ChangeName(string name) {
        _name = name;
        // be sure to call an update to the file here (or do this in the manager)
    }

    public bool GetIsSetEnabled() => _enabled;

    public bool IsSetLocked() => _enabled;


    
}