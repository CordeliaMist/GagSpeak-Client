using Dalamud.Plugin;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using OtterGui.Classes;
using OtterGui.Log;
using OtterGui.Services;
using Penumbra.GameData.Structs;
using Penumbra.GameData.Enums;
using XivCommon.Functions;
using GagSpeak.ChatMessages;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.Events;
using GagSpeak.Garbler.Translator;
using GagSpeak.Interop;
using GagSpeak.UI;
using GagSpeak.UI.Tabs.HelpPageTab;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;
using GagSpeak.UI.Tabs.WardrobeTab;
using GagSpeak.ChatMessages.ChatControl;
using GagSpeak.CharacterData;
using GagSpeak.Wardrobe;
using GagSpeak.Gagsandlocks;
using GagSpeak.ToyboxandPuppeteer;
using GagSpeak.UI.Tabs.PuppeteerTab;
using GagSpeak.UI.Tabs.ToyboxTab;
using GagSpeak.UI.Tabs.WorkshopTab;
using GagSpeak.Hardcore;
using GagSpeak.UI.Tabs.HardcoreTab;

namespace GagSpeak.Services;

/// <summary> This class is used to handle the services for the GagSpeak plugin. </summary>
public static class ServiceHandler
{
    /// <summary> Initializes a new instance of the <see cref="ServiceProvider"/> class. </summary>
    /// <returns>The created service provider.</returns>
    public static ServiceManager CreateProvider(DalamudPluginInterface pi, Logger log) {
        // introduce the logger to log any debug messages.
        EventWrapperBase.ChangeLogger(log);
        var services = new ServiceManager(log)
            .AddExistingService(log)
            .AddServiceClasses()
            .AddCharacterData()
            .AddChatMessages()
            .AddEvents()
            .AddGagsAndLocks()
            .AddGarbleCore()
            .AddHardcore()
            .AddInterop()
            .AddToybox()
            .AddUi()
            .AddWardrobe()
            .AddApi();
        DalamudServices.AddServices(services, pi);
        services.AddIServices(typeof(EquipItem).Assembly);
        services.AddIServices(typeof(EquipSlot).Assembly);
        services.AddIServices(typeof(GagSpeak).Assembly);
        services.AddIServices(typeof(EquipFlag).Assembly);
        services.CreateProvider();
        return services;
    }

    /// <summary> Adds the existing services to the service collection </summary>
    private static ServiceManager AddServiceClasses(this ServiceManager services)
        => services.AddSingleton<MessageService>()
            .AddSingleton<FilenameService>()
            .AddSingleton<BackupService>()
            .AddSingleton<FrameworkManager>()
            .AddSingleton<SaveService>()
            .AddSingleton<ConfigMigrationService>()
            .AddSingleton<FontService>()
            .AddSingleton<GagService>()
            .AddSingleton<InfoRequestService>()
            .AddSingleton<OnFrameworkService>()
            .AddSingleton<TimerService>()
            .AddSingleton<GagSpeakConfig>()
            .AddSingleton<PlugService>()
            .AddSingleton<TextureService>();
                
    /// <summary> Classes to add to the service collection from the [Character Data] folder </summary>
    private static ServiceManager AddCharacterData(this ServiceManager services)
        => services.AddSingleton<CharacterHandler>();

    /// <summary> Classes to add to the service collection from the [Chat Messages] folder </summary>
    private static ServiceManager AddChatMessages(this ServiceManager services)
        => services.AddSingleton<MessageEncoder>()
            .AddSingleton<MessageDictionary>()
            .AddSingleton<MessageDecoder>()
            .AddSingleton<ResultLogic>()
            .AddSingleton<OnChatMsgManager>()
            .AddSingleton<EncodedMsgDetector>()
            .AddSingleton<TriggerWordDetector>()
            .AddSingleton<DecodedMessageMediator>()

            // for the xivcommon chat detour/injection classes
            .AddSingleton<ChatInputProcessor>(_ => {
                var sigService = _.GetRequiredService<ISigScanner>(); 
                var interop = _.GetRequiredService<IGameInteropProvider>();
                var config = _.GetRequiredService<GagSpeakConfig>();
                var charaHandler = _.GetRequiredService<CharacterHandler>();
                var gagManagerService = _.GetRequiredService<GagGarbleManager>();
                var dictionary = _.GetRequiredService<MessageDictionary>();
                return new ChatInputProcessor(sigService, interop, config, charaHandler, gagManagerService, dictionary);})
            .AddSingleton<RealChatInteraction>(_ => {
                var sigService = _.GetRequiredService<ISigScanner>();
                return new RealChatInteraction(sigService);}
            );

    /// <summary> Classes to add to the service collection from the [Events] folder </summary>
    private static ServiceManager AddEvents(this ServiceManager services)
        => services.AddSingleton<SafewordUsedEvent>()
            .AddSingleton<InfoRequestEvent>()
            .AddSingleton<InteractOrPermButtonEvent>()
            .AddSingleton<LanguageChangedEvent>()
            .AddSingleton<GagSpeakGlamourEvent>()
            .AddSingleton<ActiveDeviceChangedEvent>()
            .AddSingleton<RestraintSetListChanged>();

    /// <summary> Classes to add to the service collection from the [GagsAndLocks] folder </summary>
    private static ServiceManager AddGagsAndLocks(this ServiceManager services)
        => services.AddSingleton<GagAndLockManager>()
            .AddSingleton<GagGarbleManager>();

    /// <summary> Classes to add to the service collection from the [Garble Core] folder </summary>
    private static ServiceManager AddGarbleCore(this ServiceManager services)
        => services.AddSingleton<IpaParserEN_FR_JP_SP>()
            .AddSingleton<IpaParserCantonese>()
            .AddSingleton<IpaParserMandarian>()
            .AddSingleton<IpaParserPersian>();

    /// <summary> Classes to add to the service collection from the [Hardcore] folder </summary>
    private static ServiceManager AddHardcore(this ServiceManager services)
        => services.AddSingleton<ActionManager>()
            .AddSingleton<MovementManager>();

    /// <summary> Classes to add to the service collection from the [Interop] folder </summary>
    private static ServiceManager AddInterop(this ServiceManager services)
        => services.AddSingleton<GlamourerService>()
            .AddSingleton<GlamourerFunctions>();

    /// <summary> Classes to add to the service collection from the [Toybox] folder </summary>
    private static ServiceManager AddToybox(this ServiceManager services)
        => services.AddSingleton<PatternHandler>()
            .AddSingleton<PatternPlayback>()
            .AddSingleton<WorkshopMediator>()
            .AddSingleton<PuppeteerMediator>()
            .AddSingleton<ToyboxPatternTable>()
            .AddSingleton<SoundPlayer>();

    /// <summary> Classes to add to the service collection from the [UI] folder </summary>
    private static ServiceManager AddUi(this ServiceManager services)
        => services.AddSingleton<GagSpeakWindowManager>()
            .AddSingleton<MainWindow>()
            .AddSingleton<GeneralTab>()
            .AddSingleton<GagListingsDrawer>()
            .AddSingleton<WhitelistTab>()
            .AddSingleton<WhitelistSelector>()
            .AddSingleton<WhitelistPlayerPermissions>()
            .AddSingleton<WardrobeTab>()
            .AddSingleton<WardrobeGagCompartment>()
            .AddSingleton<GagStorageSelector>()
            .AddSingleton<GagStorageDetails>()
            .AddSingleton<WardrobeRestraintCompartment>()
            .AddSingleton<RestraintSetSelector>()
            .AddSingleton<RestraintSetEditor>()
            .AddSingleton<PuppeteerTab>()
            .AddSingleton<PuppeteerSelector>()
            .AddSingleton<PuppeteerPanel>()
            .AddSingleton<PuppeteerAliasTable>()
            .AddSingleton<ToyboxTab>()
            .AddSingleton<ToyboxSelector>()
            .AddSingleton<ToyboxPanel>()
            .AddSingleton<SetupAndInfoSubtab>()
            .AddSingleton<PatternSubtab>()
            .AddSingleton<TriggersSubtab>()
            .AddSingleton<WorkshopTab>()
            .AddSingleton<HardcoreTab>()
            .AddSingleton<HardcoreSelector>()
            .AddSingleton<HardcoreMainPanel>()
            .AddSingleton<HC_RestraintSetProperties>()
            .AddSingleton<HC_OrdersControl>()
            .AddSingleton<HC_Humiliation>()
            .AddSingleton<ConfigSettingsTab>()
            .AddSingleton<HelpPageTab>()
            .AddSingleton<TutorialWindow>()
            .AddSingleton<UserProfileWindow>()
            .AddSingleton<SavePatternWindow>()
            .AddSingleton<DebugWindow>()
            .AddSingleton<GagSpeakChangelog>();

    /// <summary> Classes to add to the service collection from the [Wardrobe] folder </summary>
    private static ServiceManager AddWardrobe(this ServiceManager services)
        => services.AddSingleton<GagStorageManager>()
            .AddSingleton<RestraintSetManager>();

    /// <summary> Adds the "General API & Unsorted" classes to the service collection </summary>
    private static ServiceManager AddApi(this ServiceManager services)
        => services.AddSingleton<CommandManager>();
}
