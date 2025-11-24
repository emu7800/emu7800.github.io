// © Mike Murphy

using System;

namespace EMU7800.Shell;

public sealed class FrameRendererDefault : IFrameRenderer
{
    public void UpdateDynamicBitmapData(ReadOnlySpan<uint> palette, ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
    {
    }
}