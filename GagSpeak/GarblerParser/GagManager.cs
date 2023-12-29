using System;
using System.Collections.Generic;
using System.Linq;
using GagSpeak.Events;
using GagSpeak.Garbler.Translator;

// using System.Text.RegularExpressions;
// using FFXIVClientStructs.FFXIV.Component.GUI;
using GagSpeak.Services;

namespace GagSpeak.Data;
public class GagManager : IDisposable
{
    private readonly    GagSpeakConfig          _config;        // Da configggg
    private readonly    GagService              _gagService;    // the gag service for getting the information off of the json stored within the obj
    private readonly    IpaParserEN_FR_JP_SP    _IPAParser;     // the class used to translate sent message to an IPA string that we can convert to gagspeak here. 
    public              List<Gag>               _activeGags;

    public GagManager(GagSpeakConfig config, GagService gagService, IpaParserEN_FR_JP_SP IPAParser) {
        _config = config;
        _gagService = gagService;
        _IPAParser = IPAParser;
        // Filter the _gagTypes list to only include gags with names in _config.selectedGagTypes (i know i could just invoke it, but im playing around with different methods)
        _activeGags = _config.selectedGagTypes
            .Where(gagType => _gagService._gagTypes.Any(gag => gag._gagName == gagType))
            .Select(gagType => _gagService._gagTypes.First(gag => gag._gagName == gagType))
            .ToList();
        // subscribe to our events
        _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;

    }

    public void Dispose() {
        _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
        //_config.phoneticSymbolList.ItemChanged -= OnPhoneticSymbolListChanged
    }


    private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
        // Update _activeGags when _config.selectedGagTypes changes
        _activeGags = _config.selectedGagTypes
            .Where(gagType => _gagService._gagTypes.Any(gag => gag._gagName == gagType))
            .Select(gagType => _gagService._gagTypes.First(gag => gag._gagName == gagType))
            .ToList();
    }

    public string ProcessMessage(string inputMessage) {
        string outputStr = "";
        try {
            outputStr = ConvertToGagSpeak(inputMessage);
        }
        catch (Exception e) {
            GagSpeak.Log.Error($"[GagManager] Error processing message: {e.Message}");
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
            GagSpeak.Log.Debug($"[GagManager] All gags are None, returning original message.");
            return inputMessage;
        }

        GagSpeak.Log.Debug($"[GagManager] Converting message to GagSpeak, at least one gag is not None.");
        // Initialize the final message
        string finalMessage = "";
        // initialize a detection on if we are skipping translations or not
        bool skipTranslation = false;
        // begin the translation
        try {
            // Convert the message to a list of phonetics for each word
            List<Tuple<string, List<string>>> wordsAndPhonetics = _IPAParser.ToIPAList(inputMessage);
            // Iterate over each word and its phonetics
            foreach (Tuple<string, List<string>> entry in wordsAndPhonetics) {
                // if the word is either just *, or has * at the start or end of the word, toggle the skip translation flag
                if (entry.Item1 == "*" || entry.Item1.StartsWith("*") || entry.Item1.EndsWith("*")) {
                    skipTranslation = !skipTranslation;
                    finalMessage += entry.Item1 + " ";
                    continue;
                }
                // if we are skipping translation, just add the word to the final message and continue
                if (skipTranslation) {
                    finalMessage += entry.Item1 + " ";
                    continue;
                }
                // otherwise, extract puncuation, captialization, and convert!
                bool isFirstLetterCapitalized = char.IsUpper(entry.Item1[0]);
                char? leadingPunctuation = char.IsPunctuation(entry.Item1[0]) ? entry.Item1[0] : null; // punctuation at the start of the word
                char? trailingPunctuation = char.IsPunctuation(entry.Item1[^1]) ? entry.Item1[^1] : null; // punctuation at the end of the word
                // Convert the phonetics to GagSpeak if the list is not empty, otherwise use the original word
                string gagSpeak = entry.Item2.Any() ? ConvertPhoneticsToGagSpeak(entry.Item2, isFirstLetterCapitalized) : entry.Item1;
                // Add the GagSpeak to the final message
                finalMessage += (leadingPunctuation?.ToString() ?? "") + gagSpeak + (trailingPunctuation?.ToString() ?? "") + " ";
            }
        }
        catch (Exception e) {
            GagSpeak.Log.Error($"[GagManager] Error converting from IPA Spaced to final output. Puncutation error or other type possible : {e.Message}");
        }
        // Return the final message 
        return finalMessage.Trim();
    }

    /// <summary>
    /// Converts a list of phonetics that spell out a word to their garbled string text
    /// <list type="bullet">
    /// <item><c>phonetics</c><param name="phonetics"> - The list of phonetic symbols to be translated.</param></item>
    /// </list> </summary>
    public string ConvertPhoneticsToGagSpeak(List<string> phonetics, bool isFirstLetterCapitalized) {
        string outputString = "";
        // Iterate over each phonetic symbol
        foreach (string phonetic in phonetics) {
            try{
                // Find the index of the gag with the maximum muffle strength for the phonetic
                int GagIndex = _activeGags
                    .Select((gag, index) => new { gag, index })
                    .Where(item => item.gag._muffleStrOnPhoneme.ContainsKey(phonetic))
                    .OrderByDescending(item => item.gag._muffleStrOnPhoneme[phonetic])
                    .FirstOrDefault()?.index ?? -1;
                // Now that we have the index, let's get the translation value for the phonetic
                string translationSound = _activeGags[GagIndex]._ipaSymbolSound[phonetic];
                // Add the symbol sound to the output string
                outputString += translationSound;

                // If the first letter of the word is capitalized, capitalize the first letter of the output string
                if (isFirstLetterCapitalized && outputString.Length > 0) {
                    outputString = char.ToUpper(outputString[0]) + outputString.Substring(1);
                }
            }
            catch (Exception e) {
                GagSpeak.Log.Error($"[GagManager] Error converting phonetic {phonetic} to GagSpeak: {e.Message}");
            }
        }
        // Return the output string
        return outputString;
    }
}
