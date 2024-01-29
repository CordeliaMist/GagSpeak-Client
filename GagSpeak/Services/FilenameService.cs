using System.IO;
using Dalamud.Plugin; // Used for the DalamudPluginInterface which provides access to various Dalamud features

namespace GagSpeak.Services;

/// <summary> Service for managing file names. </summary>
public class FilenameService
{
    public readonly string ConfigDirectory; // Directory for the additional plugin files
    public readonly string ConfigFile;      // Configuration file
    public readonly string RestraintSetsFile; // Directory for restraint sets
    public readonly string GagStorageFile;    // for storing information about our gags (coming soon probably)

    /// <summary>
    /// Initializes a new instance of the <see cref="FilenameService"/> class.
    /// </summary>
    public FilenameService(DalamudPluginInterface pi) {
        // the FOLDER labeled "GagSpeak"
        ConfigDirectory = pi.ConfigDirectory.FullName;
        // where the pluginconfig file is stored
        ConfigFile = pi.ConfigFile.FullName;
        // where the extra files should be going
        RestraintSetsFile = Path.Combine(ConfigDirectory, "RestraintSets.json");
        GagStorageFile = Path.Combine(ConfigDirectory, "GagStorage.json");
    }
}