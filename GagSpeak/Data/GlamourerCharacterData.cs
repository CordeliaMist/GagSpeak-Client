using System.Collections.Generic;

namespace GagSpeak.Data;
/// <summary>
/// A data class for any customization pulled from glamourer, and can be manipulated in a way that allows for easy modification and re-serilization.
/// <para> You need to have a reference for everything, even the things we dont use, in order for it to be parsed out correctly. </para>
/// </summary>
public class GlamourerCharacterData {
    public int FileVersion { get; set; }
    // public string Identifier { get; set; } = "";
    // public string CreationDate { get; set; } = "";
    // public string LastEdit { get; set; } = "";
    // public string Name { get; set; } = "";
    // public string Description { get; set; } = "";
    // public string Color { get; set; } = "";
    // public List<object> Tags { get; set; } = new();
    // public bool WriteProtected { get; set; } = false;
    public Equipment Equipment { get; set; } = new();
    public Customize Customize { get; set; } = new();
    // public List<object> Mods { get; set; } = new();
}

// equipment components
public class Equipment {
    public MainHand MainHand { get; set; } = new();
    public OffHand OffHand { get; set; } = new();
    public Head Head { get; set; } = new();
    public Body Body { get; set; } = new();
    public Hands Hands { get; set; } = new();
    public Legs Legs { get; set; } = new();
    public Feet Feet { get; set; } = new();
    public Ears Ears { get; set; } = new();
    public Neck Neck { get; set; } = new();
    public Wrists Wrists { get; set; } = new();
    public RFinger RFinger { get; set; } = new();
    public LFinger LFinger { get; set; } = new();
    public Hat Hat { get; set; } = new();
    public Visor Visor { get; set; } = new();
    public Weapon Weapon { get; set; } = new();
}

#region Equipment
public class MainHand {
    public ulong ItemId { get; set; } = 0;
    public int Stain { get; set; } = 0;
    public bool Apply { get; set; } = false;
    public bool ApplyStain { get; set; } = false;
}
// Similar classes for OffHand, Head, Body, Hands, Legs, Feet, Ears, Neck, Wrists, RFinger, LFinger
public class OffHand {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Head {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Body {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Hands {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Legs {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Feet {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Ears {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Neck {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Wrists {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class RFinger {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class LFinger {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
}

public class Hat {
    public bool Show { get; set; } = false;
    public bool Apply { get; set; } = false;
}

public class Visor {
    public bool IsToggled { get; set; } = false;
    public bool Apply { get; set; } = false;
}

public class Weapon {
    public bool Show { get; set; } = false;
    public bool Apply { get; set; } = false;
}
#endregion Equipment

public class Customize {
    public int ModelId { get; set; } = 0;
    public Race Race { get; set; } = new();
    public Gender Gender { get; set; } = new();
    public BodyType BodyType { get; set; } = new();
    public Height Height { get; set; } = new();
    public Clan Clan { get; set; } = new();
    public Face Face { get; set; } = new();
    public Hairstyle Hairstyle { get; set; } = new();
    public Highlights Highlights { get; set; } = new();
    public SkinColor SkinColor { get; set; } = new();
    public EyeColorRight EyeColorRight { get; set; } = new();
    public HairColor HairColor { get; set; } = new();
    public HighlightsColor HighlightsColor { get; set; } = new();
    public FacialFeature1 FacialFeature1 { get; set; } = new();
    public FacialFeature2 FacialFeature2 { get; set; } = new();
    public FacialFeature3 FacialFeature3 { get; set; } = new();
    public FacialFeature4 FacialFeature4 { get; set; } = new();
    public FacialFeature5 FacialFeature5 { get; set; } = new();
    public FacialFeature6 FacialFeature6 { get; set; } = new();
    public FacialFeature7 FacialFeature7 { get; set; } = new();
    public LegacyTattoo LegacyTattoo { get; set; } = new();
    public TattooColor TattooColor { get; set; } = new();
    public Eyebrows Eyebrows { get; set; } = new();
    public EyeColorLeft EyeColorLeft { get; set; } = new();
    public EyeShape EyeShape { get; set; } = new();
    public SmallIris SmallIris { get; set; } = new();
    public Nose Nose { get; set; } = new();
    public Jaw Jaw { get; set; } = new();
    public Mouth Mouth { get; set; } = new();
    public Lipstick Lipstick { get; set; } = new();
    public LipColor LipColor { get; set; } = new();
    public MuscleMass MuscleMass { get; set; } = new();
    public TailShape TailShape { get; set; } = new();
    public BustSize BustSize { get; set; } = new();
    public FacePaint FacePaint { get; set; } = new();
    public FacePaintReversed FacePaintReversed { get; set; } = new();
    public FacePaintColor FacePaintColor { get; set; } = new();
    public Wetness Wetness { get; set; } = new();
}

#region Customize
public class Race {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Gender {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class BodyType {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Height {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Clan {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Face {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Hairstyle {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Highlights {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class SkinColor {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class EyeColorRight {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class HairColor {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class HighlightsColor {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature1 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature2 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature3 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature4 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature5 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature6 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacialFeature7 {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class LegacyTattoo {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class TattooColor {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Eyebrows {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class EyeColorLeft {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class EyeShape {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class SmallIris {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Nose {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Jaw {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Mouth {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Lipstick {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class LipColor {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class MuscleMass {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class TailShape {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class BustSize {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacePaint {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacePaintReversed {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class FacePaintColor {
    public int Value { get; set; }
    public bool Apply { get; set; }
}
public class Wetness {
    public bool Value { get; set; }
    public bool Apply { get; set; }
}
#endregion Customize