using System;
using System.Timers;

namespace PowerConsole
{
    /// <summary>
    /// Represents an object that encapsulates data related to a timer event.
    /// </summary>
    public sealed class TimerEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimerEventArgs"/> class
        /// using the specified parameters.
        /// </summary>
        /// <param name="console">The used <see cref="SmartConsole"/>.</param>
        /// <param name="timer">An instance of a <see cref="Timer"/>.</param>
        /// <param name="signalTime">The date and time when the <see cref="Timer.Elapsed"/> event was raised.</param>
        /// <param name="ticks">The number of times the <see cref="Timer.Elapsed"/> event was raised.</param>
        public TimerEventArgs(SmartConsole console, Timer timer, DateTime signalTime, ulong ticks)
        {
            Console = console;
            Timer = timer;
            SignalTime = signalTime;
            Ticks = ticks;
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
    }
}