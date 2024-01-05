using OtterGui.Widgets; // used to get the ChangeLog component from otterGui

// taken off of glamourer's changelog system becuz i want to learn all the knowledge
namespace GagSpeak.UI;

/// <summary> This class is used to handle the changelog for the GagSpeak plugin. </summary>
public class GagSpeakChangelog
{
    public const int LastChangelogVersion = 0;
    private readonly GagSpeakConfig _config;
    public readonly Changelog Changelog;

    /// <summary>
    /// Initializes a new instance of the <see cref="GagSpeakChangelog"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    public GagSpeakChangelog(GagSpeakConfig config) {
        _config   = config; // initialize the config in our constructor
        Changelog = new Changelog("GagSpeak Changelog", ConfigData, Save); // initialize the changelog
        // what displays inside the change log
        Add0_9_9_10(Changelog); 
        Add0_9_9_0(Changelog); 
        Add0_9_7_0(Changelog);
    }

    /// <summary> Retrieves the configuration data. </summary>
    private (int, ChangeLogDisplayType) ConfigData()
        => (_config.Version, _config.ChangeLogDisplayType); // by just sending it our changelog version and display type.

    /// <summary>
    /// Saves the version and display type.
    /// <list type="bullet">
    /// <item><c>version</c><param name="version"> - The version to save.</param></item>
    /// <item><c>type</c><param name="type"> - The display type to save.</param></item>
    /// </list> </summary>
    private void Save(int version, ChangeLogDisplayType type) {
        // if the config version is not the same as the version we are trying to save
        if (_config.Version != version) {
            // set the config version to the version we are trying to save & save config.
            _config.Version = version;
            _config.Save();
        }
        // if the config display type is not the same as the display type we are trying to save
        if (_config.ChangeLogDisplayType != type) {
            // set the config display type to the display type we are trying to save & save config.
            _config.ChangeLogDisplayType = type;
            _config.Save();
        }
    }

    // all versions are added here, the order doesnt madder, but it should be in order of newest to oldest.
    private static void Add0_9_9_10(Changelog log)
        => log.NextVersion("Version 1.0.0.1 Pre-Release")
            .RegisterHighlight("Advanced Chat Garbler now fully implemented!.")
            .RegisterHighlight("Plugin File Size slightly increased due to the inclusion of dictionaries.")
            .RegisterHighlight("Debug Menu moved to /gagspeak debug , will no longer show up in config menu.");
    private static void Add0_9_9_0(Changelog log)
        => log.NextVersion("Version 0.9.9.0 Pre-Release")
            .RegisterHighlight("GagAndLockManager to syncronize config and padlockidentifier locks has been implemented.")
            .RegisterHighlight("Profiles have been polished out with more visual appeal.")
            .RegisterHighlight("Commands made to work on other people are likely to sync up better, but still not perfect.")
            .RegisterEntry("Adjusted safeword cooldown for final tests")
            .RegisterEntry("new popout window for user profiles added.")
            .RegisterEntry("Additional feedback messages in chat after using commands and buttons have been improved.");

    private static void Add0_9_7_0(Changelog log)
        => log.NextVersion("Version 0.9.7.0 Pre-Release")
            .RegisterHighlight("Majority of the code has been rewritten.")
            .RegisterHighlight("Lots of overhaul to feedback, players will now recieve messages about the results of commands in chat.")
            .RegisterEntry("Majority of UI Button interactions should now be working")
            .RegisterEntry("New Popout window for profile viewing should now be something that exists.")
            .RegisterEntry("Added this sick changelog thing.");
}