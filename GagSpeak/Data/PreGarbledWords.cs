using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using System.Text.RegularExpressions;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GagSpeak.Services;

namespace GagSpeak.Data {
    
    // public enum PhonemeType { NotDefined, Vowel, Consonant }
    // public enum LipPos { NotDefined, Close, NearClose, CloseMid, Mid, OpenMid, NearOpen, Open }
    // public enum TonguePos { NotDefined, Front, NearFront, Central, NearBack, Back }
    // public enum ConsonantType { NotDefined, Plosive, Nasal, Trill, TapOrFlap, Fricative, LateralFricative, Approximant, LateralApproximant }
    // public enum ConsonantPlace { NotDefined, Bilabial, LabioDental, Dental, Alveolar, PostAlveolar, Retroflex, Palatal, Velar, Uvular, Pharyngeal, Glottal }
    // public enum RestrictionLvl { None, Partial, Complete, Silenced }

    
    // phonetic definitions
    public class PhoneticSymbol {
        public string           Phoneme { get; set; }
        public PhonemeType      Type { get; set; }
        public LipPos           LipPos { get; set; }
        public TonguePos        TonguePos { get; set; }
        public ConsonantType    ConsonantType { get; set; }
        public ConsonantPlace   ConsonantPlace { get; set; }
        public string           PartialMuffledSound { get; set; }
        public string           FullMuffledSound { get; set; }
        public string           TotalMuffledSound { get; set; }

        #region PhoneticSymbolConstructors
        // default constructor
        public PhoneticSymbol(string phoneme) {
            Phoneme = phoneme;
            Type = PhonemeType.NotDefined;
            LipPos = LipPos.NotDefined;
            TonguePos = TonguePos.NotDefined;
            ConsonantType = ConsonantType.NotDefined;
            ConsonantPlace = ConsonantPlace.NotDefined;
            PartialMuffledSound = "m";
            FullMuffledSound = "m";
            TotalMuffledSound = "m";
        }

        // Constructor for vowels
        public PhoneticSymbol(string phoneme, LipPos lipPosition, TonguePos tonguePosition, string partialMuffledSound = "m", string fullMuffledSound ="m", string totalMuffledSound = "m"){
            // set the phoneme attributes
            Phoneme = phoneme;
            Type = PhonemeType.Vowel;
            LipPos = lipPosition;
            TonguePos = tonguePosition;
            ConsonantType = ConsonantType.NotDefined;
            ConsonantPlace = ConsonantPlace.NotDefined;
            // set the muffled sounds
            PartialMuffledSound = partialMuffledSound;
            FullMuffledSound = fullMuffledSound;
            TotalMuffledSound = totalMuffledSound;
        }

        // Constructor for consonants
        public PhoneticSymbol(string phoneme, ConsonantType consonantType, ConsonantPlace consonantPlace, string partialMuffledSound = "m", string fullMuffledSound ="m", string totalMuffledSound = "m") {
            // set the phoneme attributes
            Phoneme = phoneme;
            Type = PhonemeType.Consonant;
            ConsonantType = consonantType;
            ConsonantPlace = consonantPlace;
            LipPos = LipPos.NotDefined;
            TonguePos = TonguePos.NotDefined;
            // set the muffled sounds
            PartialMuffledSound = partialMuffledSound;
            FullMuffledSound = fullMuffledSound;
            TotalMuffledSound = totalMuffledSound;
        }
        #endregion PhoneticSymbolConstructors
    }

    public class GagManager : IDisposable
    {
        private readonly    GagSpeakConfig  _config;
        private readonly    GagService      _gagService;    
        public              List<Gag>       activeGags;

        public GagManager(GagSpeakConfig config, GagService gagService) {
            _config = config;
            _gagService = gagService;
            // get the IGag class type from the value of the Dictionary<string, IGag> GagTypes, where it searched to see if the key defined by selectedGagTypes is in the dictionary
            activeGags = _config.selectedGagTypes.Select(gagType => _gagService.GagTypes[gagType]).ToList();
            // subscribe to our events
            _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
            //_config.phoneticSymbolList.ItemChanged += OnPhoneticSymbolListChanged;
        }

        public void Dispose() {
            _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
            //_config.phoneticSymbolList.ItemChanged -= OnPhoneticSymbolListChanged;
        }

        private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
            activeGags = _config.selectedGagTypes.Select(gagType => _gagService.GagTypes[gagType]).ToList();
        }

        // private void OnPhoneticSymbolListChanged(object sender, ItemChangedEventArgs e) {

        // this will change eventually to do the full process. Likely tomorrow though™
        public string ProcessMessage(string IPAspacedMessage) {
            string outputStr = "";
            try {
                outputStr = activeGags[0].ConvertIPAtoGagSpeak(IPAspacedMessage);
            } catch (Exception e) {
                GagSpeak.Log.Error($"Error processing message with gag {activeGags[0].Name}: {e.Message}");
            }
            return outputStr;
        }
    }


    public class Gag
    {
        private readonly    GagSpeakConfig          _config;                            // The GagSpeak configuration
        public              string                  Name { get; set; }                  // Name of the gag
        public              LipPos                  LipPos { get; set; }           // What Position has the gag locked the lips in?
        public              RestrictionLvl          LipRestriction { get; set; }        // Restriction of the lips
        public              TonguePos               TonguePos { get; set; }        // Restricted Position of the tongue caused by the gag
        public              RestrictionLvl          TongueRestriction { get; set; }     // How restricted the tongue is by the gag. How much can it move?
        public              RestrictionLvl          JawRestriction { get; set; }        // Restriction of the jaw, how much can it open?
        public              RestrictionLvl          PackedMouthSeverity { get; set; }   // Severity of the packed mouth, how stuffed full is it? (useful for pump gags)
        private             List<RestrictionLvl>    PhoneticRestrictions;               // List of the restriction severity for each phoneme in the current phoneme master list

        public Gag(string name, LipPos lipPosition, RestrictionLvl lipRestriction, TonguePos tonguePosition,
        RestrictionLvl tongueRestriction, RestrictionLvl jawRestriction, RestrictionLvl packedMouthSeverity, GagSpeakConfig config) {
            _config = config;
            // set the attributes
            Name = name;
            LipPos = lipPosition;
            LipRestriction = lipRestriction;
            TonguePos = tonguePosition;
            TongueRestriction = tongueRestriction;
            JawRestriction = jawRestriction;
            PackedMouthSeverity = packedMouthSeverity;
            // set up the restrictions
            PhoneticRestrictions = new List<RestrictionLvl>();
            // initialize the restrictions based on the currently selected pheonme master list
            InitializePhoneticRestrictions();
        }

        /// <summary>
        /// Initializes the PhoneticRestrictions list based on the currently selected phoneme master list
        /// </summary>
        public void InitializePhoneticRestrictions() {
            foreach (var phoneticSymbol in _config.phoneticSymbolList) {
                PhoneticRestrictions.Add(CalculateRestriction(phoneticSymbol));
            }
        }

        /// <summary>
        /// Refreshes the restricted list identifiers based on the currently selected phoneme master list after it is updated. Should be called by a server/event
        /// </summary>
        public void RefreshPhoneticRestrictions() {
            PhoneticRestrictions.Clear();
            InitializePhoneticRestrictions();
        }

        public RestrictionLvl CalculateRestriction(PhoneticSymbol phoneticSymbol) {
            // Logic to calculate the RestrictionLvl based on the phonetic symbol and the gag attributes
            // This will likely involve a series of if/else or switch statements to check the phonetic symbol's properties and the gag's properties
            // and return the appropriate RestrictionLvl
            return RestrictionLvl.None;
        }

        /// <summary>
        /// Converts the given IPA spaced message into a gagged message based on the current gag's attributes
        /// </summary>
        /// <param name="IPAspacedMessage"></param> Message will be structured like "h-ɛ-m-ɪ-ŋ-ɝ". The hyphens are simply to seperate the phonems, and are removed after they are iterated through
        /// <returns></returns>
        public string ConvertIPAtoGagSpeak(string IPAspacedMessage) {
            string output = "";
            string[] words = IPAspacedMessage.Split(' ');

            foreach (string word in words) {
                string[] phonemes = word.Split('-');

                foreach (string phoneme in phonemes) {
                    int index = _config.phoneticSymbolList.FindIndex(symbol => symbol.Phoneme == phoneme);
                    RestrictionLvl restriction = PhoneticRestrictions[index];
                    // switch case based on restriction level to determine mufflesound
                    switch(restriction) {
                        case RestrictionLvl.None:
                            output += phoneme;
                            break;
                        case RestrictionLvl.Partial:
                            output += _config.phoneticSymbolList[index].PartialMuffledSound;
                            break;
                        case RestrictionLvl.Complete:
                            output += _config.phoneticSymbolList[index].FullMuffledSound;
                            break;
                        case RestrictionLvl.Silenced:
                            output += _config.phoneticSymbolList[index].TotalMuffledSound;
                            break;
                    }
                }
                output += " ";
            }

            return output.TrimEnd();
        }
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
}

/*
        /// <summary>
        /// The list of all the phonemes that can be garbled
        /// </summary>
        public static List<PhoneticSymbol> MasterList = new List<PhoneticSymbol>
        {
            new PhoneticSymbol("ˈb"),   // EX: "b" in "bat", "but", or "web"            // boundary b
            new PhoneticSymbol("aʊ"),   // EX: "ow" in "now", or "ou" in "about"
            new PhoneticSymbol("t"),    // EX: "t" in "top", "stop", or "bet" 
            new PhoneticSymbol("k"),    // EX: "k" in "cat", "kite", or "back"
            new PhoneticSymbol("ə"),    // EX: "a" in "about", "above", or "around"
            new PhoneticSymbol("z"),    // EX: "z" in "zoo", "buzz", or "jazz"
            new PhoneticSymbol("ˈk"),   // EX: "c" in "cat", "kite", or "back"          // boundary k
            new PhoneticSymbol("ɔ"),    // EX: "aw" in "s/aw/", "/a/ll", or "b/ou/ght"
            new PhoneticSymbol("ɹ"),    // EX: "r" in "red", "car", or "four"
            new PhoneticSymbol("s"),    // EX: "s" in "sit", "miss", or "bus"
            new PhoneticSymbol("j"),    // EX: "y" in "yes", "yellow", or "you"
            new PhoneticSymbol("u"),    // EX: "oo" in "boot", "food", or "you"
            new PhoneticSymbol("m"),    // EX: "m" in man, "ham", or "summer"
            new PhoneticSymbol("ˈf"),   // EX: "f" in "fat", "off", or "cou/gh/"        // boundary f
            new PhoneticSymbol("ɪ"),    // EX: "i" in "sit", "bit" or "kit"
            new PhoneticSymbol("oʊ"),   // EX: "o" in "boat", "go", or "no"
            new PhoneticSymbol("ʊ"),    // EX: "oo" in "book", "put", or "foot"
            new PhoneticSymbol("ˈɡ"),   // EX: "g" in "got", "egg", or "bag"            // boundary g
            new PhoneticSymbol("ɛ"),    // EX: "e" in "bet", "let", or "get"
            new PhoneticSymbol("n"),    // EX: "n" in "no", "pen", or "thin"
            new PhoneticSymbol("eɪ"),   // EX: "ay" in "say", "day", or "may"
            new PhoneticSymbol("ˈɹ"),   // EX: "r" in "red", "car", or "four"           // boundary r
            new PhoneticSymbol("d"),    // EX: "d" in "dog", "bed", or "bad"
            new PhoneticSymbol("ˈɛ"),   // EX: "e" in "bet", "let", or "get"            // boundary e
            new PhoneticSymbol("ˈt"),   // EX: "t" in "top", "stop", or "bet"           // boundary t
            new PhoneticSymbol("ɫ"),    // EX: "l" in "tell", "fall", or "tall"
            new PhoneticSymbol("w"),    // EX: "w" in "wet", "well", "swim" or "we"
            new PhoneticSymbol("ˈe"),   // EX: "a" in "about", "above", or "around"     // boundary e
            new PhoneticSymbol("ˌe"),   // EX: "a" in "about", "above", or "around"     // 2nd boundary e
            new PhoneticSymbol("ˈd"),   // EX: "d" in "dog", "bed", or "bad"            // boundary d
            new PhoneticSymbol("i"),    // EX: "i" in "sit", "bit" or "kit"
            new PhoneticSymbol("ˌt"),   // EX: "t" in "top", "stop", or "bet"           // 2nd boundary t
            new PhoneticSymbol("p"),    // EX: "p" in "pet", or "pot"
            new PhoneticSymbol("ˈɫ"),   // EX: "l" in "tell", "fall", or "tall"         // boundary l
            new PhoneticSymbol("ˈɑ"),   // EX: "a" in "about", "above", or "around"     // boundary a
            new PhoneticSymbol("b"),    // EX: "b" in "bat", "but", or "web"
            new PhoneticSymbol("ɝ"),    // EX: "er" in "her", "bird", or "fur"
            new PhoneticSymbol("ɡ"),    // EX: "g" in "got", "egg", or "bag"
            new PhoneticSymbol("ˌɑ"),   // EX: "o" in "hot"                             // 2nd boundary a
            new PhoneticSymbol("ɑ"),    // EX: "o" in "bob"
            new PhoneticSymbol("θ"),    // EX: "th" in "think", "bath", or "both"
            new PhoneticSymbol("ˌk"),   // EX: "c" in "cat", "kite", or "back"          // 2nd boundary k
            new PhoneticSymbol("ˌv"),   // EX: "v" in "vet", "have", or "of"            // 2nd boundary v
            new PhoneticSymbol("ˈh"),   // EX: "h" in "hat", "behind", or "who"         // boundary h
            new PhoneticSymbol("ˈæ"),   // EX: "a" in "cat", "hat", or "bad"            // boundary a
            new PhoneticSymbol("ˌb"),   // EX: "b" in "bat", "but", or "web"            // 2nd boundary b
            new PhoneticSymbol("æ"),    // EX: "a" in "cat", "hat", or "bad"
            new PhoneticSymbol("ˌæ"),   // EX: "a" in "cat", "hat", or "bad"            // 2nd boundary a
            new PhoneticSymbol("ŋ"),    // EX: "ng" in "sing", "song", or "long"
            new PhoneticSymbol("ʃ"),    // EX: "sh" in "shut", "shoe", or "nation"
            new PhoneticSymbol("ʒ"),    // EX: "s" in "pleasure", "vision", or "usual"
            new PhoneticSymbol("ˈw"),   // EX: "w" in "wet", "well", "swim" or "we"     // boundary w
            new PhoneticSymbol("ˌh"),   // EX: "h" in "hat", "behind", or "who"         // 2nd boundary h
            new PhoneticSymbol("v"),    // EX: "v" in "vat", "have", or "of"
            new PhoneticSymbol("ˈs"),   // EX: "s" in "sit", "miss", or "bus"           // boundary s
            new PhoneticSymbol("ˌs"),   // EX: "s" in "sit", "miss", or "bus"           // 2nd boundary s
            new PhoneticSymbol("ˌd"),   // EX: "d" in "dog", "bed", or "bad"            // 2nd boundary d
            new PhoneticSymbol("ˈz"),   // EX: "z" in "zoo", "buzz", or "jazz"          // boundary z
            new PhoneticSymbol("ˌɫ"),   // EX: "l" in "tell", "fall", or "tall"         // 2nd boundary l
            new PhoneticSymbol("f"),    // EX: "f" in "fat", "off", or "cou/gh/"
            new PhoneticSymbol("ˌn"),   // EX: "n" in "no", "pen", or "thin"            // 2nd boundary n
            new PhoneticSymbol("aɪ"),   // EX: "i" in "bite", "light", or "pie"
            new PhoneticSymbol("ˌɡ"),   // EX: "g" in "got", "egg", or "bag"            // 2nd boundary g
            new PhoneticSymbol("ˈm"),   // EX: "m" in "man", "ham", or "summer"         // boundary m
            new PhoneticSymbol("ˈo"),   // EX: "o" in "boat", "only", or "no"           // boundary o
            new PhoneticSymbol("ˌə"),   // EX: "a" in "about", "above", or "around"     // 2nd boundary e
            new PhoneticSymbol("ˈn"),   // EX: "n" in "no", "pen", or "thin"            // boundary n
            new PhoneticSymbol("h"),    // EX: "h" in "hat", "behind", or "who"
            new PhoneticSymbol("ˈɪ"),   // EX: "i" in "sit", "bit" or "kit"             // boundary i
            new PhoneticSymbol("ˌʃ"),   // EX: "sh" in "shut", "shoe", or "nation"      // 2nd boundary sh
            new PhoneticSymbol("ˈj"),   // EX: "y" in "yes", "yellow", or "you"         // boundary y
            new PhoneticSymbol("ˌz"),   // EX: "z" in "zoo", "buzz", or "jazz"          // 2nd boundary z
            new PhoneticSymbol("ð"),    // EX: "th" in "this", "that", or "other"
            new PhoneticSymbol("ˈp"),   // EX: "p" in "pet", or "pot"                   // boundary p
            new PhoneticSymbol("ˌa"),   // EX: "a" in "cat", "hat", or "bad"            // 2nd boundary a
            new PhoneticSymbol("ˌm"),   // EX: "m" in "man", "ham", or "summer"         // 2nd boundary m
            new PhoneticSymbol("ˌj"),   // EX: "y" in "yes", "yellow", or "you"         // 2nd boundary y
            new PhoneticSymbol("ˈθ"),   // EX: "th" in "think", "bath", or "both"       // boundary th
            new PhoneticSymbol("ˈv"),   // EX: "v" in "vat", "have", or "of"            // boundary v
            new PhoneticSymbol("ˌf"),   // EX: "f" in "fat", "off", or "cou/gh/"        // 2nd boundary f
            new PhoneticSymbol("ɔɪ"),   // EX: "oy" in "boy", "toy", or "coin"
            new PhoneticSymbol("ˌɹ"),   // EX: "r" in "red", "car", or "four"           // 2nd boundary r
            new PhoneticSymbol("ˌp"),   // EX: "p" in "pet", or "pot"                   // 2nd boundary p
            new PhoneticSymbol("ˌo"),   // EX: "o" in "boat", "only", or "no"           // 2nd boundary o
            new PhoneticSymbol("ˈi"),   // EX: "i" in "bite", "light", or "pie"         // boundary i
            new PhoneticSymbol("ˌɛ"),   // EX: "e" in "bet", "let", or "get"            // 2nd boundary e
            new PhoneticSymbol("ˌɪ"),   // EX: "i" in "sit", "bit" or "kit"             // 2nd boundary i
            new PhoneticSymbol("ˌw"),   // EX: "w" in "wet", "well", "swim" or "we"     // 2nd boundary w
            new PhoneticSymbol("ˌθ"),   // EX: "th" in "think", "bath", or "both"       // 2nd boundary th
            new PhoneticSymbol("ˈʃ"),   // EX: "sh" in "shut", "shoe", or "nation"      // boundary sh
            new PhoneticSymbol("ɪə"),   // EX: "ear" in "ear", "hear", or "clear"
            new PhoneticSymbol("ˈə"),   // EX: "a" in "about", "above", or "around"     // boundary e
            new PhoneticSymbol("ˈa"),   // EX: "a" in "cat", "hat", or "bad"            // boundary a
            new PhoneticSymbol("ˈɔ"),   // EX: "aw" in "s/aw/", "/a/ll", or "b/ou/ght"  // boundary aw
            new PhoneticSymbol("ˌɔ"),   // EX: "aw" in "s/aw/", "/a/ll", or "b/ou/ght"  // 2nd boundary aw
            new PhoneticSymbol("ˈu"),   // EX: "oo" in "boot", "food", or "you"         // boundary u
            new PhoneticSymbol("ˈð"),   // EX: "th" in "this", "that", or "other"       // boundary th
            new PhoneticSymbol("ˌi"),   // EX: "i" in "bite", "light", or "pie"         // 2nd boundary i
            new PhoneticSymbol("ʊə"),   // EX: "oor" in "poor", "tour", or "sure"
            new PhoneticSymbol("ˈʒ"),   // EX: "s" in "pleasure", "vision", or "usual"  // boundary s
            new PhoneticSymbol("ˌʒ"),   // EX: "s" in "pleasure", "vision", or "usual"  // 2nd boundary s
            new PhoneticSymbol("ˈɝ"),   // EX: "er" in "her", "bird", or "fur"          // boundary er
            new PhoneticSymbol("ˌu"),   // EX: "oo" in "boot", "food", or "you"         // 2nd boundary u
            new PhoneticSymbol("ˌŋ"),   // EX: "ng" in "sing", "song", or "long"        // 2nd boundary ng
            new PhoneticSymbol("ˌɝ"),   // EX: "er" in "her", "bird", or "fur"          // 2nd boundary er
            new PhoneticSymbol("ˈŋ"),   // EX: "ng" in "sing", "song", or "long"        // boundary ng
            new PhoneticSymbol("ˌð"),   // EX: "th" in "this", "that", or "other"       // 2nd boundary th
            new PhoneticSymbol("ˈʊ")    // EX: "oo" in "book", "put", or "foot"         // boundary u
        };
*/