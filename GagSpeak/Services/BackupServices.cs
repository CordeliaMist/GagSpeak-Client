using System.Collections.Generic; // Dictionaries & lists
using System.IO;                  // Provides classes for working with directories, files, and drives
using OtterGui.Classes;           // for the backup component
using OtterGui.Log;               // for the logger

namespace GagSpeak.Services;
/// <summary> Service for handling / managing backups of the config file. </summary>
public class BackupService
{
    private readonly Logger                  _logger;
    private readonly DirectoryInfo           _configDirectory;
    private readonly IReadOnlyList<FileInfo> _fileNames;

    /// <summary> Initializes a new instance of the <see cref="BackupService"/> class.
    /// <list type="bullet">
    /// <item><c>logger</c><param name="logger"> - The logger instance.</param></item>
    /// <item><c>fileNames</c><param name="fileNames"> - The file names service.</param></item>
    /// </list> </summary>
    public BackupService(Logger logger, FilenameService fileNames) {
        _logger          = logger;
        _fileNames       = GagSpeakFiles(fileNames);
        _configDirectory = new DirectoryInfo(fileNames.ConfigDirectory);
        // Create the backup directory if it does not exist
        Backup.CreateAutomaticBackup(logger, _configDirectory, _fileNames);
    }

    /// <summary> Create a permanent backup with a given name for migrations. </summary>
    public void CreateMigrationBackup(string name)
        => Backup.CreatePermanentBackup(_logger, _configDirectory, _fileNames, name);

    /// <summary> Collect all relevant files for GagSpeak configuration. </summary>
    private static IReadOnlyList<FileInfo> GagSpeakFiles(FilenameService fileNames) {
        var list = new List<FileInfo>(16) {
            new(fileNames.ConfigFile)
        };
        return list;
    }
}