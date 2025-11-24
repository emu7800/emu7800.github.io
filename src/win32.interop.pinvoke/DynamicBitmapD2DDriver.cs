// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class DynamicBitmapD2DDriver : IDynamicBitmapDriver
{
    public static DynamicBitmapD2DDriver Factory() => new();
    DynamicBitmapD2DDriver() {}

    #region Fields

    IntPtr _bitmapPtr;

    #endregion

    #region IDynamicBitmapDriver Members

    public int Create(SizeU size)
    {
        Release();
        return Direct2DNativeMethods.Direct2D_CreateDynamicBitmap(size, ref _bitmapPtr);
    }

    public void Draw(RectF rect, BitmapInterpolationMode interpolationMode)
      => Direct2DNativeMethods.Direct2D_DrawDynamicBitmap(_bitmapPtr, rect, interpolationMode);

    public unsafe void Load(ReadOnlySpan<byte> data, int expectedPitch)
    {
        fixed (byte* bytes = data)
        {
            Direct2DNativeMethods.Direct2D_LoadDynamicBitmapFromMemory(_bitmapPtr, bytes, expectedPitch);
        }
    }

    public void Release()
    {
        if (_bitmapPtr == IntPtr.Zero)
            return;
        Direct2DNativeMethods.Direct2D_ReleaseDynamicBitmap(_bitmapPtr);
        _bitmapPtr = IntPtr.Zero;
    }

    #endregion
}
