using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using GagSpeak.CharacterData;
using GagSpeak.Events;
using GagSpeak.Utility;


namespace GagSpeak.Services;

/// <summary>
/// Timer Service class, handles management of timers created within the GagSpeak plugin.
/// </summary>
public class TimerService : IDisposable
{
   private readonly  GagSpeakConfig                _config;                // for config options
   private readonly  CharacterHandler              _characterHandler;      // for getting the whitelist
   private readonly  InfoRequestEvent              _infoRequestEvent;      // event to notify subscribers when info is requested
   public event      Action<string, TimeSpan>?     RemainingTimeChanged;   // event to notify subscribers when remaining time changes
   public            Dictionary<string, TimerData> timers;                 // Dictionary to store active timers
   public readonly   Dictionary<string, string>    remainingTimes;         // Dictionary to store active timers for UI

   /// <summary>
   /// Initializes a new instance of the <see cref="TimerService"/> class.
   /// <list type="bullet">
   /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
   /// </list> </summary>
   public TimerService(GagSpeakConfig config, CharacterHandler characterHandler, InfoRequestEvent infoRequestEvent) {
      _config = config;
      _characterHandler = characterHandler;
      _infoRequestEvent = infoRequestEvent;
      timers = new Dictionary<string, TimerData>();
      remainingTimes = new Dictionary<string, string>();
      // this is only called every time the plugin is loaded, so call restoretimerdata to restore any active timers.
      RestoreTimerData(_config);
   }

   /// <summary>
   /// The main timer constructor to start a new timer.
   /// <list type="bullet">
   /// <item><c>timerName</c><param name="timerName"> - The name of the timer.</param></item>
   /// <item><c>input</c><param name="input"> - The input string for how long the timer should be in a regex format XdXhXmXs.</param></item>
   /// <item><c>elapsedMilliSecPeriod</c><param name="elapsedMilliSecPeriod"> - how frequently will the onElapsedTime trigger (in milliseconds).</param></item>
   /// <item><c>onElapsed</c><param name="onElapsed"> - The action to invoke when the timer elapses.</param></item>
   /// </list> </summary>
   public void StartTimer(string timerName, string input, int elapsedMilliSecPeriod, Action onElapsed) {
      StartTimer(timerName, input, elapsedMilliSecPeriod, onElapsed, _characterHandler.playerChar._selectedGagPadlockTimer, -1);}
   
   /// <summary>
   /// The augmented timer constructor to start a new timer.
   /// <list type="bullet">
   /// <item><c>timerName</c><param name="timerName"> - The name of the timer.</param></item>
   /// <item><c>input</c><param name="input"> - The input string for how long the timer should be in a regex format XdXhXmXs.</param></item>
   /// <item><c>elapsedMilliSecPeriod</c><param name="elapsedMilliSecPeriod"> - how frequently will the onElapsedTime trigger (in milliseconds).</param></item>
   /// <item><c>onElapsed</c><param name="onElapsed"> - The action to invoke when the timer elapses.</param></item>
   /// <item><c>padlockTimerList</c><param name="padlockTimerList"> - The list of padlock timers to update.</param></item>
   /// <item><c>index</c><param name="index"> - The index of the padlock timer list to update.</param></item>
   /// </list> </summary>
   public void StartTimer(string timerName, string input, int elapsedMilliSecPeriod, Action onElapsed,
   List<DateTimeOffset> padlockTimerList, int index) {
      // If the new timer's name contains "_Identifier{index}"
      if (timerName.Contains($"_Identifier{index}")) {
         // Find any existing timer with "_Identifier{index}" in its name
         var existingTimerKey = timers.Keys.FirstOrDefault(key => key.Contains($"_Identifier{index}"));
         // remove dat timer first so we can replace it!
         if (existingTimerKey != null) {
               timers.Remove(existingTimerKey);
         }
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
      DateTimeOffset endTime = DateTimeOffset.Now.Add(duration);

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
      DebugPrintRemainingTimers(); //For the stats nerds.
   }

   /// <summary>
   /// What we should look for whenever the timer elapses its scheduled millisecond interval.
   /// <list type="bullet">
   /// <item><c>timerName</c><param name="timerName"> - The name of the timer.</param></item>
   /// <item><c>timer</c><param name="timer"> - The timer object.</param></item>
   /// <item><c>onElapsed</c><param name="onElapsed"> - The action to invoke when the timer elapses.</param></item>
   /// </list> </summary>
   private void OnTimerElapsed(string timerName, Timer timer, Action onElapsed) {
      // Check if the timer still exists
      if (timers.TryGetValue(timerName, out var timerData)) {
         // Calculate remaining time
         TimeSpan remainingTime = timerData.EndTime - DateTimeOffset.Now;
         // if the remaining time is less than zero, then the timer has expired.
         if (remainingTime <= TimeSpan.Zero) {
               GagSpeak.Log.Debug($"[Timer Service]: '{timerName}'has reached zero, dumping timer.");
               timer.Stop(); // stop the timer in the timer service's TIMERS dictionary
               onElapsed?.Invoke(); // invoke the action that you put in the _timerService.StartTimer() method
               timers.Remove(timerName); // remove the timer from the timer service's TIMERS dictionary
               SaveTimerData(_config); // save the timerdata so that we properly update the config's timerdata with the correct
               // check if the info request condition is met, and if so, invoke the event.
               CheckForInfoRequestInvokeConditoin();
         }
         // if the remaining time is greater than zero, then the timer is still active.
         else {
               // Notify subscribers about remaining time change
               RemainingTimeChanged?.Invoke(timerName, remainingTime);
         }
      }
   }

   /// <summary>
   /// Checks to see if someone is requesting info and if a timer by the name of "InteractionCooldown" either
   /// isnt in the timerservice AND the configs sendInfoName is not null or empty.
   /// </summary>
   public void CheckForInfoRequestInvokeConditoin() {
      // first check to see if our interaction cooldown timer is gone, and if so, invoke the info request condition
      GagSpeak.Log.Debug($"[Timer Service]: Checking for info request invoke condition...");
      if (!timers.ContainsKey("interactionButtonPressed") && !string.IsNullOrEmpty(_config.sendInfoName) && _config.acceptingInfoRequests) {
         GagSpeak.Log.Debug($"[Timer Service]: Info request invoke condition met, invoking event...");
         _infoRequestEvent.Invoke();
      }
   }

   /// <summary>
   /// A function to be called by external classes when they wish to view the duration left on all active timers
   /// </summary>
   public void DebugPrintRemainingTimers() {
      // Print the remaining time for each timer in the timers dictionary
      foreach (var pair in timers) {
         // Calculate the remaining time in milliseconds
         var remainingTime = UIHelpers.FormatTimeSpan(pair.Value.EndTime - DateTimeOffset.Now);
         // Print the timer name and remaining time
         GagSpeak.Log.Debug($"[Timer Service] Timer            : {pair.Key}, Remaining Time: {remainingTime}");
      }
      // print the remaining time for each timer in the config.timerdata dictionary
      foreach (var pair in _config.timerData) {
         // Calculate the remaining time in milliseconds
         var remainingTime = UIHelpers.FormatTimeSpan(pair.Value - DateTimeOffset.Now);
         // Print the timer name and remaining time
         GagSpeak.Log.Debug($"[Timer Service] Config Timer Data: {pair.Key}, Remaining Time: {remainingTime}");
      }
   }

   /// <summary>
   /// a function called that is used to parse the XdXhXmXs format into a timespan object.
   /// <list type="bullet">
   /// <item><c>input</c><param name="input"> - The input string for how long the timer should be in a regex format XdXhXmXs.</param></item>
   /// </list> </summary>
   /// <returns>The timespan object of the parsed input string.</returns>
   public static TimeSpan ParseTimeInput(string input) {
      // Match hours, minutes, and seconds in the input string
      var match = Regex.Match(input, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
      // If the input string is in the correct format
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


   /// <summary>
   /// Get the remaining time on a spesific identifierpadlock
   /// <list type="bullet">
   /// <item><c>slot</c><param name="slot"> - The slot of the padlock.</param></item>
   /// </list> </summary>
   /// <returns>The remaining time on the padlock.</returns>
   public string GetRemainingTimeForPadlock(int slot) {
      // get our padlocktype from the config
      var padlockType = _config.padlockIdentifier[slot]._padlockType;
      // return the remaining time for the padlock in string format.
      return $"{remainingTimes.GetValueOrDefault($"{padlockType}_Identifier{slot}", "Time Remaining:")}";
   }

   /// <summary>
   /// save /update the timer data dictionary to the current data in the timers dictionary.
   /// <list type="bullet">
   /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
   /// </list> </summary>
   public void SaveTimerData(GagSpeakConfig config) {
      // Clear the existing timer data in the config
      config.timerData.Clear();
      // Add the current timer data to the config
      foreach (var pair in timers) {
         config.timerData[pair.Key] = pair.Value.EndTime;
      }
      // Save the config
      config.Save();
   }

   /// <summary>
   /// Restore the timers from the stored timers in the config.
   /// <list type="bullet">
   /// <item><c>config</c><param name="config"> - The GagSpeak configuration.</param></item>
   /// </list> </summary>
   public void RestoreTimerData(GagSpeakConfig _config) {
      // Clear the existing timers
      timers.Clear();
      // Create a temporary list of timers to restore
      var timersToRestore = new List<(string, TimeSpan)>();
      // Populate the list from the config
      foreach (var pair in _config.timerData) {
         // Calculate the remaining time
         var remainingTime = pair.Value - DateTimeOffset.Now;
         // Add the timer to the list
         timersToRestore.Add((pair.Key, remainingTime));
      }

      // ADD EXTRA TIMERS HERE FOR THE RESTRAINT SET MANAGER LATER

      // Restore the timers from the list
      foreach (var (timerName, remainingTime) in timersToRestore) {
         // Create a new timer with the same name and remaining time, only need to care about identifiers
         if(timerName.Contains("_Identifier0")) {
            // Check to see if the timer expired while we were offline, if it is, clear the respective data
            if (remainingTime < TimeSpan.Zero) {
               GagSpeak.Log.Debug($"[Timer Service]: {timerName} Expired while you were logged out! (End Time: {remainingTime}). Unlocking and clearing!");
               _config.isLocked[0] = false;
               _config.padlockIdentifier[0].ClearPasswords();
               _config.padlockIdentifier[0].UpdateConfigPadlockInfo(0, !_config.isLocked[0], _characterHandler);
            } else {
               GagSpeak.Log.Debug($"[Timer Service]: Restoring {timerName} with end time {remainingTime}");
               StartTimer(timerName, UIHelpers.FormatTimeSpan(remainingTime), 1000, () => {
                  _config.isLocked[0] = false;
                  _config.padlockIdentifier[0].ClearPasswords();
                  _config.padlockIdentifier[0].UpdateConfigPadlockInfo(0, !_config.isLocked[0], _characterHandler);
               });
            }
         } else if(timerName.Contains("_Identifier1")) {
            // Check to see if the timer expired while we were offline, if it is, clear the respective data
            if (remainingTime < TimeSpan.Zero) {
               GagSpeak.Log.Debug($"[Timer Service]: {timerName} Expired while you were logged out! (End Time: {remainingTime}). Unlocking and clearing!");
               _config.isLocked[1] = false;
               _config.padlockIdentifier[1].ClearPasswords();
               _config.padlockIdentifier[1].UpdateConfigPadlockInfo(1, !_config.isLocked[1], _characterHandler);
            } else {
               // Check to see if the timer expired while we were offline, if it is, clear the respective data
               GagSpeak.Log.Debug($"[Timer Service]: Restoring timer {timerName} with end time {remainingTime}");
               StartTimer(timerName, UIHelpers.FormatTimeSpan(remainingTime), 1000, () => {
                  _config.isLocked[1] = false;
                  _config.padlockIdentifier[1].ClearPasswords();
                  _config.padlockIdentifier[1].UpdateConfigPadlockInfo(1, !_config.isLocked[1], _characterHandler);
               });
            }
         } else if(timerName.Contains("_Identifier2")) {
            // Check to see if the timer expired while we were offline, if it is, clear the respective data
            if (remainingTime < TimeSpan.Zero) {
               GagSpeak.Log.Debug($"[Timer Service]: {timerName} Expired while you were logged out! (End Time: {remainingTime}). Unlocking and clearing!");
               _config.isLocked[2] = false;
               _config.padlockIdentifier[2].ClearPasswords();
               _config.padlockIdentifier[2].UpdateConfigPadlockInfo(2, !_config.isLocked[2], _characterHandler);
            } else {
               // Check to see if the timer expired while we were offline, if it is, clear the respective data
               GagSpeak.Log.Debug($"[Timer Service]: Restoring timer {timerName} with end time {remainingTime}");
               StartTimer(timerName, UIHelpers.FormatTimeSpan(remainingTime), 1000, () => {
                  _config.isLocked[2] = false;
                  _config.padlockIdentifier[2].ClearPasswords();
                  _config.padlockIdentifier[2].UpdateConfigPadlockInfo(2, !_config.isLocked[2], _characterHandler);
               });
            }
         }
      }
   }

   // subclass to store timer data in the dictionary of timers with the timer and endtime.
   public class TimerData {
      public Timer Timer { get; }
      public DateTimeOffset EndTime { get; }
      // the constructor for this subclass
      public TimerData(Timer timer, DateTimeOffset endTime) {
         Timer = timer;
         EndTime = endTime;
      }
   }

   /// <summary>
   /// Clear all timers with the identifier in their name.
   /// </summary>
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

   /// <summary>
   /// Clear restraint set timers.
   /// </summary>
   public void ClearRestraintSetTimer() {
      // Get a list of keys that contain "RestraintSet_"
      var keysToRemove = new List<string>();
      foreach (var key in timers.Keys) {
         if (key.Contains("RestraintSet_")) {
               keysToRemove.Add(key);
         }
      }
      // Remove the timers with the keys in keysToRemove
      foreach (var key in keysToRemove) {
         timers[key].Timer.Dispose(); // Dispose the timer before removing it
         timers.Remove(key);
      }
      GagSpeak.Log.Debug($"[Timer Service Timer Cleaner]: Restraint Set Timers Cleared!");
   }

   /// <summary>
   /// Clear a spesific timer with 'identifier' in their name.
   /// <list type="bullet">
   /// <item><c>layerIndex</c><param name="layerIndex"> - the index of the timerdata in the config reflecting the identifier name.</param></item>
   /// </list> </summary>
   public void ClearIdentifierTimer(int layerIndex) {
      // Get a list of keys that contain "_Identifier"
      foreach (var key in timers.Keys) {
         // if the key contains the identifier, remove it.
         if (key.Contains($"_Identifier{layerIndex}")) {
            timers[key].Timer.Dispose(); // Dispose the timer before removing it
            timers.Remove(key);
            SaveTimerData(_config);
            // update the selectedGagPadLockTimer list with the new end time. (only if using a list as input)
            _characterHandler.playerChar._selectedGagPadlockTimer[layerIndex] = DateTimeOffset.Now;
         }
      }
   }

   /// <summary>
   /// Dispose of the timers and save the timer data to the config.
   /// </summary>
   public void Dispose() {
      // save timers upon unloading, printing all active ones to the debug log.
      GagSpeak.Log.Debug("[Timer Service] --------Saving & Unloading Timers-------");
      SaveTimerData(_config);
      DebugPrintRemainingTimers();
      GagSpeak.Log.Debug("[Timer Service] --------Timers (If Any) Now Saved--------");
      // Dispose all timers
      foreach (var timerData in timers.Values) {
         timerData.Timer.Dispose();
      }
   }
}
