using System.Numerics;
using Dalamud.Interface;
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
using Penumbra.GameData.Enums;

namespace GagSpeak.UI.Tabs.HardcoreTab;
public class HC_RestraintSetProperties
{
    private readonly    GagSpeakConfig          _config;
    private readonly    CharacterHandler        _charHandler;
    private readonly    HardcoreManager         _hcManager;
    private readonly    RestraintSetManager     _restraintSetManager;
    private readonly    RestraintSetSelector    _restraintSetSelector;
    private readonly    FontService             _fontService;
    private readonly    TextureService          _textures;              // for getting the textures
    private readonly    Vector2                 _iconSize;              // size of icons that can display
    private             string[]                _eyeIcon;
    private             int                     _curSetIdx;
    public HC_RestraintSetProperties(CharacterHandler characterHandler, RestraintSetManager restraintSetManager,
    RestraintSetSelector restraintSetSelector, FontService fontService, HardcoreManager hardcoreManager,
    TextureService textures, GagSpeakConfig config) {
        _config = config;
        _charHandler = characterHandler;
        _restraintSetManager = restraintSetManager;
        _restraintSetSelector = restraintSetSelector;
        _fontService = fontService;
        _hcManager = hardcoreManager;
        _textures = textures;

        _iconSize    = ImGuiHelpers.ScaledVector2(1.5f*ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y);
        _eyeIcon = new string[EquipSlotExtensions.EqdpSlots.Count];

        _curSetIdx = _restraintSetManager._selectedIdx;

    }
    public void Draw() {
        if (!_config.hardcoreMode) { ImGui.BeginDisabled(); }
        try {
            // grab a temp var for the selectedIdx of the restraint set
            if(_curSetIdx != _restraintSetManager._selectedIdx) {
                _curSetIdx = _restraintSetManager._selectedIdx;        
            };
            // draw out the details
            using var child = ImRaii.Child("##HC_RestraintSetPropertiesChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
            if (!child)
                return;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
            ImGui.PushFont(_fontService.UidFont);
            var name = $"{_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}";
            ImGuiUtil.Center($"Restraint Set Properties for {name}");
            ImGui.PopFont();
            using (var table = ImRaii.Table("restraintSetPropertiesTable", 2, ImGuiTableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GlobalScale*235f))) {
                if (!table) { return; }

                ImGui.TableSetupColumn($" If Set is Enabled by {name}", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale*235f);
                ImGui.TableSetupColumn("Set Preview", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                // draw the header label
                DrawHeaderButton($"If this set is enabled by {name}", 
                "The Properties that you apply below this list are dependant on the set selected from this list.\n"+
                "They are ALSO dependant on the user selected from the whitelist.", 
                ImGuiHelpers.GlobalScale*235f);
                // set up the list
                var _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
                _restraintSetSelector.DrawRestraintSetSelector(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GlobalScale*235f - ImGui.GetFrameHeightWithSpacing(), _defaultItemSpacing);
                // draw the currently selected set preview
                ImGui.TableNextColumn();
                // draw the header label
                DrawHeaderButton($"Visual Reference", "A visual display reference of the restraint set you have configured in the wardrobe");
                // draw out the restraint set options
                DrawRestraintSetPreview();
            }
            // draw the header label
            DrawHeaderButton($"Apply the following properties to set [{_restraintSetManager._restraintSets[_curSetIdx]._name}]", 
            $"If {name} enables this set onto you, any property you have selected here will be applied to you");
            DrawRestraintSetProperties();
        } finally {
            if (!_config.hardcoreMode) { ImGui.EndDisabled(); }
        }
    }

    private void DrawHeaderButton(string HeaderText, string descirption = "", float width = -1f) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        ImGuiUtil.DrawDisabledButton($"{HeaderText}", new Vector2(width, ImGuiHelpers.GlobalScale*23), $"{descirption}", false, false);
        style.Pop();
    }

    private void DrawRestraintSetPreview() {
        using (var table2 = ImRaii.Table("RestraintEquipSelection", 2, ImGuiTableFlags.RowBg)) {
            if (!table2) return;
            // Create the headers for the table
            var width = ImGui.GetContentRegionAvail().X/2 - ImGui.GetStyle().ItemSpacing.X;
            // setup the columns
            ImGui.TableSetupColumn("EquipmentSlots", ImGuiTableColumnFlags.WidthFixed, width);
            ImGui.TableSetupColumn("AccessorySlots", ImGuiTableColumnFlags.WidthStretch);

            // draw out the equipment slots
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            int i = 0;
            foreach(var slot in EquipSlotExtensions.EquipmentSlots) {
                _restraintSetManager._restraintSets[_curSetIdx]._drawData[slot]._gameItem.DrawIcon(_textures, _iconSize, slot);
                ImGui.SameLine();
                _eyeIcon[i] = _restraintSetManager._restraintSets[_curSetIdx]._drawData[slot]._isEnabled
                            ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
                // display either eyeslash or eye based on if it is enabled or not
                if(ImGuiUtil.DrawDisabledButton($"{_eyeIcon[i]}##{slot}VisibilityButton",
                new Vector2(ImGui.GetContentRegionAvail().X, 1.5f*ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y), "", true, true)) { }
                i++;
            }
            // i am dumb and dont know how to place adjustable divider lengths
            ImGui.TableNextColumn();
            //draw out the accessory slots
            foreach(var slot in EquipSlotExtensions.AccessorySlots) {
                _restraintSetManager._restraintSets[_curSetIdx]._drawData[slot]._gameItem.DrawIcon(_textures, _iconSize, slot);
                ImGui.SameLine();
                _eyeIcon[i] = _restraintSetManager._restraintSets[_curSetIdx]._drawData[slot]._isEnabled 
                            ? FontAwesomeIcon.Eye.ToIconString() : FontAwesomeIcon.EyeSlash.ToIconString();
                // display either eyeslash or eye based on if it is enabled or not
                if(ImGuiUtil.DrawDisabledButton($"{_eyeIcon[i]}##{slot}VisibilityButton",
                new Vector2(ImGui.GetContentRegionAvail().X, 1.5f*ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.Y), "", true, true)) { }
                i++;
            }
        }
    }


    private void DrawRestraintSetProperties() {
        using (var table2 = ImRaii.Table("PropertiesSelection", 2, ImGuiTableFlags.None)) {
            if (!table2) return;
            // Create the headers for the table
            var width = ImGui.GetContentRegionAvail().X/2 - ImGui.GetStyle().ItemSpacing.X;
            // setup the columns
            ImGui.TableSetupColumn("EquipmentSlots", ImGuiTableColumnFlags.WidthFixed, width);
            ImGui.TableSetupColumn("AccessorySlots", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow(); ImGui.TableNextColumn();
            ImGui.Spacing();
            UIHelpers.CheckboxNoConfig("Legs Are Restrained",
            "Actions for your current class that involve primary Leg movement are restricted.\n"+
            "(( Only modifies live hotbar display, no hotbar data can be lost ))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._legsRestraintedProperty,
            v => _hcManager.SetLegsRestraintedProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();
            ImGui.Spacing();
            UIHelpers.CheckboxNoConfig("Arms Are Restrained",
            "Actions for your current class that involve primary Arm movement are restricted.\n"+
            "(( Only modifies live hotbar display, no hotbar data can be lost ))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._armsRestraintedProperty,
            v => _hcManager.SetArmsRestraintedProperty(_charHandler.activeListIdx, _curSetIdx, v)
            );
            ImGui.TableNextColumn();
            UIHelpers.CheckboxNoConfig("Gagged",
            "Actions for your current class that involve orders to pets or verbal spells are restricted.\n"+
            "(( Only modifies live hotbar display, no hotbar data can be lost ))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._gaggedProperty,
            v => _hcManager.SetGaggedProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();    
            UIHelpers.CheckboxNoConfig("Blindfolded",
            "Actions for your current class that are ranged instant cast moves where you need to know where the target is are restricted.\n"+
            "(( Only modifies live hotbar display, no hotbar data can be lost ))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._blindfoldedProperty,
            v => _hcManager.SetBlindfoldedProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();
            UIHelpers.CheckboxNoConfig("Immobile",
            "Actions for your current class that involving movement of any kind are restricted. You are also locked in place and cannot move.\n"+
            "(( Only modifies live hotbar display, no hotbar data can be lost ))\n"+
            "(( All movement keys are thrown away before they are sent to the game, so there is nothing to worry about. ))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._immobileProperty,
            v => _hcManager.SetImmobileProperty(_charHandler.activeListIdx, _curSetIdx,
            !_hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._immobileProperty)
            );
            ImGui.TableNextColumn();
            UIHelpers.CheckboxNoConfig("Weighty",
            "Your body is weighted down by the restraints, slowing your movement...",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._weightyProperty,
            v => _hcManager.SetWeightedProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();
            UIHelpers.CheckboxNoConfig("Light Stimulation",
            "If one is under stimulation, their mind can become slightly distracted...\n\n"+
            "(( Recast Time Multiplier for all action groups are multiplied by 1.1x))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._lightStimulationProperty,
            v => _hcManager.SetLightStimulationProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();
            UIHelpers.CheckboxNoConfig("Mild Stimulation",
            "If one is under mild stimulation, you begin to loose focus on the action at hand before you...\n\n"+
            "(( Recast Time Multiplier for all action groups are multiplied by 1.25x))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._mildStimulationProperty,
            v => _hcManager.SetMildStimulationProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();
            UIHelpers.CheckboxNoConfig("Heavy Stimulation",
            "Under heavy stimulation, you start caring more about your own pleasure than the combat before you...\n\n"+
            "(( Recast Time Multiplier for all action groups are multiplied by 1.5x))",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._rsProperties[_curSetIdx]._heavyStimulationProperty,
            v => _hcManager.SetHeavyStimulationProperty(_charHandler.activeListIdx, _curSetIdx, v));
            ImGui.TableNextColumn();
        }
    }
}
