using GagSpeak.Services;

namespace GagSpeak.CharacterData;
public partial class CharacterHandler : ISavable
{
    public void SetRestraintedLegsProperty(int whitelistIdx, int restraintSetIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._legsRestraintedProperty[restraintSetIdx] = value;
        _saveService.QueueSave(this);
    }

    public void SetRestraintedArmsProperty(int whitelistIdx, int restraintSetIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._armsRestraintedProperty[restraintSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // gagged property
    public void SetGaggedProperty(int whitelistIdx, int gagSetIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._gaggedProperty[gagSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // blindfold property
    public void SetBlindfoldedProperty(int whitelistIdx, int blindfoldSetIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._blindfoldedProperty[blindfoldSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // imobile property
    public void SetImmobileProperty(int whitelistIdx, int immobileSetIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._immobileProperty[immobileSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // weighted property
    public void SetWeightedProperty(int whitelistIdx, int weightedSetIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._weightyProperty[weightedSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // light stimulation property
    public void SetLightStimulationProperty(int whitelistIdx, int lightStimSetIdx, bool value) {
        // if mild or heavy are set, unset them
        if (value) {
            playerChar._uniquePlayerPerms[whitelistIdx]._mildStimulationProperty[lightStimSetIdx] = false;
            playerChar._uniquePlayerPerms[whitelistIdx]._heavyStimulationProperty[lightStimSetIdx] = false;
        }
        playerChar._uniquePlayerPerms[whitelistIdx]._lightStimulationProperty[lightStimSetIdx] = value;
        _saveService.QueueSave(this);
    }
    
    // mild stimulation property
    public void SetMildStimulationProperty(int whitelistIdx, int mildStimSetIdx, bool value) {
        // if light or heavy are set, unset them
        if (value) {
            playerChar._uniquePlayerPerms[whitelistIdx]._lightStimulationProperty[mildStimSetIdx] = false;
            playerChar._uniquePlayerPerms[whitelistIdx]._heavyStimulationProperty[mildStimSetIdx] = false;
        }
        playerChar._uniquePlayerPerms[whitelistIdx]._mildStimulationProperty[mildStimSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // heavy stimulation property
    public void SetHeavyStimulationProperty(int whitelistIdx, int heavyStimSetIdx, bool value) {
        // if light or mild are set, unset them
        if (value) {
            playerChar._uniquePlayerPerms[whitelistIdx]._lightStimulationProperty[heavyStimSetIdx] = false;
            playerChar._uniquePlayerPerms[whitelistIdx]._mildStimulationProperty[heavyStimSetIdx] = false;
        }
        playerChar._uniquePlayerPerms[whitelistIdx]._heavyStimulationProperty[heavyStimSetIdx] = value;
        _saveService.QueueSave(this);
    }

    // set follow me
    public void SetFollowMe(int whitelistIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._followMe = value;
        _saveService.QueueSave(this);
    }

    // set sit
    public void SetSit(int whitelistIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._sit = value;
        _saveService.QueueSave(this);
    }

    // set stay here for now
    public void SetStayHereForNow(int whitelistIdx, bool value) {
        playerChar._uniquePlayerPerms[whitelistIdx]._stayHereForNow = value;
        _saveService.QueueSave(this);
    }
    
}