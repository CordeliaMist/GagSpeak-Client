using System;

namespace GagSpeak.Data {

    /// <summary> the type of statechange provided by glamourerIPC </summary>
    public enum StateChangeType {
        Model,
        EntireCustomize,
        Customize,
        Equip,
        Weapon,
        Stain,
        Crest,
        Parameter,
        Design,
        Reset,
        Other,
    }

    public enum LockType { // unsure how ill ever use this but feel it helps make things more modular
        Padlock,        // for general padlock locking
        RestraintSet,   // for restraint set locking
        MasterMistress, // if a master or mistress locked it
        Owner,          // if an owner locked it
        Leash,          // for future if ever
    }


    /// <summary> Padlock enum listing </summary>
    public enum Padlocks {
        None,                   // No gag
        MetalPadlock,           // Metal Padlock, can be picked
        CombinationPadlock,     // Combination Padlock, must enter 4 digit combo to unlock
        PasswordPadlock,        // Password Padlock, must enter password to unlock
        FiveMinutesPadlock,     // 5 minute padlock, must wait 5 minutes to unlock
        TimerPasswordPadlock,   // Timer Password Padlock, must enter password to unlock, but only after a certain amount of time
        MistressPadlock,        // Mistress Padlock, must ask mistress to unlock
        MistressTimerPadlock,   // Mistress Timer Padlock, must ask mistress to unlock, but only after a certain amount of time
    };


    /// <summary> Gag listing </summary>
    public static class GagList {
        #region GagListEnum
        public enum GagType {
            BallGag,
            BallGagMask,
            BambooGag,
            BeltStrapGag,
            BitGag,
            BitGagPadded,
            BoneGag,
            BoneGagXL,
            CandleGag,
            CageMuzzle,
            CleaveGag,
            CloroformGag,
            ChopStickGag,
            ClothWrapGag,
            ClothStuffingGag,
            CropGag,
            CupHolderGag,
            DeepthroatPenisGag,
            DentalGag,
            DildoGag,
            DuctTapeGag,
            DusterGag,
            FunnelGag,
            FuturisticHarnessBallGag,
            FuturisticHarnessPanelGag,
            GasMask,
            HarnessBallGag,
            HarnessBallGagXL,
            HarnessPanelGag,
            HookGagMask,
            InflatableHood,
            LargeDildoGag,
            LatexHood,
            LatexBallMuzzleGag,
            LatexPostureCollarGag,
            LeatherCorsetCollarGag,
            LeatherHood,
            LipGag,
            MedicalMask,
            MuzzleGag,
            PantyStuffingGag,
            PlasticWrapGag,
            PlugGag,
            PonyGag,
            PumpGaglv1,
            PumpGaglv2,
            PumpGaglv3,
            PumpGaglv4,
            RibbonGag,
            RingGag,
            RopeGag,
            ScarfGag,
            SensoryDeprivationHood,
            SockStuffingGag,
            SpiderGag,
            TenticleGag,
            WebGag,
            WiffleGag,
        }
        #endregion GagListEnum
        #region GagListAlias
        public static string GetGagAlias(this GagType gag) => gag switch
        {
            GagType.BallGag => "Ball Gag",
            GagType.BallGagMask => "Ball Gag Mask",
            GagType.BambooGag => "Bamboo Gag",
            GagType.BeltStrapGag => "Belt Strap Gag",
            GagType.BitGag => "Bit Gag",
            GagType.BitGagPadded => "Bit Gag Padded",
            GagType.BoneGag => "Bone Gag",
            GagType.BoneGagXL => "Bone Gag (XL)",
            GagType.CandleGag => "Candle Gag",
            GagType.CageMuzzle => "Cage Muzzle",
            GagType.CleaveGag => "Cleave Gag",
            GagType.CloroformGag => "Cloroform Gag",
            GagType.ChopStickGag => "Chopstick Gag",
            GagType.ClothWrapGag => "Cloth Wrap Gag",
            GagType.ClothStuffingGag => "Cloth Stuffing Gag",
            GagType.CropGag => "Crop Gag",
            GagType.CupHolderGag => "Cup Holder Gag",
            GagType.DeepthroatPenisGag => "Deepthroat Penis Gag",
            GagType.DentalGag => "Dental Gag",
            GagType.DildoGag => "Dildo Gag",
            GagType.DuctTapeGag => "Duct Tape Gag",
            GagType.DusterGag => "Duster Gag",
            GagType.FunnelGag => "Funnel Gag",
            GagType.FuturisticHarnessBallGag => "Futuristic Harness Ball Gag",
            GagType.FuturisticHarnessPanelGag => "Futuristic Harness Panel Gag",
            GagType.GasMask => "Gas Mask",
            GagType.HarnessBallGag => "Harness Ball Gag",
            GagType.HarnessBallGagXL => "Harness Ball Gag XL",
            GagType.HarnessPanelGag => "Harness Panel Gag",
            GagType.HookGagMask => "Hook Gag Mask",
            GagType.InflatableHood => "Inflatable Hood",
            GagType.LargeDildoGag => "Large Dildo Gag",
            GagType.LatexHood => "Latex Hood",
            GagType.LatexBallMuzzleGag => "Latex Ball Muzzle Gag",
            GagType.LatexPostureCollarGag => "Latex Posture Collar Gag",
            GagType.LeatherCorsetCollarGag => "Leather Corset Collar Gag",
            GagType.LeatherHood => "Leather Hood",
            GagType.LipGag => "Lip Gag",
            GagType.MedicalMask => "Medical Mask",
            GagType.MuzzleGag => "Muzzle Gag",
            GagType.PantyStuffingGag => "Panty Stuffing Gag",
            GagType.PlasticWrapGag => "Plastic Wrap Gag",
            GagType.PlugGag => "Plug Gag",
            GagType.PonyGag => "Pony Gag",
            GagType.PumpGaglv1 => "Pump Gag Lv.1",
            GagType.PumpGaglv2 => "Pump Gag Lv.2",
            GagType.PumpGaglv3 => "Pump Gag Lv.3",
            GagType.PumpGaglv4 => "Pump Gag Lv.4",
            GagType.RibbonGag => "Ribbon Gag",
            GagType.RingGag => "Ring Gag",
            GagType.RopeGag => "Rope Gag",
            GagType.ScarfGag => "Scarf Gag",
            GagType.SensoryDeprivationHood => "Sensory Deprivation Hood",
            GagType.SockStuffingGag => "Sock Stuffing Gag",
            GagType.SpiderGag => "Spider Gag",
            GagType.TenticleGag => "Tentacle Gag",
            GagType.WebGag => "Web Gag",
            GagType.WiffleGag => "Wiffle Gag",
            _ => "Unknown Gag"
        };
        #endregion GagListAlias
    }
}