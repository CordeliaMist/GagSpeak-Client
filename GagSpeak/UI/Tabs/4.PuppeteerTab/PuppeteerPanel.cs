using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Events;
using GagSpeak.CharacterData;
using GagSpeak.ChatMessages;
using Dalamud.Interface.Utility;
using GagSpeak.Services;
using OtterGui;
using System;
using Dalamud.Interface;
using System.Linq;
using System.Collections.Generic;
using GagSpeak.Interop;
using Newtonsoft.Json;

namespace GagSpeak.UI.Tabs.PuppeteerTab;
public partial class PuppeteerPanel
{
    private readonly    PuppeteerAliasTable         _aliasTable;
    private readonly    CharacterHandler            _characterHandler;
    private readonly    GagSpeakConfig              _config;
    private readonly    FontService                 _fonts;
    private             string?                     _tempTriggerPhrase;
    private             string?                     _tempStartParameter;
    private             string?                     _tempEndParameter;
    public PuppeteerPanel(CharacterHandler characterHandler, GagSpeakConfig config, 
    PuppeteerAliasTable aliasTable, FontService fontService) {
        _characterHandler = characterHandler;
        _config = config;
        _aliasTable = aliasTable;
        _fonts = fontService;
        _tempTriggerPhrase = null;
        _tempStartParameter = null;
        _tempEndParameter = null;
    }

    public void Draw(float height, float width) {
        using (var _ = ImRaii.Group()){
            var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
            ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
            DrawPermissionsHeader();
            DrawEnabledChannels(height);
        }
        DrawPlayerPanel(width);
    }

    // draw the header
    private void DrawPermissionsHeader() {
        WindowHeader.Draw($"Setup Puppeteer Preferences For {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}",
        0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0);
    }

    private void DrawEnabledChannels(float height) {
        using var child = ImRaii.Child("##PuppeteerEnabledChannelsChild", new Vector2(ImGui.GetContentRegionAvail().X, height+ImGuiHelpers.GlobalScale), true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return; }
        // draw ourcontent
        var i = 0;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
        foreach (var e in ChatChannel.GetOrderedChannels()) {
            // See if it is already enabled by default
            var enabled = _config.ChannelsPuppeteer.Contains(e);
            // Create a new line after every 4 columns
            if (i != 0 && (i==4 || i==7 || i==11 || i==15 || i == 19)) {
                ImGui.NewLine();
                //i = 0;
            }
            // Move to the next row if it is LS1 or CWLS1
            if (e is ChatChannel.ChatChannels.LS1 or ChatChannel.ChatChannels.CWL1)
                ImGui.Separator();

            if (ImGui.Checkbox($"{e}", ref enabled)) {
                // See If the UIHelpers.Checkbox is clicked, If not, add to the list of enabled channels, otherwise, remove it.
                if (enabled) _config.ChannelsPuppeteer.Add(e);
                else _config.ChannelsPuppeteer.Remove(e);
                _config.Save();
            }

            ImGui.SameLine();
            i++;
        }
        // Set the columns back to 1 now and space over to next section
        ImGui.Columns(1);
    }

    private void DrawPlayerPanel(float width) {
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using var child = ImRaii.Child("##PuppeteerDrawPanel", Vector2.Zero, true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return; }
        
        var yPos = ImGui.GetCursorPosY();
        using (var table = ImRaii.Table("UniqueAliasListCreator", 3, ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.BordersInnerV)) {
            if (!table) { return; }
            width = ImGui.GetContentRegionAvail().X;
            ImGui.TableSetupColumn("##TriggerPhrase", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 310);
            ImGui.TableSetupColumn("##Options", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale * 110);
            ImGui.TableSetupColumn("##Checkboxes", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            // draw the trigger phrase
            ImGui.PushFont(_fonts.UidFont);
            try{
                yPos = ImGui.GetCursorPosY();
                ImGui.SetCursorPosY(yPos - 5*ImGuiHelpers.GlobalScale);
                ImGui.Text($"Trigger Phrase for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}:");
                // store the input text boxes trigger phrase
                var TriggerPhrase  = _tempTriggerPhrase ?? _characterHandler.playerChar._triggerPhraseForPuppeteer[_characterHandler.activeListIdx];
                ImGui.SetNextItemWidth(305*ImGuiHelpers.GlobalScale);
                if (ImGui.InputText($"##{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}sTriggerPhrase", ref TriggerPhrase, 64, ImGuiInputTextFlags.EnterReturnsTrue))
                    _tempTriggerPhrase = TriggerPhrase;
                // will only update our safeword once we click away or enter is pressed
                if (ImGui.IsItemDeactivatedAfterEdit()) {
                    _characterHandler.SetNewTriggerPhrase(TriggerPhrase);
                    _tempTriggerPhrase = null;
                }
            } catch (Exception e) {
                GagSpeak.Log.Error($"[PuppeteerPanel]: Error drawing trigger phrase: {e.Message}");
            } finally {
                ImGui.PopFont();
            }
            // go to the next column
            ImGui.TableNextColumn();
            // draw out the inputs for our custom start and end parameters
            var tempStartParam  = _tempStartParameter ?? _characterHandler.playerChar._StartCharForPuppeteerTrigger[_characterHandler.activeListIdx];
            ImGui.SetNextItemWidth(20*ImGuiHelpers.GlobalScale);
            if (ImGui.InputText($"##{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}sBegin",
            ref tempStartParam, 1, ImGuiInputTextFlags.EnterReturnsTrue)) {
                _tempStartParameter = tempStartParam;
            }
            if (ImGui.IsItemDeactivatedAfterEdit()) {
                if(string.IsNullOrEmpty(tempStartParam) || tempStartParam == " ") {
                    tempStartParam = "(";
                }
                _characterHandler.SetNewStartCharForPuppeteerTrigger(tempStartParam);
                _tempStartParameter = null;
            }
            ImGuiUtil.LabeledHelpMarker("Start Char", "Sets a custom start character for enclosing what gets executed after your trigger word");
            var tempEndParam  = _tempEndParameter ?? _characterHandler.playerChar._EndCharForPuppeteerTrigger[_characterHandler.activeListIdx];
            ImGui.SetNextItemWidth(20*ImGuiHelpers.GlobalScale);
            if (ImGui.InputText($"##{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}sEnd", 
            ref tempEndParam, 1, ImGuiInputTextFlags.EnterReturnsTrue)) {
                _tempEndParameter = tempEndParam;
            }
            if (ImGui.IsItemDeactivatedAfterEdit()) {
                if(string.IsNullOrEmpty(tempEndParam) || tempEndParam == " ") {
                    tempEndParam = ")";
                }
                _characterHandler.SetNewEndCharForPuppeteerTrigger(tempEndParam);
                _tempEndParameter = null;
            }
            ImGuiUtil.LabeledHelpMarker("End Char", "Sets a custom start character for enclosing what gets executed after your trigger word");
        
            // go to the next column
            ImGui.TableNextColumn();
            // draw out the permissions
            var checkbox1Value = _characterHandler.playerChar._allowSitRequests[_characterHandler.activeListIdx];
            var checkbox2Value = _characterHandler.playerChar._allowMotionRequests[_characterHandler.activeListIdx];
            var checkbox3Value = _characterHandler.playerChar._allowAllCommands[_characterHandler.activeListIdx];
            if(ImGui.Checkbox($"##Sitting", ref checkbox1Value)) {
                _characterHandler.UpdateAllowSitRequests(checkbox1Value);
            }
            ImGui.SameLine();
            ImGuiUtil.LabeledHelpMarker("Sitting", "Allows commands like /sit and /groundsit");
            // next box
            if(ImGui.Checkbox("##Emotes", ref checkbox2Value)) {
                _characterHandler.UpdateAllowMotionRequests(checkbox2Value);
            }
            ImGui.SameLine();
            ImGuiUtil.LabeledHelpMarker("Emotes", "Allows emote and expressions");
            // next box
            if(ImGui.Checkbox("##All", ref checkbox3Value)) {
                _characterHandler.UpdateAllowAllCommands(checkbox3Value);
            }
            ImGui.SameLine();
            ImGuiUtil.LabeledHelpMarker("All", "Can use any commands with arguements.\n"+
            $"Commands can be encapsulated in the start & end parameters you defined above.");
        }
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 3*ImGuiHelpers.GlobalScale);
        ImGui.Separator();
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 3*ImGuiHelpers.GlobalScale);
        // draw the table on the next line
        _aliasTable.Draw();
    }
}