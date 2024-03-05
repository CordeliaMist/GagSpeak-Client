using System;
using System.Collections.Generic;                   // Provides classes for defining generic collections
using System.IO;
using Dalamud.Plugin;
using Newtonsoft.Json;
using GagSpeak.Events;                              // Contains event classes used in the GagSpeak application
using GagSpeak.Garbler.PhonemeData;
using GagSpeak.Gagsandlocks;                 // Contains phoneme data used in the GagSpeak application

namespace GagSpeak.Services;
/// <summary> Service for managing the gags. </summary>
public class GagService : IDisposable
{
    private readonly    GagSpeakConfig                                                      _config;            // The GagSpeak configuration
    private readonly    DalamudPluginInterface                                              _pluginInterface;	// used to get the plugin interface
    private             LanguageChangedEvent                                                _languageChanged;   // Event for when the language changes
	private 			Dictionary<string, Dictionary<string, Dictionary<string, string>>>  _gagData;			// Dictionary to store the conversion rules in JSON
    public              List<Gag>                                                           _gagTypes;          // Dictionary of gag types

    public GagService(GagSpeakConfig config, DalamudPluginInterface pluginInterface, LanguageChangedEvent languageChanged) {
        _config = config;
        _pluginInterface = pluginInterface;
        _languageChanged = languageChanged;
        _gagTypes = new List<Gag>();

        // the data file with the gag information
        string data_file = "GarblerCore\\collectedGagData\\gag_data.json";
		// Try to read the JSON file and deserialize it into the obj dictionary
		try {
			// Assuming you have an instance of DalamudPluginInterface named _pluginInterface
			string jsonFilePath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, data_file);
			// read the file
			string json = File.ReadAllText(jsonFilePath);
			// deserialize the json into the obj dictionary
			_gagData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json) 
                                                        ?? new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
			// let log know that the file was read
		}
		catch (FileNotFoundException) {
			// If the file does not exist, log an error and initialize obj as an empty dictionary
			GSLogger.LogType.Debug($"[IPA Parser] File does not exist");
			_gagData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
		}
		catch (Exception ex) {
			// If any other error occurs, log the error and initialize obj as an empty dictionary
			GSLogger.LogType.Debug($"[IPA Parser] An error occurred while reading the file: {ex.Message}");
			_gagData = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
		}

        // create our gag listings
        CreateGags(); 
        // Subscribe to the LanguageChanged event
        _languageChanged.LanguageChanged += RefreshGags;

        // invoke it to make sure debug shows correct data (idk how else to better do this at the moment)
        _languageChanged.Invoke();
    }

    /// <summary> Unsubscribes from our subscribed event upon disposal </summary>
    public void Dispose() {
        _languageChanged.LanguageChanged -= RefreshGags;
    }

    private void CreateGags() {
        List<string> masterList;
        switch (_config.languageDialect) {
            case "IPA_UK":      masterList = PhonemMasterLists.MasterListEN_UK; break;
            case "IPA_US":      masterList = PhonemMasterLists.MasterListEN_US; break;
            case "IPA_SPAIN":   masterList = PhonemMasterLists.MasterListSP_SPAIN; break;
            case "IPA_MEXICO":  masterList = PhonemMasterLists.MasterListSP_MEXICO; break;
            case "IPA_FRENCH":  masterList = PhonemMasterLists.MasterListFR_FRENCH; break;
            case "IPA_QUEBEC":  masterList = PhonemMasterLists.MasterListFR_QUEBEC; break;
            case "IPA_JAPAN":   masterList = PhonemMasterLists.MasterListJP; break;
            default:            throw new Exception("Invalid language");
        }

        foreach (var gagEntry in _gagData) {
            var gagName = gagEntry.Key;
            var muffleStrOnPhoneme = new Dictionary<string, int>();
            var ipaSymbolSound = new Dictionary<string, string>();
            foreach (var phonemeEntry in gagEntry.Value) {
                var phoneme = phonemeEntry.Key;
                var properties = phonemeEntry.Value;
                muffleStrOnPhoneme[phoneme] = int.Parse(properties["MUFFLE"]);
                ipaSymbolSound[phoneme] = properties["SOUND"];
            }
            var gag = new Gag(_config);
            gag.AddInfo(gagName, muffleStrOnPhoneme, ipaSymbolSound);
            _gagTypes.Add(gag);
        }
    }

    private void RefreshGags(object sender, LanguageChangedEventArgs e) {
        _gagTypes.Clear();
        CreateGags();
    }
}