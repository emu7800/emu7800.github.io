// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.D2D.Shell
{
    public sealed class FrameRenderer160 : IFrameRenderer
    {
        #region Fields

        const int
            Width  = 160,
            Height = 230;

        readonly int _startSourceIndex, _endSourceIndex;
        readonly FrameBuffer _frameBuffer;
        readonly Memory<byte> _dynamicBitmapData;

        #endregion

        #region IFrameRenderer Members

        public void UpdateDynamicBitmapData(ReadOnlyMemory<uint> palette)
        {
            var paletteSpan = palette.Span;
            var fbufSpan = _frameBuffer.VideoBuffer.Span;
            var outSpan = _dynamicBitmapData.Span;

            for (int si = _startSourceIndex, di = 0; si < _endSourceIndex; si++)
            {
                var nc = paletteSpan[fbufSpan[si]];
                var rn = (nc >> 16) & 0xff;
                var gn = (nc >> 8)  & 0xff;
                var bn = (nc >> 0)  & 0xff;
                outSpan[di++] = (byte)bn;
                outSpan[di++] = (byte)gn;
                outSpan[di++] = (byte)rn;
                di++;
                outSpan[di++] = (byte)bn;
                outSpan[di++] = (byte)gn;
                outSpan[di++] = (byte)rn;
                di++;
            }
        }

        public void OnDynamicBitmapDataDelivered()
        {
        }

        #endregion

        #region Constructors

        public FrameRenderer160(int firstVisibleScanline, FrameBuffer frameBuffer, Memory<byte> dynamicBitmapData)
        {
            _startSourceIndex = firstVisibleScanline * Width;
            _endSourceIndex = _startSourceIndex + Width * Height;
            _frameBuffer = frameBuffer ?? throw new ArgumentNullException(nameof(frameBuffer));
            _dynamicBitmapData = dynamicBitmapData;
        }

        #endregion
    }
}