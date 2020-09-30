using System;
using System.Collections.Concurrent;
using System.Data;
using System.Timers;

namespace PowerConsole
{
    internal static class TimerManager
    {
        private static readonly ConcurrentDictionary<string, TimerRef> _timers = 
            new ConcurrentDictionary<string, TimerRef>();

        internal static TimerRef Add(this SmartConsole console, Action<TimerEventArgs> callback, double interval, string name, bool repeat)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"{Guid.NewGuid().GetHashCode():x}";
            }

            if (_timers.ContainsKey(name))
            {
                throw new DuplicateNameException();
            }

            var timerRef = new TimerRef(console, callback, interval, name, repeat);
            _timers.TryAdd(name, timerRef);

            return timerRef;
        }

        internal static bool Remove(string name)
        {
            if (_timers.TryRemove(name, out var timerRef))
            {
                timerRef.Dispose();
                return true;
            }
            return false;
        }

        internal static int Clear()
        {
            var count = 0;

            foreach (var key in _timers.Keys)
            {
                if (Remove(key))
                {
                    count++;
                }
            }

            return count;
        }
    }

    sealed class TimerRef : IDisposable
    {
        private readonly Timer _timer;
        private readonly SmartConsole _console;
        private readonly Action<TimerEventArgs> _callback;
        private double _interval;
        private bool _disposedValue;
        private ulong _ticks;

        public TimerRef(SmartConsole console, Action<TimerEventArgs> callback, double interval, string name, bool repeat)
        {
            Interval = interval;

            _console = console ?? throw new ArgumentNullException(nameof(console));
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));

            Name = name;

            _timer = new Timer { AutoReset = repeat };
            _timer.Elapsed += TimerElapsed;
            
            if (interval > 0d)
            {
                _timer.Interval = interval;
                _timer.Enabled = true;
            }
        }

        public string Name { get; }

        public double Interval
        {
            get => _interval;
            set
            {
                if (value < 0d)
                    throw new ArgumentOutOfRangeException();

                if (value != _interval)
                {
                    _interval = value;
                }
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _callback.Invoke(new TimerEventArgs(_console, _timer, e.SignalTime, ++_ticks, Name));
            }
            finally
            {
                if (!_timer.AutoReset)
                {
                    TimerManager.Remove(Name);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Stop();
                    _timer.Elapsed -= TimerElapsed;
                    _timer.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
