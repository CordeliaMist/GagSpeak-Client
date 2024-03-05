using Serilog.Events;
using System;

namespace GagSpeak.GSLogger;
// taken directly off ECommens, for the sake of not including the full lib. ref code is here:
// https://github.com/NightmareXIV/ECommons/blob/fd3ceab5345b2a42eb51f998d6dcf6b696643f45/ECommons/Logging/

public record struct InternalLogMessage
{
    public string Message;
    public LogEventLevel Level;
    public string Prefix;
    public DateTimeOffset Time;
    public InternalLogMessage(string Message, LogEventLevel Level = LogEventLevel.Information) {
        this.Message = Message;
        this.Level = Level;
        this.Time = DateTimeOffset.Now;
        this.Prefix = Level == LogEventLevel.Fatal ? "FTL" 
            : Level == LogEventLevel.Error ? "ERR"
            : Level == LogEventLevel.Warning ? "WRN"
            : Level == LogEventLevel.Information ? "INF"
            : Level == LogEventLevel.Debug ? "DBG"
            : Level == LogEventLevel.Verbose ? "VRB"
            : "UNK"; // default value in case level doesn't match any
        }
}