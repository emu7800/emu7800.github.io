using Android.Graphics;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.Android;
using System;

namespace EMU7800.D2D.Interop
{
    public enum D2DSolidColorBrush { Black, Red, Orange, Yellow, Green, Blue, Gray, White };
    public enum D2DAntiAliasMode { PerPrimitive, Aliased };
    public enum D2DBitmapInterpolationMode { NearestNeighbor, Linear };

    public class GraphicsDevice : IDisposable
    {
        #region Fields

        AndroidGameView _view;
        DrawableCache<Rectangle> _rectangleCache = new DrawableCache<Rectangle>(5);
        DrawableCache<Rectangle> _filledRectangleCache = new DrawableCache<Rectangle>(5);
        DrawableCache<Ellipse> _filledEllipseCache = new DrawableCache<Ellipse>(5);
        DrawableCache<Line> _lineCache = new DrawableCache<Line>(5);

        #endregion

        internal int Width { get; private set; }
        internal int Height { get; private set; }

        public bool IsDeviceResourcesRefreshed { get; set; }

        public void BeginDraw()
        {
            GL.Enable(All.Texture2D);
            GL.Enable(All.Blend);
            GL.ShadeModel(All.Smooth);
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
            GL.Clear((uint)All.ColorBufferBit);
        }

        public int EndDraw()
        {
            return 0;
        }

        public DynamicBitmap CreateDynamicBitmap(SizeU size)
        {
            return new DynamicBitmap(this, size);
        }

        public StaticBitmap CreateStaticBitmap(byte[] data)
        {
            return new StaticBitmap(this, data);
        }

        public TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height)
        {
            return new TextLayout(this, fontFamilyName, fontSize, text, width, height);
        }

        public void DrawLine(PointF p0, PointF p1, float strokeWidth, D2DSolidColorBrush brush)
        {
            RectF rect;
            rect.Left   = p0.X;
            rect.Top    = p0.Y;
            rect.Right  = p1.X;
            rect.Bottom = p1.Y;
            var line = _lineCache.Get(rect);

            if (line == null)
            {
                line = new Line(this, rect, Paint.Style.Stroke);
                _lineCache.Put(line);
            }

            line.Draw(Shell.Struct.ToLocation(rect), strokeWidth, brush);
        }

        public void DrawRectangle(RectF rect, float strokeWidth, D2DSolidColorBrush brush)
        {
            var rectangle = _rectangleCache.Get(rect);

            if (rectangle == null)
            {
                rectangle = new Rectangle(this, rect, Paint.Style.Stroke);
                _rectangleCache.Put(rectangle);
            }

            rectangle.Draw(Shell.Struct.ToLocation(rect), strokeWidth, brush);
        }

        public void FillRectangle(RectF rect, D2DSolidColorBrush brush)
        {
            var rectangle = _filledRectangleCache.Get(rect);

            if (rectangle == null)
            {
                rectangle = new Rectangle(this, rect, Paint.Style.Fill);
                _filledRectangleCache.Put(rectangle);
            }

            rectangle.Draw(Shell.Struct.ToLocation(rect), brush);
        }

        public void FillEllipse(RectF rect, D2DSolidColorBrush brush)
        {
            var ellipse = _filledEllipseCache.Get(rect);

            if (ellipse == null)
            {
                ellipse = new Ellipse(this, rect, Paint.Style.Fill);
                _filledEllipseCache.Put(ellipse);
            }

            ellipse.Draw(Shell.Struct.ToLocation(rect), brush);
        }

        public void DrawText(TextLayout textLayout, PointF location, D2DSolidColorBrush brush)
        {
            if (textLayout == null)
                return;
            textLayout.Draw(location, brush);
        }

        public void DrawBitmap(DynamicBitmap bitmap, RectF rect, D2DBitmapInterpolationMode interpolationMode)
        {
            if (bitmap == null)
                return;
            bitmap.Draw(rect, interpolationMode);
        }

        public void DrawBitmap(StaticBitmap bitmap, RectF rect)
        {
            if (bitmap == null)
                return;
            bitmap.Draw(rect);
        }

        public void SetAntiAliasMode(D2DAntiAliasMode antiAliasMode) { }

        public void PushAxisAlignedClip(RectF rect, D2DAntiAliasMode antiAliasMode)
        {
            GL.Enable(All.ScissorTest);
            var x = (int)rect.Left;
            var y = (int)(Height - rect.Bottom);
            var w = (int)(rect.Right - rect.Left);
            var h = (int)(rect.Bottom - rect.Top);
            GL.Scissor(x, y, w, h);
        }

        public void PopAxisAlignedClip()
        {
            GL.Disable(All.ScissorTest);
        }

        public void UpdateForWindowSizeChange()
        {
            Width = _view.Width;
            Height = _view.Height;
            GL.Viewport(0, 0, Width, Height);
        }

        public void Present()
        {
            _view.SwapBuffers();
        }

        public void Initialize(AndroidGameView view)
        {
            if (view == null)
                throw new ArgumentNullException("view");

            _view = view;

            _view.GLContextVersion = GLContextVersion.Gles1_1;
            _view.GraphicsMode      = new AndroidGraphicsMode(
                16     /* bpp color */,
                16     /* depth */,
                 0     /* stencil */,
                 0     /* samples */,
                 2     /* buffers */,
                 false /* stereo */
                 );

            IsDeviceResourcesRefreshed = true;
        }

        #region IDisposable Members

        ~GraphicsDevice()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (_rectangleCache)
                using (_filledRectangleCache)
                using (_filledEllipseCache)
                using (_lineCache)
                { }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}