namespace EMU7800.Shell;

public struct PointF
{
    public float X;
    public float Y;

    public PointF(float x, float y)
      => (X, Y) = (x, y);
    public PointF(RectF rect)
      => (X, Y) = (rect.Left, rect.Top);
    public PointF(PointF point)
      => (X, Y) = (point.X, point.Y);
}

public struct SizeF
{
    public float Width;
    public float Height;

    public SizeF(float w, float h)
      => (Width, Height) = (w, h);
    public SizeF(RectF rect)
      => (Width, Height) = (rect.Right - rect.Left, rect.Bottom - rect.Top);
}

public partial struct SizeU
{
    public uint Width;
    public uint Height;

    public SizeU(int w, int h)
      => (Width, Height) = ((uint)w, (uint)h);
}

public struct RectF
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public readonly PointF ToLocation()
        => new(this);
    public readonly SizeF ToSize()
        => new(this);

    public RectF(float left, float top, float right, float bottom)
        => (Left, Top, Right, Bottom) = (left, top, right, bottom);
    public RectF(PointF point, SizeF size)
        => (Left, Top, Right, Bottom) = (point.X, point.Y, point.X + size.Width, point.Y + size.Height);
}
