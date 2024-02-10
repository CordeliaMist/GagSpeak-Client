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

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPlayerPermissions {
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
    private readonly    ChatManager                 _chatManager;
    private readonly    UserProfileWindow           _userProfileWindow;
    public              TabType                     SelectedSubTab;
    public WhitelistPlayerPermissions(InteractOrPermButtonEvent interactOrPermButtonEvent, GagSpeakConfig config,
    CharacterHandler characterHandler, IClientState clientState, IChatGui chatGui, IDataManager dataManager,
    MessageEncoder messageEncoder, ChatManager chatManager, UserProfileWindow userProfileWindow, 
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

        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;
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
    }

    /// <summary> This function is used to draw the content of the tab. </summary>
    public void Draw(Action<bool> setInteractions, Action<bool> setViewMode, ref bool _enableInteractions, ref bool _viewMode) {
        // Lets start by drawing the child.
        using (_ = ImRaii.Group()) {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        DrawPermissionsHeader(setInteractions, setViewMode, _enableInteractions, _viewMode);
        // make content disabled
        if(!_enableInteractions) { ImGui.BeginDisabled(); }
        DrawPlayerPermissions(ref _viewMode);
        DrawPlayerPermissionsButtons();
        if(!_enableInteractions) { ImGui.EndDisabled(); }
        }
    }

    // draw the header
    private void DrawPermissionsHeader(Action<bool> setInteractions, Action<bool> setViewMode, bool _enableInteractions, bool _viewMode) {
        WindowHeader.Draw($"Status & Interactions for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}",
        0, ImGui.GetColorU32(ImGuiCol.FrameBg), 1, 0, InteractionsButton(setInteractions, _enableInteractions), ViewModeButton(setViewMode, _viewMode));
    }

    private void DrawPlayerPermissions(ref bool _viewMode) {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        using var child = ImRaii.Child("##WhitelistPlayerPermissions", new Vector2(ImGui.GetContentRegionAvail().X, -ImGui.GetFrameHeight()), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // draw ourcontent
        DrawOverview(); // Draw the overview of the permissions
        var xPosition = ImGui.GetCursorPosX();
        var yPosition = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPosition, yPosition + 5*ImGuiHelpers.GlobalScale)); 
        DrawPermissionTabs(); // draws the tabs for the sub component permissions
        DrawBody(ref _viewMode); // draws out the body for each tab of permissions
        
    }

    private void DrawPlayerPermissionsButtons() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, 0);
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0);
        // add a button to display it
        if (ImGui.Button("Mini-Profile", buttonWidth)) {
            // Get the currently selected user
            var selectedUser = _characterHandler.whitelistChars[_characterHandler.activeListIdx];
            // Check if the UserProfileWindow is already open
            _userProfileWindow.Toggle();
        }

        // draw the relationship removal
        ImGui.SameLine();
        if(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem == RoleLean.None) {
            ImGui.BeginDisabled();
            if (ImGui.Button("Remove Relation With Player##RemoveOne", buttonWidth2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                RequestRelationRemovalToPlayer();
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            }
            ImGui.EndDisabled();
        } else {
            if (ImGui.Button("Remove Relation With Player##RemoveTwo", buttonWidth2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                RequestRelationRemovalToPlayer();
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
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
            _config.SetSendInfoName(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name +
            "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld);
            _config.SetAcceptInfoRequests(false);
            // Start a 5-second cooldown timer
            _interactOrPermButtonEvent.Invoke();
        }
    }

#region Header Button Stuff
    private WindowHeader.Button InteractionsButton(Action<bool> setInteractions, bool _enableInteractions)
        => !_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)
            ? WindowHeader.Button.Invisible
            : _enableInteractions
                ? new WindowHeader.Button {
                    Description = "Disable interactions.",
                    Icon = FontAwesomeIcon.LockOpen,
                    OnClick = () => setInteractions(false),
                    Visible = true,
                    Disabled = false,
                }
                : new WindowHeader.Button {
                    Description = "Enable interactions.",
                    Icon = FontAwesomeIcon.Lock,
                    OnClick = () => setInteractions(true),
                    Visible = true,
                    Disabled = false,
                };

    // get our view mode buttons list
    private readonly FontAwesomeIcon[] _viewModeIcons = {
        FontAwesomeIcon.PersonArrowDownToLine,
        FontAwesomeIcon.PersonArrowUpFromLine,
    };
    private WindowHeader.Button ViewModeButton(Action<bool> setViewMode, bool _viewMode)
        => !_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)
            ? WindowHeader.Button.Invisible
            : _viewMode
                ? new WindowHeader.Button {
                    Description = "View your Settings.",
                    Icon = FontAwesomeIcon.PersonArrowDownToLine,
                    OnClick = () => setViewMode(false),
                    Visible = true,
                    Disabled = false,
                }
                : new WindowHeader.Button {
                    Description = "View Their settings interactions.",
                    Icon = FontAwesomeIcon.PersonArrowUpFromLine,
                    OnClick = () => setViewMode(true),
                    Visible = true,
                    Disabled = false,
                };
#endregion Header Button Stuff

    public void DrawPermissionTabs() {
        using var _ = ImRaii.PushId( "WhitelistPermissionEditTabList" );
        using var tabBar = ImRaii.TabBar( "PermissionEditorTabBar" );
        if( !tabBar ) return;

        if (ImGui.BeginTabItem("Overview")) {
            SelectedSubTab = TabType.ConfigSettings;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Gags")) {
            SelectedSubTab = TabType.General;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Warbrobe")) {
            SelectedSubTab = TabType.Wardrobe;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Puppeteer")) {
            SelectedSubTab = TabType.Puppeteer;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Toybox")) {
            SelectedSubTab = TabType.Toybox;
            ImGui.EndTabItem();
        }
    }

    public void DrawBody(ref bool _viewMode) {
        // determine which permissions we will draw out
        switch (SelectedSubTab) {
            case TabType.ConfigSettings:
                DrawOverviewPerms(ref _viewMode);
                break;
            case TabType.General:
                DrawGagInteractions(ref _viewMode);
                break;
            case TabType.Wardrobe:
                DrawWardrobePerms(ref _viewMode);
                break;
            case TabType.Puppeteer:
                DrawPuppeteerPerms(ref _viewMode);
                break;
            case TabType.Toybox:
                DrawToyboxPerms(ref _viewMode);
                break;
        }
    }
}