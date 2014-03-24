using EMU7800.Core;

namespace EMU7800.WP.View
{
    public sealed class FrameRenderer320 : FrameRenderer
    {
        const int
            Width                     = 320,
            Height                    = 230,
            BufferElementsPerScanline = Width >> BufferElement.SHIFT,
            FirstScanline             = 16;

        public override void Update(FrameBuffer frameBuffer)
        {
        }

        public override void Draw(FrameBuffer frameBuffer)
        {
            const int startSourceIndex = FirstScanline * BufferElementsPerScanline;
            const int endSourceIndex = startSourceIndex + BufferElementsPerScanline * Height;

            if (frameBuffer == null)
            {
                for (int si = startSourceIndex, di = 0; si < endSourceIndex; si++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        var c = (byte) SnowGenerator.Next(0xc0);
                        var bgr = (uint) ((0xff << 24) | (c << 16) | (c << 8) | c);
                        TextureData[di++] = bgr;
                    }
                }
            }
            else
            {
                for (int si = startSourceIndex, di = 0; si < endSourceIndex; si++)
                {
                    var be = frameBuffer.VideoBuffer[si];
                    TextureData[di++] = CurrentPalette[be[0]];
                    TextureData[di++] = CurrentPalette[be[1]];
                    TextureData[di++] = CurrentPalette[be[2]];
                    TextureData[di++] = CurrentPalette[be[3]];
                }
            }
        }

        #region Constructors

        public FrameRenderer320(int[] sourcePalette) : base(sourcePalette)
        {
        }

        #endregion
    }
}
