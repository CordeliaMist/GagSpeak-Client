using System.Collections.Generic;
using GagSpeak.Chat;

namespace GagSpeak.Data {
    public static class GagAndLockTypes
    {
        // embedded dictionary of gag types, not put into a seperate data file because i dont want to deal with doing that honestly.
        public static Dictionary<string, IGag> GagTypes { get; } = new() {
            { "None", new FashionableGag() },
            { "Ball Gag", new NancyDrewGag() },
            { "Ball Gag Mask", new SweetGwendolineGag() },
            { "Bamboo Gag", new SweetGwendolineGag() },
            { "Bit Gag", new NancyDrewGag() },
            { "Bone Gag", new NancyDrewGag() },
            { "Cage Muzzle", new FashionableGag() },
            { "Chloroform Cloth", new NancyDrewGag() },
            { "Chopstick Gag", new NancyDrewGag() },
            { "Cloth Gag", new NancyDrewGag() },
            { "Cloth Stuffing", new NancyDrewGag() },
            { "Crop", new NancyDrewGag() },
            { "Cup Holder Gag", new NancyDrewGag() },
            { "Deepthroat Penis Gag", new GimpGag() },
            { "Dental Gag", new NancyDrewGag() },
            { "Dildo Gag", new GimpGag() },
            { "Duct Tape", new SweetGwendolineGag() },
            { "Duster Gag", new SweetGwendolineGag() },
            { "Exposed Dog Muzzle", new NancyDrewGag() },
            { "Funnel Gag", new SweetGwendolineGag() },
            { "Futuristic Ball Gag", new SweetGwendolineGag() },
            { "Futuristic Harness Panel Gag", new GimpGag() },
            { "Futuristic Panel Gag", new SweetGwendolineGag() },
            { "Gas Mask", new NancyDrewGag() },
            { "Harness Ball Gag", new SweetGwendolineGag() },
            { "Harness Ball Gag XL", new GimpGag() },
            { "Harness Panel Gag", new NancyDrewGag() },
            { "Hook Gag Mask", new NancyDrewGag() },
            { "Inflatable Hood", new SweetGwendolineGag() },
            { "Large Dildo", new SweetGwendolineGag() },
            { "Latex Hood", new SweetGwendolineGag() },
            { "Latex Ball Muzzle Gag", new SweetGwendolineGag() },
            { "Latex Posture Collar Gag", new SweetGwendolineGag() },
            { "Leather Corset Collar Gag", new SweetGwendolineGag() },
            { "Leather Hood", new SweetGwendolineGag() },
            { "Lip Gag", new NancyDrewGag() },
            { "Medical Mask", new NancyDrewGag() },
            { "Muzzle Gag", new SweetGwendolineGag() },
            { "Panty Stuffing", new NancyDrewGag() },
            { "Plastic Wrap", new NancyDrewGag() },
            { "Plug Gag", new SweetGwendolineGag() },
            { "Pony Hood", new SweetGwendolineGag() },
            { "Prison Lockdown Gag", new SweetGwendolineGag() },
            { "Pump Gag lv.1", new NancyDrewGag() },
            { "Pump Gag lv.2", new NancyDrewGag() },
            { "Pump Gag lv.3", new SweetGwendolineGag() },
            { "Pump Gag lv.4", new GimpGag() },
            { "Ribbons", new NancyDrewGag() },
            { "Ring Gag", new NancyDrewGag() },
            { "Rope Gag", new NancyDrewGag() },
            { "Rubber Carrot Gag", new SweetGwendolineGag() },
            { "Scarf", new NancyDrewGag() },
            { "Sensory Deprivation Hood", new GimpGag() },
            { "Slime", new SweetGwendolineGag() },
            { "Sock Stuffing", new NancyDrewGag() },
            { "Spider Gag", new NancyDrewGag() },
            { "Steel Muzzle Gag", new SweetGwendolineGag() },
            { "Stitched Muzzle Gag", new NancyDrewGag() },
            { "Tentacle", new SweetGwendolineGag() },
            { "Web Gag", new NancyDrewGag() },
            { "Wiffle Gag", new NancyDrewGag() },
            { "XL Bone Gag", new SweetGwendolineGag() },
        };
    }
    
    /// <summary>
    /// Padlock enum listing
    /// </summary>
    public enum GagPadlocks {
        None,                   // No gag
        MetalPadlock,           // Metal Padlock, can be picked
        CombinationPadlock,     // Combination Padlock, must enter 4 digit combo to unlock
        PasswordPadlock,        // Password Padlock, must enter password to unlock
        FiveMinutesPadlock,     // 5 minute padlock, must wait 5 minutes to unlock
        TimerPasswordPadlock,   // Timer Password Padlock, must enter password to unlock, but only after a certain amount of time
        MistressPadlock,        // Mistress Padlock, must ask mistress to unlock
        MistressTimerPadlock,   // Mistress Timer Padlock, must ask mistress to unlock, but only after a certain amount of time
    };
}