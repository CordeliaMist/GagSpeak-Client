using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using GagSpeak.Hardcore.ClickSelection.BaseClickLogic;

namespace GagSpeak.Hardcore.ClickSelection;

// This will click either the yes or no button in the dialogue (was sealed)
public unsafe class ClickSelectYesNo : ClickBase<ClickSelectYesNo, AddonSelectYesno>
{
    public ClickSelectYesNo(IntPtr addon = default) : base("SelectYesno", addon) { }

    public static implicit operator ClickSelectYesNo(IntPtr addon) => new(addon);

    /// <summary> Instantiate the click </summary>
    public static ClickSelectYesNo Using(IntPtr addon) => new(addon);

    [ClickName("select_yes")]
    public void Yes() => ClickAddonButton(this.Addon->YesButton, 0);

    [ClickName("select_no")]
    public void No() => this.ClickAddonButton(this.Addon->NoButton, 1);

    [ClickName("select_confirm")]
    public void Confirm() => this.ClickAddonCheckBox(this.Addon->ConfirmCheckBox, 3);
}