using System;

namespace EMU7800.Shell;

public interface IGraphicsDeviceDriver
{
    int HR { get; }
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

public class DynamicBitmap : DisposableResource
{
    public readonly static DynamicBitmap Empty = new();

    public virtual void Draw(RectF rect, BitmapInterpolationMode interpolationMode) {}

    public virtual void Load(ReadOnlySpan<byte> data) {}

    protected DynamicBitmap() { }
}

public class StaticBitmap : DisposableResource
{
    public readonly static StaticBitmap Empty = new();

    public virtual void Draw(RectF rect) {}

    protected StaticBitmap() {}
}

public class TextLayout : DisposableResource
{
    public readonly static TextLayout Empty = new();

    public SizeF Size { get; protected set; }
    public float Width => Size.Width;
    public float Height => Size.Height;

    public virtual void Draw(PointF location) {}

    protected TextLayout() {}
}