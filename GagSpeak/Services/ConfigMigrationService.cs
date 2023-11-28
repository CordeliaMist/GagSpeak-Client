using System.IO;            // Provides classes for working with directories, files, and drives
using Newtonsoft.Json.Linq; // Provides methods and properties for parsing JSON

namespace GagSpeak.Services;

/// <summary> Service for managing the configuration migration. </summary>
public class ConfigMigrationService
{
    private readonly    SaveService     _saveService;    // Service for saving data
    private readonly    BackupService   _backupService;  // Service for backing up data
    private             GagSpeakConfig  _config = null!; // Configuration for the GagSpeak application
    private             JObject         _data   = null!; // Data for the GagSpeak application

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigMigrationService"/> class.
    /// <list type="bullet">
    /// <item><c>saveService</c><param name="saveService"> - The save service.</param></item>
    /// <item><c>backupService</c><param name="backupService"> - The backup service.</param></item>
    /// </list> </summary>
    public ConfigMigrationService(SaveService saveService, BackupService backupService) {
        _saveService         = saveService;
        _backupService       = backupService;
    }

    /// <summary>
    /// Migrate the config.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    /// <returns>The migrated config.</returns>
    public void Migrate(GagSpeakConfig config)
    {
        // Set the config to the provided config
        _config = config; 
        // If the config version is greater than or equal to the current version, or the config file does not exist, return
        if (config.Version >= GagSpeakConfig.Constants.CurrentVersion || !File.Exists(_saveService.FileNames.ConfigFile))
            return;

        // Otherwise, migrate the config
        _data = JObject.Parse(File.ReadAllText(_saveService.FileNames.ConfigFile));
        MigrateV1To2();
        MigrateV2To4();
    }

    /// <summary>
    /// Migration from v1 to v2.
    /// </summary>
    /// <returns>The migrated config.</returns>
    private void MigrateV1To2() {
        // Migrate from v1 to v2 if version is more than 1
        if (_config.Version > 1)
            return;
        
        // otherwise, make the version stay 2
        _backupService.CreateMigrationBackup("pre_v1_to_v2_migration");
        _config.Version = 2;
    }

    /// <summary>
    /// Migration from v2 to v4.
    /// </summary>
    /// <returns>The migrated config.</returns>
    private void MigrateV2To4() {
        // Migrate from v2 to v4 if version is more than 4
        if (_config.Version > 4)
            return;
    
        // otherwise, make the version stay 4
        _config.Version = 4;
    }
}
