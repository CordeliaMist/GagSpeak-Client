using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using Newtonsoft.Json;
using GagSpeak.Garbler.PhonemeData;


// This file has no current use, but is here for any potential future implementations of the IPA parser.

namespace GagSpeak.Garbler.Translator;
// Class to convert Mandarian text to International Phonetic Alphabet (IPA) notation
public class IpaParserMandarian
{
    private             string                      data_file;       // Path to the JSON file containing the conversion rules
    private             Dictionary<string, string>  obj;             // Dictionary to store the conversion rules in JSON
    private readonly    GagSpeakConfig              _config;         // The GagSpeak configuration
    private             DalamudPluginInterface      _pluginInterface; // used to get the plugin interface
	private List<string> CombinationsEng = new List<string> { "ɒː", "e", "iː", "uː", "eː", "ej", "ɒːj", "aw", "t͡ʃ", "d͡ʒ", "ts" }; 

		// List to store unique phonetic symbols
	private HashSet<string> uniqueSymbols = new HashSet<string>();
	public string uniqueSymbolsString = "";
    public IpaParserMandarian(GagSpeakConfig config, DalamudPluginInterface pluginInterface) {
        _config = config;
        _pluginInterface = pluginInterface;
        // Set the path to the JSON file based on the language dialect
        switch (_config.languageDialect) {
            case "IPA_zu_Hans":
                data_file = "GarblerCore\\jsonFiles\\zu_hans.json"; break; // french, in the standard french dialect
            case "IPA_zu_Hant":
                data_file = "GarblerCore\\jsonFiles\\zu_hant.json"; break; // french, using the quebec dialect
            default:
                data_file = "GarblerCore\\jsonFiles\\zu_hans.json"; break; // french, in the standard french dialect
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
			ExtractUniquePhonetics();
			// convert to string
			uniqueSymbolsString = string.Join(",", uniqueSymbols);
		}
		catch (Exception ex) {
			GSLogger.LogType.Debug($"[IPA Parser] An error occurred while extracting unique phonetics: {ex.Message}");
		}

    }

    /// <summary> Function for converting an input string to IPA notation.
    /// <list type="Bullet"><item><c>input</c><param name="input"> - string to convert</param></item></list>
    /// </summary><returns> The input string converted to IPA notation</returns>
    public string UpdateResult(string input) {
        string c_w = input;  
        string str = "";
        // Iterate over each character in the input string
        for (int i = 0; i < c_w.Length; i++) {
            // If the character exists in the dictionary
            if (obj.ContainsKey(c_w[i].ToString())) {
                // Initialize an array to store the words
                string[] s_words = new string[6];
                // Assign the first word
                s_words[0] = c_w[i].ToString();
                // Iterate over the next 5 characters
                for (int j = 1; j < 6; j++) {
                    // If the index is within the string length
                    if (i + j < c_w.Length) {
                        // Add the character to the word
                        s_words[j] = s_words[j - 1] + c_w[i + j];
                    }
                }
                // Find the last index of a word that exists in the dictionary
                int words_index = Array.FindLastIndex(s_words, sw => obj.ContainsKey(sw));
                // Get the word at the found index
                string search_words = s_words[words_index];
                // Add the word and its IPA notation to the result string
                str += $"( {search_words} : {obj[search_words]} ) ";
                //str += "(" + search_words + " " + obj[search_words] + " )";
                // Increment the index by the found index
                i += words_index;
            }
            // If the character does not exist in the dictionary
            else {
                // Add the character to the result string
                str += $"{c_w[i]} ";
                //str += c_w[i] + " ";
            }
        }

        // Return the formatted result string
        str = FormatMain(str);
        str = ConvertToSpacedPhonetics(str);
        return str;
    }

    /// <summary> Function for formatting the output string based on the selected dialect.
    /// <list type="Bullet">
    /// <item><c>t_str</c><param name="t_str"> - The input string to format</param></item>
    /// </list> </summary>
    /// <returns> The formatted output string</returns>
    private string FormatMain(string t_str) {
        string f_str = t_str;

        if (_config.languageDialect == "IPA_num") f_str = FormatIPANum(t_str);               // kuɔ35
        else if (_config.languageDialect == "IPA_org") f_str = FormatIPAOrg(t_str);          // kuɔ˧˥
        else if (_config.languageDialect == "Jyutping_num") f_str = FormatJyutpingNum(t_str);// kuɔ2
        else if (_config.languageDialect == "Jyutping") f_str = FormatJyutping(t_str);       // kuɔˊ

        return f_str;
    }

    private string FormatIPANum(string x) {         // kuɔ35
        x = x.Replace("˥", "5");
        x = x.Replace("˧", "3");
        x = x.Replace("˨", "2");
        x = x.Replace("˩", "1");
        x = x.Replace(":", "");
        return x;
    }

    private string FormatIPAOrg(string x) {         // kuɔ˧˥
        return x;
    }
    private string FormatJyutpingNum(string x) {    // kuɔ2
        x = FormatJyutping(x);

        x = x.Replace("ˉ", "1");
        x = x.Replace("ˊ", "2");
        x = x.Replace("ˇ", "3");
        x = x.Replace("ˋ", "4");
        x = x.Replace("˙", "˙");
        return x;
    }

    private string FormatJyutping(string x) {       // kuɔˊ
        x = x.Replace("˥˥", "ˉ");
        x = x.Replace("˧˥", "ˊ");
        x = x.Replace("˨˩˦", "ˇ");
        x = x.Replace("˨˩˩", "ˇ");
        x = x.Replace("˧˥", "ˇ");
        x = x.Replace("˥˩", "ˋ");
        x = x.Replace("˥˧", "ˋ");
        x = x.Replace("˨˩", "˙");
        x = x.Replace("˧˩", "˙");
        x = x.Replace("˦˩", "˙");
        x = x.Replace("˩˩", "˙");
        x = x.Replace("˧", "˙");
        x = x.Replace(":", "");
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
	}
}
