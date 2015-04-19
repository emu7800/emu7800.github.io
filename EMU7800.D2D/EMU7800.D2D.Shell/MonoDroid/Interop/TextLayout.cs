using Android.App;
using Android.Content;
using Android.Graphics;
using OpenTK.Graphics.ES11;
using System;

namespace EMU7800.D2D.Interop
{
    public class TextLayout : IDisposable
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

        string _fontFamilyName;
        readonly float _fontSize;
        readonly string _text;

        Paint.Align _textAlignment;

        #endregion

        public int HR { get; private set; }

        public double Width { get; private set; }
        public double Height { get; private set; }

        public int SetTextAlignment(DWriteTextAlignment textAlignment)
        {
            switch (textAlignment)
            {
                case DWriteTextAlignment.Leading:
                    _textAlignment = Paint.Align.Left;
                    GenerateTexture();
                    break;
                case DWriteTextAlignment.Center:
                    _textAlignment = Paint.Align.Center;
                    GenerateTexture();
                    break;
                case DWriteTextAlignment.Trailing:
                    _textAlignment = Paint.Align.Right;
                    GenerateTexture();
                    break;
            }
            return 0;
        }

        public int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        {
            return 0;
        }

        #region IDisposable Members

        ~TextLayout()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteTextures(1, _textureId);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        internal void Draw(PointF location, D2DSolidColorBrush brush)
        {
            var sw =  2.0f / _gd.Width;
            var sh = -2.0f / _gd.Height;

            _vertices[0] = location.X                   * sw - 1.0f;
            _vertices[1] = (location.Y + (float)Height) * sh + 1.0f;

            _vertices[2] = (location.X + (float)Width)  * sw - 1.0f;
            _vertices[3] = _vertices[1];

            _vertices[4] = _vertices[0];
            _vertices[5] = location.Y                   * sh + 1.0f;

            _vertices[6] = _vertices[2];
            _vertices[7] = _vertices[5];

            GL.BindTexture(All.Texture2D, _textureId[0]);
            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);
            GL.VertexPointer(2, All.Float, 0, _vertices);
            GL.TexCoordPointer(2, All.Float, 0, _uv);
            GL.DrawArrays(All.TriangleStrip, 0, 4);
        }

        public TextLayout(GraphicsDevice gd, string fontFamilyName, float fontSize, string text, float width, float height)
        {
            _gd = gd;

            _fontFamilyName = fontFamilyName;
            _fontSize = fontSize;
            _text = text;
            _textAlignment = Paint.Align.Left;
            Width = width;
            Height = height;

            GenerateTexture();
        }

        void GenerateTexture()
        {
            GL.DeleteTextures(1, _textureId);

            GL.GenTextures(1, _textureId);
            GL.BindTexture(All.Texture2D, _textureId[0]);

            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Nearest);

            using (var bitmap = Bitmap.CreateBitmap((int)Width, (int)Height, Bitmap.Config.Argb8888))
            using (var canvas = new Canvas(bitmap))
            using (var textPaint = new Paint())
            {
                bitmap.EraseColor(0);
                Typeface tf = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/segoeui.ttf");
                textPaint.SetTypeface(tf);
                textPaint.TextSize = _fontSize;
                textPaint.AntiAlias = true;
                textPaint.TextAlign = _textAlignment;
                textPaint.SetARGB(0xff, 0xff, 0xff, 0xff);
                var bounds = new Rect();
                textPaint.GetTextBounds(_text, 0, _text.Length, bounds);
                canvas.DrawText(_text, bounds.Left, Math.Abs(bounds.Bottom - bounds.Top), textPaint);
                Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, bitmap, 0);
            }
        }
    }
}