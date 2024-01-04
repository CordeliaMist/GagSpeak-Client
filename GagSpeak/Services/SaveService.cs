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
    /// <summary>
    /// the saveservice contructor <see cref="SaveService"/> class.
    /// <list type="bullet">
    /// <item><c>log</c><param name="log"> - The logger instance.</param></item>
    /// <item><c>framework</c><param name="framework"> - The FrameworkManager instance.</param></item>
    /// <item><c>fileNames</c><param name="fileNames"> - The FilenameService instance.</param></item>
    /// </list> </summary>
    public SaveService(Logger log, FrameworkManager framework, FilenameService fileNames)
        : base(log, framework, fileNames)
    {
        GagSpeak.Log.Debug("[SaveService] SERVICE CONSUTRCTOR INITIALIZED");
    }
}
