using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using ImGuiNET;
using OtterGui;
using Dalamud.Plugin.Services;
using GagSpeak.Hardcore.Actions;
using JetBrains.Annotations;
using Dalamud.Game.Text.SeStringHandling;
using GagSpeak.Utility;
using System.Runtime.InteropServices;
using GagSpeak.Services;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using System.Linq;
using Penumbra.GameData.Enums;
using GagSpeak.UI.Equipment;
using Penumbra.GameData.DataContainers;
using Penumbra.GameData.Data;
namespace GagSpeak.UI.Tabs.HardcoreTab;
public class HC_ControlRestrictions
{
    private readonly GagSpeakConfig     _config;
    private readonly HardcoreManager    _hcManager;
    private readonly GsActionManager    _actionManager;
    private readonly CharacterHandler   _charHandler;
    private readonly FontService        _fontService;
    private readonly BlindfoldWindow    _blindfoldWindow;
    private readonly IClientState       _client;
    private const float _comboWidth = 200;
    private readonly FontService        _fonts;                 // for getting the fonts
    private readonly IDataManager       _gameData;              // for getting the game data
    private readonly TextureService     _textures;              // for getting the textures
    private          Vector2            _iconSize;              // for setting the icon size
    private          float              _comboLength;           // for setting the combo length
    private readonly GameItemCombo[]    _gameItemCombo;         // for getting the item combo
    private readonly StainColorCombo    _stainCombo;            // for getting the stain combo
    private readonly DictStain          _stainData;             // for getting the stain data
    private readonly ItemData           _itemData;              // for getting the item data
    public HC_ControlRestrictions(HardcoreManager hardcoreManager, GsActionManager actionManager,
    CharacterHandler charHandler, FontService fontService, IClientState client, DictStain stainData,
    GagSpeakConfig config, IDataManager gameData, TextureService textures, ItemData itemData,
    FontService fonts, BlindfoldWindow blindfoldWindow) {
        _gameData = gameData;
        _fonts = fonts;
        _textures = textures;
        _itemData = itemData;
        _stainData = stainData;
        _hcManager = hardcoreManager;
        _config = config;
        _actionManager = actionManager;
        _charHandler = charHandler;
        _blindfoldWindow = blindfoldWindow;
        _fontService = fontService;
        _client = client;

        _iconSize    = ImGuiHelpers.ScaledVector2(ImGui.GetFrameHeight()+ImGui.GetFrameHeightWithSpacing());
        _gameItemCombo = EquipSlotExtensions.EqdpSlots.Select(e => new GameItemCombo(_gameData, e, _itemData,  GagSpeak.Log)).ToArray();
        _stainCombo = new StainColorCombo(_comboWidth-20, _stainData);
    }
    public void Draw() {
        _comboLength = _comboWidth * ImGuiHelpers.GlobalScale;
        // if we are not in hardcore mode, then disable interaction with anything in this tab
        if (!_config.hardcoreMode) { ImGui.BeginDisabled(); }
        try {
            using var child = ImRaii.Child("##HC_RestrictionsChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
            if (!child)
                return;
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
            var name = $"{_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}";
            var yourName = "";
            if(_client.LocalPlayer == null) { yourName = "You"; }
            else { yourName = $"{_client.LocalPlayer.Name.ToString().Split(' ')[0]}"; }
            // show header
            ImGui.PushFont(_fontService.UidFont);
            try{
                ImGuiUtil.Center($"Restriction Permissions for {name}");
                if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"This determines what you are allowing {name} to be able to control you to do.\n"+
                "You are NOT controlling what you can do to them");
                }
            } finally { ImGui.PopFont(); }

            ImGui.Separator();
            // draw out the options
            UIHelpers.CheckboxNoConfig($"{name} can blindfold you.",
            $"Whenever {name} wants, they can blindfold you. (Triggers are NOT case sensative)",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._allowBlindfold,
            v => _hcManager.SetAllowBlindfold(_charHandler.activeListIdx, v)
            );
            ImGui.SameLine();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.RightAlign((_hcManager._perPlayerConfigs[_charHandler.activeListIdx]._blindfolded ? FontAwesomeIcon.UserCheck : FontAwesomeIcon.UserTimes).ToIconString());
            }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"Shows if the current option is currently active for your player, or inactive");
            }
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            try { ImGui.TextWrapped($"Whenever {name} wants, they can blindfold you."); } finally { ImGui.PopStyleColor(); }
            // get the blindfold item (temp)
            EquipDrawData blindfoldItem = _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._blindfoldItem;
            // draw out title and slot selection
            using (var group = ImRaii.Group()) {
                var blindfoldType = (int)_config.blindfoldType;
                ImGui.SetNextItemWidth(125);
                if(ImGui.Combo("##BlindfoldType", ref blindfoldType, new string[] { "Light", "Sensual" }, 2)) {
                    // Update the selected slot when the combo box selection changes
                    _config.blindfoldType = (BlindfoldType)blindfoldType;
                    _config.Save();
                    _blindfoldWindow.ChangeBlindfoldType(_config.blindfoldType);
                }
                // draw out the blindfold selection
                ImGui.SetNextItemWidth(125);
                if(ImGui.Combo("##BlindfoldEquipSlotDD", ref blindfoldItem._activeSlotListIdx,
                EquipSlotExtensions.EqdpSlots.Select(slot => slot.ToName()).ToArray(), EquipSlotExtensions.EqdpSlots.Count)) {
                    // Update the selected slot when the combo box selection changes
                    blindfoldItem.SetDrawDataSlot(EquipSlotExtensions.EqdpSlots[blindfoldItem._activeSlotListIdx]);
                    blindfoldItem.ResetDrawDataGameItem();
                    _hcManager.Save();
                }
            }
            // draw the combo
            ImGui.SameLine();
            DrawEquip(blindfoldItem, _gameItemCombo, _stainCombo, _stainData, _comboLength);
            ImGui.SameLine();
            blindfoldItem._gameItem.DrawIcon(_textures, _iconSize, blindfoldItem._slot);


            ImGui.Separator();    
            UIHelpers.CheckboxNoConfig($"Allow {name} to lock you away.",
            $"Allow {name} to lock you away in a private chamber, estate, or other location. (Triggers are NOT case sensative)",
            _hcManager._perPlayerConfigs[_charHandler.activeListIdx]._allowForcedToStay,
            v => _hcManager.SetAllowForcedToStay(_charHandler.activeListIdx, v));
            ImGui.SameLine();
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.RightAlign((_hcManager._perPlayerConfigs[_charHandler.activeListIdx]._forcedToStay ? FontAwesomeIcon.UserCheck : FontAwesomeIcon.UserTimes).ToIconString());
            }
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
                $"Shows if the current option is currently active for your player, or inactive");
            }
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            try {
            ImGui.TextWrapped($"Activation: If {name} says, \"{yourName}, stay here until I return.\"\n"+
            $"Deactivation: When {name} says \"thank you for waiting, {yourName}.\"\n"+
            $"Restrictions applied: Leaving Estates & Private Chambers, teleporting, and return become blocked.");
            } finally { ImGui.PopStyleColor(); }
            // we will want to display additional features here
            try {
                DrawForcedStayConfigUI();
            } catch (System.Exception e) {
                GSLogger.LogType.Error($"Error drawing forced stay config UI, {e.Message}");
            }
            ImGui.Separator();
        } finally {
            if (!_config.hardcoreMode) { ImGui.EndDisabled(); }
        }
    }


    public void DrawEquip(EquipDrawData blindfoldItem, GameItemCombo[] _gameItemCombo, StainColorCombo _stainCombo, DictStain _stainData, float _comboLength) {
        using var id      = ImRaii.PushId((int)blindfoldItem._slot + "BlindfoldItem");
        var       spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemSpacing.Y };
        using var style   = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        var right = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        var left  = ImGui.IsItemClicked(ImGuiMouseButton.Left);

        using var group = ImRaii.Group();
        DrawItem(blindfoldItem, out var label, right, left, _comboLength, _gameItemCombo);
        DrawStain(blindfoldItem, _comboLength, _stainCombo, _stainData);
    }
    private void DrawItem(EquipDrawData blindfoldItem, out string label,bool clear, bool open, float width, GameItemCombo[] _gameItemCombo) {
        // draw the item combo.
        var combo = _gameItemCombo[blindfoldItem._slot.ToIndex()];
        label = combo.Label;
        if (!blindfoldItem._locked && open) { UIHelpers.OpenCombo($"##BlindfoldItem{combo.Label}"); }
        // draw the combo
        using var disabled = ImRaii.Disabled(blindfoldItem._locked);
        var change = combo.Draw(blindfoldItem._gameItem.Name, blindfoldItem._gameItem.ItemId, width, width);
        // conditionals to detect for changes in the combo's
        if (change && !blindfoldItem._gameItem.Equals(combo.CurrentSelection)) {
            blindfoldItem.SetDrawDataGameItem(combo.CurrentSelection);
            _hcManager.Save();
        }
        if (clear || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            blindfoldItem.ResetDrawDataGameItem();
            _hcManager.Save();
            GSLogger.LogType.Debug($"[Hardcore Blindfold] Right Click processed, item reverted to none!");
        }
    }
    private void DrawStain(EquipDrawData blindfoldItem, float width, StainColorCombo _stainCombo, DictStain _stainData) {
        // fetch the correct stain from the stain data
        var       found    = _stainData.TryGetValue(blindfoldItem._gameStain, out var stain);
        using var disabled = ImRaii.Disabled(blindfoldItem._locked);
        // draw the stain combo
        if (_stainCombo.Draw($"##BlindfoldItemStain-{blindfoldItem._slot}", stain.RgbaColor, stain.Name, found, stain.Gloss, width)) {
            if (_stainData.TryGetValue(_stainCombo.CurrentSelection.Key, out stain)) {
                blindfoldItem.SetDrawDataGameStain(stain.RowIndex);
                _hcManager.Save();
                GSLogger.LogType.Debug($"[Hardcore Blindfold] Stain Changed: {stain.RowIndex}");
            }
            else if (_stainCombo.CurrentSelection.Key == Penumbra.GameData.Structs.Stain.None.RowIndex) { }
        }
        // conditionals to detect for changes in the combo's via reset
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
            blindfoldItem.ResetDrawDataGameStain();
            _hcManager.Save();
            GSLogger.LogType.Debug($"[Hardcore Blindfold] Right Click processed, stain reverted to none!");
        }
    }


    // Draw out the forced to stay options
    public void DrawForcedStayConfigUI() {
        using var child = ImRaii.Child("##HC_OrdersChild", new Vector2(ImGui.GetContentRegionAvail().X, (_hcManager.StoredEntriesFolder.Children.Count + 1)* ImGui.GetFrameHeight()), false, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // spacing
        DisplayTextButtons();
        ImGui.Spacing();
        // draw the text list
        DisplayTextNodes();
    }

    private void DisplayTextButtons() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        try {
            if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.SearchPlus.ToIconString(), new Vector2(ImGuiHelpers.GlobalScale*25, ImGuiHelpers.GlobalScale*25),
            "Add last seen as new entry interface as last entry\n(Must have active to record latest dialog option.)\n(Auto-selecting yes is not an allowed option)", false, true))
            {
                var newNode = new TextEntryNode() {
                    Enabled = true,
                    Label = _hcManager.LastSeenDialogText.Item1 + "-Label",
                    Text = _hcManager.LastSeenDialogText.Item1,
                    Options = _hcManager.LastSeenDialogText.Item2.ToArray(),
                };
                // if the list only has two elements
                if (_hcManager.StoredEntriesFolder.Children.Count <= 6) {
                    // add it to the end
                    _hcManager.StoredEntriesFolder.Children.Add(newNode);
                } else {
                    // it has more than two elements, so insert it one before the last element
                    _hcManager.StoredEntriesFolder.Children.Insert(_hcManager.StoredEntriesFolder.Children.Count - 1, newNode);
                }
                _hcManager.Save();
            }
            ImGui.SameLine();
            ImGuiUtil.DrawDisabledButton("Blockers List", new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GlobalScale*25), "", true);
        } finally { style.Pop(); }
    }





    private void DisplayTextNodes() {
        if (_hcManager.StoredEntriesFolder.Children.Count == 0) {
            _hcManager.StoredEntriesFolder.Children.Add(new TextEntryNode() {
                Enabled = false,
                Text = "NodeName",
                Label = "Placeholder Node, Add Last Selected Entry for proper node."
            });
            _hcManager.Save();
        }
        // if the list only has two elements (the required ones)
        if (_hcManager.StoredEntriesFolder.Children.Count <= 6) {
            // add it to the end
            _hcManager.StoredEntriesFolder.Children.Add(new TextEntryNode() {
                Enabled = false,
                Text = "NodeName",
                Label = "Placeholder Node, Add Last Selected Entry for proper node."
            });
            _hcManager.Save();
        }

        foreach (var node in _hcManager.StoredEntriesFolder.Children.ToArray())  {
            DisplayTextNode(node);
        }
    }
    private void DisplayTextNode(ITextNode node) {
        if (node is TextEntryNode textNode)
            DisplayTextEntryNode(textNode);
    }

    private void DisplayTextEntryNode(TextEntryNode node) {
        if (node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 1f, 1f));
        if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));

        ImGui.TreeNodeEx($"[{node.Text}] {node.Label}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        ImGui.PopStyleColor();

        if (ImGui.IsItemHovered()) {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) {
                node.Enabled = !node.Enabled;
                _hcManager.Save();
                return;
            }
            else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) {
                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        var disableElements = false;
        if(_hcManager.StoredEntriesFolder.Children.Count >= 6
        && (_hcManager.StoredEntriesFolder.Children[0] == node
         || _hcManager.StoredEntriesFolder.Children[1] == node
         || _hcManager.StoredEntriesFolder.Children[2] == node
         || _hcManager.StoredEntriesFolder.Children[3] == node
         || _hcManager.StoredEntriesFolder.Children[4] == node
         || _hcManager.StoredEntriesFolder.Children[5] == node))
        {
            disableElements = true;
        }
        TextNodePopup(node, disableElements);
    }

    private void TextNodePopup(TextEntryNode node, bool disableElements = false) {
        var style = ImGui.GetStyle();
        var newItemSpacing = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        if (ImGui.BeginPopup($"{node.GetHashCode()}-popup")) {
            if (node is TextEntryNode entryNode) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);
                try{
                    var enabled = entryNode.Enabled;
                    if(disableElements) { ImGui.BeginDisabled();}
                    try {
                        if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.TrashAlt.ToIconString(), new Vector2(),
                        "Delete Custom Addition", false, true))
                        {
                            if (_hcManager.TryFindParent(node, out var parentNode)) {
                                parentNode!.Children.Remove(node);
                                // if the new size is now just 2 contents
                                if (parentNode.Children.Count == 0) {
                                    // create a new blank one
                                    parentNode.Children.Add(new TextEntryNode() {
                                        Enabled = false,
                                        Text = "NodeName (Placeholder Node)",
                                        Label = "Add Last Selected Entry for proper node."
                                    });
                                }
                                _hcManager.Save();
                            }
                        }
                    } finally { if(disableElements) { ImGui.EndDisabled();} }
                    // Define the options for the dropdown menu
                    // Define the options for the dropdown menu
                    string[] options = entryNode.Options.ToArray(); // Use the node's options list
                    int currentOption = entryNode.SelectThisIndex; // Set the current option based on the SelectThisIndex property

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*200);
                    // Create the dropdown menu
                    if(disableElements) { ImGui.BeginDisabled();}
                    try {
                        if (ImGui.Combo("##Options", ref currentOption, options, options.Length)) {
                            // Update the IsYes property based on the selected option
                            entryNode.SelectThisIndex = currentOption;
                            // the list of options contains the entry "Yes"
                            if (options[currentOption] == "Yes") {
                                // select a different option within bounds
                                if (currentOption + 1 < options.Length) {
                                    entryNode.SelectThisIndex = currentOption + 1;
                                } else {
                                    entryNode.SelectThisIndex = 0;
                                }
                            }
                            _hcManager.Save();
                        }
                        if(ImGui.IsItemHovered()) { ImGui.SetTooltip( "The option to automatically select. Yes is always disabled"); }
                    } finally { if(disableElements) { ImGui.EndDisabled();} }

                    ImGui.SameLine();
                    if(disableElements) { ImGui.BeginDisabled();}
                    try {
                        if (ImGui.Checkbox("Enabled", ref enabled)) {
                            entryNode.Enabled = enabled;
                            _hcManager.Save();
                        }
                    } finally { if(disableElements) { ImGui.EndDisabled();} }
                    // draw the text input
                    if(disableElements) { ImGui.BeginDisabled();}
                    try {
                        var matchText = entryNode.Text;
                        if(entryNode.Text != "") { ImGui.BeginDisabled();}
                        try{
                            ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*225);
                            if (ImGui.InputText($"Node Name##{node.Name}-matchTextLebel", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue)) {
                                entryNode.Label = matchText;
                                _hcManager.Save();
                            }
                        } finally { if(entryNode.Text != "") { ImGui.EndDisabled();} }
                        var matchText2 = entryNode.Label;
                        ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*225);
                        if (ImGui.InputText($"Node Label##{node.Name}-matchText", ref matchText2, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue)) {
                            entryNode.Label = matchText2;
                            _hcManager.Save();
                        }
                    } finally { if(disableElements) { ImGui.EndDisabled();} }
                } 
                finally {
                    ImGui.PopStyleVar();
                }
            }
            ImGui.EndPopup();
        }
    }
}