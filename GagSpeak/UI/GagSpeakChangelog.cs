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
        Add0_9_7_0(Changelog);
        Add0_9_9_0(Changelog); 
        Add0_9_9_10(Changelog); 
        Add1_0_1_3(Changelog);
        Add1_0_2_0(Changelog);
        Add1_1_0_0(Changelog);
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
    private static void Add1_1_0_0(Changelog log)
        => log.NextVersion("Version 1.1.0.0 Release")
            .RegisterImportant("New Component now in GagSpeak: THE PUPPETEER TAB")
            .RegisterEntry("    》 Set unique trigger phrases to each person on your whitelist, so you make sure they are truely the only one who can make you do things!")
            .RegisterEntry("    》 Configure permission parameters for each person individually.")
            .RegisterEntry("    》 Regex paramater trigger words are a WIP atm and are coming soon!")
            .RegisterImportant("New Componet now in GagSpeak: THE TOYBOX TAB")
            .RegisterEntry("    》 Unique configuration control for each person on your whitelist, dictate who can and cant use your vibrator!")
            .RegisterEntry("    》 Toggle on and off your partners vibe, and know when it is connected!")
            .RegisterEntry("    》 Set and adjust the intensity of the vibrator at will")
            .RegisterEntry("    》 Create and execute patterns of vibration for your vibrator")
            .RegisterEntry("    》 Lock the UI of the toybox to prevent any changes from being made, making your partner enjoy their partner's pleasure")
            .RegisterImportant("GagSpeak Permissions have gone a FULL OVERHAUL")
            .RegisterEntry("It is very likely to encounter new bugs, so please report them as you find them! I had to reformat the whole backend of message/info transfer")
            .RegisterEntry("Permissions now are stored in 2 catagories, Global & Individual")
            .RegisterEntry("Global permss = change for everyone when modified, so everyone will see that setting update when changed")
            .RegisterEntry("Individual perms = unique to each whitelist player. EX: Give 1 person access to toybox & everyone else besides them still wont have access")
            .RegisterEntry("Interaction permissions are now ranked in TIERS, dynamics have a /DynamicTier/ score. The higher the tier, more access you surrender!")
            .RegisterImportant("Don't give absolute-slave dynamic out unless you are sure you trust them. This system is made to keep you safe, respect it!")
            .RegisterEntry("Whitelist Interactions UI has been fully overhauled. Now you can see both what permissions your partner has configured & your own")
            .RegisterHighlight("You can now toggle an option in the settings menu to disable the warning about live chat garbler when switching zones.");


    private static void Add1_0_2_0(Changelog log)
        => log.NextVersion("Version 1.0.2.6 Release")
            .RegisterImportant("A New Wardrobe tab has now been added!")
            .RegisterEntry("Wardrobe tab introduces the ability to automatically equip preassigned items whenever a gag is worn.")
            .RegisterEntry("Wardrobe tab now allows you to define spesific restraint sets")
            .RegisterEntry("    》 NEW CONFIG SETTING ADDED: EnableWardrobe (Dictates if anything in the wardrobe will work)")
            .RegisterEntry("    》 NEW CONFIG SETTING ADDED: Allow Item Auto-Equip (A universal disable for all gags's in the gag storage)")
            .RegisterEntry("    》 NEW CONFIG SETTING ADDED: Allow Restraint Locking (Must be enabled for anyone but you to apply and lock restraints to)")
            .RegisterEntry("    》 NEW CONFIG SETTING ADDED: Surrender Absolute Control (A totally work in progress button that does nothing ATM)")
            .RegisterHighlight("A new compartment within the GagSpeak Kink Wardrobe has been added: The Gag Storage!")
            .RegisterEntry("Define an in game item and stain to link to each gag type")
            .RegisterEntry("When each gagtype is equipped by yourself in the UI, applied via commands, or through whitelist, "+
            "they will equip the item and stain you have defined for them.")
            .RegisterHighlight("A new compartment within the GagSpeak Kink Wardrobe has been added: The Restraint Outfits Compartment!")
            .RegisterEntry("These are outfits you can define composed of gear that you have modded as a restraint set. These are then /overlayed/ ontop of your glamour whenever applied.")
            .RegisterImportant("GagSpeak Plugin config information is now stored into seperate files, allowing information like restraint set data and gag storage data to be more easily sharable and savable!")
            .RegisterEntry("Create as many restraint sets as you possibly want!")
            .RegisterHighlight("A new section was added to the whitelist tab for Wardrobe related interactions!")
            .RegisterEntry("Whitelist tab now has 2 additional buttons for locking and unlocking whitelisted players for a spesified time!")
            .RegisterEntry("If someone besides yourself assigns a restraint set onto you, they must be the same person unlocking it for it to work! Truely immersive")
            .RegisterHighlight("The Live Chat Garbler Lock warning message has now been changed to notify you whenever you switch zones, not just when you login!")
            .RegisterEntry("Any Enabled wardrobe features will automatically reapply themselves whenever you switch any jobs, items, or stains to gear!");
    private static void Add1_0_1_3(Changelog log)
        => log.NextVersion("Version 1.0.2.6 Release")
            .RegisterImportant("Plugin has been officially released!")
            .RegisterHighlight("Fixed a majority of feedback bugs that made it past all the QA, fixing tons of issues!");
    
    private static void Add0_9_9_10(Changelog log)
        => log.NextVersion("Version 0.9.9.10 Pre-Release")
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