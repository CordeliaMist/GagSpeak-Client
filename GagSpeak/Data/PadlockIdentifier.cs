using System;
using GagSpeak.Services;
using ImGuiNET;
using System.Text.RegularExpressions;
using System.Timers;


#pragma warning disable IDE1006 
namespace GagSpeak.Data;
// a struct to hold information on whitelisted players.
public class PadlockIdentifier
{
    private readonly GagSpeakConfig _config;
    private readonly TimerService _timerService;
    public string _storedPassword { get; private set; } // 20 character max string
    public string _storedCombination { get; private set; } // This will be a string in the format of 0000
    public string _storedTimer { get; private set; } // This will be a string in the format of 00h00m00s
    public GagPadlocks _padlockType { get; private set; } = GagPadlocks.None;

    public PadlockIdentifier(GagSpeakConfig config, TimerService timerService)
    {
        _config = config;
        _timerService = timerService;
    }

    /// <summary>
    /// This function is used typically by the command manager, for when we have no field to insert
    /// a password to, and do it directly instead.
    /// <param name="locktype"></param>
    /// <param name="password"></param>
    /// </summary>
    public bool SetAndValidate(string locktype, string password = null)
    {
        if (!Enum.TryParse(locktype, true, out GagPadlocks padlockType))
            return false; // or throw an exception
        
        switch (_padlockType)
        {
            case GagPadlocks.CombinationPadlock:
                this._storedCombination = password;
                break;
            case GagPadlocks.PasswordPadlock:
                this._storedPassword = password;
                break;
            case GagPadlocks.TimerPasswordPadlock:
                this._storedTimer = password;
                break;
            case GagPadlocks.MistressPadlock:
                // handle MistressPadlock case
                break;
            case GagPadlocks.MistressTimerPadlock:
                // handle MistressTimerPadlock case
                break;
        }

        return ValidatePadlockPasswords();
    }
    /// <summary>
    /// This function will serve as the primary function called by anyone who is wanting to create a password field for their padlock dropdown.
    /// <param name="padlock">The padlock type we have selected.</param>
    /// </summary>
    public void DisplayPasswordField(GagPadlocks padlockType) 
    {
        _padlockType = padlockType;
        switch (padlockType) 
        {
            case GagPadlocks.CombinationPadlock:
                _storedCombination = DisplayInputField("##Combination_Input", "Enter 4 digit combination...", _storedCombination, 4);
                break;
            case GagPadlocks.PasswordPadlock:
                _storedPassword = DisplayInputField("##Password_Input", "Enter password", _storedPassword, 20);
                break;
            case GagPadlocks.TimerPasswordPadlock:
                _storedPassword = DisplayInputField("##Password_Input", "Enter password", _storedPassword, 20, 2 / 3f);
                _storedTimer = DisplayInputField("##Timer_Input", "[]h[]m[]s. ~Ex: 0h10m15s", _storedTimer, 10, 1 / 3f);
                break;
            case GagPadlocks.MistressTimerPadlock:
                _storedTimer = DisplayInputField("##Timer_Input", "Enter timer", _storedTimer, 10);
                break;
            default:
                // No password field should be displayed
                break;
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

    public bool ValidatePadlockPasswords() {
        switch (_padlockType) {
            case GagPadlocks.CombinationPadlock:
                return ValidateCombination();
            case GagPadlocks.PasswordPadlock:
                return ValidatePassword();
            case GagPadlocks.TimerPasswordPadlock:
                return ValidatePassword() && ValidateTimer();
            case GagPadlocks.MistressPadlock:
                return ValidateMistress();
            case GagPadlocks.MistressTimerPadlock:
                return ValidateMistress() && ValidateTimer();
            default:
                return false;
        }
    }
    private bool ValidatePassword() {
        // Passwords must be less than 20 characters and cannot contain spaces
        return !string.IsNullOrWhiteSpace(_storedPassword) && _storedPassword.Length <= 20 && !_storedPassword.Contains(" ");
    }
    private bool ValidateCombination() {
        // Combinations must be 4 digits
        return int.TryParse(_storedCombination, out _) && _storedCombination.Length == 4;
    }
    private bool ValidateTimer() {
        // Timers must be in the format of 00h00m00s
        var match = Regex.Match(_storedTimer, @"^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
        return match.Success;
    }
    private bool ValidateMistress() {
        // Replace this with the actual logic to validate the mistress
        return true;
    }


}
#pragma warning restore IDE1006  

// you just finished making this class, please start applying it into the general tab and the whitelist tab.
// Additionally, set the checks for request and accepts.
