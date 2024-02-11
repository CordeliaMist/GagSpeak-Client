using System.Linq;

namespace GagSpeak.ChatMessages;

// this class functions as a way to store incoming information correctly by using keywords instead of a list
#pragma warning disable CS8618
public enum DecodedMessageType {
    None = 0,
    GagSpeak = 1,
    Relationship = 2,
    Wardrobe = 3,
    Puppeteer = 4,
    Toybox = 5,
    InfoExchange = 6
}

public class DecodedMessageMediator
{
    public DecodedMessageType msgType { get; set; }     // the type of decoded message
    public int encodedMsgIndex { get; set; }            // the index of the encoded message
#region Attributes
    public string encodedCmdType { get; set; }          // the type of encoded message
    public string assignerName { get; set; }            // the assigner who gave you the command
    public int layerIdx { get; set; }                   // the layer it is meant to be applied for
    public string dynamicLean { get; set; }             // the dynamic lean request sent, if any
    public string theirDynamicLean { get; set; }        // used for transfering both leans in info exchange
    public bool safewordUsed { get; set; }              // if the safeword is used or not
    public bool extendedLockTimes { get; set; }         // if they allow extended lock times [ TIER 2 ]
    public bool directChatGarblerActive { get; set; }   // if the direct chat garbler is active [ TIER 4]
    public bool directChatGarblerLocked { get; set; }   // if the direct chat garbler is locked [ TIER 3]
    /////////////////////////// GAG AND LOCK INFO ///////////////////////////
    public string[] layerGagName { get; set; }         // layer gag name
    public string[] layerPadlock { get; set; }         // layer padlock type
    public string[] layerPassword { get; set; }        // layer padlock password
    public string[] layerTimer { get; set; }           // layer padlock timer
    public string[] layerAssigner { get; set; }        // layer padlock assigner
    /////////////////////////// WARDROBE INFO ///////////////////////////
    public bool isWardrobeEnabled { get; set; }         // is wardrobe enabled [ TIER 0 ]
    public bool isGagStorageLockUIEnabled { get; set; } // state of gag storage lock UI on gaglock [ TIER 1 ]
    public bool isEnableRestraintSets { get; set; }     // is player allowed to enable restraint sets [ TIER 2 ]
    public bool isRestraintSetLocking { get; set; }     // is player allowed to lock restraint sets [ TIER 1 ]
    public string setToLockOrUnlock { get; set; }       // name of the restraint set to lock or unlock 
    /////////////////////////// PUPPETEER INFO ///////////////////////////
    public bool isPuppeteerEnabled { get; set; }        // lets the player set if puppeteer will function [ TIER 4 ]
    public string triggerPhrase { get; set; }           // trigger phrase of messageSender for puppeteer compartment [ TIER 0 ]
    public string triggerStartChar { get; set; }        // what to have instead of () surrounding full commands [ TIER 0 ]
    public string triggerEndChar { get; set; }          // what to have instead of () surrounding full commands [ TIER 0 ]
    public bool allowSitRequests { get; set; }          // does messageSender allow sit requests [ TIER 1 ]
    public bool allowMotionRequests { get; set; }       // does messageSender allow motion requests [ TIER 2 ]
    public bool allowAllCommands { get; set; }          // does messageSender allow all commands [ TIER 4 ]
    /////////////////////////// TOYBOX INFO ///////////////////////////
    public bool isToyboxEnabled { get; set; }           // is messageSenders toybox enabled? [_enableToybox] [ TIER 4 ]
    public bool isChangingToyStateAllowed { get; set; } // state of active toy? ((or pending state update)) [ TIER 1 ]
    public bool isIntensityControlAllowed { get; set; } // does messageSender allow adjusting intensity of toy? [ TIER 3 ]
    public bool toyState { get; set; }                  // state of toy? (is it on) [ Dependant permission on isChangingToyStateAllowed ]
    public int intensityLevel { get; set; }             // current intensity level of active toy ((or new intensity level being sent)) [ TIER 2 ]
    public int toyStepCount { get; set; }               // current step count of active toy ((or new step count being sent)) [ TIER 2 ]
    public bool isUsingPatternsAllowed { get; set; }    // does messageSender allow you to execute storedToyPatterns? [ TIER 4 ]
    public string patternNameToExecute { get; set; }    // name of pattern to execute (not given in infoRequests) (STRING)
    public bool isToyboxLockingAllowed { get; set; }    // does messageSender allow you to lock the toybox UI? [ TIER 3 ]
#endregion Attributes
    public DecodedMessageMediator() {
        // sets all to default values
        ResetAttributes();
    }

    // Helper function for all decoders to get the correct layer out
    public void AssignLayerIdx(string layerIdxStr) {
        if (layerIdxStr == "first")  { layerIdx = 0; return; }
        if (layerIdxStr == "second") { layerIdx = 1; return; }
        if (layerIdxStr == "third")  { layerIdx = 2; return; }
        layerIdx = -1;
    }

    public string GetPlayerName(string playerNameWorld) {
        string[] parts = playerNameWorld.Split(' ');
        return string.Join(" ", parts.Take(parts.Length - 1));
    }

    public void ResetAttributes() {
        // sets all to default values
        msgType = DecodedMessageType.GagSpeak;
        encodedMsgIndex = -1;
        encodedCmdType = "";
        assignerName = "";
        layerIdx = -1;
        dynamicLean = "";
        safewordUsed = false;
        extendedLockTimes = false;
        directChatGarblerActive = false;
        directChatGarblerLocked = false;

        layerGagName = new string[3];
        layerPadlock = new string[3];
        layerPassword = new string[3];
        layerTimer = new string[3];
        layerAssigner = new string[3];

        isWardrobeEnabled = false;
        isGagStorageLockUIEnabled = false;
        isEnableRestraintSets = false;
        isRestraintSetLocking = false;
        setToLockOrUnlock = "";

        isPuppeteerEnabled = false;
        triggerPhrase = "";
        triggerStartChar = "";
        triggerEndChar = "";
        allowSitRequests = false;
        allowMotionRequests = false;
        allowAllCommands = false;

        isToyboxEnabled = false;
        isChangingToyStateAllowed = false;
        isIntensityControlAllowed = false;
        toyState = false;
        intensityLevel = 0;
        toyStepCount = 0;
        isUsingPatternsAllowed = false;
        patternNameToExecute = "";
        isToyboxLockingAllowed = false;
    }
}
#pragma warning restore CS8618