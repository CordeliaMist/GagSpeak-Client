using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System.Linq;
using GagSpeak.Garbler.PhonemeData;

namespace GagSpeak.Garbler.Translator;

// Class to convert English, French, Japanese, and Spanish text to International Phonetic Alphabet (IPA) notation
public class IpaParserEN_FR_JP_SP
{
	private 			string 						data_file;		 	// Path to the JSON file containing the conversion rules
	private 			Dictionary<string, string> 	obj;			 	// Dictionary to store the conversion rules in JSON
	private readonly 	GagSpeakConfig 				_config;		 	// The GagSpeak configuration
    private             DalamudPluginInterface  _pluginInterface;		// used to get the plugin interface
	// private List<string> CombinationsEng = new List<string> { "aʊ", "oʊ", "eɪ", "aɪ", "dʒ", "tʃ", "ɔɪ", "ɪə" }; 
	// private HashSet<string> uniqueSymbols = new HashSet<string>();
	public string uniqueSymbolsString = "";
	
	
	public IpaParserEN_FR_JP_SP(GagSpeakConfig config, DalamudPluginInterface pluginInterface) {
		_config = config;
		_pluginInterface = pluginInterface;

		// Set the path to the JSON file based on the language dialect
		GSLogger.LogType.Debug($"[IPA Parser] Language dialect: {_config.languageDialect}");
		switch (_config.languageDialect) {
			case "IPA_US":		data_file = "GarblerCore\\jsonFiles\\en_US.json"; break;
			case "IPA_UK":		data_file = "GarblerCore\\jsonFiles\\en_UK.json"; break;
			case "IPA_FRENCH":	data_file = "GarblerCore\\jsonFiles\\fr_FR.json"; break;
			case "IPA_QUEBEC":	data_file = "GarblerCore\\jsonFiles\\fr_QC.json"; break;
			case "IPA_JAPAN":	data_file = "GarblerCore\\jsonFiles\\ja.json";	break;
			case "IPA_SPAIN":	data_file = "GarblerCore\\jsonFiles\\es_ES.json"; break;
			case "IPA_MEXICO":	data_file = "GarblerCore\\jsonFiles\\es_MX.json";	break;
			default:			data_file = "GarblerCore\\jsonFiles\\en_US.json"; break;
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
			GSLogger.LogType.Debug($"[IPA Parser] File read: {data_file}");
		}
		catch (FileNotFoundException) {
			// If the file does not exist, log an error and initialize obj as an empty dictionary
			GSLogger.LogType.Debug($"[IPA Parser] File does not exist: {data_file}");
			obj = new Dictionary<string, string>();
		}
		catch (Exception ex) {
			// If any other error occurs, log the error and initialize obj as an empty dictionary
			GSLogger.LogType.Debug($"[IPA Parser] An error occurred while reading the file: {ex.Message}");
			obj = new Dictionary<string, string>();
		}

		// extraction time
		try {
			///////// This method is used for generating new unique lists in new dictionaries //////////
			// ExtractUniquePhonetics();
			// // convert to string
			// uniqueSymbolsString = string.Join(",", uniqueSymbols);

			//////// New Method of simply getting it from the master list in phoneticlists.cs //////////
			SetUniqueSymbolsString();
		}
		catch (Exception ex) {
			GSLogger.LogType.Debug($"[IPA Parser] An error occurred while extracting unique phonetics: {ex.Message}");
		}
	}

	/// <summary> Preprocess input string by converting it to lower case and removing certain characters.
	/// <list type="Bullet"><item><c>x</c><param name="x"> - String to preprocess</param></item></list>
	/// </summary> <returns> The preprocessed input string</returns>
	private string Preprocess(string x) {
		x = Regex.Replace(x, @"\n", "");
		return x;
	}

	/// <summary> Function for converting an input string to IPA notation.
	/// <para> THIS IS FOR UI DISPLAY PURPOSES, Hince the DASHED SPACE BETWEEN PHONEMES </para>
	/// <list type="Bullet"><item><c>input</c><param name="input"> - String to convert</param></item></list>
	/// </summary><returns> The input string converted to IPA notation</returns>
    public string ToIPAStringDisplay(string input) {
		GSLogger.LogType.Debug($"[IPA Parser] Parsing IPA string from message: {input}");
		// split the string by the spaces between words
        string[] c_w = (Preprocess(input) + " ").Split(" ");
        // the new string to output
		string str = "";
		// iterate over each word in the input string
        foreach (var word in c_w) {
			// if the word is not empty
            if (!string.IsNullOrEmpty(word)) {
				// remove punctuation from the word
				string wordWithoutPunctuation = Regex.Replace(word, @"\p{P}", "");
				wordWithoutPunctuation = wordWithoutPunctuation.ToLower();
				// if the word exists in the dictionary
                if (obj.ContainsKey(wordWithoutPunctuation)) {
					// append the word and its phonetic to the string
                    str += $"( {word} : {obj[wordWithoutPunctuation]} ) ";
                }
				// if not, append the word by itself
                else {
                    str += $"{word} ";
                }
            }
        }
		GSLogger.LogType.Debug($"[IPA Parser] Parsed IPA string: {str}");
		//str = ConvertToSpacedPhonetics(str);
        return str;
    }

	/// <summary>
	/// The same as ToIPAStringDisp but shows the next step where its split by dashes
	/// </summary>
	public string ToIPAStringSpacedDisplay(string input) {
		string str = input;
		List<Tuple<string, List<string>>> parsedStr = ToIPAList(str);
		str = ConvertDictionaryToSpacedPhonetics(parsedStr);
		return str;
	}

	/// <summary> Converts an input string to a dictionary where each word maps to a list of its phonetic symbols.
	/// <param name="input">The input string to convert.</param>
	/// <returns>A dictionary where each word from the input string maps to a list of its phonetic symbols.</returns></summary>
	public List<Tuple<string, List<string>>> ToIPAList(string input) {
		// Log the input string
		GSLogger.LogType.Debug($"[IPA Parser] Parsing IPA string from original message: {input}");
		// Split the input string into words
		string[] c_w = (Preprocess(input) + " ").Split(" ");
		// Initialize the result dictionary
		List<Tuple<string, List<string>>> result = new List<Tuple<string, List<string>>>();
		// Iterate over each word in the input string
		foreach (var word in c_w) {
			// If the word is not empty
			if (!string.IsNullOrEmpty(word)) {
				// remove punctuation from the word
				string wordWithoutPunctuation = Regex.Replace(word, @"\p{P}", "");
				wordWithoutPunctuation = wordWithoutPunctuation.ToLower();
				// If the word exists in the obj dictionary
				if (obj.ContainsKey(wordWithoutPunctuation)) {
					// Retrieve the phonetic representation of the word
					string phonetics = obj[wordWithoutPunctuation];
					// Process the phonetic representation to remove unwanted characters
					phonetics = phonetics.Replace("/", "");
					if (phonetics.Contains(",")) {
						phonetics = phonetics.Split(',')[0].Trim();
					}
					phonetics = phonetics.Replace("ˈ", "").Replace("ˌ", "");
					// Initialize a list to hold the phonetic symbols
					List<string> phoneticSymbols = new List<string>();
					// Iterate over the phonetic symbols
					for (int i = 0; i < phonetics.Length; i++) {
						// Check for known combinations of symbols
						if (i < phonetics.Length - 1) {
							// first 
							string possibleCombination = phonetics.Substring(i, 2);
							int index = GetMasterListBasedOnDialect().FindIndex(t => t == possibleCombination);
							if (index != -1) {
								// If a combination is found, add it to the list and skip the next character
								phoneticSymbols.Add(GetMasterListBasedOnDialect()[index]);
								i++;
							} else {
								// If no combination is found, add the current character to the list
								phoneticSymbols.Add(phonetics[i].ToString());
							}
						} else {
							// Add the last character to the list
							phoneticSymbols.Add(phonetics[i].ToString());
						}
					}
					// Add the list of phonetic symbols to the result dictionary
					result.Add(Tuple.Create(word, phoneticSymbols));
				} else {
					// If the word does not exist in the obj dictionary, add an empty list to the result dictionary
					result.Add(Tuple.Create(word, new List<string>()));
				}
			}
		}
		GSLogger.LogType.Debug("[IPA Parser] String parsed to list successfully: " +
						$"{string.Join(", ", result.Select(t => $"{t.Item1}: [{string.Join(", ", t.Item2)}]"))}");
		return result;
	}

	/// <summary>
	/// Converts a dictionary of words and their phonetic symbols to a string of spaced phonetics
	/// </summary>
	public string ConvertDictionaryToSpacedPhonetics(List<Tuple<string, List<string>>> inputTupleList) {
		// Initialize a string to hold the result
		string result = "";

		// Iterate over each entry in the dictionary
		foreach (Tuple<string, List<string>> entry in inputTupleList) {
        // If the list has content, join the phonetic symbols with a dash
        // Otherwise, just use the normal word
        string phonetics = entry.Item2.Any() ? string.Join("-", entry.Item2) : entry.Item1;

        // Add the phonetics to the result string
        result += $"{phonetics} ";
		}

		// Return the result string
		return result.Trim();
	}

	/// <summary>
	/// Returns the master list of phonemes for the selected language
	/// </summary>
	public List<string> GetMasterListBasedOnDialect() {
		switch (_config.languageDialect) {
			case "IPA_UK":      return PhonemMasterLists.MasterListEN_UK;
			case "IPA_US":      return PhonemMasterLists.MasterListEN_US;
			case "IPA_SPAIN":   return PhonemMasterLists.MasterListSP_SPAIN;
			case "IPA_MEXICO":  return PhonemMasterLists.MasterListSP_MEXICO;
			case "IPA_FRENCH":  return PhonemMasterLists.MasterListFR_FRENCH;
			case "IPA_QUEBEC":  return PhonemMasterLists.MasterListFR_QUEBEC;
			case "IPA_JAPAN":   return PhonemMasterLists.MasterListJP;
			default:            throw new Exception("Invalid language Dialect");
		}
	}

	/// <summary>
	/// Sets the uniqueSymbolsString to the master list of phonemes for the selected language
	/// </summary>
	public void SetUniqueSymbolsString() {
        switch (_config.languageDialect) {
            case "IPA_UK":      uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListEN_UK); break;
            case "IPA_US":      uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListEN_US); break;
            case "IPA_SPAIN":   uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListSP_SPAIN); break;
            case "IPA_MEXICO":  uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListSP_MEXICO); break;
            case "IPA_FRENCH":  uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListFR_FRENCH); break;
            case "IPA_QUEBEC":  uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListFR_QUEBEC); break;
            case "IPA_JAPAN":   uniqueSymbolsString = string.Join(",", PhonemMasterLists.MasterListJP); break;
            default:            throw new Exception("Invalid language Dialect");
        }
	}
	/*
	public string ConvertToSpacedPhonetics(string input) {
		GSLogger.LogType.Debug($"[IPA Parser] Converting phonetics to spaced phonetics: {input}");
		string output = "";
		// Add a placeholder at the start and end of the input string
		input = " " + input + " ";
		// Split the input into phonetic representations
		string[] phoneticRepresentations = Regex.Split(input, @"(?<=\))\s*(?=\()");
		// Iterate over the phonetic representations
		foreach (var representation in phoneticRepresentations) {
			GSLogger.LogType.Debug($"[IPA Parser] Phonetic representation: {representation}");
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
						int index = PhonemMasterLists.MasterListEN_US.FindIndex(t => t == possibleCombination);
						if (index != -1) {
							spacedPhonetics += PhonemMasterLists.MasterListEN_US[index] + "-"; // Use the phoneme from the Translator object
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
		GSLogger.LogType.Debug($"[IPA Parser] Converted phonetics to spaced phonetics: {output}");
		// Remove the trailing space and return the output
		return output.TrimEnd();
	}*/

	/// <summary>
	/// Sets the uniqueSymbolsString to the master list of phonemes for the selected language
	/// </summary>

	// helper function for extracting new languages phonetic dictionaries. not actually used in the plugin
	// public List<string> ExtractUniquePhonetics() {
	// 	// Iterate over each word in the dictionary
	// 	foreach (KeyValuePair<string, string> entry in obj) {
	// 		// Extract the phonetic symbols between the slashes
    //     	string phonetics = entry.Value.Replace("/", "").Replace(",", "");
	// 		// Check for known combinations first
	// 		for (int i = 0; i < phonetics.Length - 1; i++) {
	// 			string possibleCombination = phonetics.Substring(i, 2);
	// 			if (CombinationsEng.Contains(possibleCombination)) {
	// 				uniqueSymbols.Add(possibleCombination);
	// 				i++; // Skip next character as it's part of the combination
	// 			} else {
	// 				if (phonetics[i] != ',') { // Skip commas
	// 					uniqueSymbols.Add(phonetics[i].ToString());
    //             	}
	// 			}
	// 		}
	// 		// Check the last character if it wasn't part of a combination
    //     	if (!uniqueSymbols.Contains(phonetics[^1].ToString()) && phonetics[^1] != ',') {
	// 			uniqueSymbols.Add(phonetics[^1].ToString());
	// 		}
	// 	}
	// 	return uniqueSymbols.ToList();
	// }
}