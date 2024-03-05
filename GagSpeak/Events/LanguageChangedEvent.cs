using System;

namespace GagSpeak.Events;

/// <summary> This class is used to handle the language changed event, and triggers every time it is fired. </summary>
public class LanguageChangedEvent 
{
    public delegate void LanguageChangedEventHandler(object sender, LanguageChangedEventArgs e); // define the event handler
    public event LanguageChangedEventHandler? LanguageChanged;                                    // define the event

    /// <summary> Manually triggered event invoker </summary>
    public void Invoke() {
        GSLogger.LogType.Debug("[LanguageChangedEventHandler] Invoked");
        LanguageChanged?.Invoke(this, new LanguageChangedEventArgs());
    }
}

/// <summary>
/// This class is used to handle the language changed event arguments, if any ever end up existing
/// </summary>
public class LanguageChangedEventArgs : EventArgs
{
    // You can add properties here to pass additional information about the language change
}