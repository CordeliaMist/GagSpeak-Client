using System.IO;
using Dalamud.Plugin; // Used for the DalamudPluginInterface which provides access to various Dalamud features

namespace GagSpeak.Services;

/// <summary> Service for managing file names. </summary>
public class FilenameService
{
    public readonly string ConfigDirectory; // Directory for the additional plugin files
    public readonly string ConfigFile;      // Configuration file
    public readonly string RestraintSetsFile; // Directory for restraint sets
    public readonly string GagStorageFile;    // for storing information about our gags 
    public readonly string CharacterDataFile;     // for storing information about our whitelist
    public readonly string PatternStorageFile; // for storing information about our patterns
    public readonly string ToyboxTriggersFile; // for storing information about our toybox triggers
    public readonly string HardcoreSettingsFile; // for storing information about our hardcore settings

    public FilenameService(DalamudPluginInterface pi) {
        ConfigDirectory     = pi.ConfigDirectory.FullName;
        ConfigFile          = pi.ConfigFile.FullName;
        RestraintSetsFile   = Path.Combine(ConfigDirectory, "RestraintSets.json");
        GagStorageFile      = Path.Combine(ConfigDirectory, "GagStorage.json");
        CharacterDataFile   = Path.Combine(ConfigDirectory, "CharacterData.json");
        PatternStorageFile  = Path.Combine(ConfigDirectory, "PatternStorage.json");
        ToyboxTriggersFile  = Path.Combine(ConfigDirectory, "ToyboxTriggers.json");
        HardcoreSettingsFile= Path.Combine(ConfigDirectory, "HardcoreSettings.json");
    }
}