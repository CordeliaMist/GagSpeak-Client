using System.Collections.Concurrent;// for the concurrent queue
using System.Collections.Generic;   // Dictionaries
using System.Linq;                  // for lists
using System.Timers;                // Provides server-based timer services

// Pulled from Sillychat, used for logging a history of the translations
namespace GagSpeak.Services;
/// <summary> Manage history of translations. </summary>
public class HistoryService {
    private readonly    GagSpeakConfig                  _config;                                // Configuration for the GagSpeak application
    private readonly    Timer                           _processTranslationsTimer;              // Timer to process translations
    public bool                                         IsProcessing { get; private set; }      // Is the history being processed?
    public              List<Translation>               TranslationsDisplay {get; private set;} // List of translations for display
    public              ConcurrentQueue<Translation>    Translations { get; }                   // Queue of translations
    
    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryService"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    public HistoryService(GagSpeakConfig config) {
        // Set the readonlys
        _config = config; // Assign the provided config to the local config variable
        // Set the translations to a new concurrent queue
        this.Translations = new ConcurrentQueue<Translation>();
        // Set the translations display to a new list of translations
        this.TranslationsDisplay = new List<Translation>();
        // Set the process translations timer to a new timer
        _processTranslationsTimer = new Timer {
            Interval = _config.ProcessTranslationInterval,
            Enabled = true,
        };
        // subscribe to the elapsed event
        _processTranslationsTimer.Elapsed += this.ProcessTranslationsTimerOnElapsed;
        // start the timer
        _processTranslationsTimer.Start();
    }

    /// <summary>
    /// Add a translation to queue.
    /// <list type="bullet">
    /// <item><c>translation</c><param name="translation"> - The translation to add.</param></item>
    /// </list> </summary>
    public void AddTranslation(Translation translation) {
        // Enqueue the translation to the Translations queue
        this.Translations.Enqueue(translation);
        if (!this.IsProcessing) {
            // If not currently processing, add the translation to the TranslationsDisplay list
            this.TranslationsDisplay.Add(translation);
        }
    }

    /// <summary> Dispose of the plugins history service. </summary>
    public void Dispose() {
        // Stop the timer, and the elapsed event
        _processTranslationsTimer.Stop();
        _processTranslationsTimer.Elapsed -= this.ProcessTranslationsTimerOnElapsed;
    }

    /// <summary>
    /// Process translations timer after it has elapsed.
    /// <list type="bullet">
    /// <item><c>sender</c><param name="sender"> - The sender of the event.</param></item>
    /// <item><c>e</c><param name="e"> - The event arguments.</param></item>
    /// </list> </summary>
    private void ProcessTranslationsTimerOnElapsed(object? sender, ElapsedEventArgs e) {
        try
        {
            // What to try
            this.IsProcessing = true;
            while (this.Translations.Count > _config.TranslationHistoryMax) {
                this.Translations.TryDequeue(out _);
            }
            // Set the translations display to the translations list
            this.TranslationsDisplay = this.Translations.ToList();
            this.IsProcessing = false;
        } catch {
            // What to do if the exception is caught
            GagSpeak.Log.Error("Failed to process translations.");
            this.IsProcessing = false;
        }
    }
}