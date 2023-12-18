using System;                                       // Provides fundamental classes for base data types
using System.Collections.Generic;                   // Provides classes for defining generic collections
using GagSpeak.Services;                            // Contains service classes used in the GagSpeak application
using GagSpeak.Events;                              // Contains event classes used in the GagSpeak application
using GagSpeak.Data;                                // Contains data classes used in the GagSpeak application
using GagSpeak.UI.Helpers;                          // Contains chat classes used in the GagSpeak application
using Dalamud.Plugin.Services;                      // Contains service classes provided by the Dalamud plugin framework
using Dalamud.Game.Text.SeStringHandling.Payloads;  // Contains classes for handling special encoded (SeString) payloads in the Dalamud game
using Dalamud.Game.Text.SeStringHandling;           // Contains classes for handling special encoded (SeString) strings in the Dalamud game
using OtterGui.Classes;                             // Contains classes for managing the OtterGui framework

namespace GagSpeak.Services;

/// <summary> Service for managing the gags. </summary>
public class GagService 
{
    private readonly    GagSpeakConfig          _config;    // The GagSpeak configuration
    public              Dictionary<string, Gag> GagTypes;   // Dictionary of gag types

    public GagService(GagSpeakConfig config) {
        _config = config;
        
        GagTypes = new() {
        // no special notes
        { "None",
        new Gag("None", LipPos.NotDefined, RestrictionLvl.None, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.Partial, _config)},
        
        // Keeps the lips in open pos. Tongue can still move around but not past near-front positions. Jaw restricted slightly due to ball, which doubles as
        // partial packing for the mouth.
        { "Ball Gag", 
        new Gag("Ball Gag", LipPos.Open, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Partial, _config)},
        
        // Ball Gag Mask 
        { "Ball Gag Mask", 
        new Gag("Ball Gag Mask", LipPos.Open, RestrictionLvl.Complete, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.Partial, _config)},
        
        // bamboo gag , the same as the bitgag
        { "Bamboo Gag", 
        new Gag("Bamboo Gag", LipPos.Mid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},
        
        // bit gag keeps the lips in a mid-open position. lips restriction however is very minimal. Tongue and jaw are not restricted at all.
        { "Bit Gag", 
        new Gag("Bit Gag", LipPos.Mid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},
        
        // The bone gag acts much like the bitgag, except the lips are in a mid-open position, and have partial restriction. all else is the same.
        { "Bone Gag", 
        new Gag("Bone Gag", LipPos.OpenMid, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},
        
        // The cage muzzle is mostly for aesthetic purposes, it does not muffle anything, and allows the jaw to move freely. The lips are restricted to a mid-open position.
        { "Cage Muzzle", 
        new Gag("Cage Muzzle", LipPos.NotDefined, RestrictionLvl.None, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.None, _config)}, 
        
        // Cloroform cloth would keep the mouth in a mid position to take in the chemicals, but it would also be partially packed. The tongue and jaw wouldnt be
        // restricted, and have full movement.
        { "Chloroform Cloth", 
        new Gag("Chloroform Cloth", LipPos.Mid, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.Partial, _config)},
        
        // chopstick gag keeps the lips shut tight, with very high restriction. the tongue is stuck between the chopsticks, and jaw's restriction is limited by the strain
        // caused from the tongue as you open it down further
        { "Chopstick Gag", 
        new Gag("Chopstick Gag", LipPos.CloseMid, RestrictionLvl.Silenced, TonguePos.Front, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.None, _config)},
        
        // Cloth gags keep the mouth near-open, but have light restriction. The tongue and jaw are not restricted at all.
        { "Cloth Gag", 
        new Gag("Cloth Gag", LipPos.OpenMid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Partial, _config)},
        
        // Cloth Stuffing doesnt impair your lip movement at all, however, it does force the tongue to the near-back, with a complete restriction. 
        // It also partially opens the jaw, and has complete packing.
        { "Cloth Stuffing", 
        new Gag("Cloth Stuffing", LipPos.NotDefined, RestrictionLvl.None, TonguePos.NearBack, RestrictionLvl.Complete, RestrictionLvl.Partial, RestrictionLvl.Complete, _config)},
        
        // the crop is light the bitgag, but even less intense. You are basically just holding onto something for them.
        { "Crop", 
        new Gag("Crop", LipPos.Close, RestrictionLvl.Complete, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},
        
        // Open-Mid	Partial	None	None	Light	Partial
        { "Cup Holder Gag", 
        new Gag("Cup Holder Gag", LipPos.OpenMid, RestrictionLvl.Partial, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Partial, _config)},
        
        // Big penis gag, goes far back deep, pushes tongue back and forces it into place, completely silences airflow via total packing
        { "Deepthroat Penis Gag", 
        new Gag("Deepthroat Penis Gag", LipPos.Open, RestrictionLvl.Partial, TonguePos.Back, RestrictionLvl.Complete, RestrictionLvl.Light, RestrictionLvl.Silenced, _config)},
        
        // Dental gag keeps the mouth mostly open and strains the lips locked in place. Tongue not affected, jaw partially lowered, no packing
        { "Dental Gag",
        new Gag("Dental Gag", LipPos.NearOpen, RestrictionLvl.Silenced, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.None, _config)}, 
        
        // Dildo Gag is a lighter version of the deepthrough, only pushing the tongue back to the central zone, with still partial restriction
        { "Dildo Gag",
        new Gag("Dildo Gag", LipPos.Open, RestrictionLvl.Light, TonguePos.Central, RestrictionLvl.Partial, RestrictionLvl.Light, RestrictionLvl.Complete, _config)},
        
        // 
        { "Duct Tape",
        new Gag("Duct Tape", LipPos.NearClose, RestrictionLvl.Complete, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},

        // Duster gag keeps the lips in a mid-open position, with partial restriction. The tongue is forced to the near-front, with partial restriction.
        // The jaw is partially lowered, and the mouth is partially packed.
        { "Duster Gag",
        new Gag("Duster Gag", LipPos.OpenMid, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},
        
        // Funnel Gag keeps the lips in a mid-open position, with partial restriction. The tongue is forced to the near-front, with no restriction, light
        // jaw movement and packing as a result
        { "Funnel Gag",
        new Gag("Funnel Gag", LipPos.OpenMid, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},
        
        // The Futuristic ball gag is a more advanced version of the ball gag. It keeps the lips in an open position with complete lockdown on lip movement.
        // The tongue can move, but the jaw is restricted a significant amount. Due to the perfection of the futuristic style, the mouth is completely packed.
        { "Futuristic Ball Gag",
        new Gag("Futuristic Ball Gag", LipPos.Open, RestrictionLvl.Complete, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Complete, _config)},
        
        // The Futuristic Harness Panel Gag is a more advanced version of the harness panel gag. While it allows freedom to the lips and tongue, it is securely
        // packed around the surface of your skin, preventing any air from escaping.
        { "Futuristic Harness Panel Gag",
        new Gag("Futuristic Harness Panel Gag", LipPos.NotDefined, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Silenced, _config)},
        
        // The Gas Mask is primarily for aesthetic purposes, it does not muffle anything, and allows the jaw to move freely. But there is a light packing to subtly muffle some sounds
        { "Gas Mask",
        new Gag("Gas Mask", LipPos.NotDefined, RestrictionLvl.None, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.Light, _config)},
        
        // The harness ballgag behaves pretty much like the ballgag, except the chinstrap helps confine the jaws movement, making the lips a little more firmly sealed around the ball.
        { "Harness Ball Gag",
        new Gag("Harness Ball Gag", LipPos.Open, RestrictionLvl.Complete, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.Partial, _config)},

        // The XL size gives a slightly higher restriction to the chin and lips, but not much else.
        { "Harness Ball Gag XL",
        new Gag("Harness Ball Gag XL", LipPos.Open, RestrictionLvl.Silenced, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Silenced, RestrictionLvl.Partial, _config)},

        // Harness panel gag keeps mouth moderately open with a partial seal against the lips. Tongue is free to move and the jaw restriction is slightly present
        { "Harness Panel Gag",
        new Gag("Harness Panel Gag", LipPos.Mid, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.Light, _config)},

        // The Hook Gag mask is the same as the hook gag, except has a full head mask, further restricting the jaw and lips
        { "Hook Gag Mask",
        new Gag("Hook Gag Mask", LipPos.Open, RestrictionLvl.Silenced, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.None, _config)},

        // Pretty much seals off any and all exit holes for air, and has a air pump control dictating how much air goes out. Needless to say, its brutal
        { "Inflatable Hood",
        new Gag("Inflatable Hood", LipPos.OpenMid, RestrictionLvl.Complete, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.Silenced, _config)},

        // The Dildo Gag, but larger
        { "Large Dildo Gag",
        new Gag("Large Dildo Gag", LipPos.Open, RestrictionLvl.Partial, TonguePos.NearBack, RestrictionLvl.Complete, RestrictionLvl.Partial, RestrictionLvl.Complete, _config)},
        
        // its a latex hood, but slightly more reasonable
        { "Latex Hood",
        new Gag("Latex Hood", LipPos.Mid, RestrictionLvl.Partial, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Silenced, _config)},
        
        // Latex ball Muzzle gag, a muzzle. but with a ballgag? uwu
        { "Latex Ball Muzzle Gag",
        new Gag("Latex Ball Muzzle Gag", LipPos.NearOpen, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Partial, _config)},
        
        // Latex Posture Collar Gag, a posture collar with a ballgag
        { "Latex Posture Collar Gag",
        new Gag("Latex Posture Collar Gag", LipPos.NotDefined, RestrictionLvl.Light, TonguePos.Front, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.Complete, _config)},
        
        // Leather Corset Collar Gag, a corset collar with a ballgag
        { "Leather Corset Collar Gag",
        new Gag("Leather Corset Collar Gag", LipPos.NotDefined, RestrictionLvl.Light, TonguePos.Front, RestrictionLvl.None, RestrictionLvl.Complete, RestrictionLvl.Complete, _config)},
        
        // Leather Hood, a hood like the latex hood, just with a different material
        { "Leather Hood",
        new Gag("Leather Hood", LipPos.Mid, RestrictionLvl.Partial, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Silenced, _config)},
        
        // Lip gag keeps the lips locked in an open position. Like a ring gag, but more restrictive on the lips, and less gaping
        { "Lip Gag",
        new Gag("Lip Gag", LipPos.Open, RestrictionLvl.Silenced, TonguePos.Front, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.None, _config)},

        // Medical Mask, very light, only meant to imply light packing
        { "Medical Mask",
        new Gag("Medical Mask", LipPos.NotDefined, RestrictionLvl.None, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.Light, _config)},

        // Muzzzzleeeeeee
        { "Muzzle Gag",
        new Gag("Muzzle Gag", LipPos.NotDefined, RestrictionLvl.Light, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Light, _config)},

        // Primarily for packing the mouth, a little lighter load than the cloth stuffing
        { "Panty Stuffing",
        new Gag("Panty Stuffing", LipPos.NotDefined, RestrictionLvl.None, TonguePos.Central, RestrictionLvl.Partial, RestrictionLvl.Light, RestrictionLvl.Partial, _config)},
        
        // Plastic wrap for transparent tight stretchwrap!
        { "Plastic Wrap",
        new Gag("Plastic Wrap", LipPos.NearClose, RestrictionLvl.Complete, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Partial, _config)},

        // The plug gag, basic plug gag
        { "Plug Gag",
        new Gag("Plug Gag", LipPos.OpenMid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.Partial, _config)},

        // Different pump gag levels, each level dampens the sound more and more
        { "Pump Gag lv.1",
        new Gag("Pump Gag lv.1", LipPos.Mid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.Light, RestrictionLvl.None, RestrictionLvl.Light, _config)},

        { "Pump Gag lv.2",
        new Gag("Pump Gag lv.2", LipPos.OpenMid, RestrictionLvl.Partial, TonguePos.Central, RestrictionLvl.Partial, RestrictionLvl.Light, RestrictionLvl.Partial, _config)},

        { "Pump Gag lv.3",
        new Gag("Pump Gag lv.3", LipPos.OpenMid, RestrictionLvl.Complete, TonguePos.Central, RestrictionLvl.Complete, RestrictionLvl.Partial, RestrictionLvl.Complete, _config)},

        { "Pump Gag lv.4",
        new Gag("Pump Gag lv.4", LipPos.OpenMid, RestrictionLvl.Silenced, TonguePos.NearBack, RestrictionLvl.Silenced, RestrictionLvl.Complete, RestrictionLvl.Silenced, _config)},

        // the ribbon gag is mostly for style purposes, but also has its functionality
        { "Ribbon Gag",
        new Gag("Ribbon Gag", LipPos.Mid, RestrictionLvl.None, TonguePos.Front, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.Light, _config)},

        // Ring gag keeps the lips in a mid-open position, with complete restriction. The tongue is forced to the central zone, with partial restriction.
        // The jaw is partially lowered, and the mouth is partially packed.
        { "Ring Gag",
        new Gag("Ring Gag", LipPos.Open, RestrictionLvl.Complete, TonguePos.Central, RestrictionLvl.Complete, RestrictionLvl.None, RestrictionLvl.None, _config)},
        
        // The rope gag is simple, much like the ribbon gag
        { "Rope Gag",
        new Gag("Rope Gag", LipPos.CloseMid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.None, _config)},
        
        // the rubber carrot gag acts a lot like the basic dildo
        { "Rubber Carrot Gag",
        new Gag("Rubber Carrot Gag", LipPos.OpenMid, RestrictionLvl.Light, TonguePos.Central, RestrictionLvl.Partial, RestrictionLvl.Light, RestrictionLvl.Complete, _config)},
        
        // scarf gag is like the cloth wrap gag
        { "Scarf",
        new Gag("Scarf", LipPos.OpenMid, RestrictionLvl.Light, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Partial, _config)},

        // Im sure the name of this hood is self explanitory for what it does.
        { "Sensory Deprivation Hood",
        new Gag("Sensory Deprivation Hood", LipPos.NearOpen, RestrictionLvl.Silenced, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Silenced, RestrictionLvl.Silenced, _config)},
        
        // Bondage Club Users will know the primaral fear of slime...
        { "Slime",
        new Gag("Slime", LipPos.NearClose, RestrictionLvl.Complete, TonguePos.NearBack, RestrictionLvl.Complete, RestrictionLvl.Light, RestrictionLvl.Complete, _config)},
        
        // Think like a panty stuffing, but the end of the sock kinda pokes out of the mouth
        { "Sock Stuffing",
        new Gag("Sock Stuffing", LipPos.CloseMid, RestrictionLvl.Light, TonguePos.NearBack, RestrictionLvl.Complete, RestrictionLvl.Partial, RestrictionLvl.Complete, _config)},
        
        // gape the mouth open, but like, more harsh
        { "Spider Gag",
        new Gag("Spider Gag", LipPos.NearOpen, RestrictionLvl.Complete, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.None, _config)},

        // for the monster porn anthusiasts
        { "Tentacle Gag",
        new Gag("Tentacle Gag", LipPos.OpenMid, RestrictionLvl.Partial, TonguePos.Central, RestrictionLvl.Partial, RestrictionLvl.Light, RestrictionLvl.Complete, _config)},
        
        // Silk covered mouth, for the spider crazy people out there
        { "Web Gag",
        new Gag("Web Gag", LipPos.NearClose, RestrictionLvl.Complete, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.Light, RestrictionLvl.Partial, _config)},

        // Its a wiffle ball! Like a ballgag but slightly more air can pass through!
        { "Wiffle Ball Gag",
        new Gag("Wiffle Ball Gag", LipPos.Open, RestrictionLvl.Partial, TonguePos.NearFront, RestrictionLvl.None, RestrictionLvl.Partial, RestrictionLvl.Partial, _config)},

        // The XL size gives a slightly higher restriction to the chin and lips, but not much else.
        { "XL Bone Gag",
        new Gag("XL Bone Gag", LipPos.NotDefined, RestrictionLvl.None, TonguePos.NotDefined, RestrictionLvl.None, RestrictionLvl.None, RestrictionLvl.None, _config)}
        };
    }
}
    