using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Gagger.Windows;
using System.IO;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Drawing;

namespace Gagger
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Gagger ";
        private const string CommandName = "/gagspeak";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Gagger");
        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }


        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "cog.png");
            var settingsImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, (ImGuiScene.TextureWrap)settingsImage);

            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            //declare command name, handler, and help text
            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Garble your text when gagged."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            MainWindow.IsOpen = true;
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }
        

    }
}
