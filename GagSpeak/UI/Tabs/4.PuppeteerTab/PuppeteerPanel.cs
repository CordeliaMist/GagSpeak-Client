using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.CharacterData;
using GagSpeak.ChatMessages;
using Dalamud.Interface.Utility;
using GagSpeak.Services;
using OtterGui;
using System;
using GagSpeak.Utility;

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

    public void Draw() {
        using (var _ = ImRaii.Group()){
            var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
            ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
            DrawPermissionsHeader();
            DrawPlayerPanel();
        }
    }

    // draw the header
    private void DrawPermissionsHeader() {
        WindowHeader.Draw($"Setup Puppeteer Preferences For {AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess)}",
        0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0);
    }

    private void DrawPlayerPanel() {
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using var child = ImRaii.Child("##PuppeteerDrawPanel", Vector2.Zero, true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return; }
        var width = ImGui.GetContentRegionAvail().X;
        var yPos = ImGui.GetCursorPosY();
        // temp name
        string tempPlayerName = AltCharHelpers.FetchName(_characterHandler.activeListIdx, _characterHandler.whitelistChars[_characterHandler.activeListIdx]._charNAWIdxToProcess);
        // draw the trigger phrase
        ImGui.PushFont(_fonts.UidFont);
        try{
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
            ImGui.Text($"Trigger that {tempPlayerName.Split(' ')[0]} can use on You");
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip($"The Trigger Phrase that you have set for {tempPlayerName.Split(' ')[0]}.\n"+
                                $"If {tempPlayerName.Split(' ')[0]} says this in chat in any enabled channels,\n"+
                                $"you will execute whatever comes after the trigger phrase,\n(or what is enclosed within the start and end brackets)");
            }
        } finally {
            ImGui.PopFont();
        }

        if(!string.IsNullOrEmpty(_characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._triggerPhraseForPuppeteer)) {
           bool hasSplits = _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._triggerPhraseForPuppeteer.Contains("|");
            var displayText ="";
            if(hasSplits) {
                displayText = _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._triggerPhraseForPuppeteer.Split('|')[0];
            } else {
                displayText = _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._triggerPhraseForPuppeteer;
            }
            // example display
            ImGui.Text("Example:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1.0f,1.0f,0.0f,1.0f), $"<{tempPlayerName}> "+
            $"{displayText} {_characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._StartCharForPuppeteerTrigger} "+
            $"glamour apply Hogtied | p | [me] {_characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._EndCharForPuppeteerTrigger}");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"The spaces between the brackets and commands/trigger phrases are optional."); }
        }
        // trigger phrase
        ImGui.PushFont(_fonts.UidFont);
        try {
            // store the input text boxes trigger phrase
            var TriggerPhrase  = _tempTriggerPhrase ?? _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._triggerPhraseForPuppeteer;
            ImGui.SetNextItemWidth(width);
            if (ImGui.InputTextWithHint($"##{tempPlayerName}sTriggerPhrase",
            "Phrase that makes you execute commands", ref TriggerPhrase, 64, ImGuiInputTextFlags.EnterReturnsTrue)) {
                _tempTriggerPhrase = TriggerPhrase;
            }
            // will only update our safeword once we click away or enter is pressed
            if (ImGui.IsItemDeactivatedAfterEdit()) {
                _characterHandler.SetNewTriggerPhrase(TriggerPhrase);
                _tempTriggerPhrase = null;
            }
        } finally {
            ImGui.PopFont();
        }
        // draw out the start and end characters
        var tempStartParam  = _tempStartParameter ?? _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._StartCharForPuppeteerTrigger;
        ImGui.SetNextItemWidth(20*ImGuiHelpers.GlobalScale);
        if (ImGui.InputText($"##{tempPlayerName}sBegin",
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
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 15*ImGuiHelpers.GlobalScale);
        ImGuiUtil.LabeledHelpMarker("", 
        $"Custom Start Character that replaces the left enclosing bracket.\n"+
        "Replaces the [ ( ] in Ex: [ TriggerPhrase (commandToExecute) ]");
        var tempEndParam  = _tempEndParameter ?? _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._EndCharForPuppeteerTrigger;
        ImGui.SameLine();
        ImGui.SetNextItemWidth(20*ImGuiHelpers.GlobalScale);
        if (ImGui.InputText($"##{tempPlayerName}sEnd", 
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
        ImGuiUtil.LabeledHelpMarker("", 
            $"Custom End Character that replaces the right enclosing bracket.\n"+
            "Replaces the [ ) ] in Ex: [ TriggerPhrase (commandToExecute) ]");
        ImGui.SameLine();
        // draw out the permissions
        var checkbox1Value = _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowSitRequests;
        var checkbox2Value = _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowMotionRequests;
        var checkbox3Value = _characterHandler.playerChar._uniquePlayerPerms[_characterHandler.activeListIdx]._allowAllCommands;
        if(ImGui.Checkbox($"##Sitting", ref checkbox1Value)) {
            _characterHandler.UpdateAllowSitRequests(checkbox1Value);
        }
        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker("Sitting", 
        $"If you are giving {tempPlayerName.Split(' ')[0]} access "+
        "to make you execute /sit and /groundsit commands with your trigger phrase.");
        // next box
        ImGui.SameLine();
        if(ImGui.Checkbox("##Emotes", ref checkbox2Value)) {
            _characterHandler.UpdateAllowMotionRequests(checkbox2Value);
        }
        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker("Emotes", 
            $"If you are giving {tempPlayerName.Split(' ')[0]} access "+
            "to make you execute emotes and expressions with your trigger phrase.");
        ImGui.SameLine();
        // next box
        if(ImGui.Checkbox("##All", ref checkbox3Value)) {
            _characterHandler.UpdateAllowAllCommands(checkbox3Value);
        }
        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker("All", 
            $"If you are giving {tempPlayerName.Split(' ')[0]} access "+
            "to make you execute any command with your trigger phrase.");
        // go to draw out the alias table.
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 3*ImGuiHelpers.GlobalScale);
        ImGui.Separator();
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 3*ImGuiHelpers.GlobalScale);
        // draw the table on the next line
        _aliasTable.Draw();
    }
}