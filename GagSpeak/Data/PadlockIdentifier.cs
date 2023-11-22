using System;
using ImGuiNET;
using System.Text.RegularExpressions;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;

#pragma warning disable IDE1006 
namespace GagSpeak.Data;
// a struct to hold information on whitelisted players.
public class PadlockIdentifier {
    public string _inputPassword { get; set; } // This will hold the input password
    public string _inputCombination { get; set; } // This will hold the input combination
    public string _inputTimer { get; set; } // This will hold the input timer
    public string _storedPassword { get; set; } // 20 character max string
    public string _storedCombination { get; set; } // This will be a string in the format of 0000
    public string _storedTimer { get; set; } // This will be a string in the format of 00h00m00s
    public string _mistressAssignerName { get; set; } // This will be the name of the player who assigned the padlock
    public GagPadlocks _padlockType { get; set; } = GagPadlocks.None;

    public PadlockIdentifier() {
        // set default values for our strings
        if(_inputPassword == null) { _inputPassword = "";}
        if(_inputCombination == null) { _inputCombination = "";}
        if(_inputTimer == null) { _inputTimer = "";}
        if(_storedPassword == null) { _storedPassword = "";}
        if(_storedCombination == null) { _storedCombination = "";}
        if(_storedTimer == null) { _storedTimer = "";}
        if(_mistressAssignerName == null) { _mistressAssignerName = "";}
    }

    public void SetType(GagPadlocks padlockType) {
        _padlockType = padlockType;
    }
    
    /// <summary>
    /// This function is used typically by the command manager, for when we have no field to insert
    /// a password to, and do it directly instead.
    /// <param name="locktype"></param> 
    /// <param name="password"></param>
    /// </summary>
    public bool SetAndValidate(GagSpeakConfig _config, string locktype, string password = "", string secondPassword = "",
    string assignerPlayerName = null, string targetPlayerName = null) {
        if (!Enum.TryParse(locktype, true, out GagPadlocks padlockType)) {
            return false;}// or throw an exception
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Setting padlock type to {padlockType}");
        switch (_padlockType) {
            case GagPadlocks.None:
                return false;
            case GagPadlocks.MetalPadlock:
                // handle MetalPadlock case
                break;
            case GagPadlocks.CombinationPadlock:
                this._storedCombination = password;
                break;
            case GagPadlocks.PasswordPadlock:
                this._storedPassword = password;
                break;
            case GagPadlocks.FiveMinutesPadlock:
                break;
            case GagPadlocks.TimerPasswordPadlock:
                this._storedPassword = password;
                this._storedTimer = secondPassword;
                break;
            case GagPadlocks.MistressPadlock:
                // handle MistressPadlock case
                break;
            case GagPadlocks.MistressTimerPadlock:
                this._storedTimer = password;
                break;
        }
        return ValidatePadlockPasswords(false, _config, assignerPlayerName, targetPlayerName);
    }
    /// <summary>
    /// This function will serve as the primary function called by anyone who is wanting to create a password field for their padlock dropdown.
    /// <param name="padlock">The padlock type we have selected.</param>
    /// </summary>
    public bool DisplayPasswordField(GagPadlocks padlockType) {
        _padlockType = padlockType;
        switch (padlockType) {
            case GagPadlocks.CombinationPadlock:
                _inputCombination = DisplayInputField("##Combination_Input", "Enter 4 digit combination...", _inputCombination, 4);
                return true;
            case GagPadlocks.PasswordPadlock:
                _inputPassword = DisplayInputField("##Password_Input", "Enter password", _inputPassword, 20);
                return true;
            case GagPadlocks.TimerPasswordPadlock:
                _inputPassword = DisplayInputField("##Password_Input", "Enter password", _inputPassword, 20, 2 / 3f);
                ImGui.SameLine();
                _inputTimer = DisplayInputField("##Timer_Input", "Ex: 0h2m7s", _inputTimer, 12);
                return true;
            case GagPadlocks.MistressTimerPadlock:
                _inputTimer = DisplayInputField("##Timer_Input", "Ex: 0h2m7s", _inputTimer, 12);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// This function will serve as the primary function called by anyone who is wanting to create a password field for their padlock dropdown.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="hint"></param>
    /// <param name="value"></param>
    /// <param name="maxLength"></param>
    /// <param name="widthRatio"></param>
    /// <returns></returns>
    private string DisplayInputField(string id, string hint, string value, uint maxLength, float widthRatio = 1f) {
        string result = value;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * widthRatio);
        if (ImGui.InputTextWithHint(id, hint, ref result, maxLength, ImGuiInputTextFlags.None))
            return result;
        return value;
    }

    public bool ValidatePadlockPasswords(bool isUnlocking, GagSpeakConfig _config, string assignerPlayerName = null, string targetPlayerName = null) {
        bool ret = false;
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Validating password");
        switch (_padlockType) {
            case GagPadlocks.None:
                return false;
            case GagPadlocks.MetalPadlock:
                return true;
            case GagPadlocks.CombinationPadlock:
                ret = ValidateCombination();
                if(ret && !isUnlocking && _inputCombination != "") {_storedCombination = _inputCombination; _inputCombination = "";}
                return ret;
            case GagPadlocks.PasswordPadlock:
                ret = ValidatePassword();
                if(ret && !isUnlocking && _inputPassword != "") {_storedPassword = _inputPassword; _inputPassword = "";}
                return ret;
            case GagPadlocks.FiveMinutesPadlock:
                _storedTimer = "0h0m5s";
                return true;
            case GagPadlocks.TimerPasswordPadlock:
                ret = (ValidatePassword() && ValidateTimer());
                if(ret && !isUnlocking && _inputPassword != "" && _inputTimer != "") {
                    _storedPassword = _inputPassword;
                    _storedTimer = _inputTimer;
                    _inputPassword = "";
                    _inputTimer = "";}
                return ret;
            case GagPadlocks.MistressPadlock:
                ret = ValidateMistress(_config, assignerPlayerName, targetPlayerName);
                if(ret && !isUnlocking) {
                    _mistressAssignerName = assignerPlayerName;
                }
                return ret;
            case GagPadlocks.MistressTimerPadlock:
                ret = (ValidateMistress(_config, assignerPlayerName, targetPlayerName) && ValidateTimer());
                if(ret && !isUnlocking) { _mistressAssignerName = assignerPlayerName; }
                if(ret && !isUnlocking && _inputTimer != "") {
                    _storedTimer = _inputTimer;
                    _inputTimer = "";}
                return ret;
            default:
                return true;
        }
    }
    private bool ValidatePassword() {
        if(_inputPassword == "") {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingPassword from set&Validate [{_storedPassword}]");
            return !string.IsNullOrWhiteSpace(_storedPassword) && _storedPassword.Length <= 20 && !_storedPassword.Contains(" ");
        } else {   
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingPassword from DisplayPasswordField [{_inputPassword}]");
            return !string.IsNullOrWhiteSpace(_inputPassword) && _inputPassword.Length <= 20 && !_inputPassword.Contains(" ");
        }
    }
    private bool ValidateCombination() {
        if(_inputCombination == "") {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingCombination from set&Validate [{_storedCombination}]");
            return int.TryParse(_storedCombination, out _) && _storedCombination.Length == 4;
        } else {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingCombination from DisplayPasswordField [{_inputCombination}]");
            return int.TryParse(_inputCombination, out _) && _inputCombination.Length == 4;
        }
    }
    private bool ValidateTimer() {
        // Timers must be in the format of 00h00m00s
        if (_inputTimer == "") {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingTimer from set&Validate [{_storedTimer}]");
            var match = Regex.Match(_storedTimer, @"^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
            return match.Success;
        } else {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingTimer from DisplayPasswordField [{_inputTimer}]");
            var match = Regex.Match(_inputTimer, @"^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
            return match.Success;
        }
    }
    private bool ValidateMistress(GagSpeakConfig _config, string assignerPlayerName, string targetPlayerName) {
        GagSpeak.Log.Debug($"[PadlockIdentifer]: AssignedPlayerName: {assignerPlayerName}");
        GagSpeak.Log.Debug($"[PadlockIdentifer]: TargetPlayerName {targetPlayerName}");
        
        if(assignerPlayerName == null) {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: Assigner name is null!"); return false;}
        // if we are trying to assign it to ourselves, then we can just return true.
        if (assignerPlayerName == targetPlayerName || // if we are trying to assign it to ourselves, or are a mistress to whitelisted player
        _config.Whitelist.Any(w => assignerPlayerName.Contains(w.name) && w.relationshipStatus == "Mistress")) {
            return true;
        }
        GagSpeak.Log.Debug($"[PadlockIdentifer]: {assignerPlayerName} is not your mistress!");
        return false;
    }

    // check the password when attempting to unlock it.
    public bool CheckPassword(GagSpeakConfig _config, string assignerName = null, string targetName = null) {
        bool isValid = false;
        switch (_padlockType) {
            case GagPadlocks.None:
                return false;
            case GagPadlocks.MetalPadlock:
                return true;
            case GagPadlocks.CombinationPadlock:
                isValid = _storedCombination == _inputCombination;
                break;
            case GagPadlocks.PasswordPadlock:
                isValid = _storedPassword == _inputPassword;
                break;
            case GagPadlocks.TimerPasswordPadlock:
                isValid = _storedPassword == _inputPassword;
                break;
            case GagPadlocks.MistressPadlock:
                isValid = ValidateMistress(_config, assignerName, targetName);
                break;
            case GagPadlocks.MistressTimerPadlock:
                isValid = ValidateMistress(_config, assignerName, targetName);
                break;
            default:
                return false;
        }

        if (!isValid) {
            _inputPassword = "";
            _inputCombination = "";
            _inputTimer = "";
        }

        return isValid;
    }

    // Doing this we can use this just before updateconfig to use the update for unlock and lock functions
    public void ClearPasswords() {
    _inputPassword = "";
    _inputCombination = "";
    _inputTimer = "";
    _storedPassword = "";
    _storedCombination = "";
    _storedTimer = "";
    _mistressAssignerName = "";
    }
    
    // a way to update our password information in the config file. (For User Padlocks Only)
    public void UpdateConfigPadlockInfo(int layerIndex, bool isUnlocking, GagSpeakConfig _config) {
        GagPadlocks padlockType = _padlockType;
        if (isUnlocking) { _padlockType = GagPadlocks.None; GagSpeak.Log.Debug("[Padlock] Unlocking Padlock");}
        // timers are handled by the timer service so we dont need to worry about it.
        switch (padlockType) {
            case GagPadlocks.MetalPadlock:
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                break;
            case GagPadlocks.CombinationPadlock:
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                _config.selectedGagPadlocksPassword[layerIndex] = _storedCombination;
                break;
            case GagPadlocks.PasswordPadlock:
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                _config.selectedGagPadlocksPassword[layerIndex] = _storedPassword;
                break;
            case GagPadlocks.FiveMinutesPadlock:
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                break;
            case GagPadlocks.TimerPasswordPadlock:
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                _config.selectedGagPadlocksPassword[layerIndex] = _storedPassword;
                break;
            case GagPadlocks.MistressPadlock:
                // handle MistressPadlock case
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                _config.selectedGagPadlocksAssigner[layerIndex] = _mistressAssignerName;
                break;
            case GagPadlocks.MistressTimerPadlock:
                _config.selectedGagPadlocks[layerIndex] = _padlockType;
                _config.selectedGagPadlocksAssigner[layerIndex] = _mistressAssignerName;
                break;
            default:
                // No password field should be displayed
                break;
        }
    }
    public void UpdateWhitelistPadlockInfo(WhitelistCharData character, int layer, bool isUnlocking, GagSpeakConfig _config) {
        GagPadlocks padlockType = _padlockType;
        if (isUnlocking) { _padlockType = GagPadlocks.None; GagSpeak.Log.Debug("[Whitelist Padlock] Unlocking Padlock");}
        // timers are handled by the timer service so we dont need to worry about it.
        switch (padlockType) {
            case GagPadlocks.MetalPadlock:
                character.selectedGagPadlocks[layer] = _padlockType;
                break;
            case GagPadlocks.CombinationPadlock:
                character.selectedGagPadlocks[layer] = _padlockType;
                character.selectedGagPadlocksPassword[layer] = _storedCombination;
                break;
            case GagPadlocks.PasswordPadlock:
                character.selectedGagPadlocks[layer] = _padlockType;
                character.selectedGagPadlocksPassword[layer] = _storedPassword;
                break;
            case GagPadlocks.FiveMinutesPadlock:
                character.selectedGagPadlocks[layer] = _padlockType;
                break;
            case GagPadlocks.TimerPasswordPadlock:
                character.selectedGagPadlocks[layer] = _padlockType;
                character.selectedGagPadlocksPassword[layer] = _storedPassword;
                break;
            case GagPadlocks.MistressPadlock:
                // handle MistressPadlock case
                character.selectedGagPadlocks[layer] = _padlockType;
                character.selectedGagPadlocksAssigner[layer] = _mistressAssignerName;
                break;
            case GagPadlocks.MistressTimerPadlock:
                character.selectedGagPadlocks[layer] = _padlockType;
                character.selectedGagPadlocksAssigner[layer] = _mistressAssignerName;
                break;
            default:
                // No password field should be displayed
                break;
        }
    }
}
#pragma warning restore IDE1006  
