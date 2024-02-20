using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using GagSpeak.Services;
using GagSpeak.UI.Tabs.WardrobeTab;
using GagSpeak.Utility;
using GagSpeak.Wardrobe;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_RestraintSetProperties
{
    private readonly CharacterHandler _charHandler;
    private readonly RestraintSetManager _restraintSetManager;
    private readonly RestraintSetSelector _restraintSetSelector;
    private readonly FontService _fontService;
    public HC_RestraintSetProperties(CharacterHandler characterHandler, RestraintSetManager restraintSetManager,
    RestraintSetSelector restraintSetSelector, FontService fontService) {
        _charHandler = characterHandler;
        _restraintSetManager = restraintSetManager;
        _restraintSetSelector = restraintSetSelector;
        _fontService = fontService;
    }
    public void Draw() {
        using var child = ImRaii.Child("##HC_RestraintSetPropertiesChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true);
        if (!child)
            return;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
        ImGui.PushFont(_fontService.UidFont);
        ImGuiUtil.Center($"Restraint Set Properties for {_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}");
        ImGui.PopFont();
        using (var table = ImRaii.Table("restraintSetPropertiesTable", 2, ImGuiTableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GlobalScale*300f))) {
            if (!table) { return; }

            var name = _charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0];
            ImGui.TableSetupColumn($" If Set is Enabled by {name}", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale*200f);
            ImGui.TableSetupColumn("Apply These Properties To Self", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
            var _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
            _restraintSetSelector.DrawRestraintSetSelector(ImGui.GetContentRegionAvail().X+ImGuiHelpers.GlobalScale*3, ImGuiHelpers.GlobalScale*242f, _defaultItemSpacing);
            
            ImGui.TableNextColumn();
            DrawRestraintSetProperties();
            ImGui.TableNextColumn();
            // DrawRestraintSetPreview();
        }
        ImGuiUtil.Center("None of these features currently work and are WIP");
    }

    private void DrawRestraintSetProperties() {
        UIHelpers.CheckboxNoConfig("Legs Are Restrained",
        "Any Action which typically involves fast leg movement is restricted if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._legsRestraintedProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetRestraintedLegsProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Arms Are Restrained",
        "Any Action which typically involves fast arm movement is restricted if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._armsRestraintedProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetRestraintedArmsProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Gagged",
        "Any action requiring speech is restricted if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._gaggedProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetGaggedProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Blindfolded",
        "Any actions requiring awareness or sight is restricted if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._blindfoldedProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetBlindfoldedProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Immobile",
        "Player becomes unable to move in this set if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._immobileProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetImmobileProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Weighty",
        "Player is forced to only walk while wearing this restraint if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._weightyProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetWeightedProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Light Stimulation",
        "Any action requring focus or concentration has its casttime being slightly slower if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._lightStimulationProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetLightStimulationProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Mild Stimulation",
        "Any action requring focus or concentration has its casttime being noticably slower if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._mildStimulationProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetMildStimulationProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );

        UIHelpers.CheckboxNoConfig("Heavy Stimulation",
        "Any action requring focus or concentration has its casttime being significantly slower if this is a active property of the set",
        _charHandler.playerChar._uniquePlayerPerms[_charHandler.activeListIdx]._heavyStimulationProperty[_restraintSetManager._selectedIdx],
        v => _charHandler.SetHeavyStimulationProperty(_charHandler.activeListIdx, _restraintSetManager._selectedIdx, v)
        );
    }
}
