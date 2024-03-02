using System;
using System.Timers;

namespace GagSpeak.ToyboxandPuppeteer;
public class TimerRecorder : IDisposable
{
    private readonly Timer _timer;
    private readonly ElapsedEventHandler _handler;
    private DateTime _startTime;

    public TimerRecorder(int interval, ElapsedEventHandler handler) {
        _timer = new Timer(interval);
        _handler = handler;
        _timer.Elapsed += _handler;
    }

    public TimeSpan Elapsed {
        get {
            if (_timer.Enabled)
                return DateTime.Now - _startTime;
            else
                return TimeSpan.Zero;
        }
    }

    public bool IsRunning {
        get {
            return _timer.Enabled;
        }
    }

    public void Start() {
        _startTime = DateTime.Now;
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