
using GagSpeak.Hardcore.ClickSelection.Memory;

namespace GagSpeak.Hardcore.ClickSelection;
public abstract class SharedBuffer
{
    static SharedBuffer() {
        Buffer = new CircularBuffer(0x2048);
    }

    protected static CircularBuffer Buffer { get; }

    public static void Dispose() {
        Buffer.Dispose();
    }
}
