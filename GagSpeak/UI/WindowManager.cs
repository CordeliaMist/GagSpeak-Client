using System;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using GagSpeak.Chat;
using OtterGui.Widgets;

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

// practicing modular design
namespace GagSpeak.UI;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class GagSpeakWindowManager : IDisposable
{
    private readonly WindowSystem               _windowSystem = new("GagSpeak");
    private readonly UiBuilder                  _uiBuilder;
    private readonly MainWindow                 _ui;
    private readonly HistoryWindow              _uiHistory;

    public GagSpeakWindowManager(UiBuilder uiBuilder, MainWindow ui, GagSpeakConfig config, HistoryWindow uiHistory)
    {
        // set the main ui window
        _uiBuilder       = uiBuilder;
        _ui              = ui;
        _uiHistory       = uiHistory;
        _windowSystem.AddWindow(ui);
        _windowSystem.AddWindow(uiHistory);
//        _windowSystem.AddWindow(Changelog.Changelog);
        // Draw the ui and the toggles
        _uiBuilder.Draw                  += _windowSystem.Draw;     // for drawing the UI stuff
        _uiBuilder.OpenConfigUi          += _ui.Toggle;             // for toggling the UI stuff
    }

    // for disposing the UI things
    public void Dispose()
    {
        _uiBuilder.Draw         -= _windowSystem.Draw;
        _uiBuilder.OpenConfigUi -= _ui.Toggle;
    }
}
#pragma warning restore IDE1006
