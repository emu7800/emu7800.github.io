using System;

namespace EMU7800.D2D.Interop
{
    public enum D2DSolidColorBrush { Black, Red, Orange, Yellow, Green, Blue, Gray, White };
    public enum D2DAntiAliasMode { PerPrimitive, Aliased };
    public enum D2DBitmapInterpolationMode { NearestNeighbor, Linear };

    public class GraphicsDevice : IDisposable
    {
        public bool IsDeviceResourcesRefreshed { get; set; }

        public void BeginDraw()
        {
        }

        public int EndDraw()
        {
            return 0;
        }

        public DynamicBitmap CreateDynamicBitmap(SizeU size)
        {
            return null;
        }

        public StaticBitmap CreateStaticBitmap(byte[] data)
        {
            return null;
        }

        public TextFormat CreateTextFormat(string fontFamilyName, float fontSize)
        {
            return null;
        }

        public TextFormat CreateTextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize)
        {
            return null;
        }

        public TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height)
        {
            return null;
        }

        public TextLayout CreateTextLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height)
        {
            return null;
        }

        public void DrawLine(PointF p0, PointF p1, float strokeWidth, D2DSolidColorBrush brush) { }
        public void DrawRectangle(RectF rect, float strokeWidth, D2DSolidColorBrush brush) { }
        public void FillRectangle(RectF rect, D2DSolidColorBrush brush) { }
        public void DrawEllipse(RectF rect, float strokeWidth, D2DSolidColorBrush brush) { }
        public void FillEllipse(RectF rect, D2DSolidColorBrush brush) { }
        public void DrawText(TextFormat textFormat, string text, RectF rect, D2DSolidColorBrush brush) { }
        public void DrawText(TextLayout textLayout, PointF location, D2DSolidColorBrush brush) { }
        public void DrawBitmap(DynamicBitmap bitmap, RectF rect, D2DBitmapInterpolationMode interpolationMode) { }
        public void DrawBitmap(StaticBitmap bitmap, RectF rect) { }
        public void SetAntiAliasMode(D2DAntiAliasMode antiAliasMode) { }
        public void PushAxisAlignedClip(RectF rect, D2DAntiAliasMode antiAliasMode) { }
        public void PopAxisAlignedClip() { }
        public void Initialize(float dpi, int dxgiModeRotation) { }
        public void HandleDeviceLost() { }
        public void CreateDeviceIndependentResources() { }
        public void CreateDeviceResources() { }
        public void SetDpi(float dpi) { }
        public void UpdateForWindowSizeChange() { }
        public void CreateWindowSizeDependentResources() { }
        public void Present() { }
        public void ValidateDevice() { }
        public void Trim() { }

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