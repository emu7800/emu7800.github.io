using System;

namespace EMU7800.Shell;

public class DynamicBitmap : DisposableResource
{
    public static DynamicBitmap Empty { get; } = new();

    public virtual void Draw(RectF rect, BitmapInterpolationMode interpolationMode) {}

    public virtual void Load(ReadOnlySpan<byte> data) {}

    #region Constructors

    protected DynamicBitmap() {}

    #endregion
}
