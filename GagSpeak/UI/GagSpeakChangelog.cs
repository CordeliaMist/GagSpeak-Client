using GagSpeak;
using OtterGui.Widgets;

// taken off of glamourer's changelog system becuz i want to learn all the knowledge
namespace Glamourer.Gui;
public class GagSpeakChangelog
{
    public const     int           LastChangelogVersion = 0;
    private readonly GagSpeakConfig _config;
    public readonly  Changelog     Changelog;

    public GagSpeakChangelog(GagSpeakConfig config) {
        _config   = config;
        Changelog = new Changelog("GagSpeak Changelog", ConfigData, Save);

        Add0_9_7_0(Changelog);
    }

    private (int, ChangeLogDisplayType) ConfigData()
        => (_config.Version, _config.ChangeLogDisplayType);

    private void Save(int version, ChangeLogDisplayType type)
    {
        if (_config.Version != version)
        {
            _config.Version = version;
            _config.Save();
        }

        if (_config.ChangeLogDisplayType != type)
        {
            _config.ChangeLogDisplayType = type;
            _config.Save();
        }
    }

    private static void Add0_9_7_0(Changelog log)
        => log.NextVersion("Version 0.9.7.0 Pre-Release")
            .RegisterHighlight("Majority of the code has been rewritten.")
            .RegisterHighlight("Lots of overhaul to feedback, players will now recieve messages about the results of commands in chat.")
            .RegisterEntry("Majority of UI Button interactions should now be working")
            .RegisterEntry("New Popout window for profile viewing should now be something that exists.")
            .RegisterEntry("Added this sick changelog thing.");
    // add more below here whenever we have an update.
}