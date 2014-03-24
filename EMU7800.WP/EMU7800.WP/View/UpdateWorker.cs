using System;
using System.Diagnostics;
using System.Threading;

namespace EMU7800.WP.View
{
    public sealed class UpdateWorker : IDisposable
    {
        #region Fields

        readonly object _locker = new object();
        readonly Stopwatch _stopwatch;
        readonly RateProfiler _profilerUpdateRequestRate;
        readonly RateProfiler _profilerUpdateActualRate;
        Thread _thread;
        long _startOfCalibrationIntervalMilliseconds;

        #endregion

        public event EventHandler Update;

        public int UpdateBacklogCount { get; private set; }

        public bool Stopped { get; private set; }

        public double UpdateRequestsPerSecond { get { return _profilerUpdateRequestRate.SamplesPerSecond; } }

        public double UpdatesPerSecond { get { return _profilerUpdateActualRate.SamplesPerSecond; } }

        public bool CalibrationInfoReady
        {
            get { return (_stopwatch.ElapsedMilliseconds - _startOfCalibrationIntervalMilliseconds) > 1000; }
        }

        public UpdateWorker(Stopwatch stopwatch)
        {
            _stopwatch = stopwatch;

            _thread = new Thread(RunWorker) { IsBackground = false };
            _profilerUpdateRequestRate = new RateProfiler(_stopwatch);
            _profilerUpdateActualRate = new RateProfiler(_stopwatch);
        }

        public void Start()
        {
            if (_thread == null || _thread.IsAlive)
                return;

            _thread.Start();
            ResetCalibrationInterval();
        }

        public void ResetCalibrationInterval()
        {
            _profilerUpdateRequestRate.Reset();
            _profilerUpdateActualRate.Reset();
            _startOfCalibrationIntervalMilliseconds = _stopwatch.ElapsedMilliseconds;
        }

        public void ResetBacklogCount()
        {
            lock (_locker)
            {
                UpdateBacklogCount = 0;
            }
        }

        public void RequestUpdate()
        {
            lock (_locker)
            {
                UpdateBacklogCount++;
                _profilerUpdateRequestRate.Sample();

                Monitor.Pulse(_locker);
            }
        }

        public void Stop()
        {
            lock (_locker)
            {
                Stopped = true;
                Monitor.Pulse(_locker);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_thread == null || !_thread.IsAlive)
                return;

            Stop();
            try
            {
                _thread.Join();
            }
            catch (ThreadStateException)
            {
            }
            _thread = null;
            Update = null;
        }

        #endregion

        #region Helpers

        void RunWorker()
        {
            while (true)
            {
                lock (_locker)
                {
                    while (UpdateBacklogCount == 0 && !Stopped)
                        Monitor.Wait(_locker);

                    if (Stopped)
                        return;

                    UpdateBacklogCount--;
                    _profilerUpdateActualRate.Sample();
                }

                if (Update != null)
                    Update(null, null);
            }
        }

        #endregion
    }
}