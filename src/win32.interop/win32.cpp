// © Mike Murphy

#include "pch.h"

#define WINDOW_MIN_WIDTH  800
#define WINDOW_MIN_HEIGHT 480

#define D2D_DPI 96.0f

#define HID_USAGE_PAGE_GENERIC        0x01
#define HID_USAGE_GENERIC_MOUSE       0x02
#define HID_USAGE_GENERIC_KEYBOARD    0x06

void (*g_KeyboardKeyPressedCallback)(USHORT, bool)   = NULL;
void (*g_MouseMovedCallback)(int, int, int, int)     = NULL;
void (*g_MouseButtonChangedCallback)(int, int, bool) = NULL;
void (*g_MouseWheelChangedCallback)(int, int, int)   = NULL;
void (*g_LURCycleCallback)()                         = NULL;
void (*g_VisibilityChangedCallback)(bool)            = NULL;
void (*g_ResizedCallback)(int, int)                  = NULL;

EXTERN_C IMAGE_DOS_HEADER __ImageBase;
HINSTANCE GetThisInstance()
{
    return (HINSTANCE)&__ImageBase;
}

LRESULT WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
        case WM_CREATE:
        {
            // About Raw Input
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms645543(v=vs.85).aspx

            RAWINPUTDEVICE rid[2] = { 0 };
            rid[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
            rid[0].usUsage     = HID_USAGE_GENERIC_MOUSE;
            rid[0].hwndTarget  = 0;

            rid[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
            rid[1].usUsage     = HID_USAGE_GENERIC_KEYBOARD;
            rid[1].hwndTarget  = 0;

            RegisterRawInputDevices(rid, 2, sizeof(RAWINPUTDEVICE));
            return 0;
        }

        case WM_INPUT:
        {
            static UINT lpbSize = 0;
            static LPBYTE lpb = NULL;

            if (GET_RAWINPUT_CODE_WPARAM(wParam) != RIM_INPUT)
                break;

            UINT dwSize = 0;
            GetRawInputData((HRAWINPUT)lParam, RID_INPUT, NULL, &dwSize, sizeof(RAWINPUTHEADER));
            if (!lpb || lpbSize < dwSize)
            {
                HANDLE hHeap = GetProcessHeap();
                if (lpb)
                    HeapFree(hHeap, 0, lpb);
                lpb = (BYTE*)HeapAlloc(hHeap, 0, dwSize);
                if (!lpb)
                    break;
            }

            GetRawInputData((HRAWINPUT)lParam, RID_INPUT, lpb, &dwSize, sizeof(RAWINPUTHEADER));
            RAWINPUT* raw = (RAWINPUT*)lpb;
            if (raw->header.dwType == RIM_TYPEMOUSE)
            {
                POINT pt = { 0 };
                RECT rect = { 0 };
                if (!GetCursorPos(&pt) || !ScreenToClient(hWnd, &pt) || !GetClientRect(hWnd, &rect))
                    break;

                if (pt.x < rect.left || pt.x > rect.right || pt.y < rect.top || pt.y > rect.bottom)
                    break;

                int dpi = GetDpiForSystem();
                int x = static_cast<INT>(round(pt.x * dpi / D2D_DPI));
                int y = static_cast<INT>(round(pt.y * dpi / D2D_DPI));

                int dx = raw->data.mouse.lLastX;
                int dy = raw->data.mouse.lLastY;

                if (raw->data.mouse.usFlags & MOUSE_MOVE_ABSOLUTE)
                {
                    static int last_x = x;
                    static int last_y = y;
                    dx = x - last_x;
                    dy = y - last_y;
                    last_x = x;
                    last_y = y;
                }

                if (g_MouseMovedCallback && (dx || dy))
                {
                    g_MouseMovedCallback(x, y, dx, dy);
                }

                if (g_MouseWheelChangedCallback && (raw->data.mouse.usButtonFlags & RI_MOUSE_WHEEL))
                {
                    int delta = (SHORT)raw->data.mouse.usButtonData;
                    g_MouseWheelChangedCallback(x, y, delta);
                }

                const UINT mouseButtonFlags = RI_MOUSE_LEFT_BUTTON_DOWN | RI_MOUSE_LEFT_BUTTON_UP | RI_MOUSE_RIGHT_BUTTON_DOWN | RI_MOUSE_RIGHT_BUTTON_UP;
                if (g_MouseButtonChangedCallback && (raw->data.mouse.ulButtons & mouseButtonFlags))
                {
                    bool leftDown = (raw->data.mouse.ulButtons & RI_MOUSE_LEFT_BUTTON_DOWN) == RI_MOUSE_LEFT_BUTTON_DOWN;
                    bool rightDown = (raw->data.mouse.ulButtons & RI_MOUSE_RIGHT_BUTTON_DOWN) == RI_MOUSE_RIGHT_BUTTON_DOWN;
                    g_MouseButtonChangedCallback(x, y, leftDown || rightDown);
                }
            }
            else if (g_KeyboardKeyPressedCallback && raw->header.dwType == RIM_TYPEKEYBOARD)
            {
                bool down = raw->data.keyboard.Message == WM_KEYDOWN || raw->data.keyboard.Message == WM_SYSKEYDOWN;
                g_KeyboardKeyPressedCallback(raw->data.keyboard.VKey, down);
            }
            return 0;
        }

        case WM_SIZE:
        {
            if (g_ResizedCallback)
            {
                int w = LOWORD(lParam);
                int h = HIWORD(lParam);
                g_ResizedCallback(w, h);
            }
            return 0;
        }

        case WM_GETMINMAXINFO:
        {
            MINMAXINFO* pMinMaxInfo = reinterpret_cast<MINMAXINFO*>(lParam);
            pMinMaxInfo->ptMinTrackSize.x = WINDOW_MIN_WIDTH;
            pMinMaxInfo->ptMinTrackSize.y = WINDOW_MIN_HEIGHT;
            return 0;
        }

        case WM_MOVE:
        {
            return 0;
        }

        case WM_DISPLAYCHANGE:
        {
            InvalidateRect(hWnd, NULL, FALSE);
            return 0;
        }

        case WM_PAINT:
        {
            ValidateRect(hWnd, NULL);
            if (g_LURCycleCallback)
            {
                g_LURCycleCallback();
            }
            return 0;
        }

        case WM_DESTROY:
        {
            PostQuitMessage(0);
            return 0;
        }
    }
    return DefWindowProc(hWnd, message, wParam, lParam);
}

extern "C" __declspec(dllexport) HWND __stdcall Win32_Initialize(
    void (*keyboardKeyPressedCallback)(USHORT, bool),
    void (*mouseMovedCallback)(int, int, int, int),
    void (*mouseButtonChangedCallback)(int, int, bool),
    void (*mouseWheelChangedCallback)(int, int, int),
    void (*lurCycleCallback)(),
    void (*visibilityChangedCallback)(bool),
    void (*resizedCallback)(int, int))
{
    g_KeyboardKeyPressedCallback = keyboardKeyPressedCallback;
    g_MouseMovedCallback         = mouseMovedCallback;
    g_MouseButtonChangedCallback = mouseButtonChangedCallback;
    g_MouseWheelChangedCallback  = mouseWheelChangedCallback;
    g_LURCycleCallback           = lurCycleCallback;
    g_VisibilityChangedCallback  = visibilityChangedCallback;
    g_ResizedCallback            = resizedCallback;

    WNDCLASSEX wcex    = { sizeof(wcex) };
    wcex.style         = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc   = (WNDPROC)WndProc;
    wcex.cbWndExtra    = sizeof(LONG_PTR);
    wcex.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wcex.hIcon         = LoadIcon(GetThisInstance(), MAKEINTRESOURCE(IDI_ICON1));
    wcex.lpszClassName = L"EMU7800";

    if (!RegisterClassEx(&wcex))
    {
        return 0;
    }

    int dpi = GetDpiForSystem();

    int windowWidth   = static_cast<UINT>(round(WINDOW_MIN_WIDTH  * dpi / D2D_DPI));
    int windowHeight  = static_cast<UINT>(round(WINDOW_MIN_HEIGHT * dpi / D2D_DPI));
    int desktopWidth  = GetSystemMetrics(SM_CXFULLSCREEN);
    int desktopHeight = GetSystemMetrics(SM_CYFULLSCREEN);
    int posX = (desktopWidth  >> 1) - (windowWidth  >> 1);
    int posY = (desktopHeight >> 1) - (windowHeight >> 1);

    return CreateWindow(L"EMU7800", L"EMU7800",
        WS_OVERLAPPEDWINDOW, posX, posY, windowWidth, windowHeight,
        NULL, NULL, NULL, NULL);
}

extern "C" __declspec(dllexport) void __stdcall Win32_ProcessEvents(HWND hWnd)
{
    ShowWindow(hWnd, SW_SHOWNORMAL);
    UpdateWindow(hWnd);

    bool isVisible = true;

    MSG msg = { 0 };
    while (true)
    {
        if (IsIconic(hWnd))
        {
            if (isVisible)
            {
                isVisible = false;
                if (g_VisibilityChangedCallback)
                {
                    g_VisibilityChangedCallback(isVisible);
                }
            }
        }
        else
        {
            if (!isVisible)
            {
                isVisible = true;
                if (g_VisibilityChangedCallback)
                {
                    g_VisibilityChangedCallback(isVisible);
                }
            }
        }

        BOOL gotMsg = isVisible ? PeekMessage(&msg, NULL, 0, 0, PM_REMOVE) : GetMessage(&msg, NULL, 0, 0);

        if (msg.message == WM_QUIT)
            break;

        if (gotMsg)
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        else
        {
            if (g_LURCycleCallback)
            {
                g_LURCycleCallback();
            }
        }
    }

    ShowWindow(hWnd, SW_HIDE);
}

extern "C" _declspec(dllexport) void __stdcall Win32_Quit()
{
    PostQuitMessage(0);
}