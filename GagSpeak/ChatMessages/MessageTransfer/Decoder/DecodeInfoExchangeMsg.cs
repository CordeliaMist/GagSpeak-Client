using System.Text.RegularExpressions;

namespace GagSpeak.ChatMessages.MessageTransfer;
/// <summary> This class is used to handle the decoding of messages for the GagSpeak plugin. </summary>
public partial class MessageDecoder {
    /// <summary> decodes the recieved and sent messages related to the information exchange with the player.
    public void DecodeInfoExchangeMsg(string recievedMessage, DecodedMessageMediator decodedMessageMediator) {        
        // decoder for requesting information from whitelisted player. [ ID == 36 ]
        if (decodedMessageMediator.encodedMsgIndex == 37) {
            // define the pattern using regular expressions
            string pattern = @"^\((?<playerInfo>.+) would enjoy it if you started our scene together by reminding them of all the various states you were left in\, before we took a break from things for awhile\~\)$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "requestInfo"; // assign "requestInfo" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: request info: (Type) "+
                $"{decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: request info: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for sharing info about player (part 1)
        else if (decodedMessageMediator.encodedMsgIndex == 38) {
            // define the pattern using regular expressions
            string pattern = @"^\*(?<playerInfo>.+)\, their (?<theirStatusToYou>.+)\'s (?<yourStatusToThem>.+) nodded in agreement\, informing their partner of how when they last played together\, (?<safewordUsed>.+)\. (?<extendedLocks>.+)\, (?<gaggedVoice>.+)\, (?<sealedLips>.+)\. \-\>$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                GagSpeak.Log.Debug($"[Message Decoder]: share info groups: {match.Groups["playerInfo"].Value.Trim()} || {match.Groups["yourStatusToThem"].Value.Trim()} || "+
                $"{match.Groups["theirStatusToYou"].Value.Trim()} || {match.Groups["safewordUsed"].Value.Trim()} || {match.Groups["extendedLocks"].Value.Trim()} || "+
                $"{match.Groups["gaggedVoice"].Value.Trim()} || {match.Groups["sealedLips"].Value.Trim()}");
                // command type
                decodedMessageMediator.encodedCmdType = "shareInfoPartOne"; // assign "shareInfo" to decodedMessage[0]
                // player info
                string[] playerInfoParts = match.Groups["playerInfo"].Value.Trim().Split(" from ");
                decodedMessageMediator.assignerName = playerInfoParts[0].Trim() + " " + playerInfoParts[1].Trim();
                // their status to you
                decodedMessageMediator.theirDynamicLean = match.Groups["theirStatusToYou"].Value.Trim();
                // your status to them
                decodedMessageMediator.dynamicLean = match.Groups["yourStatusToThem"].Value.Trim();
                // safeword used
                decodedMessageMediator.safewordUsed = match.Groups["safewordUsed"].Value.Trim() == "they had used their safeword" ? true : false;
                // extended locks
                decodedMessageMediator.extendedLockTimes = match.Groups["extendedLocks"].Value.Trim() == "They didnt mind the enduring binds" ? true : false;
                // gagged voice
                decodedMessageMediator.directChatGarblerActive = match.Groups["gaggedVoice"].Value.Trim() == "and they certain enjoyed their gagged voice" ? true : false;
                // sealed lips
                decodedMessageMediator.directChatGarblerLocked = match.Groups["sealedLips"].Value.Trim() == "for even now their lips were sealed tight" ? true : false;
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: share info1: (Type) {decodedMessageMediator.encodedCmdType} || (Assigner) {decodedMessageMediator.assignerName} || "+
                $"(YourStatusToThem) {decodedMessageMediator.dynamicLean} || (TheirStatusToYou) {decodedMessageMediator.theirDynamicLean} || "+
                $"(SafewordUsed) {decodedMessageMediator.safewordUsed} || (ExtendedLocks) {decodedMessageMediator.extendedLockTimes} || "+
                $"(GaggedVoice) {decodedMessageMediator.directChatGarblerActive} || (SealedLips) {decodedMessageMediator.directChatGarblerLocked}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for sharing info about player (part 2)
        else if (decodedMessageMediator.encodedMsgIndex == 39) {
            // Split the message into substrings for each layer
            string pattern = @"^\|\| When they had last played\, (?<layerInfo>.*?) \-\>$";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(recievedMessage);

            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "shareInfoPartTwo"; // assign "shareInfo2" to decodedMessage[0]
                // split the layer info into the sub sections for each part
                string[] layerInfoParts = match.Groups["layerInfo"].Value.Trim().Split(". ");
                // loop through each layer
                for (int i = 0; i < 3; i++) {
                    // store the parts we want to look for
                    string layerName = i == 0 ? "undermost" : i == 1 ? "main" : "uppermost";
                    string startingWords = i == 0 ? "On her " + layerName + " layer, " : i == 1 ? "Over their mouths " + layerName + " layer, " : "Finally on her " + layerName + " layer, ";
                    // store the current regex for this section
                    string layerInfo = layerInfoParts[i];
                    GagSpeak.Log.Debug($"[Message Decoder]: share info2: layerInfo: {layerInfo}");
                    // if it contains nothing present, then we know we have a blank entry.
                    if (layerInfo.Contains("nothing present")) {
                        decodedMessageMediator.layerGagName[i] = "None";
                        decodedMessageMediator.layerPadlock[i] = "None";
                        decodedMessageMediator.layerTimer[i] = "";
                        decodedMessageMediator.layerAssigner[i] = "";
                    } else {
                        // otherwise, check what the gagtype was.
                        decodedMessageMediator.layerGagName[i] = layerInfo.Split("she had a ")[1].Split(" fastened in good and tight")[0].Trim();
                        // if it was locked, then we need to get the lock type
                        if (layerInfo.Contains("locked with a")) {
                            decodedMessageMediator.layerPadlock[i] = layerInfo.Split("locked with a")[1].Trim().Split(",")[0].Trim();
                            // if it was locked, we need to get the assigner
                            if (layerInfo.Contains("which had been secured by")) {
                                decodedMessageMediator.layerAssigner[i] = layerInfo.Split("which had been secured by")[1].Trim().Split("with")[0].Trim();
                            }
                            // if it was locked with a timer, then we need to get the timer
                            if (layerInfo.Contains("with") && layerInfo.Contains("remaining")) {
                                decodedMessageMediator.layerTimer[i] = layerInfo.Split("with")[1].Trim().Split("remaining")[0].Trim();
                            }
                            // if it was locked with a password, then we need to get the password
                            if(layerInfo.Contains("with the password")) {
                                decodedMessageMediator.layerPassword[i] = layerInfo.Split("with the password")[1].Trim().Split("on the lock")[0].Trim();
                            }
                        }
                    }
                }
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: share info2: (gagtype[0]) = {decodedMessageMediator.layerGagName[0]} || (gagtype[1]) {decodedMessageMediator.layerGagName[1]} "+
                $"|| (gagtype[2]) {decodedMessageMediator.layerGagName[2]} || (gagpadlock[0]) {decodedMessageMediator.layerPadlock[0]} || (gagpadlock[1]) {decodedMessageMediator.layerPadlock[1]} "+
                $"|| (gagpadlock[2]) {decodedMessageMediator.layerPadlock[2]} || (gagtimer[0]) {decodedMessageMediator.layerTimer[0]} || (gagtimer[1]) {decodedMessageMediator.layerTimer[1]} "+
                $"|| (gagtimer[2]) {decodedMessageMediator.layerTimer[2]} || (gagAssigner[0]) {decodedMessageMediator.layerAssigner[0]} || (gagAssigner[1]) {decodedMessageMediator.layerAssigner[1]} "+
                $"|| (gagAssigner[2]) {decodedMessageMediator.layerAssigner[2]} || (gagPassword[0]) {decodedMessageMediator.layerPassword[0]} || (gagPassword[1]) {decodedMessageMediator.layerPassword[1]} "+
                $"|| (gagPassword[2]) {decodedMessageMediator.layerPassword[2]}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info2: Failed to decode message: {recievedMessage}");
            }
        }

        // decoder for our sharing info about player (part 3)
        else if (decodedMessageMediator.encodedMsgIndex == 40) {
            // get the pattern
            string pattern = @"^\|\|\s*(?<wardrobeState>.+?)\.\s*(?<gagStorageState>.+?)\,\s*(?<restraintSetEnable>.+?)\.\s*(?<restraintLock>.+?)\,\s*(?<allowPuppeteer>.+?)\s*their partner whispered\s*(.)(?<puppeteerTrigger>.*?)(.)\s*causing them to\s*(?<sitRequestState>.+?)\.\s*(?<motionRequestState>.+?)\,\s*(?<allCommandsState>.+?)\.\s*\-\>$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if (match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "shareInfoPartThree"; // assign "shareInfo3" to decodedMessage[0]
                // wardrobe state
                decodedMessageMediator.isWardrobeEnabled = match.Groups["wardrobeState"].Value.Trim() == "Their kink wardrobe was accessible for their partner" ? true : false;
                // gag storage state
                decodedMessageMediator.isGagStorageLockUIEnabled = match.Groups["gagStorageState"].Value.Trim() == "The wardrobes gag compartment was closed shut" ? true : false;
                // restraint set enable
                decodedMessageMediator.isEnableRestraintSets = match.Groups["restraintSetEnable"].Value.Trim() == "and their restraint compartment was accessible for their partner" ? true : false;
                // restraint lock
                decodedMessageMediator.isRestraintSetLocking = match.Groups["restraintLock"].Value.Trim() == "They recalled their partner locking their restraints" ? true : false;
                // puppeteer enabled?
                decodedMessageMediator.isPuppeteerEnabled = match.Groups["allowPuppeteer"].Value.Trim() == "loyal as ever when" ? true : false;                
                // puppeteer trigger
                string puppeteerTrigger = match.Groups["puppeteerTrigger"].Value.Trim();
                decodedMessageMediator.triggerPhrase = puppeteerTrigger.Substring(1, puppeteerTrigger.Length - 2);
                decodedMessageMediator.triggerStartChar = puppeteerTrigger[0].ToString();
                decodedMessageMediator.triggerEndChar = puppeteerTrigger[puppeteerTrigger.Length - 1].ToString();
                // sit request state
                decodedMessageMediator.allowSitRequests = match.Groups["sitRequestState"].Value.Trim() == "sit down on command" ? true : false;
                // motion request state
                decodedMessageMediator.allowMotionRequests = match.Groups["motionRequestState"].Value.Trim() == "For their partner controlled their movements" ? true : false;
                // all commands state
                decodedMessageMediator.allowAllCommands = match.Groups["allCommandsState"].Value.Trim() == "and all of their actions" ? true : false;
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: share info3: (Type) {decodedMessageMediator.encodedCmdType} || (WardrobeState) {decodedMessageMediator.isWardrobeEnabled} || "+
                $"(GagStorageState) {decodedMessageMediator.isGagStorageLockUIEnabled} || (RestraintSetEnable) {decodedMessageMediator.isEnableRestraintSets} || "+
                $"(RestraintLock) {decodedMessageMediator.isRestraintSetLocking} || (PuppeteerTrigger) {decodedMessageMediator.triggerPhrase} || "+
                $"(SitRequestState) {decodedMessageMediator.allowSitRequests} || (MotionRequestState) {decodedMessageMediator.allowMotionRequests} || "+
                $"(AllCommandsState) {decodedMessageMediator.allowAllCommands} || (toyboxEnabled) {decodedMessageMediator.isToyboxEnabled}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info3: Failed to decode message: {recievedMessage}");
            }
        }

        
                // decoder for our sharing info about player (part 4)
        else if (decodedMessageMediator.encodedMsgIndex == 41) {
            // get the pattern
            string pattern = @"^\|\|\s*(?<toyboxEnabled>.+?)\.\s*Within the drawer there\s*(?<toggleToyState>.+?),\s*(?<canControlIntensity>.+?)\s*currently set to\s*(?<intensityLevel>\d+)\.\s*(?<toyPatternState>.+?)\,\s*(?<toyboxLockState>.+?)\.\s*(?<toyActiveState>.+?)$";
            // use regex to match the pattern
            Match match = Regex.Match(recievedMessage, pattern);
            // check if the match is sucessful
            if(match.Success) {
                // command type
                decodedMessageMediator.encodedCmdType = "shareInfoPartFour"; // assign "shareInfo3" to decodedMessage[0]
                // toybox enabled
                decodedMessageMediator.isToyboxEnabled = match.Groups["toyboxEnabled"].Value.Trim() == "Their toybox compartment accessible to use" ? true : false;
                // toybox state
                decodedMessageMediator.isChangingToyStateAllowed = match.Groups["toggleToyState"].Value.Trim() == "was powered Vibrator" ? true : false;
                // intensity control state
                decodedMessageMediator.isIntensityControlAllowed = match.Groups["canControlIntensity"].Value.Trim() == "with an adjustable intensity level" ? true : false;
                // intensity level
                decodedMessageMediator.intensityLevel = int.Parse(match.Groups["intensityLevel"].Value.Trim());
                // toy pattern state
                decodedMessageMediator.isUsingPatternsAllowed = match.Groups["toyPatternState"].Value.Trim() == "The vibrator was able to execute set patterns" ? true : false;
                // toybox lock state
                decodedMessageMediator.isToyboxLockingAllowed = match.Groups["toyboxLockState"].Value.Trim() == "with the viberator strapped tight to their skin" ? true : false;
                // toy active state
                decodedMessageMediator.toyState = match.Groups["toyActiveState"].Value.Trim() == "Left on to buzz away" ? true : false;
                // debug result
                GagSpeak.Log.Debug($"[Message Decoder]: share info4: (Type) {decodedMessageMediator.encodedCmdType} || (ToggleToyState) {decodedMessageMediator.isChangingToyStateAllowed} || "+
                $"(CanControlIntensity) {decodedMessageMediator.isIntensityControlAllowed} || (IntensityLevel) {decodedMessageMediator.intensityLevel} || "+
                $"(ToyPatternState) {decodedMessageMediator.isUsingPatternsAllowed} || (ToyboxLockState) {decodedMessageMediator.isToyboxLockingAllowed}" 
                + $" || (ToyActiveState) {decodedMessageMediator.toyState}");
            } else {
                GagSpeak.Log.Error($"[Message Decoder]: share info4: Failed to decode message: {recievedMessage}");
            }
        
        
        }
    }
}
