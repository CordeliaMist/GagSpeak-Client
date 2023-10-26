

// An interface of the GagSpeak plugin used for interacting across various windows.

// Very much looks out of date and not nessisary, may remove later.

namespace GagSpeak
{
    // GagSpeak plugin interface.
    public interface IGagSpeakPlugin {
        // Gets configuration.
        GagSpeakConfig Configuration { get; }

        /// Gets history service.
        HistoryService HistoryService { get; }

        /// Save plugin configuration.
        void SaveConfig();
    }
}