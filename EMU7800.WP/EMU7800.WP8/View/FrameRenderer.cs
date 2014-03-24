using EMU7800.Core;
using System;
using System.Collections.Generic;

namespace EMU7800.WP.View
{
    public abstract class FrameRenderer
    {
        #region Fields

        readonly static uint[] _palette = new uint[256];
        readonly static uint[] _hudPalette = new uint[256];

        protected static readonly Random SnowGenerator = new Random();
        protected uint[] CurrentPalette;

        static readonly byte[] _controllerSprites =
        {
              0,    0,    0,    0,    0, // 0: blank
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,

              0,    0,    0,    0,    0, // 1: left
              0,    0,    0,    0,    0,
           0xff, 0xff, 0xff,    0,    0,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,

           0xff,    0,    0,    0,    0, // 2: left/up
              0, 0xff,    0,    0,    0,
              0,    0, 0xff,    0,    0,
              0,    0, 0   ,    0,    0,
              0,    0, 0   ,    0,    0,

              0,    0, 0xff,    0,    0, // 3: up
              0,    0, 0xff,    0,    0,
              0,    0, 0xff,    0,    0,
              0,    0, 0   ,    0,    0,
              0,    0, 0   ,    0,    0,

              0,    0,    0,    0, 0xff, // 4: up/right
              0,    0,    0, 0xff,    0,
              0,    0, 0xff,    0,    0,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,

              0,    0,    0,    0,    0, // 5: right
              0,    0,    0,    0,    0,
              0,    0, 0xff, 0xff, 0xff,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,

              0,    0,    0,    0,    0, // 6: right/down
              0,    0,    0,    0,    0,
              0,    0, 0xff,    0,    0,
              0,    0,    0, 0xff,    0,
              0,    0,    0,    0, 0xff,

              0,    0,    0,    0,    0, // 7: down
              0,    0,    0,    0,    0,
              0,    0, 0xff,    0,    0,
              0,    0, 0xff,    0,    0,
              0,    0, 0xff,    0,    0,

              0,    0,    0,    0,    0, // 8: left/down
              0,    0,    0,    0,    0,
              0,    0, 0xff,    0,    0,
              0, 0xff,    0,    0,    0,
           0xff,    0,    0,    0,    0,

              0,    0,    0,    0,    0, // 9: fire2
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,
              0, 0xff,    0,    0,    0,
           0xff,    0,    0,    0,    0,

              0,    0,    0,    0, 0xff, // 10: fire1
              0,    0,    0, 0xff,    0,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,
              0,    0,    0,    0,    0,

              0,    0,    0,    0, 0xff, // 11: fire1/2
              0,    0,    0, 0xff,    0,
              0,    0,    0,    0,    0,
              0, 0xff,    0,    0,    0,
           0xff,    0,    0,    0,    0,

           0xff,    0,    0,    0, 0xff, // 12: Moga connected
           0xff, 0xff,    0, 0xff, 0xff,
           0xff,    0, 0xff,    0, 0xff,
           0xff,    0,    0,    0, 0xff,
           0xff,    0,    0,    0, 0xff
        };

        #endregion

        public uint[] TextureData { get; private set; }

        public void NotifyHudIsUp()
        {
            CurrentPalette = _hudPalette;
        }

        public void NotifyHudIsDown()
        {
            CurrentPalette = _palette;
        }

        public virtual void Update(FrameBuffer frameBuffer)
        {
        }

        public virtual void Draw(FrameBuffer frameBuffer)
        {
        }

        public void DrawDPadControllerFeedback(bool isLeft, bool isUp, bool isRight, bool isDown)
        {
            var index = 0;

            if (      isLeft && !isUp && !isRight && !isDown)
                index = 1;
            else if ( isLeft &&  isUp && !isRight && !isDown)
                index = 2;
            else if (!isLeft &&  isUp && !isRight && !isDown)
                index = 3;
            else if (!isLeft &&  isUp &&  isRight && !isDown)
                index = 4;
            else if (!isLeft && !isUp &&  isRight && !isDown)
                index = 5;
            else if (!isLeft && !isUp &&  isRight)
                index = 6;
            else if (!isLeft && !isUp &&  isDown)
                index = 7;
            else if ( isLeft && !isUp && !isRight)
                index = 8;

            DrawGraphic(index, 0, 0);
        }

        public void DrawFireButtonControllerFeedback(bool isFire1, bool isFire2)
        {
            var index = 0;

            if (isFire1 && !isFire2)
                index = 10;
            else if (!isFire1 && isFire2)
                index = 9;
            else if (isFire1)
                index = 11;

            DrawGraphic(index, 320-5, 0);
        }

        public void DrawMogaConnectedFeedback()
        {
            DrawGraphic(12, 0, 0);
        }

        #region Constructors

        protected FrameRenderer(IList<int> sourcePalette)
        {
            if (sourcePalette == null)
                throw new ArgumentNullException("sourcePalette");
            if (sourcePalette.Count != 256)
                throw new ArgumentException("sourcePalette");

            TextureData = new uint[320*240];

            for (var i = 0; i < _palette.Length; i++)
            {
                var color = sourcePalette[i];
                var r = (color >> 16) & 0xFF;
                var g = (color >> 8)  & 0xFF;
                var b =  color        & 0xFF;

                _palette[i]    = (uint)((0xff << 24) |  (b       << 16) |  (g       << 8) |  r      );
                _hudPalette[i] = (uint)((0xff << 24) | ((b >> 1) << 16) | ((g >> 1) << 8) | (r >> 1));
            }

            CurrentPalette = _palette;
        }

        #endregion

        #region Helpers

        void DrawGraphic(int index, int x, int y)
        {
            const int
                srcpitch = 5,
                dstpitch = 320;

            var src = index * srcpitch * srcpitch;
            var dst = y * dstpitch + x;
            for (var i = 0; i < srcpitch; i++)
            {
                for (var j = 0; j < srcpitch; j++)
                {
                    TextureData[dst++] = CurrentPalette[_controllerSprites[src++]];
                }
                dst += (dstpitch - srcpitch);
            }
        }

        #endregion
    }
}