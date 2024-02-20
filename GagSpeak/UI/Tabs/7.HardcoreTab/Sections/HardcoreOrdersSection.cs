using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_OrdersControl
{
    private readonly CharacterHandler _charHandler;
    public HC_OrdersControl(CharacterHandler characterHandler) {
        _charHandler = characterHandler;
    }
    public void Draw() {
        using var child = ImRaii.Child("##HC_OrdersControlChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child)
            return;

        ImGuiUtil.Center("uwu");
        
        UIHelpers.CheckboxNoConfig("Follow Me",
        "When this whitelisted user says \"follow me\" you will be forced to follow them.\n"+
        "Your movement is restricted until your character stops moving for a set amount of time.",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._followMe,
        v => _charHandler.SetFollowMe(_charHandler.activeListIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Sit",
        "You will be forced to sit if this whitelisted player says 'sit'.\n"+
        "Your movement is restricted until they say 'stand'",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._sit,
        v => _charHandler.SetSit(_charHandler.activeListIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Stay Here for Now",
        "When this whitelisted player says \"stay here for now\", teleport & return are blocked.\n"+
        "Additionally, all exit confirmation UI's that pops up will automatically hit no on yesno confirmations.\n"+
        "These permissions are restored when they say \"come along now\"",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._stayHereForNow,
        v => _charHandler.SetStayHereForNow(_charHandler.activeListIdx, v)
        );

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