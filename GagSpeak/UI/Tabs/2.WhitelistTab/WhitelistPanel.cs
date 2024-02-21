using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Events;
using Dalamud.Plugin.Services;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.ChatMessages;
using GagSpeak.CharacterData;
using Dalamud.Interface;
using System;
using GagSpeak.Services;
using Dalamud.Interface.Utility;
using GagSpeak.UI.Equipment;
using GagSpeak.UI.Tabs.GeneralTab;
using OtterGui;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPanel {
    private readonly    InteractOrPermButtonEvent   _interactOrPermButtonEvent;
    private readonly    GagSpeakConfig              _config;
    private readonly    CharacterHandler            _characterHandler;
    private readonly    GagService                  _gagService;
    private readonly    GagListingsDrawer           _gagListingsDrawer;
    private readonly    FontService                 _fontService;
    private readonly    IClientState                _clientState;
    private readonly    IChatGui                    _chatGui;
    private readonly    IDataManager                _dataManager;
    private readonly    MessageEncoder              _messageEncoder;
    private readonly    OnChatMsgManager            _chatManager;
    private readonly    UserProfileWindow           _userProfileWindow;
    // helpers for making code easier to read
    private int                         _tempWhitelistIdx;
    private WhitelistedCharacterInfo    _tempWhitelistChar;
    private WhitelistPanelTab           _activePanelTab;

    public WhitelistPanel(InteractOrPermButtonEvent interactOrPermButtonEvent, GagSpeakConfig config,
    CharacterHandler characterHandler, IClientState clientState, IChatGui chatGui, IDataManager dataManager,
    MessageEncoder messageEncoder, OnChatMsgManager chatManager, UserProfileWindow userProfileWindow, 
    FontService fontService, GagService gagService, GagListingsDrawer gagListingsDrawer) {
        _interactOrPermButtonEvent = interactOrPermButtonEvent;
        _config = config;
        _characterHandler = characterHandler;
        _clientState = clientState;
        _chatGui = chatGui;
        _dataManager = dataManager;
        _messageEncoder = messageEncoder;
        _chatManager = chatManager;
        _userProfileWindow = userProfileWindow;
        _fontService = fontService;
        _gagService = gagService;
        _gagListingsDrawer = gagListingsDrawer;
        // set our variables to defaults (temp ones too)
        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;
        _tempWhitelistIdx = _characterHandler.activeListIdx;
        _tempWhitelistChar = _characterHandler.whitelistChars[_tempWhitelistIdx];
        _activePanelTab = WhitelistPanelTab.Overview;
        // draw out our gagtype filter combo listings
        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_gagService),
            new GagTypeFilterCombo(_gagService),
            new GagTypeFilterCombo(_gagService)
        };
        // draw out our gagpadlock filter combo listings
        _gagLockFilterCombo = new GagLockFilterCombo[] {
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config)
        };
        // initialize the tooltips
        InitializeToolTips();
    }

    public void Draw(ref bool _interactions)
    {
        // update temp vars for easier code if a change occurs
        if(_tempWhitelistIdx != _characterHandler.activeListIdx) {
            _tempWhitelistIdx = _characterHandler.activeListIdx;
            _tempWhitelistChar = _characterHandler.whitelistChars[_tempWhitelistIdx];
        }
        // draw out the panel
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        using (var group = ImRaii.Group()) {
            DrawShelfSelection();
            if(!_interactions) { ImGui.BeginDisabled(); }
            try{
                DrawBody(ref _interactions);
                DrawPlayerPermissionsButtons();
            } finally {
                if(!_interactions) { ImGui.EndDisabled(); }
            }
        }
    }

    private void DrawBody(ref bool _interactions) {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        using var child = ImRaii.Child("##WhitelistPlayerPermissions", new Vector2(ImGui.GetContentRegionAvail().X, -ImGui.GetFrameHeight()), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // content here
        if(_activePanelTab == WhitelistPanelTab.Overview) {
            DrawOverview(ref _interactions);
        }
        else if(_activePanelTab == WhitelistPanelTab.TheirSettings) {
            DrawTheirSettings(ref _interactions);
        }
        else if(_activePanelTab == WhitelistPanelTab.YourSettings) {
            DrawYourSettings(ref _interactions);
        }
    }

    private void DrawShelfSelection() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero).Push(ImGuiStyleVar.FrameRounding, 0);
        // button size
        var width = ImGui.GetContentRegionAvail().X;
        var buttonSize1 = new Vector2(width*.2f, ImGui.GetFrameHeight());
        var buttonSize2 = new Vector2(width*.35f, ImGui.GetFrameHeight());
        var buttonSize3 = new Vector2(width*.45f, ImGui.GetFrameHeight());

        // tab selection
        if (ImGuiUtil.DrawDisabledButton("Overview", buttonSize1,
        $"View {_tempWhitelistChar._name.Split(' ')[0]}'s Basic Attributes & Settings", _activePanelTab == WhitelistPanelTab.Overview)) {
            _activePanelTab = WhitelistPanelTab.Overview;
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton($"{_tempWhitelistChar._name.Split(' ')[0]}'s Settings",
        buttonSize2,
        $"Inspect what settings {_tempWhitelistChar._name.Split(' ')[0]} has enabled for you.\n"+
        $"You can override their settings if you have a strong enough Dynamic Tier.\n"+
        "Otherwise, the toggle buttons will be disabled.\n"+
        "Look in the Overview Section of the Whitelist Tab to see how people aquire certain Tiers",

        _activePanelTab == WhitelistPanelTab.TheirSettings))
        {
            _activePanelTab = WhitelistPanelTab.TheirSettings;
        }
        ImGui.SameLine();
        if (ImGuiUtil.DrawDisabledButton($"Your Settings for {_tempWhitelistChar._name.Split(' ')[0]}",
        buttonSize3,
        $"Configure which permissions you want to give {_tempWhitelistChar._name.Split(' ')[0]} Access to.\n"+
        $"Any Settings you enable here, {_tempWhitelistChar._name.Split(' ')[0]} will be able to use.\n"+
        $"{_tempWhitelistChar._name.Split(' ')[0]} can override your Settings if they have a strong enough Dynamic Tier.\n"+
        "Look in the Overview Section of the Whitelist Tab to see how people aquire certain Tiers",
        _activePanelTab == WhitelistPanelTab.YourSettings))
        {
            _activePanelTab = WhitelistPanelTab.YourSettings;
        }
    }

    private void DrawPlayerPermissionsButtons() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, 0);
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0);
        // add a button to display it
        if (ImGui.Button("Mini-Profile", buttonWidth)) {
            // Get the currently selected user
            var selectedUser = _tempWhitelistChar;
            // Check if the UserProfileWindow is already open
            _userProfileWindow.Toggle();
        }
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip($"View a fucking adorable mini-profile window.\nThis window displays all information about {_tempWhitelistChar._name.Split(' ')[0]}'s active gags.");
        }

        // draw the relationship removal
        ImGui.SameLine();
        if(_tempWhitelistChar._yourStatusToThem == RoleLean.None) {
            ImGui.BeginDisabled();
            if (ImGui.Button("Remove Relation With Player##RemoveOne", buttonWidth2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                RequestRelationRemovalToPlayer();
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip($"Removes both ends of the dynamic relation with {_tempWhitelistChar._name.Split(' ')[0]}.");
            }
            ImGui.EndDisabled();
        } else {
            if (ImGui.Button("Remove Relation With Player##RemoveTwo", buttonWidth2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                RequestRelationRemovalToPlayer();
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            }
            if(ImGui.IsItemHovered()) {
                ImGui.SetTooltip($"Removes both ends of the dynamic relation with {_tempWhitelistChar._name.Split(' ')[0]}.");
            }
        } 

        // for requesting info
        ImGui.SameLine();
        if (ImGui.Button("Request Info", buttonWidth)) {
            // send a message to the player requesting their current info
            GagSpeak.Log.Debug("[Whitelist]: Sending Request for Player Info");
            InfoSendAndRequestHelpers.RequestInfoFromPlayer(_characterHandler.activeListIdx,
            _characterHandler, _chatManager, _messageEncoder, _clientState, _chatGui);
            // we need to set the sendInfoName to the player name @ world so we know who we are looking for when we start recieving info
            _config.SetSendInfoName(_tempWhitelistChar._name +
            "@" + _tempWhitelistChar._homeworld);
            _config.SetAcceptInfoRequests(false);
            // Start a 5-second cooldown timer
            _interactOrPermButtonEvent.Invoke(5);
        }
        if(ImGui.IsItemHovered()) {
            ImGui.SetTooltip($"Sends a request for information to {_tempWhitelistChar._name.Split(' ')[0]}.\n"+
            "This then makes them send back all of their information to you. This process can around 8seconds to process.\n\n"+
            "It is HIGHLY RECOMMENDED to use this whenever meet up with someone before you begin interacting.");
        }
    }
  
    public void DrawTheirSettings(ref bool _interactions) {
        // draw out the panel
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw out the collapsible tabs and their bodies
        ImGui.PushFont(_fontService.UidFont);
        try { ImGuiUtil.Center($"Inspect / Override {_tempWhitelistChar._name.Split(' ')[0]}'s Settings"); }
        finally { ImGui.PopFont(); }
        // draw the dropdowns
        var text = $"{_tempWhitelistChar._name.Split(' ')[0]}";
        var suffix = "'s";
        try{
            DrawDetailsOverview(ref _interactions, text, suffix);
            DrawDetailsGags(ref _interactions, text, suffix);
            DrawDetailsWardrobe(ref _interactions, text, suffix);
            DrawDetailsPuppeteer(ref _interactions, text, suffix);
            DrawDetailsToybox(ref _interactions, text, suffix);
            // DrawDetailsHardcore(ref _interactions);
        } finally {
            ImGui.PopStyleVar();
        }
    }

    public void DrawYourSettings(ref bool _interactions) {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 5*ImGuiHelpers.GlobalScale);
        // draw out the panel
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw out the collapsible tabs and their bodies
        ImGui.PushFont(_fontService.UidFont);
        try { ImGuiUtil.Center($"Set what {_tempWhitelistChar._name.Split(' ')[0]} can do to You"); }
        finally { ImGui.PopFont(); }
        // draw the warning
        if(_tempWhitelistChar._yourStatusToThem == RoleLean.None || _tempWhitelistChar._theirStatusToYou == RoleLean.None) {
            try{
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                ImGuiUtil.Center($"Before establishing a 2 way dynamic, make sure you have setup");
                ImGuiUtil.Center($"the options you want to grant {_tempWhitelistChar._name.Split(' ')[0]} Access to.");
                ImGuiUtil.Center("Doing so after will cause a lot of desync and not recommended!");
            } finally {
                ImGui.PopStyleColor();
            }
        }
        // draw the dropdowns
        var text = "You";
        var suffix = "r";
        // if our hardcord condition is fullfilled, begin disable
        // draw the dropdowns
        try{
            DrawDetailsOverview(ref _interactions, text, suffix);
            DrawDetailsWardrobe(ref _interactions, text, suffix);
            DrawDetailsPuppeteer(ref _interactions, text, suffix);
            DrawDetailsToybox(ref _interactions, text, suffix);
            // DrawDetailsHardcore(ref _interactions);
        } finally {
            ImGui.PopStyleVar();
        }
    }

    private void DrawDetailsOverview(ref bool _interactions, string text, string suffix) {
        var tooltipText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"View {text}{suffix} configured settings & general permissions, and which ones they have enabled for you."
            : $"View your own configured settings and general permissions.";
        if(!ImGui.CollapsingHeader($"{text}{suffix} General Info & Settings")) { if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); } return; }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
        // draw the overview
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }
        DrawOverviewPerms(ref _interactions, text, suffix);
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.EndDisabled(); }
    }
    private void DrawDetailsGags(ref bool _interactions, string text, string suffix) {
        var tooltipText = $"Apply, Lock, Unlock, or Remove both Gags & Padlocks to {text}.";
        if(!ImGui.CollapsingHeader($"{text}{suffix} Gag Interactions")) { 
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
            return;
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
        // draw the gags
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }
        DrawGagInteractions(ref _interactions);
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.EndDisabled(); }
    }
    private void DrawDetailsWardrobe(ref bool _interactions, string text, string suffix) {
        var tooltipText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"View which permissions {text} has set for you.\nAdditionally, you can enable/disable and lock restraint sets, given permission to do so is granted."
            : $"Configure what wardrobe spesific permissions {_tempWhitelistChar._name.Split(' ')[0]} will be able to use on you.";
        var headerText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"{text}{suffix} Wardrobe Permissions & Interactions"
            : $"{text}{suffix} Wardrobe Permissions for {_tempWhitelistChar._name.Split(' ')[0]}";
        if(!ImGui.CollapsingHeader($"{headerText}")) { 
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
            return;
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
        // draw the wardrobe
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }
        DrawWardrobePerms(ref _interactions, text, suffix);
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.EndDisabled(); }
    }
    private void DrawDetailsPuppeteer(ref bool _interactions, string text, string suffix) { 
        var tooltipText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"View which puppeteer permissions {text} has set for you.\n"+
              $"You can also view the trigger phrase they have set for you, and the custom start/end character brackets they have setup."
            : $"View what puppeteer permissions you have given {_tempWhitelistChar._name.Split(' ')[0]} access to execute with your trigger phrase.\n"+
              "Any permissions that cannot be toggled are readonly because they must be toggled in the puppeteer module.";
        var headerText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"{text}{suffix} Puppeteer Permissions & Interactions"
            : $"{text}{suffix} Puppeteer Permissions for {_tempWhitelistChar._name.Split(' ')[0]}";
        if(!ImGui.CollapsingHeader($"{headerText}")) { 
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
            return;
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }
        DrawPuppeteerPerms(ref _interactions, text, suffix);
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.EndDisabled(); }
    }
    private void DrawDetailsToybox(ref bool _interactions, string text, string suffix) { 
        var tooltipText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"View which toybox permissions {text} has set for you.\n"+
              $"You can also execute patterns and adjust their active toy's intensity if you have access to do so."
            : $"Configure what toybox permissions you wish to give {_tempWhitelistChar._name.Split(' ')[0]} access to.\n"+
              "Any permissions that cannot be toggled are readonly because they must be toggled in the toybox module.";
        
        var headerText = WhitelistPanelTab.TheirSettings == _activePanelTab
            ? $"{text}{suffix} Toybox Permissions & Interactions"
            : $"{text}{suffix} Toybox Permissions for {_tempWhitelistChar._name.Split(' ')[0]}";
        if(!ImGui.CollapsingHeader($"{headerText}")) { 
            if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
            return;
        }
        if(ImGui.IsItemHovered()) { ImGui.SetTooltip($"{tooltipText}"); }
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }
        DrawToyboxPerms(ref _interactions, text, suffix);
        if(_characterHandler.IsLeanLesserThanPartner(_tempWhitelistIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }
    }
}