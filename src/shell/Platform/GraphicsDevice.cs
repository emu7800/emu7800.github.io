using System;

namespace EMU7800.Shell;

public interface IGraphicsDeviceDriver
{
    int EC { get; }
    void BeginDraw();
    DynamicBitmap CreateDynamicBitmap(SizeU size);
    StaticBitmap CreateStaticBitmap(ReadOnlySpan<byte> data);
    TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush);
    void Draw(DynamicBitmap bitmap, RectF rect, BitmapInterpolationMode interpolationMode);
    void Draw(StaticBitmap bitmap, RectF rect);
    void Draw(TextLayout textLayout, PointF location);
    void DrawEllipse(RectF rect, float strokeWidth, SolidColorBrush brush);
    void DrawLine(PointF p0, PointF p1, float strokeWidth, SolidColorBrush brush);
    void DrawRectangle(RectF rect, float strokeWidth, SolidColorBrush brush);
    int EndDraw();
    void FillEllipse(RectF rect, SolidColorBrush brush);
    void FillRectangle(RectF rect, SolidColorBrush brush);
    void PopAxisAlignedClip();
    void PushAxisAlignedClip(RectF rect, AntiAliasMode antiAliasMode);
    void Resize(SizeU usize);
    void SetAntiAliasMode(AntiAliasMode antiAliasMode);
    void Shutdown();
}

public sealed class EmptyGraphicsDeviceDriver : IGraphicsDeviceDriver
{
    public static readonly EmptyGraphicsDeviceDriver Default = new();
    EmptyGraphicsDeviceDriver() {}

    #region IGraphicsDeviceDriver Members

    public int EC { get; }
    public void BeginDraw() {}
    public DynamicBitmap CreateDynamicBitmap(SizeU size) => DynamicBitmap.Empty;
    public StaticBitmap CreateStaticBitmap(ReadOnlySpan<byte> data) => StaticBitmap.Empty;
    public TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush) => TextLayout.Empty;
    public void Draw(DynamicBitmap bitmap, RectF rect, BitmapInterpolationMode interpolationMode) {}
    public void Draw(StaticBitmap bitmap, RectF rect) {}
    public void Draw(TextLayout textLayout, PointF location) {}
    public void DrawEllipse(RectF rect, float strokeWidth, SolidColorBrush brush) {}
    public void DrawLine(PointF p0, PointF p1, float strokeWidth, SolidColorBrush brush) {}
    public void DrawRectangle(RectF rect, float strokeWidth, SolidColorBrush brush) {}
    public int EndDraw() => -1;
    public void FillEllipse(RectF rect, SolidColorBrush brush) {}
    public void FillRectangle(RectF rect, SolidColorBrush brush) {}
    public void PopAxisAlignedClip() {}
    public void PushAxisAlignedClip(RectF rect, AntiAliasMode antiAliasMode) {}
    public void Resize(SizeU usize) {}
    public void SetAntiAliasMode(AntiAliasMode antiAliasMode) {}
    public void Shutdown() {}

    #endregion
}

public static class GraphicsDevice
{
    static IGraphicsDeviceDriver _driver = EmptyGraphicsDeviceDriver.Default;

    public static int EC => _driver.EC;

    public static void Initialize(IGraphicsDeviceDriver driver)
      => _driver = driver;

    public static void BeginDraw()
      => _driver.BeginDraw();

    public static DynamicBitmap CreateDynamicBitmap(SizeU size)
      => _driver.CreateDynamicBitmap(size);

    public static StaticBitmap CreateStaticBitmap(ReadOnlySpan<byte> data)
      => _driver.CreateStaticBitmap(data);

    public static TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush)
      => _driver.CreateTextLayout(fontFamilyName, fontSize, text, width, height, paragraphAlignment, textAlignment, brush);

    public static void Draw(DynamicBitmap bitmap, RectF rect, BitmapInterpolationMode interpolationMode)
      => _driver.Draw(bitmap, rect, interpolationMode);

    public static void Draw(StaticBitmap bitmap, RectF rect)
      => _driver.Draw(bitmap, rect);

    public static void Draw(TextLayout textLayout, PointF location)
      => _driver.Draw(textLayout, location);

    public static void DrawEllipse(RectF rect, float strokeWidth, SolidColorBrush brush)
      => _driver.DrawEllipse(rect, strokeWidth, brush);

    public static void DrawLine(PointF p0, PointF p1, float strokeWidth, SolidColorBrush brush)
      => _driver.DrawLine(p0, p1, strokeWidth, brush);

    public static void DrawRectangle(RectF rect, float strokeWidth, SolidColorBrush brush)
      => _driver.DrawRectangle(rect, strokeWidth, brush);

    public static int EndDraw()
      => _driver.EndDraw();

    public static void FillEllipse(RectF rect, SolidColorBrush brush)
      => _driver.FillEllipse(rect, brush);

    public static void FillRectangle(RectF rect, SolidColorBrush brush)
      => _driver.FillRectangle(rect, brush);

    public static void PopAxisAlignedClip()
      => _driver.PopAxisAlignedClip();

    public static void PushAxisAlignedClip(RectF rect, AntiAliasMode antiAliasMode)
      => _driver.PushAxisAlignedClip(rect, antiAliasMode);

    public static void Resize(SizeU usize)
      => _driver.Resize(usize);

    public static void SetAntiAliasMode(AntiAliasMode antiAliasMode)
      => _driver.SetAntiAliasMode(antiAliasMode);

    public static void Shutdown()
      => _driver.Shutdown();
}