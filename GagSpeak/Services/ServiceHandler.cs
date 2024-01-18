using Dalamud.Plugin;                           // Contains interfaces and classes for creating Dalamud plugins
using XivCommon.Functions;                      // Contains classes for common functions in the Xiv game
using Dalamud.Game;                             // Contains classes for interacting with the game
using Dalamud.Plugin.Services;                  // Contains classes for Dalamud plugin services
using Microsoft.Extensions.DependencyInjection; // Provides classes for dependency injection
using OtterGui.Classes;                         // Contains classes for the OtterGui library
using OtterGui.Log;                             // Contains classes for logging in the OtterGui library
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
using GagSpeak.UI.GagListings;                  // Contains classes for the gag listings in the GagSpeak plugin
using GagSpeak.UI.Tabs.GeneralTab;              // Contains classes for the general tab in the GagSpeak plugin
using GagSpeak.UI.Tabs.WhitelistTab;            // Contains classes for the whitelist tab in the GagSpeak plugin
using GagSpeak.UI.Tabs.ConfigSettingsTab;       // Contains classes for the config settings tab in the GagSpeak plugin
using GagSpeak.UI.UserProfile;

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
    public static ServiceProvider CreateProvider(DalamudPluginInterface pi, Logger log) {
        // introduce the logger to log any debug messages.
        EventWrapper.ChangeLogger(log);
        // Create a service collection (see Dalamud.cs, if confused about AddDalamud, that is what AddDalamud(pi) pulls from)
        var services = new ServiceCollection()
            .AddSingleton(log)
            .AddDalamud(pi)
            .AddChat()
            .AddData()
            .AddEvent()
            .AddGarbleCore()
            .AddInterop()
            .AddServiceClasses()
            .AddUi()
            .AddApi();
        // return the built services provider in the form of a instanced service collection
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    /// <summary> Adds the Dalamud services to the service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// <item><c>pi</c><param name="pi"> - The Dalamud plugin interface.</param></item> </list> </summary>
    private static IServiceCollection AddDalamud(this IServiceCollection services, DalamudPluginInterface pi) {
        // Add the dalamudservices to the service collection
        new DalamudServices(pi).AddServices(services);
        return services;
    }

    /// <summary> Adds the Chat related classes to the Chat service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddChat(this IServiceCollection services)
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
    private static IServiceCollection AddData(this IServiceCollection services)
        => services.AddSingleton<GagSpeakConfig>()
            .AddSingleton<PadlockIdentifier>();

    /// <summary> Adds the event related classes to the service collection
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddEvent(this IServiceCollection services)
        => services.AddSingleton<SafewordUsedEvent>()
                .AddSingleton<InfoRequestEvent>()
                .AddSingleton<LanguageChangedEvent>(); // idkwhy but this works fine without observable list event

    /// <summary> Adds the classes related to the core of the gagspeak garbler to the service collection
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddGarbleCore(this IServiceCollection services)
        => services.AddSingleton<IpaParserEN_FR_JP_SP>()
                .AddSingleton<IpaParserCantonese>()
                .AddSingleton<IpaParserMandarian>()
                .AddSingleton<IpaParserPersian>()
                .AddSingleton<GagManager>();


    private static IServiceCollection AddInterop(this IServiceCollection services)
        => services.AddSingleton<GlamourerInterop>();
    /// <summary> Adds the classes identified as self-made services for the overarching service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddServiceClasses(this IServiceCollection services)
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
                .AddSingleton<TimerService>();

    /// <summary> Adds the UI related classes to the service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddUi(this IServiceCollection services)
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
            .AddSingleton<GagSpeakChangelog>();

    /// <summary> Adds the API services to the API service collection. (just command manager for now but oh well)
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddApi(this IServiceCollection services)
        => services.AddSingleton<CommandManager>();
}
