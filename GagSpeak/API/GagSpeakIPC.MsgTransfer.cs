
// using System;
// using GagSpeak.Events;
// using Penumbra.Api.Helpers;

// namespace GagSpeak.API;

// public partial class GagSpeakIpc
// {
//     // you can make use of this IPC to track the address of the player transferring the information,
//     // and then decline any sent information from people that are not the player that requested it.

//     public const string LabelGPoseChanged        = "GagSpeak.InfoRequested";    // fires whenever a player requests information
//     public const string LabelPlayerInfoSent      = "GagSpeak.InfoSent";         // fires once a player begins sending information that was requested
//     public const string LabelPlayerInfoReceived  = "GagSpeak.InfoReceived";     // fires once a player has finished sending all information

//     // the pointer address of the player that requested information
//     private readonly EventProvider<IntPtr> _infoRequestedProvider;

//     // the pointer address of the player that began sending information
//     private readonly EventProvider<IntPtr> _infoSentProvider;

//     // the pointer address of the player that finished sending information
//     private readonly EventProvider<IntPtr> _infoReceivedProvider;

//     private void OnInfoRequested(object sender, IntPtr e)
//         => _infoRequestedProvider.Invoke(e);

//     private void OnInfoSent(object sender, IntPtr e)
//         => _infoSentProvider.Invoke(e);

//     private void OnInfoReceived(object sender, IntPtr e)
//         => _infoReceivedProvider.Invoke(e);
// }

// // some experiementing with glamourer's IPC but it seems too wide spread for gagspeaks purposes right now