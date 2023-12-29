namespace GagSpeak.Data {
    /// <summary> Restrictive level of a certain property of a gag </summary>
    public enum RestrictedSoundLvl { 
        None,       // the sound comes out as it
        Partial,    // the sound is somewhat muffled, but you can hear part of the sound still
        Complete,   // the sound is mostly muffled or a different word comes out
        Silenced    // the sound literally is unable to be made
    };
    /// <summary> Padlock enum listing </summary>
    public enum GagPadlocks {
        None,                   // No gag
        MetalPadlock,           // Metal Padlock, can be picked
        CombinationPadlock,     // Combination Padlock, must enter 4 digit combo to unlock
        PasswordPadlock,        // Password Padlock, must enter password to unlock
        FiveMinutesPadlock,     // 5 minute padlock, must wait 5 minutes to unlock
        TimerPasswordPadlock,   // Timer Password Padlock, must enter password to unlock, but only after a certain amount of time
        MistressPadlock,        // Mistress Padlock, must ask mistress to unlock
        MistressTimerPadlock,   // Mistress Timer Padlock, must ask mistress to unlock, but only after a certain amount of time
    };
}