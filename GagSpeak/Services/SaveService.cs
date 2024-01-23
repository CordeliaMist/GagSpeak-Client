using OtterGui.Classes; // Used for the FrameworkManager class which manages the OtterGui framework
using OtterGui.Log;     // Used for the Logger class which handles logging

namespace GagSpeak.Services;

/// <summary>
/// Interface for any file type that we want to save via SaveService.
/// </summary>
public interface ISavable : ISavable<FilenameService>
{
    // no interface elements nessisary here
}

/// <summary>
/// SaveService class that extends SaveServiceBase with FilenameService as the generic parameter.
/// </summary>
public sealed class SaveService : SaveServiceBase<FilenameService>
{
    public SaveService(Logger log, FrameworkManager framework, FilenameService fileNames)
        : base(log, framework, fileNames) {
        // WHAT IS LOVE, BABY DONT HURT ME! DonT HURT ME, NO MORE!
    }
}
