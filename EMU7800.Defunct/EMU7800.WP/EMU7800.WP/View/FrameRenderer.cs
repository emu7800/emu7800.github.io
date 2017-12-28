using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using EMU7800.Core;

namespace EMU7800.WP.View
{
    public abstract class FrameRenderer
    {
        #region Fields

        readonly static ushort[] _palette = new ushort[256];
        readonly static ushort[] _hudPalette = new ushort[256];

        readonly GraphicsDevice _graphicsDevice;

        protected static readonly Random SnowGenerator = new Random();
        protected ushort[] CurrentPalette;
        protected readonly ushort[] TextureData;

        #endregion

        public Texture2D Texture { get; private set; }

        public Rectangle TargetRect { get; private set; }

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

        protected void SetTextureData()
        {
            _graphicsDevice.Textures[0] = null;
            Texture.SetData(TextureData);
        }

        #region Constructors

        protected FrameRenderer(int[] sourcePalette, int width, int height)
        {
            if (sourcePalette == null)
                throw new ArgumentNullException("sourcePalette");
            if (sourcePalette.Length != 256)
                throw new ArgumentException("sourcePalette");

            _graphicsDevice = SharedGraphicsDeviceManager.Current.GraphicsDevice;

            Texture = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Bgr565);
            TextureData = new ushort[width * height];
            TargetRect = new Rectangle(160, 0, 640, 2 * 230);

            for (var i = 0; i < _palette.Length; i++)
            {
                var color = sourcePalette[i];
                var r = (color >> 16) & 0xFF;
                var g = (color >> 8)  & 0xFF;
                var b =  color        & 0xFF;
                _palette[i]    = (ushort)((((r >> 3) & 0x1f) << 11) | (((g >> 2) & 0x3f) << 5) | ((b >> 3) & 0x1f));
                _hudPalette[i] = (ushort)((((r >> 4) & 0x1f) << 11) | (((g >> 3) & 0x3f) << 5) | ((b >> 4) & 0x1f));
            }
            CurrentPalette = _palette;
        }

        #endregion
    }
}