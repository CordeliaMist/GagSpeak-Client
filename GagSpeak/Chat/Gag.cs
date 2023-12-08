using System;
using System.Collections.Generic; // Dictionaries & lists
using System.Linq; // For .Contains()
using GagSpeak.Events; // For ItemChangedEventArgs
using GagSpeak.Data; // For GagAndLockTypes
using System.Text.RegularExpressions;
using GagSpeak.Chat.Garbler;
using System.Xml.Serialization;

namespace GagSpeak.Chat;
/* --------------------------------------------------------------
            To anyone who happens to read this file
    Please note that this is very early in development as a more 
    sophisticated message garbler. It is very difficult to account
    for all speech patterns, much less how they are affected by
    various gags. This is a work in progress, and will be updated
    as more information is recieved, and algorithms updated and
    optimized to be more versitile
--------------------------------------------------------------- */


/// <summary>
/// Enum to represent priority levels
/// </summary>
public enum GagCatagory
{
    Fashionable = 0,        // For gag types such as the cage muzzle or loose ribbon wraps. Where sound can clearly be heard, and is for decoration.
    NancyDrew = 1,          // For gags that 
    SweetGwendoline = 2,    // For gags that 
    Gimp = 3,               // For gags that completely seal the mouth, such as mouth sealed latex hoods, pump gags, Dildo's.
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
    public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message)
    {
        GagSpeak.Log.Debug($"IGag GarbleMessage, Garbling message with gag {Catagory}");
        // string temp = message;
        // if (!LeavesGapsOnCorners)
        // {
        //     //not sure what would be changed without gaps on the corners
        // }
        // if (!AllowsRearPalletConsonants)
        // {
        //     //K, G, J, Y
        //     temp = temp.Replace("k", "gh");
        //     temp = temp.Replace("g", "gh");
        //     temp = temp.Replace("j", "gh");
        //     temp = temp.Replace("y", "gh");
        // }
        // if (!AllowsLipFormedConsonants)
        // {
        //     //P, B, W
        //     temp = temp.Replace('p', 'w');
        //     temp = temp.Replace('b', 'w');
        // }
        // if (!AllowsToothConsonants)
        // {
        //     //T, D
        //     temp = temp.Replace('t', '\'');
        //     temp = temp.Replace('d', '\'');

        // }
        // if (!AllowsAirConsonants)
        // {
        //     // TH, S, SH, F, H
        //     temp = temp.Replace("th", "d");
        //     temp = temp.Replace('s', ' ');
        //     temp = temp.Replace("sh", "");
        //     temp = temp.Replace('f',' ');
        //     temp = temp.Replace('h',' ');
        // }
        // if (!AllowsHummedConsonants)
        // {
        //     //M, N, R
        //     temp = temp.Replace('m', 'b');
        //     temp = temp.Replace('n', 'd');
        //     //not sure what to do with an R here, it's not a nasal like the other two
        // }

        // var garbled = temp;

        return message;
    }

    #endregion IGagProperties
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

    public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message) {
        // first see if we are in experimental mode, if so, use the experimental garbler
        if (config.ExperimentalGarbler) {
            GagSpeak.Log.Debug($"NancyGag GarbleMessage, Garbling message with gag {Catagory}");
            // Split the message into words
            var words = message.Split(' ');
            // Process each word
            for (int i = 0; i < words.Length; i++) {
                var word = words[i];
                // Process each character in the word
                try {
                    // check to see if the word is in our preconverted words dictionary
                    if(!TranslateWord(ref word)){
                        // do the nancy drew cases
                        GagSpeak.Log.Debug($"Processing word {word}");
                        ReplaceKQ(ref word);
                        ReplaceGDBP(ref word);
                        ReplaceTS(ref word);
                        ReplaceF(ref word);
                        GagSpeak.Log.Debug($"Word after nancy drew cases: {word}");
                        HamperOrEliminateLongVowels(ref word, words[i]);
                        GagSpeak.Log.Debug($"Word after nancy drew vowel cases: {word}");
                        // do base cases and compare the words
                        ReplaceEr(ref word, words[i]);
                        PolishWordLength(ref word, words[i]);
                        GagSpeak.Log.Debug($"Word after base cases: {word}");
                    }
                    // set new word
                    words[i] = word;
                }
                catch (Exception e) {
                    GagSpeak.Log.Error($"Error processing word {word} with gag {Catagory}: {e.Message}");
                }
                // Replace the word in the array with the processed word
            }
            // Join the words back together into a single string
            return string.Join(' ', words);
        } else {
            return messageGarbler.GarbleMessage(message, 2);
        }
    }

    /// <summary> Preconverted words for the Nancy Drew gag </summary>
    public bool TranslateWord(ref string word) {
        // Check if the word has been preconverted
        if (PreGarbledWords.NancyPremadeWords.ContainsKey(word)) {
            // If it has, return the preconverted word
            GagSpeak.Log.Debug($"Matched word {PreGarbledWords.NancyPremadeWords[word]} in preconverted words");
            word = PreGarbledWords.NancyPremadeWords[word];
            return true;
        }
        return false;
    }

    /// <summary> Convert any K and Q, if not at the beginning of the word, to “Gh", or "Ch”
    /// <para> Additional Note: Many ‘Nancy Drew’ class gags can pass stilted “K” sounds. They are better represented with a more ‘submissive’ “C” unless that suggests a word different than the one intended </para>
    /// </summary>
    public void ReplaceKQ(ref string word) {
        // Define a regex pattern for K/k and Q/q not at the start of the word
        var regexK = new Regex("(?<=.)(K)", RegexOptions.IgnoreCase);
        var regexQ = new Regex("(?<=.)(Q)", RegexOptions.IgnoreCase);

        // Replace matches with "Gh" or "gh" and "Ch" or "ch" depending on the case
        word = regexK.Replace(word, m => m.Value == "K" ? "Gh" : "gh");
        word = regexQ.Replace(word, m => m.Value == "Q" ? "Ch" : "ch");
    }

    /// <summary> "G", "D", "B", "P" become "Gh" (based on if the respective consonants booleans is true) </summary>
    public void ReplaceGDBP(ref string word) {
        // Define a regex pattern for G, D, B, P in both cases
        var regex = new Regex("[GDPgdp]");
        // Replace matches with "Gh" or "gh" depending on the case
        word = regex.Replace(word, m => "GDBP".Contains(m.Value) ? "Gh" : "gh");
    }

    /// <summary> "T" becomes "Th", and "S" becomes "Sh" ALMOST universally, we'll introduce exceptions as we find them </summary>
    public void ReplaceTS(ref string word) {
        // Define a regex pattern for T/t not followed by h/H and S/s not followed by h/H
        var regexT = new Regex("(?<=.)(T)(?!h)", RegexOptions.IgnoreCase);
        word = regexT.Replace(word, m => m.Value == "T" ? "Th" : "th");
        var regexST = new Regex("(?<=.)(S)(?=T)(?!Th)", RegexOptions.IgnoreCase);
        word = regexST.Replace(word, m => m.Value == "S" ? "STh" : "sth");
        var regexS = new Regex("(?<=.)(S)(?!h)", RegexOptions.IgnoreCase);
        word = regexS.Replace(word, m => m.Value == "S" ? "Sh" : "sh");
    }

    /// <summary> "F" ALWAYS becomes "Ph" (Only possible exception is if at start of word) </summary>
    public void ReplaceF(ref string word) {
        // Define a regex pattern for F/f not at the start of the word
        var regex = new Regex("(?<=.)(f|b)", RegexOptions.IgnoreCase);
        // Replace matches with "Ph" or "ph" depending on the case
        word = regex.Replace(word, m => m.Value == "F" || m.Value == "B" ? "Ph" : "ph");
    }

    /* --- With a Light Gag, Certain Long Vowel Sounds Are Hampered Or Eliminated --- */
    /// <summary> Converts vowels not following general case exceptions to "M" and "N", and flat out remove any hard vowels not in general cases </summary>
    public void HamperOrEliminateLongVowels(ref string word, string originalword) {
        /* This regular expression matches a vowel followed by an optional non-vowel and then an 'e' at the end of a word
        (\b is a word boundary), or two vowels next to each other. However, please note that this will not correctly identify
        all long vowel sounds due to the complexity of English pronunciation rules.*/
        // 1. Check for double vowels and replace them with 'w'
        var regexDoubleVowels = new Regex("[aeiou]{2}", RegexOptions.IgnoreCase);
        if (regexDoubleVowels.IsMatch(word)) {
            GagSpeak.Log.Debug($"Matched double vowel {regexDoubleVowels.Match(word).Value} in word {word}");
            word = regexDoubleVowels.Replace(word, "w");
        }
        // 2. Check for two vowels side by side with one non-vowel letter between them where the first vowel is the first letter of the word, and remove the second vowel
        if (word.Length > 2 && "aeiou".Contains(char.ToLower(word[0])) && "aeiou".Contains(char.ToLower(word[2]))) {
            GagSpeak.Log.Debug($"Matched starting word double vowel {word[0]}{word[1]}{word[2]} in word {word}");
            word = word.Remove(2, 1);
        }
        // 3. Check for two vowels side by side with one non-vowel letter between them and remove both vowels
        var regexVowelsWithNonVowelBetween = new Regex("[aeiou][^aeiou][aeiou]", RegexOptions.IgnoreCase);
        if (regexVowelsWithNonVowelBetween.IsMatch(word)) {
            GagSpeak.Log.Debug($"Matched double vowel {regexVowelsWithNonVowelBetween.Match(word).Value} in word {word}");
            word = regexVowelsWithNonVowelBetween.Replace(word, m => m.Value[1].ToString());
        }
        // 4. If a word contains one vowel and the word length is longer than the original word, remove the vowel
        if (word.Length > originalword.Length) {
            var regexSingleVowel = new Regex("(?<=.)[aeiou]", RegexOptions.IgnoreCase);
            var match = regexSingleVowel.Match(word);
            if (match.Success) {
                GagSpeak.Log.Debug($"Matched single vowel {match.Value} in word {word}");
                int vowelIndex = match.Index;
                word = regexSingleVowel.Replace(word, "");
                if (word.Length < originalword.Length) {
                    word = word.Insert(vowelIndex, "h");
                }
            }

        /* TEST SAMPLES:
        
        You'll never get away with this!
        You little bitch! I'll get you if it's the last thing i do!

        */
        }
        // Define regex patterns for remaining vowels
        var regexAorE = new Regex("(?<=.)[ae]", RegexOptions.IgnoreCase);
        var regexO = new Regex("(?<=.)o", RegexOptions.IgnoreCase);
        var regexU = new Regex("(?<=.)u", RegexOptions.IgnoreCase);
        var regexI = new Regex("(?<=.)i", RegexOptions.IgnoreCase);
        // Replace remaining vowels
        word = regexAorE.Replace(word, m => m.Value == "a" || m.Value == "e" ? "eh" : "EH");
        word = regexO.Replace(word, m => m.Value == "o" ? "ah" : "AH");
        word = regexU.Replace(word, m => m.Value == "u" ? "ooh" : "OOH");
        word = regexI.Replace(word, "");
    }

    /// <summary> Try to use the same number of letters in your gag speak to the original words </summary>
    public void PolishWordLength(ref string messageword, string originalword) {
        // Define a regex pattern for any two-character sequence repeated
        var regex = new Regex(@"(\w{2})\1");
        // Replace matches with a single instance of the sequence
        messageword = regex.Replace(messageword, "$1");

        // if the word is longer than the original word, remove the vowels
        if (messageword.Length > originalword.Length) {
            var regexVowels = new Regex("[aeiou]", RegexOptions.IgnoreCase);
            messageword = regexVowels.Replace(messageword, "");
        }
    
    }

    /// <summary> Goal: “er” becomes “rr” or just “r” (if the word is short) </summary>
    public void ReplaceEr(ref string messageword, string originalword) {
        // Define a regex pattern for 'r'
        var regex = new Regex("r", RegexOptions.IgnoreCase);
        // Find all matches in the original word and the messageword
        var originalMatches = regex.Matches(originalword);
        var messageMatches = regex.Matches(messageword);
        // Iterate over the matches
        for (int i = 0; i < originalMatches.Count && i < messageMatches.Count; i++) {
            // If the 'r' in the original word follows an 'e', replace it in the messageword
            if(messageword.Length < 5) {
                messageword = messageword.Insert(messageMatches[i].Index, "r");
            }
        }
    }

    /// <summary> Use the ‘real’ original vowel in the first or second position of the word</summary>
    /// <returns>the index of the word where it is found</returns>
    public int FirstVowelIndex(ref string messageword) {
        // Define a regex pattern for vowels
        var regex = new Regex("[aeiouAEIOU]");
        // Find the first match in the string
        var match = regex.Match(messageword);
        // If a match is found, return its index. Otherwise, return -1
        return match.Success ? match.Index : -1;
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

    public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message) {
        // first see if we are in experimental mode, if so, use the experimental garbler
        if (false) {
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
        } else {
            return messageGarbler.GarbleMessage(message, 5);
        }
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

    public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message) {
        // first see if we are in experimental mode, if so, use the experimental garbler
        if (false) {
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
        } else {
            return messageGarbler.GarbleMessage(message, 7);
        }
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
    private readonly MessageGarbler _messageGarbler;
    public List<IGag> activeGags;

    public GagManager(GagSpeakConfig config, MessageGarbler messageGarbler) {
        _config = config;
        _messageGarbler = messageGarbler;
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
            message = activeGags[highestPriorityGag].GarbleMessage(_config, _messageGarbler, message);
            GagSpeak.Log.Debug($"Message after gag {activeGags[highestPriorityGag].Catagory}: {message}");
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
        GagSpeak.Log.Debug($"Active Gag: {activeGags[i].Catagory}");
        // print out the value of the index at the enum for the catagory
        GagSpeak.Log.Debug($"Active Gag Enum: {(int)activeGags[i].Catagory}");
        if (activeGags[i].Catagory > highestPriority) {
            highestPriority = activeGags[i].Catagory;
            highestPriorityGag = i;
        }
    }
        GagSpeak.Log.Debug($"Index of Highest Priority Gag: {highestPriorityGag}");
        // will have correct index after this
        return highestPriorityGag;
    }
}