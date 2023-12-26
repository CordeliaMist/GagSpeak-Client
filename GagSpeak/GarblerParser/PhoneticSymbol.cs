using System.Collections.Generic;
using GagSpeak.Data;

namespace GagSpeak.Garbler.PhonemeData;

public class PhoneticSymbol
{
    public string           Phoneme { get; set; }
    public GagEnums.PhonemeType      Type { get; set; }
    public GagEnums.LipPos           LipPos { get; set; }
    public GagEnums.TonguePos        TonguePos { get; set; }
    public GagEnums.ConsonantType    ConsonantType { get; set; }
    public GagEnums.ConsonantPlace   ConsonantPlace { get; set; }
    public string           PartialMuffledSound { get; set; }
    public string           FullMuffledSound { get; set; }
    public string           TotalMuffledSound { get; set; }

    #region PhoneticSymbolConstructors
    // default constructor
    public PhoneticSymbol() {
        Phoneme = "";
        Type = GagEnums.PhonemeType.NotDefined;
        LipPos = GagEnums.LipPos.NotDefined;
        TonguePos = GagEnums.TonguePos.NotDefined;
        ConsonantType = GagEnums.ConsonantType.NotDefined;
        ConsonantPlace = GagEnums.ConsonantPlace.NotDefined;
        PartialMuffledSound = "m";
        FullMuffledSound = "m";
        TotalMuffledSound = "m";
    }

    public PhoneticSymbol(string phoneme) {
        Phoneme = phoneme;
        Type = GagEnums.PhonemeType.NotDefined;
        LipPos = GagEnums.LipPos.NotDefined;
        TonguePos = GagEnums.TonguePos.NotDefined;
        ConsonantType = GagEnums.ConsonantType.NotDefined;
        ConsonantPlace = GagEnums.ConsonantPlace.NotDefined;
        PartialMuffledSound = "m";
        FullMuffledSound = "m";
        TotalMuffledSound = "m";
    }

    // Constructor for vowels
    public PhoneticSymbol(string phoneme, GagEnums.LipPos lipPosition, GagEnums.TonguePos tonguePosition, string partialMuffledSound = "m", string fullMuffledSound ="m", string totalMuffledSound = "m"){
        // set the phoneme attributes
        Phoneme = phoneme;
        Type = GagEnums.PhonemeType.Vowel;
        LipPos = lipPosition;
        TonguePos = tonguePosition;
        ConsonantType = GagEnums.ConsonantType.NotDefined;
        ConsonantPlace = GagEnums.ConsonantPlace.NotDefined;
        // set the muffled sounds
        PartialMuffledSound = partialMuffledSound;
        FullMuffledSound = fullMuffledSound;
        TotalMuffledSound = totalMuffledSound;
    }

    // Constructor for consonants
    public PhoneticSymbol(string phoneme, GagEnums.ConsonantType consonantType, GagEnums.ConsonantPlace consonantPlace, string partialMuffledSound = "m", string fullMuffledSound ="m", string totalMuffledSound = "m") {
        // set the phoneme attributes
        Phoneme = phoneme;
        Type = GagEnums.PhonemeType.Consonant;
        ConsonantType = consonantType;
        ConsonantPlace = consonantPlace;
        LipPos = GagEnums.LipPos.NotDefined;
        TonguePos = GagEnums.TonguePos.NotDefined;
        // set the muffled sounds
        PartialMuffledSound = partialMuffledSound;
        FullMuffledSound = fullMuffledSound;
        TotalMuffledSound = totalMuffledSound;
    }
    #endregion PhoneticSymbolConstructors
}

public static class PhonemMasterLists {
    /// <summary>
    /// The list of all the phonemes that can be garbled (English List) (refer to the spreadsheet maybe)
    /// </summary>
    public static List<PhoneticSymbol> MasterListEN_US = new List<PhoneticSymbol>
    {
        new PhoneticSymbol("p"),    // EX: "p" in "pet", or "pot"                   Bilabial    Plosive
        new PhoneticSymbol("b"),    // EX: "b" in "bat", "but", or "web"            Bilabial    Plosive
        new PhoneticSymbol("m"),    // EX: "m" in man, "ham", or "summer"           Bilabial    Nasal
        new PhoneticSymbol("w"),    // EX: "w" in "we", "will", or "away"           Bilabial    Approximant
        new PhoneticSymbol("f"),    // EX: "f" in "fit", "off", or "leaf"           Labiodental Fricative
        new PhoneticSymbol("v"),    // EX: "v" in "vase", "have", or "love"         Labiodental Fricative
        new PhoneticSymbol("θ"),    // EX: "th" in "think", "bath", or "both"       Dental      Fricative
        new PhoneticSymbol("ð"),    // EX: "th" in "this", "that", or "there"       Dental      Fricative
        new PhoneticSymbol("t"),    // EX: "t" in "top", "stop", or "bet"           Alveolar    Plosive
        new PhoneticSymbol("d"),    // EX: "d" in "dog", "bed", or "bad"            Alveolar    Plosive
        new PhoneticSymbol("s"),    // EX: "s" in "sit", "miss", or "bus"           Alveolar    Fricative
        new PhoneticSymbol("z"),    // EX: "z" in "zoo", "buzz", or "jazz"          Alveolar    Fricative
        new PhoneticSymbol("n"),    // EX: "n" in "no", "pen", or "thin"            Alveolar    Nasal
        new PhoneticSymbol("ɫ"),    // EX: "l" in "feel", "milk", or "full"         Alveolar    Lateral
        new PhoneticSymbol("ɹ"),    // EX: "r" in "red", "car", or "four"           Alveolar    Approximant
        new PhoneticSymbol("ʃ"),    // EX: "sh" in "shut", "shoe", or "nation"      Palato-Alveolar Fricative
        new PhoneticSymbol("ʒ"),    // EX: "s" in "pleasure", "vision", or "usual"  Palato-Alveolar Fricative
        new PhoneticSymbol("dʒ"),   // EX: "j" in "jam", "jelly", or "jump"         Palato-Alveolar Affricate
        new PhoneticSymbol("tʃ"),   // EX: "ch" in "chat", "chance", or "catch"     Palato-Alveolar Affricate
        new PhoneticSymbol("j"),    // EX: "y" in "yes", "yellow", or "you"         Palatal     Approximant
        new PhoneticSymbol("k"),    // EX: "k" in "cat", "kite", or "back"          Velar       Plosive
        new PhoneticSymbol("ɡ"),    // EX: "g" in "got", "egg", or "bag"            Velar       Plosive
        new PhoneticSymbol("ŋ"),    // EX: "ng" in "sing", "song", or "long"        Velar       Nasal
        new PhoneticSymbol("h"),    // EX: "h" in "hat", "ahead", or "who"          Glottal     Fricative
        new PhoneticSymbol("eɪ"),   // EX: "ay" in "say", "day", or "may"           Vowels from here on down   
        new PhoneticSymbol("ə"),    // EX: "a" in "about", "above", or "around"            
        new PhoneticSymbol("ɔ"),    // EX: "aw" in "s/aw/", "/a/ll", or "b/ou/ght"     
        new PhoneticSymbol("æ"),    // EX: "a" in "cat", "hat", or "bad"            
        new PhoneticSymbol("i"),    // EX: "ee" in "see", "tree", or "bee"
        new PhoneticSymbol("ɛ"),    // EX: "e" in "bet", "let", or "get"
        new PhoneticSymbol("ɝ"),    // EX: "er" in "her", "bird", or "fur"
        new PhoneticSymbol("ɪə"),   // EX: "ear" in "near", "here", or "beer"
        new PhoneticSymbol("aɪ"),   // EX: "i" in "my", "fly", or "why"
        new PhoneticSymbol("ɪ"),    // EX: "i" in "sit", "bit" or "kit"
        new PhoneticSymbol("oʊ"),   // EX: "o" in "boat", "go", or "no"
        new PhoneticSymbol("u"),    // EX: "oo" in "boot", "food", or "you"
        new PhoneticSymbol("ɑ"),    // EX: "o" in "bob"
        new PhoneticSymbol("ʊ"),    // EX: "oo" in "book", "put", or "foot"
        new PhoneticSymbol("aʊ"),   // EX: "ow" in "now", or "ou" in "about"
        new PhoneticSymbol("ɔɪ"),   // EX: "oy" in "boy", "toy", or "joy"
    };

    /// <summary>
    /// Very much likely incorrect, was not parsed properly at first, and the :'s were removed during parsing so very likely off
    /// </summary>
    public static List<PhoneticSymbol> MasterListEN_UK = new List<PhoneticSymbol>
    {
        new PhoneticSymbol("b"),    // Bilabial Plosive: "b" in "bat" or "web"
        new PhoneticSymbol("t"),    // Alveolar Plosive: "t" in "top" or "bet"
        new PhoneticSymbol("k"),    // Velar Plosive: "k" in "cat" or "back"
        new PhoneticSymbol("z"),    // Alveolar Fricative: "z" in "zoo" or "jazz"
        new PhoneticSymbol("ɹ"),    // Alveolar Approximant: "r" in "red" or "car"
        new PhoneticSymbol("s"),    // Alveolar Fricative: "s" in "sit" or "miss"
        new PhoneticSymbol("j"),    // Palatal Approximant: "y" in "yes" or "yellow"
        new PhoneticSymbol("m"),    // Bilabial Nasal: "m" in "man" or "summer"
        new PhoneticSymbol("f"),    // Labiodental Fricative: "f" in "fit" or "off"
        new PhoneticSymbol("n"),    // Alveolar Nasal: "n" in "no" or "pen"
        new PhoneticSymbol("w"),    // Bilabial Approximant: "w" in "we" or "will"
        new PhoneticSymbol("p"),    // Bilabial Plosive: "p" in "pet" or "pot"
        new PhoneticSymbol("ɡ"),    // Velar Plosive: "g" in "got" or "bag"
        new PhoneticSymbol("ɫ"),    // Alveolar Lateral: "l" in "feel" or "milk"
        new PhoneticSymbol("θ"),    // Dental Fricative: "th" in "think" or "bath"
        new PhoneticSymbol("v"),    // Labiodental Fricative: "v" in "vase" or "have"
        new PhoneticSymbol("h"),    // Glottal Fricative: "h" in "hat" or "ahead"
        new PhoneticSymbol("ɝ"),    // Mid-Central Rounded Vowel: "er" in "her" or "bird"
        new PhoneticSymbol("ʃ"),    // Palato-Alveolar Fricative: "sh" in "shut" or "shoe"
        new PhoneticSymbol("ʒ"),    // Palato-Alveolar Fricative: "s" in "pleasure" or "vision"
        new PhoneticSymbol("dʒ"),   // Palato-Alveolar Affricate: "j" in "jam" or "jelly"
        new PhoneticSymbol("tʃ"),   // Palato-Alveolar Affricate: "ch" in "chat" or "chance"
        new PhoneticSymbol("i"),    // Close Front Unrounded Vowel: "ee" in "see" or "tree"
        new PhoneticSymbol("ɪ"),    // Near-Close Near-Front Unrounded Vowel: "i" in "sit" or "kit"
        new PhoneticSymbol("u"),    // Close Back Rounded Vowel: "oo" in "boot" or "you"
        new PhoneticSymbol("oʊ"),   // Diphthong: "o" in "boat" or "go"
        new PhoneticSymbol("ʊ"),    // Near-Close Near-Back Rounded Vowel: "oo" in "book" or "put"
        new PhoneticSymbol("eɪ"),   // Diphthong: "ay" in "say" or "day"
        new PhoneticSymbol("ɛ"),    // Open-Mid Front Unrounded Vowel: "e" in "bet" or "get"
        new PhoneticSymbol("ɔ"),    // Open-Mid Back Rounded Vowel: "aw" in "saw" or "bought"
        new PhoneticSymbol("æ"),    // Near-Open Front Unrounded Vowel: "a" in "cat" or "hat"
        new PhoneticSymbol("ɑ"),    // Open Back Unrounded Vowel: "o" in "bob"
        new PhoneticSymbol("ɪə"),   // Diphthong: "ear" in "near" or "here"
        new PhoneticSymbol("aɪ"),   // Diphthong: "i" in "my" or "fly"
        new PhoneticSymbol("ɔɪ")    // Diphthong: "oy" in "boy" or "toy"
    };
}