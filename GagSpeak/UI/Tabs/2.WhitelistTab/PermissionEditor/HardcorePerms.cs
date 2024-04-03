using System.Numerics;
using ImGuiNET;
using GagSpeak.Utility;
using OtterGui;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface;
using GagSpeak.CharacterData;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {

#region DrawPuppeteerPerms
    public void DrawHardcorePerms(ref bool _interactions) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _tempWhitelistChar.GetDynamicTierClient();
        // store temp name for display

        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("HardcoreManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;

            ImGui.TableSetupColumn($"Hardcore Option",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Allowed?",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Allowed?").X);
            ImGui.TableSetupColumn("State",         ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Toggle Trigger",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Toggle Trigger").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();

            // the follow order command
            ImGuiUtil.DrawFrameColumn($"Follow Order:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["FollowOrderTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._allowForcedFollow ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If this permission is allowed by {AltCharHelpers.FetchCurrentName().Split(' ')[0]} or not"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._forcedFollow ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If {AltCharHelpers.FetchCurrentName().Split(' ')[0]} is currently performing this order or not"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("HOVER ME");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"To Enable the follow command, say \"{AltCharHelpers.FetchCurrentName().Split(' ')[0]}, follow me.\"\n"+
                 "To Disable the order, they must remain still for 6 seconds.");
            }

            // then sit order
            ImGuiUtil.DrawFrameColumn($"Sit Order:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["SitOrderTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._allowForcedSit ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If this permission is allowed by {AltCharHelpers.FetchCurrentName().Split(' ')[0]} or not"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._forcedSit ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If {AltCharHelpers.FetchCurrentName().Split(' ')[0]} is currently performing this order or not"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("HOVER ME");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"To Enable the sit command, say \"{AltCharHelpers.FetchCurrentName().Split(' ')[0]}, sit.\"\n"+
                $"To Enable the groundsit command, say \"{AltCharHelpers.FetchCurrentName().Split(' ')[0]}, on your knees.\"\n"+
                $"To Disable the sit command, say \"you may stand now {AltCharHelpers.FetchCurrentName().Split(' ')[0]}.\"");
            }

            // the locked away order
            ImGuiUtil.DrawFrameColumn($"Lock Away Order:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["LockAwayTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._allowForcedToStay ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If this permission is allowed by {AltCharHelpers.FetchCurrentName().Split(' ')[0]} or not"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._forcedToStay ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If {AltCharHelpers.FetchCurrentName().Split(' ')[0]} is currently performing this order or not"); }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("HOVER ME");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"To Enable lock away command, say \"{AltCharHelpers.FetchCurrentName().Split(' ')[0]}, stay here until I return.\"\n"+
                $"To Disable, say \"thank you for waiting, {AltCharHelpers.FetchCurrentName().Split(' ')[0]}.\"");
            }

            // the blindfold order
            ImGuiUtil.DrawFrameColumn($"Blindfold Order:");
            if(ImGui.IsItemHovered()) { var tt = tooltips["BlindfoldTT"](); ImGui.SetTooltip($"{tt}"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._allowBlindfold ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If this permission is allowed by {AltCharHelpers.FetchCurrentName().Split(' ')[0]} or not"); }
            ImGui.TableNextColumn();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) { ImGuiUtil.Center((_tempWhitelistChar._blindfolded ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString()); }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"If {AltCharHelpers.FetchCurrentName().Split(' ')[0]} is currently performing this order or not"); }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleBlindfoldStateButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            tooltips["ToggleButtonTT"](), !_tempWhitelistChar._allowBlindfold)) {
                // how to treat what happens when we press the button
                ToggleBlindfoldOption();
                _interactOrPermButtonEvent.Invoke(5);

            }
        }
        // pop the style
        ImGui.PopStyleVar();
    }
#endregion DrawPuppeteerPerms
#region ButtonHelpers
    public void ToggleBlindfoldOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = AltCharHelpers.FetchNameWorldFormatByWhitelistIdxForNAWIdxToProcess(_characterHandler.activeListIdx);
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"this players Blindfold! Make sure they dont trip!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetBlindfoldCondition(_characterHandler.activeListIdx, !_tempWhitelistChar._blindfolded);
        _chatManager.SendRealMessage(_messageEncoder.EncodeBlindfoldToggleOption(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}