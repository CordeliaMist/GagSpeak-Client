using System;
using System.Numerics;
using System.IO;
using ImGuiNET;
using OtterGui.Raii;
using OtterGui.Widgets;
using Dalamud.Plugin;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using System.Collections.Generic;
using System.Linq;
using Penumbra.GameData.Enums;
using Penumbra.GameData.Structs;
using GagSpeak.Data;
using GagSpeak.Interop;
using GagSpeak.Services;
using GagSpeak.UI.Helpers;
using GagSpeak.UI.ComboListings;
using System.Runtime.CompilerServices;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Dalamud.Interface.Utility;
using Penumbra.GameData.DataContainers;
using Penumbra.GameData.Data;
using Dalamud.Interface.Internal.Windows.StyleEditor;
using OtterGui;

namespace GagSpeak.UI.Tabs.WardrobeTab;

/// <summary> Stores the UI for the restraints shelf of the kink wardrobe. </summary>
public class WardrobeRestraintCompartment
{
    private readonly    IDalamudTextureWrap             _dalamudTextureWrap1;    // for loading images
    private readonly    IDalamudTextureWrap             _dalamudTextureWrap2;    // for loading images
    private readonly    IDalamudTextureWrap             _dalamudTextureWrap3;    // for loading images
    private readonly    IDalamudTextureWrap             _dalamudTextureWrap4;    // for loading images
    private readonly    UiBuilder                       _uiBuilder;             // for loading images
    private readonly    GagSpeakConfig                  _config;                // for getting the config
    private readonly    FontService                     _fontService;           // for getting the font service

    /// <summary> Initializes a new instance wardrobe tab"/> class. <summary>
    public WardrobeRestraintCompartment(GagSpeakConfig config, FontService fontService,
    DalamudPluginInterface pluginInterface, UiBuilder uiBuilder) {
        _config = config;
        _fontService = fontService;
        _uiBuilder = uiBuilder;
        // load the images
        var imagePath1 = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "RestraintCompartmentTease1.png");
        var imagePath2 = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "RestraintCompartmentTease2.png");
        var imagePath3 = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "RestraintCompartmentTease3.png");
        var imagePath4 = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "RestraintCompartmentTease4.png");
        var TeaserImage1 = _uiBuilder.LoadImage(imagePath1);
        var TeaserImage2 = _uiBuilder.LoadImage(imagePath2);
        var TeaserImage3 = _uiBuilder.LoadImage(imagePath3);
        var TeaserImage4 = _uiBuilder.LoadImage(imagePath4);
        _dalamudTextureWrap1 = TeaserImage1;
        _dalamudTextureWrap2 = TeaserImage2;
        _dalamudTextureWrap3 = TeaserImage3;
        _dalamudTextureWrap4 = TeaserImage4;
        // images loaded
    }

    public ReadOnlySpan<byte> Label => "WardrobeRestraintCompartment"u8; // apply the tab label

    /// <summary> This Function draws the content for the window of the ConfigSettings Tab </summary>
    public void DrawContent() {
        // Create a child for the Main Window (not sure if we need this without a left selection panel)
        using var child = ImRaii.Child("WardrobeRestraintCompartmentChild");
        if (!child)
            return;

        DrawWardrobeRestraintCompartmentUI();
    }

    /// <summary> This Function draws the content for the ConfigSettings Tab </summary>
    private void DrawWardrobeRestraintCompartmentUI() {
        ImGui.PushFont(_fontService.UidFont);
        ImGuiUtil.Center("Wardrobe Restraint Compartment");
        ImGuiUtil.Center("[COMING SOON]");
        ImGui.PopFont();
        // tease the restraint set
        ImGui.NewLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX());
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() -55);
        ImGui.Image(_dalamudTextureWrap1.ImGuiHandle, new Vector2(_dalamudTextureWrap1.Width, _dalamudTextureWrap1.Height));
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() +75);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() -75);
        ImGui.Image(_dalamudTextureWrap2.ImGuiHandle, new Vector2(_dalamudTextureWrap2.Width, _dalamudTextureWrap2.Height));
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() +260);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() -345);
        ImGui.Image(_dalamudTextureWrap3.ImGuiHandle, new Vector2(_dalamudTextureWrap3.Width, _dalamudTextureWrap3.Height));
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() +325);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() -155);
        ImGui.Image(_dalamudTextureWrap4.ImGuiHandle, new Vector2(_dalamudTextureWrap4.Width, _dalamudTextureWrap4.Height));
    }
}
