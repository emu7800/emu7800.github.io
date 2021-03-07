// © Mike Murphy

using System;

namespace EMU7800.D2D.Shell
{
    public interface IFrameRenderer
    {
        void UpdateDynamicBitmapData(ReadOnlySpan<uint> palette);
        void OnDynamicBitmapDataDelivered();
    }
}