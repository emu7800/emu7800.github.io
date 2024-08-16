using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop;

public class DynamicBitmap : IDisposable
{
    public static readonly DynamicBitmap Default = new();

    #region Fields

    readonly D2D_SIZE_U _bsize;
    readonly int _expectedDataLength, _expectedPitch;

    public IntPtr BitmapPtr { get; private set; }

    public int HR { get; private set; }

    #endregion

    internal void Draw(D2D_RECT_F drect, D2DBitmapInterpolationMode interpolationMode)
        => Direct2D_DrawDynamicBitmap(BitmapPtr, drect, interpolationMode);

    public void Load(ReadOnlySpan<byte> data)
    {
        if (data.Length < _expectedDataLength || _expectedDataLength == 0)
            return;

        unsafe
        {
            fixed (byte* bytes = data)
            {
                Direct2D_LoadDynamicBitmapFromMemory(BitmapPtr, bytes, _expectedPitch);
            }
        }
    }

    public void Initialize()
    {
        if (BitmapPtr != IntPtr.Zero || _expectedDataLength == 0)
            return;
        var ptr = BitmapPtr;
        HR = Direct2D_CreateDynamicBitmap(_bsize, ref ptr);
        BitmapPtr = ptr;
    }

    #region IDispose Members

    public void Dispose()
    {
        if (BitmapPtr == IntPtr.Zero)
            return;
        Direct2D_ReleaseDynamicBitmap(BitmapPtr);
        BitmapPtr = IntPtr.Zero;
        HR = 0;
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Constructors

    DynamicBitmap() {}

    public DynamicBitmap(D2D_SIZE_U bsize)
    {
        _bsize = bsize;
        _expectedDataLength = (int)(bsize.Width * bsize.Height) << 2;
        _expectedPitch = (int)bsize.Width << 2;
        Initialize();
    }

    #endregion
}