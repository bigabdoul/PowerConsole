using System;
using System.Timers;

namespace PowerConsole
{
    /// <summary>
    /// Represents an object that encapsulates data related to a timer event.
    /// </summary>
    public sealed class TimerEventArgs
    {
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerEventArgs"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="timer">An instance of a <see cref="Timer"/>.</param>
        /// <param name="signalTime">The date and time when the <see cref="Timer.Elapsed"/> event was raised.</param>
        /// <param name="ticks">The number of times the <see cref="Timer.Elapsed"/> event was raised.</param>
        /// <param name="name">The name of the timer reference.</param>
        public TimerEventArgs(SmartConsole console, Timer timer, DateTime signalTime, ulong ticks, string name)
        {
            Console = console;
            Timer = timer;
            SignalTime = signalTime;
            Ticks = ticks;
            _name = name;
        }

        /// <summary>
        /// Gets the console that registered the timer.
        /// </summary>
        public SmartConsole Console { get; }

        /// <summary>
        /// Gets the timer that raised the current event.
        /// </summary>
        public Timer Timer { get; }

        /// <summary>
        /// Gets the date and time when the <see cref="Timer.Elapsed"/> event was raised.
        /// </summary>
        public DateTime SignalTime { get; }

        /// <summary>
        /// Gets the number of times the <see cref="Timer.Elapsed"/> event was raised.
        /// </summary>
        public ulong Ticks { get; }

        /// <summary>
        /// Instructs the internal TimerManager to remove the reference to the 
        /// <see cref="Timer"/> and dispose off all related objects.
        /// </summary>
        /// <returns>true if the reference has been removed, otherwise false.</returns>
        public bool DisposeTimer() => TimerManager.Remove(_name);

        /// <summary>
        /// Returns <see cref="Ticks"/> formatted as "hh:mm:ss"
        /// representing the number of seconds elapsed.
        /// </summary>
        /// <returns>A formatted string that represents the time that elapsed.</returns>
        public string TicksToSecondsElapsed()
        {
            return $"{TimeSpan.FromSeconds(Ticks):hh\\:mm\\:ss}";
        }
    }
}