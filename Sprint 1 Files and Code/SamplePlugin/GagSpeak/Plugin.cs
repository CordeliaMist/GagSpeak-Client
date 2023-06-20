using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Gagger.Windows;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState;
using System.Linq;
using System;

namespace Gagger
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Gagger ";
        private const string Command = "/gag";
        private const string SettingsCommand = "/gagspeak";
        private readonly ChatGui chat;

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Gagger");

        private ConfigWindow ConfigWindow { get; init; }
        //private MainWindow MainWindow { get; init; }


        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager, 
            ChatGui chat)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.chat = chat;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            //MainWindow = new MainWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            //WindowSystem.AddWindow(MainWindow);
            
            //declare command name, handler, and help text
            this.CommandManager.AddHandler(Command, new CommandInfo(OnCommand)
            {
                HelpMessage = "Garble your text when gagged."
            });

            this.CommandManager.AddHandler(SettingsCommand, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open settings window."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            
            this.CommandManager.RemoveHandler(Command);
        }

        private void OnCommand(string command, string args)
        {
            if (command == "gagspeak") ConfigWindow.IsOpen = !ConfigWindow.IsOpen;
            else if (command == "gag")
            {
                string[] arguemnts = args.Split(' ');  //plit arguments for better handling (only did this so I can implement say and set, should be useful for set by gag name)
                switch(arguemnts[0])
                {
                    case "clear":       // in response to the slash command, just display our main ui     
                    Configuration.strength = 0;    
                        break;
                    
                    case "say":
                        string sentence = garbleFunction(string.Join(' ', arguemnts , 1 , (arguemnts.Length-2) )); //recombine arguments after say into sentence and garble
                        this.chat.Print(sentence);
                        //insert chat output
                        break;

                    case "set":
                        bool parseSucc = Int32.TryParse(arguemnts[1] , out int str);
                        if(parseSucc && (str <= 20 && str >=0 )) Configuration.strength = str; //if parse successful and in limits
                        //else ;  //add error handling
                        break;
                    default:
                        //if ( lookup(string.Join(' ', arguments, 1 , (arguments.Length-2))) )
                        break;
                }

            }
            
            
                
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }
        private string garbleFunction(string input)
        {
        string beginString = input;
        string endString = "\0";
        beginString = beginString.ToLower();
        for (int ind = 0; ind < beginString.Length; ind++)
        {
            char currentChar = beginString[ind];
            if (Configuration.strength >= 20)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkvbywgpfucdlhr".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 16)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkvbywgpf".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 12)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("zqjxkv".Contains(currentChar)) { endString += " "; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 8)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else { endString += "m"; }
            }
            else if (Configuration.strength >= 7)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if (currentChar == 'b') { endString += currentChar; }
                else if ("aeiouy".Contains(currentChar)) { endString += "e"; }
                else if ("jklr".Contains(currentChar)) { endString += "a"; }
                else if ("szh".Contains(currentChar)) { endString += "h"; }
                else if ("dfgnmwtcqxpv".Contains(currentChar)) { endString += "m"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 6)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("aeiouyt".Contains(currentChar)) { endString += "e"; }
                else if ("jklrw".Contains(currentChar)) { endString += "a"; }
                else if ("szh".Contains(currentChar)) { endString += "h"; }
                else if ("dfgnm".Contains(currentChar)) { endString += "m"; }
                else if ("cqx".Contains(currentChar)) { endString += "k"; }
                else if ("bpv".Contains(currentChar)) { endString += "f"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 5)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("eiouyt".Contains(currentChar)) { endString += "e"; }
                else if ("jlrwa".Contains(currentChar)) { endString += "a"; }
                else if ("szh".Contains(currentChar)) { endString += "h"; }
                else if ("dfgm".Contains(currentChar)) { endString += "m"; }
                else if ("cqxk".Contains(currentChar)) { endString += "k"; }
                else if ("bpv".Contains(currentChar)) { endString += "f"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 4)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("vbct".Contains(currentChar)) { endString += "e"; }
                else if ("wyjlr".Contains(currentChar)) { endString += "a"; }
                else if ("sz".Contains(currentChar)) { endString += "h"; }
                else if ("df".Contains(currentChar)) { endString += "m"; }
                else if ("qkx".Contains(currentChar)) { endString += "k"; }
                else if (currentChar == 'p') { endString += "f"; }
                else if (currentChar == 'g') { endString += "n"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 3)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("vbct".Contains(currentChar)) { endString += "e"; }
                else if ("wyjlr".Contains(currentChar)) { endString += "a"; }
                else if ("sz".Contains(currentChar)) { endString += "s"; }
                else if ("qkx".Contains(currentChar)) { endString += "k"; }
                else if (currentChar == 'd') { endString += "m"; }
                else if (currentChar == 'p') { endString += "f"; }
                else if (currentChar == 'g') { endString += "h"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 2)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("ct".Contains(currentChar)) { endString += "e"; }
                else if ("jlr".Contains(currentChar)) { endString += "a"; }
                else if ("qkx".Contains(currentChar)) { endString += "k"; }
                else if ("dmg".Contains(currentChar)) { endString += "m"; }
                else if (currentChar == 's') { endString += "z"; }
                else if (currentChar == 'z') { endString += "s"; }
                else if (currentChar == 'f') { endString += "h"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength >= 1)
            {
                if (Char.IsPunctuation(currentChar)) { endString += currentChar; }
                else if (Char.IsWhiteSpace(currentChar)) { endString += currentChar; }
                else if ("jlr".Contains(currentChar)) { endString += "a"; }
                else if ("cqkx".Contains(currentChar)) { endString += "k"; }
                else if ("dmg".Contains(currentChar)) { endString += "m"; }
                else if (currentChar == 't') { endString += "e"; }
                else if (currentChar == 'z') { endString += "s"; }
                else if (currentChar == 'f') { endString += "h"; }
                else { endString += currentChar; }
            }
            else if (Configuration.strength == 0)
            {
                endString += currentChar;
            }
        }
        return endString;
    }

    }
}
