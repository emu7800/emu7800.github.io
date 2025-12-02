// © Mike Murphy

using EMU7800.Shell;
using System;
using System.Collections.Generic;

using static EMU7800.SDL3.Interop.SDL3;

namespace EMU7800.SDL3.Interop;

public sealed class GraphicsDeviceSDL3Driver : IGraphicsDeviceDriver
{
    public static GraphicsDeviceSDL3Driver Factory() => new();

    static IntPtr hWnd, hRenderer;

    readonly static List<IDisposable> Disposables = [];

    #region IGraphicsDeviceDriver Members

    public int EC { get; private set; }

    public void BeginDraw()
    {
        SDL_SetRenderDrawColor(hRenderer, 0, 0, 0, 255); //SDL_ALPHA_OPAQUE
        SDL_RenderClear(hRenderer);
    }

    public DynamicBitmap CreateDynamicBitmap(SizeU size)
    {
        var bitmap = new DynamicSDL3Bitmap(/*hRenderer, size*/);
        Disposables.Add(bitmap);
        return bitmap;
    }

    public StaticBitmap CreateStaticBitmap(ReadOnlySpan<byte> data)
    {
        var bitmap = new StaticSDL3Bitmap(hRenderer, data);
        Disposables.Add(bitmap);
        return bitmap;
    }

    public TextLayout CreateTextLayout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment)
    {
        var textLayout = new TextSDL3Layout(fontFamilyName, fontSize, text, width, height, paragraphAlignment, textAlignment);
        Disposables.Add(textLayout);
        return textLayout;
    }

    public void Draw(DynamicBitmap bitmap, RectF rect, BitmapInterpolationMode interpolationMode)
      => bitmap.Draw(rect, interpolationMode);

    public void Draw(StaticBitmap bitmap, RectF rect)
      => bitmap.Draw(rect);

    public void Draw(TextLayout textLayout, PointF location, SolidColorBrush brush)
      => textLayout.Draw(location, brush);

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

    readonly Stack<SDL_Rect> _prevClips = new();

    public void PopAxisAlignedClip()
    {
        if (_prevClips.Count == 0)
            return;
        var r = _prevClips.Pop();
        SDL_SetRenderClipRect(hRenderer, ref r);
    }

    public void PushAxisAlignedClip(RectF rect, AntiAliasMode antiAliasMode)
    {
        SDL_GetRenderClipRect(hRenderer, out SDL_Rect r);
        _prevClips.Push(r);

        r = rect;
        SDL_SetRenderClipRect(hRenderer, ref r);
        //SDL_RenderClipEnabled(hRenderer);
    }

    public void Resize(SizeU usize)
      => SDL_SetWindowSize(hWnd, (int)usize.Width, (int)usize.Height);

    public void SetAntiAliasMode(AntiAliasMode antiAliasMode)
    {
    }

    public void Shutdown()
    {
        foreach (var disposable in Disposables)
        {
            disposable.Dispose();
        }
        Disposables.Clear();

        if (hRenderer != IntPtr.Zero)
            SDL_DestroyRenderer(hRenderer);
        if (hWnd != IntPtr.Zero)
            SDL_DestroyWindow(hWnd);
        hWnd = hRenderer = IntPtr.Zero;
    }

    #endregion

    #region Constructors

    public GraphicsDeviceSDL3Driver()
    {
        const int SdlWindowWidth = 800, SdlWindowHeight = 640;

        if (!SDL_CreateWindowAndRenderer(VersionInfo.EMU7800, SdlWindowWidth, SdlWindowHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE, out hWnd, out hRenderer))
        {
            SDL_Log($"Couldn't initialize SDL: CreateWindowAndRenderer: {SDL_GetError()}");
            EC = -1;
        }
        else if (!SDL_SetRenderLogicalPresentation(hRenderer, SdlWindowWidth, SdlWindowHeight, SDL_RendererLogicalPresentation.SDL_LOGICAL_PRESENTATION_DISABLED))
        {
            SDL_Log($"Couldn't initialize SDL: SetRenderLogicalPresentation: {SDL_GetError()}");
            EC = -1;
        }
        else
        {
            Window.OnResized(SdlWindowWidth, SdlWindowHeight);
        }
    }

    #endregion

    static void ApplyBrush(SolidColorBrush brush)
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
}