using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GagSpeak.Services;

/* this class will determine the many various actions for tracking the time of things, or issuing out punishments for inproper actions
   it will make use of the DateTimeOffset and TimeSpan classes to determine the time of things, and will also make use of the Timer class to determine the time of things
   inside of the service should be a function for when when a timer padlock is assigned, an inproper message is sent, or the safeword is used.
   they should all be able to store these times within a variable of the service, and be able to pass in a desireable time frame to have.
   the service should also be able to determine if the user is in a time out, and if so, how long they have left in the time out. */
public class TimerService
{
    
}