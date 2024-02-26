using System;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace GagSpeak.Hardcore.ClickSelection;
public unsafe sealed class InputData : SharedBuffer
{
    private InputData() {
        this.Data = (void**)Buffer.Add(new byte[0x40]);
        if (this.Data == null)
            throw new ArgumentNullException("InputData could not be created, null");

        this.Data[0] = null;
        this.Data[1] = null;
        this.Data[2] = null;
        this.Data[3] = null;
        this.Data[4] = null;
        this.Data[5] = null;
        this.Data[6] = null;
        this.Data[7] = null;
    }

    // the data pointer
    public void** Data { get; }

    public static InputData Empty() { return new InputData(); }

    public static InputData ForPopupMenu(PopupMenu* popupMenu, ushort index) {
        var data = new InputData();
        data.Data[0] = popupMenu->List->ItemRendererList[index].AtkComponentListItemRenderer;
        data.Data[2] = (void*)(index | ((ulong)index << 48));
        return data;
    }
}