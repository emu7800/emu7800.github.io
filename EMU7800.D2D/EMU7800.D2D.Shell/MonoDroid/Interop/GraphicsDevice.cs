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

        #endregion

        internal int Width { get; private set; }
        internal int Height { get; private set; }

        public bool IsDeviceResourcesRefreshed { get; set; }

        public void BeginDraw()
        {
            GL.Clear((uint)All.ColorBufferBit);
            GL.Viewport(0, 0, Width, Height);
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

        public void DrawLine(PointF p0, PointF p1, float strokeWidth, D2DSolidColorBrush brush) { }
        public void DrawRectangle(RectF rect, float strokeWidth, D2DSolidColorBrush brush) { }
        public void FillRectangle(RectF rect, D2DSolidColorBrush brush) { }
        public void FillEllipse(RectF rect, D2DSolidColorBrush brush) { }

        public void DrawText(TextLayout textLayout, PointF location, D2DSolidColorBrush brush)
        {
            textLayout.Draw(location, brush);
        }

        public void DrawBitmap(DynamicBitmap bitmap, RectF rect, D2DBitmapInterpolationMode interpolationMode)
        {
            bitmap.Draw(rect, interpolationMode);
        }

        public void DrawBitmap(StaticBitmap bitmap, RectF rect)
        {
            bitmap.Draw(rect);
        }

        public void SetAntiAliasMode(D2DAntiAliasMode antiAliasMode) { }
        public void PushAxisAlignedClip(RectF rect, D2DAntiAliasMode antiAliasMode) { }
        public void PopAxisAlignedClip() { }

        public void UpdateForWindowSizeChange()
        {
            Width = _view.Width;
            Height = _view.Height;

            GL.Enable(All.Texture2D);
            // TODO: figure out what this does: GL.ShadeModel(All.Smooth);
            GL.ClearColor(0, 0, 0, 0xff);
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
                16 /* bpp color */,
                16 /* depth */,
                 0,
                 0,
                 2 /* buffers */,
                 false
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
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}