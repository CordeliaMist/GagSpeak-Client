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
    private readonly ITargetManager _targetManager;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly HardcoreManager _hcManager;
    private readonly IAddonLifecycle _addonLifecycle;
    public OptionPromptListeners(ITargetManager targetManager, IGameInteropProvider gameInteropProvider, HardcoreManager hardcoreManager,
    IAddonLifecycle addonLifecycle) : base(targetManager, gameInteropProvider, hardcoreManager) {
        _addonLifecycle = addonLifecycle;
        _targetManager = targetManager;
        _gameInteropProvider = gameInteropProvider;
        _hcManager = hardcoreManager;
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
        base.Enable();
        GSLogger.LogType.Debug("[GagSpeak] Activating Listeners");
        _addonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectString", AddonStrSetup);
        _addonLifecycle.RegisterListener(AddonEvent.PostSetup, "SelectYesno", AddonYNSetup);
    }

    public override void Disable() {
        base.Disable();
        _addonLifecycle.UnregisterListener(AddonYNSetup);
        _addonLifecycle.UnregisterListener(AddonStrSetup);
    }

    protected unsafe void AddonYNSetup(AddonEvent eventType, AddonArgs addonInfo) {
        var addon = (AtkUnitBase*)addonInfo.Addon;
        // FIX THIS
        if (!_hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedToStay) {
            return;
        }

        var dataPtr = (AddonSelectYesNoOnSetupData*)addon;
        if (dataPtr == null)
            return;   

        var text = GS_GetSeString.GetSeStringText(new nint(addon->AtkValues[0].String));
        _hcManager.LastSeenDialogText = Tuple.Create(text, new List<string>{ "Yes", "No" });
        GSLogger.LogType.Debug($"[GagSpeak] YesNo Prompt Text => {text}");

        var nodes = _hcManager.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                continue;

            if (!EntryMatchesText(node, text))
                continue;

            GSLogger.LogType.Debug($"AddonSelectYesNo: Matched on {node.Text}");
            AddonSelectYesNoExecute((nint)addon, node.SelectThisIndex);
            return;
        }
    }

    private unsafe void AddonSelectYesNoExecute(IntPtr addon, int SelectThisIndex)
    {
        if (SelectThisIndex==0)
        {
            var addonPtr = (AddonSelectYesno*)addon;
            var yesButton = addonPtr->YesButton;
            if (yesButton != null && !yesButton->IsEnabled)
            {
                GSLogger.LogType.Debug("AddonSelectYesNo: Enabling yes button");
                var flagsPtr = (ushort*)&yesButton->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags;
                *flagsPtr ^= 1 << 5;
            }

            GSLogger.LogType.Debug("AddonSelectYesNo: Selecting yes");
            ClickSelectYesNo.Using(addon).Yes();
        }
        else
        {
            GSLogger.LogType.Debug("AddonSelectYesNo: Selecting no");
            ClickSelectYesNo.Using(addon).No();
        }
    }

    private static bool EntryMatchesText(TextEntryNode node, string text)
    {
        GSLogger.LogType.Debug($"AddonSelectYesNo: Comparing {node.Text} to {text}");
        return text.Contains(node.Text);
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData
    {
        [FieldOffset(0x8)]
        public IntPtr TextPtr;
    }
    // -----------------------------------------------------------------------------------------------
    protected unsafe void AddonStrSetup(AddonEvent eventType, AddonArgs addonInfo)
    {
        var addon = (AtkUnitBase*)addonInfo.Addon;

        if (!_hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedToStay)
            return;

        var addonPtr = (AddonSelectString*)addon;
        var popupMenu = &addonPtr->PopupMenu.PopupMenu;
        GSLogger.LogType.Debug($"AddonSelectString: {addonPtr->PopupMenu.PopupMenu.EntryCount}");
        SetupOnItemSelectedHook(popupMenu);
        var options = GetEntryTexts(popupMenu).Select(option => option ?? string.Empty).ToList();

        var text = options[0] ?? string.Empty;
        var text2 = GS_GetSeString.GetSeStringText(new nint(addon->AtkValues[0].String)) ?? string.Empty;
        _hcManager.LastSeenDialogText = Tuple.Create(text2, options);

        var nodes = _hcManager.GetAllNodes().OfType<TextEntryNode>();
        for (int i = 0; i < options.Count; i++)
        {
            foreach (var node in nodes)
            {
                if (!node.Enabled || string.IsNullOrEmpty(node.Text))
                    continue;

                if (!EntryMatchesText(node, options[i]))
                    continue;

                GSLogger.LogType.Debug($"AddonSelectString: Matched on {node.Text}");
                SelectItemExecute((IntPtr)addon, node.SelectThisIndex);
                return;
            }
        }
        CompareNodesToEntryTexts((nint)addon, popupMenu);
    }
    
    protected override void SelectItemExecute(IntPtr addon, int index) {
        GSLogger.LogType.Debug($"AddonSelectString: Selecting {index}");
        ClickSelectString.Using(addon).SelectItem((ushort)index);
    }
}
