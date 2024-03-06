using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using GagSpeak.Hardcore.BaseListener;
using GagSpeak.Hardcore.ClickSelection;
using GagSpeak.Utility;

namespace GagSpeak.Hardcore;

public class OptionPromptListeners : OnSetupSelectListFeature, IDisposable
{
    private readonly ITargetManager         _targetManager;
    private readonly IGameInteropProvider   _gameInteropProvider;
    private readonly HardcoreManager        _hcManager;
    private readonly IAddonLifecycle        _addonLifecycle;
    private          bool                   DisablePromptHooks = false;
    public OptionPromptListeners(ITargetManager targetManager, IGameInteropProvider gameInteropProvider, HardcoreManager hardcoreManager,
    IAddonLifecycle addonLifecycle) : base(targetManager, gameInteropProvider, hardcoreManager) {
        _addonLifecycle = addonLifecycle;
        _targetManager = targetManager;
        _gameInteropProvider = gameInteropProvider;
        _hcManager = hardcoreManager;
        DisablePromptHooks = false;
        Enable();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Call the base Dispose method
            base.Dispose(disposing);
            // Add any additional dispose logic for OptionPromptListeners here
            Disable();
        }
    }
 
    public override void Enable() {
        // if our disable prompt hooks was marked as disabled, then we should enable it!
        if(!DisablePromptHooks)
        {
            base.Enable();
            GSLogger.LogType.Information("[GagSpeak] Activating Listeners");
            _addonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectString", AddonStrSetup);
            _addonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", AddonYNSetup);
            DisablePromptHooks = true; // set it to true so we dont keep repeating it
        }
    }

    public override void Disable() {
        // if our disable prompt hooks was marked as enabled, then we should disable it!
        if(DisablePromptHooks) {
            base.Disable();
            GSLogger.LogType.Information("[GagSpeak] Deactivating Listeners");
            _addonLifecycle.UnregisterListener(AddonStrSetup);
            _addonLifecycle.UnregisterListener(AddonYNSetup);
            DisablePromptHooks = false; // set it to false so we dont keep repeating it
        }
    }

    protected unsafe void AddonYNSetup(AddonEvent eventType, AddonArgs addonInfo) {
        var addon = (AtkUnitBase*)addonInfo.Addon;
        // pointerbase for yesno
        var dataPtr = (AddonSelectYesNoOnSetupData*)addon;
        if (dataPtr == null)
            return;   
        // get the text
        var text = GS_GetSeString.GetSeStringText(new nint(addon->AtkValues[0].String));
        _hcManager.LastSeenDialogText = Tuple.Create(text, new List<string>{ "No", "Yes" });
        GSLogger.LogType.Debug($"[GagSpeak] YesNo Prompt Text => {text}");

        var nodes = _hcManager.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes) {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!EntryMatchesText(node, text))
                continue;

            GSLogger.LogType.Debug($"AddonSelectYesNo: Matched on {node.Text}");
            // if nobody is making us stay, then just escape and dont process it
            if (!_hcManager.IsForcedToStayForAny(out int enabledIdx, out string playerWhoForceStayedYou)) {
                return;
            }
            // otherwise, process it
            AddonSelectYesNoExecute((nint)addon, node.SelectThisIndex);
            return;
        }
    }

    private unsafe void AddonSelectYesNoExecute(IntPtr addon, int SelectThisIndex) {
        if (SelectThisIndex==1) {
            var addonPtr = (AddonSelectYesno*)addon;
            var yesButton = addonPtr->YesButton;
            if (yesButton != null && !yesButton->IsEnabled) {
                GSLogger.LogType.Debug("Auto-Select YesNo: Enabling yes button");
                var flagsPtr = (ushort*)&yesButton->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags;
                *flagsPtr ^= 1 << 5;
            }

            GSLogger.LogType.Debug("Auto-Select YesNo: Selecting yes");
            ClickSelectYesNo.Using(addon).Yes();
        }
        else {
            GSLogger.LogType.Debug("Auto-Select YesNo: Selecting no");
            ClickSelectYesNo.Using(addon).No();
        }
    }

    private static bool EntryMatchesText(TextEntryNode node, string text) {
        GSLogger.LogType.Debug($"Comparing {node.Text} to {text}"); // for YesNo prompts, this is all we need
        return text.Contains(node.Text);
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData {
        [FieldOffset(0x8)]
        public IntPtr TextPtr;
    }

    // -----------------------------------------------------------------------------------------------
    protected unsafe void AddonStrSetup(AddonEvent eventType, AddonArgs addonInfo) {
        // get the unit base for our addon
        var addon = (AtkUnitBase*)addonInfo.Addon;
        // get the addonSelectstring pointer
        var addonPtr = (AddonSelectString*)addon;
        // get the popup menu pointer
        var popupMenu = &addonPtr->PopupMenu.PopupMenu;
        // setup
        SetupOnItemSelectedHook(popupMenu);
        // grab the options
        var options = GetEntryTexts(popupMenu).Select(option => option ?? string.Empty).ToList();

        var target = _targetManager.Target;
        var targetName = target != null ? GS_GetSeString.GetSeStringText(target.Name) : string.Empty;
        // create the tuple for the dialog options
        _hcManager.LastSeenDialogText = Tuple.Create(targetName, options);
        // get all current nodes and see if it matches with any
        var nodes = _hcManager.GetAllNodes().OfType<TextEntryNode>();
        // iterate through all our nodes to see if their node.text matches the targetName
        foreach (var node in nodes) {
            GSLogger.LogType.Verbose($"Auto-Select Dialog: Checking {node.Text} of size {node.Options.Length} against {targetName} of size {options.Count}");
            // if the node is not enabled or the text is empty, then skip it
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            // node.text must match targetName, and both must have the same size, if either fail, skip
            if(!(node.Text == targetName && options.Count == node.Options.Length))
                continue;

            // we reached here, so we know this is the node we are looking for,
            // if we fail to match with the option at the index we want to select, continue
            if (!EntryMatchesListText(node, options))
                continue;

            // if it does match, then select the index automatically
            GSLogger.LogType.Information($"Auto-Select Dialog Matched!: Matched on {node.Text}");
            // if nobody is making us stay, then just escape and dont process it
            if (!_hcManager.IsForcedToStayForAny(out int enabledIdx, out string playerWhoForceStayedYou)) {
                return;
            }
            SelectItemExecute((IntPtr)addon, node.SelectThisIndex);
            return;
        }
        GSLogger.LogType.Information($"Node Check Finished");
        GSLogger.LogType.Information($"StoredInfo: {_hcManager.LastSeenDialogText.Item1} => {string.Join(", ", _hcManager.LastSeenDialogText.Item2)}");
        // CompareNodesToEntryTexts((nint)addon, popupMenu);
    }

    private static bool EntryMatchesListText(TextEntryNode node, List<string> targetNodeOptions) {
        // try to see if the option at our index to select is at the same index in the list of the target nodes options
        GSLogger.LogType.Verbose($"Checking if [{node.Options[node.SelectThisIndex]}] is in  any of the about found strings");
        // Compare the option at our index to select with the same index in the list of the target nodes options
        return node.Options[node.SelectThisIndex] == targetNodeOptions[node.SelectThisIndex];
    }
    
    protected override void SelectItemExecute(IntPtr addon, int index) {
        GSLogger.LogType.Information($"Auto-Select Dialog: Selecting {index}");
        ClickSelectString.Using(addon).SelectItem((ushort)index);
    }
}
