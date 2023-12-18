using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GagSpeak.Translator;

// Class to convert English text to International Phonetic Alphabet (IPA) notation
public class IpaParserSpanish
{  
	
	private 			string 						data_file;		 // Path to the JSON file containing the conversion rules
	private 			Dictionary<string, string> 	obj;			 // Dictionary to store the conversion rules in JSON
	private readonly 	GagSpeakConfig 				_config;		 // The GagSpeak configuration
	
	/// <summary>
	/// Constructor for the EnglishToIPA class.
	/// <list type="Bullet">
	/// <item><c>usedLangDialect</c><param name="usedLangDialect"> - The language dialect to use for the IPA conversion</param></item>
	/// </list> </summary>
	public IpaParserSpanish(GagSpeakConfig config) {
		_config = config;

		// Set the path to the JSON file based on the language dialect
		switch (_config.languageDialect) {
			case "IPA_Spain":
			data_file = "./jsonFiles/es_ES.json";
			break;
			case "IPA_Mexico":
			data_file = "./jsonFiles/es_MX.json";
			break;
			default:
			data_file = "./jsonFiles/es_ES.json";
			break;
		}
		// Try to read the JSON file and deserialize it into the obj dictionary
		try
		{
			string json = File.ReadAllText(data_file);
			obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
		}
		catch (FileNotFoundException)
		{
			// If the file does not exist, log an error and initialize obj as an empty dictionary
			Console.WriteLine($"File does not exist: {data_file}");
			obj = new Dictionary<string, string>();
		}
		catch (Exception ex)
		{
			// If any other error occurs, log the error and initialize obj as an empty dictionary
			Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
			obj = new Dictionary<string, string>();
		}
	}

	/// <summary>
	/// Function for converting an input string to IPA notation.
	/// <list type="Bullet">
	/// <item><c>input</c><param name="input"> - The input string to convert</param></item>
	/// </list> </summary>
	/// <returns> The input string converted to IPA notation</returns>
    public string UpdateResult(string input) {
        string[] c_w = (PreprocessEng(input) + " ").Split(" ");
        string str = "";

        foreach (var word in c_w) {
            if (!string.IsNullOrEmpty(word)) {
                if (obj.ContainsKey(word)) {
                    string ipa = obj[word];
                    str += $"( {word} : {ipa} ) ";
                }
                else {
                    str += $"{word} ";
                }
            }
        }
        return str;
    }

	/// <summary>
	/// Function for preprocessing an input string by converting it to lower case and removing certain characters.
	/// <list type="Bullet">
	/// <item><c>x</c><param name="x"> - The input string to preprocess</param></item>
	/// </list> </summary>
	/// <returns> The preprocessed input string</returns>
	private string PreprocessEng(string x) {
		x = x.ToLower();
		x = Regex.Replace(x, @"\.", "");
		x = Regex.Replace(x, @"\,", "");
		x = Regex.Replace(x, @"\n", "");
		return x;
	}
}