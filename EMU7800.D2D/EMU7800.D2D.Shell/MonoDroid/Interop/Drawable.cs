using Android.Graphics;
using OpenTK.Graphics.ES11;
using System;

namespace EMU7800.D2D.Interop
{
    public abstract class Drawable : IDisposable
    {
        #region Fields

        readonly GraphicsDevice _gd;

        readonly int[] _textureId = new int[1];

        readonly float[] _uv =
        {
            0.0f, 1.0f,  // bottom-left
            1.0f, 1.0f,  // bottom-right
            0.0f, 0.0f,  // top-left
            1.0f, 0.0f,  // top-right
        };

        readonly float[] _vertices = new float[4 * 2];

        Bitmap _bitmap;
        Canvas _canvas;

        #endregion

        public float DrawableWidth { get; private set; }
        public float DrawableHeight { get; private set; }
        public uint Key { get; set; }

        protected bool RequestBitmapRefresh { get; set; }
        protected float BitmapWidth { get; private set; }
        protected float BitmapHeight { get; private set; }
        protected int BitmapMargin { get; private set; }

        public static uint ToKey(float width, float height)
        {
            return (uint)(Math.Abs(width) * Math.Abs(height) + 0.5f);
        }

        protected Canvas Canvas { get { return _canvas; } }

        protected void Draw(PointF location)
        {
            location.X -= BitmapMargin;
            location.Y -= BitmapMargin;

            var sw = 2.0f / _gd.Width;
            var sh = -2.0f / _gd.Height;

            _vertices[0] = location.X * sw - 1.0f;
            _vertices[1] = (location.Y + BitmapHeight) * sh + 1.0f;

            _vertices[2] = (location.X + BitmapWidth) * sw - 1.0f;
            _vertices[3] = _vertices[1];

            _vertices[4] = _vertices[0];
            _vertices[5] = location.Y * sh + 1.0f;

            _vertices[6] = _vertices[2];
            _vertices[7] = _vertices[5];

            GL.BindTexture(All.Texture2D, _textureId[0]);

            if (RequestBitmapRefresh)
            {
                _bitmap.EraseColor(0);
                RefreshBitmap();
                Android.Opengl.GLUtils.TexSubImage2D(Android.Opengl.GLES10.GlTexture2d, 0, 0, 0, _bitmap);
                RequestBitmapRefresh = false;
            }

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);
            GL.VertexPointer(2, All.Float, 0, _vertices);
            GL.TexCoordPointer(2, All.Float, 0, _uv);
            GL.DrawArrays(All.TriangleStrip, 0, 4);
        }

        protected virtual void RefreshBitmap()
        {
        }

        protected Color ToColor(D2DSolidColorBrush brush)
        {
            switch (brush)
            {
                case D2DSolidColorBrush.White:  return Color.White;
                case D2DSolidColorBrush.Black:  return Color.Black;
                case D2DSolidColorBrush.Red:    return Color.Red;
                case D2DSolidColorBrush.Orange: return Color.Orange;
                case D2DSolidColorBrush.Yellow: return Color.Yellow;
                case D2DSolidColorBrush.Green:  return Color.Green;
                case D2DSolidColorBrush.Blue:   return Color.Blue;
                case D2DSolidColorBrush.Gray:   return Color.Gray;
                default:                        return Color.White;
            }
        }

        protected static float ToWidth(RectF rect)
        {
            return Math.Abs(rect.Right - rect.Left);
        }

        protected static float ToHeight(RectF rect)
        {
            return Math.Abs(rect.Bottom - rect.Top);
        }

        #region IDisposable Members

        ~Drawable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_canvas)
                using (_bitmap)
                {
                }
                GL.DeleteTextures(1, _textureId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Constructors

        protected Drawable(GraphicsDevice gd, float width, float height, int margin)
        {
            _gd = gd;

            DrawableWidth = width;
            DrawableHeight = height;

            Key = ToKey(width, height);

            BitmapMargin = margin;
            BitmapWidth = width + 2 * margin;
            BitmapHeight = height + 2 * margin;

            _bitmap = Bitmap.CreateBitmap((int)BitmapWidth, (int)BitmapHeight, Bitmap.Config.Argb8888);
            _bitmap.EraseColor(0);
            _canvas = new Canvas(_bitmap);

            GL.GenTextures(1, _textureId);
            GL.BindTexture(All.Texture2D, _textureId[0]);
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Nearest);
            Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, _bitmap, 0);

            RequestBitmapRefresh = true;
        }

        #endregion
    }
}