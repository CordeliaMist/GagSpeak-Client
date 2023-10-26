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

namespace GagSpeak
{
    public unsafe partial class GagSpeak : IDalamudPlugin {
        // First we must determine what to do with chat messages, and how we will handle their payloads.
        private void Chat_OnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString chatmessage, ref bool isHandled) {
            // If isHandled is true, we want to immidiately back out of the function.
            if (isHandled) return;

            // If the message is not in one of our spesified channels, we want to back out of the function.
            if (!_channels.Contains(type)) return;

            // TRY CHAT BUBBLES WAY OF HANDLING THIS LATER
            // First we need to get the payload off the SeString and store it into a format message
            var formatMessage = new SeString(new List<Payload>());
            // also get the newline payload for later
            var nline = new SeString(new List<Payload>());
            // Add the newline to the end of the nline payload
            nline.Payloads.Add(new TextPayload("\n"));
        }

        // General conditions that must be met for the message manipulation to occur
        // 1) The player using the command must either be you, or someone on your whitelist.
        // 2) the message must be in one of the allowed chat types
        // 3) the message, if 1 & 2 are fulfilled, must be passed into the translator with the appropriate garble strength.

    /// Silly Chats Way of Handling the translation.
        // private void Translate(SeString message) {
        //     try {
        //         foreach (var payload in message.Payloads) {
        //             if (payload is TextPayload textPayload) {
        //                 var input = textPayload.Text;
        //                 if (string.IsNullOrEmpty(input) || input.Contains('\uE0BB')) {
        //                     continue;
        //                 }

        //                 var output = this.TranslationService.Translate(input);
        //                 if (!input.Equals(output)) {
        //                     textPayload.Text = output;
        //                     Services.PluginLog.Debug($"{input}|{output}");
        //                     this.HistoryService.AddTranslation(new Translation(input, output));
        //                 }
        //             }
        //         }
        //     } catch {
        //         Services.PluginLog.Debug($"Failed to process message: {message}.");
        //     }
        // }
    }
}