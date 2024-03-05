using System.Collections.Generic;

namespace GagSpeak.CharacterData;

public partial class CharacterHandler
{
#region WhitelistSetters
    public void SetWhitelistSafewordUsed(int index, bool value) {
        if(whitelistChars[index]._safewordUsed != value) {
            whitelistChars[index]._safewordUsed = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistGrantExtendedLockTimes(int index, bool value) {
        if(whitelistChars[index]._grantExtendedLockTimes != value) {
            whitelistChars[index]._grantExtendedLockTimes = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetBlindfoldCondition(int index, bool value) {
        if(whitelistChars[index]._blindfolded != value) {
            whitelistChars[index]._blindfolded = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistDirectChatGarblerActive(int index, bool value) {
        if(whitelistChars[index]._directChatGarblerActive != value) {
            whitelistChars[index]._directChatGarblerActive = value;
            _saveService.QueueSave(this);
        }
    }
    
    public void SetWhitelistDirectChatGarblerLocked(int index, bool value) {
        if(whitelistChars[index]._directChatGarblerLocked != value) {
            whitelistChars[index]._directChatGarblerLocked = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistEnableWardrobe(int index, bool value) {
        if(whitelistChars[index]._enableWardrobe != value) {
            whitelistChars[index]._enableWardrobe = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistLockGagStorageOnGagLock(int index, bool value) {
        if(whitelistChars[index]._lockGagStorageOnGagLock != value) {
            whitelistChars[index]._lockGagStorageOnGagLock = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistEnableRestraintSets(int index, bool value) {
        if(whitelistChars[index]._enableRestraintSets != value) {
            whitelistChars[index]._enableRestraintSets = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistRestraintSetLocking(int index, bool value) {
        if(whitelistChars[index]._restraintSetLocking != value) {
            whitelistChars[index]._restraintSetLocking = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowPuppeteer(int index, bool value) {
        if(whitelistChars[index]._allowPuppeteer != value) {
            whitelistChars[index]._allowPuppeteer = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistTriggerPhraseForPuppeteer(int index, string value) {
        if(whitelistChars[index]._theirTriggerPhrase != value) {
            whitelistChars[index]._theirTriggerPhrase = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistTriggerPhraseStartChar(int index, string value) {
        if(whitelistChars[index]._theirTriggerStartChar != value) {
            whitelistChars[index]._theirTriggerStartChar = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistTriggerPhraseEndChar(int index, string value) {
        if(whitelistChars[index]._theirTriggerEndChar != value) {
            whitelistChars[index]._theirTriggerEndChar = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowSitRequests(int index, bool value) {
        if(whitelistChars[index]._allowsSitRequests != value) {
            whitelistChars[index]._allowsSitRequests = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowMotionRequests(int index, bool value) {
        if(whitelistChars[index]._allowsMotionRequests != value) {
            whitelistChars[index]._allowsMotionRequests = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowAllCommands(int index, bool value) {
        if(whitelistChars[index]._allowsAllCommands != value) {
            whitelistChars[index]._allowsAllCommands = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistEnableToybox(int index, bool value) {
        if(whitelistChars[index]._enableToybox != value) {
            whitelistChars[index]._enableToybox = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowChangingToyState(int index, bool value) {
        if(whitelistChars[index]._allowChangingToyState != value) {
            whitelistChars[index]._allowChangingToyState = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowIntensityControl(int index, bool value) {
        if(whitelistChars[index]._allowsIntensityControl != value) {
            whitelistChars[index]._allowsIntensityControl = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistIntensityLevel(int index, byte value) {
        if(whitelistChars[index]._intensityLevel != value) {
            whitelistChars[index]._intensityLevel = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowUsingPatterns(int index, bool value) {
        if(whitelistChars[index]._allowsUsingPatterns != value) {
            whitelistChars[index]._allowsUsingPatterns = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistAllowToyboxLocking(int index, bool value) {
        if(whitelistChars[index]._lockToyboxUI != value) {
            whitelistChars[index]._lockToyboxUI = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistToyIsActive(int index, bool value) {
        if(whitelistChars[index]._isToyActive != value) {
            whitelistChars[index]._isToyActive = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistToyStepSize(int index, int value) {
        if(whitelistChars[index]._activeToystepSize != value) {
            whitelistChars[index]._activeToystepSize = value;
            _saveService.QueueSave(this);
        }
    }

    // import list stuff
    public void StoreRestraintListForPlayer(int index, List<string> restraintList) {
        whitelistChars[index]._storedRestraintSets = restraintList;
        _saveService.QueueSave(this);
    }

    public void StoredAliasDetailsForPlayer(int index, Dictionary<string,string> aliasDetails) {
        whitelistChars[index]._storedAliases = aliasDetails;
        _saveService.QueueSave(this);
    }

    public void StorePatternNames(int index, List<string> patternNames) {
        whitelistChars[index]._storedPatternNames = patternNames;
        _saveService.QueueSave(this);
    }
#endregion WhitelistSetters
}