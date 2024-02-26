using System;
using System.Runtime.InteropServices;

namespace GagSpeak.Hardcore.ClickSelection.Memory;
public class CircularBuffer : IDisposable
{
    public enum ItemFit
    {
        No,
        Yes,
        StartOfBuffer
    }

    public int Offset { get; set; }
    public IntPtr Address { get; set; }
    public int Size { get; set; }

    public IntPtr WritePointer => Address + Offset;
    private int Remaining => Size - Offset;

    public CircularBuffer(int size)
    {
        Offset = 0;
        Size = size;
        Address = Marshal.AllocHGlobal(Size);
    }

    ~CircularBuffer()
    {
        Dispose();
    }

    public IntPtr Add(byte[] bytesToWrite)
    {
        switch (CanItemFit(bytesToWrite.Length))
        {
            case ItemFit.No:
                return IntPtr.Zero;
            case ItemFit.StartOfBuffer:
                Offset = 0;
                break;
        }

        IntPtr writePointer = WritePointer;
        Marshal.Copy(bytesToWrite, 0, writePointer, bytesToWrite.Length);
        Offset += bytesToWrite.Length;
        return writePointer;
    }

    public ItemFit CanItemFit(int objectSize)
    {
        if (Remaining >= objectSize)
        {
            return ItemFit.Yes;
        }

        if (Size >= objectSize)
        {
            return ItemFit.StartOfBuffer;
        }

        return ItemFit.No;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(Address);
        GC.SuppressFinalize(this);
    }
}