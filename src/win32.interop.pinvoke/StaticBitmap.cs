using System;

namespace EMU7800.Win32.Interop;

using static Direct2DNativeMethods;

public class StaticBitmap(byte[] data) : IDisposable
{
    #region Fields

    public static readonly StaticBitmap Default = new([]);

    readonly IntPtr BitmapPtr = InitializeStaticBitmap(data);

    bool _disposed;

    #endregion

    internal void Draw(D2D_RECT_F drect)
        => Direct2D_DrawStaticBitmap(BitmapPtr, drect);

    #region IDispose Members

    public void Dispose()
    {
        if (_disposed)
            return;
        ReleaseStaticBitmap(BitmapPtr);
        GC.SuppressFinalize(this);
        _disposed = true;
    }

    ~StaticBitmap()
        => ReleaseStaticBitmap(BitmapPtr);

    #endregion

    #region Helpers

    static IntPtr InitializeStaticBitmap(byte[] data)
    {
        if (data.Length == 0)
            return IntPtr.Zero;

        var ptr = IntPtr.Zero;
        unsafe
        {
            fixed (byte* bytes = data)
            {
                _ = Direct2D_CreateStaticBitmap(bytes, data.Length, ref ptr);
            }
        }
        return ptr;
    }

    static void ReleaseStaticBitmap(IntPtr bitmapPtr)
    {
        if (bitmapPtr == IntPtr.Zero)
            return;
        Direct2D_ReleaseStaticBitmap(bitmapPtr);
    }

    #endregion
}