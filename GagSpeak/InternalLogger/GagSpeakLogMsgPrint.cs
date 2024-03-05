using Dalamud.Plugin.Services;
using Serilog.Events;
// taken directly off ECommens, for the sake of not including the full lib. ref code is here:
// https://github.com/NightmareXIV/ECommons/blob/fd3ceab5345b2a42eb51f998d6dcf6b696643f45/ECommons/Logging/
namespace GagSpeak.GSLogger;

public class LogType
{
    private static IFramework _framework = GagSpeak._services.GetService<IFramework>();
    public static void Information(string s) {
        GagSpeak.Log.Information($"{s}");
        _framework.RunOnFrameworkThread(delegate { InternalLog.Messages.PushBack(new(s, LogEventLevel.Information)); });
    }
    public static void Error(string s) {
        GagSpeak.Log.Error($"{s}");
        _framework.RunOnFrameworkThread(delegate { InternalLog.Messages.PushBack(new(s, LogEventLevel.Error)); });
    }
    public static void Fatal(string s) {
        GagSpeak.Log.Fatal($"{s}");
        _framework.RunOnFrameworkThread(delegate { InternalLog.Messages.PushBack(new(s, LogEventLevel.Fatal)); });
    }
    public static void Debug(string s) {
        GagSpeak.Log.Debug($"{s}");
        _framework.RunOnFrameworkThread(delegate { InternalLog.Messages.PushBack(new(s, LogEventLevel.Debug)); });
    }
    public static void Verbose(string s) {
        GagSpeak.Log.Verbose($"{s}");
        _framework.RunOnFrameworkThread(delegate {InternalLog.Messages.PushBack(new(s, LogEventLevel.Verbose)); });
    }
    public static void Warning(string s) {
        GagSpeak.Log.Warning($"{s}");
        _framework.RunOnFrameworkThread(delegate { InternalLog.Messages.PushBack(new(s, LogEventLevel.Warning)); });
    }
    public static void LogInformation(string s) {
        Information(s);
    }
    public static void LogError(string s)
    {
        Error(s);
    }
    public static void LogFatal(string s)
    {
        Fatal(s);
    }
    public static void LogDebug(string s)
    {
        Debug(s);
    }
    public static void LogVerbose(string s)
    {
        Verbose(s);
    }
    public static void LogWarning(string s)
    {
        Warning(s);
    }
    public static void Log(string s)
    {
        Information(s);
    }
}