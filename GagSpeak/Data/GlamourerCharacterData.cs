using System.Collections.Generic;
#pragma warning disable CS8618 // any non-nullable field will always be initialized when this is called.
namespace GagSpeak.Data;
/// <summary>
/// A data class for any customization pulled from glamourer, and can be manipulated in a way that allows for easy modification and re-serilization.
/// <para> You need to have a reference for everything, even the things we dont use, in order for it to be parsed out correctly. </para>
/// </summary>
public class GlamourerCharacterData {
    public int FileVersion { get; set; }
    ///////////// KEEPING THESE HERE INCASE THE JSON EVER CHANGES REQUIREMENTS ////////////
    // public string Identifier { get; set; } = "";
    // public string CreationDate { get; set; } = "";
    // public string LastEdit { get; set; } = "";
    // public string Name { get; set; } = "";
    // public string Description { get; set; } = "";
    // public string Color { get; set; } = "";
    // public List<object> Tags { get; set; }
    // public bool WriteProtected { get; set; } = false;
    //////////////////////////////////////////////////////////////////////////////////////
    public Equipment Equipment { get; set; }
    public Customize Customize { get; set; }
    public Parameters Parameters { get; set; }
    // public List<object> Mods { get; set; }
}

// equipment components
#region EquipmentClass
public class Equipment {
    public MainHand MainHand { get; set; }
    public OffHand OffHand { get; set; }
    public Head Head { get; set; }
    public Body Body { get; set; }
    public Hands Hands { get; set; }
    public Legs Legs { get; set; }
    public Feet Feet { get; set; }
    public Ears Ears { get; set; }
    public Neck Neck { get; set; }
    public Wrists Wrists { get; set; }
    public RFinger RFinger { get; set; }
    public LFinger LFinger { get; set; }
    public Hat Hat { get; set; }
    public Visor Visor { get; set; }
    public Weapon Weapon { get; set; }
}
#endregion EquipmentClass

#region Equipment
public class MainHand {
    public ulong ItemId { get; set; } = 0;
    public int Stain { get; set; } = 0;
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}
// Similar classes for OffHand, Head, Body, Hands, Legs, Feet, Ears, Neck, Wrists, RFinger, LFinger
public class OffHand {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Head {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Body {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Hands {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Legs {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Feet {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Ears {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Neck {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class Wrists {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class RFinger {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; }
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; }
}

public class LFinger {
    public ulong ItemId { get; set; }
    public int Stain { get; set; }
    public bool Crest { get; set; } = false;
    public bool Apply { get; set; }
    public bool ApplyStain { get; set; }
    public bool ApplyCrest { get; set; } = false;
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

#region CustomizeClass
public class Customize {
    public int ModelId { get; set; } = 0;
    public Race Race { get; set; }
    public Gender Gender { get; set; }
    public BodyType BodyType { get; set; }
    public Height Height { get; set; }
    public Clan Clan { get; set; }
    public Face Face { get; set; }
    public Hairstyle Hairstyle { get; set; }
    public Highlights Highlights { get; set; }
    public SkinColor SkinColor { get; set; }
    public EyeColorRight EyeColorRight { get; set; }
    public HairColor HairColor { get; set; }
    public HighlightsColor HighlightsColor { get; set; }
    public FacialFeature1 FacialFeature1 { get; set; }
    public FacialFeature2 FacialFeature2 { get; set; }
    public FacialFeature3 FacialFeature3 { get; set; }
    public FacialFeature4 FacialFeature4 { get; set; }
    public FacialFeature5 FacialFeature5 { get; set; }
    public FacialFeature6 FacialFeature6 { get; set; }
    public FacialFeature7 FacialFeature7 { get; set; }
    public LegacyTattoo LegacyTattoo { get; set; }
    public TattooColor TattooColor { get; set; }
    public Eyebrows Eyebrows { get; set; }
    public EyeColorLeft EyeColorLeft { get; set; }
    public EyeShape EyeShape { get; set; }
    public SmallIris SmallIris { get; set; }
    public Nose Nose { get; set; }
    public Jaw Jaw { get; set; }
    public Mouth Mouth { get; set; }
    public Lipstick Lipstick { get; set; }
    public LipColor LipColor { get; set; }
    public MuscleMass MuscleMass { get; set; }
    public TailShape TailShape { get; set; }
    public BustSize BustSize { get; set; }
    public FacePaint FacePaint { get; set; }
    public FacePaintReversed FacePaintReversed { get; set; }
    public FacePaintColor FacePaintColor { get; set; }
    public Wetness Wetness { get; set; }
}
#endregion CustomizeClass

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

#region ParametersClass
public class Parameters {
    public FacePaintUvMultiplier FacePaintUvMultiplier { get; set; }
    public FacePaintUvOffset FacePaintUvOffset { get; set; }
    public MuscleTone MuscleTone { get; set; }
    public SkinDiffuse SkinDiffuse { get; set; }
    public SkinSpecular SkinSpecular { get; set; }
    public HairDiffuse HairDiffuse { get; set; }
    public HairSpecular HairSpecular { get; set; }
    public HairHighlight HairHighlight { get; set; }
    public LeftEye LeftEye { get; set; }
    public RightEye RightEye { get; set; }
    public FeatureColor FeatureColor { get; set; }
    public LipDiffuse LipDiffuse { get; set; }
    public DecalColor DecalColor { get; set; }
}
#endregion ParametersClass

#region Parameters
public class FacePaintUvMultiplier {
    public double Value { get; set; }
    public bool Apply { get; set; }
}

public class FacePaintUvOffset {
    public double Value { get; set; }
    public bool Apply { get; set; }
}

public class MuscleTone {
    public double Percentage { get; set; }
    public bool Apply { get; set; }
}

public class SkinDiffuse {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class SkinSpecular {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class HairDiffuse {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class HairSpecular {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class HairHighlight {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class LeftEye {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class RightEye {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class FeatureColor {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public bool Apply { get; set; }
}

public class LipDiffuse {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public double Alpha { get; set; }
    public bool Apply { get; set; }
}

public class DecalColor {
    public double Red { get; set; }
    public double Green { get; set; }
    public double Blue { get; set; }
    public double Alpha { get; set; }
    public bool Apply { get; set; }
}
#endregion Parameters
#pragma warning restore CS8618 // any non-nullable field will always be initialized when this is called.