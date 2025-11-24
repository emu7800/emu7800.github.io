using System;

namespace EMU7800.Shell;

public interface IStaticBitmapDriver
{
    int Create(ReadOnlySpan<byte> data);
    void Draw(RectF rect);
    void Release();
}

public sealed class EmptyStaticBitmapDriver : IStaticBitmapDriver
{
    public static readonly EmptyStaticBitmapDriver Default = new();
    EmptyStaticBitmapDriver() {}

    #region IStaticBitmapDriver Members

    public int Create(ReadOnlySpan<byte> data) => -1;
    public void Draw(RectF rect) {}
    public void Release() {}

    #endregion
}

public sealed class StaticBitmap : IDisposable
{
    public static readonly StaticBitmap Default = new();
    public static Func<IStaticBitmapDriver> DriverFactory { get; set; } = () => EmptyStaticBitmapDriver.Default;

    #region Fields

    readonly IStaticBitmapDriver _driver = EmptyStaticBitmapDriver.Default;
    bool _disposed;

    public int HR { get; init; } = -1;

    #endregion

    public void Draw(RectF rect)
      => _driver.Draw(rect);

    #region IDispose Members

    public void Dispose()
    {
        if (_disposed)
            return;
        _driver.Release();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~StaticBitmap()
      => _driver.Release();

    #endregion

    #region Constructors

    StaticBitmap() {}

    public StaticBitmap(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0)
            throw new ArgumentException("Data is empty.");

        _driver = DriverFactory();
        HR = _driver.Create(data);
    }

    #endregion
}
