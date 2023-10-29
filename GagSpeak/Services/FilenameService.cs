using System.Collections.Generic;
using System.IO;
using Dalamud.Plugin;

// practicing modular design
namespace GagSpeak.Services;


// I personally have no idea why the fuck this needs to exist but may remove later once i learn more.
public class FilenameService
{
    // define catagories to save.
    public readonly string ConfigDirectory; // need
    public readonly string ConfigFile; // need
    // public readonly string DesignFileSystem; // dont need likely
    // public readonly string MigrationDesignFile; // dont need likely
    // public readonly string DesignDirectory; // dont need
    // public readonly string AutomationFile; // dont need
    // public readonly string UnlockFileCustomize; // dont need
    // public readonly string UnlockFileItems; // dont need
    // public readonly string FavoriteFile; // dont need

    public FilenameService(DalamudPluginInterface pi)
    {
        ConfigDirectory     = pi.ConfigDirectory.FullName;
        ConfigFile          = pi.ConfigFile.FullName;
        // AutomationFile      = Path.Combine(ConfigDirectory, "automation.json");
        // DesignFileSystem    = Path.Combine(ConfigDirectory, "sort_order.json");
        // MigrationDesignFile = Path.Combine(ConfigDirectory, "Designs.json");
        // UnlockFileCustomize = Path.Combine(ConfigDirectory, "unlocks_customize.json");
        // UnlockFileItems     = Path.Combine(ConfigDirectory, "unlocks_items.json");
        // DesignDirectory     = Path.Combine(ConfigDirectory, "designs");
        // FavoriteFile        = Path.Combine(ConfigDirectory, "favorites.json");
    }


    // public IEnumerable<FileInfo> Designs()
    // {
    //     if (!Directory.Exists(DesignDirectory))
    //         yield break;

    //     foreach (var file in Directory.EnumerateFiles(DesignDirectory, "*.json", SearchOption.TopDirectoryOnly))
    //         yield return new FileInfo(file);
    // }

    // public string DesignFile(string identifier)
    //     => Path.Combine(DesignDirectory, $"{identifier}.json");

    // public string DesignFile(Design design)
    //     => DesignFile(design.Identifier.ToString());
}
