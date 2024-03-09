using GagSpeak.Events;
using GagSpeak.Wardrobe;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace GagSpeak.Hardcore;
public partial class HC_PerPlayerConfig
{
    public void RestraintSetListModified(ListUpdateType updateType, int setIndex) {
        // update the player chars things to match the restraint set list change
        switch(updateType) {
            case ListUpdateType.AddedRestraintSet : {
                _rsProperties.Add(new HC_RestraintProperties());
                }
                break;
            case ListUpdateType.ReplacedRestraintSet: {
                _rsProperties[setIndex] = new HC_RestraintProperties();
                }
                break;
            case ListUpdateType.RemovedRestraintSet: {
                _rsProperties.RemoveAt(setIndex);
                }
                break;
            case ListUpdateType.SizeIntegrityCheck: {
                IntegrityCheck(setIndex);
                break;
            }
        }
    }
#region Manager Methods
    // run integrity check to make sure the size of _rsProperties is the same as the restraint set size
    public void IntegrityCheck(int RestraintSetListSize) {
        if(_rsProperties.Count < RestraintSetListSize) {
            for(int i = _rsProperties.Count; i < RestraintSetListSize; i++) {
                _rsProperties.Add(new HC_RestraintProperties());
            }
        } else if(_rsProperties.Count > RestraintSetListSize) {
            _rsProperties.RemoveRange(RestraintSetListSize, _rsProperties.Count - RestraintSetListSize);
        }
    }

#endregion Manager Methods

#region property setters
    public void SetAllowForcedFollow(bool newVal) {
        GSLogger.LogType.Debug($"[HC_PerPlayerConfig] SetAllowForcedFollow: {newVal}");
        _allowForcedFollow = newVal;
    }

    public void SetForcedFollow(bool newVal) {
        GSLogger.LogType.Debug($"[HC_PerPlayerConfig] SetForcedFollow: {newVal}");
        _forcedFollow = newVal;
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedFollow, newVal ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetAllowForcedSit(bool newVal) { _allowForcedSit = newVal;}
    public void SetForcedSit(bool newVal) { 
        _forcedSit = newVal;
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedSit, newVal ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetAllowForcedToStay(bool newVal) { _allowForcedToStay = newVal;}
    public void SetForcedToStay(bool newVal) {
        _forcedToStay = newVal;
        _rsPropertyChanged.Invoke(HardcoreChangeType.ForcedToStay, newVal ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetAllowBlindfold(bool newVal) { _allowBlindfold = newVal;}
    public void SetForcedFirstPerson(bool newVal) { _forceLockFirstPerson = newVal;}
    public void SetBlindfolded(bool blindfolded) {
        _blindfolded = blindfolded;
        _rsPropertyChanged.Invoke(HardcoreChangeType.Blindfolded, blindfolded ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetLegsRestraintedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._legsRestraintedProperty = value;
        _rsPropertyChanged.Invoke(HardcoreChangeType.LegsRestraint, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetArmsRestraintedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._armsRestraintedProperty = value;
        _rsPropertyChanged.Invoke(HardcoreChangeType.ArmsRestraint, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetGaggedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._gaggedProperty = value;
        _rsPropertyChanged.Invoke(HardcoreChangeType.Gagged, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetBlindfoldedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._blindfoldedProperty = value;
        _rsPropertyChanged.Invoke(HardcoreChangeType.Blindfolded, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetImmobileProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._immobileProperty = value;
        _rsPropertyChanged.Invoke(HardcoreChangeType.Immobile, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }

    public void SetWeightedProperty(int setIndex, bool value) {
        _rsProperties[setIndex]._weightyProperty = value;
        _rsPropertyChanged.Invoke(HardcoreChangeType.Weighty, value ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
}
#endregion property setters
