// © Mike Murphy

using System;
using EMU7800.Core;

namespace EMU7800.D2D.Shell
{
    public sealed class FrameRenderer160Blender : IFrameRenderer
    {
        #region Fields

        const int
            Width                     = 160 << 1,
            Height                    = 230,
            BufferElementsPerScanline = Width >> 1 >> BufferElement.SHIFT;

        readonly int _startSourceIndex, _endSourceIndex;
        readonly FrameBuffer _frameBuffer;
        readonly byte[] _dynamicBitmapData;

        bool _dynamicBitmapDataDelivered;

        #endregion

        #region IFrameRenderer Members

        public void UpdateDynamicBitmapData(uint[] palette)
        {
            if (palette == null)
                return;

            var di = 0;
            int r, g, b, ro, go, bo, rn, gn, bn, nc, ci;
            for (var si = _startSourceIndex; si < _endSourceIndex; si++)
            {
                var be = _frameBuffer.VideoBuffer[si];
                for (var k = 0; k < BufferElement.SIZE; k++)
                {
                    ro = _dynamicBitmapData[di + 2];
                    go = _dynamicBitmapData[di + 1];
                    bo = _dynamicBitmapData[di];
                    ci = be[k];
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
                        nc = (int)palette[ci];
                        rn = (nc >> 16) & 0xff;
                        gn = (nc >> 8)  & 0xff;
                        bn =  nc        & 0xff;
                        r = (_dynamicBitmapDataDelivered || ro == 0) ? rn : (rn + ro) >> 1;
                        g = (_dynamicBitmapDataDelivered || go == 0) ? gn : (gn + go) >> 1;
                        b = (_dynamicBitmapDataDelivered || bo == 0) ? bn : (bn + bo) >> 1;
                    }
                    _dynamicBitmapData[di++] = (byte)b;
                    _dynamicBitmapData[di++] = (byte)g;
                    _dynamicBitmapData[di++] = (byte)r;
                    di++;
                    _dynamicBitmapData[di++] = (byte)b;
                    _dynamicBitmapData[di++] = (byte)g;
                    _dynamicBitmapData[di++] = (byte)r;
                    di++;
                }
            }

            _dynamicBitmapDataDelivered = false;
        }

        public void OnDynamicBitmapDataDelivered()
        {
            _dynamicBitmapDataDelivered = true;
        }

        #endregion

        #region Constructors

        public FrameRenderer160Blender(int firstVisibleScanline, FrameBuffer frameBuffer, byte[] dynamicBitmapData)
        {
            if (frameBuffer == null)
                throw new ArgumentNullException("frameBuffer");
            if (dynamicBitmapData == null)
                throw new ArgumentNullException("dynamicBitmapData");

            _startSourceIndex = firstVisibleScanline * BufferElementsPerScanline;
            _endSourceIndex = _startSourceIndex + BufferElementsPerScanline * Height;
            _frameBuffer = frameBuffer;
            _dynamicBitmapData = dynamicBitmapData;
        }

        #endregion
    }
}