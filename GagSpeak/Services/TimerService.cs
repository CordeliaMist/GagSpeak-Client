using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace GagSpeak.Services;

// TimerService class manages timers and notifies when remaining time changes
public class TimerService : IDisposable
{
   private readonly GagSpeakConfig _config;
   // Event to notify subscribers when remaining time changes
   public event Action<string, TimeSpan> RemainingTimeChanged;

   public TimerService(GagSpeakConfig config) {
      _config = config;
      RestoreTimerData(_config);
   }

   // Dictionary to store active timers
   private readonly Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();
   
   // creating a dictionary to store a list of times from the timer serivce to display to UI
   public readonly Dictionary<string, string> remainingTimes = new Dictionary<string, string>();

   // Method to start a new timer
   public void StartTimer(string timerName, string input, int elapsedMilliSecPeriod, Action onElapsed) {
      StartTimer(timerName, input, elapsedMilliSecPeriod, onElapsed, null, -1);}
   
   // the augmented constructor for the timer service to handle padlock timers
   public void StartTimer(string timerName, string input,  int elapsedMilliSecPeriod, Action onElapsed,
   List<DateTimeOffset> padlockTimerList, int index) {
      // Check if a timer with the same name already exists
      if (timers.ContainsKey(timerName)) {
         GagSpeak.Log.Debug($"Timer with name '{timerName}' already exists. Use a different name.");
         return;
      }

      // Parse the input string to get the duration
      TimeSpan duration = ParseTimeInput(input);

      // Check if the duration is valid
      if (duration == TimeSpan.Zero){
         GagSpeak.Log.Debug($"Invalid time format for timer '{timerName}'.");
         return;
      }

      // Calculate the end time of the timer
      DateTimeOffset endTime = DateTimeOffset.Now.Add(duration);//

      // update the selectedGagPadLockTimer list with the new end time. (only if using a list as input)
      if (padlockTimerList != null && index >= 0 && index < padlockTimerList.Count) {
         padlockTimerList[index] = endTime;
      }

      // Create a new timer
      Timer timer = new Timer(elapsedMilliSecPeriod);
      timer.Elapsed += (sender, args) => OnTimerElapsed(timerName, timer, onElapsed);
      timer.Start();

      // Store the timer data in the dictionary
      timers[timerName] = new TimerData(timer, endTime);

      // save the timer data
      SaveTimerData(_config);
      DebugPrintRemainingTimers();

   }

    // Method called when a timer elapses
   private void OnTimerElapsed(string timerName, Timer timer, Action onElapsed) {
      if (timers.TryGetValue(timerName, out var timerData)) {
         // Calculate remaining time
         TimeSpan remainingTime = timerData.EndTime - DateTimeOffset.Now;
         if (remainingTime <= TimeSpan.Zero) {
               // Timer expired
               GagSpeak.Log.Debug($"Timer '{timerName}' expired.");
               timer.Stop();
               onElapsed?.Invoke();
               timers.Remove(timerName);
         }
         else {
               // Notify subscribers about remaining time change
               RemainingTimeChanged?.Invoke(timerName, remainingTime);
         }
      }
   }

   // Method to parse time input string
   public static TimeSpan ParseTimeInput(string input) {
      // Match hours, minutes, and seconds in the input string
      var match = Regex.Match(input, @"^(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");

      if (match.Success) { // Parse hours, minutes, and seconds
         int.TryParse(match.Groups[1].Value, out int hours);
         int.TryParse(match.Groups[2].Value, out int minutes);
         int.TryParse(match.Groups[3].Value, out int seconds);
         // Return the total duration
         return new TimeSpan(hours, minutes, seconds);
      }

      // If the input string is not in the correct format, return TimeSpan.Zero
      return TimeSpan.Zero;
   }

   // save our data
   public void SaveTimerData(GagSpeakConfig config)
   {
      // Clear the existing timer data in the config
      config.TimerData.Clear();

      // Add the current timer data to the config
      foreach (var pair in timers) {
         config.TimerData[pair.Key] = pair.Value.EndTime;
      }

      // Save the config
      config.Save();
   }

   public void RestoreTimerData(GagSpeakConfig _config)
   {
      // Clear the existing timers
      timers.Clear();

      // Restore the timers from the config
      foreach (var pair in _config.TimerData)
      {
         // Create a new timer with the same name and end time
         if(pair.Key.Contains("_Identifier0")) {
            GagSpeak.Log.Debug($"Restoring timer {pair.Key} with end time {pair.Value}");
            StartTimer(pair.Key, (pair.Value - DateTimeOffset.Now).ToString(), 1000, () => {
               _config._isLocked[0] = false;
               _config._padlockIdentifier[0].ClearPasswords();
               _config._padlockIdentifier[0].UpdateConfigPadlockPasswordInfo(0, !_config._isLocked[0], _config);
            });
         } else if (pair.Key.Contains("_Identifier1")) {
            GagSpeak.Log.Debug($"Restoring timer {pair.Key} with end time {pair.Value}");
            StartTimer(pair.Key, (pair.Value - DateTimeOffset.Now).ToString(), 1000, () => {
               _config._isLocked[1] = false;
               _config._padlockIdentifier[1].ClearPasswords();
               _config._padlockIdentifier[1].UpdateConfigPadlockPasswordInfo(1, !_config._isLocked[1], _config);
            });
         } else if (pair.Key.Contains("_Identifier2")) {
            GagSpeak.Log.Debug($"Restoring timer {pair.Key} with end time {pair.Value}");
            StartTimer(pair.Key, (pair.Value - DateTimeOffset.Now).ToString(), 1000, () => {
               _config._isLocked[2] = false;
               _config._padlockIdentifier[2].ClearPasswords();
               _config._padlockIdentifier[2].UpdateConfigPadlockPasswordInfo(2, !_config._isLocked[2], _config);
            });
         }
         // otherwise dont make any new padlock.
      }
   }

   public void DebugPrintRemainingTimers()
   {
      foreach (var pair in _config.TimerData)
      {
         // Calculate the remaining time in milliseconds
         var remainingTime = (pair.Value - DateTimeOffset.Now).TotalMilliseconds;

         // Print the timer name and remaining time
         GagSpeak.Log.Debug($"Timer: {pair.Key}, Remaining Time: {remainingTime} ms");
      }
   }

   // method to get the current state of all timers
   private class TimerData
   {
      public Timer Timer { get; }
      public DateTimeOffset EndTime { get; }

      public TimerData(Timer timer, DateTimeOffset endTime) {
         Timer = timer;
         EndTime = endTime;
      }
   }

   // Method to dispose the service
   public void Dispose()
   {
      // Dispose all timers
      foreach (var timerData in timers.Values) {
         timerData.Timer.Dispose();
      }
   }
}
