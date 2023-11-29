using System;
using System.Numerics;
using System.IO;
using ImGuiNET;
using OtterGui;
using OtterGui.Raii;
using OtterGui.Widgets;
using Dalamud.Plugin;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using GagSpeak.Data;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.GagListings;

namespace GagSpeak.UI.Tabs.ConfigSettingsTab;
/// <summary> This class is used to handle the ConfigSettings Tab. </summary>
public class ConfigSettingsTab : ITab
{
    private readonly IDalamudTextureWrap    _dalamudTextureWrap;
    private readonly GagListingsDrawer      _gagListingsDrawer;
    private readonly GagSpeakConfig         _config;
    private readonly UiBuilder              _uiBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigSettingsTab"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>uiBuilder</c><param name="uiBuilder"> - The UiBuilder.</param></item>
    /// <item><c>pluginInterface</c><param name="pluginInterface"> - The DalamudPluginInterface.</param></item>
    /// </list> </summary>
    public ConfigSettingsTab(GagSpeakConfig config, UiBuilder uiBuilder, DalamudPluginInterface pluginInterface, GagListingsDrawer gagListingsDrawer) {
        _config = config;
        _uiBuilder = uiBuilder;
        _gagListingsDrawer = gagListingsDrawer;
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
        // See "setpanel.cs" for other checkbox options that base off the above ^^
        ImGui.Text("Gag Configuration:");
        // Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        Checkbox("Only Friends", "Only processes process /gag (target) commands from others if they are on your friend list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.friendsOnly, v => _config.friendsOnly = v);
        // Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        Checkbox("Only Party Members", "Only processes /gag (target) commands from others if they are in your current party list.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.partyOnly, v => _config.partyOnly = v);
        // Checkbox will dictate if only players from their friend list are allowed to use /gag (target) commands on them.
        Checkbox("Only Whitelist", "Only processes /gag (target) commands from others if they are in your whitelist.\n" +
            "(Does NOT need to be enabled for you to use /gag (target) commands on them)", _config.whitelistOnly, v => _config.whitelistOnly = v);
        // only allow checkbox interaction iif _config.LockDirectChatGarbler is false
        if(_config.LockDirectChatGarbler == true) {ImGui.BeginDisabled();}
        Checkbox("DirectChatGarblerMode", "Automatically translate chat messages in all enabled channels to gagspeak automatically. (WITHOUT commands).\n" +
            "This does make use of chat to server interception. Even though now it is ensured safe, always turn this OFF after any patch or game update, until the plug curator says it's safe",
            _config.DirectChatGarbler, v => _config.DirectChatGarbler = v);
        if(_config.LockDirectChatGarbler == true) {ImGui.EndDisabled();}
        // Checkbox to display debug information
        Checkbox("Debug Display", "Displays information for plugin variables. For developer", _config.DebugMode, v => _config.DebugMode = v);
        // Checkbox will dictate if only players from their party are allowed to use /gag (target) commands on them.
        ImGui.NextColumn();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 50);
        // you might normally want to embed resources and load them from the manifest stream
        ImGui.Image(_dalamudTextureWrap.ImGuiHandle, new Vector2(_dalamudTextureWrap.Width, _dalamudTextureWrap.Height));
        ImGui.Columns(1);

        // Show Debug Menu when Debug logging is enabled
        if (_config.DebugMode)
            DrawDebug();
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
                // See If the checkbox is clicked, If not, add to the list of enabled channels, otherwise, remove it.
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

    /// <summary>
    /// This function draws a checkbox with a label and tooltip, and saves the value to the config.
    /// <list type="bullet">
    /// <item><c>label</c><param name="label"> - The label to display outside the checkbox</param></item>
    /// <item><c>tooltip</c><param name="tooltip"> - The tooltip to display when hovering over the checkbox</param></item>
    /// <item><c>current</c><param name="current"> - The current value of the checkbox</param></item>
    /// <item><c>setter</c><param name="setter"> - The setter for the checkbox</param></item>
    /// </list>
    /// </summary>
    private void Checkbox(string label, string tooltip, bool current, Action<bool> setter) {
        using var id  = ImRaii.PushId(label);
        var       tmp = current;
        if (ImGui.Checkbox(string.Empty, ref tmp) && tmp != current) {
            setter(tmp);
            _config.Save();
        }

        ImGui.SameLine();
        ImGuiUtil.LabeledHelpMarker(label, tooltip);
    }

    /// <summary>
    /// This just literally displays extra information for debugging variables in game to keep track of them.
    /// </summary>
    private void DrawDebug() {
        ImGui.Text("DEBUG INFORMATION:");
        try
        {
            ImGui.Text($"Fresh Install?: {_config.FreshInstall} || Is Enabled?: {_config.Enabled} || In Dom Mode?: {_config.InDomMode}");
            ImGui.Text($"Debug Mode?: {_config.DebugMode} || In DirectChatGarbler Mode?: {_config.DirectChatGarbler}");
            ImGui.Text($"Safeword: {_config.Safeword}");
            ImGui.Text($"Friends Only?: {_config.friendsOnly} || Party Only?: {_config.partyOnly} || Whitelist Only?: {_config.whitelistOnly}");
            ImGui.Text($"Garble Level: {_config.GarbleLevel}");
            ImGui.Text($"Process Translation Interval: {_config.ProcessTranslationInterval} || Max Translation History: {_config.TranslationHistoryMax}");
            ImGui.Text($"Total Gag List Count: {_config.GagTypes.Count}");
            ImGui.Text("Selected GagTypes: ||"); ImGui.SameLine(); foreach (var gagType in _config.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
            ImGui.Text("Selected GagPadlocks: ||"); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in _config.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} ||");};
            ImGui.Text("Selected GagPadlocks Passwords: ||"); ImGui.SameLine(); foreach (var gagPadlockPassword in _config.selectedGagPadlocksPassword) { ImGui.SameLine(); ImGui.Text($"{gagPadlockPassword} ||"); };
            ImGui.Text("Selected GagPadlock Timers: ||"); ImGui.SameLine(); foreach (var gagPadlockTimer in _config.selectedGagPadLockTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} ||"); };
            ImGui.Text("Selected GagPadlocks Assigners: ||"); ImGui.SameLine(); foreach (var gagPadlockAssigner in _config.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} ||"); };
            ImGui.Text($"Translatable Chat Types:");
            foreach (var chanel in _config.Channels) { ImGui.SameLine(); ImGui.Text(chanel.ToString()); };
            ImGui.Text($"Current ChatBox Channel: {ChatChannel.GetChatChannel()} || Requesting Info: {_config.SendInfoName} || Accepting?: {_config.acceptingInfoRequests}");
            ImGui.Text("Whitelist:"); ImGui.Indent();
            foreach (var whitelistPlayerData in _config.Whitelist) {
                ImGui.Text(whitelistPlayerData.name);
                ImGui.Indent();
                ImGui.Text($"Relationship to this Player: {whitelistPlayerData.relationshipStatus}");
                ImGui.Text($"Commitment Duration: {whitelistPlayerData.GetCommitmentDuration()}");
                ImGui.Text($"Locked Live Chat Garbler: {whitelistPlayerData.lockedLiveChatGarbler}");
                ImGui.Text($"Pending Relationship Request: {whitelistPlayerData.PendingRelationRequestFromPlayer}");
                ImGui.Text($"Pending Relationship Request From You: {whitelistPlayerData.PendingRelationRequestFromYou}");
                ImGui.Text($"Selected GagTypes: || "); ImGui.SameLine(); foreach (var gagType in whitelistPlayerData.selectedGagTypes) { ImGui.SameLine(); ImGui.Text(gagType); };
                ImGui.Text($"Selected GagPadlocks: || "); ImGui.SameLine(); foreach (GagPadlocks gagPadlock in whitelistPlayerData.selectedGagPadlocks) { ImGui.SameLine(); ImGui.Text($"{gagPadlock.ToString()} || ");};
                ImGui.Text($"Selected GagPadlocks Timers: || "); ImGui.SameLine(); foreach (var gagPadlockTimer in whitelistPlayerData.selectedGagPadlocksTimer) { ImGui.SameLine(); ImGui.Text($"{UIHelpers.FormatTimeSpan(gagPadlockTimer - DateTimeOffset.Now)} || "); };
                ImGui.Text($"Selected GagPadlocks Assigners: || "); ImGui.SameLine(); foreach (var gagPadlockAssigner in whitelistPlayerData.selectedGagPadlocksAssigner) { ImGui.SameLine(); ImGui.Text($"{gagPadlockAssigner} || "); };
                ImGui.Unindent();
            }
            ImGui.Unindent();
            ImGui.NewLine();
            ImGui.Text("Padlock Identifiers Variables:");
            // output debug messages to display the gaglistingdrawers boolean list for _islocked, _adjustDisp. For each padlock identifer, diplay all of its public varaibles
            ImGui.Text($"Listing Drawer _isLocked: ||"); ImGui.SameLine(); foreach(var index in _config._isLocked) { ImGui.SameLine(); ImGui.Text($"{index} ||"); };
            ImGui.Text($"Listing Drawer _adjustDisp: ||"); ImGui.SameLine(); foreach(var index in _gagListingsDrawer._adjustDisp) { ImGui.SameLine(); ImGui.Text($"{index} ||"); };
            var width = ImGui.GetContentRegionAvail().X / 3;
            foreach(var index in _config._padlockIdentifier) {
                ImGui.Columns(3,"DebugColumns", true);
                ImGui.SetColumnWidth(0,width); ImGui.SetColumnWidth(1,width); ImGui.SetColumnWidth(2,width);
                ImGui.Text($"Input Password: {index._inputPassword}"); ImGui.NextColumn();
                ImGui.Text($"Input Combination: {index._inputCombination}"); ImGui.NextColumn();
                ImGui.Text($"Input Timer: {index._inputTimer}");ImGui.NextColumn();
                ImGui.Text($"Stored Password: {index._storedPassword}");ImGui.NextColumn();
                ImGui.Text($"Stored Combination: {index._storedCombination}");ImGui.NextColumn();
                ImGui.Text($"Stored Timer: {index._storedTimer}");ImGui.NextColumn();
                ImGui.Text($"Padlock Type: {index._padlockType}");ImGui.NextColumn();
                ImGui.Text($"Padlock Assigner: {index._mistressAssignerName}");ImGui.NextColumn();
                ImGui.Columns(1);
                ImGui.NewLine();
            }
            ImGui.Columns(3,"DebugColumns", true);
            ImGui.SetColumnWidth(0,width); ImGui.SetColumnWidth(1,width); ImGui.SetColumnWidth(2,width);
            ImGui.Text($"Input Password: {_config._whitelistPadlockIdentifier._inputPassword}"); ImGui.NextColumn();
            ImGui.Text($"Input Combination: {_config._whitelistPadlockIdentifier._inputCombination}"); ImGui.NextColumn();
            ImGui.Text($"Input Timer: {_config._whitelistPadlockIdentifier._inputTimer}");ImGui.NextColumn();
            ImGui.Text($"Stored Password: {_config._whitelistPadlockIdentifier._storedPassword}");ImGui.NextColumn();
            ImGui.Text($"Stored Combination: {_config._whitelistPadlockIdentifier._storedCombination}");ImGui.NextColumn();
            ImGui.Text($"Stored Timer: {_config._whitelistPadlockIdentifier._storedTimer}");ImGui.NextColumn();
            ImGui.Text($"Padlock Type: {_config._whitelistPadlockIdentifier._padlockType}");ImGui.NextColumn();
            ImGui.Text($"Padlock Assigner: {_config._whitelistPadlockIdentifier._mistressAssignerName}");ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.NewLine();   
        }
        catch (Exception e)
        {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching config in debug: {e}");
            ImGui.NewLine();
        }
    }
}
