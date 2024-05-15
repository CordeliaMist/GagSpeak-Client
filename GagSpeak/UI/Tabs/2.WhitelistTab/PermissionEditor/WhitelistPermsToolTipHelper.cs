using System;
using System.Collections.Generic;
using GagSpeak.Utility;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {
    private Dictionary<string, Func<string>> tooltips;

    private void InitializeToolTips() {
        // temp name storage
        string tempPlayerName = AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess);
    
        tooltips = new Dictionary<string, Func<string>>
        {
            // general settings
            ["usedSafewordTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has recently used their safeword, and their grace period is still active."
                : "If you have already used your safeword and are on cooldown until you can use GagSpeak features again.",
            ["hardcoreModeTT"] = () => $"Indicates if {tempPlayerName.Split(' ')[0]} is in hardcore mode or not",
            ["ExtendedLockTimesTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"if {tempPlayerName.Split(' ')[0]} is allowing you to lock their padlocks for longer than 12 hours."
                : $"If you are allowing {tempPlayerName.Split(' ')[0]} to lock your padlocks for longer than 12 hours.",
            ["LiveChatGarblerTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} currently has their direct chat garbler enabled.\n"+
                "If this is enabled, it means anything they say in their enabled channels are converted to GagSpeak while gagged."
                : "If you currently have your direct chat garbler enabled.",
            ["LiveChatGarblerLockTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} currently has their direct chat garbler locked.\n"+
                "If they have this locked, it means nobody can disable their direct chat garbler except for the person who locked it."
                : "If you currently have your direct chat garbler locked.",
            ["WardrobeEnabledTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has checked [Enable Wardrobe] in their config settings."
                : "If you have checked off [Enable Wardrobe] in your config settings.",
            ["PuppeteerEnabledTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has checked [Enable Puppeteer] in their config settings."
                : "If you have checked off [Enable Puppeteer] in your config settings.",
            ["ToyboxEnabledTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has checked [Enable Toybox] in their config settings."
                : "If you have checked off [Enable Toybox] in your config settings.",
            // gag interactions
            ["GagLayerSelectionTT"] = () => "Selects Which Gag Layer you are trying to interact with",
            ["GagTypeSelectionTT"] = () => $"Used to pick which type of gag you want to apply to {tempPlayerName.Split(' ')[0]}",
            ["GagPadlockSelectionTT"] = () => $"Used to pick which padlock type you want to apply to {tempPlayerName.Split(' ')[0]}.\n"+
            $"To Unlock one of {tempPlayerName.Split(' ')[0]}'s padlocks, you must have the current padlock locked onto them selected.\n"+
            "(EX. If they currently have a combination padlock locked onto them, you need to have the combination padlock selected to unlock it.)",
            ["ApplyGagTT"] = () => $"Applies the selected gag to {tempPlayerName.Split(' ')[0]}",
            ["ApplyPadlockTT"] = () => $"Applies the selected padlock to {tempPlayerName.Split(' ')[0]}",
            ["UnlockPadlockTT"] = () => $"Unlocks the selected padlock from {tempPlayerName.Split(' ')[0]}",
            ["RemoveGagTT"] = () => $"Removes the selected gag from {tempPlayerName.Split(' ')[0]}",
            ["RemoveAllGagsTT"] = () => $"Removes all gags from {tempPlayerName.Split(' ')[0]}",
            // wardrobe tooltips
            ["LockGagStorageTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]}'s GagStorage UI becomes locked when they are gagged."
                : "If your GagStorage UI becomes locked when you are gagged.",
            ["AllowTogglingRestraintSetsTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has allowed you to toggle their restraint sets."
                : $"If wish to allow {tempPlayerName.Split(' ')[0]} to toggle your restraint sets.",
            ["AllowLockingRestraintSetsTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has allowed you to lock their restraint sets."
                : $"If wish to allowing {tempPlayerName.Split(' ')[0]} to lock your restraint sets.",
            ["ToggleSetTT"] = () => $"If the textfield contains a valid restraint set in {tempPlayerName.Split(' ')[0]}'s restraint set list, you can toggle it on and off.",
            ["LockSetTT"] = () => $"If the textfield contains a valid timer format, and a valid set is in the text field above,\n"+
                                  $"you can lock {tempPlayerName.Split(' ')[0]}'s active restraint set.",
            ["UnlockSetTT"] = () => $"If this textfield contains a valid restraint set, you can attempt to unlock {tempPlayerName.Split(' ')[0]}'s locked restraint set.",
            ["StoredSetListTT"] = () => $"Contains the full list of {tempPlayerName.Split(' ')[0]}'s active restraint sets.\n"+
                $"To get this list of restraint sets, you will need to have {tempPlayerName.Split(' ')[0]} send you their copied list,\n"+
                "Then click the handcuff icon on the bottom left of the whitelist tab to paste the set list in.",
            // puppetter tooltips
            ["AllowSitPermTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} allows you to make them execute /sit and /groundsit commands using their trigger phrase"
                : $"If you are giving {tempPlayerName.Split(' ')[0]} access to make you execute /sit and /groundsit commands with your trigger phrase.",
            ["AllowMotionPermTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} allows you to force them to do emotes and expressions with their trigger phrase."
                : $"If you are giving {tempPlayerName.Split(' ')[0]} access to make you execute emotes and expressions with your trigger phrase.",
            ["AllowAllCommandsPermTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} allows you to make them execute any command."
                : $"If you are giving {tempPlayerName.Split(' ')[0]} access to make you execute any command with your trigger phrase.",
            ["TriggerPhraseTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"The Phrase that {tempPlayerName.Split(' ')[0]} has set for you.\n"+
                  $"If you say this trigger phrase in chat, they will execute everything after it in the message.\n"+
                  $"Optionally, you can surround the command after the trigger in their start & end chars."
                : $"The Trigger Phrase that you have set for {tempPlayerName.Split(' ')[0]}.\n"+
                  $"If {tempPlayerName.Split(' ')[0]} says this in chat in any enabled channels,\n"+
                  $"you will execute whatever comes after the trigger phrase,\n(or what is enclosed within the start and end brackets)",
            ["StartCharTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"The Start Character that {tempPlayerName.Split(' ')[0]} has defined as the left enclosing bracket character.\n"+
                  $"Replaces the [ ( ] in Ex: [ TriggerPhrase (commandToExecute) ]"
                : $"The Start Character that you have defined as the left enclosing bracket character for your trigger phrase.\n"+
                  "Replaces the [ ( ] in Ex: [ TriggerPhrase (commandToExecute) ]",
            ["EndCharTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"The End Character that {tempPlayerName.Split(' ')[0]} has defined as the right enclosing bracket character.\n"+
                  $"Replaces the [ ) ] in Ex: [ TriggerPhrase (commandToExecute) ]"
                : $"The End Character that you have defined as the right enclosing bracket character for your trigger phrase.\n"+
                  "Replaces the [ ) ] in Ex: [ TriggerPhrase (commandToExecute) ]",
            ["AliasInputTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"When you say this input command, {tempPlayerName.Split(' ')[0]} will take it in as an alias and replace it with the output command."
                : $"When {tempPlayerName.Split(' ')[0]} says this as a part of the command for you to execute after your trigger phrase,\n"+
                  "You will replace it with the alias output command before executing it.",
            ["AliasOutputTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"What {tempPlayerName.Split(' ')[0]} will replace with the corrisponding input command if it is included in the command to execute"
                : $"What you will replace the alias input command with if it is included in the command to execute after your trigger phrase.",
            // toybox tooltips
            ["ToyboxStateTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has allowed you to lock their toybox UI."
                : $"If you are giving {tempPlayerName.Split(' ')[0]} access to lock your toybox UI if they please.",
            ["AllowChangeToyStateTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has allowed you to change their actively connected toys state."
                : $"If you are allowing {tempPlayerName.Split(' ')[0]} to change the state of your actively connected toys state.",
            ["CanControlIntensityTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has allowed you to control the intensity of their actively connected toy."
                : $"If you are allowing {tempPlayerName.Split(' ')[0]} to control the intensity of your actively connected toy.",
            ["CanExecutePatternsTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"If {tempPlayerName.Split(' ')[0]} has allowed you to execute patterns to their actively connected toy."
                : $"If you are allowing {tempPlayerName.Split(' ')[0]} to execute patterns to your actively connected toy.",
            ["IntensityMeterTT"] = () => $"The current intensity of {tempPlayerName.Split(' ')[0]}'s actively connected toy.\n"+
                "This scale is based on their stepsize.",
            ["PatternTT"] = () => $"The current pattern that you are going to make {tempPlayerName.Split(' ')[0]}'s actively connected toy execute.",
            ["PatternListTT"] = () => $"The list of patterns that {tempPlayerName.Split(' ')[0]}'s actively connected toy has available to execute.",
            // hardcore tooltips
            ["FollowOrderTT"] = () => $"If the [can toggle] permission is granted, saying \"{tempPlayerName.Split(' ')[0]}, follow me.\"\n"+
            $"anywhere in your message in any channel will force {tempPlayerName.Split(' ')[0]} to follow you. There movement will be blocked \n"+
            $"while following, only allowing them to move again if they remain still for 6 full seconds",
            ["SitOrderTT"] = () => $"If the [can toggle] permission is granted, saying \"{tempPlayerName.Split(' ')[0]}, sit.\"\n"+
            $"anywhere in your message in any channel will force {tempPlayerName.Split(' ')[0]} to sit. There movement will be blocked \n"+
            $"until you say \"you may stand now {tempPlayerName.Split(' ')[0]}\" anywhere in your message in any channel.",
            ["LockAwayTT"] = () => $"If the [can toggle] permission is granted, saying \"{tempPlayerName.Split(' ')[0]}, stay here until i return.\"\n"+
            $"will prevent them from teleporting away, using return, or leaving estates or private chambers, along with anything else they have added.\n"+
            $"They will be able to move again if you say \"thank you for waiting, {tempPlayerName.Split(' ')[0]}\"",
            ["BlindfoldTT"] = () => $"If the [can toggle] permission is granted, you will be able to toggle on and off {tempPlayerName.Split(' ')[0]}'s blindfold.",
            // general tooltips
            ["CurrentStateTT"] = () => $"If the Permission is allowed / not allowed",
            ["ReqTierTT"] = () => _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"The required tier you must be in order to toggle {tempPlayerName.Split(' ')[0]}'s permission for this setting."
                : $"Required tier {tempPlayerName.Split(' ')[0]} needs to have to override your setting for this permission.\n"+
                "You are able to toggle this regardless of tier, because you are configuring your own permissions.",
            ["ToggleButtonTT"] = () =>  _activePanelTab == WhitelistPanelTab.TheirSettings
                ? $"Toggle {tempPlayerName.Split(' ')[0]}'s permission"
                : $"Toggle this permission, switching its state.\n"+
                  $"If state is checked, {tempPlayerName.Split(' ')[0]} has access to it, if it is X, they do not.",
        }; 
    }
}