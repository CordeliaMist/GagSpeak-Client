using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using ImGuiNET;
using OtterGui;
using OtterGui.Classes;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Globalization;
using Dalamud.Logging;
using Num = System.Numerics;

// This serves as the hub for both:
// - OnChatMessage reading
// - Sending Garbled Messages
// - Sended tells to whitlisted players


namespace GagSpeak.Chat;

public class OnChatManager : IDisposable
{
    private readonly OnChatMessage _onChatMessage;
    private readonly OnChatTranslate _onChatTranslate;


    public void Dispose()
    {
        throw new NotImplementedException();
    }
}