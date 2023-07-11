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
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public readonly D2D_POINT_2F ToLocation()
            => new(this);
        public readonly D2D_SIZE_F ToSize()
            => new(this);
        public D2D_RECT_F(float left, float top, float right, float bottom)
            => (Left, Top, Right, Bottom) = (left, top, right, bottom);
        public D2D_RECT_F(D2D_POINT_2F point, D2D_SIZE_F size)
            => (Left, Top, Right, Bottom) = (point.X, point.Y, point.X + size.Width, point.Y + size.Height);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D_SIZE_F
    {
        public float Width;
        public float Height;

        public D2D_SIZE_F(float w, float h) => (Width, Height) = (w, h);
        public D2D_SIZE_F(D2D_RECT_F drect) => (Width, Height) = (drect.Right - drect.Left, drect.Bottom - drect.Top);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D_SIZE_U
    {
        public uint Width;
        public uint Height;

        public D2D_SIZE_U(uint w, uint h) => (Width, Height) = (w, h);
        public D2D_SIZE_U(int w, int h) => (Width, Height) = ((uint)w, (uint)h);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct D2D_POINT_2F
    {
        public float X;
        public float Y;

        public D2D_POINT_2F(float x, float y) => (X, Y) = (x, y);
        public D2D_POINT_2F(D2D_RECT_F drect) => (X, Y) = (drect.Left, drect.Top);
        public D2D_POINT_2F(D2D_POINT_2F dpt) => (X, Y) = (dpt.X, dpt.Y);
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

    internal unsafe partial class Direct2DNativeMethods
    {
        public const int
            DWRITE_FONT_WEIGHT_NORMAL  = 400,
            DWRITE_FONT_STYLE_NORMAL   = 0,
            DWRITE_FONT_STRETCH_NORMAL = 5
            ;

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int Direct2D_Initialize(IntPtr hWnd);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_BeginDraw();

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int Direct2D_EndDraw();

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_Resize(D2D_SIZE_U dsize);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_SetAntiAliasMode(D2DAntiAliasMode antialiasMode);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_PushAxisAlignedClip(D2D_RECT_F drect, D2DAntiAliasMode antialiasMode);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_PopAxisAlignedClip();

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawLine(D2D_POINT_2F dp0, D2D_POINT_2F dp1, float strokeWidth, D2DSolidColorBrush brush);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawRectangle(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_FillRectangle(D2D_RECT_F drect, D2DSolidColorBrush brush);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawEllipse(D2D_RECT_F drect, float strokeWidth, D2DSolidColorBrush brush);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_FillEllipse(D2D_RECT_F drect, D2DSolidColorBrush brush);

        #region TextFormat Methods

        [LibraryImport("EMU7800.Win32.Interop.dll", StringMarshalling = StringMarshalling.Utf16), SuppressUnmanagedCodeSecurity]
        internal static partial int Direct2D_CreateTextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, ref IntPtr ppTextFormat);

        [LibraryImport("EMU7800.Win32.Interop.dll", StringMarshalling = StringMarshalling.Utf16), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawTextFormat(IntPtr pTextFormat, string text, D2D_RECT_F drect, D2DSolidColorBrush brush);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_SetTextAlignmentForTextFormat(IntPtr pTextFormat, DWriteTextAlignment textAlignment);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_SetParagraphAlignmentForTextFormat(IntPtr pTextFormat, DWriteParaAlignment paragraphAlignment);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_ReleaseTextFormat(IntPtr pTextFormat);

        #endregion

        #region TextLayout Methods

        [LibraryImport("EMU7800.Win32.Interop.dll", StringMarshalling = StringMarshalling.Utf16), SuppressUnmanagedCodeSecurity]
        internal static partial int Direct2D_CreateTextLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height, ref IntPtr ppTextFormat, ref IntPtr ppTextLayout);

        [LibraryImport("EMU7800.Win32.Interop.dll", StringMarshalling = StringMarshalling.Utf16), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawTextLayout(IntPtr pTextLayout, D2D_POINT_2F location, D2DSolidColorBrush brush);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_SetTextAlignmentForTextLayout(IntPtr pTextLayout, DWriteTextAlignment textAlignment);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_SetParagraphAlignmentForTextLayout(IntPtr pTextLayout, DWriteParaAlignment paragraphAlignment);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_GetMetrics(IntPtr pTextLayout, ref DWRITE_TEXT_METRICS metrics);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_ReleaseTextLayout(IntPtr pTextFormat, IntPtr pTextLayout);

        #endregion

        #region StaticBitmap Methods

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int Direct2D_CreateStaticBitmap(byte* data, int len, ref IntPtr ppBitmap);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawStaticBitmap(IntPtr pBitmap, D2D_RECT_F drect);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_ReleaseStaticBitmap(IntPtr pBitmap);

        #endregion

        #region DynamicBitmap Methods

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial int Direct2D_CreateDynamicBitmap(D2D_SIZE_U bsize, ref IntPtr ppBitmap);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_LoadDynamicBitmapFromMemory(IntPtr pBitmap, byte* data, int expectedPitch);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_DrawDynamicBitmap(IntPtr pBitmap, D2D_RECT_F drect, D2DBitmapInterpolationMode interpolationMode);

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_ReleaseDynamicBitmap(IntPtr pBitmap);

        #endregion

        [LibraryImport("EMU7800.Win32.Interop.dll"), SuppressUnmanagedCodeSecurity]
        internal static partial void Direct2D_Shutdown();
    }
}
