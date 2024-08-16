using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop;

public static class GraphicsDevice
{
    public static int Initialize(IntPtr hWnd)
        => Direct2D_Initialize(hWnd);

    public static void BeginDraw()
        => Direct2D_BeginDraw();

    public static int EndDraw()
        => Direct2D_EndDraw();

    public static void Draw(TextLayout textLayout, D2D_POINT_2F location, D2DSolidColorBrush brush)
        => textLayout.Draw(location, brush);

    public static void Draw(TextFormat textFormat, string text, D2D_RECT_F drect, D2DSolidColorBrush brush)
        => textFormat.Draw(text, drect, brush);

    public static void Draw(StaticBitmap bitmap, D2D_RECT_F drect)
        => bitmap.Draw(drect);

    public static void Draw(DynamicBitmap bitmap, D2D_RECT_F drect, D2DBitmapInterpolationMode interpolationMode)
        => bitmap.Draw(drect, interpolationMode);

    public static void DrawLine(D2D_POINT_2F dp0, D2D_POINT_2F dp1, float strokeWidth, D2DSolidColorBrush brush)
        => Direct2D_DrawLine(dp0, dp1, strokeWidth, brush);

    public static void DrawRectangle(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush)
        => Direct2D_DrawRectangle(drect, strokeWidth, brush);

    public static void FillRectangle(D2D_RECT_F drect, D2DSolidColorBrush brush)
        => Direct2D_FillRectangle(drect, brush);

    public static void DrawEllipse(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush)
        => Direct2D_DrawEllipse(drect, strokeWidth, brush);

    public static void FillEllipse(D2D_RECT_F drect, D2DSolidColorBrush brush)
        => Direct2D_FillEllipse(drect, brush);

    public static void SetAntiAliasMode(D2DAntiAliasMode antiAliasMode)
        => Direct2D_SetAntiAliasMode(antiAliasMode);

    public static void PushAxisAlignedClip(D2D_RECT_F drect, D2DAntiAliasMode antiAliasMode)
        => Direct2D_PushAxisAlignedClip(drect, antiAliasMode);

    public static void PopAxisAlignedClip()
        => Direct2D_PopAxisAlignedClip();

    public static void Resize(D2D_SIZE_U usize)
        => Direct2D_Resize(usize);

    public static void Shutdown()
        => Direct2D_Shutdown();
}