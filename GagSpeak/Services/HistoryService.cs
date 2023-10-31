using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

using GagSpeak.Chat;

// Pulled from Sillychat, used for logging a history of the translations

namespace GagSpeak.Services;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention

/// <summary>
/// Manage history of translations.
/// </summary>
public class HistoryService {
    // Declare the variables for the history service
    private readonly GagSpeakConfig _config;
    private readonly Timer _processTranslationsTimer;

    /// Initializes a new instance of HistoryService class, taking in the plugin information
    public HistoryService(GagSpeakConfig config) {
        // Set the readonlys
        _config = config;
        
        // Set the translations to a new concurrent queue
        this.Translations = new ConcurrentQueue<MessageGarbler>();
        // Set the translations display to a new list of translations
        this.TranslationsDisplay = new List<MessageGarbler>();
        // Set the process translations timer to a new timer
        _processTranslationsTimer = new Timer {
            Interval = _config.ProcessTranslationInterval,
            Enabled = true,
        };
        _processTranslationsTimer.Elapsed += this.ProcessTranslationsTimerOnElapsed;
        _processTranslationsTimer.Start();
    }

    // Gets a value indicating whether gets indicator if history is being processed.
    public bool IsProcessing { get; private set; }

    /// Gets list of current historical records for display.
    public List<MessageGarbler> TranslationsDisplay { get; private set; }

    /// Gets queue for historical translations.
    public ConcurrentQueue<MessageGarbler> Translations { get; }

    /// Add a translation to queue.
    public void AddTranslation(MessageGarbler translation) {
        // I honestly dont fully understand this process, comment on it once you learn more about it!
        this.Translations.Enqueue(translation);
        if (!this.IsProcessing) {
            // Add the translation to the translation to the translations display list, used in the history window
            this.TranslationsDisplay.Add(translation);
        }
    }

    /// Dispose of the plugins history service.
    public void Dispose() {
        // Stop the timer, and the elapsed event
        _processTranslationsTimer.Stop();
        _processTranslationsTimer.Elapsed -= this.ProcessTranslationsTimerOnElapsed;
    }

    // Process translations timer elapsed event.
    // Use exception handling to log errors because i have no clue what this is doing
    private void ProcessTranslationsTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        try
        {
            // What to try
            this.IsProcessing = true;
            while (this.Translations.Count > _config.TranslationHistoryMax) {
                this.Translations.TryDequeue(out _);
            }

            this.TranslationsDisplay = this.Translations.ToList();
            this.IsProcessing = false;
        }
        catch (Exception ex)
        {
            // What to do if the exception is caught
            GagSpeak.Log.Error($"{ex}Failed to process translations.");
            this.IsProcessing = false;
        }
    }
}
#pragma warning restore IDE1006