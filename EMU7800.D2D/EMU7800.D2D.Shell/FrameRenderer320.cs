// © Mike Murphy

using System;
using EMU7800.Core;

namespace EMU7800.D2D.Shell
{
    public sealed class FrameRenderer320 : IFrameRenderer
    {
        #region Fields

        const int
            Width                     = 320,
            Height                    = 230,
            BufferElementsPerScanline = Width >> BufferElement.SHIFT;

        readonly int _startSourceIndex, _endSourceIndex;
        readonly FrameBuffer _frameBuffer;
        readonly byte[] _dynamicBitmapData;

        #endregion

        #region IFrameRenderer Members

        public void UpdateDynamicBitmapData(uint[] palette)
        {
            if (palette == null)
                return;

            for (int si = _startSourceIndex, di = 0; si < _endSourceIndex; si++)
            {
                var be = _frameBuffer.VideoBuffer[si];
                for (var k = 0; k < BufferElement.SIZE; k++)
                {
                    var ci = be[k];
                    var nc = palette[ci];
                    var rn = (nc >> 16) & 0xff;
                    var gn = (nc >> 8)  & 0xff;
                    var bn = (nc >> 0)  & 0xff;
                    _dynamicBitmapData[di++] = (byte)bn;
                    _dynamicBitmapData[di++] = (byte)gn;
                    _dynamicBitmapData[di++] = (byte)rn;
                    di++;
                }
            }
        }

        public void OnDynamicBitmapDataDelivered()
        {
        }

        #endregion

        #region Constructors

        public FrameRenderer320(int firstVisibleScanline, FrameBuffer frameBuffer, byte[] dynamicBitmapData)
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
