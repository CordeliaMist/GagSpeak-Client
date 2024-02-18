using System.Collections.Generic;
using System.Diagnostics;

namespace GagSpeak.ToyboxandPuppeteer;


// a mediator for holding timer information and tracks when a pattern has been saved
public class WorkshopMediator
{
    public Stopwatch recordingStopwatch;
    public List<byte> storedRecordedPositions;
    public string patternName = "";
    public bool finishedRecording = false;
    public bool isRecording = false;
    public PatternData tempNewPattern;

    public WorkshopMediator() {
        tempNewPattern = new PatternData();
        storedRecordedPositions = new List<byte>();
        patternName = "";
        finishedRecording = false;
        isRecording = false;

        recordingStopwatch = new Stopwatch();
    }

    public void Dispose() {
        recordingStopwatch.Stop();
        recordingStopwatch.Reset();
    }
}
