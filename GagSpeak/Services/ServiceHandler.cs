using Dalamud.Plugin;
using GagSpeak.Events;
using GagSpeak.Chat;
using GagSpeak.UI;
using GagSpeak.UI.Tabs;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;
using Microsoft.Extensions.DependencyInjection;
using OtterGui.Classes;
using OtterGui.Log;


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
            //.AddSingleton(log)

            // Add the rest of our needed services. Each service below is a CATAGORY. Scroll down to see sub-sections.
            .AddDalamud(pi)
            .AddMeta()
            //.AddInterop()
            .AddEvents()
            //.AddData()
            .AddChat()
            //.AddState()
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

    // All / most of these services are in glamourer's service tab. Uncomment as you run into cases where you need them
    private static IServiceCollection AddMeta(this IServiceCollection services)
        => services.AddSingleton<GagSpeakConfig>()
             .AddSingleton<FilenameService>()
            // .AddSingleton<BackupService>()
            // .AddSingleton<FrameworkManager>()
             .AddSingleton<SaveService>();
            // .AddSingleton<CodeService>()
            // .AddSingleton<ConfigMigrationService>()
            // .AddSingleton<MessageService>()
            // .AddSingleton<TextureService>()
            // .AddSingleton<FavoriteManager>();

    // For adding any events to the service collections (most of these were under glamourer's event tab)
    private static IServiceCollection AddEvents(this IServiceCollection services)
        => services.AddSingleton<TabSelected>();
            // .AddSingleton<SlotUpdating>()
            // .AddSingleton<DesignChanged>()
            // .AddSingleton<AutomationChanged>()
            // .AddSingleton<StateChanged>()
            // .AddSingleton<WeaponLoading>()
            // .AddSingleton<HeadGearVisibilityChanged>()
            // .AddSingleton<WeaponVisibilityChanged>()
            // .AddSingleton<ObjectUnlocked>()
            // .AddSingleton<VisorStateChanged>()
            // .AddSingleton<MovedEquipment>()
            // .AddSingleton<GPoseService>()
            // .AddSingleton<PenumbraReloaded>();

    // SERVICES FOR DATA, INCLUDE IF EVER NEEDED.
    // private static IServiceCollection AddData(this IServiceCollection services)
    //     => services.AddSingleton<IdentifierService>()
    //         .AddSingleton<ItemService>()
    //         .AddSingleton<ActorService>()
    //         .AddSingleton<CustomizationService>()
    //         .AddSingleton<ItemManager>()
    //         .AddSingleton<HumanModelList>();

    // SERVICES FOR INTEROP, INCLUDE IF EVER NEEDED.
    // private static IServiceCollection AddInterop(this IServiceCollection services)
    //     => services.AddSingleton<VisorService>()
    //         .AddSingleton<ChangeCustomizeService>()
    //         .AddSingleton<MetaService>()
    //         .AddSingleton<UpdateSlotService>()
    //         .AddSingleton<WeaponService>()
    //         .AddSingleton<PenumbraService>()
    //         .AddSingleton<ObjectManager>()
    //         .AddSingleton<PenumbraAutoRedraw>()
    //         .AddSingleton<JobService>()
    //         .AddSingleton<CustomizeUnlockManager>()
    //         .AddSingleton<ItemUnlockManager>()
    //         .AddSingleton<DatFileService>()
    //         .AddSingleton<InventoryService>()
    //         .AddSingleton<ContextMenuService>()
    //         .AddSingleton<ScalingService>();

    // SERVICES FOR ONCHAT, INCLUDE IF EVER NEEDED.
    private static IServiceCollection AddChat(this IServiceCollection services)
        => services.AddSingleton<OnChatManager>();
            // .AddSingleton<DesignFileSystem>()
            // .AddSingleton<AutoDesignManager>()
            // .AddSingleton<AutoDesignApplier>()
            // .AddSingleton<FixedDesignMigrator>()
            // .AddSingleton<DesignConverter>();

    // SERVICES FOR STATE, INCLUDE IF EVER NEEDED.
    // private static IServiceCollection AddState(this IServiceCollection services)
    //     => services.AddSingleton<StateManager>()
    //         .AddSingleton<StateApplier>()
    //         .AddSingleton<StateEditor>()
    //         .AddSingleton<StateListener>()
    //         .AddSingleton<FunModule>();

    // SERVICES FOR UI, INCLUDE IF EVER NEEDED.
    private static IServiceCollection AddUi(this IServiceCollection services)
        => services.AddSingleton<GagSpeakWindowManager>()
            //.AddSingleton<MessagesTab>()
            .AddSingleton<GeneralTab>()
            .AddSingleton<WhitelistTab>()
            .AddSingleton<ConfigSettingsTab>()
            //.AddSingleton<ActorPanel>()
            .AddSingleton<MainWindow>();
            //.AddSingleton<GenericPopupWindow>()
            //.AddSingleton<DebugTab>()
            //.AddSingleton<CustomizationDrawer>()
            //.AddSingleton<EquipmentDrawer>()
            //.AddSingleton<DesignFileSystemSelector>()
            //.AddSingleton<DesignPanel>()
            //.AddSingleton<DesignTab>()
            //.AddSingleton<DesignCombo>() <-- Looks important
            //.AddSingleton<RevertDesignCombo>()
            //.AddSingleton<ModAssociationsTab>()
            //.AddSingleton<DesignDetailTab>()
            //.AddSingleton<UnlockTable>()
            //.AddSingleton<UnlockOverview>()
            //.AddSingleton<UnlocksTab>()
            //.AddSingleton<PenumbraChangedItemTooltip>()
            //.AddSingleton<AutomationTab>()
            //.AddSingleton<SetSelector>()
            //.AddSingleton<SetPanel>()
            //.AddSingleton<IdentifierDrawer>()
            //.AddSingleton<GagSpeakChangelog>() <-- May implement later
            //.AddSingleton<DesignQuickBar>();

    // SERVICES FOR API, INCLUDE IF EVER NEEDED. (Commands, Config?)
    private static IServiceCollection AddApi(this IServiceCollection services)
        => services.AddSingleton<CommandManager>();
            // .AddSingleton<GagSpeakIpc>();
}
