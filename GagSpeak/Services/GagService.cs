using System.Collections.Generic;                   // Provides classes for defining generic collections
using GagSpeak.Data;                                // Contains data classes used in the GagSpeak application

namespace GagSpeak.Services;
/// <summary> Service for managing the gags. </summary>
public class GagService 
{
    private readonly    GagSpeakConfig  _config;    // The GagSpeak configuration
    public              List<Gag>       GagTypes;   // Dictionary of gag types

    public GagService(GagSpeakConfig config) {
        _config = config;
        
        GagTypes = new List<Gag>();

        // no special notes
        AddGagType("None",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial);
        
        // Keeps the lips in open pos. Tongue can still move around but not past near-front positions. Jaw restricted slightly due to ball, which doubles as
        // partial packing for the mouth.
        AddGagType("Ball Gag", 
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Partial);
        
        // Ball Gag Mask 
        AddGagType("Ball Gag Mask", 
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Partial);
        
        // bamboo gag , the same as the bitgag
        AddGagType("Bamboo Gag", 
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);
        
        // bit gag keeps the lips in a mid-open position. lips restriction however is very minimal. Tongue and jaw are not restricted at all.
        AddGagType("Bit Gag", 
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);
        
        // The bone gag acts much like the bitgag, except the lips are in a mid-open position, and have partial restriction. all else is the same.
        AddGagType("Bone Gag", 
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);
        
        // The cage muzzle is mostly for aesthetic purposes, it does not muffle anything, and allows the jaw to move freely. The lips are restricted to a mid-open position.
        AddGagType("Cage Muzzle", 
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None); 
        
        // Cloroform cloth would keep the mouth in a mid position to take in the chemicals, but it would also be partially packed. The tongue and jaw wouldnt be
        // restricted, and have full movement.
        AddGagType("Chloroform Cloth", 
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial);
        
        // chopstick gag keeps the lips shut tight, with very high restriction. the tongue is stuck between the chopsticks, and jaw's restriction is limited by the strain
        // caused from the tongue as you open it down further
        AddGagType("Chopstick Gag", 
        GagEnums.LipPos.CloseMid, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.Front, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None);
        
        // Cloth gags keep the mouth near-open, but have light restriction. The tongue and jaw are not restricted at all.
        AddGagType("Cloth Gag", 
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Partial);
        
        // Cloth Stuffing doesnt impair your lip movement at all, however, it does force the tongue to the near-back, with a complete restriction. 
        // It also partially opens the jaw, and has complete packing.
        AddGagType("Cloth Stuffing", 
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.NearBack, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Complete);
        
        // the crop is light the bitgag, but even less intense. You are basically just holding onto something for them.
        AddGagType("Crop", 
        GagEnums.LipPos.Close, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);
        
        // Open-Mid	Partial	None	None	Light	Partial
        AddGagType("Cup Holder Gag", 
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Partial);
        
        // Big penis gag, goes far back deep, pushes tongue back and forces it into place, completely silences airflow via total packing
        AddGagType("Deepthroat Penis Gag", 
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.Back, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Silenced);
        
        // Dental gag keeps the mouth mostly open and strains the lips locked in place. Tongue not affected, jaw partially lowered, no packing
        AddGagType("Dental Gag",
        GagEnums.LipPos.NearOpen, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.None); 
        
        // Dildo Gag is a lighter version of the deepthrough, only pushing the tongue back to the central zone, with still partial restriction
        AddGagType("Dildo Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.Central, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Complete);
        
        // 
        AddGagType("Duct Tape",
        GagEnums.LipPos.NearClose, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);

        // Duster gag keeps the lips in a mid-open position, with partial restriction. The tongue is forced to the near-front, with partial restriction.
        // The jaw is partially lowered, and the mouth is partially packed.
        AddGagType("Duster Gag",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);
        
        // Funnel Gag keeps the lips in a mid-open position, with partial restriction. The tongue is forced to the near-front, with no restriction, light
        // jaw movement and packing as a result
        AddGagType("Funnel Gag",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);
        
        // The Futuristic ball gag is a more advanced version of the ball gag. It keeps the lips in an open position with complete lockdown on lip movement.
        // The tongue can move, but the jaw is restricted a significant amount. Due to the perfection of the futuristic style, the mouth is completely packed.
        AddGagType("Futuristic Ball Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Complete);
        
        // The Futuristic Harness Panel Gag is a more advanced version of the harness panel gag. While it allows freedom to the lips and tongue, it is securely
        // packed around the surface of your skin, preventing any air from escaping.
        AddGagType("Futuristic Harness Panel Gag",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Silenced);
        
        // The Gas Mask is primarily for aesthetic purposes, it does not muffle anything, and allows the jaw to move freely. But there is a light packing to subtly muffle some sounds
        AddGagType("Gas Mask",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light);
        
        // The harness ballgag behaves pretty much like the ballgag, except the chinstrap helps confine the jaws movement, making the lips a little more firmly sealed around the ball.
        AddGagType("Harness Ball Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Partial);

        // The XL size gives a slightly higher restriction to the chin and lips, but not much else.
        AddGagType("Harness Ball Gag XL",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Silenced, GagEnums.RestrictionLvl.Partial);

        // Harness panel gag keeps mouth moderately open with a partial seal against the lips. Tongue is free to move and the jaw restriction is slightly present
        AddGagType("Harness Panel Gag",
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Light);

        // The Hook Gag mask is the same as the hook gag, except has a full head mask, further restricting the jaw and lips
        AddGagType("Hook Gag Mask",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.None);

        // Pretty much seals off any and all exit holes for air, and has a air pump control dictating how much air goes out. Needless to say, its brutal
        AddGagType("Inflatable Hood",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Silenced);

        // The Dildo Gag, but larger
        AddGagType("Large Dildo Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearBack, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Complete);
        
        // its a latex hood, but slightly more reasonable
        AddGagType("Latex Hood",
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Silenced);
        
        // Latex ball Muzzle gag, a muzzle. but with a ballgag? uwu
        AddGagType("Latex Ball Muzzle Gag",
        GagEnums.LipPos.NearOpen, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Partial);
        
        // Latex Posture Collar Gag, a posture collar with a ballgag
        AddGagType("Latex Posture Collar Gag",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.Front, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Complete);
        
        // Leather Corset Collar Gag, a corset collar with a ballgag
        AddGagType("Leather Corset Collar Gag",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.Front, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Complete);
        
        // Leather Hood, a hood like the latex hood, just with a different material
        AddGagType("Leather Hood",
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Silenced);
        
        // Lip gag keeps the lips locked in an open position. Like a ring gag, but more restrictive on the lips, and less gaping
        AddGagType("Lip Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.Front, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.None);

        // Medical Mask, very light, only meant to imply light packing
        AddGagType("Medical Mask",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light);

        // Muzzzzleeeeeee
        AddGagType("Muzzle Gag",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);

        // Primarily for packing the mouth, a little lighter load than the cloth stuffing
        AddGagType("Panty Stuffing",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.Central, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Partial);
        
        // Plastic wrap for transparent tight stretchwrap!
        AddGagType("Plastic Wrap",
        GagEnums.LipPos.NearClose, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Partial);

        // The plug gag, basic plug gag
        AddGagType("Plug Gag",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial);

        // Just like the bitgag bit gag keeps the lips in a mid-open position. lips restriction however is very minimal. Tongue and jaw are not restricted at all.
        AddGagType("Pony Gag", 
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Light);

        // Different pump gag levels, each level dampens the sound more and more
        AddGagType("Pump Gag lv.1",
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light);

        AddGagType("Pump Gag lv.2",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.Central, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Partial);

        AddGagType("Pump Gag lv.3",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.Central, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Complete);

        AddGagType("Pump Gag lv.4",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.NearBack, GagEnums.RestrictionLvl.Silenced, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Silenced);

        // the ribbon gag is mostly for style purposes, but also has its functionality
        AddGagType("Ribbon Gag",
        GagEnums.LipPos.Mid, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.Front, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light);

        // Ring gag keeps the lips in a mid-open position, with complete restriction. The tongue is forced to the central zone, with partial restriction.
        // The jaw is partially lowered, and the mouth is partially packed.
        AddGagType("Ring Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.Central, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None);
        
        // The rope gag is simple, much like the ribbon gag
        AddGagType("Rope Gag",
        GagEnums.LipPos.CloseMid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.None);
        
        // scarf gag is like the cloth wrap gag
        AddGagType("Scarf",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Partial);

        // Im sure the name of this hood is self explanitory for what it does.
        AddGagType("Sensory Deprivation Hood",
        GagEnums.LipPos.NearOpen, GagEnums.RestrictionLvl.Silenced, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Silenced, GagEnums.RestrictionLvl.Silenced);
        
        // Bondage Club Users will know the primaral fear of slime...
        AddGagType("Slime",
        GagEnums.LipPos.NearClose, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NearBack, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Complete);
        
        // Think like a panty stuffing, but the end of the sock kinda pokes out of the mouth
        AddGagType("Sock Stuffing",
        GagEnums.LipPos.CloseMid, GagEnums.RestrictionLvl.Light, GagEnums.TonguePos.NearBack, GagEnums.RestrictionLvl.Complete, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Complete);
        
        // gape the mouth open, but like, more harsh
        AddGagType("Spider Gag",
        GagEnums.LipPos.NearOpen, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.None);

        // for the monster porn anthusiasts
        AddGagType("Tentacle Gag",
        GagEnums.LipPos.OpenMid, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.Central, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Complete);
        
        // Silk covered mouth, for the spider crazy people out there
        AddGagType("Web Gag",
        GagEnums.LipPos.NearClose, GagEnums.RestrictionLvl.Complete, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Light, GagEnums.RestrictionLvl.Partial);

        // Its a wiffle ball! Like a ballgag but slightly more air can pass through!
        AddGagType("Wiffle Ball Gag",
        GagEnums.LipPos.Open, GagEnums.RestrictionLvl.Partial, GagEnums.TonguePos.NearFront, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.Partial, GagEnums.RestrictionLvl.Partial);

        // The XL size gives a slightly higher restriction to the chin and lips, but not much else.
        AddGagType("XL Bone Gag",
        GagEnums.LipPos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.TonguePos.NotDefined, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None, GagEnums.RestrictionLvl.None);
    }

    private void AddGagType(string name, GagEnums.LipPos lipPos, GagEnums.RestrictionLvl lipRestriction, GagEnums.TonguePos tonguePos,
    GagEnums.RestrictionLvl tongueRestriction, GagEnums.RestrictionLvl jawRestriction, GagEnums.RestrictionLvl packedMouthSeverity) {
        // define the new gag
        var newGag = new Gag(_config);
        // add the info to it
        newGag.AddInfo(name, lipPos, lipRestriction, tonguePos, tongueRestriction, jawRestriction, packedMouthSeverity);
        // append it to the list
        GagTypes.Add(newGag);
    }
}
    