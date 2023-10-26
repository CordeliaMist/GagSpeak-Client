using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Dalamud.Logging;
using Dalamud.Plugin;

// Pulled from Sillychat, used for logging a history of the translations

namespace GagSpeak
{
    /// Manage history of translations.
    public class HistoryService {
        // Declare the variables for the history service
        private readonly IGagSpeakPlugin plugin;
        private readonly Timer processTranslationsTimer;


        /// Initializes a new instance of HistoryService class, taking in the plugin information
        public HistoryService(IGagSpeakPlugin plugin) {
            // Set the plugin variable to the plugin passed in
            this.plugin = plugin;
            // Set the translations to a new concurrent queue
            this.Translations = new ConcurrentQueue<Translation>();
            // Set the translations display to a new list of translations
            this.TranslationsDisplay = new List<Translation>();
            // Set the process translations timer to a new timer
            this.processTranslationsTimer = new Timer {
                Interval = plugin.Configuration.ProcessTranslationInterval,
                Enabled = true,
            };
            this.processTranslationsTimer.Elapsed += this.ProcessTranslationsTimerOnElapsed;
            this.processTranslationsTimer.Start();
        }

        // Gets a value indicating whether gets indicator if history is being processed.
        public bool IsProcessing { get; private set; }

        /// Gets list of current historical records for display.
        public List<Translation> TranslationsDisplay { get; private set; }

        /// Gets queue for historical translations.
        public ConcurrentQueue<Translation> Translations { get; }

        /// Add a translation to queue.
        public void AddTranslation(Translation translation) {
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
            this.processTranslationsTimer.Stop();
            this.processTranslationsTimer.Elapsed -= this.ProcessTranslationsTimerOnElapsed;
        }

        // Process translations timer elapsed event.
        // Use exception handling to log errors because i have no clue what this is doing
        private void ProcessTranslationsTimerOnElapsed(object? sender, ElapsedEventArgs e) {
            try
            {
                // What to try
                this.IsProcessing = true;
                while (this.Translations.Count > this.plugin.Configuration.TranslationHistoryMax) {
                    this.Translations.TryDequeue(out _);
                }

                this.TranslationsDisplay = this.Translations.ToList();
                this.IsProcessing = false;
            }
            catch (Exception ex)
            {
                // What to do if the exception is caught
                Services.PluginLog.Error(ex, "Failed to process translations.");
                this.IsProcessing = false;
            }
        }
    }
}