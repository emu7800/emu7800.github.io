// © Mike Murphy

using EMU7800.Core;
using System;

namespace EMU7800.D2D.Shell
{
    public sealed class FrameRenderer160Blender : IFrameRenderer
    {
        #region Fields

        const int
            Width  = 160,
            Height = 230;

        readonly int _startSourceIndex, _endSourceIndex;
        readonly FrameBuffer _frameBuffer;
        readonly Memory<byte> _dynamicBitmapData;

        bool _dynamicBitmapDataDelivered;

        #endregion

        #region IFrameRenderer Members

        public void UpdateDynamicBitmapData(ReadOnlyMemory<uint> palette)
        {
            var paletteSpan = palette.Span;
            var fbufSpan = _frameBuffer.VideoBuffer.Span;
            var outSpan = _dynamicBitmapData.Span;

            var di = 0;
            int r, g, b, ro, go, bo, rn, gn, bn, nc, ci;
            for (var si = _startSourceIndex; si < _endSourceIndex; si++)
            {
                ro = outSpan[di + 2];
                go = outSpan[di + 1];
                bo = outSpan[di];
                ci = fbufSpan[si];
                if (ci == 0 || ci == 1)
                {
                    if (_dynamicBitmapDataDelivered)
                    {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                    else
                    {
                        di += 8;
                        continue;
                    }
                }
                else
                {
                    nc = (int)paletteSpan[ci];
                    rn = (nc >> 16) & 0xff;
                    gn = (nc >> 8)  & 0xff;
                    bn =  nc        & 0xff;
                    r = (_dynamicBitmapDataDelivered || ro == 0) ? rn : (rn + ro) >> 1;
                    g = (_dynamicBitmapDataDelivered || go == 0) ? gn : (gn + go) >> 1;
                    b = (_dynamicBitmapDataDelivered || bo == 0) ? bn : (bn + bo) >> 1;
                }
                outSpan[di++] = (byte)b;
                outSpan[di++] = (byte)g;
                outSpan[di++] = (byte)r;
                di++;
                outSpan[di++] = (byte)b;
                outSpan[di++] = (byte)g;
                outSpan[di++] = (byte)r;
                di++;
            }

            _dynamicBitmapDataDelivered = false;
        }

        public void OnDynamicBitmapDataDelivered()
        {
            _dynamicBitmapDataDelivered = true;
        }

        #endregion

        #region Constructors

        public FrameRenderer160Blender(int firstVisibleScanline, FrameBuffer frameBuffer, Memory<byte> dynamicBitmapData)
        {
            _startSourceIndex = firstVisibleScanline * Width;
            _endSourceIndex = _startSourceIndex + Width * Height;
            _frameBuffer = frameBuffer ?? throw new ArgumentNullException(nameof(frameBuffer));
            _dynamicBitmapData = dynamicBitmapData;
        }

        #endregion
    }
}