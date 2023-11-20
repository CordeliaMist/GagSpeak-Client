using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using GagSpeak.UI.Helpers;

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
      // If the new timer's name contains "_Identifier{index}"
      if (timerName.Contains($"_Identifier{index}")) {
         // Find any existing timer with "_Identifier{index}" in its name
         var existingTimerKey = timers.Keys.FirstOrDefault(key => key.Contains($"_Identifier{index}"));
         // If an existing timer is found, remove it
         if (existingTimerKey != null) {
               timers.Remove(existingTimerKey);
         }
      }
      // Also Check if a timer with the same name already exists
      if (timers.ContainsKey(timerName)) {
         GagSpeak.Log.Debug($"[Timer Service]: Timer with name '{timerName}' already exists. Use different name.");
         return;
      }
      // Check if a timer with the same name already exists
      if (timers.ContainsKey(timerName)) {
         GagSpeak.Log.Debug($"[Timer Service]: Timer with name '{timerName}' already exists. Use different name.");
         return;
      }

      // Parse the input string to get the duration
      TimeSpan duration = ParseTimeInput(input);

      GagSpeak.Log.Debug($"[Timer Service]: '{timerName}' started with duration {duration}.");
      // Check if the duration is valid
      if (duration == TimeSpan.Zero){
         GagSpeak.Log.Debug($"[Timer Service]: Invalid time format for timer '{timerName}'.");
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
      timers[timerName] = new TimerData(timer, endTime);//

      // save the timer data
      SaveTimerData(_config);
      // DebugPrintRemainingTimers(); For the stats nerds.

   }

    // Method called when a timer elapses
   private void OnTimerElapsed(string timerName, Timer timer, Action onElapsed) {
      if (timers.TryGetValue(timerName, out var timerData)) {
         // Calculate remaining time
         TimeSpan remainingTime = timerData.EndTime - DateTimeOffset.Now;
         if (remainingTime <= TimeSpan.Zero) {
               // Timer expired
               GagSpeak.Log.Debug($"[Timer Service]: '{timerName}'has reached zero, dumpting timer.");
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
      var match = Regex.Match(input, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");

      if (match.Success) { 
         // Parse days, hours, minutes, and seconds
         int.TryParse(match.Groups[1].Value, out int days);
         int.TryParse(match.Groups[2].Value, out int hours);
         int.TryParse(match.Groups[3].Value, out int minutes);
         int.TryParse(match.Groups[4].Value, out int seconds);
         // Return the total duration
         return new TimeSpan(days, hours, minutes, seconds);
      }

      // If the input string is not in the correct format, return TimeSpan.Zero
      return TimeSpan.Zero;
   }

   // get remaining time for one of the padlock identifier slots
   public string GetRemainingTimeForPadlock(int slot) {
      var padlockType = _config._padlockIdentifier[slot]._padlockType;
      return $"{remainingTimes.GetValueOrDefault($"{padlockType}_Identifier{slot}", "Time Remaining:")}";
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

   public void RestoreTimerData(GagSpeakConfig _config) {
      // Clear the existing timers
      timers.Clear();

      // Create a temporary list of timers to restore
      var timersToRestore = new List<(string, TimeSpan)>();

      // Populate the list from the config
      foreach (var pair in _config.TimerData) {
         // Calculate the remaining time
         var remainingTime = pair.Value - DateTimeOffset.Now;
         // Add the timer to the list
         timersToRestore.Add((pair.Key, remainingTime));
      }

      // Restore the timers from the list
      foreach (var (timerName, remainingTime) in timersToRestore) {
         // Create a new timer with the same name and remaining time
         if(timerName.Contains("_Identifier0")) {
            GagSpeak.Log.Debug($"[Timer Service]: Restoring {timerName} with end time {remainingTime}");
            StartTimer(timerName, UIHelpers.FormatTimeSpan(remainingTime), 1000, () => {
               _config._isLocked[0] = false;
               _config._padlockIdentifier[0].ClearPasswords();
               _config._padlockIdentifier[0].UpdateConfigPadlockPasswordInfo(0, !_config._isLocked[0], _config);
            });
         } else if(timerName.Contains("_Identifier1")) {
            GagSpeak.Log.Debug($"[Timer Service]: Restoring timer {timerName} with end time {remainingTime}");
            StartTimer(timerName, UIHelpers.FormatTimeSpan(remainingTime), 1000, () => {
               _config._isLocked[1] = false;
               _config._padlockIdentifier[1].ClearPasswords();
               _config._padlockIdentifier[1].UpdateConfigPadlockPasswordInfo(1, !_config._isLocked[1], _config);
            });
         } else if(timerName.Contains("_Identifier2")) {
            GagSpeak.Log.Debug($"[Timer Service]: Restoring timer {timerName} with end time {remainingTime}");
            StartTimer(timerName, UIHelpers.FormatTimeSpan(remainingTime), 1000, () => {
               _config._isLocked[2] = false;
               _config._padlockIdentifier[2].ClearPasswords();
               _config._padlockIdentifier[2].UpdateConfigPadlockPasswordInfo(2, !_config._isLocked[2], _config);
            });
         }
      }
   }

   public void DebugPrintRemainingTimers()
   {
      foreach (var pair in _config.TimerData)
      {
         // Calculate the remaining time in milliseconds
         var remainingTime = UIHelpers.FormatTimeSpan(pair.Value - DateTimeOffset.Now);

         // Print the timer name and remaining time
         GagSpeak.Log.Debug($"[Timer Service] Timer: {pair.Key}, Remaining Time: {remainingTime}");
      }
   }

   // method to get the current state of all timers
   private class TimerData {
      public Timer Timer { get; }
      public DateTimeOffset EndTime { get; }

      public TimerData(Timer timer, DateTimeOffset endTime) {
         Timer = timer;
         EndTime = endTime;
      }
   }

   public void ClearIdentifierTimers() {
      // Get a list of keys that contain "_Identifier"
      var keysToRemove = new List<string>();
      foreach (var key in timers.Keys) {
         if (key.Contains("_Identifier")) {
               keysToRemove.Add(key);
         }
      }

      // Remove the timers with the keys in keysToRemove
      foreach (var key in keysToRemove) {
         timers[key].Timer.Dispose(); // Dispose the timer before removing it
         timers.Remove(key);
      }
   }

   // Method to dispose the service
   public void Dispose() {
      GagSpeak.Log.Debug("--------Saving & Unloading Timers-------"); // save timers upon unloading.
      SaveTimerData(_config);
      DebugPrintRemainingTimers();
      GagSpeak.Log.Debug("--------Timers Saved Sucessfully--------");
      // Dispose all timers
      foreach (var timerData in timers.Values) {
         timerData.Timer.Dispose();
      }
   }
}
