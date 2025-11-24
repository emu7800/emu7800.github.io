using System;

namespace EMU7800.Shell;

public interface IDynamicBitmapDriver
{
    int Create(SizeU size);
    void Draw(RectF rect, BitmapInterpolationMode interpolationMode);
    void Load(ReadOnlySpan<byte> data, int expectedPitch);
    void Release();
}

public sealed class EmptyDynamicBitmapDriver : IDynamicBitmapDriver
{
    public static readonly EmptyDynamicBitmapDriver Default = new();
    EmptyDynamicBitmapDriver() {}

    #region IDynamicBitmapDriver Members

    public int Create(SizeU size) => -1;
    public void Draw(RectF rect, BitmapInterpolationMode interpolationMode) {}
    public void Load(ReadOnlySpan<byte> data, int expectedPitch) {}
    public void Release() {}

    #endregion
}

public sealed class DynamicBitmap : IDisposable
{
    public static readonly DynamicBitmap Default = new();
    public static Func<IDynamicBitmapDriver> DriverFactory { get; set; } = () => EmptyDynamicBitmapDriver.Default;

    #region Fields

    readonly IDynamicBitmapDriver _driver = EmptyDynamicBitmapDriver.Default;
    readonly int _expectedPitch;
    bool _disposed;

    public int HR { get; init; } = -1;

    #endregion

    public void Draw(RectF rect, BitmapInterpolationMode interpolationMode)
      => _driver.Draw(rect, interpolationMode);

    public void Load(ReadOnlySpan<byte> data)
      => _driver.Load( data, _expectedPitch);

    #region IDispose Members

    public void Dispose()
    {
        if (_disposed)
            return;
        _driver.Release();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~DynamicBitmap()
      => _driver.Release();

    #endregion

    #region Constructors

    DynamicBitmap() {}

    public DynamicBitmap(SizeU size)
    {
        _expectedPitch = (int)size.Width << 2;

        if (_expectedPitch == 0)
            throw new ArgumentException("Size has zero width or height.");

        _driver = DriverFactory();
        HR = _driver.Create(size);
    }

    #endregion
}
