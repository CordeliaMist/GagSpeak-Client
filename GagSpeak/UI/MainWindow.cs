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
using GagSpeak.UI.Tabs.WardrobeTab;
using GagSpeak.UI.Tabs.PuppeteerTab;
using GagSpeak.UI.Tabs.ToyboxTab;

namespace GagSpeak.UI;

public enum TabType {
    None            = -1,   // No tab selected
    General         = 0,    // Where you select your gags and safewords and lock types. Put into own tab for future proofing beauty spam
    Whitelist       = 1,    // Where you can append peoples names to a whitelist, which is used to account for permissions on command usage.
    Wardrobe        = 2,    // Where you can set what equips when what is worn & config automatic bind & lock options.
    Puppeteer       = 3,    // for controlling others~
    Toybox          = 4,    // for controlling toys, fun fun~
    ConfigSettings  = 5,    // Where you can change the plugin settings, such as debug mode, and other things.
    HelpPage        = 6     // Where you can find information on how to use the plugin, and how to get support.
}
/// <summary> This class is used to handle the main window. </summary>
public class MainWindow : Window
{
    private readonly    GagSpeakConfig      _config;
    private readonly    ITab[]              _tabs;
    public readonly     GeneralTab          General;
    public readonly     WhitelistTab        Whitelist;
    public readonly     WardrobeTab         Wardrobe;
    public readonly		  PuppeteerTab		    Puppeteer;
    public readonly		  ToyboxTab			      Toybox;
    public readonly     ConfigSettingsTab   ConfigSettings;
    public readonly     HelpPageTab         HelpPage;
    public              TabType             SelectTab = TabType.None;

    /// <summary> Constructs the primary 'MainWindow'. Hosts the space for the other windows to fit in.
    /// <para> Note: The 'MainWindow' is the window space hosting the UI when you type /gagspeak, not any independant tab.
    /// </para> </summary>
    public MainWindow(DalamudPluginInterface pluginInt, GagSpeakConfig config, GeneralTab general,
	WhitelistTab whitelist, ConfigSettingsTab configsettings, WardrobeTab wardrobeTab,
	PuppeteerTab puppeteer, ToyboxTab toybox, HelpPageTab helpPageTab): base(GetLabel()) {
		// Let's first make sure that we disable the plugin while inside of gpose.
		pluginInt.UiBuilder.DisableGposeUiHide = true;
		// Next let's set the size of the window
		SizeConstraints = new WindowSizeConstraints() {
			MinimumSize = new Vector2(540, 525),     // Minimum size of the window
			MaximumSize = new Vector2(600, 1000)     // Maximum size of the window
		};

    //Size = new Vector2(540, 525);
		// set the private readonly's to the passed in data of the respective names
		General = general;
		Whitelist = whitelist;
		Wardrobe = wardrobeTab;
		Puppeteer = puppeteer;
		Toybox = toybox;
		ConfigSettings = configsettings;
		HelpPage = helpPageTab;
		// Below are the stuff besides the tabs that are passed through
		_config    = config;
		// the tabs to be displayed
		_tabs = new ITab[]
		{
			general,
			whitelist,
			wardrobeTab,
			puppeteer,
			toybox,
			configsettings,
			helpPageTab
		};
	}

    public override void Draw() {

        var yPos = ImGui.GetCursorPosY();
        // set the cursor position to the top left of the window
        if (TabBar.Draw("##tabs", ImGuiTabBarFlags.None, ToLabel(SelectTab), out var currentTab, () => { }, _tabs)) {
            SelectTab           = TabType.None; // set the selected tab to none
            _config.SelectedTab = FromLabel(currentTab); // set the config selected tab to the current tab
            _config.Save();
        }
        // We want to display the save & close, and the donation buttons on the topright, so lets draw those as well.
        ImGui.SetCursorPos(new Vector2(ImGui.GetWindowContentRegionMax().X - 6f * ImGui.GetFrameHeight(), yPos - ImGuiHelpers.GlobalScale));
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
        ImGui.Text(" ");
        ImGui.SameLine();
        if (ImGui.Button("Toss Cordy a Thanks♥")) {
            ImGui.SetTooltip( "Only if you want to though!");
            Process.Start(new ProcessStartInfo {FileName = "https://ko-fi.com/cordeliamist", UseShellExecute = true});
        }
        // pop off the colors we pushed
        ImGui.PopStyleColor(3);
    }

    /// <summary> Gets the correct label to display for each tab type </summary> 
    private ReadOnlySpan<byte> ToLabel(TabType type)
        => type switch // we do this via a switch statement
        {
            TabType.General         => General.Label,
            TabType.Whitelist       => Whitelist.Label,
            TabType.Wardrobe        => Wardrobe.Label,
            TabType.Puppeteer       => Puppeteer.Label,
            TabType.Toybox          => Toybox.Label,
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
        if (label == Wardrobe.Label)        return TabType.Wardrobe;
        if (label == Puppeteer.Label)       return TabType.Puppeteer;
        if (label == Toybox.Label)          return TabType.Toybox;
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