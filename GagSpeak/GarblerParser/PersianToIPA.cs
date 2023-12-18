using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace GagSpeak.Translator;
// Class to convert Persian text to International Phonetic Alphabet (IPA) notation
public class IpaParserPersian
{
    private             string                      data_file;       // Path to the JSON file containing the conversion rules
    private             Dictionary<string, string>  obj;             // Dictionary to store the conversion rules in JSON
    private readonly    GagSpeakConfig              _config;         // The GagSpeak configuration
    private             DalamudPluginInterface      _pluginInterface; // used to get the plugin interface

    public IpaParserPersian(GagSpeakConfig config, DalamudPluginInterface pluginInterface)
    {
        _config = config;
        _pluginInterface = pluginInterface;
        // Set the path to the JSON file based on the language dialect
        data_file = "GarblerParser\\jsonFiles\\fa.json";
		// Try to read the JSON file and deserialize it into the obj dictionary
		try {
			// Assuming you have an instance of DalamudPluginInterface named _pluginInterface
			string jsonFilePath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, data_file);
			// read the file
			string json = File.ReadAllText(jsonFilePath);
			// deserialize the json into the obj dictionary
			obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
			// let log know that the file was read
			GagSpeak.Log.Debug($"[IPA Parser] File read: {jsonFilePath}");
		}
		catch (FileNotFoundException) {
			// If the file does not exist, log an error and initialize obj as an empty dictionary
			GagSpeak.Log.Debug($"[IPA Parser] File does not exist: {data_file}");
			obj = new Dictionary<string, string>();
		}
		catch (Exception ex) {
			// If any other error occurs, log the error and initialize obj as an empty dictionary
			GagSpeak.Log.Debug($"[IPA Parser] An error occurred while reading the file: {ex.Message}");
			obj = new Dictionary<string, string>();
		}
    }

    /// <summary> Function for converting an input string to IPA notation.
    /// <list type="Bullet"><item><c>input</c><param name="input"> - string to convert</param></item></list>
    /// </summary><returns> The input string converted to IPA notation</returns>
    public string UpdateResult(string input) {
        // Preprocessing the input and splitting it into words
        string[] c_w = (Preprocess(input) + " ").Split(" ");
        string str = "";
        // Looping through each word
        for (int i = 0; i < c_w.Length; i++) {
            string word = c_w[i];
            // Checking if the word is not empty
            if (!string.IsNullOrEmpty(word)) {
                // Checking if the word exists in the dictionary
                if (obj.ContainsKey(word)) {
                    // Initializing an array to store potential multi-word entries
                    string[] s_words = new string[6];
                    // Adding the first word to the array
                    s_words[0] = c_w[i];
                    // iterating through the next 5 words
                    for (int j = 1; j < 6; j++) {
                        // if index is within the bounds of the array
                        if (i + j < c_w.Length) {
                            // Adding the next word to the array
                            s_words[j] = s_words[j - 1] + " " + c_w[i + j];
                        }
                    }
                    // Find the last index of a word that exists in the dictionary
                    int words_index = Array.FindLastIndex(s_words, sw => obj.ContainsKey(sw));
                    // Getting the word from the dictionary at found index
                    string search_words = s_words[words_index];
                    // Adding the word and its corresponding value in the dictionary to the result string
                    str += "{ " + search_words + " - " + obj[search_words] + " }";
                    // Incrementing the index by the number of words in the multi-word entry
                    i += words_index;
                } else {
                    // If the word DNE in dictionary, add to the result string as original text
                    str += word + " ";
                }
            }
        }
        return str;
    }

	/// <summary> Preprocess input string by converting it to lower case and removing certain characters.
	/// <list type="Bullet"><item><c>x</c><param name="x"> - String to preprocess</param></item></list>
	/// </summary> <returns> The preprocessed input string</returns>
	private string Preprocess(string x) {
        // Converting to lowercase
		x = x.ToLower();
        // Removing all punctuation and newlines
		x = Regex.Replace(x, @"\.", "");
		x = Regex.Replace(x, @"\,", "");
		x = Regex.Replace(x, @"\n", "");
		return x;
	}
}