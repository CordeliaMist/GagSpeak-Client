using System;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using GagSpeak.Wardrobe;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace GagSpeak.Hardcore;
public partial class HardcoreManager
{
#region NodeManagement
    public IEnumerable<ITextNode> GetAllNodes() {
        return new ITextNode[]{StoredEntriesFolder}.Concat(GetAllNodes(StoredEntriesFolder.Children));
    }

    public IEnumerable<ITextNode> GetAllNodes(IEnumerable<ITextNode> nodes) {
        foreach (var node in nodes) {
            yield return node;
            if (node is TextFolderNode folder) {
                var children = GetAllNodes(folder.Children);
                foreach (var childNode in children) {
                    yield return childNode;
                }
            }
        }
    }

    public bool TryFindParent(ITextNode node, out TextFolderNode? parent) {
        foreach (var candidate in GetAllNodes()) {
            if (candidate is TextFolderNode folder && folder.Children.Contains(node)) {
                parent = folder;
                return true;
            }
        }

        parent = null;
        return false;
    }

    public void CreateTextNode(TextFolderNode folder) {
        var newNode = new TextEntryNode()
        {
            Enabled = true,
            Text = LastSeenDialogText.Item1,
            Options = LastSeenDialogText.Item2.ToArray(),
        };
        folder.Children.Add(newNode);
    }
#endregion NodeManagement
#region events
    public void OnRestraintSetListModified(object? sender, RestraintSetListChangedArgs e) {
        // update the player chars things to match the restraint set list change
        switch(e.UpdateType) {
            case ListUpdateType.AddedRestraintSet : {
                // add a new set of properties for each playersettings object
                foreach(var playerSettings in _perPlayerConfigs) {
                    playerSettings._rsProperties.Add(new HC_RestraintProperties());
                }
                }
                break;
            case ListUpdateType.ReplacedRestraintSet: {
                // replace the set of properties for each playersettings object
                foreach(var playerSettings in _perPlayerConfigs) {
                    playerSettings._rsProperties[e.SetIndex] = new HC_RestraintProperties();
                }
                }
                break;
            case ListUpdateType.RemovedRestraintSet: {
                // remove the set of properties for each playersettings object
                foreach(var playerSettings in _perPlayerConfigs) {
                    playerSettings._rsProperties.RemoveAt(e.SetIndex);
                }
                }
                break;
            case ListUpdateType.SizeIntegrityCheck: {
                IntegrityCheck(e.SetIndex);
                break;
            }
        }
    }

    public void OnRestraintSetToggled(object? sender, RS_ToggleEventArgs e) {
        // if the restraint set is being toggled, we need to update the activeHCsetIdx
        
        // do stuff to update the active config and apply the correct multiplier when active
    }

#endregion events
#region Manager Methods
    public void ResetEverythingDueToSafeword() {
        // call the reset everything for each playerSettings in the list
        foreach(var playerSettings in _perPlayerConfigs) {
            playerSettings.ResetEverythingDueToSafeword();
        }
    }

    // run integrity check to make sure the size of _rsProperties is the same as the restraint set size
    private void IntegrityCheck(int setIndex) {
        // perform an integrity check to each playerSettings in the list
        foreach(var playerSettings in _perPlayerConfigs) {
            playerSettings.IntegrityCheck(setIndex);
        }
    }

    public void ApplyMultipler() {
        if(_perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[ActiveHCsetIdx]._lightStimulationProperty)
        {
            StimulationMultipler = 1.125;
        }
        else if(_perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[ActiveHCsetIdx]._mildStimulationProperty)
        {
            StimulationMultipler = 1.25;
        }
        else if(_perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[ActiveHCsetIdx]._heavyStimulationProperty)
        {
            StimulationMultipler = 1.5;
        }
        else
        {
            StimulationMultipler = 1.0;
        }
    }
#endregion Manager Methods

#region property setters
    public void SetAllowForcedFollow(bool forcedFollow) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowForcedFollow to {forcedFollow}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetAllowForcedFollow(forcedFollow);
        Save();
    }
    
    public void SetForcedFollow(bool forcedFollow) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting ForcedFollow to {forcedFollow}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetForcedFollow(forcedFollow);
        _saveService.QueueSave(this);
    }

    public void SetAllowForcedSit(bool forcedSit) { 
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowForcedSit to {forcedSit}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetAllowForcedSit(forcedSit);
        Save();
    }

    public void SetForcedSit(bool forcedSit) { 
        GagSpeak.Log.Debug($"[HardcoreManager] Setting ForcedSit to {forcedSit}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetForcedSit(forcedSit);
        _saveService.QueueSave(this);
    }

    public void SetAllowForcedToStay(bool forcedToStay) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowForcedToStay to {forcedToStay}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetAllowForcedToStay(forcedToStay);
        Save();
    }

    public void SetForcedToStay(bool forcedToStay) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting ForcedToStay to {forcedToStay}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetForcedToStay(forcedToStay);
        _saveService.QueueSave(this);
    }

    public void SetAllowBlindfold(bool allowBlindfold) { 
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowBlindfold to {allowBlindfold}");
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetAllowBlindfold(allowBlindfold); 
        Save();
    }

    public void SetBlindfolded(bool blindfolded) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetBlindfolded(blindfolded);
        _saveService.QueueSave(this);
    }

    public void SetLegsRestraintedProperty(int setIndex, bool value) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetLegsRestraintedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetArmsRestraintedProperty(int setIndex, bool value) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetArmsRestraintedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetGaggedProperty(int setIndex, bool value) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetGaggedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetBlindfoldedProperty(int setIndex, bool value) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetBlindfoldedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetImmobileProperty(int setIndex, bool value) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetImmobileProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetWeightedProperty(int setIndex, bool value) {
        _perPlayerConfigs[ActivePlayerCfgListIdx].SetWeightedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetLightStimulationProperty(int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._lightStimulationProperty = true;
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._mildStimulationProperty  = false;
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.125;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._lightStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.LightStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetMildStimulationProperty(int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._lightStimulationProperty = false;
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._mildStimulationProperty  = true;
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.25;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._mildStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.MildStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetHeavyStimulationProperty(int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._lightStimulationProperty = false;
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._mildStimulationProperty  = false;
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._heavyStimulationProperty = true;
            // and change the multiplier
            StimulationMultipler = 1.5;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _perPlayerConfigs[ActivePlayerCfgListIdx]._rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.HeavyStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
}
#endregion property setters
