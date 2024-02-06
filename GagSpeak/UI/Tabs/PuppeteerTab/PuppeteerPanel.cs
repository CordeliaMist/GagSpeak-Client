using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Events;
using GagSpeak.CharacterData;
using GagSpeak.ChatMessages;
using Dalamud.Interface.Utility;
using GagSpeak.Services;

namespace GagSpeak.UI.Tabs.PuppeteerTab;
public partial class PuppeteerPanel
{
    private readonly    PuppeteerAliasTable         _aliasTable;
    private readonly    CharacterHandler            _characterHandler;
    private readonly    GagSpeakConfig              _config;
    private readonly    FontService                 _fonts;
    private             string?                     _tempTriggerPhrase;
    public PuppeteerPanel(CharacterHandler characterHandler, GagSpeakConfig config, 
    PuppeteerAliasTable aliasTable, FontService fontService) {
        _characterHandler = characterHandler;
        _config = config;
        _aliasTable = aliasTable;
        _fonts = fontService;
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
        using var child = ImRaii.Child("##PuppeteerEnabledChannelsChild", new Vector2(ImGui.GetContentRegionAvail().X, height), true, ImGuiWindowFlags.NoScrollbar);
        if (!child) { return; }
        // draw ourcontent
        var i = 0;
        foreach (var e in ChatChannel.GetOrderedChannels()) {
            // See if it is already enabled by default
            var enabled = _config.ChannelsPuppeteer.Contains(e);
            // Create a new line after every 4 columns
            if (i != 0 && (i==5 || i==9 || i==13 || i==17 || i == 21)) {
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

        // change column count to 2
        ImGui.Columns(2,"PuppeteerSettings", false);
        // set width to 2/3 the width of the content region
        ImGui.SetColumnWidth(0, width*2.3f);
        // draw the trigger phrase
        ImGui.PushFont(_fonts.UidFont);
        ImGui.Text($"Your Trigger Phrase for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}:");
        // store the input text boxes trigger phrase
        var TriggerPhrase  = _tempTriggerPhrase ?? _characterHandler.playerChar._triggerPhraseForPuppeteer[_characterHandler.activeListIdx];
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        if (ImGui.InputText($"##{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}sTriggerPhrase", ref TriggerPhrase, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            _tempTriggerPhrase = TriggerPhrase;
        // will only update our safeword once we click away or enter is pressed
        if (ImGui.IsItemDeactivatedAfterEdit()) {
            _characterHandler.SetNewTriggerPhrase(TriggerPhrase);
            _tempTriggerPhrase = null;
        }
        ImGui.PopFont();
        // go to the next column
        ImGui.NextColumn();
        // draw out the permissions
        var checkbox1Value = _characterHandler.playerChar._allowSitRequests[_characterHandler.activeListIdx];
        var checkbox2Value = _characterHandler.playerChar._allowMotionRequests[_characterHandler.activeListIdx];
        var checkbox3Value = _characterHandler.playerChar._allowAllCommands[_characterHandler.activeListIdx];
        if(ImGui.Checkbox($"Sit & Groundsit", ref checkbox1Value)) {
            _characterHandler.UpdateAllowSitRequests(checkbox1Value);
        }
        // next box
        if(ImGui.Checkbox("Motion Commands", ref checkbox2Value)) {
            _characterHandler.UpdateAllowMotionRequests(checkbox2Value);
        }
        // next box
        if(ImGui.Checkbox("All Commands", ref checkbox3Value)) {
            _characterHandler.UpdateAllowAllCommands(checkbox3Value);
        }
        // end the columns
        ImGui.Columns(1);
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 3*ImGuiHelpers.GlobalScale);
        ImGui.Separator();
        yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(yPos + 3*ImGuiHelpers.GlobalScale);
        // draw the table on the next line
        _aliasTable.Draw();
    }

}