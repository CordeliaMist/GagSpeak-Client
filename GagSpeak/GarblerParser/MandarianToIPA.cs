using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace GagSpeak.Translator;
// Class to convert Mandarian text to International Phonetic Alphabet (IPA) notation
public class IpaParserMandarian
{
    private             string                      data_file;       // Path to the JSON file containing the conversion rules
    private             Dictionary<string, string>  obj;             // Dictionary to store the conversion rules in JSON
    private readonly    GagSpeakConfig              _config;         // The GagSpeak configuration
    private             DalamudPluginInterface      _pluginInterface; // used to get the plugin interface

    public IpaParserMandarian(GagSpeakConfig config, DalamudPluginInterface pluginInterface) {
        _config = config;
        _pluginInterface = pluginInterface;
        // Set the path to the JSON file based on the language dialect
        switch (_config.languageDialect) {
            case "IPA_zu_Hans":
                data_file = "GarblerParser\\jsonFiles\\zu_hans.json"; break; // french, in the standard french dialect
            case "IPA_zu_Hant":
                data_file = "GarblerParser\\jsonFiles\\zu_hant.json"; break; // french, using the quebec dialect
            default:
                data_file = "GarblerParser\\jsonFiles\\zu_hans.json"; break; // french, in the standard french dialect
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
                str += "(" + search_words + " " + obj[search_words] + " )";
                // Increment the index by the found index
                i += words_index;
            }
            // If the character does not exist in the dictionary
            else {
                // Add the character to the result string
                str += c_w[i] + " ";
            }
        }

        // Return the formatted result string
        return FormatMain(str);
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
}
