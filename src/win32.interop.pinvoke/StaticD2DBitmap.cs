// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class StaticD2DBitmap : StaticBitmap
{
    #region Fields

    readonly IntPtr _bitmapPtr;

    #endregion

    public override void Draw(RectF rect)
      => Direct2DNativeMethods.Direct2D_DrawStaticBitmap(_bitmapPtr, rect);

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            if (_bitmapPtr != IntPtr.Zero && HR == 0)
                Direct2DNativeMethods.Direct2D_ReleaseStaticBitmap(_bitmapPtr);
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Constructors

    public StaticD2DBitmap(ReadOnlySpan<byte> data)
    {
        HR = Direct2DNativeMethods.Direct2D_CreateStaticBitmap(data, data.Length, ref _bitmapPtr);
    }

    #endregion
}