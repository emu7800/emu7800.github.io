using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800.Win32.Interop
{
    public enum D2DSolidColorBrush { Black, Red, Orange, Yellow, Green, Blue, Gray, White };
    public enum D2DAntiAliasMode { PerPrimitive, Aliased };
    public enum D2DBitmapInterpolationMode { NearestNeighbor, Linear };
    public enum DWriteTextAlignment { Leading, Trailing, Center };
    public enum DWriteParaAlignment { Near, Far, Center };

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D_RECT_F
    {
        internal float Left;
        internal float Top;
        internal float Right;
        internal float Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D_SIZE_U
    {
        internal uint Width;
        internal uint Height;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D_POINT_2F
    {
        internal float X;
        internal float Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DWRITE_TEXT_METRICS
    {
        internal float left;
        internal float top;
        internal float width;
        internal float widthIncludingTrailingWhitespace;
        internal float height;
        internal float layoutWidth;
        internal float layoutHeight;
        internal uint maxBidiReorderingDepth;
        internal uint lineCount;
    }

    internal unsafe class Direct2DNativeMethods
    {
        public const int
            DWRITE_FONT_WEIGHT_NORMAL  = 400,
            DWRITE_FONT_STYLE_NORMAL   = 0,
            DWRITE_FONT_STRETCH_NORMAL = 5
            ;

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int Direct2D_Initialize(IntPtr hWnd);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_BeginDraw();

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int Direct2D_EndDraw();

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_Resize(D2D_SIZE_U dsize);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_SetAntiAliasMode(D2DAntiAliasMode antialiasMode);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_PushAxisAlignedClip(D2D_RECT_F drect, D2DAntiAliasMode antialiasMode);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_PopAxisAlignedClip();

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawLine(D2D_POINT_2F dp0, D2D_POINT_2F dp1, float strokeWidth, D2DSolidColorBrush brush);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawRectangle(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_FillRectangle(D2D_RECT_F drect, D2DSolidColorBrush brush);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawEllipse(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_FillEllipse(D2D_RECT_F drect, D2DSolidColorBrush brush);

        #region TextFormat Methods

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        public static extern int Direct2D_CreateTextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, ref IntPtr ppTextFormat);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawTextFormat(IntPtr pTextFormat, string text, D2D_RECT_F drect, D2DSolidColorBrush brush);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_SetTextAlignmentForTextFormat(IntPtr pTextFormat, DWriteTextAlignment textAlignment);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_SetParagraphAlignmentForTextFormat(IntPtr pTextFormat, DWriteParaAlignment paragraphAlignment);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_ReleaseTextFormat(IntPtr pTextFormat);

        #endregion

        #region TextLayout Methods

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        public static extern int Direct2D_CreateTextLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height, ref IntPtr ppTextFormat, ref IntPtr ppTextLayout);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawTextLayout(IntPtr pTextLayout, D2D_POINT_2F location, D2DSolidColorBrush brush);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_SetTextAlignmentForTextLayout(IntPtr pTextLayout, DWriteTextAlignment textAlignment);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_SetParagraphAlignmentForTextLayout(IntPtr pTextLayout, DWriteParaAlignment paragraphAlignment);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_GetMetrics(IntPtr pTextLayout, out DWRITE_TEXT_METRICS metrics);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_ReleaseTextLayout(IntPtr pTextFormat, IntPtr pTextLayout);

        #endregion

        #region StaticBitmap Methods

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int Direct2D_CreateStaticBitmap(byte* data, int len, ref IntPtr ppBitmap);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawStaticBitmap(IntPtr pBitmap, D2D_RECT_F drect);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_ReleaseStaticBitmap(IntPtr pBitmap);

        #endregion

        #region DynamicBitmap Methods

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern int Direct2D_CreateDynamicBitmap(D2D_SIZE_U bsize, ref IntPtr ppBitmap);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_LoadDynamicBitmapFromMemory(IntPtr pBitmap, byte* data, int expectedPitch);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_DrawDynamicBitmap(IntPtr pBitmap, D2D_RECT_F drect, D2DBitmapInterpolationMode interpolationMode);

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_ReleaseDynamicBitmap(IntPtr pBitmap);

        #endregion

        [DllImport("EMU7800.Win32.Interop.Unmanaged.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
        public static extern void Direct2D_Shutdown();
    }
}
