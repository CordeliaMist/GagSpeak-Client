
// using System;
// using GagSpeak.Events;
// using Penumbra.Api.Helpers;

// namespace GagSpeak.API;

// public partial class GagSpeakIpc
// {
//     public const string LabelGlamourEventChanged = "GagSpeak.GlamourEventChanged";  // fires whenever a glamour event is fired.
//     public const string LabelRSPropertyChanged   = "GagSpeak.RSPropertyChanged";    // whenever a hardcore property for a restraint set changes
//     public const string LabelRSListChanged       = "GagSpeak.RSListChanged";        // whenever a change to the restraint set list is made
//     public const string LabelStateChanged        = "GagSpeak.RSToggled";            // whenever a restraint set toggles

//     // UpdateType, GagType, Assigner Name
//     private readonly EventProvider<UpdateType, string, string>                  _glamourEventFiredProvider;

//     // ChangeType of Hardcore Property, and if it is disabling or enabling
//     private readonly EventProvider<HardcoreChangeType, RestraintSetChangeType>  _rsPropertyChangedProvider;

//     // the type of change made to the list, and index in the list that it occurs in
//     private readonly EventProvider<ListUpdateType, int>                         _rsListChangedProvider;

//     // if the set was toggled on or off, the index of the set that was toggled, and the assigner attempting to toggle it
//     private readonly EventProvider<RestraintSetToggleType, int, string>         _rsToggledProvider;


//     private void OnGlamourEventFired(object sender, GagSpeakGlamourEventArgs e)
//         => _glamourEventFiredProvider.Invoke(e.UpdateType, e.GagType, e.AssignerName); // send this information to any IPC consumers

//     private void OnRSPropertyChanged(object sender, RS_PropertyChangedEventArgs e)
//         => _rsPropertyChangedProvider.Invoke(e.PropertyType, e.ChangeType);

//     private void OnRSListModified(object sender, RestraintSetListChangedArgs e)
//         => _rsListChangedProvider.Invoke(e.UpdateType, e.SetIndex);

//     private void OnRSToggledChanged(object sender, RS_ToggleEventArgs e)
//         => _rsToggledProvider.Invoke(e.ToggleType, e.SetIndex, e.AssignerName);
// }