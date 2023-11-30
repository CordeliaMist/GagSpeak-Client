using System;
using System.Collections.Generic; // Dictionaries & lists
using System.Linq; // For .Contains()
using GagSpeak.Events; // For ItemChangedEventArgs
using GagSpeak.Data; // For GagAndLockTypes


namespace GagSpeak.Chat;

/// <summary>
/// Enum to represent priority levels
/// </summary>
public enum GagCatagory
{
    Fashionable,        // For gag types such as the cage muzzle or loose ribbon wraps. Where sound can clearly be heard, and is for decoration.
    NancyDrew,          // For gags that 
    SweetGwendoline,    // For gags that 
    Gimp,               // For gags that completely seal the mouth, such as mouth sealed latex hoods, pump gags, Dildo's.
}

/// <summary>
/// Interface for all gags
/// </summary>
public interface IGag
{
    #region IGagProperties
    GagCatagory Catagory{ get; set; }
    
    // The first thing all gags should consider is how vowel's can exist in speech based off the gag type. This depends a lot on the gag.
    //
    // * Longer Vowels are usually removed the moment one is gagged, words like 'page' or 'meat' or 'vice' (see nancy drew ruleset)
    // * Shorter Vowels can be heard from the corners of a cleave gag, wiffle gag, or bit gag, but otherwise not. thus, gag dependant.
    // * If the mouth is sealed off, then no vowels can be heard. Thus, they become hummed constants
    // * Hummed constants consist of M, N, and R. Think of them like vowels with the lid left on. This dont require mouth articulation,
    // and just vibrating vocal cords, which are why they are go to when we replace vowels.
    // 
    // In summary:
    // * Long Vowels are always removed when wearing a gag of any type.
    // * Allowing gaps on corners determines if short vowels are still heard.
    // * Hummed constanants are set to true if LeaveGapOnCorners is false, and false if it is true.
    bool        LeavesGapsOnCorners { get; set; }           // Are short vowels heard?  Is the mouth packed? 
    bool        AllowsHummedConsonants { get; set; }        // Possibly not needed. Determines if we use them, but they are only used when the above is false.     
    
    // The second thing to consider that are affected by gags is consonants.
    //
    // * Air Consonants (TH, S, SH, F, H) - Sustained air sounds with different configurations of teeth lips & tongue.
    // * Tooth Consonants (T,D) - Sounds formed by tongue against the teeth.
    // * Lip-Formed Consonants (P, B, W) - Sounds formed by lips similar to Tooth consonants, but done in backward order.
    // * Rear Pallet Consonants (K, hard G, J, Y) - Formed from tongue touching roof of mouth... Articulate similar to lip & tooth constantants...
    // Important to note K & G use tongue further back, and thus capable of being used for gagged captive's speech... J & Y however, are muted by
    // gags more easily, due to forming at roof of mouth
    //
    // In summary:
    // * Air Consonants are sustained air sounds with different configurations of teeth lips & tongue.
    // * Tooth Consonants are sounds formed by tongue against the teeth.
    // * Lip-Formed Consonants are sounds formed by lips similar to Tooth consonants, but done in backward order.
    // * Rear Pallet Consonants are formed from tongue touching roof of mouth. K & G use tongue further back and thus can be used for gagged speech.
    bool        AllowsAirConsonants { get; set; }           // TH, S, SH, F, H          Can air pass through mouth?
    bool        AllowsToothConsonants { get; set; }         // T, D                     Can tongue touch teeth?
    bool        AllowsLipFormedConsonants { get; set; }     // P, B, W                  Can lips touch?
    bool        AllowsRearPalletConsonants { get; set; }    // K, G, J, Y               Can tongue touch roof of mouth?
    
    #endregion IGagProperties
    #region IGagFunctions
    char randomizeVowelReplacement()
    {
        Random rand = new Random();
        return rand.Next() % 2 == 0 ? 'm' : 'n';
    }

    string GarbleMessage(string message)
    {
        GagSpeak.Log.Debug($"IGag GarbleMessage, Garbling message with gag {Catagory}");
        string temp = message;
        if (!LeavesGapsOnCorners)
        {
            //not sure what would be changed without gaps on the corners
        }
        if (!AllowsRearPalletConsonants)
        {
            //K, G, J, Y
            temp = temp.Replace("k", "gh");
            temp = temp.Replace("g", "gh");
            temp = temp.Replace("j", "gh");
            temp = temp.Replace("y", "gh");
        }
        if (!AllowsLipFormedConsonants)
        {
            //P, B, W
            temp = temp.Replace('p', 'w');
            temp = temp.Replace('b', 'w');
        }
        if (!AllowsToothConsonants)
        {
            //T, D
            temp = temp.Replace('t', '\'');
            temp = temp.Replace('d', '\'');

        }
        if (!AllowsAirConsonants)
        {
            // TH, S, SH, F, H
            temp = temp.Replace("th", "d");
            temp = temp.Replace('s', ' ');
            temp = temp.Replace("sh", "");
            temp = temp.Replace('f',' ');
            temp = temp.Replace('h',' ');
        }
        if (!AllowsHummedConsonants)
        {
            //M, N, R
            temp = temp.Replace('m', 'b');
            temp = temp.Replace('n', 'd');
            //not sure what to do with an R here, it's not a nasal like the other two
        }

        var garbled = temp;

        return garbled;
    }

    /// <summary> Callable by any gag class to handle.
    /// <para> GOAL: Try to use the same number of letters in your gag speak to the original words </para>
    /// </summary>
    public void PolishWordLength(ref string messageword) {
        // We tend to read words as shapes, this will help readers to understand them
    }

    /// <summary> Callable by any gag class to handle
    /// <para> GOAL: Use the ‘real’ original vowel in the first or second position of the word</para>
    /// </summary>
    /// <returns>the index of the word where it is found</returns>
    public int FirstVowelIndex(ref string messageword) {
        // helps the reader to understand the word by doing this
        return 0;
    }

    /// <summary>
    /// Callable by any gag class to handle
    /// <para>Goal: “er” becomes “rr” or just “r” (if the word is short)</para>
    /// </summary>
    public void ReplaceEr(ref string messageword) {
        // This will help the reader to understand the word
    }

    /// <summary>
    /// Callable by any gag class to handle
    /// <para>Goal: “w” can be a good filler for un-emphasized vowel syllables and sometimes consonance syllables</para>
    /// </summary>
    public void HandleUnemphasizedVowelSyllables(ref string messageword) {
        // This will help the reader to understand the word
    }

    #endregion IGagFunctions
}



public class FashionableGag : IGag
{
    public GagCatagory Catagory { get; set; }
    public bool LeavesGapsOnCorners { get; set; }
    public bool AllowsHummedConsonants { get; set; }
    public bool AllowsAirConsonants { get; set; }
    public bool AllowsToothConsonants { get; set; }
    public bool AllowsLipFormedConsonants { get; set; }
    public bool AllowsRearPalletConsonants { get; set; }

    // default constructor that calls the augmented with all booleans set to true
    public FashionableGag() : this(true, true, true, true, true, true) { }
    public FashionableGag(bool _cornerGaps, bool _allowHummed, bool _allowAir, bool _allowLipFormed, bool _allowTooth, bool _allowRearPallet) {
        Catagory = GagCatagory.Fashionable;
        LeavesGapsOnCorners = _cornerGaps;
        AllowsRearPalletConsonants = _allowRearPallet;
        AllowsLipFormedConsonants = _allowLipFormed;
        AllowsToothConsonants = _allowTooth;
        AllowsAirConsonants = _allowAir;
        AllowsHummedConsonants = _allowHummed;
    }
    public string GarbleMessage(string message) { // this is just fashionable, so we dont need to translate it at all
        GagSpeak.Log.Debug($"FashionableGag GarbleMessage, Garbling message with gag {Catagory}");
        return message;
    }
}

/// <summary> The Nancy Drew Gag Class.
/// <para>Realistically, most gagged speech intended to be 'understood' is going to be from a Nancy Drew class Gag</para>
/// <list type="bullet">
/// <item>Think soft gags, in general(ineffective at blocking sound, but enough to muffle words somewhat)</item>
/// <item>Unpacked [ LeaveGapsOnCorners == TRUE ]</item>
/// <item>Lips can touch (with effort, so maybe 75% chance) [ AllowsLipFormedConsonants == TRUE ]</item>
/// <item>Jaw can move (partially) [ ??? ] </item>
/// <item>Some Air can pass through the mouth (maybe 50% or 75% chance?) [ AllowsAirConsonants == TRUE ]</item>
/// <item>Tongue can touch the roof of the mouth [ AllowsRearPalletConsonants == TRUE ]</item>
/// <item>Tongue can't touch the teeth [ AllowsToothConsonants == FALSE ]</item>
/// </list> </summary>
public class NancyDrewGag : IGag
{
    #region NancyGagProperties
    public GagCatagory Catagory { get; set; }
    public bool LeavesGapsOnCorners { get; set; }
    public bool AllowsHummedConsonants { get; set; }
    public bool AllowsAirConsonants { get; set; }
    public bool AllowsToothConsonants { get; set; }
    public bool AllowsLipFormedConsonants { get; set; }
    public bool AllowsRearPalletConsonants { get; set; }

    // default constructor that calls the augmented with all booleans set to true
    public NancyDrewGag() : this(true, true, true, true, true, true) { }
    public NancyDrewGag(bool _cornerGaps, bool _allowHummed, bool _allowAir, bool _allowLipFormed, bool _allowTooth, bool _allowRearPallet) {
        Catagory = GagCatagory.NancyDrew;
        LeavesGapsOnCorners = _cornerGaps;
        AllowsRearPalletConsonants = _allowRearPallet;
        AllowsLipFormedConsonants = _allowLipFormed;
        AllowsToothConsonants = _allowTooth;
        AllowsAirConsonants = _allowAir;
        AllowsHummedConsonants = _allowHummed;
    }

    #endregion NancyGagProperties
    #region NancyGagFunctions

    public string GarbleMessage(string message) {
        GagSpeak.Log.Debug($"NancyGag GarbleMessage, Garbling message with gag {Catagory}");
        // Split the message into words
        var words = message.Split(' ');
        // Process each word
        for (int i = 0; i < words.Length; i++) {
            var word = words[i];
            // Process each character in the word
            for (int j = 0; j < word.Length; j++) {
                ReplaceKQ(ref word, j);
                ReplaceGDBP(ref word, j);
                ReplaceTS(ref word, j);
                ReplaceF(ref word, j);
                IsLetterLYR(ref word, j);
                ConvertAndRemoveVowels(ref word, j);
                ReplaceExceptedVowels(ref word, j);
            }
            // Replace the word in the array with the processed word
            words[i] = word;
        }
        // Join the words back together into a single string
        return string.Join(' ', words);
    }

    /// <summary> Convert any K & Q, if not at the beginning of the word, to “Gh", or "Ch” </summary>
    public void ReplaceKQ(ref string word, int index) {
        if (index > 0 && (word[index] == 'K' || word[index] == 'Q')) {
            word = word.Remove(index, 1).Insert(index, "Gh");
        }
    }

    /// <summary> "G", "D", "B", "P" become "Gh" (based on if the respective consonants booleans is true) </summary>
    public void ReplaceGDBP(ref string word, int index) {
        if ("GDBP".Contains(word[index])) {
            word = word.Remove(index, 1).Insert(index, "Gh");
        }
    }

    /// <summary> "T" becomes "Th", and "S" becomes "Sh" ALMOST universally, we'll introduce exceptions as we find them </summary>
    public void ReplaceTS(ref string word, int index) {
        if (word[index] == 'T') {
            word = word.Remove(index, 1).Insert(index, "Th");
        } else if (word[index] == 'S') {
            word = word.Remove(index, 1).Insert(index, "Sh");
        }
    }

    /// <summary> "F" ALWAYS becomes "Ph" (Only possible exception is if at start of word) </summary>
    public void ReplaceF(ref string word, int index) {
        if (word[index] == 'F') {
            word = word.Remove(index, 1).Insert(index, "Ph");
        }
    }

    /// <summary> "L", "Y", "R" should be kept as is, but sparingly. </summary>
    public void IsLetterLYR(ref string word, int index) {
        char currentChar = word[index];
        if ("LYR".Contains(currentChar) && word.Count(c => c == currentChar) > 1) {
            // remove the letter from the word
            word = word.Remove(index, 1);
        }
    }

    /* --- With a Light Gag, Certain Long Vowel Sounds Are Hampered Or Eliminated --- */
    /// <summary> Converts vowels not following general case exceptions to "M" and "N", and flat out remove any hard vowels not in general cases </summary>
    public void ConvertAndRemoveVowels(ref string word, int index) {
        if ("AEIOU".Contains(word[index])) {
            word = word.Remove(index, 1).Insert(index, "M");
        }
    }

    /// <summary> Replace general case exception vowels with passive vowel sounds "eh", "ah", "oh", "oo" </summary>
    public void ReplaceExceptedVowels(ref string word, int index) {
        string replacement = word[index] switch {
            'A' => "ah",
            'E' => "eh",
            'I' => "eh",
            'O' => "oh",
            'U' => "oo",
            _ => ""
        };
        if (replacement != "") { 
            word = word.Remove(index, 1).Insert(index, replacement);
        }
    }

    #endregion NancyGagFunctions
}

/// <summary> The Sweet Gwendoline Gag Class.
/// <para>This class is for gags that are packed, but not completely sealed. Intends to not
/// be that capable of having understandable speech, but an attempt should be visable.</para>
/// <list type="bullet">
/// <item>Unsealed gags, but are packed [ LeavesGapsOnCorners == FALSE ] </item>
/// <item>Lips can't touch/seperate from Gag [ AllowsLipFormedConsonants == FALSE ] </item>
/// <item>Jaw can barely move [ ??? ] </item>
/// <item>Tongue can't articulate from molars forward [ AllowsRearPalletConsonants AND AllowToothConsonants == FALSE ] </item>
/// </list> </summary>
public class SweetGwendolineGag : IGag
{
    #region GwendolineGagProperties
    public GagCatagory Catagory { get; set; }
    public bool LeavesGapsOnCorners { get; set; }
    public bool AllowsHummedConsonants { get; set; }
    public bool AllowsAirConsonants { get; set; }
    public bool AllowsToothConsonants { get; set; }
    public bool AllowsLipFormedConsonants { get; set; }
    public bool AllowsRearPalletConsonants { get; set; }

    // default constructor that calls the augmented with all booleans set to true
    public SweetGwendolineGag() : this(true, true, true, true, true, true) { }
    public SweetGwendolineGag(bool _cornerGaps, bool _allowHummed, bool _allowAir, bool _allowLipFormed, bool _allowTooth, bool _allowRearPallet) {
        Catagory = GagCatagory.SweetGwendoline;
        LeavesGapsOnCorners = _cornerGaps;
        AllowsRearPalletConsonants = _allowRearPallet;
        AllowsLipFormedConsonants = _allowLipFormed;
        AllowsToothConsonants = _allowTooth;
        AllowsAirConsonants = _allowAir;
        AllowsHummedConsonants = _allowHummed;
    }

    #endregion GwendolineGagProperties
    #region GwendolineGagFunctions

    public string GarbleMessage(string message) {
        GagSpeak.Log.Debug($"SweetGwendolineGag GarbleMessage, Garbling message with gag {Catagory}");
        // Split the message into words
        var words = message.Split(' ');
        // Process each word
        for (int i = 0; i < words.Length; i++) {
            var word = words[i];
            // Process each character in the word
            for (int j = 0; j < word.Length; j++) {
                ReplaceVowels(ref word, j);
                ReplaceUnemphasizedSyllables(ref word, j);
                ReplaceHardConsonants(ref word, j);
            }
            // Replace the word in the array with the processed word
            words[i] = word;
        }
        // Join the words back together into a single string
        return string.Join(' ', words);
    }

    /// <summary> All vowels become “m” or “n” (which is applied depends on where in the word) </summary>
    public void ReplaceVowels(ref string word, int index) {
        if ("AEIOU".Contains(word[index])) {
            word = word.Remove(index, 1).Insert(index, index % 2 == 0 ? "m" : "n");
        }
    }

    /// <summary> use "w" as a filler for non-emphasized syllables for both vowels and the consonances (see attributes comments for letters) </summary>
    public void ReplaceUnemphasizedSyllables(ref string word, int index) {
        // Assuming that unemphasized syllables are those that are not stressed (i.e., not capitalized)
        if (char.IsLower(word[index])) {
            word = word.Remove(index, 1).Insert(index, "w");
        }
    }

    /// <summary> All hard consonance become the glottal “gh” with rare exceptions </summary>
    public void ReplaceHardConsonants(ref string word, int index) {
        if ("BDGKPTQ".Contains(word[index])) {
            word = word.Remove(index, 1).Insert(index, "gh");
        }
    }

    #endregion GwendolineGagFunctions
}

/// <summary> The Gimp Gag Class. 
/// <para>This gag class reduces sound to animal-like grunts and failed attempts at intelligable words.
/// Intend to only give rythm of speech, should not be understandable </para>
/// <list type="bullet">
/// <item>Large, Tight BallGags, Inflated Pump Gags, Sealed Tape Gags.</item>
/// <item>Mouth fully sealed, no air can escape [ AllowsAirConsonants == FALSE ] </item>
/// <item>Tongue is immobilized, no speech articulations [ AllowsRearPalletConsonants AND AllowsToothConsonants AND AllowsLipFormedConsonants == FALSE ] </item>
/// <item>Only muted vowel sounds can escape through the nose and throat [ AllowsHummedConsonants == TRUE, LeavesGapsOnCorners == FALSE ] </item>
/// </list> </summary>
public class GimpGag : IGag
{
    #region GimpGagProperties
    public GagCatagory Catagory { get; set; }
    public bool LeavesGapsOnCorners { get; set; }
    public bool AllowsHummedConsonants { get; set; }
    public bool AllowsAirConsonants { get; set; }
    public bool AllowsToothConsonants { get; set; }
    public bool AllowsLipFormedConsonants { get; set; }
    public bool AllowsRearPalletConsonants { get; set; }

    // default constructor that calls the augmented with all booleans set to true
    public GimpGag() : this(true, true, true, true, true, true) { }
    public GimpGag(bool _cornerGaps, bool _allowHummed, bool _allowAir, bool _allowLipFormed, bool _allowTooth, bool _allowRearPallet) {
        Catagory = GagCatagory.Gimp;
        LeavesGapsOnCorners = _cornerGaps;
        AllowsRearPalletConsonants = _allowRearPallet;
        AllowsLipFormedConsonants = _allowLipFormed;
        AllowsToothConsonants = _allowTooth;
        AllowsAirConsonants = _allowAir;
        AllowsHummedConsonants = _allowHummed;
    }

    #endregion GimpGagProperties
    #region GimpGagFunctions

    public string GarbleMessage(string message) {
        GagSpeak.Log.Debug($"GimpGag GarbleMessage, Garbling message with gag {Catagory}");
        var words = message.Split(' ');

        for (int i = 0; i < words.Length; i++) {
            var word = words[i];

            RemoveVowelsAndPunctuation(ref word);
            ReplaceConsonants(ref word);
            MixAndCorrectR(ref word);
            InsertGhForGaspsAndGrunts(ref word);

            words[i] = word;
        }

        return string.Join(' ', words);
    }

    /// <summary> Remove all vowels and punctuation be gone</summary>
    public void RemoveVowelsAndPunctuation(ref string word) {
        word = new string(word.Where(c => !"AEIOU.,;'".Contains(c)).ToArray());
    }

    /// <summary> all consonants should be replaced by “m” or “n”. In a word, only 3 letters max should be used. </summary>
    public void ReplaceConsonants(ref string word) {
        int mCount = 0, nCount = 0;
        for (int i = 0; i < word.Length; i++) {
            if ("BCDFGHJKLMNPQRSTVWXYZ".Contains(word[i])) {
                if (mCount < 3) {
                    word = word.Remove(i, 1).Insert(i, "m");
                    mCount++;
                } else if (nCount < 3) {
                    word = word.Remove(i, 1).Insert(i, "n");
                    nCount++;
                }
            }
        }
    }

    /// <summary> r's should be used to denote anger, and only used towards the end (other r's should be cut off typically) </summary>
    public void MixAndCorrectR(ref string messageword, int index = 0) {
        // Not really sure what to do here...
    }

    /// <summary> Use "gh" for grunts and muted gasps in concert with “m”, “n”, “r”, “ph” “th” and sometimes “p”.
    /// <para>If words dont match original word length, try mixing in endings. "Ghmp", "Grrgh", "Ghnnth", "Ghmp"</para>
    /// </summary>
    public void InsertGhForGaspsAndGrunts(ref string word, int index = 0) {
        if (index == word.Length - 1) {
            word += "gh";
        }
    }

    #endregion GimpGagFunctions
}

/// <summary> Class to manage multiple gags </summary>


// WIP ATM
public class GagManager : IDisposable
{
    private readonly GagSpeakConfig _config;
    public List<IGag> activeGags;

    public GagManager(GagSpeakConfig config) {
        _config = config;
        // get the IGag class type from the value of the Dictionary<string, IGag> GagTypes, where it searched to see if the key defined by selectedGagTypes is in the dictionary
        activeGags = _config.selectedGagTypes.Select(gagType => GagAndLockTypes.GagTypes[gagType]).ToList();
        // print our active list:
        foreach (var gag in activeGags) {
            GagSpeak.Log.Debug($"Active Gag: {gag.Catagory}");
        }

        
        // subscribe to our events
        _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
    }

    public void Dispose() {
        // unsubscribe from our events
        _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
    }

    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        activeGags = _config.selectedGagTypes.Select(gagType => GagAndLockTypes.GagTypes[gagType]).ToList();
    }

    public string ProcessMessage(string message) {
        int highestPriorityGag = GetHighestPriorityGag();
        GagSpeak.Log.Debug($"Processing message with gag {activeGags[highestPriorityGag].Catagory}");
        try {
            message = activeGags[highestPriorityGag].GarbleMessage(message);
        } 
        catch (Exception e) {
            GagSpeak.Log.Error($"Error processing message with gag {activeGags[highestPriorityGag].Catagory}: {e.Message}");
        }
        // return the message
        return message;
    }

    private int GetHighestPriorityGag() {
        int highestPriorityGag = 0; 
        GagCatagory highestPriority = GagCatagory.Fashionable;

    for (int i = 0; i < activeGags.Count; i++) {
        if (activeGags[i].Catagory > highestPriority) {
            highestPriority = activeGags[i].Catagory;
            highestPriorityGag = i;
        }
    }
        GagSpeak.Log.Debug($"Highest Priority Gag: {highestPriorityGag}");
        // will have correct index after this
        return highestPriorityGag;
    }
}