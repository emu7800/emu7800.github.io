// © Mike Murphy

using System;

namespace EMU7800.D2D.Shell
{
    public sealed class FrameRenderer160 : IFrameRenderer
    {
        #region Fields

        const int Width = 160, Height = 230;

        readonly int _startSourceIndex, _endSourceIndex;

        #endregion

        #region IFrameRenderer Members

        public void UpdateDynamicBitmapData(ReadOnlySpan<uint> palette, ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
        {
            for (int si = _startSourceIndex, di = 0; si < _endSourceIndex; si++)
            {
                var nc = palette[inputBuffer[si]];
                var rn = (nc >> 16) & 0xff;
                var gn = (nc >> 8)  & 0xff;
                var bn = (nc >> 0)  & 0xff;
                outputBuffer[di++] = (byte)bn;
                outputBuffer[di++] = (byte)gn;
                outputBuffer[di++] = (byte)rn;
                di++;
                outputBuffer[di++] = (byte)bn;
                outputBuffer[di++] = (byte)gn;
                outputBuffer[di++] = (byte)rn;
                di++;
            }
        }

        #endregion

        #region Constructors

        public FrameRenderer160(int firstVisibleScanline)
        {
            _startSourceIndex = firstVisibleScanline * Width;
            _endSourceIndex = _startSourceIndex + Width * Height;
         }

        #endregion
    }
}