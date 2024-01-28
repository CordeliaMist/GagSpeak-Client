using Dalamud.Plugin;                           // Contains interfaces and classes for creating Dalamud plugins
using GagSpeak.Chat;                            // Contains classes for handling chat in the GagSpeak plugin
using GagSpeak.Chat.MsgDecoder;                 // Contains classes for decoding chat messages in the GagSpeak plugin
using GagSpeak.Chat.MsgEncoder;                 // Contains classes for encoding chat messages in the GagSpeak plugin
using GagSpeak.Data;
using GagSpeak.Chat.MsgResultLogic;             // Contains classes for handling the result of chat messages in the GagSpeak plugin
using GagSpeak.Events;                          // Contains classes for handling events in the GagSpeak plugin
using GagSpeak.Garbler.Translator;
using GagSpeak.Interop;
using GagSpeak.UI;                              // Contains classes for the UI of the GagSpeak plugin
using GagSpeak.UI.Tabs.HelpPageTab;             // Contains classes for the help page tab in the GagSpeak plugin
using GagSpeak.UI.ComboListings;                  // Contains classes for the gag listings in the GagSpeak plugin
using GagSpeak.UI.Tabs.GeneralTab;              // Contains classes for the general tab in the GagSpeak plugin
using GagSpeak.UI.Tabs.WhitelistTab;            // Contains classes for the whitelist tab in the GagSpeak plugin
using GagSpeak.UI.Tabs.ConfigSettingsTab;       // Contains classes for the config settings tab in the GagSpeak plugin
using GagSpeak.UI.UserProfile;
using GagSpeak.UI.Tabs.WardrobeTab;
using GagSpeak.Utility;
using GagSpeak.Wardrobe;
using XivCommon.Functions;                      // Contains classes for common functions in the Xiv game
using Dalamud.Game;                             // Contains classes for interacting with the game
using Dalamud.Plugin.Services;                  // Contains classes for Dalamud plugin services
using Microsoft.Extensions.DependencyInjection; // Provides classes for dependency injection
using OtterGui.Classes;                         // Contains classes for the OtterGui library
using OtterGui.Log;  
using OtterGui.Services;
using System.Linq;
using System.Collections;
using Penumbra.GameData.Structs;
using Penumbra.GameData.Enums;

// following namespace naming convention
namespace GagSpeak.Services;


/// <summary> This class is used to handle the services for the GagSpeak plugin. </summary>
public static class ServiceHandler
{
    /// <summary> Initializes a new instance of the <see cref="ServiceProvider"/> class.
    /// <list type="bullet">
    /// <item><c>pi</c><param name="pi"> - The Dalamud plugin interface.</param></item>
    /// <item><c>log</c><param name="log"> - The logger instance.</param></item>
    /// </list> </summary>
    /// <returns>The created service provider.</returns>
    public static ServiceManager CreateProvider(DalamudPluginInterface pi, Logger log) {
        // introduce the logger to log any debug messages.
        EventWrapperBase.ChangeLogger(log);
        var services = new ServiceManager(log)
            .AddExistingService(log)
            .AddServiceClasses()
            .AddChat()
            .AddData()
            .AddEvent()
            .AddGarbleCore()
            .AddInterop()
            .AddUi()
            .AddApi();
        DalamudServices.AddServices(services, pi);
        services.AddIServices(typeof(EquipItem).Assembly);
        services.AddIServices(typeof(EquipSlot).Assembly);
        services.AddIServices(typeof(GagSpeak).Assembly);
        services.AddIServices(typeof(EquipFlag).Assembly);
        services.CreateProvider();
        return services;
    }

    /// <summary> Adds the Chat related classes to the Chat service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddChat(this ServiceManager services)
        => services.AddSingleton<ChatManager>()
             .AddSingleton<ChatInputProcessor>(_ => {
                // this shit is all a bit wild but its nessisary to handle our danger file stuff correctly.
                // Until you learn more about signatures, i dont advise you to try and replicate this.
                // However, when you do, just know this is how to correctly integrate them into a service collection
                var sigService = _.GetRequiredService<ISigScanner>(); 
                var interop = _.GetRequiredService<IGameInteropProvider>();
                var config = _.GetRequiredService<GagSpeakConfig>();
                var historyService = _.GetRequiredService<HistoryService>();
                var gagManagerService = _.GetRequiredService<GagManager>();
                return new ChatInputProcessor(sigService, interop, config, historyService, gagManagerService);})
             .AddSingleton<RealChatInteraction>(_ => {
                var sigService = _.GetRequiredService<ISigScanner>();
                return new RealChatInteraction(sigService);})
            // rest of the normal singletons
             .AddSingleton<MessageResultLogic>()
             .AddSingleton<MessageEncoder>()
             .AddSingleton<MessageDecoder>();

    /// <summary> Adds the data related classes to the service collection
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddData(this ServiceManager services)
        => services.AddSingleton<GagSpeakConfig>()
            .AddSingleton<PadlockIdentifier>()
            .AddSingleton<GagStorageManager>()
            .AddSingleton<RestraintSetManager>();

    /// <summary> Adds the event related classes to the service collection
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddEvent(this ServiceManager services)
        => services.AddSingleton<SafewordUsedEvent>()
                .AddSingleton<InfoRequestEvent>()
                .AddSingleton<LanguageChangedEvent>()
                .AddSingleton<JobChangedEvent>()
                .AddSingleton<ItemAutoEquipEvent>();

    /// <summary> Adds the classes related to the core of the gagspeak garbler to the service collection
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddGarbleCore(this ServiceManager services)
        => services.AddSingleton<IpaParserEN_FR_JP_SP>()
                .AddSingleton<IpaParserCantonese>()
                .AddSingleton<IpaParserMandarian>()
                .AddSingleton<IpaParserPersian>()
                .AddSingleton<GagManager>();


    private static ServiceManager AddInterop(this ServiceManager services)
        => services.AddSingleton<GlamourerInterop>()
                .AddSingleton<CharaDataHelpers>()
                .AddSingleton<GlamourerIpcFuncs>();
    /// <summary> Adds the classes identified as self-made services for the overarching service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddServiceClasses(this ServiceManager services)
        => services.AddSingleton<FrameworkManager>()
                .AddSingleton<GagAndLockManager>()
                .AddSingleton<MessageService>()
                .AddSingleton<BackupService>()
                .AddSingleton<ConfigMigrationService>()
                .AddSingleton<FilenameService>()
                .AddSingleton<FontService>()
                .AddSingleton<GagService>()
                .AddSingleton<HistoryService>()
                .AddSingleton<InfoRequestService>()
                .AddSingleton<SaveService>()
                .AddSingleton<TimerService>()
                .AddSingleton<TextureService>();

    /// <summary> Adds the UI related classes to the service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddUi(this ServiceManager services)
        => services.AddSingleton<GagSpeakWindowManager>()
            .AddSingleton<GeneralTab>()
            .AddSingleton<WhitelistTab>()
            .AddSingleton<ConfigSettingsTab>()
            .AddSingleton<HelpPageTab>()
            .AddSingleton<MainWindow>()
            .AddSingleton<HistoryWindow>()
            .AddSingleton<UserProfileWindow>()
            .AddSingleton<DebugWindow>()
            .AddSingleton<GagListingsDrawer>()
            .AddSingleton<WardrobeTab>()
            .AddSingleton<WardrobeGagCompartment>()
            .AddSingleton<WardrobeRestraintCompartment>()
            .AddSingleton<RestraintSetManager>()
            .AddSingleton<GagSpeakChangelog>();

    /// <summary> Adds the API services to the API service collection. (just command manager for now but oh well)
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static ServiceManager AddApi(this ServiceManager services)
        => services.AddSingleton<CommandManager>();
}
