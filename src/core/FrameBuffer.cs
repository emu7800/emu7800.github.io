using System;
using EMU7800.Core.Extensions;

namespace EMU7800.Core;

public class FrameBuffer
{
    public static readonly FrameBuffer Default = new(1, 100);

    /// <summary>
    /// Number of visible pixels on a single horizontal line.
    /// </summary>
    public int VisiblePitch { get; }

    /// <summary>
    /// Number of visible scan lines.
    /// </summary>
    public int Scanlines { get; }

    /// <summary>
    /// The buffer containing computed pixel data.
    /// </summary>
    public Memory<byte> VideoBuffer { get; } = Memory<byte>.Empty;

    /// <summary>
    /// The buffer containing computed PCM audio data.
    /// </summary>
    public Memory<byte> SoundBuffer { get; } = Memory<byte>.Empty;

    #region Constructors

    internal FrameBuffer(int visiblePitch, int scanLines)
    {
        ArgumentException.ThrowIf(visiblePitch < 1, "visiblePitch must be 1 or greater", nameof(visiblePitch));
        ArgumentException.ThrowIf(scanLines < 1, "scanLines must be 1 or greater", nameof(scanLines));

        VisiblePitch = visiblePitch;
        Scanlines = scanLines;
        VideoBuffer = new Memory<byte>(new byte[VisiblePitch * Scanlines]);
        SoundBuffer = new Memory<byte>(new byte[Scanlines << 1]);
    }

    #endregion
}