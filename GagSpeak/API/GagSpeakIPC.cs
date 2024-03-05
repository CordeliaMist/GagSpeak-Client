// using Dalamud.Game.ClientState.Objects.SubKinds;
// using Dalamud.Game.ClientState.Objects.Types;
// using Dalamud.Plugin;
// using GagSpeak.CharacterData;
// using GagSpeak.Hardcore;
// using GagSpeak.ToyboxandPuppeteer;
// using GagSpeak.Wardrobe;
// using Penumbra.A_pi.Helpers;
// using Penumbra.Api.Helpers;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;

// namespace GagSpeak.API;
// /// <summary>
// /// Quick Overview on how API works
// /// 
// /// - GetIpcProvider is required for each IPC call, the type of regersters vary between their application
// /// 
// /// REGISTER FUNC - This will execute a function to GagSpeak's code whenever this IPC is called,
// /// it is not something sent out, but rather something passed in. (will return something as a value to the client)
// /// 
// /// REGISTER ACTION - This will execute a function to GagSpeak's code whenever this IPC is called, however this call
// /// does not return anything to the client, it simply executes code to the plugin
// /// 
// /// </summary>
// public sealed partial class GagSpeakIpc : IDisposable
// {
//     public const int CurrentApiVersion = 1;
//     private readonly CharacterHandler       _charaHandler;      // stores character and whitelist info
//     private readonly GagSpeakConfig         _config;            // stores general config info
//     private readonly HardcoreManager        _hardcoreManager;   // stores hardcore information
//     private readonly PatternHandler         _patternHandler;    // stores pattern information
//     private readonly RestraintSetManager    _rsManager;         // stores restraint set information
//     private DalamudPluginInterface          _pi;                 // the plugin interface

//     public GagSpeakIpc(DalamudPluginInterface pi, CharacterHandler charaHandler, GagSpeakConfig config,
//     HardcoreManager hardcoreManager, PatternHandler patternHandler, RestraintSetManager rsManager)
//     {
//         // class initializers
//         _charaHandler = charaHandler;
//         _config = config;
//         _hardcoreManager = hardcoreManager;
//         _patternHandler = patternHandler;
//         _rsManager = rsManager;
//         _pi = pi;

//         // API version handler
//         _pi.GetIpcProvider<int>("GagSpeak.ApiVersion").RegisterFunc(() => CurrentApiVersion);
//         _apiVersionProvider  = new FuncProvider<int>(pi, LabelApiVersion, ApiVersion);

//         // for the event IPC calls
//         _pi.GetIpcProvider<


//         _pi.GetIpcProvider<PlayerCharacter, string>("GagSpeak.GetStatusManagerByPC").RegisterFunc(GetStatusManager);
//         _pi.GetIpcProvider<nint, string>("GagSpeak.GetStatusManagerByPtr").RegisterFunc(GetStatusManager);
//         _pi.GetIpcProvider<string, string>("GagSpeak.GetStatusManagerByName").RegisterFunc(GetStatusManager);

//         _pi.GetIpcProvider<string>("GagSpeak.GetStatusManagerLP").RegisterFunc(() => GetStatusManager(_pi.ClientState.LocalPlayer));
//         _pi.GetIpcProvider<PlayerCharacter, string, object>("GagSpeak.SetStatusManagerByPC").RegisterAction(SetStatusManager);
//         _pi.GetIpcProvider<nint, string, object>("GagSpeak.SetStatusManagerByPtr").RegisterAction(SetStatusManager);
//         _pi.GetIpcProvider<string, string, object>("GagSpeak.SetStatusManagerByName").RegisterAction(SetStatusManager);

//         _pi.GetIpcProvider<PlayerCharacter, object>("GagSpeak.ClearStatusManagerByPC").RegisterAction(ClearStatusManager);
//         _pi.GetIpcProvider<nint, object>("GagSpeak.ClearStatusManagerByPtr").RegisterAction(ClearStatusManager);
//         _pi.GetIpcProvider<string, object>("GagSpeak.ClearStatusManagerByName").RegisterAction(ClearStatusManager);

//         _pi.GetIpcProvider<object>("GagSpeak.Ready").SendMessage(); // sends message telling IPC consumers it is finished.

//     }

//     public void Dispose()
//     {
//         _pi.GetIpcProvider<object>("Moodles.Unloading").SendMessage();

//         _pi.GetIpcProvider<int>("Moodles.Version").UnregisterFunc();

//         _pi.GetIpcProvider<string>("Moodles.GetStatusManagerLP").UnregisterFunc();
//         _pi.GetIpcProvider<PlayerCharacter, string>("Moodles.GetStatusManagerByPC").UnregisterFunc();
//         _pi.GetIpcProvider<nint, string>("Moodles.GetStatusManagerByPtr").UnregisterFunc();
//         _pi.GetIpcProvider<string, string>("Moodles.GetStatusManagerByName").UnregisterFunc();

//         _pi.GetIpcProvider<PlayerCharacter, string, object>("Moodles.SetStatusManagerByPC").UnregisterAction();
//         _pi.GetIpcProvider<nint, string, object>("Moodles.SetStatusManagerByPtr").UnregisterAction();
//         _pi.GetIpcProvider<string, string, object>("Moodles.SetStatusManagerByName").UnregisterAction();

//         _pi.GetIpcProvider<PlayerCharacter, object>("Moodles.ClearStatusManagerByPC").UnregisterAction();
//         _pi.GetIpcProvider<nint, object>("Moodles.ClearStatusManagerByPtr").UnregisterAction();
//         _pi.GetIpcProvider<string, object>("Moodles.ClearStatusManagerByName").UnregisterAction();
//     }

//     void ClearStatusManager(string name)
//     {
//         var obj = Svc.Objects.FirstOrDefault(x => x is PlayerCharacter pc && pc.GetNameWithWorld() == name);
//         obj ??= Svc.Objects.FirstOrDefault(x => x is PlayerCharacter pc && pc.Name.ToString() == name);
//         if (obj == null) return;
//         ClearStatusManager((PlayerCharacter)obj);
//     }
//     void ClearStatusManager(nint ptr) => ClearStatusManager((PlayerCharacter)Svc.Objects.CreateObjectReference(ptr));
//     void ClearStatusManager(PlayerCharacter pc)
//     {
//         var m = pc.GetMyStatusManager();
//         foreach(var s in m.Statuses)
//         {
//             if (!s.Persistent)
//             {
//                 m.Cancel(s);
//             }
//         }
//         m.Ephemeral = false;
//     }
// }