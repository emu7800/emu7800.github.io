// © Mike Murphy

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace EMU7800.Win32.Interop;

internal delegate IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

[StructLayout(LayoutKind.Sequential)]
internal struct WNDCLASSEX
{
    public int cbSize;
    public uint style;
    public IntPtr lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    [MarshalAs(UnmanagedType.LPStr)]
    public string? lpszMenuName;
    [MarshalAs(UnmanagedType.LPStr)]
    public string? lpszClassName;
    public IntPtr hIconSm;
}

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int x;
    public int y;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MINMAXINFO
{
    public POINT ptReserved;
    public POINT ptMaxSize;
    public POINT ptMaxPosition;
    public POINT ptMinTrackSize;
    public POINT ptMaxTrackSize;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MSG
{
    public IntPtr hwnd;
    public int message;
    public IntPtr wParam;
    public IntPtr lParam;
    public int time;
    public POINT pt;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUTDEVICE
{
    public ushort usUsagePage;
    public ushort usUsage;
    public uint dwFlags;
    public IntPtr hwndTarget;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RAWINPUTHEADER
{
    public uint dwType;
    public uint dwSize;
    public IntPtr hDevice;
    public IntPtr wParam;
}

[StructLayout(LayoutKind.Explicit)]
internal struct RAWMOUSE
{
    [FieldOffset( 0)] public ushort usFlags;
    [FieldOffset( 4)] public uint ulButtons;
    [FieldOffset( 4)] public ushort usButtonFlags;
    [FieldOffset( 6)] public ushort usButtonData;
    [FieldOffset( 8)] public uint ulRawButtons;
    [FieldOffset(12)] public int lLastX;
    [FieldOffset(16)] public int lLastY;
    [FieldOffset(20)] public uint ulExtraInformation;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RAWKEYBOARD
{
    public ushort MakeCode;
    public ushort Flags;
    public ushort Reserved;
    public ushort VKey;
    public uint Message;
    public uint ExtraInformation;
}

[StructLayout(LayoutKind.Explicit)]
internal struct RAWINPUTdata
{
    [FieldOffset(0)] public RAWMOUSE mouse;
    [FieldOffset(0)] public RAWKEYBOARD keyboard;
}

[StructLayout(LayoutKind.Explicit)]
internal struct RAWINPUT
{
    [FieldOffset( 0)] public RAWINPUTHEADER header;
    [FieldOffset(24)] public RAWINPUTdata data;
}

internal static unsafe partial class Win32NativeMethods
{
    const int
        WM_CREATE        = 0x001,
        WM_DESTROY       = 0x002,
        WM_MOVE          = 0x003,
        WM_SIZE          = 0x005,
        WM_PAINT         = 0x00f,
        WM_QUIT          = 0x012,
        WM_GETMINMAXINFO = 0x024,
        WM_DISPLAYCHANGE = 0x07e,
        WM_INPUT         = 0x0ff,
        WM_KEYDOWN       = 0x100,
        WM_SYSKEYDOWN    = 0x104,
        WM_DEVICECHANGE  = 0x219,

        RIM_INPUT        = 0,
        RIM_TYPEMOUSE    = 0,
        RIM_TYPEKEYBOARD = 1,

        RID_INPUT = 0x10000003,

        MOUSE_MOVE_ABSOLUTE = 1,

        RI_MOUSE_WHEEL            = 0x400,
        RI_MOUSE_LEFT_BUTTON_DOWN = 0x001,
        RI_MOUSE_LEFT_BUTTON_UP   = 0x002,

        DBT_DEVNODES_CHANGED = 0x7,

        IDC_ARROW = 32512,

        CS_VREDRAW = 1,
        CS_HREDRAW = 2,

        WINDOW_MIN_WIDTH  = 800,
        WINDOW_MIN_HEIGHT = 480,

        SM_CXFULLSCREEN = 16,
        SM_CYFULLSCREEN = 17,

        PM_REMOVE = 1,

        HID_USAGE_PAGE_GENERIC     = 1,
        HID_USAGE_GENERIC_MOUSE    = 2,
        HID_USAGE_GENERIC_KEYBOARD = 6,

        WS_OVERLAPPED  = 0x000000,
        WS_CAPTION     = 0xC00000,
        WS_SYSMENU     = 0x080000,
        WS_THICKFRAME  = 0x040000,
        WS_MINIMIZEBOX = 0x020000,
        WS_MAXIMIZEBOX = 0x010000,
        WS_OVERLAPPEDWINDOW
                       = WS_OVERLAPPED
                       | WS_CAPTION
                       | WS_SYSMENU
                       | WS_THICKFRAME
                       | WS_MINIMIZEBOX
                       | WS_MAXIMIZEBOX
        ;

    const float
        D2D_DPI = 96.0f
        ;

    const string
        EMU7800 = "EMU7800"
        ;

    static readonly WndProc WndProcDelegate = WndProc;
    static int lpdSize, dpiForSystem, last_x, last_y;
    static IntPtr lpd;

    static IntPtr WndProc(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam)
    {
        switch (message)
        {
            case WM_CREATE:

                // About Raw Input
                // http://msdn.microsoft.com/en-us/library/windows/desktop/ms645543(v=vs.85).aspx

                var rid = new RAWINPUTDEVICE[2];

                rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
                rid[0].usUsage     = HID_USAGE_GENERIC_MOUSE;
                rid[0].hwndTarget  = IntPtr.Zero;
                rid[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
                rid[1].usUsage     = HID_USAGE_GENERIC_KEYBOARD;
                rid[1].hwndTarget  = IntPtr.Zero;

                RegisterRawInputDevices(rid, 2, sizeof(RAWINPUTDEVICE));
                return 0;

            case WM_DESTROY:
                PostQuitMessage(0);
                return 0;

            case WM_MOVE:
                return 0;

            case WM_SIZE:
                var w = (int)(lParam & 0xffff);
                var h = (int)((lParam >> 16) & 0xffff);
                Win32Window.Resized(w, h);
                return 0;

            case WM_PAINT:
                ValidateRect(hWnd, IntPtr.Zero);
                Win32Window.LURCycle();
                return 0;

            case WM_GETMINMAXINFO:
                var pMinMaxInfo = (MINMAXINFO*)lParam;
                pMinMaxInfo->ptMinTrackSize.x = WINDOW_MIN_WIDTH;
                pMinMaxInfo->ptMinTrackSize.y = WINDOW_MIN_HEIGHT;
                return 0;

            case WM_DISPLAYCHANGE:
                InvalidateRect(hWnd, IntPtr.Zero, 0);
                return 0;

            case WM_INPUT:
                if ((wParam & 0xff) != RIM_INPUT)
                {
                    break;
                }

                var dwSize = 0;
                GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref dwSize, sizeof(RAWINPUTHEADER));
                if (lpd == IntPtr.Zero || lpdSize < dwSize)
                {
                    if (lpd != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(lpd);
                        lpdSize = 0;
                        lpd = IntPtr.Zero;
                    }
                    lpdSize = dwSize;
                    lpd = Marshal.AllocHGlobal(dwSize);
                    if (lpd == IntPtr.Zero)
                    {
                        break;
                    }
                }

                GetRawInputData(lParam, RID_INPUT, lpd, ref dwSize, sizeof(RAWINPUTHEADER));
                var raw = (RAWINPUT*)lpd;
                if (raw->header.dwType == RIM_TYPEMOUSE)
                {
                    POINT pt;
                    RECT rect;
                    if (!GetCursorPos(&pt) || !ScreenToClient(hWnd, &pt) || !GetClientRect(hWnd, &rect))
                    {
                        break;
                    }
                    if (pt.x < rect.left || pt.x > rect.right || pt.y < rect.top || pt.y > rect.bottom)
                    {
                        break;
                    }

                    var x = (int)(pt.x * dpiForSystem / D2D_DPI);
                    var y = (int)(pt.y * dpiForSystem / D2D_DPI);

                    var dx = raw->data.mouse.lLastX;
                    var dy = raw->data.mouse.lLastY;

                    if ((raw->data.mouse.usFlags & MOUSE_MOVE_ABSOLUTE) != 0)
                    {
                        (dx, dy) = (x - last_x, y - last_y);
                        (last_x, last_y) = (x, y);
                    }

                    Win32Window.MouseMoved(x, y, dx, dy);

                    if ((raw->data.mouse.usButtonFlags & RI_MOUSE_WHEEL) != 0)
                    {
                        var delta = (short)raw->data.mouse.usButtonData;
                        Win32Window.MouseWheelChanged(x, y, delta);
                    }

                    if ((raw->data.mouse.ulButtons & (RI_MOUSE_LEFT_BUTTON_DOWN | RI_MOUSE_LEFT_BUTTON_UP)) != 0)
                    {
                        var down = (raw->data.mouse.ulButtons & RI_MOUSE_LEFT_BUTTON_DOWN) != 0;
                        Win32Window.MouseButtonChanged(x, y, down);
                    }
                }
                else if (raw->header.dwType == RIM_TYPEKEYBOARD)
                {
                    var down = raw->data.keyboard.Message == WM_KEYDOWN || raw->data.keyboard.Message == WM_SYSKEYDOWN;
                    Win32Window.KeyboardKeyPressed(raw->data.keyboard.VKey, down);
                }
                return 0;

            case WM_DEVICECHANGE:
                if (wParam == DBT_DEVNODES_CHANGED)
                {
                    Win32Window.DeviceChanged();
                }
                return 0;
        }

        return DefWindowProc(hWnd, message, wParam, lParam);
    }

    public static IntPtr Win32_CreateWindow()
    {
        var wcex = new WNDCLASSEX
        {
            cbSize        = Marshal.SizeOf<WNDCLASSEX>(),
            style         = CS_HREDRAW | CS_VREDRAW,
            hInstance     = -1,
            lpfnWndProc   = Marshal.GetFunctionPointerForDelegate(WndProcDelegate),
            hCursor       = LoadCursor(IntPtr.Zero, IDC_ARROW),
            hIcon         = ExtractAssociatedIcon(IntPtr.Zero, new StringBuilder("EMU7800.exe"), out _),
            lpszClassName = EMU7800,
            lpszMenuName  = null
        };

        if (!RegisterClassEx(in wcex))
        {
            return 0;
        }

        dpiForSystem = GetDpiForSystem();

        var windowWidth   = (int)(WINDOW_MIN_WIDTH  * dpiForSystem / D2D_DPI);
        var windowHeight  = (int)(WINDOW_MIN_HEIGHT * dpiForSystem / D2D_DPI);
        var desktopWidth  = GetSystemMetrics(SM_CXFULLSCREEN);
        var desktopHeight = GetSystemMetrics(SM_CYFULLSCREEN);
        var posX = (desktopWidth  >> 1) - (windowWidth  >> 1);
        var posY = (desktopHeight >> 1) - (windowHeight >> 1);

         return CreateWindowEx(0, EMU7800, EMU7800, WS_OVERLAPPEDWINDOW,
            posX, posY, windowWidth, windowHeight, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    public static void Win32_ProcessEvents(IntPtr hWnd, int nCmdShow)
    {
        ShowWindow(hWnd, nCmdShow);
        UpdateWindow(hWnd);

        var isVisible = true;

        while (true)
        {
            if (IsIconic(hWnd))
            {
                if (isVisible)
                {
                    isVisible = false;
                    Win32Window.VisibilityChanged(isVisible);
                }
            }
            else
            {
                if (!isVisible)
                {
                    isVisible = true;
                    Win32Window.VisibilityChanged(isVisible);
                }
            }

            MSG msg;
            var gotMsg = isVisible ? PeekMessage(&msg, IntPtr.Zero, 0, 0, PM_REMOVE) : GetMessage(&msg, IntPtr.Zero, 0, 0);

            if (msg.message == WM_QUIT)
            {
                break;
            }

            if (gotMsg)
            {
                TranslateMessage(&msg);
                DispatchMessage(&msg);
            }
            else
            {
                Win32Window.LURCycle();
            }
        }
    }

    #pragma warning disable SYSLIB1054
    #pragma warning disable CA2101

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool RegisterClassEx(in WNDCLASSEX lpWndClass);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern IntPtr CreateWindowEx(int dwExStyle, [param: MarshalAs(UnmanagedType.LPStr)] string lpClassName, [param: MarshalAs(UnmanagedType.LPWStr)] string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("shell32.dll"), SuppressUnmanagedCodeSecurity]
    static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder lpIconPath, out ushort lpiIcon);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int GetDpiForSystem();

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern void PostQuitMessage(int nExitCode);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, int uiNumDevices, int cbSize);

    [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);


    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial void InvalidateRect(IntPtr hWnd, IntPtr lpRect, int bErase);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial void ValidateRect(IntPtr hWnd, IntPtr lpRect);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool IsIconic(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW"), SuppressUnmanagedCodeSecurity]
    internal static partial IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref int pcdSize, int cbSizeHeader);

    [LibraryImport("user32.dll", EntryPoint = "PeekMessageW"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool PeekMessage(MSG* lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax, int wRemoveMsg);

    [LibraryImport("user32.dll", EntryPoint = "GetMessageW"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetMessage(MSG* lpMsg, IntPtr hWnd, int wMsgFilterMin, int wMsgFilterMax);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    internal static partial void TranslateMessage(MSG* lpMsg);

    [LibraryImport("user32.dll", EntryPoint = "DispatchMessageW"), SuppressUnmanagedCodeSecurity]
    internal static partial void DispatchMessage(MSG* lpMsg);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetCursorPos(POINT* lpPoint);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ScreenToClient(IntPtr hWnd, POINT* lpPoint);

    [LibraryImport("user32.dll"), SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetClientRect(IntPtr hWnd, RECT* lpRect);
}