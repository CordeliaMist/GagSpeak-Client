using Dalamud.Plugin;
using GagSpeak.Events;
using GagSpeak.Chat;
using GagSpeak.UI;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;
using Microsoft.Extensions.DependencyInjection;
using OtterGui.Classes;
using OtterGui.Log;
using XivCommon.Functions;
using Dalamud.Game;
using Lumina.Excel.GeneratedSheets;


// practicing modular design
namespace GagSpeak.Services;

public static class ServiceHandler
{
    // Lets make a service handler to manage our services.
    public static ServiceProvider CreateProvider(DalamudPluginInterface pi, Logger log) {
        // If we eventually need events, we can introduce the logger, but for now we dont need to.
        EventWrapper.ChangeLogger(log);
        
        // Create a service collection (see Dalamud.cs)
        var services = new ServiceCollection()
            .AddSingleton(log) // Many services DEPEND on the logger, so we need to add it!!!!
            // Add the rest of our needed services. Each service below is a CATAGORY. Scroll down to see sub-sections.
            .AddDalamud(pi)
            .AddMeta()
            .AddEvents()
            .AddChat()
            .AddUi()
            .AddApi();
        // return the built services provider
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
    }

    private static IServiceCollection AddDalamud(this IServiceCollection services, DalamudPluginInterface pi)
    {
        // Add the dalamudservices to the service collection
        new DalamudServices(pi).AddServices(services);
        return services;
    }

    private static IServiceCollection AddMeta(this IServiceCollection services)
        => services.AddSingleton<GagSpeakConfig>()
             .AddSingleton<FilenameService>()
             .AddSingleton<HistoryService>()
             .AddSingleton<FrameworkManager>()
             .AddSingleton<SaveService>()
             .AddSingleton<BackupService>()
             .AddSingleton<ConfigMigrationService>()
             .AddSingleton<MessageService>();

    private static IServiceCollection AddEvents(this IServiceCollection services)
        => services.AddSingleton<TabSelected>();

    //SERVICES FOR ONCHAT, INCLUDE IF EVER NEEDED.
    private static IServiceCollection AddChat(this IServiceCollection services)
        => services.AddSingleton<ChatManager>()
             .AddSingleton<RealChatInteraction>(_ => {var sigService = _.GetRequiredService<ISigScanner>(); return new RealChatInteraction(sigService);});
              /* I want to add ISigService service here */

    // SERVICES FOR UI, INCLUDE IF EVER NEEDED.
    private static IServiceCollection AddUi(this IServiceCollection services)
        => services.AddSingleton<GagSpeakWindowManager>()
            .AddSingleton<GeneralTab>()
            .AddSingleton<WhitelistTab>()
            .AddSingleton<ConfigSettingsTab>()
            .AddSingleton<HistoryWindow>()
            .AddSingleton<MainWindow>();

    // SERVICES FOR API, INCLUDE IF EVER NEEDED. (Commands, Config?)
    private static IServiceCollection AddApi(this IServiceCollection services)
        => services.AddSingleton<CommandManager>();
}
