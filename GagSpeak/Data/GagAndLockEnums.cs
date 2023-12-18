namespace GagSpeak.Data {
    /// <summary> Enum for the phoneme types </summary>
    public enum PhonemeType { 
        NotDefined,
        Vowel,
        Consonant
    }
    /// <summary> Enum for the lip positions </summary>
    public enum LipPos {
        NotDefined,
        Close,
        NearClose,
        CloseMid,
        Mid,
        OpenMid,
        NearOpen,
        Open
    }
    /// <summary> Enum for the tongue positions. </summary>
    public enum TonguePos {
        NotDefined,
        Front,
        NearFront,
        Central,
        NearBack,
        Back
    }
    /// <summary> Enum for the consonant types </summary>
    public enum ConsonantType {
        NotDefined,
        Plosive,
        Nasal,
        Trill,
        TapOrFlap,
        Fricative,
        LateralFricative,
        Approximant,
        LateralApproximant
    }
    /// <summary> Enum for the consonant places </summary>
    public enum ConsonantPlace {
        NotDefined,
        Bilabial,
        LabioDental,
        Dental,
        Alveolar,
        PostAlveolar,
        Retroflex,
        Palatal,
        Velar,
        Uvular,
        Pharyngeal,
        Glottal
    }
    /// <summary> Restrictive level of a certain property of a gag </summary>
    public enum RestrictionLvl { 
        None,
        Light,
        Partial,
        Complete,
        Silenced
    }
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