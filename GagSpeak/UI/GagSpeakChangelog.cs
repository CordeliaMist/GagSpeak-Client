using OtterGui.Widgets; // used to get the ChangeLog component from otterGui

// taken off of glamourer's changelog system becuz i want to learn all the knowledge
namespace GagSpeak.UI;

/// <summary> This class is used to handle the changelog for the GagSpeak plugin. </summary>
public class GagSpeakChangelog
{
    public const int LastChangelogVersion = 0;
    private readonly GagSpeakConfig _config;
    public readonly Changelog Changelog;

    public GagSpeakChangelog(GagSpeakConfig config) {
        _config   = config; // initialize the config in our constructor
        Changelog = new Changelog("GagSpeak Changelog", ConfigData, Save); // initialize the changelog
        // what displays inside the change log
        Add0_9_7_0(Changelog);
        Add0_9_9_0(Changelog); 
        Add0_9_9_10(Changelog); 
        Add1_0_1_3(Changelog);
        Add1_0_2_0(Changelog);
        Add2_0_0_0(Changelog);
        Add2_2_0_0(Changelog);
        Add2_4_0_0(Changelog);
        Add2_5_0_0(Changelog);
        Add2_6_0_0(Changelog);
        Add2_7_0_0(Changelog);
        Add2_8_0_0(Changelog);
        Add2_8_4_0(Changelog);
        Add2_8_5_0(Changelog);
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

    private static void Add2_8_5_0(Changelog log)
        => log.NextVersion("Version 2.8.5.1 Release")
            .RegisterImportant("The Hardcore Tab is now in a fully functional state. (should be)")
            .RegisterHighlight("Hardcore tab can no longer be triggered by beta-testers, and must be triggered by the person selected on the whitelist.")
            .RegisterEntry("Be careful with who you give access to on hardcore options!")
            .RegisterEntry("Hardcore tab ForcedStay feature now sucessfully blocks all teleports and returns")
            .RegisterHighlight("The Panel Gag and Pump gag for all levels have been added to the chat garbler.")
            .RegisterEntry("The Gag Storage has been fixed to allow gags to equip when they should be")
            .RegisterHighlight("The bug which prevented automation from being applied on a refresh all event has been fixed")
            .RegisterEntry("The bug which prevented blindfolds from being unequipped has been fixed with a temp solution.")
            .RegisterImportant("To account for correcting a massive underlying issue in the gag storage, the GagStorage configuration has been reset, and you will need to set it up again.")
            .RegisterEntry("Extra options for a groundsit start to the forcedsitcommand has been added.")
            .RegisterEntry("You can now choose to redraw your character upon restraint set toggling, so animation mods and VFX changes update immediately.");

    private static void Add2_8_4_0(Changelog log)
        => log.NextVersion("Version 2.8.4.0 Release")
            .RegisterEntry("Did some polishing up to the actions manager")
            .RegisterEntry("Added interaction commands for the hardcore tab")
            .RegisterEntry("Added some new interactions to the whitelist manager")
            .RegisterEntry("Added in some outlines for API within the code but nothing official yet as I feel uneasy with privacy concerns")
            .RegisterEntry("Added the phonetics to some of the gags without any")
            .RegisterEntry("Polished up the toybox tab");

    private static void Add2_8_0_0(Changelog log)
        => log.NextVersion("Version 2.8.0.0 Release")
            .RegisterImportant("The Hardcore Tab is now in a fully functional state.")
            .RegisterEntry("Major reported issues patched, hotbar issue is known and working on logic for it")
            .RegisterHighlight("You can now bind certain penumbra mods to restraint sets, so they activate when you enable the set!")
            .RegisterEntry("Enabled sets have their associated mods priority enabled in the current collection and have their priority set to 99 while active.")
            .RegisterEntry("Disabled sets have their associated mods priority disabled in the current collection and have their priority set to their original priority they had before")
            .RegisterHighlight("You can now ALT+RIGHT CLICK to apply an item in a penumbra mods changed item list to your active restraint set in the wardrobe tab!")
            .RegisterEntry("You can select if you watn each of the associated mods to stay enabled when the set is is toggled off, or to disable the mod again when the set is toggled off.")
            .RegisterEntry("Fixed the memeory corruption issue with texture rendering causing the whole plugin to break.")
            .RegisterEntry("Fixed the issue where the plugin would crash if you tried to use the toybox tab without having the hardcore tab enabled.")
            .RegisterEntry("Fixed multiple instances where you would have your whole screen become disabled")
            .RegisterEntry("Added a big fat warning screen when trying to enable hardcore mode so you don't enable it by accident.");

    private static void Add2_7_0_0(Changelog log)
        => log.NextVersion("Version 2.7.0.0 Release")
            .RegisterImportant("YIPEEEEE!!!! I FINALLY GOT PROPER MOVEMENT PREVENTION CODE IN PLACE!!!")
            .RegisterEntry("To all beta testers: thank you for the paitence, I have been in touch with lots of other devs and reverse engineers to figure out a proper way to do this.")
            .RegisterEntry("You literally have no idea how much effort i put in and things I had to learn just to make forcefollow and forcesit work properly.")
            .RegisterImportant("Literally nobody (at least according to the 20+ hours of asking everywhere has told me) has figured out how to stop movement on LMB+RMB properly before now, so you're welcome for the new feature.")
            .RegisterEntry("/gagspeak blindfold no longer works, and has been moved under the hardcore tab's blindfold control")
            .RegisterEntry("forced /follow now properly works, I would recommend you make sure you are testing it with someone and not by yourself!")
            .RegisterEntry("forced /sit SHOULD not work")
            .RegisterImportant("I am working on fixing the glamourer issues, a lot changed in the latest update with the new Glamourer IPC, so please be patient.");

    private static void Add2_6_0_0(Changelog log)
        => log.NextVersion("Version 2.6.0.0 Release")
            .RegisterHighlight("Hardcore tab features now work on a per-player basis")
            .RegisterEntry("A LOT OF HARDCORE TAB THINGS LIKELY WILL JUST NOT WORK AT ALL, PLEASE DONT REPORT THESE I WILL BE TESTING TODAY")
            .RegisterEntry("If things work but are broken, then feel free to report them.")
            .RegisterEntry("Made hideUI the default option")
            .RegisterEntry("There is now a blindfold (beta)")
            .RegisterHighlight("You can try out the blindfold for now with /gagspeak blindfold");

    private static void Add2_5_0_0(Changelog log)
        => log.NextVersion("Version 2.5.2.0 Release")
            .RegisterHighlight("The Hardcore Tab Beta Tester Availablity is live")
            .RegisterImportant("DO NOT TRY TO BRUTE FORCE THE PASSWORD THIS TIME PLEASE")
            .RegisterImportant("If you do, and things go south, i am not responcible for what happens to you. You've been warned.")
            .RegisterImportant("Fixed the recursion issue causing glamourer's to not write")
            .RegisterEntry("Fixes to the way wardrobe is applied on login, so it enables your active sets on login similar to enabling the plugin")
            .RegisterEntry("The issue where logging in broke the wardrobe and required a disable and re-enable of the plugin to fix has been resolved")
            .RegisterEntry("The issue where the workshop and toybox tabs would not load / render properly is fixed")
            .RegisterEntry("Several more backend functionality for the hardcore tab are now in place")
            .RegisterEntry("Overall modular improvements to everything.");

    private static void Add2_4_0_0(Changelog log)
        => log.NextVersion("Version 2.4.0.0 Release")
            .RegisterHighlight("The Hardcore Tab is now in experiemental mode.")
            .RegisterEntry("Minor fixes to issues reported in puppeteer channels not being able to be selected")
            .RegisterEntry("Minor fixes to issues reported in toybox not loading")
            .RegisterEntry("Minor fixes to issues reported with restraint set locking")
            .RegisterEntry("Minor fixes to issues reported with the wardrobe tab not loading");

    private static void Add2_2_0_0(Changelog log)
        => log.NextVersion("Version 2.2.2.3 Release")
            .RegisterHighlight("Paving the way for the new Hardcore Tab, which will have its logic developed by others.")
            .RegisterEntry("Restructured the CharacterData, added a config migrator for the pre 2.1.6.0 users.")
            .RegisterEntry("Lots of backend work done to make things more organized and less spaghetti.")
            .RegisterEntry("Modular preperation for toybox triggers and hardcore functionality is now implemented.");


    // all versions are added here, the order doesnt madder, but it should be in order of newest to oldest.
    private static void Add2_0_0_0(Changelog log)
        => log.NextVersion("Version 2.1.6.0 Release")
            .RegisterHighlight("I'm aware I have no awareness of version updates, making a full 2.0 update right away, but here we are.")
            .RegisterImportant("New Component now in GagSpeak: THE PUPPETEER TAB")
            .RegisterEntry("    》 Set unique trigger phrases to each person on your whitelist, so you make sure they are truely the only one who can make you do things!")
            .RegisterEntry("    》 Configure permission parameters for each person individually.")
            .RegisterEntry("    》 Create Unique alias tables for each person on your whitelist, so you can make them do things with ease!")
            .RegisterEntry("    》 Regex paramater trigger words are a WIP atm and are coming soon!")
            .RegisterImportant("New Componet now in GagSpeak: THE TOYBOX TAB")
            .RegisterEntry("    》 Unique configuration control for each person on your whitelist, dictate who can and cant use your vibrator!")
            .RegisterEntry("    》 Toggle on and off your partners vibe, and know when it is connected!")
            .RegisterEntry("    》 Set and adjust the intensity of the vibrator at will")
            .RegisterEntry("    》 Create and execute patterns of vibration for your vibrator")
            .RegisterEntry("    》 Lock the UI of the toybox to prevent any changes from being made, making your partner enjoy their partner's pleasure")
            .RegisterEntry("    》 Create your own fully fledged patterns within the new workshop sub-tab!")
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
        => log.NextVersion("Version 1.0.2.0 Release")
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
        => log.NextVersion("Version 1.0.3.0 Release")
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