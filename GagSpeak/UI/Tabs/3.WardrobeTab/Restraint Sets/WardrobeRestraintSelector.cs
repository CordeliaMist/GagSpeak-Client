using System;
using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using System.Linq;
using GagSpeak.Wardrobe;
using GagSpeak.Gagsandlocks;
using OtterGui;
using Dalamud.Interface;
using GagSpeak.Services;
using GagSpeak.Utility;
using System.Collections.Generic;
using Newtonsoft.Json;
using GagSpeak.Interop;
using Dalamud.Interface.Utility;
using GagSpeak.Events;
using GagSpeak.Interop.Penumbra;

namespace GagSpeak.UI.Tabs.WardrobeTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class RestraintSetSelector : IDisposable
{
    private readonly    RestraintSetManager _restraintSetManager; // for getting the restraint sets
    private readonly    RS_ListChanged      _rsListChanged; // for getting the restraint set list changed event
    private readonly    ListCopier          _listCopier;          // for getting the list copier
    private             Vector2             _defaultItemSpacing;
    private             bool                _listNeedsUpdate = true;

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public RestraintSetSelector(RestraintSetManager restraintSetManager, TimerService timerService,
    RS_ListChanged restraintSetListChanged, ModAssociations modAssociations) {
        _restraintSetManager = restraintSetManager;
        _rsListChanged = restraintSetListChanged;
        _listCopier = new ListCopier(new List<string>());

        _rsListChanged.SetListModified += OnRestraintSetListChanged;
    }

    public void Dispose() {
        _rsListChanged.SetListModified -= OnRestraintSetListChanged;
    }

    public void Draw(float width) {
        _defaultItemSpacing = ImGui.GetStyle().ItemSpacing;
        DrawSelector(width);
    }

    private void DrawSelector(float width) {
        // group these
        using var group = ImRaii.Group();
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);

        DrawRestraintSetSelector(width, ImGui.GetContentRegionAvail().Y - 300*ImGuiHelpers.GlobalScale, _defaultItemSpacing);
        DrawSelectionButtons(width);
    }

#region  RestraintSetSelector
    public void DrawRestraintSetSelector(float width, float height, Vector2 ItemSpacing, bool borderAllowed = true) {
        using var child = ImRaii.Child("##Selector", new Vector2(width, height), borderAllowed, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;

        using var style     = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, ItemSpacing);
        var       skips     = OtterGui.ImGuiClip.GetNecessarySkips(ImGui.GetTextLineHeight());
        var       remainder = OtterGui.ImGuiClip.ClippedDraw(
                                    _restraintSetManager._restraintSets, skips, DrawSelectable);
        OtterGui.ImGuiClip.DrawEndDummy(remainder, ImGui.GetTextLineHeight());
    }

    private void DrawSelectable(RestraintSet restraintSet) {
        var equals = _restraintSetManager._selectedIdx == _restraintSetManager.GetRestraintSetIndex(restraintSet._name);
        if (ImGui.Selectable(restraintSet._name, equals) && !equals)
        {
            // update the selected index in our restraint set manager
            _restraintSetManager._selectedIdx = _restraintSetManager.GetRestraintSetIndex(restraintSet._name);
        }
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Right-Click this set to rename it!");
        };
        if (ImGui.BeginPopupContextItem(restraintSet._name + "Context"))
        {
            string currentText = restraintSet._name;
            string oldText = currentText;

            // Display input text field in context menu
            ImGui.Text("Rename Restraint Set:");
            if (ImGui.InputTextWithHint("##Rename", "Input new set name here...", ref currentText, 100, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                // If text is updated, update the name of the restraint set
                if (currentText != oldText) {
                    restraintSet._name = currentText;
                    _listNeedsUpdate = true;
                }
                // Close the context menu
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }


    private void DrawSelectionButtons(float width) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(((width / 2) - ImGui.GetTextLineHeight()), 0);

        if (ImGui.Button("Add Set", new Vector2(buttonWidth.X-ImGuiHelpers.GlobalScale*10, buttonWidth.Y))) {
            _restraintSetManager.AddNewRestraintSet();
            _listNeedsUpdate = true;
        }
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Append a new restraint set to the list here");
        };
        ImGui.SameLine();
        if(_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._enabled) { ImGui.BeginDisabled();}
        // draw out the remove button
        try{
            if (ImGui.Button("Remove Set", new Vector2(buttonWidth.X+ImGuiHelpers.GlobalScale*10, buttonWidth.Y))) {
                // if the set only has one item, just replace it with a blank template
                if (_restraintSetManager._restraintSets.Count == 1) {
                    _restraintSetManager._restraintSets[0] = new RestraintSet();
                    _restraintSetManager.SetSelectedIdx(0); // will also save
                    _listNeedsUpdate = true; // stupid primative solution to a larger problem
                } else {
                    _restraintSetManager.DeleteRestraintSet(_restraintSetManager._selectedIdx);
                    _restraintSetManager.SetSelectedIdx(0); // will also save
                    _listNeedsUpdate = true; // stupid primative solution to a larger problem
                }
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip("Remove the selected restraint set from the list");
            };
        } finally {
            if(_restraintSetManager._restraintSets[_restraintSetManager._selectedIdx]._enabled) { ImGui.EndDisabled();}
        }
        ImGui.SameLine();
        // copy restraint set list button
        if(ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Copy.ToIconString(), new Vector2(2*ImGui.GetTextLineHeight(),0),
        $"Copy Your Restraint Set List to the Clipboard", false, true)) {
            ImGui.OpenPopup("Copy Restraint Set List");
            // it should open the list copier here with the correct parameters and stuff
        }
        
        // update the list copier if we need to
        if(_restraintSetManager._restraintSets.Count != _listCopier._items.Count || _listNeedsUpdate) {
            _listCopier.UpdateListInfo(_restraintSetManager._restraintSets.Select(x => x._name).ToList());
            _listNeedsUpdate = false;
        }
        // list copier should draw the button here with the correct parameters
        _listCopier.DrawCopyButton("Copy Restraint Set List", "Copied Restraint Set List to clipboard",
        "Could not copy Restraint Set List to clipboard");
    }

#endregion RestraintSetSelector
    private void OnRestraintSetListChanged(object sender, RestraintSetListChangedArgs e) {
        if(e.UpdateType == ListUpdateType.NameChanged) {
            _listCopier.UpdateListInfo(_restraintSetManager._restraintSets.Select(x => x._name).ToList());
        }
    }
}