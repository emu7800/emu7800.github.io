namespace EMU7800.Shell;

public class StaticBitmap : DisposableResource
{
    public static StaticBitmap Empty { get; } = new();

    public virtual void Draw(RectF rect) {}

    protected StaticBitmap() {}
}
