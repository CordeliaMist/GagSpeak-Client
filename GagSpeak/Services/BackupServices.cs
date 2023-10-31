using System.Collections.Generic;
using System.IO;
using OtterGui.Classes;
using OtterGui.Log;

// practice for modular design
namespace GagSpeak.Services;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class BackupService
{
    private readonly Logger                  _logger;
    private readonly DirectoryInfo           _configDirectory;
    private readonly IReadOnlyList<FileInfo> _fileNames;

    public BackupService(Logger logger, FilenameService fileNames) {
        _logger          = logger;
        _fileNames       = GagSpeakFiles(fileNames);
        _configDirectory = new DirectoryInfo(fileNames.ConfigDirectory);
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
#pragma warning restore IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention