using OtterGui.Classes;
using OtterGui.Log;


namespace GagSpeak.Services;

/// <summary>
/// Any file type that we want to save via SaveService.
/// </summary>
public interface ISavable : ISavable<FilenameService>
{ }

public sealed class SaveService : SaveServiceBase<FilenameService>
{
    public SaveService(Logger log, FrameworkManager framework, FilenameService fileNames)
        : base(log, framework, fileNames)
    { }
}
