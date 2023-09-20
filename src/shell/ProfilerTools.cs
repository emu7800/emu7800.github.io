// © Mike Murphy

using System.Diagnostics;

namespace EMU7800.D2D.Shell;

public class DurationProfiler(Stopwatch stopwatch)
{
    #region Fields

    readonly Stopwatch _stopwatch = stopwatch;
    long _startTick, _accumulatedBeginEndTicks, _count;
    bool _requestReset = true;

    #endregion

    public double AvgMillisecondsPerSample => (_count > 0)
        ? 1000.0 * _accumulatedBeginEndTicks / _count / Stopwatch.Frequency : 0.0;

    public void Reset()
    {
        _requestReset = true;
    }

    public void Begin()
    {
        if (_requestReset)
        {
            _accumulatedBeginEndTicks = 0;
            _count = 0;
            _requestReset = false;
        }
        _startTick = _stopwatch.ElapsedTicks;
    }

    public void End()
    {
        var stopwatchElapsedTicks = _stopwatch.ElapsedTicks;
        _accumulatedBeginEndTicks += (stopwatchElapsedTicks - _startTick);
        _startTick = stopwatchElapsedTicks;
        _count++;
    }
}

public class RateProfiler(Stopwatch stopwatch)
{
    #region Fields

    readonly Stopwatch _stopwatch = stopwatch;
    long _startTick;
    bool _requestReset = true;

    #endregion

    public long SampleCount { get; private set; }

    public double SamplesPerSecond
    {
        get
        {
            if (SampleCount == 0)
                return 0.0;
            var elapsedTicks = _stopwatch.ElapsedTicks - _startTick;
            var avgTicksPerCount = elapsedTicks / SampleCount;
            return (double)Stopwatch.Frequency / avgTicksPerCount;
        }
    }

    public void Reset()
    {
        _requestReset = true;
    }

    public void Sample()
    {
        if (_requestReset)
        {
            _startTick = _stopwatch.ElapsedTicks;
            SampleCount = 0;
            _requestReset = false;
        }
        SampleCount++;
    }
}
