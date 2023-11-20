﻿
using System;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Utility.Raii;


namespace GagSpeak.UI.UserProfile;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class UserProfileWindow : Window, IDisposable
{
    private readonly IDalamudTextureWrap _dalamudTextureWrap;
    private readonly GagSpeakConfig _config;
    private readonly UiBuilder _uiBuilder;

    public int _profileIndexOfUserSelected { get; set;}
    public Vector2 mainWindowPosition { get; set; }

    public UserProfileWindow(GagSpeakConfig config, UiBuilder uiBuilder,
        DalamudPluginInterface pluginInterface): base(GetLabel()) {
    {
        // Set the readonlys
        _config = config;
        _uiBuilder = uiBuilder;
        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "ReallyHeavyGag.png");
        GagSpeak.Log.Debug($"[Profile Popout]: Loading image from {imagePath}");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        GagSpeak.Log.Debug($"[Profile Popout]: Loaded image from {imagePath}");

        _dalamudTextureWrap = IconImage;
    }
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(250, 350),     // Minimum size of the window
            MaximumSize = new Vector2(250, 350) // Maximum size of the window
        };
        // add flags that allow you to move, but not resize the window, also disable collapsible
        Flags = ImGuiWindowFlags.NoCollapse;
        Flags |= ImGuiWindowFlags.NoResize;
        _config = config;
    }

    public override void Draw() {
        // otherwise draw the content
        try
        {
            ImGui.Columns(2,"ProfileTop", false);
            // set column 0 to width of 62
            ImGui.SetColumnWidth(0, 62);
            ImGui.Image(_dalamudTextureWrap.ImGuiHandle, new Vector2(60, 60));
            ImGui.NextColumn();
            var whitelistPlayerData = _config.Whitelist[_profileIndexOfUserSelected];
            ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.2f, 0.9f), $"Player: {whitelistPlayerData.name}");
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1.0f), $"World: {whitelistPlayerData.homeworld}");
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1.0f), $"Lean: {(whitelistPlayerData.isDomMode ? "Dominant" : "Submissive")}");
            ImGui.NextColumn(); ImGui.Columns(1); ImGui.Separator();
            if(whitelistPlayerData.relationshipStatus == "None" || whitelistPlayerData.relationshipStatus == "") {
                ImGui.Text($"Player Relation: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.4f, 1.0f), $"{whitelistPlayerData.relationshipStatus}");
            } else {
                ImGui.Text($"Player Is Your "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.2f, 0.4f, 1.0f), $"{whitelistPlayerData.relationshipStatus}");
                ImGui.Text($"Commited for: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.2f, 1.0f), $"{whitelistPlayerData.GetCommitmentDuration()}");
            }
            ImGui.Text($"Gag Strength: "); ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.4f, 1.0f), $"{whitelistPlayerData.garbleLevel}");
            ImGui.Separator();
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
            // Set the window background color
            // append style to show headers for table
            using (var table = ImRaii.Table("GagInfoTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders)) {
                if (!table) { return; } // make sure our table was made

                var width = ImGui.GetContentRegionAvail().X/3;
                ImGui.TableSetupColumn("LayerOne", ImGuiTableColumnFlags.WidthFixed, width);
                ImGui.TableSetupColumn("LayerTwo", ImGuiTableColumnFlags.WidthFixed, width);
                ImGui.TableSetupColumn("LayerThree", ImGuiTableColumnFlags.WidthFixed, width);

                // Create the rows for the table
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.Text("Layer1"); ImGui.TableNextColumn(); ImGui.Text("Layer2"); ImGui.TableNextColumn(); ImGui.Text("Layer3");
                ImGui.TableNextRow(); ImGui.TableNextColumn();

                // display gag information
                ImGui.Text($"{whitelistPlayerData.selectedGagTypes[0]}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagTypes[1]}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagTypes[2]}"); ImGui.TableNextColumn();
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagPadlocks[0]}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagPadlocks[1]}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagPadlocks[2]}"); ImGui.TableNextColumn();
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.GetPadlockTimerDurationLeft(0)}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.GetPadlockTimerDurationLeft(1)}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.GetPadlockTimerDurationLeft(2)}"); ImGui.TableNextColumn();
                ImGui.TableNextRow(); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagPadlocksAssigner[0]}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagPadlocksAssigner[1]}"); ImGui.TableNextColumn();
                ImGui.Text($"{whitelistPlayerData.selectedGagPadlocksAssigner[2]}"); ImGui.TableNextColumn();
                // Restore the original style
            }
            ImGui.Columns(1);
        }
        catch {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching profile information");
            ImGui.NewLine();
        }
    }

    public void Dispose() {
        _dalamudTextureWrap.Dispose();
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakProfileViewer###GagSpeakProfileViewer"; 
}

#pragma warning restore IDE1006