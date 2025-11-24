// © Mike Murphy

using System;

namespace EMU7800.Shell;

public interface IFrameRenderer
{
    void UpdateDynamicBitmapData(ReadOnlySpan<uint> palette, ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer);
}