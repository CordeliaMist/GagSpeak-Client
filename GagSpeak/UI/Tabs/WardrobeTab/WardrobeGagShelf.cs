using System.Linq;
using Dalamud.Interface.Utility;
using GagSpeak.CharacterData;
using ImGuiNET;

namespace GagSpeak.UI.Tabs.WardrobeTab;

public class WardrobeGagCompartment
{
    private readonly CharacterHandler _characterHandler;
    private readonly GagStorageSelector _selector;
    private readonly GagStorageDetails  _details;

    public WardrobeGagCompartment(CharacterHandler characterHandler, GagStorageSelector selector,
    GagStorageDetails details) {
        _characterHandler = characterHandler;
        _selector = selector;
        _details  = details;
    }

    public void DrawContent()
    {
        if(_characterHandler.playerChar._lockGagStorageOnGagLock 
        && _characterHandler.playerChar._selectedGagPadlocks.Any(x => x != Gagsandlocks.Padlocks.None))
        {
            ImGui.BeginDisabled();
        }
        _selector.Draw(GetSetSelectorSize());
        ImGui.SameLine();
        _details.Draw();

        if(_characterHandler.playerChar._lockGagStorageOnGagLock 
        && _characterHandler.playerChar._selectedGagPadlocks.Any(x => x != Gagsandlocks.Padlocks.None))
        {
            ImGui.EndDisabled();
        }
    }

    public float GetSetSelectorSize()
        => 160f * ImGuiHelpers.GlobalScale;
}
