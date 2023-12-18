using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Dalamud.Plugin;
using Newtonsoft.Json;
using OtterGui;
using ImGuiNET;
using System.Text;
using System.Linq;
using GagSpeak.Data;

namespace GagSpeak.Translator;

// Class to convert English, French, Japanese, and Spanish text to International Phonetic Alphabet (IPA) notation
public class IpaParserEN_FR_JP_SP
{
	private 			string 						data_file;		 	// Path to the JSON file containing the conversion rules
	private 			Dictionary<string, string> 	obj;			 	// Dictionary to store the conversion rules in JSON
	private readonly 	GagSpeakConfig 				_config;		 	// The GagSpeak configuration
    private             DalamudPluginInterface  _pluginInterface;		// used to get the plugin interface
		// Define known combinations
	private List<string> CombinationsEng = new List<string> { "ɑː", "ɔː", "iː", "eɪ", "uː", "juː", "ɪə", "eə", "ʊə",
	"ɑː", "aɪ", "ɔɪ", "oʊ", "aʊ", "ɑːr", "tʃ", "dʒ"};

		// List to store unique phonetic symbols
	private HashSet<string> uniqueSymbols = new HashSet<string>();
	public string uniqueSymbolsString = "";
	
	
	public IpaParserEN_FR_JP_SP(GagSpeakConfig config, DalamudPluginInterface pluginInterface) {
		_config = config;
		_pluginInterface = pluginInterface;

		// Set the path to the JSON file based on the language dialect
		switch (_config.languageDialect) {
			case "IPA_US":
			data_file = "GarblerParser\\jsonFiles\\en_US.json";
			break;
			case "IPA_UK":
			data_file = "GarblerParser\\jsonFiles\\en_UK.json";
			break;
			case "IPA_FRENCH":
			data_file = "GarblerParser\\jsonFiles\\fr_FR.json";
			break;
			case "IPA_QUEBEC":
			data_file = "GarblerParser\\jsonFiles\\fr_QC.json";
			break;
			case "IPA_JAPAN":
			data_file = "GarblerParser\\jsonFiles\\ja.json";
			break;
			case "IPA_SPAIN":
			data_file = "GarblerParser\\jsonFiles\\es_ES.json";
			break;
			case "IPA_MEXICO":
			data_file = "GarblerParser\\jsonFiles\\es_MX.json";
			break;
			default:
			data_file = "GarblerParser\\jsonFiles\\en_US.json";
			break;
		}
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

		// extraction time
		try {
			ExtractUniquePhonetics();
			// convert to string
			uniqueSymbolsString = string.Join(",", uniqueSymbols);
		}
		catch (Exception ex) {
			GagSpeak.Log.Debug($"[IPA Parser] An error occurred while extracting unique phonetics: {ex.Message}");
		}
	}

	/// <summary> Function for converting an input string to IPA notation.
	/// <list type="Bullet"><item><c>input</c><param name="input"> - String to convert</param></item></list>
	/// </summary><returns> The input string converted to IPA notation</returns>
    public string UpdateResult(string input) {
		GagSpeak.Log.Debug($"[IPA Parser] Parsing IPA string from message: {input}");
		// split the string by the spaces between words
        string[] c_w = (Preprocess(input) + " ").Split(" ");
        // the new string to output
		string str = "";
		// iterate over each word in the input string
        foreach (var word in c_w) {
			// if the word is not empty
            if (!string.IsNullOrEmpty(word)) {
				// if the word exists in the dictionary
                if (obj.ContainsKey(word)) {
					// append the word and its phonetic to the string
                    str += $"( {word} : {obj[word]} ) ";
                }
				// if not, append the word by itself
                else {
                    str += $"{word} ";
                }
            }
        }
		GagSpeak.Log.Debug($"[IPA Parser] Parsed IPA string: {str}");
		// return the formatted string
        UTF8Encoding utf8 = new();


        // // Convert a string to a byte array
        // byte[] bytes = utf8.GetBytes(str);
		// GagSpeak.Log.Debug($"[IPA Parser] Parsed IPA string bytes: {bytes}");

        // // Convert a byte array back to a string
        // string decodedText = utf8.GetString(bytes);
		// GagSpeak.Log.Debug($"[IPA Parser] Parsed IPA string decoded: {decodedText}");
        // ImGui.TextUnformatted(decodedText);

		str = ConvertToSpacedPhonetics(str);
        return str;
    }

	/// <summary> Preprocess input string by converting it to lower case and removing certain characters.
	/// <list type="Bullet"><item><c>x</c><param name="x"> - String to preprocess</param></item></list>
	/// </summary> <returns> The preprocessed input string</returns>
	private string Preprocess(string x) {
		x = x.ToLower();
		x = Regex.Replace(x, @"\.", "");
		x = Regex.Replace(x, @"\,", "");
		x = Regex.Replace(x, @"\n", "");
		return x;
	}

	public List<string> ExtractUniquePhonetics()
	{
		// Iterate over each word in the dictionary
		foreach (KeyValuePair<string, string> entry in obj)
		{
			// Extract the phonetic symbols between the slashes
        	string phonetics = entry.Value.Replace("/", "").Replace(",", "");

			// Check for known combinations first
			for (int i = 0; i < phonetics.Length - 1; i++)
			{
				string possibleCombination = phonetics.Substring(i, 2);
				if (CombinationsEng.Contains(possibleCombination))
				{
					uniqueSymbols.Add(possibleCombination);
					i++; // Skip next character as it's part of the combination
				}
				else
				{
					// Skip commas
					if (phonetics[i] != ',')
					{
						uniqueSymbols.Add(phonetics[i].ToString());
                }
				}
			}

			// Check the last character if it wasn't part of a combination
        	if (!uniqueSymbols.Contains(phonetics[^1].ToString()) && phonetics[^1] != ',')
			{
				uniqueSymbols.Add(phonetics[^1].ToString());
			}
		}

		return uniqueSymbols.ToList();
	}

	public string ConvertToSpacedPhonetics(string input)
	{
		GagSpeak.Log.Debug($"[IPA Parser] Converting phonetics to spaced phonetics: {input}");
		string output = "";
		// Add a placeholder at the start and end of the input string
		input = " " + input + " ";
		// Split the input into phonetic representations
		string[] phoneticRepresentations = Regex.Split(input, @"(?<=\))\s*(?=\()");
		// Iterate over the phonetic representations
		foreach (var representation in phoneticRepresentations) {
			GagSpeak.Log.Debug($"[IPA Parser] Phonetic representation: {representation}");
			// Remove the placeholders
			string phonetics = representation.Trim();
			// Check if the representation has a phonetic representation
			if (phonetics.StartsWith("(") && phonetics.EndsWith(")")) {
				// Extract the phonetic representation
				phonetics = phonetics.Trim('(', ')').Split(':')[1].Trim().Trim('/');
				// If there are multiple phonetic representations, only take the first one
				if (phonetics.Contains(",")) {
					phonetics = phonetics.Split(',')[0].Trim();
				}
				// Remove the primary and secondary stress symbols (delete this later if we find a use for them)
				phonetics = phonetics.Replace("ˈ", "").Replace("ˌ", "");
				// Initialize an empty string to hold the spaced out phonetics
				string spacedPhonetics = "";
				// Iterate over the phonetic symbols
				for (int i = 0; i < phonetics.Length; i++) {
					// Check for known combinations first
					if (i < phonetics.Length - 1) {
						string possibleCombination = phonetics.Substring(i, 2);
						int index = PhonemMasterLists.MasterListEN_US.FindIndex(t => t.Phoneme == possibleCombination);
						if (index != -1) {
							spacedPhonetics += PhonemMasterLists.MasterListEN_US[index].Phoneme + "-"; // Use the phoneme from the Translator object
							i++; // Skip next character as it's part of the combination
						} else {
							spacedPhonetics += phonetics[i] + "-";
						}
					} else {
						spacedPhonetics += phonetics[i] + "-";
					}
				}
				// Remove the trailing "-" and add the spaced out phonetics to the output
				output += spacedPhonetics.TrimEnd('-') + " ";
			} else {
				// If the representation doesn't have a phonetic representation, add it to the output as is
				output += phonetics + " ";
			}
		}
		GagSpeak.Log.Debug($"[IPA Parser] Converted phonetics to spaced phonetics: {output}");
		// Remove the trailing space and return the output
		return output.TrimEnd();
	}
}