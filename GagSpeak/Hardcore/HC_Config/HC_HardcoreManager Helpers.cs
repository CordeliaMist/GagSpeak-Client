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

    public void ListIntegrityCheck(int whitelistSize) {
        // perform an integrity check to each playerSettings in the list
        if(_perPlayerConfigs.Count < whitelistSize) {
            for(int i = _perPlayerConfigs.Count; i < whitelistSize; i++) {
                _perPlayerConfigs.Add(new HC_PerPlayerConfig(_rsPropertyChanged));
            }
        } else if(_perPlayerConfigs.Count > whitelistSize) {
            _perPlayerConfigs.RemoveRange(whitelistSize, _perPlayerConfigs.Count - whitelistSize);
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
    public void SetAllowForcedFollow(int playerIdx, bool forcedFollow) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowForcedFollow to {forcedFollow}");
        _perPlayerConfigs[playerIdx].SetAllowForcedFollow(forcedFollow);
        Save();
    }
    
    public void SetForcedFollow(int playerIdx, bool forcedFollow) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting ForcedFollow to {forcedFollow}");
        _perPlayerConfigs[playerIdx].SetForcedFollow(forcedFollow);
        _saveService.QueueSave(this);
    }

    public void SetAllowForcedSit(int playerIdx, bool forcedSit) { 
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowForcedSit to {forcedSit}");
        _perPlayerConfigs[playerIdx].SetAllowForcedSit(forcedSit);
        Save();
    }

    public void SetForcedSit(int playerIdx, bool forcedSit) { 
        GagSpeak.Log.Debug($"[HardcoreManager] Setting ForcedSit to {forcedSit}");
        _perPlayerConfigs[playerIdx].SetForcedSit(forcedSit);
        _saveService.QueueSave(this);
    }

    public void SetAllowForcedToStay(int playerIdx, bool forcedToStay) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowForcedToStay to {forcedToStay}");
        _perPlayerConfigs[playerIdx].SetAllowForcedToStay(forcedToStay);
        Save();
    }

    public void SetForcedToStay(int playerIdx, bool forcedToStay) {
        GagSpeak.Log.Debug($"[HardcoreManager] Setting ForcedToStay to {forcedToStay}");
        _perPlayerConfigs[playerIdx].SetForcedToStay(forcedToStay);
        _saveService.QueueSave(this);
    }

    public void SetAllowBlindfold(int playerIdx, bool allowBlindfold) { 
        GagSpeak.Log.Debug($"[HardcoreManager] Setting AllowBlindfold to {allowBlindfold}");
        _perPlayerConfigs[playerIdx].SetAllowBlindfold(allowBlindfold); 
        Save();
    }

    public void SetBlindfolded(int playerIdx, bool blindfolded) {
        _perPlayerConfigs[playerIdx].SetBlindfolded(blindfolded);
        _saveService.QueueSave(this);
    }

    public void SetLegsRestraintedProperty(int playerIdx, int setIndex, bool value) {
        _perPlayerConfigs[playerIdx].SetLegsRestraintedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetArmsRestraintedProperty(int playerIdx, int setIndex, bool value) {
        _perPlayerConfigs[playerIdx].SetArmsRestraintedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetGaggedProperty(int playerIdx, int setIndex, bool value) {
        _perPlayerConfigs[playerIdx].SetGaggedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetBlindfoldedProperty(int playerIdx, int setIndex, bool value) {
        _perPlayerConfigs[playerIdx].SetBlindfoldedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetImmobileProperty(int playerIdx, int setIndex, bool value) {
        _perPlayerConfigs[playerIdx].SetImmobileProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetWeightedProperty(int playerIdx, int setIndex, bool value) {
        _perPlayerConfigs[playerIdx].SetWeightedProperty(setIndex, value);
        _saveService.QueueSave(this);
    }

    public void SetLightStimulationProperty(int playerIdx, int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._lightStimulationProperty = true;
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._mildStimulationProperty  = false;
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.125;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._lightStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.LightStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetMildStimulationProperty(int playerIdx, int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._lightStimulationProperty = false;
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._mildStimulationProperty  = true;
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.25;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._mildStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.MildStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetHeavyStimulationProperty(int playerIdx, int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._lightStimulationProperty = false;
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._mildStimulationProperty  = false;
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._heavyStimulationProperty = true;
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
