// for the base feature functionality
namespace GagSpeak.Hardcore.BaseListener;
public class BaseFeature
{
    public virtual bool Enabled { get; protected set; }
    public virtual string Key => GetType().Name;

    public virtual void Enable()
    {
        GSLogger.LogType.Debug($"Enabling {Key}");
        Enabled = true;
    }

    public virtual void Disable()
    {
        GSLogger.LogType.Debug($"Disabling {Key}");
        Enabled = false;
    }
}
