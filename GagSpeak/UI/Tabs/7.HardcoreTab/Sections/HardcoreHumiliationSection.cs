using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using ImGuiNET;
using OtterGui;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin.Services;
using GagSpeak.Hardcore.Actions;
using JetBrains.Annotations;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.Utility;
using System.Runtime.InteropServices;
namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_Humiliation
{
    private readonly HardcoreManager _hardcoreManager;
    private readonly GsActionManager _actionManager;
    private readonly IDataManager _data;
    private readonly IconManager _iconManager;
    public HC_Humiliation(HardcoreManager hardcoreManager, IDataManager data,
    GsActionManager actionManager, IconManager iconManager) {
        _hardcoreManager = hardcoreManager;
        _data = data;
        _actionManager = actionManager;
        _iconManager = iconManager;
    }

    private int selectedSavedIndex = 0;
    public enum HotBarType { Normal, Cross, }

    private unsafe void DrawHotbarType(RaptureHotbarModule* hotbarModule, HotBarType type) {
        var isNormalBar = type == HotBarType.Normal;
        var baseSpan = isNormalBar ? hotbarModule->StandardHotBars : hotbarModule->CrossHotBars;
        
        if (ImGui.BeginTabBar("##hotbarTabs")) {
            for (var i = 0; i < baseSpan.Length; i++) {
                if (ImGui.BeginTabItem($"{i+1:00}##hotbar{i}")) {
                    var hotbar = baseSpan.GetPointer(i);
                    if (hotbar != null) {
                        DrawHotbar(hotbarModule, hotbar);
                    }                  
                    ImGui.EndTabItem();
                }

            }
            // Pet hotbar is a special case
            if (ImGui.BeginTabItem("Pet##hotbarex")) {
                
                var petBar = isNormalBar ? &hotbarModule->PetHotBar : &hotbarModule->PetCrossHotBar;
                DrawHotbar(hotbarModule, petBar);                
                ImGui.EndTabItem();
            }
            // New tab for hotbarSkills
            if (ImGui.BeginTabItem("Hotbar Skills##hotbarskills")) {
                ImGui.Columns(10, "##hotbarskillscolumns", true);
                for (var i = 0; i < _actionManager.hotbarSkills.Length; i++) {
                    ImGui.Text($"{_actionManager.hotbarSkills[i]}");
                    ImGui.NextColumn();
                }
                ImGui.Columns(1);
                ImGui.EndTabItem();
            }

            // new tab for drawing all icons
            if (ImGui.BeginTabItem("All Icons##allicons")) {
                ImGui.Columns(10, "##allicons", true);
                for (var i = 0; i < 1000; i++) {
                    var iconGood = false;
                    Dalamud.Interface.Internal.IDalamudTextureWrap? icon = null;
                    try{
                        icon = _iconManager.GetIconTexture(i % 1000000, i >= 1000000);
                        if (icon != null) {
                            ImGui.Image(icon.ImGuiHandle, new Vector2(32));
                            iconGood = true;
                        }
                    } catch (System.Exception e) {
                        GagSpeak.Log.Error($"{e}");
                    }
                    if (!iconGood) {
                        ImGui.GetWindowDrawList().AddRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(32), 0xFF0000FF, 4);
                        ImGui.GetWindowDrawList().AddText(ImGui.GetCursorScreenPos(), 0xFFFFFFFF, $"{i}");
                        
                        ImGui.Dummy(new Vector2(32));
                    }
                    ImGui.NextColumn();
                }
                ImGui.Columns(1);
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
    }

    private unsafe void DrawHotbar(RaptureHotbarModule* hotbarModule, HotBar* hotbar) {
        try{
        using var tableBorderLight = ImRaii.PushColor(ImGuiCol.TableBorderLight, ImGui.GetColorU32(ImGuiCol.Border));
        using var tableBorderStrong = ImRaii.PushColor(ImGuiCol.TableBorderStrong, ImGui.GetColorU32(ImGuiCol.Border));
        if (!ImGui.BeginTable("HotbarTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable)) return;
        
        ImGui.TableSetupColumn("##", ImGuiTableColumnFlags.WidthFixed, 30);
        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 150);
        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 180);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 180);
        ImGui.TableSetupColumn("Cooldown", ImGuiTableColumnFlags.WidthFixed, 180);
        ImGui.TableSetupColumn("Struct", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();
        
        for (var i = 0; i < 16; i++) {
            var slot = hotbar->SlotsSpan.GetPointer(i);
            if (slot == null) break;
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (slot->CommandType == HotbarSlotType.Empty) {
                ImGui.PushStyleColor(ImGuiCol.Text, slot->CommandType == HotbarSlotType.Empty ? 0x99999999 : 0xFFFFFFFF);
                //DebugManager.ClickToCopyText($"{i+1:00}", $"{(ulong)slot:X}");
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(1, ImGui.GetTextLineHeight() * 4));
                ImGui.TableNextColumn();
                ImGui.Text("Empty");
                ImGui.PopStyleColor();
                continue;
            }
                
            var adjustedId = slot->CommandType == HotbarSlotType.Action ? 
                    FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetAdjustedActionId(slot->CommandId) : slot->CommandId;
            //DebugManager.ClickToCopyText($"{i+1:00}", $"{(ulong)slot:X}");
            ImGui.SameLine();
            ImGui.Dummy(new Vector2(1, ImGui.GetTextLineHeight() * 4));
            ImGui.TableNextColumn();
                
            ImGui.Text($"{slot->CommandType} : {slot->CommandId}");
            if (slot->CommandType == HotbarSlotType.Action) {
                ImGui.Text($"Adjusted: {adjustedId}");
            }

            if (slot->CommandType == HotbarSlotType.Macro) {
                ImGui.Text($"{(slot->CommandId >= 256 ? "Shared" : "Individual")} #{slot->CommandId % 256}");
            }
            
            ImGui.TableNextColumn();

            var iconGood = false;
            if (slot->Icon >= 0) {
                Dalamud.Interface.Internal.IDalamudTextureWrap? icon = null;
                try{
                icon = _iconManager.GetIconTexture(slot->Icon % 1000000, slot->Icon >= 1000000);
                } catch (System.Exception e) {
                    GagSpeak.Log.Error($"{e}");
                }
                if (icon != null) {
                    ImGui.Image(icon.ImGuiHandle, new Vector2(32));
                    iconGood = true;
                }
            }
            if (!iconGood) {
                ImGui.GetWindowDrawList().AddRect(ImGui.GetCursorScreenPos(), ImGui.GetCursorScreenPos() + new Vector2(32), 0xFF0000FF, 4);
                ImGui.GetWindowDrawList().AddText(ImGui.GetCursorScreenPos(), 0xFFFFFFFF, $"{slot->Icon}");
                   
                ImGui.Dummy(new Vector2(32));
            }
            ImGui.SameLine();
                
            ImGui.Text($"A: {slot->IconTypeA}#{slot->IconA}\nB: {slot->IconTypeB}#{slot->IconB}");

            // Column "Name"
            ImGui.TableNextColumn();
            
            var popUpHelp = SeString.Parse(slot->PopUpHelp).ToString();
            if (popUpHelp.IsNullOrEmpty()) {
                ImGui.TextDisabled("Empty PopUpHelp");
            } else {
                ImGui.TextWrapped(popUpHelp);
            }

            if (this.ResolveSlotName(slot->CommandType, slot->CommandId, out var resolvedName)) {
                ImGui.TextWrapped($"Resolved: {resolvedName}");
            } else {
                ImGui.TextDisabled($"Resolved: {resolvedName}");
            }
                
            // Column "Cooldown"
            ImGui.TableNextColumn();

            var cooldownGroup = -1;
                
            switch (slot->CommandType) {
                case HotbarSlotType.Action: {
                    var action = _data.Excel.GetSheet<Action>()!.GetRow(adjustedId);
                    if (action == null) {
                        ImGui.TextDisabled("Not Found");
                        break;
                    }
                    cooldownGroup = action.CooldownGroup;
                    break;
                }
                case HotbarSlotType.Item: {
                    var item = _data.Excel.GetSheet<Item>()!.GetRow(slot->CommandId);
                    if (item == null) {
                        ImGui.TextDisabled("Not Found");
                        break;
                    }
                        
                    var cdg = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroup(2, slot->CommandId);
                    if (cdg < 81) cooldownGroup = cdg + 1;
                        
                    break;
                }
                case HotbarSlotType.GeneralAction: {
                    var action = _data.Excel.GetSheet<GeneralAction>()!.GetRow(slot->CommandId);
                    if (action?.Action == null) {
                        ImGui.TextDisabled("Not Found");
                        break;
                    }

                    cooldownGroup = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroup(5, slot->CommandId);
                    break;
                }
            }

            if (cooldownGroup > 0) {
                    
                ImGui.Text($"Cooldown Group: {cooldownGroup}");

                var cooldown = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroupDetail(cooldownGroup);
                ImGui.Text($"{(ulong)cooldown:X}");
                if (cooldown != null) {
                    ImGui.Text($"{cooldown->IsActive} / {cooldown->Elapsed} / {cooldown->Total}");
                } else {
                    ImGui.Text("Failed");
                }
            }
            
            // Column "Struct"
            ImGui.TableNextColumn();

            switch (slot->CommandType) {
                case HotbarSlotType.Action:
                case HotbarSlotType.GeneralAction:
                case HotbarSlotType.Marker:
                case HotbarSlotType.Item: {
                    
                    ImGui.Text($"IsActive: {slot->IsSlotUsable(slot->IconTypeB, slot->IconB)}");
                    try{
                    ImGui.Text($"{Marshal.ReadByte((System.IntPtr)slot, 204)}");
                    } catch (System.Exception e) {
                        GagSpeak.Log.Error(e.ToString());
                    }
                    // get the current byte address of where it is fetching this from
                    //ImGui.Text($"Address: {(ulong)slot->GetSlotUsableAddress(slot->IconTypeB, slot->IconB):X}");
                }
                break;
            }
            }
        } catch (System.Exception e) {
            GagSpeak.Log.Error(e.ToString());
        } finally {
            ImGui.EndTable();
        }
    }

    public unsafe void Draw() {
        using var child = ImRaii.Child("##HC_Humiliation", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child)
            return;

        ImGuiUtil.Center("whats this? Idk.");
        ImGui.Separator();
        if (ImGui.BeginTabBar("##hotbarDebugDisplay")) {
        try{
            if (ImGui.BeginTabItem("Current Bars")) {
                if (ImGui.BeginTabBar($"###{GetType().Name}_debug_tabs")) {
                    if (ImGui.BeginTabItem("Normal")) {
                        DrawHotbarType(_actionManager.raptureHotarModule, HotBarType.Normal);
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Cross")) {
                        DrawHotbarType(_actionManager.raptureHotarModule, HotBarType.Cross);
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Saved Bars")) {
                var classJobSheet = _data.GetExcelSheet<ClassJob>()!;
                
                if (ImGui.BeginChild("savedBarsIndexSelect", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true)) {
                    for (byte i = 0; i < _actionManager.raptureHotarModule->SavedHotBarsSpan.Length; i++) {
                        var classJobId = _actionManager.raptureHotarModule->GetClassJobIdForSavedHotbarIndex(i);
                        var jobName = classJobId == 0 ? "Shared" : classJobSheet.GetRow(classJobId)?.Abbreviation?.RawString;
                        var isPvp = i >= classJobSheet.RowCount;
                        
                        // hack for unreleased jobs
                        if (jobName.IsNullOrEmpty() || (i > classJobSheet.RowCount && classJobId == 0)) jobName = "Unknown";
                        
                        if (ImGui.Selectable($"{i}: {(isPvp ? "[PVP]" : "")} {jobName}", selectedSavedIndex == i)) {
                            selectedSavedIndex = i;
                        }
                    }
                }
                ImGui.EndChild();
                ImGui.SameLine();
                ImGui.BeginGroup();
                var savedBarClassJob = _actionManager.raptureHotarModule->SavedHotBarsSpan.GetPointer(selectedSavedIndex);
                if (savedBarClassJob != null && ImGui.BeginTabBar("savedClassJobBarSelectType")) {


                    void ShowBar(int b) {

                        var savedBar = savedBarClassJob->HotBarsSpan.GetPointer(b);
                        if (savedBar == null) {
                            ImGui.Text("Bar is Null");
                            return;
                        }

                        if (ImGui.BeginTable("savedClassJobBarSlots", 4)) {

                            ImGui.TableSetupColumn("Slot", ImGuiTableColumnFlags.WidthFixed, 50);
                            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.WidthFixed, 80);
                            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 100);
                            ImGui.TableSetupColumn("Resolved Name", ImGuiTableColumnFlags.WidthStretch, 128);

                            ImGui.TableHeadersRow();

                            for (var i = 0; i < 16; i++) {
                                ImGui.TableNextColumn();
                                ImGui.Text($"{i:00}");
                                ImGui.TableNextColumn();
                                var slot = savedBar->SlotsSpan.GetPointer(i);
                                if (slot == null) {
                                    ImGui.TableNextRow();
                                    continue;
                                }
                                ImGui.Text($"{slot->CommandType}");
                                ImGui.TableNextColumn();
                                ImGui.Text($"{slot->CommandId}");
                                ImGui.TableNextColumn();
                                if (this.ResolveSlotName(slot->CommandType, slot->CommandId, out var resolvedName)) {
                                    ImGui.TextWrapped(resolvedName);
                                } else {
                                    ImGui.TextDisabled(resolvedName);
                                }
                            }

                            ImGui.EndTable();
                        }



                    }

                    if (ImGui.BeginTabItem("Normal")) {
                        if (ImGui.BeginTabBar("savecClassJobBarSelectCross")) {
                            for (var i = 0; i < 10; i++) {
                                if (ImGui.BeginTabItem($"{i + 1:00}")) {
                                    ShowBar(i);
                                    ImGui.EndTabItem();
                                }
                            }
                            ImGui.EndTabBar();
                        }

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Cross")) {
                        if (ImGui.BeginTabBar("savecClassJobBarSelectCross")) {
                            for (var i = 10; i < 18; i++) {
                                if (ImGui.BeginTabItem($"{i-9:00}")) {
                                    ShowBar(i);
                                    ImGui.EndTabItem();
                                }
                            }
                            ImGui.EndTabBar();
                        }

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
                ImGui.EndGroup();
            }
        } catch (System.Exception e) {
            GagSpeak.Log.Error(e.ToString());
        }
        ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }

    private unsafe bool ResolveSlotName(HotbarSlotType type, uint commandId, [CanBeNull] out string resolvedName) {
        resolvedName = "Not Found";

        switch (type) {
            case HotbarSlotType.Empty: {
                resolvedName = "N/A";
                return false;
            }
            case HotbarSlotType.Action: {

                var action = _data.Excel.GetSheet<Action>()!.GetRow(commandId);
                if (action == null) {
                    return false;
                }

                resolvedName = action.Name;
                return true;
            }

            case HotbarSlotType.Item: {
                var item = _data.GetExcelSheet<Item>()!.GetRow(commandId % 500000);
                if (item == null) {
                    return false;
                }

                resolvedName = item.Name;
                return true;
            }

            case HotbarSlotType.CraftAction: {
                var action = _data.GetExcelSheet<CraftAction>()!.GetRow(commandId);
                if (action == null) {
                    return false;
                }

                resolvedName = action.Name;
                return true;
            }

            case HotbarSlotType.GeneralAction: {
                var action = _data.GetExcelSheet<GeneralAction>()!.GetRow(commandId);
                if (action == null) {
                    return false;
                }

                resolvedName = action.Name;
                return true;
            }

            case HotbarSlotType.MainCommand: {
                var action = _data.GetExcelSheet<MainCommand>()!.GetRow(commandId);
                if (action == null) {
                    return false;
                }

                resolvedName = action.Name;
                return true;
            }

            case HotbarSlotType.ExtraCommand: {
                var exc = _data.GetExcelSheet<ExtraCommand>()!.GetRow(commandId);
                if (exc == null) {
                    return false;
                }

                resolvedName = exc.Name;
                return true;
            }

            case HotbarSlotType.GearSet: {
                var gearsetModule = RaptureGearsetModule.Instance();
                var gearset = gearsetModule->GetGearset((int)commandId);

                if (gearset == null) {
                    resolvedName = $"InvalidGearset#{commandId}";
                    return false;
                }

                // resolvedName = $"{Encoding.UTF8.GetString(gearset->Name, 0x2F)}";
                return true;
            }

            case HotbarSlotType.Macro: {
                var macroModule = RaptureMacroModule.Instance();
                var macro = macroModule->GetMacro(commandId / 256, commandId % 256);
                
                if (macro == null) {
                    return false;
                }

                var macroName = macro->Name.ToString();
                if (macroName.IsNullOrEmpty()) {
                    macroName = $"{(commandId >= 256 ? "Shared" : "Individual")} #{commandId % 256}";
                }
                
                resolvedName = macroName;
                return true;
            }

            case HotbarSlotType.Emote: {
                var m = _data.GetExcelSheet<Emote>()!.GetRow(commandId);
                if (m == null) {
                    return false;
                }

                resolvedName = m.Name;
                return true;
            }

            case HotbarSlotType.EventItem: {
                var item = _data.GetExcelSheet<EventItem>()!.GetRow(commandId);
                if (item == null) {
                    return false;
                }

                resolvedName = $"{item.Name}";
                return true;
            }

            case HotbarSlotType.Mount: {
                var m = _data.Excel.GetSheet<Mount>()!.GetRow(commandId);
                if (m == null) {
                    return false;
                }

                resolvedName = $"{m.Singular}";
                return true;
            }

            case HotbarSlotType.Companion: {
                var m = _data.Excel.GetSheet<Companion>()!.GetRow(commandId);
                if (m == null) {
                    return false;
                }

                resolvedName = $"{m.Singular}";
                return true;
            }

            case HotbarSlotType.McGuffin: {
                var c = _data.Excel.GetSheet<McGuffin>()!.GetRow(commandId);
                if (c == null) {
                    return false;
                }

                resolvedName = c.UIData.Value!.Name;
                return true;
            }

            case HotbarSlotType.PetAction: {
                var pa = _data.GetExcelSheet<PetAction>()!.GetRow(commandId);
                if (pa == null) {
                    return false;
                }

                resolvedName = pa.Name;
                return true;
            }
            
            default: {
                resolvedName = "Not Yet Supported";
                return false;
            }
        }
    }
}
