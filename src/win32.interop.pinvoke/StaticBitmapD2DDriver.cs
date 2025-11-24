// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class StaticBitmapD2DDriver : IStaticBitmapDriver
{
    public static StaticBitmapD2DDriver Factory() => new();
    StaticBitmapD2DDriver() {}

    #region Fields

    IntPtr _bitmapPtr;

    #endregion

    #region IStateBitmapDriver Members

    public unsafe int Create(ReadOnlySpan<byte> data)
    {
        Release();
        fixed (byte* bytes = data)
        {
            return Direct2DNativeMethods.Direct2D_CreateStaticBitmap(bytes, data.Length, ref _bitmapPtr);
        }
    }

    public void Draw(RectF rect)
      => Direct2DNativeMethods.Direct2D_DrawStaticBitmap(_bitmapPtr, rect);

    public void Release()
    {
        if (_bitmapPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_ReleaseStaticBitmap(_bitmapPtr);
        _bitmapPtr = IntPtr.Zero;
    }

    #endregion
}
