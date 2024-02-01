using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Events;
using Dalamud.Plugin.Services;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.Chat;
using GagSpeak.UI.UserProfile;
namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPermissionEditor {
    private readonly    InteractOrPermButtonEvent   _interactOrPermButtonEvent;
    private readonly    GagSpeakConfig              _config;
    private readonly    IClientState                _clientState;
    private readonly    IChatGui                    _chatGui;                   // for interacting with the chatbox
    private readonly    IDataManager                _dataManager;
    // for button message sending
    private readonly    MessageEncoder              _messageEncoder;            // for encoding messages to send
    private readonly    ChatManager                 _chatManager;               // for managing the chat
    private readonly    UserProfileWindow           _userProfileWindow;
    public TabType SelectedSubTab;
    public WhitelistPermissionEditor(InteractOrPermButtonEvent interactOrPermButtonEvent, GagSpeakConfig config, IClientState clientState, 
    IChatGui chatGui, IDataManager dataManager, MessageEncoder messageEncoder, ChatManager chatManager, UserProfileWindow userProfileWindow) {
        _interactOrPermButtonEvent = interactOrPermButtonEvent;
        _config = config;
        _clientState = clientState;
        _chatGui = chatGui;
        _dataManager = dataManager;

        _messageEncoder = messageEncoder;
        _chatManager = chatManager;
        _userProfileWindow = userProfileWindow;
    }

    /// <summary> This function is used to draw the content of the tab. </summary>
    public void Draw(ref int currentWhitelistItem) {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##HelpPagePanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw the content help tabs
        DrawPermissionTabs();
        DrawBody(currentWhitelistItem);
    }

    public void DrawPermissionTabs() {
        using var _ = ImRaii.PushId( "WhitelistPermissionEditTabList" );
        using var tabBar = ImRaii.TabBar( "PermissionEditorTabBar" );
        if( !tabBar ) return;

        // select the tab to view this persons current permissions
        if (ImGui.BeginTabItem("Overview")) {
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
        if (ImGui.BeginTabItem("Override Options")) {
            SelectedSubTab = TabType.ConfigSettings;
            ImGui.EndTabItem();
        }
    }

    public void DrawBody(int currentWhitelistItem) {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
        // determine which permissions we will draw out
        switch (SelectedSubTab) {
            case TabType.General: 
                DrawGeneralPerms(currentWhitelistItem);
                break;
            case TabType.Wardrobe:
                DrawWardrobePerms(currentWhitelistItem);
                break;
            case TabType.Puppeteer:
                DrawPuppeteerPerms(currentWhitelistItem);
                break;
            case TabType.Toybox:
                DrawToyboxPerms(currentWhitelistItem);
                break;
            case TabType.ConfigSettings:
                DrawSettingOverridePerms(currentWhitelistItem);
                break;
        }
    }
}