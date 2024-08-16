// © Mike Murphy

using System;

namespace EMU7800.D2D.Shell;

public sealed class FrameRendererDefault : IFrameRenderer
{
    public void UpdateDynamicBitmapData(ReadOnlySpan<uint> palette, ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
    {
    }
}