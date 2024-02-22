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
    private readonly HardcoreManager _hardcoreManager;
    private readonly RestraintSetManager _restraintSetManager;
    private readonly RestraintSetSelector _restraintSetSelector;
    private readonly FontService _fontService;
    public HC_RestraintSetProperties(CharacterHandler characterHandler, RestraintSetManager restraintSetManager,
    RestraintSetSelector restraintSetSelector, FontService fontService, HardcoreManager hardcoreManager) {
        _charHandler = characterHandler;
        _hardcoreManager = hardcoreManager;
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
        // these are intentionally global for now
        UIHelpers.CheckboxNoConfig("Legs Are Restrained",
        "Any Action which typically involves fast leg movement is restricted if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._legsRestraintedProperty,
        v => _hardcoreManager.SetLegsRestraintedProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._legsRestraintedProperty)
        );

        UIHelpers.CheckboxNoConfig("Arms Are Restrained",
        "Any Action which typically involves fast arm movement is restricted if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._armsRestraintedProperty,
        v => _hardcoreManager.SetArmsRestraintedProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._armsRestraintedProperty)
        );

        UIHelpers.CheckboxNoConfig("Gagged",
        "Any action requiring speech is restricted if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._gaggedProperty,
        v => _hardcoreManager.SetGaggedProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._gaggedProperty)
        );
            
        UIHelpers.CheckboxNoConfig("Blindfolded",
        "Any actions requiring awareness or sight is restricted if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._blindfoldedProperty,
        v => _hardcoreManager.SetBlindfoldedProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._blindfoldedProperty)
        );

        UIHelpers.CheckboxNoConfig("Immobile",
        "Player becomes unable to move in this set if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._immobileProperty,
        v => _hardcoreManager.SetImmobileProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._immobileProperty)
        );

        UIHelpers.CheckboxNoConfig("Weighty",
        "Player is forced to only walk while wearing this restraint if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._weightyProperty,
        v => _hardcoreManager.SetWeightedProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._weightyProperty)
        );

        UIHelpers.CheckboxNoConfig("Light Stimulation",
        "Any action requring focus or concentration has its casttime being slightly slower if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._lightStimulationProperty,
        v => _hardcoreManager.SetLightStimulationProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._lightStimulationProperty)
        );

        UIHelpers.CheckboxNoConfig("Mild Stimulation",
        "Any action requring focus or concentration has its casttime being noticably slower if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._mildStimulationProperty,
        v => _hardcoreManager.SetMildStimulationProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._mildStimulationProperty)
        );

        UIHelpers.CheckboxNoConfig("Heavy Stimulation",
        "Any action requring focus or concentration has its casttime being significantly slower if this is a active property of the set",
        _hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._heavyStimulationProperty,
        v => _hardcoreManager.SetHeavyStimulationProperty(_restraintSetManager._selectedIdx,
        !_hardcoreManager._rsProperties[_restraintSetManager._selectedIdx]._heavyStimulationProperty)
        );
    }
}
