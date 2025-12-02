// © Mike Murphy

using EMU7800.Shell;
using System;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class StaticSDL3Bitmap : StaticBitmap
{
    #region Fields

    readonly IntPtr _hRenderer, _surface, _texture;

    #endregion

    public override void Draw(RectF rect)
    {
        SDL_FRect drect = rect;
        SDL_RenderTexture(_hRenderer, _texture, ref drect, ref drect);
    }

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            if (_surface != IntPtr.Zero && HR == 0)
                SDL_DestroySurface(_surface);
            if (_texture != IntPtr.Zero && HR == 0)
                SDL_DestroyTexture(_texture);
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    ~StaticSDL3Bitmap()
    {
        Dispose(false);
    }

    #endregion

    #region Constructors

    public unsafe StaticSDL3Bitmap(IntPtr hRenderer, ReadOnlySpan<byte> data)
    {
        _hRenderer = hRenderer;
        _surface = (IntPtr)SDL_LoadPNG_IO(SDL_IOFromConstMem(data, data.Length), true);
        _texture = (IntPtr)SDL_CreateTextureFromSurface(_hRenderer, _surface);
    }

    #endregion
}