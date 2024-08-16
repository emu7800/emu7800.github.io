// © Mike Murphy

using System;
using System.Diagnostics;

namespace EMU7800.D2D.Shell;

public sealed class TimerDevice
{
    #region Fields

    readonly Stopwatch _stopwatch = new();
    long _lastEndOfRenderingTick, _endOfRenderingTick;

    #endregion

    public static long Frequency => Stopwatch.Frequency;
    public static float SecondsPerTick => 1.0f / Stopwatch.Frequency;

    public int DeltaTicks { get; private set; }
    public float DeltaInSeconds { get; private set; }

    public void Update()
    {
        var tick = _stopwatch.ElapsedTicks;
        _lastEndOfRenderingTick = _endOfRenderingTick;
        _endOfRenderingTick = tick;

        DeltaTicks = (int)(_endOfRenderingTick - _lastEndOfRenderingTick);
        DeltaInSeconds = DeltaTicks * SecondsPerTick;
    }

    #region Constructors

    public TimerDevice()
    {
        if (!Stopwatch.IsHighResolution || Stopwatch.Frequency == 0)
            throw new NotSupportedException("High resolution timer not available");

        _stopwatch.Start();
    }

    #endregion
}