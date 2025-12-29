// © Mike Murphy

using EMU7800.Core;
using EMU7800.Shell;
using System;
using System.Collections.Generic;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class GraphicsDeviceSDL3Driver : DisposableResource, IGraphicsDeviceDriver
{
    readonly IntPtr hWnd, hRenderer;
    readonly uint _displayId;
    readonly List<IDisposable> _disposables = [];
    readonly Stack<SDL_Rect> _prevClips = [];
    readonly Dictionary<float, IntPtr> _cachedFonts = [];
    readonly ILogger _logger;

    public SizeU WindowSize { get; private set; }
    public float ScaleFactor { get; private set; } = 1f;

    #region IGraphicsDeviceDriver Members

    public void BeginDraw()
    {
        SDL_SetRenderDrawColor(hRenderer, 0, 0, 0, 255 /* SDL_ALPHA_OPAQUE */);
        SDL_RenderClear(hRenderer);
        SDL_SetRenderScale(hRenderer, ScaleFactor, ScaleFactor);
    }

    public DynamicBitmap CreateDynamicBitmap(SizeU size)
    {
        var bitmap = new DynamicSDL3Bitmap(hRenderer, size);
        _disposables.Add(bitmap);
        return bitmap;
    }

    public StaticBitmap CreateStaticBitmap(ReadOnlySpan<byte> data)
    {
        var bitmap = new StaticSDL3Bitmap(hRenderer, data);
        _disposables.Add(bitmap);
        return bitmap;
    }

    public TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment, SolidColorBrush brush)
    {
        if (!_cachedFonts.TryGetValue(fontSize, out var hFont))
        {
            const string FontFileName = "OpenSans-VariableFont.ttf";
            hFont = TTF_OpenFont(FontFileName, fontSize);
            if (hFont == IntPtr.Zero)
            {
                _logger.Log(1, $"CreateTextLayout: Unable to locate font: {FontFileName}");
            }
            _cachedFonts.Add(fontSize, hFont);
        }

        var textLayout = new TextSDL3Layout(hRenderer, hFont, text, width, height, paragraphAlignment, textAlignment, brush);
        _disposables.Add(textLayout);
        return textLayout;
    }

    public void Draw(DynamicBitmap bitmap, RectF rect, BitmapInterpolationMode interpolationMode)
      => bitmap.Draw(rect, interpolationMode);

    public void Draw(StaticBitmap bitmap, RectF rect)
      => bitmap.Draw(rect);

    public void Draw(TextLayout textLayout, PointF location)
      => textLayout.Draw(location);

    public void DrawEllipse(RectF rect, float strokeWidth, SolidColorBrush brush)
    {
        ApplyBrush(brush);
        SDL_FRect r = rect;
        SDL_RenderRect(hRenderer, ref r);
    }

    public void DrawLine(PointF dp0, PointF dp1, float strokeWidth, SolidColorBrush brush)
    {
        ApplyBrush(brush);
        SDL_RenderLine(hRenderer, dp0.X, dp0.Y, dp1.X, dp1.Y);
    }

    public void DrawRectangle(RectF rect, float strokeWidth, SolidColorBrush brush)
    {
        ApplyBrush(brush);
        SDL_FRect r = rect;
        SDL_RenderRect(hRenderer, ref r);
        while (strokeWidth >= 1.0)
        {
            r.x -= 0.5f;
            r.y -= 0.5f;
            r.w += 1f;
            r.h += 1f;
            strokeWidth -= 1.0f;
            SDL_RenderRect(hRenderer, ref r);
        }
    }

    public int EndDraw()
      => SDL_RenderPresent(hRenderer) ? 0 : -1;

    public void FillEllipse(RectF rect, SolidColorBrush brush)
    {
        ApplyBrush(brush);
        SDL_FRect r = rect;
        SDL_RenderFillRect(hRenderer, ref r);
    }

    public void FillRectangle(RectF rect, SolidColorBrush brush)
    {
        ApplyBrush(brush);
        SDL_FRect r = rect;
        SDL_RenderFillRect(hRenderer, ref r);
    }

    public void PopAxisAlignedClip()
    {
        if (_prevClips.Count > 0)
        {
            _prevClips.Pop();
        }
        if (_prevClips.Count <= 0)
        {
            ClearRenderClipRect();
        }
        else
        {
            SetRenderClipRect(_prevClips.Peek());
        }
    }

    public void PushAxisAlignedClip(RectF rect, AntiAliasMode antiAliasMode)
    {
        SetRenderClipRect(rect);
        _prevClips.Push(rect);
    }

    public void Resize(SizeU usize)
    {
        WindowSize = usize;
        SDL_SetWindowSize(hWnd, (int)(usize.Width * ScaleFactor), (int)(usize.Height * ScaleFactor));
    }

    public void SetAntiAliasMode(AntiAliasMode antiAliasMode)
    {
    }

    public void Shutdown()
    {
        foreach (var disposable in _disposables)
        {
            disposable.Dispose();
        }
        _disposables.Clear();

        foreach (var kvp in _cachedFonts)
        {
            TTF_CloseFont(kvp.Value);
        }
        _cachedFonts.Clear();

        if (hRenderer != IntPtr.Zero)
            SDL_DestroyRenderer(hRenderer);
        if (hWnd != IntPtr.Zero)
            SDL_DestroyWindow(hWnd);

        TTF_Quit();
        SDL_Quit();
    }

    #endregion

    #region IDispose Members

    protected override void Dispose(bool disposing)
    {
        if (!_resourceDisposed)
        {
            Shutdown();
            _resourceDisposed = true;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Constructors

    public GraphicsDeviceSDL3Driver(ILogger logger, bool startMaximized)
    {
        _logger = logger;

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_TIMER | SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_GAMEPAD))
        {
            _logger.Log(1, $"Couldn't initialize SDL: {SDL_GetError()}");
            HR = -1;
            return;
        }

        if (!TTF_Init())
        {
            _logger.Log(1, $"Couldn't initialize SDL TTF: {SDL_GetError()}");
            HR = -1;
            return;
        }

        const int WINDOW_MIN_WIDTH = 800, WINDOW_MIN_HEIGHT = 480;

        var windowFlags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;
        if (startMaximized)
            windowFlags |= SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;

        if (!SDL_CreateWindowAndRenderer(VersionInfo.EMU7800, WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT, windowFlags, out nint hwnd, out nint hrenderer))
        {
            _logger.Log(1, $"Couldn't initialize SDL: CreateWindowAndRenderer: {SDL_GetError()}");
            HR = -1;
            return;
        }

        hWnd = hwnd;
        hRenderer = hrenderer;

        if (!SDL_SetRenderLogicalPresentation(hRenderer, WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT, SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED))
        {
            _logger.Log(1, $"Couldn't initialize SDL: SetRenderLogicalPresentation: {SDL_GetError()}");
            HR = -1;
            return;
        }

        ScaleFactor = SDL_GetWindowDisplayScale(hWnd);

        _logger.Log(3, $"SDL window display scale: {ScaleFactor}");

        _displayId = SDL_GetDisplayForWindow(hWnd);
        SDL_GetDisplayBounds(_displayId, out var desktopRect);

        var windowWidth  = (int)(WINDOW_MIN_WIDTH * ScaleFactor);
        var windowHeight = (int)(WINDOW_MIN_HEIGHT * ScaleFactor);
        var posX = (desktopRect.w >> 1) - (windowWidth >> 1);
        var posY = (desktopRect.h >> 1) - (windowHeight >> 1);

        SDL_SetWindowPosition(hWnd, posX, posY);

        Resize(new SizeU(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
    }

    #endregion

    void ApplyBrush(SolidColorBrush brush)
    {
        switch (brush)
        {
            case SolidColorBrush.Black:
                SDL_SetRenderDrawColor(hRenderer, 0, 0, 0, 255);
                break;
            case SolidColorBrush.Red:
                SDL_SetRenderDrawColor(hRenderer, 255, 0, 0, 255);
                break;
            case SolidColorBrush.Orange:
                SDL_SetRenderDrawColor(hRenderer, 255, 165, 0, 255);
                break;
            case SolidColorBrush.Yellow:
                SDL_SetRenderDrawColor(hRenderer, 255, 255, 0, 255);
                break;
            case SolidColorBrush.Green:
                SDL_SetRenderDrawColor(hRenderer, 0, 255, 0, 255);
                break;
            case SolidColorBrush.Blue:
                SDL_SetRenderDrawColor(hRenderer, 0, 0, 255, 255);
                break;
            case SolidColorBrush.Gray:
                SDL_SetRenderDrawColor(hRenderer, 128, 128, 128, 255);
                break;
            case SolidColorBrush.White:
                SDL_SetRenderDrawColor(hRenderer, 255, 255, 255, 255);
                break;
        }
    }

    unsafe void SetRenderClipRect(SDL_Rect srect)
      => SDL_SetRenderClipRect(hRenderer, &srect);

    unsafe void ClearRenderClipRect()
      => SDL_SetRenderClipRect(hRenderer, (SDL_Rect*)IntPtr.Zero);
}