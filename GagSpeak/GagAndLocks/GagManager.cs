using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GagSpeak.Events;
using GagSpeak.Garbler.Translator;

// using System.Text.RegularExpressions;
// using FFXIVClientStructs.FFXIV.Component.GUI;
using GagSpeak.Services;

namespace GagSpeak.Data;
public class GagGarbleManager : IDisposable
{
    private readonly    GagSpeakConfig          _config;        // Da configggg
    private readonly    GagService              _gagService;    // the gag service for getting the information off of the json stored within the obj
    private readonly    IpaParserEN_FR_JP_SP    _IPAParser;     // the class used to translate sent message to an IPA string that we can convert to gagspeak here. 
    public              List<Gag>               _activeGags;    // the list of gags that are currently active

    public GagGarbleManager(GagSpeakConfig config, GagService gagService, IpaParserEN_FR_JP_SP IPAParser) {
        _config = config;
        _gagService = gagService;
        _IPAParser = IPAParser;
        // Filter the _gagTypes list to only include gags with names in _config.playerInfo._selectedGagTypes (i know i could just invoke it, but im playing around with different methods)
        _activeGags = _config.playerInfo._selectedGagTypes
            .Where(gagType => _gagService._gagTypes.Any(gag => gag._gagName == gagType))
            .Select(gagType => _gagService._gagTypes.First(gag => gag._gagName == gagType))
            .ToList();
        // subscribe to our events
        _config.playerInfo._selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
    }

    public void Dispose() {
        _config.playerInfo._selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
    }


    /// <summary>
    /// Changes the gag list to match the equipped gags whenever a gag item is changed.
    /// </summary>
    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        // Update _activeGags when _config.playerInfo._selectedGagTypes changes
        _activeGags = _config.playerInfo._selectedGagTypes
            .Where(gagType => _gagService._gagTypes.Any(gag => gag._gagName == gagType))
            .Select(gagType => _gagService._gagTypes.First(gag => gag._gagName == gagType))
            .ToList();
    }

    /// <summary>
    /// Processes a message and returns the GagSpeak translation.
    /// <list type="bullet">
    /// <item><c>inputMessage</c><param name="inputMessage"> - The message to be translated.</param></item>
    /// </list> </summary>
    /// <returns> The GagSpeak translation of the message. </returns>
    public string ProcessMessage(string inputMessage) {
        string outputStr = "";
        try {
            outputStr = ConvertToGagSpeak(inputMessage);
            GagSpeak.Log.Debug($"[GagGarbleManager] Converted message to GagSpeak: {outputStr}");
        }
        catch (Exception e) {
            GagSpeak.Log.Error($"[GagGarbleManager] Error processing message: {e.Message}");
        }
        return outputStr;
    }

    /// <summary>
    /// Converts an IPA spaced message to a dictionary of words and their phonetics listing in order.
    /// <list type="bullet">
    /// <item><c>phonetics</c><param name="phonetics"> - The list of phonetic symbols to be translated.</param></item>
    /// </list> </summary>
    public string ConvertToGagSpeak(string inputMessage) {
        // firstly check to see if all gags are None, and if so, just return the original input string
        if (_activeGags.All(gag => gag._gagName == "None")) {
            GagSpeak.Log.Debug($"[GagGarbleManager] All gags are None, returning original message.");
            return inputMessage;
        }
        // Initialize the algorithmed scoped variables 
        GagSpeak.Log.Debug($"[GagGarbleManager] Converting message to GagSpeak, at least one gag is not None.");
        StringBuilder finalMessage = new StringBuilder(); // initialize a stringbuilder object so we dont need to make a new string each time
        bool skipTranslation = false;

        // begin the translation
        try {
            // Convert the message to a list of phonetics for each word
            List<Tuple<string, List<string>>> wordsAndPhonetics = _IPAParser.ToIPAList(inputMessage);
            // Iterate over each word and its phonetics
            foreach (Tuple<string, List<string>> entry in wordsAndPhonetics) {
                string word = entry.Item1; // create a variable to store the word (which includes its puncuation)
                // If the word is "*", then toggle skip translations
                if (word == "*") {
                    skipTranslation = !skipTranslation;
                    finalMessage.Append(word + " "); // append the word to the string
                    continue; // Skip the rest of the loop for this word
                }
                // If the word starts with "*", toggle skip translations and remove the "*"
                if (word.StartsWith("*")) {
                    skipTranslation = !skipTranslation;
                }
                // If the word ends with "*", remove the "*" and set a flag to toggle skip translations after processing the word
                bool toggleAfter = false;
                if (word.EndsWith("*")) {
                    toggleAfter = true;
                }
                // If the word is not to be translated, just add the word to the final message and continue
                if (!skipTranslation && word.Any(char.IsLetter)) {
                    // do checks for punctuation stuff
                    bool isAllCaps = word.All(c => !char.IsLetter(c) || char.IsUpper(c));       // Set to true if the full letter is in caps
                    bool isFirstLetterCaps = char.IsUpper(word[0]);
                    // Extract all leading and trailing punctuation
                    string leadingPunctuation = new string(word.TakeWhile(char.IsPunctuation).ToArray());
                    string trailingPunctuation = new string(word.Reverse().TakeWhile(char.IsPunctuation).Reverse().ToArray());
                    // Remove leading and trailing punctuation from the word
                    string wordWithoutPunctuation = word.Substring(leadingPunctuation.Length, word.Length - leadingPunctuation.Length - trailingPunctuation.Length);
                    // Convert the phonetics to GagSpeak if the list is not empty, otherwise use the original word
                    string gaggedSpeak = entry.Item2.Any() ? ConvertPhoneticsToGagSpeak(entry.Item2, isAllCaps, isFirstLetterCaps) : wordWithoutPunctuation;
                    // Add the GagSpeak to the final message
                    GagSpeak.Log.Debug($"[GagGarbleManager] Converted [{leadingPunctuation}] + [{word}] + [{trailingPunctuation}]");
                    finalMessage.Append(leadingPunctuation + gaggedSpeak + trailingPunctuation + " ");
                } else {
                    finalMessage.Append(word + " "); // append the word to the string
                }
                // If the word ended with "*", toggle skip translations now
                if (toggleAfter) {
                    skipTranslation = !skipTranslation;
                }
            }
        } catch (Exception e) {
            GagSpeak.Log.Error($"[GagGarbleManager] Error converting from IPA Spaced to final output. Puncutation error or other type possible : {e.Message}");
        }
        return finalMessage.ToString().Trim();
    }

    /// <summary>
    /// Converts a list of phonetics that spell out a word to their garbled string text
    /// <list type="bullet">
    /// <item><c>phonetics</c><param name="phonetics"> - The list of phonetic symbols to be translated.</param></item>
    /// </list> </summary>
    public string ConvertPhoneticsToGagSpeak(List<string> phonetics, bool isAllCaps, bool isFirstLetterCapitalized) {
        string outputString = "";
        // Iterate over each phonetic symbol
        foreach (string phonetic in phonetics) {
            try{
                // Find the index of the gag with the maximum muffle strength for the phonetic
                int GagIndex = _activeGags
                    .Select((gag, index) => new { gag, index })
                                        .Where(item => item.gag._muffleStrOnPhoneme.ContainsKey(phonetic) && !string.IsNullOrEmpty(item.gag._ipaSymbolSound[phonetic]))
                    .OrderByDescending(item => item.gag._muffleStrOnPhoneme[phonetic])
                    .FirstOrDefault()?.index ?? -1;
                // Now that we have the index, let's get the translation value for the phonetic
                string translationSound = _activeGags[GagIndex]._ipaSymbolSound[phonetic];
                // Add the symbol sound to the output string
                outputString += translationSound;
                // If the original word is all caps, make the output string all caps
                if (isAllCaps) { 
                    outputString = outputString.ToUpper();
                }
                // If the first letter of the word is capitalized, capitalize the first letter of the output string
                if (isFirstLetterCapitalized && outputString.Length > 0) {
                    outputString = char.ToUpper(outputString[0]) + outputString.Substring(1);
                }
            }
            catch (Exception e) {
                GagSpeak.Log.Error($"[GagGarbleManager] Error converting phonetic {phonetic} to GagSpeak: {e.Message}");
            }
        }
        // Return the output string
        return outputString;
    }
}
