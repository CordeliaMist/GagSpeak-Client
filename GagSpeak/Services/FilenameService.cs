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
    public readonly string CharacterData;     // for storing information about our whitelist
    public readonly string PatternStorageFile; // for storing information about our patterns

    /// <summary>
    /// Initializes a new instance of the <see cref="FilenameService"/> class.
    /// </summary>
    public FilenameService(DalamudPluginInterface pi) {
        ConfigDirectory     = pi.ConfigDirectory.FullName;
        ConfigFile          = pi.ConfigFile.FullName;
        RestraintSetsFile   = Path.Combine(ConfigDirectory, "RestraintSets.json");
        GagStorageFile      = Path.Combine(ConfigDirectory, "GagStorage.json");
        CharacterData       = Path.Combine(ConfigDirectory, "CharacterData.json");
        PatternStorageFile  = Path.Combine(ConfigDirectory, "PatternStorage.json");
    }
}