using System;
using System.Numerics;
using System.IO;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using Dalamud.Plugin;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using GagSpeak.Data;
using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.GagListings;

namespace GagSpeak.UI.Tabs.ConfigSettingsTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class ConfigSettingsTab : ITab
{
    private readonly IDalamudTextureWrap    _dalamudTextureWrap;
    private readonly GagSpeakConfig         _config;
    private readonly UiBuilder              _uiBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigSettingsTab"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>uiBuilder</c><param name="uiBuilder"> - The UiBuilder.</param></item>
    /// <item><c>pluginInterface</c><param name="pluginInterface"> - The DalamudPluginInterface.</param></item>
    /// </list> </summary>
    public ConfigSettingsTab(GagSpeakConfig config, UiBuilder uiBuilder, DalamudPluginInterface pluginInterface,
    GagListingsDrawer gagListingsDrawer, GagService gagService) {
        _config = config;
        _uiBuilder = uiBuilder;
        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "icon.png");
        GagSpeak.Log.Debug($"[Image Display]: Loading image from {imagePath}");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        GagSpeak.Log.Debug($"[Image Display]: Loaded image from {imagePath}");

        _dalamudTextureWrap = IconImage;
    }

    public ReadOnlySpan<byte> Label => "Settings"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("MainWindowChild");
        if (!child)
            return;

        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("SettingsChild")) {
            DrawHeader();
            DrawConfigSettings();
        }
    }

    /// <summary> This Function draws the header for the ConfigSettings Tab </summary>
    private void DrawHeader()
        => WindowHeader.Draw("Configuration & Settings", 0, ImGui.GetColorU32(ImGuiCol.FrameBg));

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawConfigSettings() {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##SettingsPanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        ImGui.Columns(2,"ConfigColumns", false);
        // See "setpanel.cs" for other UIHelpers.Checkbox options that base off the above ^^
        ImGui.Text("Gag Configuration:");
        // UIHelpers.Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        UIHelpers.Checkbox("Only Friends", "Only processes process /gag (target) commands from others if they are on your friend list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.friendsOnly, v => _config.friendsOnly = v, _config);
        // UIHelpers.Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        UIHelpers.Checkbox("Only Party Members", "Only processes /gag (target) commands from others if they are in your current party list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.partyOnly, v => _config.partyOnly = v, _config);
        // UIHelpers.Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        UIHelpers.Checkbox("Only Whitelist", "Only processes /gag (target) commands from others if they are in your whitelist.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.whitelistOnly, v => _config.whitelistOnly = v, _config);
        // only allow UIHelpers.Checkbox interaction iif _config.LockDirectChatGarbler is false
        if(_config.LockDirectChatGarbler == true) {ImGui.BeginDisabled();}
        UIHelpers.Checkbox("DirectChatGarblerMode", "Automatically translate chat messages in all enabled channels to gagspeak automatically. (WITHOUT commands).\n" +
            "This does make use of chat to server interception. Even though now it is ensured safe, always turn this OFF after any patch or game update, until the plug curator says it's safe",
            _config.DirectChatGarbler, v => _config.DirectChatGarbler = v, _config);
        if(_config.LockDirectChatGarbler == true) {ImGui.EndDisabled();}
        
        ImGui.NextColumn();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 50);
        // you might normally want to embed resources and load them from the manifest stream
        ImGui.Image(_dalamudTextureWrap.ImGuiHandle, new Vector2(_dalamudTextureWrap.Width, _dalamudTextureWrap.Height));
        ImGui.Columns(1);

        // Show Debug Menu when Debug logging is enabled
        if(_config.LockDirectChatGarbler == true) {ImGui.BeginDisabled();}
        ImGui.Text("Enabled Channels:"); ImGui.Separator();
        var i = 0;
        foreach (var e in ChatChannel.GetOrderedChannels()) {
            // See if it is already enabled by default
            var enabled = _config.Channels.Contains(e);
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
                if (enabled) _config.Channels.Add(e);
                else _config.Channels.Remove(e);
            }

            ImGui.SameLine();
            i++;
        }
        // Set the columns back to 1 now and space over to next section
        ImGui.Columns(1);
        ImGui.PopStyleVar();
        if(_config.LockDirectChatGarbler == true) {ImGui.EndDisabled();}
    }
}
