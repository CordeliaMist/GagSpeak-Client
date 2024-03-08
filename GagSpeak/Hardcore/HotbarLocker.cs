using GagSpeak.Utils;
using FFXIVClientStructs.FFXIV.Client.UI;

// taken from SimpleTweaksPlugin/Utility/Common.cs
// controls the state of the UI
namespace GagSpeak.Hardcore;
public unsafe class HotbarLocker
{
    private readonly    AtkHelpers _atkHelpers;
    public              bool _lockState;
    public HotbarLocker(AtkHelpers atkHelpers) {
        _atkHelpers = atkHelpers;
    }
    public void SetHotbarLockState(bool state) {
        // set the lock state
        var actionBar = _atkHelpers.GetUnitBase("_ActionBar");
        if (actionBar == null) return;
        // only change lock state if _lockState == false
        if (!state) AtkHelpers.GenerateCallback(actionBar, 8, 3, 51u, 0u, state);
        // set the lock visibility
        var lockNode = actionBar->GetNodeById(21);
        if (lockNode == null) return;
        var lockComponentNode = lockNode->GetAsAtkComponentNode();
        if (lockComponentNode == null) return;
        lockComponentNode->AtkResNode.ToggleVisibility(!state);
    }

    public void SetHotbarStateToCurrentState() {
        var addon = _atkHelpers.GetUnitBase<AddonActionBarBase>("_ActionBar");
        // if the addon is not null
        if (addon != null) {
            // get the lock node
            bool isLocked = addon->IsLocked;
            // set the lock state to the hotbars _lockState
            _lockState = isLocked;
        }
    }
}
