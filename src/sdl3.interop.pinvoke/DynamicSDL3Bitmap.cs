using EMU7800.Shell;
using System;

namespace EMU7800.SDL3.Interop;

public sealed class DynamicSDL3Bitmap : DynamicBitmap
{
    #region Fields
    #endregion

    public override void Draw(RectF rect, BitmapInterpolationMode interpolationMode)
    {
    }

    public override void Load(ReadOnlySpan<byte> data)
    {
    }

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            // TODO
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    ~DynamicSDL3Bitmap()
    {
        Dispose(false);
    }

    #endregion

    #region Constructors

    public DynamicSDL3Bitmap()
    {
    }

    #endregion
}
