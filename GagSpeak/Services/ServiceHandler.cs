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
using GagSpeak.Chat.MsgResultLogic;             // Contains classes for handling the result of chat messages in the GagSpeak plugin
using GagSpeak.Events;                          // Contains classes for handling events in the GagSpeak plugin
using GagSpeak.UI;                              // Contains classes for the UI of the GagSpeak plugin
using GagSpeak.UI.Tabs.HelpPageTab;             // Contains classes for the help page tab in the GagSpeak plugin
using GagSpeak.UI.GagListings;                  // Contains classes for the gag listings in the GagSpeak plugin
using GagSpeak.UI.Tabs.GeneralTab;              // Contains classes for the general tab in the GagSpeak plugin
using GagSpeak.UI.Tabs.WhitelistTab;            // Contains classes for the whitelist tab in the GagSpeak plugin
using GagSpeak.UI.Tabs.ConfigSettingsTab;       // Contains classes for the config settings tab in the GagSpeak plugin
using GagSpeak.UI.UserProfile;                  // Contains classes for the user profile in the GagSpeak plugin

// following namespace naming convention
namespace GagSpeak.Services;


/// <summary>
/// This class is used to handle the services for the GagSpeak plugin.
/// </summary>
public static class ServiceHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceProvider"/> class.
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
            .AddMeta()
            .AddChat()
            .AddEvent()
            .AddUi()
            .AddApi();
        // return the built services provider in the form of a instanced service collection
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    /// <summary>
    /// Adds the Dalamud services to the service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// <item><c>pi</c><param name="pi"> - The Dalamud plugin interface.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddDalamud(this IServiceCollection services, DalamudPluginInterface pi) {
        // Add the dalamudservices to the service collection
        new DalamudServices(pi).AddServices(services);
        return services;
    }

    /// <summary>
    /// Adds the meta services to the Meta service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddMeta(this IServiceCollection services)
        => services.AddSingleton<GagSpeakConfig>()
             .AddSingleton<FilenameService>()
             .AddSingleton<HistoryService>()
             .AddSingleton<FrameworkManager>()
             .AddSingleton<SaveService>()
             .AddSingleton<BackupService>()
             .AddSingleton<ConfigMigrationService>()
             .AddSingleton<MessageService>()
             .AddSingleton<GagAndLockManager>()
             .AddSingleton<TimerService>();

    /// <summary>
    /// Adds the chat services to the Chat service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddChat(this IServiceCollection services)
        => services.AddSingleton<ChatManager>()
             .AddSingleton<RealChatInteraction>(_ => {var sigService = _.GetRequiredService<ISigScanner>(); return new RealChatInteraction(sigService);})
             .AddSingleton<ChatInputProcessor>(_ => {
                // this shit is all a bit wild but its nessisary to handle our danger file stuff correctly. Until you learn more about signatures, i dont advise
                // you to try and replicate this. However, when you do, just know this is how to correctly integrate them into a service collection structure
                var sigService = _.GetRequiredService<ISigScanner>(); 
                var interop = _.GetRequiredService<IGameInteropProvider>();
                var config = _.GetRequiredService<GagSpeakConfig>();
                var historyService = _.GetRequiredService<HistoryService>();
                return new ChatInputProcessor(sigService, interop, config, historyService);})
             .AddSingleton<MessageEncoder>()
             .AddSingleton<MessageDecoder>()
             .AddSingleton<MessageResultLogic>()
             .AddSingleton<GagManager>();

    /// <summary>
    /// Adds the event services to the Event service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddEvent(this IServiceCollection services)
        => services.AddSingleton<SafewordUsedEvent>();

    /// <summary>
    /// Adds the UI services to the UI service collection.
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    private static IServiceCollection AddUi(this IServiceCollection services)
        => services.AddSingleton<GagSpeakWindowManager>()
            .AddSingleton<GeneralTab>()
            .AddSingleton<WhitelistTab>()
            .AddSingleton<ConfigSettingsTab>()
            .AddSingleton<HistoryWindow>()
            .AddSingleton<UserProfileWindow>()
            .AddSingleton<MainWindow>()
            .AddSingleton<HelpPageTab>()
            .AddSingleton<GagListingsDrawer>()
            .AddSingleton<GagSpeakChangelog>();

    /// <summary>
    /// Adds the API services to the API service collection. (idk why i put the command manager in here but oh well)
    /// <list type="bullet">
    /// <item><c>services</c><param name="services"> The service collection to add services to.</param></item>
    /// </list> </summary>
    /// <returns>The service collection.</returns>
    private static IServiceCollection AddApi(this IServiceCollection services)
        => services.AddSingleton<CommandManager>();
}
