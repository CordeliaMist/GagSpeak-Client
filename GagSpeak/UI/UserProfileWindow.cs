﻿
using System;
using System.Numerics;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Internal;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using OtterGui;
using GagSpeak.CharacterData;
using System.Runtime.CompilerServices;

namespace GagSpeak.UI;
/// <summary> 
/// <para>This class is used to handle the user profile window.</para>
/// <para>I wont be commenting this as it is purely just free handing a visual apperance and serves no signifigance to the larger scale of the plugin.</para>
/// </summary>
public class UserProfileWindow : Window, IDisposable
{
    private readonly IDalamudTextureWrap _dalamudTextureWrap;
    private readonly GagSpeakConfig     _config;
    private readonly CharacterHandler   _characterHandler;
    private readonly UiBuilder          _uiBuilder;
    public           int                _profileIndexOfUserSelected { get; set;}

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileWindow"/> class.
    /// <list type="bullet">
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>uiBuilder</c><param name="uiBuilder"> - The UI builder.</param></item>
    /// <item><c>pluginInterface</c><param name="pluginInterface"> - The DalamudPluginInterface.</param></item>
    /// </list> </summary>
    public UserProfileWindow(GagSpeakConfig config, CharacterHandler characterhandler,
    UiBuilder uiBuilder, DalamudPluginInterface pluginInterface)
    : base(GetLabel()) {
        // Set the readonlys
        _config = config;
        _characterHandler = characterhandler;
        _uiBuilder = uiBuilder;
        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "ReallyHeavyGag.png");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        GagSpeak.Log.Debug($"[Profile Popout]: Loaded mini-profile image sucessfully");

        _dalamudTextureWrap = IconImage;
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(250, 325),     // Minimum size of the window
            MaximumSize = new Vector2(300, 375) // Maximum size of the window
        };
        // add flags that allow you to move, but not resize the window, also disable collapsible
        Flags = ImGuiWindowFlags.NoCollapse;
        _config = config;
    }

    /// <summary> This function is used to draw the user profile window. </summary>
    public override void Draw() {
        // otherwise draw the content
        try
        {
            ImGui.Columns(2,"ProfileTop", false);
            // set column 0 to width of 62
            ImGui.SetColumnWidth(0, 62);
            ImGui.Image(_dalamudTextureWrap.ImGuiHandle, new Vector2(60, 60));
            ImGui.NextColumn();
            var whitelistPlayerData = _characterHandler.whitelistChars[_profileIndexOfUserSelected];
            ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.2f, 0.9f), $"Player: {whitelistPlayerData._name}");
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1.0f), $"World: {whitelistPlayerData._homeworld}");
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1.0f), $"Lean: {whitelistPlayerData._theirStatusToYou})");
            ImGui.NextColumn(); ImGui.Columns(1); ImGui.Separator();
            if(whitelistPlayerData._yourStatusToThem == RoleLean.None) {
                ImGui.Text($"Relation To Character: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.4f, 1.0f), $"{whitelistPlayerData._yourStatusToThem}");
            } else {
                ImGui.Text($"You are {whitelistPlayerData._name.Split(' ')[0]}'s: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.2f, 0.4f, 1.0f), $"{whitelistPlayerData._yourStatusToThem}");
            }
            if(whitelistPlayerData._theirStatusToYou == RoleLean.None) {
                ImGui.Text($"Their Relation to You: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.4f, 1.0f), $"{whitelistPlayerData._theirStatusToYou}");
            } else {
                ImGui.Text($"{whitelistPlayerData._name.Split(' ')[0]} is your: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.2f, 0.4f, 1.0f), $"{whitelistPlayerData._theirStatusToYou}");
            }
            if(whitelistPlayerData._yourStatusToThem != RoleLean.None && whitelistPlayerData._theirStatusToYou != RoleLean.None) {
                ImGui.Text($"Commited for: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.2f, 1.0f), $"{whitelistPlayerData.GetCommitmentDuration()}");
            }
            ImGui.Separator();
            // create a table with 3 columns
            DrawGagTabs();
        }
        catch {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching profile information");
            ImGui.NewLine();
        }
    }

    /// <summary> This function is used to draw the gaglisting tabs. </summary>
    public void DrawGagTabs() {
        using var _ = ImRaii.PushId( "ProfileGagListingInfo" );
        using var tabBar = ImRaii.TabBar( "Tabs");
        if( !tabBar ) return;

        if( ImGui.BeginTabItem( "Layer One" ) ) {
            DrawLayerOneInfo();
            ImGui.EndTabItem();
        }
        if( ImGui.BeginTabItem( "Layer Two" ) ) {
            DrawLayerTwoInfo();
            ImGui.EndTabItem();
        }
        if( ImGui.BeginTabItem( "Layer Three" ) ) {
            DrawLayerThreeInfo();
            ImGui.EndTabItem();
        }
    }

    public void DrawLayerOneInfo() {
        using var child2 = ImRaii.Child( "LayerOneInfo" );
        using (var table2 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table2)
                return;
            ImGui.TableSetupColumn("Info Piece", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X/3);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Type: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagTypes[0]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Padlock: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagPadlocks[0]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Time Left: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected].GetPadlockTimerDurationLeft(0)}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Assigner: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagPadlockAssigner[0]}");
        }
    }

    public void DrawLayerTwoInfo() {
        using var child3 = ImRaii.Child( "LayerTwoInfo" );
        using (var table3 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table3)
                return;
            ImGui.TableSetupColumn("Info Piece", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X/3);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Type: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagTypes[1]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Padlock: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagPadlocks[1]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Time Left: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected].GetPadlockTimerDurationLeft(1)}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Assigner: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagPadlockAssigner[1]}");
        }
    }

    public void DrawLayerThreeInfo() {
        using var child4 = ImRaii.Child( "LayerThreeInfo" );
        using (var table4 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table4)
                return;
            ImGui.TableSetupColumn("Info Piece", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X/3);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Type: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagTypes[2]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Padlock: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagPadlocks[2]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Time Left: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected].GetPadlockTimerDurationLeft(2)}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Assigner: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_characterHandler.whitelistChars[_profileIndexOfUserSelected]._selectedGagPadlockAssigner[2]}");
        }
    }

    public void Dispose() {
        _dalamudTextureWrap.Dispose();
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakProfileViewer###GagSpeakProfileViewer"; 
}