// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class DynamicD2DBitmap : DynamicBitmap
{
    #region Fields

    readonly int _expectedPitch;
    readonly IntPtr _bitmapPtr;

    #endregion

    public override void Draw(RectF rect, BitmapInterpolationMode interpolationMode)
      => Direct2DNativeMethods.Direct2D_DrawDynamicBitmap(_bitmapPtr, rect, interpolationMode);

    public override unsafe void Load(ReadOnlySpan<byte> data)
      => Direct2DNativeMethods.Direct2D_LoadDynamicBitmapFromMemory(_bitmapPtr, data, _expectedPitch);

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            if (_bitmapPtr != IntPtr.Zero && HR == 0)
                Direct2DNativeMethods.Direct2D_ReleaseDynamicBitmap(_bitmapPtr);
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    ~DynamicD2DBitmap()
    {
        Dispose(false);
    }

    #endregion

    #region Constructors

    public DynamicD2DBitmap(SizeU size)
    {
        _expectedPitch = (int)size.Width << 2;
        if (_expectedPitch <= 0)
            throw new ArgumentException("Size has zero width or height.");
        HR = Direct2DNativeMethods.Direct2D_CreateDynamicBitmap(size, ref _bitmapPtr);
    }

    #endregion
}
