// © Mike Murphy

using EMU7800.Shell;
using System;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class DynamicSDL3Bitmap : DynamicBitmap
{
    #region Fields

    readonly int _expectedPitch;
    readonly IntPtr _hRenderer, _texture;

    #endregion

    public override void Draw(RectF rect, BitmapInterpolationMode interpolationMode)
    {
        SDL_FRect srect = rect;
        SDL_RenderTexture(_hRenderer, _texture, IntPtr.Zero, ref srect);
    }

    public unsafe override void Load(ReadOnlySpan<byte> data)
    {
        //SDL_LockTexture(_texture, IntPtr.Zero, out var pixels, out var _);
        //fixed (void *src = data)
        //{
        //    Buffer.MemoryCopy(src, (void*)pixels, data.Length, data.Length);
        //}
        //SDL_UnlockTexture(_texture);

        SDL_UpdateTexture(_texture, IntPtr.Zero, data, _expectedPitch);
    }

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            if (_texture != IntPtr.Zero && HR == 0)

                SDL_DestroyTexture(_texture);
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

    public unsafe DynamicSDL3Bitmap(IntPtr hRenderer, SizeU size)
    {
        _expectedPitch = (int)size.Width << 2;
        if (_expectedPitch <= 0)
            throw new ArgumentException("Size has zero width or height.");

        _hRenderer = hRenderer;
        _texture = (IntPtr)SDL_CreateTexture(
            hRenderer,
            SDL_PixelFormat.SDL_PIXELFORMAT_XRGB8888,
            SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
            (int)size.Width, (int)size.Height);

        HR = _texture != IntPtr.Zero ? 0 : -1;
    }

    #endregion
}
