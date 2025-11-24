using System;

namespace EMU7800.Shell;

public interface IGraphicsDeviceDriver
{
    int EC { get; }
    void BeginDraw();
    void DrawEllipse(RectF drect, float strokeWidth, SolidColorBrush brush);
    void DrawLine(PointF dp0, PointF dp1, float strokeWidth, SolidColorBrush brush);
    void DrawRectangle(RectF drect, float strokeWidth, SolidColorBrush brush);
    int EndDraw();
    void FillEllipse(RectF drect, SolidColorBrush brush);
    void FillRectangle(RectF drect, SolidColorBrush brush);
    void PopAxisAlignedClip();
    void PushAxisAlignedClip(RectF drect, AntiAliasMode antiAliasMode);
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
    public void DrawEllipse(RectF drect, float strokeWidth, SolidColorBrush brush) {}
    public void DrawLine(PointF dp0, PointF dp1, float strokeWidth, SolidColorBrush brush) {}
    public void DrawRectangle(RectF drect, float strokeWidth, SolidColorBrush brush) {}
    public int EndDraw() => -1;
    public void FillEllipse(RectF drect, SolidColorBrush brush) {}
    public void FillRectangle(RectF drect, SolidColorBrush brush) {}
    public void PopAxisAlignedClip() {}
    public void PushAxisAlignedClip(RectF drect, AntiAliasMode antiAliasMode) {}
    public void Resize(SizeU usize) {}
    public void SetAntiAliasMode(AntiAliasMode antiAliasMode) {}
    public void Shutdown() {}

    #endregion
}

public static class GraphicsDevice
{
    public static Func<IGraphicsDeviceDriver> DriverFactory { get; set; } = () => EmptyGraphicsDeviceDriver.Default;

    static IGraphicsDeviceDriver _driver = EmptyGraphicsDeviceDriver.Default;

    public static int EC => _driver.EC;

    public static void Initialize()
      => _driver = DriverFactory();

    public static void BeginDraw()
      => _driver.BeginDraw();

    public static void Draw(TextLayout textLayout, PointF location, SolidColorBrush brush)
      => textLayout.Draw(location, brush);

    public static void Draw(TextFormat textFormat, string text, RectF drect, SolidColorBrush brush)
      => textFormat.Draw(text, drect, brush);

    public static void Draw(StaticBitmap bitmap, RectF drect)
      => bitmap.Draw(drect);

    public static void Draw(DynamicBitmap bitmap, RectF drect, BitmapInterpolationMode interpolationMode)
      => bitmap.Draw(drect, interpolationMode);

    public static void DrawEllipse(RectF drect, float strokeWidth, SolidColorBrush brush)
      => _driver.DrawEllipse(drect, strokeWidth, brush);

    public static void DrawLine(PointF dp0, PointF dp1, float strokeWidth, SolidColorBrush brush)
      => _driver.DrawLine(dp0, dp1, strokeWidth, brush);

    public static void DrawRectangle(RectF drect, float strokeWidth, SolidColorBrush brush)
      => _driver.DrawRectangle(drect, strokeWidth, brush);

    public static int EndDraw()
      => _driver.EndDraw();

    public static void FillEllipse(RectF drect, SolidColorBrush brush)
      => _driver.FillEllipse(drect, brush);

    public static void FillRectangle(RectF drect, SolidColorBrush brush)
      => _driver.FillRectangle(drect, brush);

    public static void PopAxisAlignedClip()
      => _driver.PopAxisAlignedClip();

    public static void PushAxisAlignedClip(RectF drect, AntiAliasMode antiAliasMode)
      => _driver.PushAxisAlignedClip(drect, antiAliasMode);

    public static void Resize(SizeU usize)
      => _driver.Resize(usize);

    public static void SetAntiAliasMode(AntiAliasMode antiAliasMode)
      => _driver.SetAntiAliasMode(antiAliasMode);

    public static void Shutdown()
      => _driver.Shutdown();
}