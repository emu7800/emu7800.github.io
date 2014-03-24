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
            if (frameBuffer == null)
            {
                for (var i = 0; i < TextureData.Length; i += 2)
                {
                    var c = (byte)SnowGenerator.Next(0xc0);
                    var bgr565 = (ushort)((((c >> 3) & 0x1f) << 11) | (((c >> 2) & 0x3f) << 5) | ((c >> 3) & 0x1f));
                    TextureData[i] = TextureData[i + 1] = bgr565;
                }
            }
            else
            {
                const int startSourceIndex = FirstScanline * BufferElementsPerScanline;
                const int endSourceIndex = startSourceIndex + BufferElementsPerScanline*Height;
                for (int si = startSourceIndex, di = 0; si < endSourceIndex; si++)
                {
                    var be = frameBuffer.VideoBuffer[si];
                    TextureData[di++] = CurrentPalette[be[0]];
                    TextureData[di++] = CurrentPalette[be[1]];
                    TextureData[di++] = CurrentPalette[be[2]];
                    TextureData[di++] = CurrentPalette[be[3]];
                }
            }

            SetTextureData();
        }

        #region Constructors

        public FrameRenderer320(int[] sourcePalette) : base(sourcePalette, Width, Height)
        {
        }

        #endregion
    }
}
