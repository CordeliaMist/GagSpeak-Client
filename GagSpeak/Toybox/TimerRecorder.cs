using System;
using System.Timers;

namespace GagSpeak.ToyboxandPuppeteer;
public class TimerRecorder : IDisposable
{
    private readonly Timer _timer;
    private readonly ElapsedEventHandler _handler;

    public TimerRecorder(int interval, ElapsedEventHandler handler) {
        _timer = new Timer(interval);
        _handler = handler;
        _timer.Elapsed += _handler;
    }

    public void Start() {
        _timer.Start();
    }

    public void Stop() {
        _timer.Stop();
    }

    public void Dispose() {
        _timer.Elapsed -= _handler;
        _timer.Dispose();
    }
}