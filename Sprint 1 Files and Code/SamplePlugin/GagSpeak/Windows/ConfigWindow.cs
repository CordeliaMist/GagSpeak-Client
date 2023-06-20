using System;
using System.Numerics;
using Dalamud.Configuration;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Gagger;
namespace Gagger.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base(
        "A Wonderful Configuration Window",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Size = new Vector2(232, 175);
        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var enable = this.Configuration.status;
        if (ImGui.Checkbox("Enable", ref enable))
        {
            this.Configuration.status = enable;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            this.Configuration.Save();
        }
        int power = Configuration.strength;
        if (ImGui.SliderInt("Strength", ref power, (int)0, (int)20))
        {
            Configuration.strength = power;
        }
        int GagID= 0;
        string[] Name = {"None","Ball Gag","Panel Gag","Plug Gag","Pump Gag","Ring Gag"};
        int[] gagStrength = {0,5,3,5,2,3};
        if (ImGui.BeginCombo("Gag", Name[GagID]))
        {
            int i = 0;
            foreach (string gag in Name)
            {
                i++;
                if (ImGui.Selectable(gag, gag == Name[GagID]) && gag != Name[GagID])
                {
                    GagID = i;
                    Configuration.strength = gagStrength[i];
                } 
            }
            ImGui.EndCombo();
        }
    }
}
