// using System;
// using System.Collections.Generic; // Dictionaries & lists
// using System.Linq; // For .Contains()
// using GagSpeak.Events; // For ItemChangedEventArgs
// using GagSpeak.Data; // For GagAndLockTypes
// using System.Text.RegularExpressions;
// using GagSpeak.Chat.Garbler;
// using System.Xml.Serialization;

// namespace GagSpeak.Chat;

// /// <summary>
// /// Enum to represent priority levels
// /// </summary>
// public enum GagCatagory
// {
//     Fashionable = 0,        // For gag types such as the cage muzzle or loose ribbon wraps. Where sound can clearly be heard, and is for decoration.
//     NancyDrew = 1,          // For gags that 
//     SweetGwendoline = 2,    // For gags that 
//     Gimp = 3,               // For gags that completely seal the mouth, such as mouth sealed latex hoods, pump gags, Dildo's.
// }

// /// <summary>
// /// Interface for all gags
// /// </summary>
// public interface IGag
// {    
//     GagCatagory Catagory { get; set; }
//     public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message){
//         return message;
//     }
// }

// public class FashionableGag : IGag
// {
//     public GagCatagory Catagory { get; set; }

//     // default constructor that calls the augmented with all booleans set to true
//     public FashionableGag() { Catagory = GagCatagory.Fashionable;}
//     public string GarbleMessage(string message) { // this is just fashionable, so we dont need to translate it at all
//         GagSpeak.Log.Debug($"FashionableGag GarbleMessage, Garbling message with gag {Catagory}");
//         return message;
//     }
// }

// /// <summary> The Nancy Drew Gag Class.
// /// <para>Realistically, most gagged speech intended to be 'understood' is going to be from a Nancy Drew class Gag</para>
// /// <list type="bullet">
// /// <item>Think soft gags, in general(ineffective at blocking sound, but enough to muffle words somewhat)</item>
// /// <item>Unpacked [ LeaveGapsOnCorners == TRUE ]</item>
// /// <item>Lips can touch (with effort, so maybe 75% chance) [ AllowsLipFormedConsonants == TRUE ]</item>
// /// <item>Jaw can move (partially) [ ??? ] </item>
// /// <item>Some Air can pass through the mouth (maybe 50% or 75% chance?) [ AllowsAirConsonants == TRUE ]</item>
// /// <item>Tongue can touch the roof of the mouth [ AllowsRearPalletConsonants == TRUE ]</item>
// /// <item>Tongue can't touch the teeth [ AllowsToothConsonants == FALSE ]</item>
// /// </list> </summary>
// public class NancyDrewGag : IGag
// {
//     public GagCatagory Catagory { get; set; }
//     // default constructor that calls the augmented with all booleans set to true
//     public NancyDrewGag() { Catagory = GagCatagory.NancyDrew; }

//     public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message) {
//         return messageGarbler.GarbleMessage(message, 2);
//     }
// }


// public class SweetGwendolineGag : IGag
// {
//     public GagCatagory Catagory { get; set; }
//     // default constructor that calls the augmented with all booleans set to true
//     public SweetGwendolineGag() { Catagory = GagCatagory.SweetGwendoline; }

//     public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message) {
//         return messageGarbler.GarbleMessage(message, 4);
//     }
// }



// public class GimpGag : IGag
// {
//     public GagCatagory Catagory { get; set; }
//     // default constructor that calls the augmented with all booleans set to true
//     public GimpGag() { Catagory = GagCatagory.Gimp; }

//     public string GarbleMessage(GagSpeakConfig config, MessageGarbler messageGarbler, string message) {
//         return messageGarbler.GarbleMessage(message, 7);
//     }
// }

// /// <summary> Class to manage multiple gags </summary>


// // WIP ATM
// public class GagManager : IDisposable
// {
//     private readonly GagSpeakConfig _config;
//     private readonly MessageGarbler _messageGarbler;
//     public List<IGag> activeGags;

//     public GagManager(GagSpeakConfig config, MessageGarbler messageGarbler) {
//         _config = config;
//         _messageGarbler = messageGarbler;
//         // get the IGag class type from the value of the Dictionary<string, IGag> GagTypes, where it searched to see if the key defined by selectedGagTypes is in the dictionary
//         activeGags = _config.selectedGagTypes.Select(gagType => GagAndLockTypes.GagTypes[gagType]).ToList();
//         // print our active list:
//         foreach (var gag in activeGags) {
//             GagSpeak.Log.Debug($"Active Gag: {gag.Catagory}");
//         }

        
//         // subscribe to our events
//         _config.selectedGagTypes.ItemChanged += OnSelectedTypesChanged;
//     }

//     public void Dispose() {
//         // unsubscribe from our events
//         _config.selectedGagTypes.ItemChanged -= OnSelectedTypesChanged;
//     }

//     private void OnSelectedTypesChanged(object sender, ItemChangedEventArgs e) {
//         activeGags = _config.selectedGagTypes.Select(gagType => GagAndLockTypes.GagTypes[gagType]).ToList();
//     }

//     public string ProcessMessage(string message) {
//         int highestPriorityGag = GetHighestPriorityGag();
//         GagSpeak.Log.Debug($"Processing message with gag {activeGags[highestPriorityGag].Catagory}");
//         try {
//             message = activeGags[highestPriorityGag].GarbleMessage(_config, _messageGarbler, message);
//             GagSpeak.Log.Debug($"Message after gag {activeGags[highestPriorityGag].Catagory}: {message}");
//         } 
//         catch (Exception e) {
//             GagSpeak.Log.Error($"Error processing message with gag {activeGags[highestPriorityGag].Catagory}: {e.Message}");
//         }
//         // return the message
//         return message;
//     }

//     private int GetHighestPriorityGag() {
//         int highestPriorityGag = 0; 
//         GagCatagory highestPriority = GagCatagory.Fashionable;

//     for (int i = 0; i < activeGags.Count; i++) {
//         GagSpeak.Log.Debug($"Active Gag: {activeGags[i].Catagory}");
//         // print out the value of the index at the enum for the catagory
//         GagSpeak.Log.Debug($"Active Gag Enum: {(int)activeGags[i].Catagory}");
//         if (activeGags[i].Catagory > highestPriority) {
//             highestPriority = activeGags[i].Catagory;
//             highestPriorityGag = i;
//         }
//     }
//         GagSpeak.Log.Debug($"Index of Highest Priority Gag: {highestPriorityGag}");
//         // will have correct index after this
//         return highestPriorityGag;
//     }
// }