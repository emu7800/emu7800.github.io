using System;
using System.Runtime.InteropServices;
using System.Security;
using EMU7800.Shell;

namespace EMU7800.Win32.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct D2DSolidColorBrush
{
    private readonly int _value;

    private D2DSolidColorBrush(int value) => _value = value;
    public static implicit operator D2DSolidColorBrush(SolidColorBrush b) => b switch
    {
        SolidColorBrush.Black  => new(0),
        SolidColorBrush.Red    => new(1),
        SolidColorBrush.Orange => new(2),
        SolidColorBrush.Yellow => new(3),
        SolidColorBrush.Green  => new(4),
        SolidColorBrush.Blue   => new(5),
        SolidColorBrush.Gray   => new(6),
        SolidColorBrush.White  => new(7),
        _ => new(0)
    };
    public static implicit operator int(D2DSolidColorBrush b) => b._value;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct D2DAntiAliasMode
{
    private readonly int _value;

    private D2DAntiAliasMode(int value) => _value = value;
    public static implicit operator D2DAntiAliasMode(AntiAliasMode m) => m switch
    {
        AntiAliasMode.PerPrimitive => new(0),
        AntiAliasMode.Aliased      => new(1),
        _ => new(0)
    };
    public static implicit operator int(D2DAntiAliasMode m) => m._value;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct D2DBitmapInterpolationMode
{
    private readonly int _value;

    private D2DBitmapInterpolationMode(int value) => _value = value;
    public static implicit operator D2DBitmapInterpolationMode(BitmapInterpolationMode m) => m switch
    {
        BitmapInterpolationMode.NearestNeighbor => new(0),
        BitmapInterpolationMode.Linear          => new(1),
        _ => new(0)
    };
    public static implicit operator int(D2DBitmapInterpolationMode m) => m._value;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct DWriteTextAlignment
{
    private readonly int _value;

    private DWriteTextAlignment(int value) => _value = value;
    public static implicit operator DWriteTextAlignment(WriteTextAlignment a) => a switch
    {
        WriteTextAlignment.Leading  => new(0),
        WriteTextAlignment.Trailing => new(1),
        WriteTextAlignment.Center   => new(2),
        _ => new(0)
    };
    public static implicit operator int(DWriteTextAlignment a) => a._value;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct DWriteParaAlignment
{
    private readonly int _value;

    private DWriteParaAlignment(int value) => _value = value;
    public static implicit operator DWriteParaAlignment(WriteParaAlignment a) => a switch
    {
        WriteParaAlignment.Near   => new(0),
        WriteParaAlignment.Far    => new(1),
        WriteParaAlignment.Center => new(2),
        _ => new(0)
    };
    public static implicit operator int(DWriteParaAlignment a) => a._value;
}

[StructLayout(LayoutKind.Sequential)]
internal struct D2D_RECT_F
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;
    public static implicit operator D2D_RECT_F(RectF r) => new() { Left = r.Left, Top = r.Top, Right = r.Right, Bottom = r.Bottom };
}

[StructLayout(LayoutKind.Sequential)]
internal struct D2D_SIZE_F
{
    public float Width;
    public float Height;
    public static implicit operator D2D_SIZE_F(SizeF s) => new() { Width = s.Width, Height = s.Height };
}

[StructLayout(LayoutKind.Sequential)]
internal partial struct D2D_SIZE_U
{
    public uint Width;
    public uint Height;
}

internal partial struct D2D_SIZE_U
{
    public static implicit operator D2D_SIZE_U(SizeU s) => new() { Width = s.Width, Height = s.Height };
}

[StructLayout(LayoutKind.Sequential)]
internal struct D2D_POINT_2F
{
    public float X;
    public float Y;
    public static implicit operator D2D_POINT_2F(PointF pt) => new() { X = pt.X, Y = pt.Y };
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