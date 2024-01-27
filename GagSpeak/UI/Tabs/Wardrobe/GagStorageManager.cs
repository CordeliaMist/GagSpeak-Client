

using GagSpeak.Services;

namespace GagSpeak.Wardrobe;

// Mess with this concept later for optimization purposes.


// // stores, updates, and saves the manager for the gag storage
// public class GagStorageManager : ISavable, IReadOnlyList<AutoDesignSet>, IDisposable
// {
//     public const int CurrentVersion = 1;

//     private readonly SaveService _saveService;

//     public GagStorageManager(SaveService saveService)
//     {
//         _saveService = saveService;
//         // _saveService.RegisterSavable(this);
//     }



//     private JObject Serialize()
//     {
//         var array = new JArray();
//         foreach (var set in _data)
//             array.Add(set.Serialize());

//         return new JObject()
//         {
//             ["Version"] = CurrentVersion,
//             ["Data"]    = array,
//         };
//     }