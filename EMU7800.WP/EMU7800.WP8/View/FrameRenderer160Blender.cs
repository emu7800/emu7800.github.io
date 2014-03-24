using EMU7800.Core;

namespace EMU7800.WP.View
{
    public sealed class FrameRenderer160Blender : FrameRenderer
    {
        const int
            Width                     = 160,
            Height                    = 230,
            BufferElementsPerScanline = Width >> BufferElement.SHIFT,
            FirstScanline             = 16;

        public override void Update(FrameBuffer frameBuffer)
        {
            if (frameBuffer == null)
                return;

            const int startSourceIndex = FirstScanline * BufferElementsPerScanline;
            const int endSourceIndex = startSourceIndex + BufferElementsPerScanline * Height;
            int r, g, b, ro, go, bo, rn, gn, bn, oc, nc, ci;
            for (int si = startSourceIndex, di = 0; si < endSourceIndex; si++)
            {
                var be = frameBuffer.VideoBuffer[si];
                for (var k = 0; k < BufferElement.SIZE; k++)
                {
                    oc = (int)TextureData[di];
                    ro =  oc        & 0xff;
                    go = (oc >>  8) & 0xff;
                    bo = (oc >> 16) & 0xff;
                    ci = be[k];
                    if (ci == 0)
                    {
                        r = ((ro << 1) + ro) >> 2;
                        g = ((go << 1) + go) >> 2;
                        b = ((bo << 1) + bo) >> 2;
                    }
                    else
                    {
                        nc = (int)CurrentPalette[ci];
                        rn =  nc        & 0xff;
                        gn = (nc >>  8) & 0xff;
                        bn = (nc >> 16) & 0xff;
                        r = (ro + ((rn << 1) + rn)) >> 2;
                        g = (go + ((gn << 1) + gn)) >> 2;
                        b = (bo + ((bn << 1) + bn)) >> 2;
                        if (r == ro && g == go && b == bo)
                        {
                            r = rn; g = gn; b = bn;
                        }
                    }
                    var bgr = (uint)((0xff << 24) | (b << 16) | (g << 8) | r);
                    TextureData[di++] = bgr;
                    TextureData[di++] = bgr;
                }
            }
        }

        public override void Draw(FrameBuffer frameBuffer)
        {
            if (frameBuffer != null)
                return;

            const int startSourceIndex = FirstScanline * BufferElementsPerScanline;
            const int endSourceIndex = startSourceIndex + BufferElementsPerScanline * Height;

            for (int si = startSourceIndex, di = 0; si < endSourceIndex; si++)
            {
                for (var i = 0; i < 8; i++)
                {
                    var c = (byte)SnowGenerator.Next(0xc0);
                    var bgr = (uint)((0xff << 24) | (c << 16) | (c << 8) | c);
                    TextureData[di++] = bgr;
                }
            }
        }

        #region Constructors

        public FrameRenderer160Blender(int[] sourcePalette) : base(sourcePalette)
        {
        }

        #endregion
    }
}