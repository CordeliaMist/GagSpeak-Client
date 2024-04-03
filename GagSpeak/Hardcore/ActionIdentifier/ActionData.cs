using System.Collections.Generic;

namespace GagSpeak.Hardcore;
// enums for helping retain the correct type of spell for the accosiated properties checked on the action
public enum AcReqProps // "Action Required Properties"
{
    None,           // if the actions has no properties attached
    Movement,       // if the action requires any movement of any kind
    LegMovement,    // if the action requires dramatic movement from legs (flips ext.) 
    ArmMovement,    // if the action requires dramatic movement from arms (punches ext.)
    Speech,         // if the action requires a verbal scream or use of their mouth
    Sight,          // if the action requires direct sight / contact to point at something with, such as ordering a pet to attack something
    Weighted,       // if the action requires a heavy object to be lifted or moved
}

public enum JobType : uint
{
    ADV = 0, // Adventurer
    GLA = 1, // Gladiator
    PGL = 2, // Pugilist
    MRD = 3, // Marauder
    LNC = 4, // Lancer
    ARC = 5, // Archer
    CNJ = 6, // Conjurer
    THM = 7, // Thaumaturge
    CRP = 8, // Carpenter
    BSM = 9, // Blacksmith
    ARM = 10, // Armorer
    GSM = 11, // Goldsmith
    LTW = 12, // Leatherworker
    WVR = 13, // Weaver
    ALC = 14, // Alchemist
    CUL = 15, // Culinarian
    MIN = 16, // Miner
    BTN = 17, // Botanist
    FSH = 18, // Fisher
    PLD = 19, // Paladin
    MNK = 20, // Monk
    WAR = 21, // Warrior
    DRG = 22, // Dragoon
    BRD = 23, // Bard
    WHM = 24, // WhiteMage
    BLM = 25, // BlackMage
    ACN = 26, // Arcanist
    SMN = 27, // Summoner
    SCH = 28, // Scholar
    ROG = 29, // Rogue
    NIN = 30, // Ninja
    MCH = 31, // Machinist
    DRK = 32, // DarkKnight
    AST = 33, // Astrologian
    SAM = 34, // Samurai
    RDM = 35, // RedMage
    BLU = 36, // BlueMage
    GNB = 37, // Gunreturner
    DNC = 38, // Dancer
    RPR = 39, // Reaper
    SGE = 40, // Sage
}

// class for identifying which action is being used and the properties associated with it.
public class ActionData
{
    public static void GetJobActionProperties(JobType job, out Dictionary<uint, AcReqProps[]> bannedActions ) {
        // return the correct dictionary from our core data.
        switch(job) {
            case JobType.ADV : { bannedActions = ActionDataCore.Adventurer; return;} 
            case JobType.GLA : { bannedActions = ActionDataCore.Gladiator; return; }
            case JobType.PGL : { bannedActions = ActionDataCore.Pugilist; return; }
            case JobType.MRD : { bannedActions = ActionDataCore.Marauder; return; }
            case JobType.LNC : { bannedActions = ActionDataCore.Lancer; return; }
            case JobType.ARC : { bannedActions = ActionDataCore.Archer; return; }
            case JobType.CNJ : { bannedActions = ActionDataCore.Conjurer; return; }
            case JobType.THM : { bannedActions = ActionDataCore.Thaumaturge; return; }
            case JobType.CRP : { bannedActions = ActionDataCore.Carpenter; return; }
            case JobType.BSM : { bannedActions = ActionDataCore.Blacksmith; return; }
            case JobType.ARM : { bannedActions = ActionDataCore.Armorer; return; }
            case JobType.GSM : { bannedActions = ActionDataCore.Goldsmith; return; }
            case JobType.LTW : { bannedActions = ActionDataCore.Leatherworker; return; }
            case JobType.WVR : { bannedActions = ActionDataCore.Weaver; return; }
            case JobType.ALC : { bannedActions = ActionDataCore.Alchemist; return; }
            case JobType.CUL : { bannedActions = ActionDataCore.Culinarian; return; }
            case JobType.MIN : { bannedActions = ActionDataCore.Miner; return; }
            case JobType.BTN : { bannedActions = ActionDataCore.Botanist; return; }
            case JobType.FSH : { bannedActions = ActionDataCore.Fisher; return; }
            case JobType.PLD : { bannedActions = ActionDataCore.Paladin; return; }
            case JobType.MNK : { bannedActions = ActionDataCore.Monk; return; }
            case JobType.WAR : { bannedActions = ActionDataCore.Warrior; return; }
            case JobType.DRG : { bannedActions = ActionDataCore.Dragoon; return; }
            case JobType.BRD : { bannedActions = ActionDataCore.Bard; return; }
            case JobType.WHM : { bannedActions = ActionDataCore.WhiteMage; return; }
            case JobType.BLM : { bannedActions = ActionDataCore.BlackMage; return; }
            case JobType.ACN : { bannedActions = ActionDataCore.Arcanist; return; }
            case JobType.SMN : { bannedActions = ActionDataCore.Summoner; return; }
            case JobType.SCH : { bannedActions = ActionDataCore.Scholar; return; }
            case JobType.ROG : { bannedActions = ActionDataCore.Rogue; return; }
            case JobType.NIN : { bannedActions = ActionDataCore.Ninja; return; }
            case JobType.MCH : { bannedActions = ActionDataCore.Machinist; return; }
            case JobType.DRK : { bannedActions = ActionDataCore.DarkKnight; return; }
            case JobType.AST : { bannedActions = ActionDataCore.Astrologian; return; }
            case JobType.SAM : { bannedActions = ActionDataCore.Samurai; return; }
            case JobType.RDM : { bannedActions = ActionDataCore.RedMage; return; }
            case JobType.BLU : { bannedActions = ActionDataCore.BlueMage; return; }
            case JobType.GNB : { bannedActions = ActionDataCore.Gunbreaker; return; }
            case JobType.DNC : { bannedActions = ActionDataCore.Dancer; return; }
            case JobType.RPR : { bannedActions = ActionDataCore.Reaper; return; }
            case JobType.SGE : { bannedActions = ActionDataCore.Sage; return; }
            default: { bannedActions = new Dictionary<uint, AcReqProps[]>(); return; } // return an empty list if job does not exist
        }
    }
}