// for the base feature functionality
namespace GagSpeak.Hardcore.BaseListener;
public abstract class BaseFeature
{
    public virtual bool Enabled { get; protected set; }
    public virtual string Key => GetType().Name;

    public virtual void Enable()
    {
        GagSpeak.Log.Debug($"Enabling {Key}");
        Enabled = true;
    }

    public virtual void Disable()
    {
        GagSpeak.Log.Debug($"Disabling {Key}");
        Enabled = false;
    }
}
