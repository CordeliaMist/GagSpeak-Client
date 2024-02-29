using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GagSpeak.CharacterData;
using GagSpeak.Hardcore;
using GagSpeak.Hardcore.Actions;
using GagSpeak.Services;
using GagSpeak.Utility;
using ImGuiNET;
using OtterGui;

namespace GagSpeak.UI.Tabs.HardcoreTab;

public class HC_Orders
{
    private readonly HardcoreManager _hcManager;
    private readonly GsActionManager _actionManager;
    private readonly CharacterHandler _charHandler;
    private readonly FontService _fontService;
    private readonly IClientState _client;
    public HC_Orders(HardcoreManager hardcoreManager, GsActionManager actionManager,
    CharacterHandler charHandler, FontService fontService, IClientState client) {
        _hcManager = hardcoreManager;
        _actionManager = actionManager;
        _charHandler = charHandler;
        _fontService = fontService;
        _client = client;
    }
    public void Draw() {
        using var child = ImRaii.Child("##HC_OrdersChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        var name = $"{_charHandler.whitelistChars[_charHandler.activeListIdx]._name.Split(' ')[0]}";
        var yourName = "";
        if(_client.LocalPlayer == null) { yourName = "You"; }
        else { yourName = $"{_client.LocalPlayer.Name.ToString().Split(' ')[0]}"; }
        // show header
        ImGui.PushFont(_fontService.UidFont);
        try{
            ImGuiUtil.Center($"Order Permissions for {name}");
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip(
            $"This determines what you are allowing {name} to be able to order you to do.\n"+
            "You are NOT controlling what you can do to them");
            }
        } finally { ImGui.PopFont(); }

        ImGui.Separator();
        // draw out the options
        UIHelpers.CheckboxNoConfig($"{name} can order you to follow them.",
        $"Automatically follow {name} when they say to you \"{yourName}, follow me.\" in any channel.",
        _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._allowForcedFollow,
        v => _hcManager.SetAllowForcedFollow(v));
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("", $"Enable the forced follow from {name}, over allow forced follow",
        _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedFollow,
        v => _hcManager.SetForcedFollow(v));
        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
        try {
            ImGui.TextWrapped($"Follow {name} when they say \"{yourName}, follow me.\" in any channel.\n"+
            $"Movement Input is blocked until your character stops moving for a set amount of time.");
        } finally { ImGui.PopStyleColor(); }

        ImGui.Separator();
        // draw out sit option
        UIHelpers.CheckboxNoConfig($"{name} can order you to sit.",
        $"You will be forcibily sat down on your nees when {name} says to you \"{yourName}, sit.\" in any channel.\n"+
        $"Your movement is restricted until they say \"you may stand now {yourName}.\"",
        _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._allowForcedSit,
        v => _hcManager.SetAllowForcedSit(v));
        ImGui.SameLine();
        UIHelpers.CheckboxNoConfig("", $"Enable the forced sit from {name}, over allow forced sit",
        _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedSit,
        v => _hcManager.SetForcedSit(v));
        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
        try {
            ImGui.TextWrapped($"When {name} says to you \"{yourName}, sit.\" in any channel. You will be forcibly sat down. "+
            $"Movement is restricted until they say \"you may stand now {yourName}.\"");
        } finally { ImGui.PopStyleColor(); }

        ImGui.Separator();
        // draw out stay here for now option
        using (var table = ImRaii.Table("StayHereSettingTable", 2, ImGuiTableFlags.BordersInnerV, new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GlobalScale*100))) {
            if (!table) { return; }
            // set up columns
            ImGui.TableSetupColumn("##StayHereSettingTableCol1", ImGuiTableColumnFlags.WidthFixed, ImGuiHelpers.GlobalScale*240);
            ImGui.TableSetupColumn("##StayHereSettingTableCol2", ImGuiTableColumnFlags.WidthStretch);
            // draw out the options
            ImGui.TableNextRow();
            ImGui.TableNextColumn();


            UIHelpers.CheckboxNoConfig($"Allow {name} to lock you away.",
            $"If {name} says to you, \"{yourName}, stay here until I return.\" your teleport & return actions become blocked. "+
            "You are also unable to leave any private chamblers or estates while this is active.\n"+
            $"Restrictions reverted when {name} says \"thank you for waiting, {yourName}.\"",
            _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._allowForcedToStay,
            v => _hcManager.SetAllowForcedToStay(v));
            UIHelpers.CheckboxNoConfig("", $"{name} Can lock you away.",
            _hcManager._perPlayerConfigs[_hcManager.ActivePlayerCfgListIdx]._forcedToStay,
            v => _hcManager.SetForcedToStay(v));
            ImGui.Spacing();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
            try {
                ImGui.TextWrapped($"If {name} says to you,\n"+
                $"\"{yourName}, stay here until I return.\"\n"+
                $"your teleport & return actions become blocked.\n"+
                $"You are also unable to leave any private chamblers or estates while this is active.\n"+
                $"Restrictions reverted when {name} says\n"+
                $"\"thank you for waiting, {yourName}.\"");
            } finally { ImGui.PopStyleColor(); }
            // we will want to display additional features here
            ImGui.TableNextColumn();
            try {
                DrawForcedStayConfigUI();
            } catch (System.Exception e) {
                GagSpeak.Log.Error($"Error drawing forced stay config UI, {e.Message}");
            }
        }
    }

    // Draw out the forced to stay options
    public void DrawForcedStayConfigUI() {
        using var child = ImRaii.Child("##HC_OrdersChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), false, ImGuiWindowFlags.NoScrollbar);
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
            "Add last seen as new entry interface as last entry", false, true))
            {
                GagSpeak.Log.Debug($"{_hcManager.LastSeenListSelection} || {_hcManager.LastSeenDialogText.Item1} || {_hcManager.LastSeenListTarget}");
                var newNode = new TextEntryNode() {
                    Enabled = true,
                    Text = _hcManager.LastSeenListSelection == "" ? _hcManager.LastSeenDialogText.Item1 : _hcManager.LastSeenListSelection,
                    Options = _hcManager.LastSeenDialogText.Item2.ToArray(),
                };
                // if the list only has two elements
                if (_hcManager.StoredEntriesFolder.Children.Count <= 2) {
                    // add it to the end
                    _hcManager.StoredEntriesFolder.Children.Add(newNode);
                } else {
                    // it has more than two elements, so insert it one before the last element
                    _hcManager.StoredEntriesFolder.Children.Insert(_hcManager.StoredEntriesFolder.Children.Count - 1, newNode);
                }
                _hcManager.Save();
            }
            ImGui.SameLine();
            ImGuiUtil.DrawDisabledButton("Blockers List", new Vector2(ImGui.GetContentRegionAvail().X, ImGuiHelpers.GlobalScale*25), "", false);
        } finally { style.Pop(); }
    }

    private void DisplayTextNodes() {
        if (_hcManager.StoredEntriesFolder.Children.Count == 0) {
            _hcManager.StoredEntriesFolder.Children.Add(new TextEntryNode() {
                Enabled = false,
                Text = "Add some text here!"
            });
            _hcManager.Save();
        }
        // if the list only has two elements (the required ones)
        if (_hcManager.StoredEntriesFolder.Children.Count <= 2) {
            // add it to the end
            _hcManager.StoredEntriesFolder.Children.Add(new TextEntryNode() {
                Enabled = false,
                Text = "Add some text here!"
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

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
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
        if(_hcManager.StoredEntriesFolder.Children.Count >= 2
        && _hcManager.StoredEntriesFolder.Children[0] == node || _hcManager.StoredEntriesFolder.Children[1] == node)
        {
            disableElements = true;
        }
        TextNodePopup(node, disableElements);
    }

    private void TextNodePopup(ITextNode node, bool disableElements = false) {
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
                                if (parentNode.Children.Count == 2) {
                                    // create a new blank one
                                    parentNode.Children.Add(new TextEntryNode() {
                                        Enabled = false,
                                        Text = "Add some text here!"
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
                    ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*125);
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
                        ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*225);
                        if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue)) {
                            entryNode.Text = matchText;
                            _hcManager.Save();
                        }
                    } finally { if(disableElements) { ImGui.EndDisabled();} }
                    ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale*225);
                } 
                finally {
                    ImGui.PopStyleVar();
                }
            }
            ImGui.EndPopup();
        }
    }
}