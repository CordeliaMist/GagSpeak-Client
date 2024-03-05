// using ECommons.DalamudServices;
// using Serilog.Events;

// // taken directly off ECommens, for the sake of not including the full lib. ref code is here:
// // https://github.com/NightmareXIV/ECommons/blob/fd3ceab5345b2a42eb51f998d6dcf6b696643f45/ECommons/Logging/
// namespace GagSpeak.InternalLogger;

// public static class PluginLog
// {

//     public static void Information(string s) {
//         Svc.Log.Information($"{s}");
//         Svc.Framework?.RunOnFrameworkThread(delegate
//         {
//             InternalLog.Messages.PushBack(new(s, LogEventLevel.Information));
//         });
//     }
//     public static void Error(string s)
//     {
//         Svc.Log.Error($"{s}");
//         Svc.Framework?.RunOnFrameworkThread(delegate
//         {
//             InternalLog.Messages.PushBack(new(s, LogEventLevel.Error));
//         });
//     }
//     public static void Fatal(string s)
//     {
//         Svc.Log.Fatal($"{s}");
//         Svc.Framework?.RunOnFrameworkThread(delegate
//         {
//             InternalLog.Messages.PushBack(new(s, LogEventLevel.Fatal));
//         });
//     }
//     public static void Debug(string s)
//     {
//         Svc.Log.Debug($"{s}");
//         Svc.Framework?.RunOnFrameworkThread(delegate
//         {
//             InternalLog.Messages.PushBack(new(s, LogEventLevel.Debug));
//         });
//     }
//     public static void Verbose(string s)
//     {
//         Svc.Log.Verbose($"{s}");
//         Svc.Framework?.RunOnFrameworkThread(delegate
//         {
//             InternalLog.Messages.PushBack(new(s, LogEventLevel.Verbose));
//         });
//     }
//     public static void Warning(string s)
//     {
//         Svc.Log.Warning($"{s}");
//         Svc.Framework?.RunOnFrameworkThread(delegate
//         {
//             InternalLog.Messages.PushBack(new(s, LogEventLevel.Warning));
//         });
//     }
//     public static void LogInformation(string s)
//     {
//         Information(s);
//     }
//     public static void LogError(string s)
//     {
//         Error(s);
//     }
//     public static void LogFatal(string s)
//     {
//         Fatal(s);
//     }
//     public static void LogDebug(string s)
//     {
//         Debug(s);
//     }
//     public static void LogVerbose(string s)
//     {
//         Verbose(s);
//     }
//     public static void LogWarning(string s)
//     {
//         Warning(s);
//     }
//     public static void Log(string s)
//     {
//         Information(s);
//     }
// }