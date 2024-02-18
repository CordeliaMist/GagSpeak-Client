using OtterGui;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class TriggersSubtab
{
    public TriggersSubtab() { }

    public void Draw() {
        // any other multiple components to draw here
        DrawTriggersUI();
    }

    private void DrawTriggersUI() {
        ImGuiUtil.Center($"Will be a bit before this is present");
    }
}