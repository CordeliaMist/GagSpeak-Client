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
using GagSpeak.Hardcore.Movement;
using GagSpeak.UI;
using System.Threading.Tasks;
using GagSpeak.Utility;

namespace GagSpeak.Hardcore;
public partial class HardcoreManager
{
    // for camera manager
    public unsafe GameCameraManager* cameraManager = GameCameraManager.Instance(); // for the camera manager object
    private readonly BlindfoldWindow _blindfoldWindow;

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
            case ListUpdateType.AddedRestraintSet : 
                foreach(var playerSettings in _perPlayerConfigs)
                    playerSettings._rsProperties.Add(new HC_RestraintProperties());
                
                break;
            case ListUpdateType.ReplacedRestraintSet:
                foreach(var playerSettings in _perPlayerConfigs)
                    playerSettings._rsProperties[e.SetIndex] = new HC_RestraintProperties();
                
                break;
            case ListUpdateType.RemovedRestraintSet:
                foreach(var playerSettings in _perPlayerConfigs)
                    playerSettings._rsProperties.RemoveAt(e.SetIndex);
                
                break;
            case ListUpdateType.SizeIntegrityCheck: {
                IntegrityCheck(e.SetIndex);
                break;
            }
        }
    }
#endregion events
#region Manager Methods
    /// <summary> Sees if any set is enabled, like the restraint set manager func, but also returns idx of the found assigner </summary>
    /// <returns> true if it is found, false if not, passes out the enabled set IDX, assigner name, and IDX of assigner name in whitelist </returns>
    public bool IsAnySetEnabled(out int enabledIdx, out string assignerName, out int assignerIdx) {
        // if we get the sucess on the restraintsetmanager func it means it exists
        if(_restraintSetManager.IsAnySetEnabled(out enabledIdx, out assignerName))
        {
            // if this is true it means enabledIdx is valid, lets double check the assigner is valid
            if(AltCharHelpers.IsPlayerInWhitelist(assignerName, out int whitelistCharIdx))
            {
                assignerIdx = whitelistCharIdx;
                return true;
            }
            else
            {
                assignerIdx = -1;
                return false;
            }
        }
        else
        {
            assignerIdx = -1;
            return false;
        }
    }


    /// <summary> If forced follow is active for you by any person in your whitelist </summary>
    /// <returns> true if yes, false is no, passed out the index of enabled option and name of player who did it </returns>
    public bool IsForcedFollowingForAny(out int enabledIdx, out string playerWhoForceFollowedYou) {
        // check if any of the players have forced follow active
        enabledIdx = _perPlayerConfigs.FindIndex(x => x._forcedFollow);
        // if the index is not -1, then find the name of the index you are on
        if(enabledIdx != -1) {
            playerWhoForceFollowedYou = _characterHandler.whitelistChars[enabledIdx]._charNAW[_characterHandler.whitelistChars[enabledIdx]._charNAWIdxToProcess]._name;
            return true;
        } else {
            playerWhoForceFollowedYou = "INVALID";
            return false;
        }
    }

    /// <summary> If forced sit is active for you by any person in your whitelist </summary>
    /// <returns> true if yes, false is no, passed out the index of enabled option and name of player who did it </returns>
    public bool IsForcedSittingForAny(out int enabledIdx, out string playerWhoForceSittedYou) {
        // check if any of the players have forced sit active
        enabledIdx = _perPlayerConfigs.FindIndex(x => x._forcedSit);
        // if the index is not -1, then find the name of the index you are on
        if(enabledIdx != -1) {
            playerWhoForceSittedYou = _characterHandler.whitelistChars[enabledIdx]._charNAW[_characterHandler.whitelistChars[enabledIdx]._charNAWIdxToProcess]._name;
            return true;
        } else {
            playerWhoForceSittedYou = "INVALID";
            return false;
        }
    }

    /// <summary> If forced to stay is active for you by any person in your whitelist </summary>
    /// <returns> true if yes, false is no, passed out the index of enabled option and name of player who did it </returns>
    public bool IsForcedToStayForAny(out int enabledIdx, out string playerWhoForceStayedYou) {
        // check if any of the players have forced to stay active
        enabledIdx = _perPlayerConfigs.FindIndex(x => x._forcedToStay);
        // if the index is not -1, then find the name of the index you are on
        if(enabledIdx != -1) {
            playerWhoForceStayedYou = _characterHandler.whitelistChars[enabledIdx]._charNAW[_characterHandler.whitelistChars[enabledIdx]._charNAWIdxToProcess]._name;
            return true;
        } else {
            playerWhoForceStayedYou = "INVALID";
            return false;
        }
    }

    /// <summary> If blindfolded is active for you by any person in your whitelist </summary>
    /// <returns> true if yes, false is no, passed out the index of enabled option and name of player who did it </returns>
    public bool IsBlindfoldedForAny(out int enabledIdx, out string playerWhoBlindfoldedYou) {
        // check if any of the players have blindfolded active
        enabledIdx = _perPlayerConfigs.FindIndex(x => x._blindfolded);
        // if the index is not -1, then find the name of the index you are on
        if(enabledIdx != -1) {
            // might change to proper idx later
            playerWhoBlindfoldedYou = _characterHandler.whitelistChars[enabledIdx]._charNAW[_characterHandler.whitelistChars[enabledIdx]._charNAWIdxToProcess]._name;
            return true;
        } else {
            playerWhoBlindfoldedYou = "INVALID"; // should never EVER reach here.
            return false;
        }
    }
    

    public void ResetEverythingDueToSafeword() {
        // call the reset everything for each playerSettings in the list
        for(int i = 0; i < _perPlayerConfigs.Count; i++) {
            // set all states to false, handling their logic accordingly
            _perPlayerConfigs[i].SetAllowForcedFollow(false);
            if(_perPlayerConfigs[i]._forcedFollow) {
                SetForcedFollow(i, false);
            }

            _perPlayerConfigs[i].SetAllowForcedSit(false);
            if(_perPlayerConfigs[i]._forcedSit) {
                SetForcedSit(i, false);
            }

            _perPlayerConfigs[i].SetAllowForcedToStay(false);
            if(_perPlayerConfigs[i]._forcedToStay) {
                SetForcedToStay(i, false);
            }

            _perPlayerConfigs[i].SetAllowBlindfold(false);
            if(_perPlayerConfigs[i]._blindfolded) {
                Task.Run(() => SetBlindfolded(i, false));
            }
        }
        // invoke safeword
        _rsPropertyChanged.Invoke(HardcoreChangeType.Safeword, RestraintSetChangeType.Disabled);
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
        // let's see if any set is enabled first
        if(IsAnySetEnabled(out int enabledIdx, out string assignerOfSet, out int assignerIdx)) {
            // it is valid, so let's set the multiplier based on the attribute
            if(_perPlayerConfigs[assignerIdx]._rsProperties[enabledIdx]._lightStimulationProperty) {
                GSLogger.LogType.Verbose($"[HardcoreManager] Light Stimulation Multiplier applied from set with factor of 1.125x!");
                StimulationMultipler = 1.125;
            }
            // apply mild stim
            else if(_perPlayerConfigs[assignerIdx]._rsProperties[enabledIdx]._mildStimulationProperty) {
                GSLogger.LogType.Verbose($"[HardcoreManager] Mild Stimulation Multiplier applied from set with factor of 1.25x!");
                StimulationMultipler = 1.25;
            }
            // apply heavy stim
            else if(_perPlayerConfigs[assignerIdx]._rsProperties[enabledIdx]._heavyStimulationProperty) {
                GSLogger.LogType.Verbose($"[HardcoreManager] Heavy Stimulation Multiplier applied from set with factor of 1.5x!");
                StimulationMultipler = 1.5;
            }
        } else {
            GSLogger.LogType.Verbose($"[HardcoreManager] No Stimulation Multiplier applied from set, defaulting to 1.0x!");
            StimulationMultipler = 1.0;
        }
    }
#endregion Manager Methods

#region property setters
    public void SetAllowForcedFollow(int playerIdx, bool forcedFollow) {
        GSLogger.LogType.Debug($"[HardcoreManager] Setting AllowForcedFollow to {forcedFollow}");
        _perPlayerConfigs[playerIdx].SetAllowForcedFollow(forcedFollow);
        Save();
    }
    
    public void SetForcedFollow(int playerIdx, bool forcedFollow) {
        // set the last recorded time
        LastMovementTime = DateTimeOffset.Now;
        // log and set it
        GSLogger.LogType.Debug($"[HardcoreManager] Setting ForcedFollow to {forcedFollow}");
        _perPlayerConfigs[playerIdx].SetForcedFollow(forcedFollow);
        _saveService.QueueSave(this);
        // handle the forced follow logic
        HandleForcedFollow(playerIdx, forcedFollow);
    }

    public void HandleForcedFollow(int playerIdx, bool newState) {
        // toggle movement type to legacy if we are not on legacy
        if(GagSpeakConfig.usingLegacyControls == false && playerIdx != -1) {
            // if forced follow is still on, dont switch it back to false
            uint mode = newState ? (uint)MovementMode.Legacy : (uint)MovementMode.Standard;
            GameConfig.UiControl.Set("MoveMode", mode);
        }
    }

    public void SetAllowForcedSit(int playerIdx, bool forcedSit) { 
        GSLogger.LogType.Debug($"[HardcoreManager] Setting AllowForcedSit to {forcedSit}");
        _perPlayerConfigs[playerIdx].SetAllowForcedSit(forcedSit);
        Save();
    }

    public void SetForcedSit(int playerIdx, bool forcedSit) { 
        GSLogger.LogType.Debug($"[HardcoreManager] Setting ForcedSit to {forcedSit}");
        _perPlayerConfigs[playerIdx].SetForcedSit(forcedSit);
        _saveService.QueueSave(this);
        // no need to toggle movement type, player will be immobile completely
    }

    public void SetAllowForcedToStay(int playerIdx, bool forcedToStay) {
        GSLogger.LogType.Debug($"[HardcoreManager] Setting AllowForcedToStay to {forcedToStay}");
        _perPlayerConfigs[playerIdx].SetAllowForcedToStay(forcedToStay);
        Save();
    }

    public void SetForcedToStay(int playerIdx, bool forcedToStay) {
        GSLogger.LogType.Debug($"[HardcoreManager] Setting ForcedToStay to {forcedToStay}");
        _perPlayerConfigs[playerIdx].SetForcedToStay(forcedToStay);
        _saveService.QueueSave(this);
    }

    public void SetAllowBlindfold(int playerIdx, bool allowBlindfold) { 
        GSLogger.LogType.Debug($"[HardcoreManager] Setting AllowBlindfold to {allowBlindfold}");
        _perPlayerConfigs[playerIdx].SetAllowBlindfold(allowBlindfold); 
        Save();
    }

    public void SetForcedFirstPerson(int playerIdx, bool forcedFirstPerson) { 
        GSLogger.LogType.Debug($"[HardcoreManager] Setting ForcedFirstPerson to {forcedFirstPerson}");
        _perPlayerConfigs[playerIdx].SetForcedFirstPerson(forcedFirstPerson); 
        Save();
    }

    public async Task SetBlindfolded(int playerIdx, bool blindfolded, string assignerName = "") {
        // if our new state is enabled and there is currently any other index currently enabled, return false
        // (blindfolded == true means going from not blindfolded to blindfolded)
        if(blindfolded && IsBlindfoldedForAny(out int enabledIdx, out string playerWhoBlindfoldedYou)) {
            GSLogger.LogType.Debug($"[HardcoreManager] Failed to set blindfolded to {blindfolded}, {playerWhoBlindfoldedYou} has already blindfolded you!");
            return;
        }
        // otherwise, we can handle the blindfold logic
        await HandleBlindfoldLogic(playerIdx, blindfolded, assignerName);
        // apply the changes
        _perPlayerConfigs[playerIdx].SetBlindfolded(blindfolded);
        _saveService.QueueSave(this);
    }

    public async Task HandleBlindfoldLogic(int playerIdx, bool newState, string assignerName) {
        // if the idx is not -1, process logic
        if(playerIdx != -1) {
            // toggle our window based on conditions
            if(newState == true && !_blindfoldWindow.IsOpen) {
                _blindfoldWindow.ActivateWindow();
            }
            if (newState == false && _blindfoldWindow.IsOpen) {
                _blindfoldWindow.DeactivateWindow();
            }
            if(newState) {
                // go in right away
                DoCamerVoodoo(playerIdx, newState);
                // apply the blindfold
                _glamourEvent.Invoke(UpdateType.BlindfoldEquipped, "", assignerName);
                
            } else {
                // wait a bit before doing the camera voodoo
                await Task.Delay(2000);
                DoCamerVoodoo(playerIdx, newState);
                // call a refresh all
                _glamourEvent.Invoke(UpdateType.BlindfoldUnEquipped, "", assignerName);
            }
        }
    }

    private unsafe void DoCamerVoodoo(int playerIdx, bool newValue) {
        // force the camera to first person, but dont loop the force
        if(newValue) {
            if(cameraManager != null && cameraManager->Camera != null
            && cameraManager->Camera->Mode != (int)CameraControlMode.FirstPerson)
            {
                cameraManager->Camera->Mode = (int)CameraControlMode.FirstPerson;
            }
        } else {
            if(cameraManager != null && cameraManager->Camera != null
            && cameraManager->Camera->Mode == (int)CameraControlMode.FirstPerson)
            {
                cameraManager->Camera->Mode = (int)CameraControlMode.ThirdPerson;
            }
        }
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
            _perPlayerConfigs[playerIdx]._rsProperties[setIndex]._heavyStimulationProperty = false;
            // and change the multiplier
            StimulationMultipler = 1.0;
        }
        _saveService.QueueSave(this);
        // Update the property
        _rsPropertyChanged.Invoke(HardcoreChangeType.HeavyStimulation, newValue ? RestraintSetChangeType.Enabled : RestraintSetChangeType.Disabled);
    }
}
#endregion property setters
