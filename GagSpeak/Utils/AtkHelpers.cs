using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Runtime.InteropServices;
using System;
using Dalamud.Plugin.Services;
using System.Text;
using FFXIVClientStructs.Attributes;
using System.Reflection;
using System.Linq;

namespace GagSpeak.Utils;

public unsafe class AtkHelpers {
    private IGameGui _gameGui;
    public AtkHelpers(IGameGui gameGui) {
        _gameGui = gameGui;
    }
    
    public static void GenerateCallback(AtkUnitBase* unitBase, params object[] values) {
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

    public AtkUnitBase* GetUnitBase(string name, int index = 1) {
        return (AtkUnitBase*)_gameGui.GetAddonByName(name, index);
    }

    public T* GetUnitBase<T>(string name = null!, int index = 1) where T : unmanaged {
        if (string.IsNullOrEmpty(name)) {
            var attr = (Addon) typeof(T).GetCustomAttribute(typeof(Addon))!;
            if (attr != null) {
                name = attr.AddonIdentifiers.FirstOrDefault()!;
            }
        }

        if (string.IsNullOrEmpty(name)) return null;
        return (T*) _gameGui.GetAddonByName(name, index);
    }

    public bool GetUnitBase<T>(out T* unitBase, string name=null!, int index = 1) where T : unmanaged {
        unitBase = null;
        if (string.IsNullOrEmpty(name)) {
            var attr = (Addon) typeof(T).GetCustomAttribute(typeof(Addon))!;
            if (attr != null) {
                name = attr.AddonIdentifiers.FirstOrDefault()!;
            }
        }

        if (string.IsNullOrEmpty(name)) return false;
            
        unitBase = (T*) _gameGui.GetAddonByName(name, index);
        return unitBase != null;
    }

    public static AtkValue* CreateAtkValueArray(params object[] values) {
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