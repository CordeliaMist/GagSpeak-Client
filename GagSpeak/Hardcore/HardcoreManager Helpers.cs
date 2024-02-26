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
    public void OnRestraintSetListModified(object sender, RestraintSetListChangedArgs e) {
        // update the player chars things to match the restraint set list change
        switch(e.UpdateType) {
            case ListUpdateType.AddedRestraintSet : {
                _rsProperties.Add(new HC_RestraintProperties());
                }
                break;
            case ListUpdateType.ReplacedRestraintSet: {
                _rsProperties[e.SetIndex] = new HC_RestraintProperties();
                }
                break;
            case ListUpdateType.RemovedRestraintSet: {
                _rsProperties.RemoveAt(e.SetIndex);
                }
                break;
            case ListUpdateType.SizeIntegrityCheck: {
                // call the integrity check function from uniquePlayerPerms
                IntegrityCheck(e.SetIndex);
                break;
            }
        }
        Save();
    }
    // this event will simply make sure that when we 
    private void OnJobChange(object sender, GagSpeakGlamourEventArgs e) {
        if(e.UpdateType == UpdateType.JobChange && _restraintSetManager._restraintSets.Any(x => x._enabled)) {
            ActiveSetIdxEnabled = _restraintSetManager._restraintSets.FindIndex(x => x._enabled);
        }
    }
#endregion events
#region Manager Methods
    public void ResetEverythingDueToSafeword() {
        _forcedSit = false;
        _forcedFollow = false;
        _forcedToStay = false;
        _blindfolded = false;
        _saveService.QueueSave(this);
        // invoke safeword
        _rsPropertyChanged.Invoke(HardcoreChangeType.Safeword, RestraintSetChangeType.Disabled);
    }

    // run integrity check to make sure the size of _rsProperties is the same as the restraint set size
    private void IntegrityCheck(int setIndex) {
        if(_rsProperties.Count < setIndex) {
            for(int i = _rsProperties.Count; i < setIndex; i++) {
                _rsProperties.Add(new HC_RestraintProperties());
            }
        } else if(_rsProperties.Count > setIndex) {
            _rsProperties.RemoveRange(setIndex, _rsProperties.Count - setIndex);
        }
    }

    public void ApplyMultipler() {
        if(_restraintSetManager._restraintSets.Any(x => x._enabled)) {
            ActiveSetIdxEnabled = _restraintSetManager._restraintSets.FindIndex(x => x._enabled);
            if(_rsProperties[ActiveSetIdxEnabled]._lightStimulationProperty) {
                StimulationMultipler = 1.125;
            } else if(_rsProperties[ActiveSetIdxEnabled]._mildStimulationProperty) {
                StimulationMultipler = 1.25;
            } else if(_rsProperties[ActiveSetIdxEnabled]._heavyStimulationProperty) {
                StimulationMultipler = 1.5;
            } else {
                StimulationMultipler = 1.0;
            }
        }
    }
#endregion Manager Methods

#region property setters
    public void SetForcedSit(bool forcedSit) { 
        _forcedSit = forcedSit;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedSit, forcedSit ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
    public void SetForcedFollow(bool forcedFollow) {
        _forcedFollow = forcedFollow;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedFollow, forcedFollow ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
    public void SetForcedToStay(bool forcedToStay) {
        _forcedToStay = forcedToStay;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedToStay, forcedToStay ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
    public void SetBlindfolded(bool blindfolded) {
        _blindfolded = blindfolded;
        _saveService.QueueSave(this);
        // invoke the change
        _rsPropertyChanged.Invoke(HardcoreChangeType.Blindfolded, blindfolded ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetLegsRestraintedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._legsRestraintedProperty = value;
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.LegsRestraint, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetArmsRestraintedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._armsRestraintedProperty = value;
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.ArmsRestraint, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetGaggedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._gaggedProperty = value;
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.Gagged, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetBlindfoldedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._blindfoldedProperty = value;
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.Blindfolded, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetImmobileProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._immobileProperty = value;
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.Immobile, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetWeightedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._weightyProperty = value;
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.Weighty, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetLightStimulationProperty(int setIndex, bool newValue) {
        // if it is curretly active
        if(newValue == true) {
            // turn off the other two
            _rsProperties[setIndex]._lightStimulationProperty = true;
            _rsProperties[setIndex]._mildStimulationProperty  = false;
            _rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.125;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _rsProperties[setIndex]._lightStimulationProperty = false;
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
            _rsProperties[setIndex]._lightStimulationProperty = false;
            _rsProperties[setIndex]._mildStimulationProperty  = true;
            _rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.25;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _rsProperties[setIndex]._mildStimulationProperty = false;
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
            _rsProperties[setIndex]._lightStimulationProperty = false;
            _rsProperties[setIndex]._mildStimulationProperty  = false;
            _rsProperties[setIndex]._heavyStimulationProperty = true;
            // and change the multiplier
            StimulationMultipler = 1.5;
        }
        // otherwise, we are disabling it, so just disable it and reset the multiplier
        else {
            _rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.HeavyStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
}
#endregion property setters
