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
            BufferElement be;
            for (int si = startSourceIndex, di = 0; si < endSourceIndex; si++)
            {
                be = frameBuffer.VideoBuffer[si];
                for (var k = 0; k < BufferElement.SIZE; k++)
                {
                    oc = TextureData[di];
                    ro = (oc >> 11) & 0x1f;
                    go = (oc >> 5)  & 0x3f;
                    bo =  oc        & 0x1f;
                    ci = be[k];
                    if (ci == 0)
                    {
                        r = ((ro << 1) + ro) >> 2;
                        g = ((go << 1) + go) >> 2;
                        b = ((bo << 1) + bo) >> 2;
                    }
                    else
                    {
                        nc = CurrentPalette[ci];
                        rn = (nc >> 11) & 0x1f;
                        gn = (nc >> 5)  & 0x3f;
                        bn =  nc        & 0x1f;
                        r = (ro + ((rn << 1) + rn)) >> 2;
                        g = (go + ((gn << 1) + gn)) >> 2;
                        b = (bo + ((bn << 1) + bn)) >> 2;
                        if (r == ro && g == go && b == bo)
                        {
                            r = rn; g = gn; b = bn;
                        }
                    }
                    TextureData[di++] = (ushort)(((r & 0x1f) << 11) | ((g & 0x3f) << 5) | (b & 0x1f));
                }
            }
        }

        public override void Draw(FrameBuffer frameBuffer)
        {
            if (frameBuffer == null)
            {
                for (var i = 0; i < TextureData.Length; i++)
                {
                    var c = (byte)SnowGenerator.Next(0xc0);
                    var bgr565 = (ushort)((((c >> 3) & 0x1f) << 11) | (((c >> 2) & 0x3f) << 5) | ((c >> 3) & 0x1f));
                    TextureData[i] = bgr565;
                }
            }

            SetTextureData();
        }

        #region Constructors

        public FrameRenderer160Blender(int[] sourcePalette) : base(sourcePalette, Width, Height)
        {
        }

        #endregion
    }
}