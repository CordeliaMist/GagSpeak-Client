// using Dalamud.Plugin;
// using Penumbra.Api.Helpers;

// namespace GagSpeak.API;

// public partial class GagSpeakIpc
// {
//     // the label of the API version subsciber
//     public const string LabelApiVersion  = "GagSpeak.ApiVersion";
//     // the func provider
//     private readonly FuncProvider<int> _apiVersionProvider;
//     // the func subscriber
//     public static FuncSubscriber<int> ApiVersionSubscriber(DalamudPluginInterface pi)
//         => new(pi, LabelApiVersion);
//     //nction executed by the subscriber
//     public int ApiVersion() => CurrentApiVersion;
// }