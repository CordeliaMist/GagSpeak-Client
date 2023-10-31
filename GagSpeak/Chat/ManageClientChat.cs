using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Num = System.Numerics;

using GagSpeak.Chat;

namespace GagSpeak.Chat;

/// <summary>
/// <para>The following class scans, reads, triggers other functions, and manages chat messages</para>
/// <para>These chat messages are, when done properly, all sent to the dalamud chatGUI, and so are any modifications to their payloads.</para>
/// <para>None of these are sent to the server, so they are entirely safe to use.</para>
/// </summary>
public class ManageClientChat
{

    private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
    //     // If isHandled is true, we want to immidiately back out of the function.
    //     if (isHandled) return;

    //     // If the message is not in one of our spesified channels, we want to back out of the function.
    //     if (!_channels.Contains(type)) return;

    //     // TRY CHAT BUBBLES WAY OF HANDLING THIS LATER
    //     // First we need to get the payload off the SeString and store it into a format message
    //     var formatMessage = new SeString(new List<Payload>());
    //     // also get the newline payload for later
    //     var nline = new SeString(new List<Payload>());
    //     // Add the newline to the end of the nline payload
    //     nline.Payloads.Add(new TextPayload("\n"));
    // }

    // General conditions that must be met for the message manipulation to occur
    // 1) The player using the command must either be you, or someone on your whitelist.
    // 2) the message must be in one of the allowed chat types
    // 3) the message, if 1 & 2 are fulfilled, must be passed into the translator with the appropriate garble strength. 
    }
}