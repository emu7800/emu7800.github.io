/*
 * Rectangulator
 * 
 * Determines differences between frames and issues draw rectangle primitives
 *
 * Copyright © 2003, 2004 Mike Murphy
 *
 */
using System.Drawing;
using EMU7800.Core;

namespace EMU7800.Win.Gdi
{
    internal struct DisplRect
    {
        internal byte Colu;
        internal int Argb;
        internal int X, Y, Width, Height;

        byte DimmedColu
        {
            get
            {
                var co = (byte)(Colu & 0xf0);
                var lu = (byte)(Colu & 0x0f);
                if (lu >= 2)
                {
                    lu -= 2;
                }
                return (byte)(co | lu);
            }
        }

        internal void SetArgb(int[] palette, bool pauseDim)
        {
            unchecked { Argb = (int)0xff000000 | palette[pauseDim ? DimmedColu : Colu]; }
        }

        internal int Right
        {
            get { return X + Width; }
        }

        internal Rectangle Rectangle
        {
            get { return new Rectangle(X, Y, Width, Height); }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }
    }

    internal delegate void UpdateRectHandler(DisplRect r);

    internal sealed class Rectangulator
    {
        public UpdateRectHandler UpdateRect;
        public int FrameRectCount;
        public int[] Palette;

        // Transformation parameters
        public Size ViewPortSize;
        public int PixelAspectXRatio;  // pixel aspect ration: width/height
        public int OffsetLeft;
        public int ClipTop, ClipHeight;

        // Internal transformation parameters
        Size CenteringOffset, PixelSize;
        int ClipBottom;

        readonly byte[] FrameBuffer;
        Size FrameBufferSize;
        bool PauseDim;

        DisplRect[] aRects;
        DisplRect[] cRects;
        int aIdx, aCount;
        int cIdx, cCount;
        bool ForceDifference;

        public void DrawEntireFrame(bool pauseDim)
        {
            ForceDifference = true;
            PauseDim = pauseDim;
            StartFrame();
            var w = FrameBufferSize.Width;
            var h = FrameBufferSize.Height;
            var buff = new byte[w];
            for (var i = 0; i < h; i++)
            {
                for (var j = 0; j < w; j++)
                {
                    buff[j] = FrameBuffer[i * w + j];
                }
                InputScanline(buff, i, 0, w);
            }
            EndFrame();
            PauseDim = false;
            ForceDifference = false;
        }

        public void StartFrame()
        {
            // Empty both rectangle lists
            aIdx = 0;
            aCount = 0;
            cIdx = 0;
            cCount = 0;

            FrameRectCount = 0;
        }

        public void EndFrame()
        {
            // Flush any remaining rectangles on the active list
            while (aIdx < aCount)
            {
                DoRectUpdated(aRects[aIdx]);
                aIdx++;
            }
        }

        public void InputScanline(byte[] scanlineBuffer, int scanline, int hposStart, int updateClocks)
        {
            if (scanline < ClipTop || scanline >= FrameBufferSize.Height || scanline >= ClipBottom)
                return;

            var sli = hposStart;
            var fbi = scanline * FrameBufferSize.Width + hposStart;

            // Build the current rectangle list, horizontally merging when possible
            while (updateClocks-- > 0 && sli < FrameBufferSize.Width)
            {
                var colu = scanlineBuffer[sli];
                if (colu != FrameBuffer[fbi] || ForceDifference)
                {
                    if (cCount > 0 && colu == cRects[cCount - 1].Colu
                        && cRects[cCount - 1].Right == sli)
                    {
                        cRects[cCount - 1].Width++;
                    }
                    else
                    {
                        cRects[cCount].Colu = colu;
                        cRects[cCount].Rectangle = new Rectangle(sli, scanline, 1, 1);
                        cCount++;
                    }
                    FrameBuffer[fbi] = colu;
                }
                sli++; fbi++;
            }

            // Exit early if we are not at the end of the scanline
            if (sli < scanlineBuffer.Length)
                return;

            // Look for opportunities to vertically merge with
            // rectangles on the active list, otherwise flush the
            // unmergable active rectangles
            while (cIdx < cCount && aIdx < aCount)
            {
                if (cRects[cIdx].X > aRects[aIdx].X)
                {
                    DoRectUpdated(aRects[aIdx]);
                    aIdx++;
                }
                else if (cRects[cIdx].X == aRects[aIdx].X
                        && cRects[cIdx].Width == aRects[aIdx].Width
                        && cRects[cIdx].Colu == aRects[aIdx].Colu)
                {
                    cRects[cIdx] = aRects[aIdx];
                    cRects[cIdx].Height++;
                    aIdx++;
                }
                cIdx++;
            }

            // Flush any remaining active rectangles that have no hope for subsequent merging
            while (aIdx < aCount)
            {
                DoRectUpdated(aRects[aIdx]);
                aIdx++;
            }

            // For the next scanline, make the current rectangle list the
            // new active list, and then empty the current list
            var swapTmp = aRects;

            aRects = cRects;
            aIdx = 0;
            aCount = cCount;

            cRects = swapTmp;
            cIdx = 0;
            cCount = 0;
        }

        public void UpdateTransformationParameters()
        {
            var xfactor = ViewPortSize.Width / (FrameBufferSize.Width / PixelAspectXRatio);
            var yfactor = ViewPortSize.Height / ClipHeight;
            var minfactor = xfactor <= yfactor ? xfactor : yfactor;

            PixelSize = new Size(PixelAspectXRatio * minfactor, minfactor);

            CenteringOffset = new Size
            {
                Width = ((ViewPortSize.Width - FrameBufferSize.Width*PixelSize.Width) >> 1),
                Height = ((ViewPortSize.Height - ClipHeight*PixelSize.Height) >> 1)
            };

            ClipBottom = ClipTop + ClipHeight;
        }

        public Rectangulator(int hpixels, int scanlines)
        {
            FrameBufferSize = new Size(hpixels, scanlines);
            FrameBuffer = new byte[hpixels * scanlines];
            aRects = new DisplRect[hpixels];
            cRects = new DisplRect[hpixels];

            ViewPortSize = new Size(320, 240);
            PixelAspectXRatio = 1;
            ClipHeight = 240;
            UpdateTransformationParameters();

            Palette = TIATables.NTSCPalette;
        }

        void DoRectUpdated(DisplRect r)
        {
            r.SetArgb(Palette, PauseDim);

            if (UpdateRect != null)
            {
                TransformRect(ref r);
                if (ClipRect(ref r))
                {
                    UpdateRect(r);
                    FrameRectCount++;
                }
            }
        }

        void TransformRect(ref DisplRect r)
        {
            r.X -= OffsetLeft;
            r.Y -= ClipTop;

            r.X *= PixelSize.Width;
            r.Y *= PixelSize.Height;
            r.Width *= PixelSize.Width;
            r.Height *= PixelSize.Height;

            r.X += CenteringOffset.Width;
            r.Y += CenteringOffset.Height;
        }

        bool ClipRect(ref DisplRect r)
        {
            if (r.X >= ViewPortSize.Width || r.Right < 0)
            {
                return false;
            }
            if (r.X < 0)
            {
                r.Width += r.X;
                r.X = 0;
            }
            if (r.Right >= ViewPortSize.Width)
            {
                r.Width -= r.Right - ViewPortSize.Width;
            }
            return r.Width > 0;
        }
    }
}