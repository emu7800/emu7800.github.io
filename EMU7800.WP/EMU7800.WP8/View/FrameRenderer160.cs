using EMU7800.Core;

namespace EMU7800.WP.View
{
    public sealed class FrameRenderer160 : FrameRenderer
    {
        const int
            Width                     = 160,
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
                    for (var i = 0; i < 8; i++)
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

                    var c = CurrentPalette[be[0]];
                    TextureData[di++] = c;
                    TextureData[di++] = c;

                    c = CurrentPalette[be[1]];
                    TextureData[di++] = c;
                    TextureData[di++] = c;

                    c = CurrentPalette[be[2]];
                    TextureData[di++] = c;
                    TextureData[di++] = c;

                    c = CurrentPalette[be[3]];
                    TextureData[di++] = c;
                    TextureData[di++] = c;
                }
            }
        }

        #region Constructors

        public FrameRenderer160(int[] sourcePalette) : base(sourcePalette)
        {
        }

        #endregion
    }
}