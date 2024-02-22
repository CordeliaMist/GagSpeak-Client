using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_OrdersControl
{
    private readonly HardcoreManager _hardcoreManager;
    public HC_OrdersControl(HardcoreManager hardcoreManager) {
        _hardcoreManager = hardcoreManager;
    }
    public void Draw() {
        using var child = ImRaii.Child("##HC_OrdersControlChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child)
            return;

        ImGuiUtil.Center("uwu");
        
        UIHelpers.CheckboxNoConfig("Follow Me",
        "When this whitelisted user says \"follow me\" you will be forced to follow them.\n"+
        "Your movement is restricted until your character stops moving for a set amount of time.",
        _hardcoreManager._forcedFollow,
        v => _hardcoreManager.SetForcedFollow(!_hardcoreManager._forcedFollow)
        );

        UIHelpers.CheckboxNoConfig("Sit",
        "You will be forced to sit if this whitelisted player says 'sit'.\n"+
        "Your movement is restricted until they say 'stand'",
        _hardcoreManager._forcedSit,
        v => _hardcoreManager.SetForcedSit(!_hardcoreManager._forcedSit)
        );

        UIHelpers.CheckboxNoConfig("Stay Here for Now",
        "When this whitelisted player says \"stay here for now\", teleport & return are blocked.\n"+
        "Additionally, all exit confirmation UI's that pops up will automatically hit no on yesno confirmations.\n"+
        "These permissions are restored when they say \"come along now\"",
        _hardcoreManager._forcedToStay,
        v => _hardcoreManager.SetForcedToStay(!_hardcoreManager._forcedToStay)
        );

        // UIHelpers.CheckboxNoConfig("Disable Movement",
        // "For Testing Purposes, Disabled all movement when checked",
        // _hardcoreManager._movementDisabled,
        // v => _hardcoreManager.SetMovementDisabled(!_hardcoreManager._movementDisabled)
        // );

        // UIHelpers.CheckboxNoConfig("Force Walk",
        // "For Testing Purposes, Forces walking when checked",
        // _hardcoreManager._forcedWalk,
        // v => _hardcoreManager.SetForcedWalk(!_hardcoreManager._forcedWalk)
        // );

        ImGuiUtil.Center("None of these features currently work and are WIP");
    }
}
/*
    - When a whitelisted user says "follow me" you will be forced to follow them and your movement is
    restricted until your character stops moving for a set amount of time.

    - when whitelisted user says "sit", you will be forced to sit and your movement is restricted until they say "stand"
    (preferably find the cpose that is on ones knees and force that cpose, or let someone select which cpose to force)

    - after a whitelisted player says "stay here for now", teleport and return will be blocked, and your any exit US that pops up will
    automatically hit no on yesno confirmations... These permissions are restored when they say "come along now"
*/