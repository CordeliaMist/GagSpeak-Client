using System;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using OtterGui.Widgets;
using GagSpeak.UI.UserProfile;
using Glamourer.Gui;

namespace GagSpeak.UI;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class GagSpeakWindowManager : IDisposable
{
    private readonly WindowSystem               _windowSystem = new("GagSpeak");
    private readonly UiBuilder                  _uiBuilder;
    private readonly MainWindow                 _ui;
    private readonly IChatGui                   _chatGui;

    public GagSpeakWindowManager(UiBuilder uiBuilder, MainWindow ui, GagSpeakConfig config,
    IChatGui chatGui, HistoryWindow uiHistory, GagSpeakChangelog changelog, UserProfileWindow userProfile)
    {
        // set the main ui window
        _uiBuilder       = uiBuilder;
        _ui              = ui;
        _chatGui         = chatGui;
        _windowSystem.AddWindow(ui);
        _windowSystem.AddWindow(uiHistory);
        _windowSystem.AddWindow(userProfile);
        _windowSystem.AddWindow(changelog.Changelog);

        _uiBuilder.Draw                  += _windowSystem.Draw;     // for drawing the UI stuff
        _uiBuilder.OpenConfigUi          += _ui.Toggle;             // for toggling the UI stuff

        //handle a fresh install
        if (config.FreshInstall){
            // They are new, so print some nice messages
            _chatGui.Print(new SeStringBuilder().AddText("Thank you for installing ").AddBlue("GagSpeak!").BuiltString);
            _chatGui.Print(new SeStringBuilder().AddYellow("Instructions: ").AddText("You can use ").AddBlue("/gagspeak help ")
                .AddText("to see main functions, ").AddBlue("/gag ").AddText("to view gagging commands, and ").AddBlue("/gsm ")
                .AddText("to chat in gagspeak.").BuiltString);
            config.FreshInstall = false;
            config.Save();
            _ui.Toggle();
        }
    }

    // for disposing the UI things
    public void Dispose()
    {
        _uiBuilder.Draw         -= _windowSystem.Draw;
        _uiBuilder.OpenConfigUi -= _ui.Toggle;
    }
}
#pragma warning restore IDE1006
