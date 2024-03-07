using Dalamud.Interface.Colors;
using ImGuiNET;
using Serilog.Events;
using System;
using System.Linq;
using System.Numerics;
using GagSpeak.Utility;
using OtterGui.Widgets;
using GagSpeak.Services;
using System.IO;
using Newtonsoft.Json;
using OtterGui;
using Dalamud.Interface;

namespace GagSpeak.GSLogger;
#nullable disable

// taken directly off ECommens, for the sake of not including the full lib. ref code is here:
// https://github.com/NightmareXIV/ECommons/blob/fd3ceab5345b2a42eb51f998d6dcf6b696643f45/ECommons/Logging/

public class InternalLogTab : ITab
{
    private readonly InternalLog _internalLog = GagSpeak._services.GetService<InternalLog>();
    public ReadOnlySpan<byte> Label => "Log###GagSpeakGSLogger"u8;
    
    public void DrawContent() {
        _internalLog.PrintImgui();
    }
}


public class InternalLog {
    public event Action OnOpenWindowRequested = delegate { };
    public static readonly CircularBuffer<InternalLogMessage> Messages = new(16000);
    static string Search = "";
    static bool Autoscroll = true;
    static int PreviousMessageCount = 0;
    public void PrintImgui() {
        bool shouldAutoscroll = false;

        ImGui.Checkbox("Autoscroll", ref Autoscroll);
        ImGui.SameLine();
        if (ImGui.Button("Copy all")) {
            GenericHelpers.Copy(string.Join("\n", Messages.Select(x => $"[{x.Level}@{x.Time}] {x.Message}")));
        }
        ImGui.SameLine();
        if (ImGui.Button("Clear")) {
            Messages.Clear();
        }
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X-ImGui.GetFrameHeight());
        ImGui.InputTextWithHint("##Filter", "Filter...", ref Search, 100);
        ImGui.SameLine();
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Expand.ToIconString(), new Vector2(ImGui.GetFrameHeight()), "Open the Logger in its own window", false, true)) {
            OnOpenWindowRequested.Invoke();
        }
        ImGui.BeginChild($"Plugin_log{GagSpeak.Name}");

        if (Messages.Size > PreviousMessageCount && Autoscroll) {
            shouldAutoscroll = true;
        }
        PreviousMessageCount = Messages.Size;

        foreach(var x in Messages)
        {
            if(Search == String.Empty || x.Level.ToString().EqualsIgnoreCase(Search) || x.Message.Contains(Search, StringComparison.OrdinalIgnoreCase))
            GenericHelpers.TextWrappedCopy(x.Level == LogEventLevel.Fatal?ImGuiColors.DPSRed
                :x.Level == LogEventLevel.Error?ImGuiColors.DalamudRed
                :x.Level == LogEventLevel.Warning?ImGuiColors.DalamudYellow
                :x.Level == LogEventLevel.Information?ImGuiColors.ParsedPink
                :x.Level == LogEventLevel.Debug?ImGuiColors.DalamudWhite
                :x.Level == LogEventLevel.Verbose?ImGuiColors.DalamudGrey
                :ImGuiColors.DalamudGrey2, $"> {x.Time.Hour:D2}:{x.Time.Minute:D2}:{x.Time.Second:D2}.{x.Time.Millisecond:D3} | {x.Prefix} | {x.Message}");
        }
        if (shouldAutoscroll) {
            ImGui.SetScrollHereY();
        }
        ImGui.EndChild();
    }
}