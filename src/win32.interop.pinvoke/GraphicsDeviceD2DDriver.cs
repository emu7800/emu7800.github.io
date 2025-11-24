// © Mike Murphy

using EMU7800.Shell;
using System;

namespace EMU7800.Win32.Interop;

public sealed class GraphicsDeviceD2DDriver : IGraphicsDeviceDriver
{
    public static GraphicsDeviceD2DDriver Factory(IntPtr hWnd) => new(hWnd);
    GraphicsDeviceD2DDriver() {}

    #region IGraphicsDeviceDriver Members

    public int EC { get; private set; }
    public void BeginDraw()
      => Direct2DNativeMethods.Direct2D_BeginDraw();
    public void DrawEllipse(RectF drect, float strokeWidth, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_DrawEllipse(drect, strokeWidth, brush);
    public void DrawLine(PointF dp0, PointF dp1, float strokeWidth, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_DrawLine(dp0, dp1, strokeWidth, brush);
    public void DrawRectangle(RectF drect, float strokeWidth, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_DrawRectangle(drect, strokeWidth, brush);
    public int EndDraw()
      => Direct2DNativeMethods.Direct2D_EndDraw();
    public void FillEllipse(RectF drect, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_FillEllipse(drect, brush);
    public void FillRectangle(RectF drect, SolidColorBrush brush)
      => Direct2DNativeMethods.Direct2D_FillRectangle(drect, brush);
    public void PopAxisAlignedClip()
      => Direct2DNativeMethods.Direct2D_PopAxisAlignedClip();
    public void PushAxisAlignedClip(RectF drect, AntiAliasMode antiAliasMode)
      => Direct2DNativeMethods.Direct2D_PushAxisAlignedClip(drect, antiAliasMode);
    public void Resize(SizeU usize)
      => Direct2DNativeMethods.Direct2D_Resize(usize);
    public void SetAntiAliasMode(AntiAliasMode antiAliasMode)
      => Direct2DNativeMethods.Direct2D_SetAntiAliasMode(antiAliasMode);
    public void Shutdown()
      => Direct2DNativeMethods.Direct2D_Shutdown();

    #endregion

    #region Constructors

    public GraphicsDeviceD2DDriver(IntPtr hWnd)
      => EC = Direct2DNativeMethods.Direct2D_Initialize(hWnd);

    #endregion
}