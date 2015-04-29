using Android.App;
using Android.Graphics;
using OpenTK.Graphics.ES11;
using System;

namespace EMU7800.D2D.Interop
{
    public class TextLayout : IDisposable
    {
        #region Fields

        static Typeface _tf = Typeface.CreateFromAsset(Application.Context.Assets, "fonts/segoeui.ttf");

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

        readonly string _fontFamilyName;
        readonly float _fontSize;
        readonly string _text;

        readonly Bitmap _bitmap;
        readonly Canvas _canvas;
        readonly Paint _textPaint;
        readonly float _bitmapWidth, _bitmapHeight;

        DWriteTextAlignment _textAlignment;
        DWriteParaAlignment _paragraphAlignment;
        D2DSolidColorBrush _brush;
        bool _refreshBitmap;

        #endregion

        public int HR { get; private set; }

        public double Width { get; private set; }
        public double Height { get; private set; }

        public int SetTextAlignment(DWriteTextAlignment textAlignment)
        {
            if (textAlignment != _textAlignment)
            {
                _textAlignment = textAlignment;
                _refreshBitmap = true;
            }
            return 0;
        }

        public int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        {
            if (paragraphAlignment != _paragraphAlignment)
            {
                _paragraphAlignment = paragraphAlignment;
                _refreshBitmap = true;
            }
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
                using (_textPaint)
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
        }

        #endregion

        internal void Draw(PointF location, D2DSolidColorBrush brush)
        {
            if (brush != _brush)
            {
                _brush = brush;
                _refreshBitmap = true;
            }

            var sw =  2.0f / _gd.Width;
            var sh = -2.0f / _gd.Height;

            _vertices[0] = location.X                   * sw - 1.0f;
            _vertices[1] = (location.Y + _bitmapHeight) * sh + 1.0f;

            _vertices[2] = (location.X + _bitmapWidth)  * sw - 1.0f;
            _vertices[3] = _vertices[1];

            _vertices[4] = _vertices[0];
            _vertices[5] = location.Y                   * sh + 1.0f;

            _vertices[6] = _vertices[2];
            _vertices[7] = _vertices[5];

            GL.BindTexture(All.Texture2D, _textureId[0]);

            if (_refreshBitmap)
            {
                RefreshBitmap();
                Android.Opengl.GLUtils.TexSubImage2D(Android.Opengl.GLES10.GlTexture2d, 0, 0, 0, _bitmap);
                _refreshBitmap = false;
            }

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
            _textAlignment = DWriteTextAlignment.Leading;
            _paragraphAlignment = fontSize >= 50 ? DWriteParaAlignment.Center : DWriteParaAlignment.Near;
            _brush = D2DSolidColorBrush.White;

            _bitmapWidth = width;
            _bitmapHeight = height;

            _bitmap = Bitmap.CreateBitmap((int)_bitmapWidth, (int)_bitmapHeight, Bitmap.Config.Argb8888);
            _bitmap.EraseColor(0);

            GL.GenTextures(1, _textureId);
            GL.BindTexture(All.Texture2D, _textureId[0]);
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Nearest);
            Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES10.GlTexture2d, 0, _bitmap, 0);

            _canvas = new Canvas(_bitmap);
            _textPaint = new Paint();
            _textPaint.SetTypeface(_tf);
            _textPaint.TextSize = _fontSize;
            _textPaint.AntiAlias = true;

            _refreshBitmap = true;
        }

        #region Helpers

        void RefreshBitmap()
        {
            switch (_brush)
            {
                case D2DSolidColorBrush.White:
                    _textPaint.Color = Color.White;
                    break;
                case D2DSolidColorBrush.Black:
                    _textPaint.Color = Color.Black;
                    break;
                case D2DSolidColorBrush.Red:
                    _textPaint.Color = Color.Red;
                    break;
                case D2DSolidColorBrush.Orange:
                    _textPaint.Color = Color.Orange;
                    break;
                case D2DSolidColorBrush.Yellow:
                    _textPaint.Color = Color.Yellow;
                    break;
                case D2DSolidColorBrush.Green:
                    _textPaint.Color = Color.Green;
                    break;
                case D2DSolidColorBrush.Blue:
                    _textPaint.Color = Color.Blue;
                    break;
                case D2DSolidColorBrush.Gray:
                    _textPaint.Color = Color.Gray;
                    break;
            }

            _textPaint.TextAlign = ToPaintAlign(_textAlignment);

            float tx = 0f, ty = 0f;

            if (_textPaint.TextAlign == Paint.Align.Right)
                tx = _bitmapWidth;
            else if (_textPaint.TextAlign == Paint.Align.Center)
                tx = _bitmapWidth / 2.0f;

            var bounds = new Rect();
            _textPaint.GetTextBounds(_text, 0, _text.Length, bounds);

            Width  = Math.Abs(bounds.Left - bounds.Right);
            Height = Math.Abs(bounds.Top - bounds.Bottom) + 2.0f;

            switch (_paragraphAlignment)
            {
                case DWriteParaAlignment.Near:      // top of the text flow is aligned to the top edge of the layout box
                    ty = (float)Height;
                    break;
                case DWriteParaAlignment.Center:    // center of the flow is aligned to the center of the layout box
                    ty = _bitmapHeight / 2.0f + (float)Height / 2.0f;
                    break;
                case DWriteParaAlignment.Far:       // bottom of the flow is aligned to the bottom edge of the layout box
                    ty = _bitmapHeight;
                    break;
            }

            _canvas.DrawText(_text, tx, ty, _textPaint);
        }

        static Paint.Align ToPaintAlign(DWriteTextAlignment textAlignment)
        {
            switch (textAlignment)
            {
                case DWriteTextAlignment.Center:
                    return Paint.Align.Center;
                case DWriteTextAlignment.Trailing:
                    return Paint.Align.Right;
                default:
                case DWriteTextAlignment.Leading:
                    return Paint.Align.Left;
            }
        }

        #endregion
    }
}