using System;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace GagSpeak.Hardcore.ClickSelection.BaseClickLogic;

/// <summary> Click base class. </summary>
public abstract unsafe class ClickBase<TImpl> where TImpl : class
{
    public ClickBase(string name, IntPtr addon) {
        this.AddonName = name;
        if (addon == default)
            addon = this.GetAddonByName(this.AddonName);

        this.AddonAddress = addon;
        this.UnitBase = (AtkUnitBase*)addon;
    }

    protected string AddonName { get; init; }
    protected IntPtr AddonAddress { get; init; }
    protected AtkUnitBase* UnitBase { get; }

    public static implicit operator TImpl(ClickBase<TImpl> cb) => (cb as TImpl)!;

    private IntPtr GetAddonByName(string name, int index = 1) {
        var atkStage = AtkStage.GetSingleton();
        if (atkStage == null)
            throw new Exception("AtkStage is not available");

        var unitMgr = atkStage->RaptureAtkUnitManager;
        if (unitMgr == null)
            throw new Exception("UnitMgr is not available");

        var addon = unitMgr->GetAddonByName(name, index);
        if (addon == null)
            throw new Exception("Addon is not available");

        return (IntPtr)addon;
    }
}
