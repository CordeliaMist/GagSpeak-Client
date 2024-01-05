using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Interface.Utility;
using ImGuiNET;
using OtterGui.Widgets;
using GagSpeak.UI.Tabs.GeneralTab;
using GagSpeak.UI.Tabs.WhitelistTab;
using GagSpeak.UI.Tabs.ConfigSettingsTab;
using GagSpeak.UI.Tabs.HelpPageTab;

namespace GagSpeak.UI;
/// <summary> This class is used to handle the main window. </summary>
public class MainWindow : Window
{
    public enum TabType {
        None            = -1,   // No tab selected
        General         = 0,    // Where you select your gags and safewords and lock types. Put into own tab for future proofing beauty spam
        Whitelist       = 1,    // Where you can append peoples names to a whitelist, which is used to account for permissions on command usage.
        ConfigSettings  = 2,    // Where you can change the plugin settings, such as debug mode, and other things.
        HelpPage        = 3     // Where you can find information on how to use the plugin, and how to get support.
    }
    private readonly    GagSpeakConfig      _config;
    private readonly    GagSpeakChangelog   _changelog;
    private readonly    ITab[]              _tabs;
    public readonly     GeneralTab          General;
    public readonly     WhitelistTab        Whitelist;
    public readonly     ConfigSettingsTab   ConfigSettings;
    public readonly     HelpPageTab         HelpPage;
    public              TabType             SelectTab = TabType.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// <list type="bullet">
    /// <item><c>pluginInt</c><param name="pluginInt"> - The DalamudPluginInterface.</param></item>
    /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
    /// <item><c>general</c><param name="general"> - The general tab.</param></item>
    /// <item><c>changelog</c><param name="changelog"> - The changelog.</param></item>
    /// <item><c>whitelist</c><param name="whitelist"> - The whitelist tab.</param></item>
    /// <item><c>configsettings</c><param name="configsettings"> - The config settings tab.</param></item>
    /// <item><c>helpPageTab</c><param name="helpPageTab"> - The help page tab.</param></item>
    /// </list> </summary>
    public MainWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, GeneralTab general, GagSpeakChangelog changelog,
    WhitelistTab whitelist, ConfigSettingsTab configsettings, HelpPageTab helpPageTab): base(GetLabel()) {
        // let the user know if their direct chat garlber is still enabled upon launch
        // Let's first make sure that we disable the plugin while inside of gpose.
        pluginInt.UiBuilder.DisableGposeUiHide = true;

        // Next let's set the size of the window
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(500, 540),     // Minimum size of the window
            MaximumSize = new Vector2(550, 1000)     // Maximum size of the window
        };

        // set the private readonly's to the passed in data of the respective names
        General = general;
        Whitelist = whitelist;
        ConfigSettings = configsettings;
        HelpPage = helpPageTab;
        
        // Below are the stuff besides the tabs that are passed through
        //_event     = @event;
        _config    = config;
        _changelog = changelog;
        // the tabs to be displayed
        _tabs = new ITab[]
        {
            general,
            whitelist,
            configsettings,
            helpPageTab
        };
    }

    public override void Draw() {
        // get our cursors Y position and store it into YPOS
        var yPos = ImGui.GetCursorPosY();
        // set the cursor position to the top left of the window
        if (TabBar.Draw("##tabs", ImGuiTabBarFlags.None, ToLabel(SelectTab), out var currentTab, () => { }, _tabs)) {
            SelectTab           = TabType.None; // set the selected tab to none
            _config.SelectedTab = FromLabel(currentTab); // set the config selected tab to the current tab
            _config.Save(); // FIND OUT HOW TO USE SaveConfig(); ACROSS CLASSES LATER.
        }
        // We want to display the save & close, and the donation buttons on the topright, so lets draw those as well.
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - 9f * ImGui.GetFrameHeight(), yPos - ImGuiHelpers.GlobalScale));
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        ImGui.Text(" ");
        ImGui.SameLine();
        if (ImGui.Button("Changelog")) {
            // force open the changelog here
            _changelog.Changelog.ForceOpen = true;
        }
        // In that same line...
        ImGui.SameLine();
        // And now have that button be for the Ko-Fi Link
        if (ImGui.Button("Toss Cordy a thanks!")) {
            ImGui.SetTooltip( "Only if you want to though!");
            Process.Start(new ProcessStartInfo {FileName = "https://ko-fi.com/cordeliamist", UseShellExecute = true});
        }
        // pop off the colors we pushed
        ImGui.PopStyleColor(3);
    }

    /// <summary> This function is used to draw the changelog window. </summary>
    private void DrawChangeLog(Changelog changelog) {
        // Draw the changelog
        changelog.ForceOpen = true;
    }

    /// <summary> 
    /// This function is used to draw the changelog window.
    /// <list type="bullet">
    /// <item><c>TabType</c><param name="type"> - the type of tab we want to go to.</param></item>
    /// </list> </summary> 
    private ReadOnlySpan<byte> ToLabel(TabType type)
        => type switch // we do this via a switch statement
        {
            TabType.General         => General.Label,
            TabType.Whitelist       => Whitelist.Label,
            TabType.ConfigSettings  => ConfigSettings.Label,
            TabType.HelpPage        => HelpPage.Label,
            _                       => ReadOnlySpan<byte>.Empty, // This label confuses me a bit. I think it is just a blank label?
        };


    /// <summary>
    /// The function used to say what tab we are going from.
    /// <list type="bullet">
    /// <item><c>label</c><param name="label"> - the label of the tab we are going from.</param></item>
    /// </list> </summary> 
    private TabType FromLabel(ReadOnlySpan<byte> label) {
        // @formatter:off
        if (label == General.Label)         return TabType.General;
        if (label == Whitelist.Label)       return TabType.Whitelist;
        if (label == ConfigSettings.Label)  return TabType.ConfigSettings;
        if (label == HelpPage.Label)        return TabType.HelpPage;
        // @formatter:on
        return TabType.None;
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeak###GagSpeakMainWindow";
}

/*
                                   ..,,,..
                             ..,,;;;;;;;;;;;;;,.            
                           '"     ;;;;;;;;';;;;;;,.           
                       .   ,  -'";.  ;;;;;;;';" ';;;,
                      '  -' ';';; ;;   ;;;;;;'     "";,
                   . "   ,;;,';,'; ;;   ;;;;;, ,;,_   ;,.'
                  ;    ;";;;"        ";, """:;;;"';;;  ' ,
                 ;   ;' ;"'            ;;,;;,  .;; ;;;   ,,,
                ;   ' ;;"               ;;;;;; ;;;  ;; ,;;;
                  , ;; ;                " ;;;; ';',;;  ;;;
               . ,' ;;;;                  ";;;, ; ;;'  ';;;
                 , ,;';;                 ''";;;   ";    ';;;;
              , ;  ;  ;;         _.,,     ' ";;, ,;;., ,;;;;;;
             ,  ;, ;, ;;'"-    =" _,."'-   ' ;;;,;;;;;   ;;;;;;,
            ,  ,;; '; .'"(o     .'(O)"-    ' ';;;;;;,  ,;;;;" ;;;
       ' ,,    ;; ,;' ;;-"'      '   '    '  ;;;;;; ;   ';;;  ';;;
             ,;;  ;'                      '  ;; ;;';   ,;;;    ;;'
            ;;;   ;,   '  .'               ' ';;;;  ,;;; ;;  ,;;;
          ,;;'    ';  ;;. '- = .'             ;;"; ;;;;'  "  ';;  ,
          ;;   ,; '  ,;';   . _               ;; ' ;;;; .;  ,'" ,;;
         ,;;  ;;;,   ;;,;;."_"_"=            ,;;  ,;;;;.;;,    ;;;'
       ,''  ,;;;;;   ;;; ; ;," ,'            ;;' ;  ';;;;;;;,   ;'
         .,;;;'      ;;; ;;  ""    .-""""''  ;; ;  ,;;";";;;;;,  ',
        ;;;'        ;;; ,;;;._  .-"           ; ;,;;;' ;,';;;;;;;, '
        ;;        .;;  ,;;;;;;;;"              ; ,;;'  ';  ";;;;;;;,
         ;;      ;;   ,;;;;;;;;;'               ; ;' ;;;    ';;;;;;;;;
          ;      ;;   ;;"';;;;;;                  ;;, ;;;;;   ;;;;;;;;;
           '      ;;  ;;   ";;"                   ';;;';;"' ,;;;;,;;;
                  ;;, ';    .                      ';;;,.' ,;;;;,;;;
                 ,;;; ;'  .'                   '   ,;;;;;, ;;;;,;;;
            ;, ,;;;' ,' .'                     '  ;;;;;;;; ;;;;;,;;;
            ';;;'     .'      .                .  ';;;;;;   ;;;;;,;;;,
                    .-        -                   ,;;;;;,    ';;;;;,;;;,
                  .'          '.               ' ;;;;;;;;,  ,.  '";;;;;;
               .-'             '                  ;;;;';;; ;;;;   ';;;;
          . .-'                '               ' ;';;;;";; ';;;  ,;;;;
          ::."                                   '; ";; ;;; ;" ,;;;;
           "                   '               '  ;  '; ;;; ;  ;;;;'
          '            '                           ; ;   "; '; ';;;,
           .        .          '              '   ; "    ,;;;,  ;;;;
                                                   ;    ;;;;;;  ;;;;,
            '.    '            '              '     ; ;;;;;;;'   ";;;,
              '.'                            .       ;;;;;"'    .,;;;
               .               '                     ';;;'   ;;;;;;;
               .                             '      '. ;;     ";;"' .
               .               '            .      /  . '      ;
              .               .             .     .                  '
              '                                        '.             .
             .              .'             '     '       .            '
                                           '    .         .            .
              .           '               '                .           '
                         .               '     '           .            .
              '         .              ,'      .         .'             .
               .                      '         .      .'               |
                       '             '               .'                ."
                '.   .              '            ' -'               .-'
                  . .             /               '.             .-'
                  ..            ,'                            .-'
                  .            -                    '.     .-'
                 .           -'                       '. .'
                .           '                           '.
               .          ,'                              '.
              '         .'                                  '.
             '       . '                                .     '.
            '         '                                  .      '.
            '       '. '                                  .       .
            '         -.                                  '.       .
             '.      ' .                                   '.       .
               .     ' .                                    '
                .     '.                                    .        .
                 '. _   '.                                  '        .
                 '.  '-.  '.                                '        .
                   .   '"-. '.                             '         .
                    .  '.  \_;                            .          .
                      .  .                               -
                     ';'. .                            .'            '
                      .; \_                          .'             '
                      . \;                          .  -  '        .
                      | .                          .
                       "                                          .
                        '                          '
                        '                          .
                        .                          .             '
                                                   '            -
                        .                          .
                        .                          .            .

                        '                          .            '
                         .
                                                   '           '
                         '                        .

                          '                       .           '
                           .
                                                 .           '
                            '
                             .                   '
                              .                             '
                                                 '
                              '        .          .        '
                               .                          .
                               .        .         '
                                                .         '
                                '                 '       .
                                .      .        . .
                                 .                .       .
                                                  .        .
                                  '
                                  .                .       '
                                                            .
                                  .                 .
                                                            .
                                   .                 .
                                                            '
                                   .                  .    .

                                    '                  .  .

                                    '                  '  .
                                     .                 .
                                      .                  '
                                                        '
                                       .               .

                                        .               .
                                         .

                                       '  .             '
                                      .
                                           .            .
                                      .
                                     .      .            .
                                           '      .
                                    .     '  .            .
                                  .      '  '      .
                                  ooooOOOo,   .            .
                                 d"      "Oo. -    ' '       .
                                d .        'O'.     .         .
                                O _.,oooo. .o             ooOOOOo.
                               -O \OOOOOOOO.'  '    .   oOO"'   .O
                              ' OoO\OOOOOOO/          .O'       .o
                             '   "O;oOOOOo/   '      .O       .'o'
                            o.   o;;;oOOo/           oO     -  ooOOOo
                            OO'.oO;;;;OO.;    '     oOO .oooOOOOOOOo
                            OOOOOO;;;;OO;;,   .     oOOOOOOOOOOOOOP
                            OOOOOO;;;;OO;;;   '    .'"OO:;;;OOOOOO
                             "OO:;;;;;OO;;; o'    . oO:;;;;' OOOO
                                     'OO ,ooO     .OO;;;;;.   OO
                                      ==oO "OOo  .OO;;;;;;;   OO
                                        OO- OoOOOOOO;;;;;;;   OO
                                        OOOOOOOOOOOO;;;;;;;   OO
                                         "OOOOOOOOOo;;;;;;;   OO
                                             '"OOO";;;;"'     ==
*/