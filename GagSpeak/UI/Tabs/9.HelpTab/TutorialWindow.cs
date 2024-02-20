using System;
using System.IO;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using ImGuiNET;
using OtterGui.Raii;

namespace GagSpeak.UI.Tabs.HelpPageTab;

public enum TutorialTabType {
    General,
    WhitelistOverview,
    WhitelistGeneral,
    WhitelistGags,
    WhitelistWardrobe,
    WhitelistPuppeteer,
    WhitelistToybox,
    GagStorage,
    RestraintSets,
    Puppeteer,
    Toybox,
    Workshop,
    Settings,
    DynamicTiers,
}

public class TutorialWindow : Window
{
    IDalamudTextureWrap[] textureWraps = new IDalamudTextureWrap[14]; // for image display
    public TutorialTabType SelectedTutorialTab { get; set; }
    private readonly UiBuilder _uiBuilder;
    public bool isShown { get; set; }
    string[] imageNames = {
        "Help_General.jpg", "Help_Whitelist_Overview.jpg", "Help_Whitelist_Subtab1.jpg", 
        "Help_Whitelist_Subtab2.jpg", "Help_Whitelist_Subtab3.jpg", "Help_Whitelist_Subtab4.jpg", 
        "Help_Whitelist_Subtab5.jpg", "Help_Wardrobe_GagStorage.jpg", "Help_Wardrobe_RestraintSets.jpg",
        "Help_Puppeteer.jpg", "Help_Toybox_Overview.jpg", "Help_Toybox_Workshop.jpg", "Help_Settings.jpg",
        "Help_DynamicTierSystem.jpg"
    };

    string[] tabNames = {
        "General", "Whitelist Overview", "Whitelist General", "Whitelist Gag-Interactions", 
        "Whitelist Wardrobe", "Whitelist Puppeteer", "Whitelist Toybox", "Wardrobe GagStorage", 
        "Wardrobe RestraintSets", "Puppeteer", "Toybox Overview", "Toybox Workshop", "Settings", "Dynamic Tier System"
    };

    TutorialTabType[] tabTypes = {
        TutorialTabType.General, TutorialTabType.WhitelistOverview, TutorialTabType.WhitelistGeneral, 
        TutorialTabType.WhitelistGags, TutorialTabType.WhitelistWardrobe, TutorialTabType.WhitelistPuppeteer, 
        TutorialTabType.WhitelistToybox, TutorialTabType.GagStorage, TutorialTabType.RestraintSets, 
        TutorialTabType.Puppeteer, TutorialTabType.Toybox, TutorialTabType.Workshop, 
        TutorialTabType.Settings, TutorialTabType.DynamicTiers
    };

    public TutorialWindow(UiBuilder uiBuilder, DalamudPluginInterface pluginInterface) : base(GetLabel()) {
        _uiBuilder = uiBuilder;
        // determine if the pop out window is shown
        IsOpen = false;
        
        // if the display size is too small, set the minimum size
        if(ImGui.GetIO().DisplaySize.X < 1920+200 || ImGui.GetIO().DisplaySize.Y < 1080+200) {
            SizeConstraints = new WindowSizeConstraints() {
                MinimumSize = new Vector2(600, 480),
                MaximumSize = ImGui.GetIO().DisplaySize,
            };
        }
        // otherwise, we can load the full image to the screen
        else {
            SizeConstraints = new WindowSizeConstraints() {
                MinimumSize = new Vector2(600, 400),
                MaximumSize = ImGui.GetIO().DisplaySize,
            };
        }
        // Load the images
        for (int i = 0; i < imageNames.Length; i++) {
            var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, imageNames[i]);
            textureWraps[i] = _uiBuilder.LoadImage(imagePath);
        }
    }

    public override void Draw() {
        // Draw the child grouping for the ConfigSettings Tab
        using (var child2 = ImRaii.Child("TutorialChild")){
            DrawHelpPage();
        }
    }

    private void DrawHelpPage() {
        // Lets start by drawing the child.
        using var child = ImRaii.Child("##HelpPagePanel", -Vector2.One, true);
        // define our spacing
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw the content help tabs
        DrawHelpTabs();
    }

    public void DrawHelpTabs() {
        using var _ = ImRaii.PushId( "TutorialList" );
        using var tabBar = ImRaii.TabBar( "TutorialTabs" );
        if( !tabBar ) return;
       // Draw the tabs
        for (int i = 0; i < tabNames.Length; i++) {
            if (ImGui.BeginTabItem(tabNames[i])) { 
                SelectedTutorialTab = tabTypes[i]; 
                ImGui.EndTabItem(); 
            }
        }
        // draw the selected tab
        switch (SelectedTutorialTab) {
            case TutorialTabType.General:           DrawGeneralTab(); break;
            case TutorialTabType.WhitelistOverview: DrawWhitelistOverviewTab(); break;
            case TutorialTabType.WhitelistGeneral:  DrawWhitelistGeneralTab(); break;
            case TutorialTabType.WhitelistGags:     DrawWhitelistGagsTab(); break;
            case TutorialTabType.WhitelistWardrobe: DrawWhitelistWardrobeTab(); break;
            case TutorialTabType.WhitelistPuppeteer:DrawWhitelistPuppeteerTab(); break;
            case TutorialTabType.WhitelistToybox:   DrawWhitelistToyboxTab(); break;
            case TutorialTabType.GagStorage:        DrawGagStorageTab(); break;
            case TutorialTabType.RestraintSets:     DrawRestraintSetsTab(); break;
            case TutorialTabType.Puppeteer:         DrawPuppeteerTab(); break;
            case TutorialTabType.Toybox:            DrawToyboxTab(); break;
            case TutorialTabType.Workshop:          DrawWorkshopTab(); break;
            case TutorialTabType.Settings:          DrawSettingsTab(); break;
            case TutorialTabType.DynamicTiers:      DrawDynamicTiersTab(); break;
        }
    }
    private void DrawGeneralTab() {
        ImGui.BeginChild("##tutorialTabInfo0", new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[0].ImGuiHandle, new Vector2(textureWraps[0].Width, textureWraps[0].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWhitelistOverviewTab() {
        ImGui.BeginChild("##tutorialTabInfo1",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[1].ImGuiHandle, new Vector2(textureWraps[1].Width, textureWraps[1].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWhitelistGeneralTab() {
        ImGui.BeginChild("##tutorialTabInfo2",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[2].ImGuiHandle, new Vector2(textureWraps[2].Width, textureWraps[2].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWhitelistGagsTab() {
        ImGui.BeginChild("##tutorialTabInfo3",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[3].ImGuiHandle, new Vector2(textureWraps[3].Width, textureWraps[3].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWhitelistWardrobeTab() {
        ImGui.BeginChild("##tutorialTabInfo4",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[4].ImGuiHandle, new Vector2(textureWraps[4].Width, textureWraps[4].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWhitelistPuppeteerTab() {
        ImGui.BeginChild("##tutorialTabInfo5",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[5].ImGuiHandle, new Vector2(textureWraps[5].Width, textureWraps[5].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWhitelistToyboxTab() {
        ImGui.BeginChild("##tutorialTabInfo6",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[6].ImGuiHandle, new Vector2(textureWraps[6].Width, textureWraps[6].Height));
        }
        ImGui.EndChild();
    }

    private void DrawGagStorageTab() {
        ImGui.BeginChild("##tutorialTabInfo7",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[7].ImGuiHandle, new Vector2(textureWraps[7].Width, textureWraps[7].Height));
        }
        ImGui.EndChild();
    }

    private void DrawRestraintSetsTab() {
        ImGui.BeginChild("##tutorialTabInfo8",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[8].ImGuiHandle, new Vector2(textureWraps[8].Width, textureWraps[8].Height));
        }
        ImGui.EndChild();
    }

    private void DrawPuppeteerTab() {
        ImGui.BeginChild("##tutorialTabInfo9",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[9].ImGuiHandle, new Vector2(textureWraps[9].Width, textureWraps[9].Height));
        }
        ImGui.EndChild();
    }

    private void DrawToyboxTab() {
        ImGui.BeginChild("##tutorialTabInfo10",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[10].ImGuiHandle, new Vector2(textureWraps[10].Width, textureWraps[10].Height));
        }
        ImGui.EndChild();
    }

    private void DrawWorkshopTab() {
        ImGui.BeginChild("##tutorialTabInfo11",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[11].ImGuiHandle, new Vector2(textureWraps[11].Width, textureWraps[11].Height));
        }
        ImGui.EndChild();
    }

    private void DrawSettingsTab() {
        ImGui.BeginChild("##tutorialTabInfo12",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[12].ImGuiHandle, new Vector2(textureWraps[12].Width, textureWraps[12].Height));
        }
        ImGui.EndChild();
    }

    private void DrawDynamicTiersTab() {
        ImGui.BeginChild("##tutorialTabInfo13",  new Vector2(-1, -1), true, ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        {
            ImGui.Image(textureWraps[13].ImGuiHandle, new Vector2(textureWraps[13].Width, textureWraps[13].Height));
        }
        ImGui.EndChild();
    }
    private static string GetLabel() => "GagSpeakTutorialWindow###GagSpeakTutorialWindow";
}