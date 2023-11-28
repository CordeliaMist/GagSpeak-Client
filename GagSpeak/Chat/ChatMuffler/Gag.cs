using System;
using System.Collections.Generic; // Dictionaries & lists

// Enum to represent priority levels
public enum GagPriority
{
    Low,
    Medium,
    High
}

// Interface for all gags
public interface IGag
{
    GagPriority Priority { get; }
    bool LeavesGapsOnCorners { get; }
    bool AllowsRearPalletConsonants { get; }
    bool AllowsLipFormedConsonants { get; }
    bool AllowsToothConsonants { get; }
    bool AllowsAirConsonants { get; }
    bool AllowsHummedConsonants { get; }
    bool AllowsVowels { get; }

    char randomizeVowelReplacement()
    {
        Random rand = new Random();
        return rand.Next() % 2 == 0 ? 'm' : 'n';
    }
    string GarbleMessage(string message)
    {
        string temp = message;
        if (!LeavesGapsOnCorners)
        {
            //not sure what would be changed without gaps on the corners
        }
        if (!AllowsRearPalletConsonants)
        {
            //K, G, J, Y
            temp.Replace("k", "gh");
            temp.Replace("g", "gh");
            temp.Replace("j", "gh");
            temp.Replace("y", "gh");
        }
        if (!AllowsLipFormedConsonants)
        {
            //P, B, W
            temp.Replace('p', 'w');
            temp.Replace('b', 'w');
        }
        if (!AllowsToothConsonants)
        {
            //T, D
            temp.Replace('t', '\'');
            temp.Replace('d', '\'');

        }
        if (!AllowsAirConsonants)
        {
            // TH, S, SH, F, H
            temp.Replace("th", "d");
            temp.Replace('s', '');
            temp.Replace("sh", "");
            temp.Replace('f', '');
            temp.Replace('h', '');
        }
        if (!AllowsHummedConsonants)
        {
            //M, N, R
            temp.Replace('m', 'b');
            temp.Replace('n', 'd');
            //not sure what to do with an R here, it's not a nasal like the other two
        }
        if (!AllowsVowels)
        {
            temp.Replace('a', randomizeVowelReplacement());
            temp.Replace('e', randomizeVowelReplacement());
            temp.Replace('i', randomizeVowelReplacement());
            temp.Replace('o', randomizeVowelReplacement());
            temp.Replace('u', randomizeVowelReplacement());
        }

        var garbled = temp;

        return garbled;
    }

    // Try to use the same number of letters in your gag speak to the original words
    // We tend to read words as shapes, this will help readers to understand them

    // Use original consonance at the beginning of a word
    // If you're going to bend the rules for the sake of clarity, use original letters first with substitutions at the middle and end

    // Use the ‘real’ original vowel in the first or second position

    // Use caps and punctuation exactly as you would in the ungagged dialogue
    // Any clues we can give the reader to help them see the sentence should be utilized
    // Following or leading any letter with an “h” can make words seem more ‘gaggy’

    // This mimics the attempts of a lightly gagged mouth to articulate using air formed and shaped by the back of the throat

    // “er” becomes “rr” or just “r”

    // “w” can be a good filler for un-emphasized vowel syllables and sometimes consonance

    // Cheating is fair game
    // The more important it is that the captive's words are understood, the more you can cheat to make that happen
    // It just needs to be clear your character is gagged
}

// Class for Nancy Drew gags
// Over-the-mouth detective gags (so ineffectual in real life that we should just consider them cleave gags in the sonic sense)
// Unpacked cleave gags, non-extreme bit gags, (no packing)
// The lips can touch (with effort)
// The jaw can move (partially)
// Some air can pass through the mouth
// The tongue can touch the roof of the mouth but not the teeth
public class NancyDrewGag : IGag
{
    public GagPriority Priority => GagPriority.Low;
    public bool LeavesGapsOnCorners => true;
    public bool AllowsRearPalletConsonants => true;
    public bool AllowsLipFormedConsonants => true;
    public bool AllowsToothConsonants => true;
    public bool AllowsAirConsonants => true;
    public bool AllowsHummedConsonants => true;
    public bool AllowsVowels => true;


    public string GarbleMessage(string message)
    {
        // Implement garbling logic for Nancy Drew gag
        return "";
    }
}

// Class for Sweet Gwendoline gags
// Unsealed cloth gags with mouth packing (not severe), wiffle ball gags, loose hand gags, small ball gags
// The lips cannot touch or separate from the gag
// The jaw can barely move
// The tongue cannot articulate from the molars forward
public class SweetGwendolineGag : IGag
{
    public GagPriority Priority => GagPriority.Medium;
    public bool LeavesGapsOnCorners => true;
    public bool AllowsRearPalletConsonants => true;
    public bool AllowsLipFormedConsonants => true;
    public bool AllowsToothConsonants => true;
    public bool AllowsAirConsonants => true;
    public bool AllowsHummedConsonants => true;
    public bool AllowsVowels => true;

    public string GarbleMessage(string message)
    {
        // Implement garbling logic for Sweet Gwendoline gag
        return "";
    }

    // All vowels become “m”, “n” 

    // “w” as filler for non-emphasized syllables for both vowels or consonances

    // All hard consonance become the glottal “gh” with rare exceptions
}

// Class for Gimp gags
// Large tight ball gags, plug gags, sealed tape gags, tight hand gags
// The Mouth is fully sealed, no air can escape
// The tongue is immobilized and can make no speech articulations
// Only muted vowel sounds can escape through the nose and throat
public class GimpGag : IGag
{
    public GagPriority Priority => GagPriority.High;
    public bool LeavesGapsOnCorners => true;
    public bool AllowsRearPalletConsonants => true;
    public bool AllowsLipFormedConsonants => true;
    public bool AllowsToothConsonants => true;
    public bool AllowsAirConsonants => true;
    public bool AllowsHummedConsonants => true;
    public bool AllowsVowels => true;

    public string GarbleMessage(string message)
    {
        // Implement garbling logic for Gimp gag
        return "";
    }

    // CAPS can determine the volume or level of stress
    // Use with discretion
    // Consider using just partial caps
    // Avoid using more than one exclamation point

    // “M” and “N” are the standard go tos for crying, yelling, moaning and whimpering
    // Repeating letters indicates a longer cry
    // Less is more, 3 repeated letters max each
    // Example of long cry: “MMMmmmnnn”
    // Hint: “m” is a more passive and submissive sound than “n”

    // Mix in “r”s to denote anger, usually towards the end.
    // The more “r”s the more angry the captive
    // Avoid using “r”s by themselves unless your captive happens to be turning into a werewolf

    // Use “gh” for grunts and muted gasps
    // in concert with “m”, “n”, “r”, “ph” “th” and sometimes “p”
    // Examples: “ghmph”, “grrgh”, “ghnnth”, “ghmp”

}

// Class to manage multiple gags
public class GagManager
{
    private List<IGag> activeGags;

    public GagManager()
    {
        activeGags = new List<IGag>();
    }

    public void AddGag(IGag gag)
    {
        activeGags.Add(gag);
    }

    public string ProcessMessage(string message)
    {
        IGag highestPriorityGag = GetHighestPriorityGag();

        if (highestPriorityGag != null)
        {
            return highestPriorityGag.GarbleMessage(message);
        }
        else
        {
            return message;
        }
    }

    private IGag GetHighestPriorityGag()
    {
        IGag highestPriorityGag = null;
        GagPriority highestPriority = GagPriority.Low;

        foreach (var gag in activeGags)
        {
            if (gag.Priority > highestPriority)
            {
                highestPriority = gag.Priority;
                highestPriorityGag = gag;
            }
        }

        return highestPriorityGag;
    }
}