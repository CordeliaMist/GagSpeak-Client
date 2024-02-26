using GagSpeak.Events;
using GagSpeak.Wardrobe;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
using System;
using Dalamud.Plugin.Services;
using System.Text;

// taken from SimpleTweaksPlugin/Utility/Common.cs
// controls the state of the UI
namespace GagSpeak.Hardcore;
public unsafe class HotbarLocker
{
    private readonly IGameGui _gameGui;
    private readonly RestraintSetManager _restraintSetManager;
    private readonly RS_ToggleEvent _rsToggleEvent;
    public HotbarLocker(RestraintSetManager restraintSetManager, RS_ToggleEvent rsToggleEvent, IGameGui gameGui) {
        _restraintSetManager = restraintSetManager;
        _rsToggleEvent = rsToggleEvent;
        _gameGui = gameGui;
    }
    public void SetHotbarLockState(bool state) {
        // set the lock state
        var actionBar = GetUnitBase("_ActionBar");
        if (actionBar == null) return;
        GenerateCallback(actionBar, 8, 3, 51u, 0u, state);
        // set the lock visibility
        var lockNode = actionBar->GetNodeById(21);
        if (lockNode == null) return;
        var lockComponentNode = lockNode->GetAsAtkComponentNode();
        if (lockComponentNode == null) return;
        lockComponentNode->AtkResNode.ToggleVisibility(!state);
    }

    private static void GenerateCallback(AtkUnitBase* unitBase, params object[] values) {
        var atkValues = CreateAtkValueArray(values);
        if (atkValues == null) return;
        try {
            unitBase->FireCallback(values.Length, atkValues);
        } finally {
            for (var i = 0; i < values.Length; i++) {
                if (atkValues[i].Type == FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String) {
                    Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                }
            }
            Marshal.FreeHGlobal(new IntPtr(atkValues));
        }
    }

    private AtkUnitBase* GetUnitBase(string name, int index = 1) {
        return (AtkUnitBase*)_gameGui.GetAddonByName(name, index);
    }

    private static AtkValue* CreateAtkValueArray(params object[] values) {
        var atkValues = (AtkValue*) Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
        if (atkValues == null) return null;
        try {
            for (var i = 0; i < values.Length; i++) {
                var v = values[i];
                switch (v) {
                    case uint uintValue:
                        atkValues[i].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt;
                        atkValues[i].UInt = uintValue;
                        break;
                    case int intValue:
                        atkValues[i].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int;
                        atkValues[i].Int = intValue;
                        break;
                    case float floatValue:
                        atkValues[i].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Float;
                        atkValues[i].Float = floatValue;
                        break;
                    case bool boolValue:
                        atkValues[i].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Bool;
                        atkValues[i].Byte = (byte) (boolValue ? 1 : 0);
                        break;
                    case string stringValue: {
                        atkValues[i].Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String;
                        var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                        var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                        Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                        Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                        atkValues[i].String = (byte*)stringAlloc;
                        break;
                    }
                    default:
                        throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                }
            }
        } catch {
            return null;
        }
        return atkValues;
    }
}
