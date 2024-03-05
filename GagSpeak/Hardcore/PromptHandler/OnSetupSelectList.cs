using System;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using GagSpeak.Utility;

namespace GagSpeak.Hardcore.BaseListener;

public abstract class OnSetupSelectListFeature : BaseFeature, IDisposable
{
    private Hook<OnItemSelectedDelegate>? onItemSelectedHook = null;
    private readonly ITargetManager _targetManager;
    private readonly IGameInteropProvider _gameInteropProvider;
    private readonly HardcoreManager _hcManager;
    protected OnSetupSelectListFeature(ITargetManager targetManager, 
    IGameInteropProvider gameInteropProvider, HardcoreManager hardcoreManager) {
        _targetManager = targetManager;
        _gameInteropProvider = gameInteropProvider;
        _hcManager = hardcoreManager;
    }

    private delegate byte OnItemSelectedDelegate(IntPtr popupMenu, uint index, IntPtr a3, IntPtr a4);

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            GSLogger.LogType.Debug("OnSetupSelectListFeature: Dispose");
            this.onItemSelectedHook?.Disable();
            this.onItemSelectedHook?.Dispose();
        }
    }

    protected unsafe void CompareNodesToEntryTexts(IntPtr addon, PopupMenu* popupMenu) {
        GSLogger.LogType.Debug("CompareNodesToEntryTexts");
        var target = _targetManager.Target;
        var targetName = target != null
            ? GS_GetSeString.GetSeStringText(target.Name)
            : string.Empty;

        var texts = this.GetEntryTexts(popupMenu);
    }

    protected abstract void SelectItemExecute(IntPtr addon, int index);

    protected unsafe void SetupOnItemSelectedHook(PopupMenu* popupMenu) {
        if (this.onItemSelectedHook != null)
            return;

        var onItemSelectedAddress = (IntPtr)popupMenu->AtkEventListener.vfunc[3];
        this.onItemSelectedHook = _gameInteropProvider.HookFromAddress<OnItemSelectedDelegate>(onItemSelectedAddress, this.OnItemSelectedDetour);
        this.onItemSelectedHook.Enable();
    }

    private unsafe byte OnItemSelectedDetour(IntPtr popupMenu, uint index, IntPtr a3, IntPtr a4) {
        if (popupMenu == IntPtr.Zero)
            return this.onItemSelectedHook!.Original(popupMenu, index, a3, a4);

        try {
            var popupMenuPtr = (PopupMenu*)popupMenu;
            if (index < popupMenuPtr->EntryCount) {
                var entryPtr = popupMenuPtr->EntryNames[index];
                var entryText = _hcManager.LastSeenListSelection = entryPtr != null
                    ? GS_GetSeString.GetSeStringText(entryPtr)
                    : string.Empty;

                var target = _targetManager.Target;
                var targetName = _hcManager.LastSeenListTarget = target != null
                    ? GS_GetSeString.GetSeStringText(target.Name)
                    : string.Empty;

                GSLogger.LogType.Debug($"ItemSelected: target={targetName} text={entryText}");
            }
        } catch (Exception ex) {
            GSLogger.LogType.Error($"Don't crash the game, please: {ex}");
        }
        return this.onItemSelectedHook!.Original(popupMenu, index, a3, a4);
    }

    public unsafe string?[] GetEntryTexts(PopupMenu* popupMenu) {
        var count = popupMenu->EntryCount;
        var entryTexts = new string?[count];

        GSLogger.LogType.Debug($"SelectString: Reading {count} strings");
        for (var i = 0; i < count; i++)
        {
            var textPtr = popupMenu->EntryNames[i];
            entryTexts[i] = textPtr != null
                ? GS_GetSeString.GetSeStringText(textPtr)
                : null;

            // Print out the string it finds
            if (entryTexts[i] != null)
            {
                GSLogger.LogType.Debug($"Found string: {entryTexts[i]}");
            }

        }

        return entryTexts;
    }
}
