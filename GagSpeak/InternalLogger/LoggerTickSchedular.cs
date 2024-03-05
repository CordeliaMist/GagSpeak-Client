using System;
using Dalamud.Plugin.Services;

namespace GagSpeak.GSLogger;
// taken directly off ECommens, for the sake of not including the full lib. ref code is here:
// https://github.com/NightmareXIV/ECommons/blob/fd3ceab5345b2a42eb51f998d6dcf6b696643f45/ECommons/Logging/

public class TickScheduler
{
    long executeAt;
    Action function;
    bool disposed = false;
    IFramework _framework;
    public TickScheduler(Action function, IFramework framework, long delayMS = 0)
    {
        executeAt = Environment.TickCount64 + delayMS;
        this.function = function;
        _framework = framework;
        _framework.Update += Execute;
    }

    public void Dispose() {
        if (!disposed) {
            _framework.Update -= Execute;
        }
        disposed = true;
    }

    void Execute(object _) {
        if (Environment.TickCount64 < executeAt) return;
        try {
            function();
        } catch (Exception e) {
            GSLogger.LogType.Error(e.Message + "\n" + e.StackTrace ?? "");
        }
        Dispose();
    }
}