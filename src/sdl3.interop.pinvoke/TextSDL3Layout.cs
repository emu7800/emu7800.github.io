// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.SDL3.Interop;

using static EMU7800.SDL3.Interop.SDL3;

public sealed class TextSDL3Layout : TextLayout
{
    readonly IntPtr _hRenderer, _texture;
    readonly SDL_FRect _dstrect;

    public override void Draw(PointF location)
    {
        if (_hRenderer == IntPtr.Zero)
            return;
        SDL_FRect dstrect = _dstrect;
        dstrect.x += location.X;
        dstrect.y += location.Y;
        SDL_RenderTexture(_hRenderer, _texture, IntPtr.Zero, ref dstrect);
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

    #endregion

    #region Constructors

    public unsafe TextSDL3Layout(IntPtr hRenderer, IntPtr hFont, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush)
    {
        if (string.IsNullOrEmpty(text) || hRenderer == IntPtr.Zero || hFont == IntPtr.Zero)
            return;

        _hRenderer = hRenderer;

        var surface = (IntPtr)TTF_RenderText_Blended_Wrapped(hFont, text, 0, ToSDLColor(brush), (int)width);
        var texture = SDL_CreateTextureFromSurface(_hRenderer, surface);

        _texture = (IntPtr)texture;

        HR = _texture != IntPtr.Zero ? 0 : -1;

        if (HR == 0)
        {
            SDL_SetTextureScaleMode(_texture, SDL_ScaleMode.SDL_SCALEMODE_NEAREST);
        }

        SDL_DestroySurface(surface);

        _dstrect.x = textAlignment switch
        {
            WriteTextAlignment.Leading  => 0,
            WriteTextAlignment.Center   => (width - texture->w) / 2,
            WriteTextAlignment.Trailing => width - texture->w,
            _ => 0
         };
        _dstrect.y = paragraphAlignment switch
        {
            WriteParaAlignment.Near     => 0,
            WriteParaAlignment.Center   => (height - texture->h) / 2,
            WriteParaAlignment.Far      => height - texture->h,
            _ => 0
        };
        _dstrect.w = texture->w;
        _dstrect.h = texture->h;

        Size = new SizeF(_dstrect.w, _dstrect.h);
    }

    #endregion

    static SDL_Color ToSDLColor(SolidColorBrush brush)
      => brush switch
      {
            SolidColorBrush.Black  => new() { r = 0,   g = 0,   b = 0,   a = 255 },
            SolidColorBrush.Red    => new() { r = 255, g = 0,   b = 0,   a = 255 },
            SolidColorBrush.Orange => new() { r = 255, g = 165, b = 0,   a = 255 },
            SolidColorBrush.Yellow => new() { r = 255, g = 255, b = 0,   a = 255 },
            SolidColorBrush.Green  => new() { r = 0,   g = 255, b = 0,   a = 255 },
            SolidColorBrush.Blue   => new() { r = 0,   g = 0,   b = 255, a = 255 },
            SolidColorBrush.Gray   => new() { r = 128, g = 128, b = 128, a = 255 },
            SolidColorBrush.White  => new() { r = 255, g = 255, b = 255, a = 255 },
            _                      => new() { r = 255, g = 255, b = 255, a = 255 },
      };
}