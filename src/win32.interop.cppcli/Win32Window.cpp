// © Mike Murphy

#include "stdafx.h"
#include "Win32Window.h"
#include "GraphicsDevice.h"
#include "resource.h"
#include "D2DStructs.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace EMU7800::D2D::Interop;

#define HID_USAGE_PAGE_GENERIC        0x01
#define HID_USAGE_GENERIC_MOUSE       0x02
#define HID_USAGE_GENERIC_KEYBOARD    0x06

#pragma unmanaged
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
HINSTANCE GetThisInstance()
{
    return (HINSTANCE)&__ImageBase;
}
#pragma managed

LRESULT Win32Window::WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
        case WM_CREATE:
        {
            // About Raw Input
            // http://msdn.microsoft.com/en-us/library/windows/desktop/ms645543(v=vs.85).aspx

            RAWINPUTDEVICE rid[2] = {0};
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
                POINT pt = {0};
                RECT rect = {0};
                if (!GetCursorPos(&pt) || !ScreenToClient(m_hWnd, &pt) || !GetClientRect(m_hWnd, &rect))
                    break;

                if (pt.x < rect.left || pt.x > rect.right || pt.y < rect.top || pt.y > rect.bottom)
                    break;

                int x = static_cast<INT>(round(pt.x * m_dpiX / D2D_DPI));
                int y = static_cast<INT>(round(pt.y * m_dpiY / D2D_DPI));

                if (MouseMoved != nullptr)
                {
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

                    if (dx || dy)
                        MouseMoved(x, y, dx, dy);
                }

                if (MouseWheelChanged != nullptr && (raw->data.mouse.usButtonFlags & RI_MOUSE_WHEEL))
                {
                    int delta = (SHORT)raw->data.mouse.usButtonData;
                    MouseWheelChanged(x, y, delta);
                }

                const UINT mouseButtonFlags = RI_MOUSE_LEFT_BUTTON_DOWN | RI_MOUSE_LEFT_BUTTON_UP | RI_MOUSE_RIGHT_BUTTON_DOWN | RI_MOUSE_RIGHT_BUTTON_UP;
                if (MouseButtonChanged != nullptr && (raw->data.mouse.ulButtons & mouseButtonFlags))
                {
                    bool leftDown  = (raw->data.mouse.ulButtons & RI_MOUSE_LEFT_BUTTON_DOWN)  == RI_MOUSE_LEFT_BUTTON_DOWN;
                    bool rightDown = (raw->data.mouse.ulButtons & RI_MOUSE_RIGHT_BUTTON_DOWN) == RI_MOUSE_RIGHT_BUTTON_DOWN;
                    MouseButtonChanged(x, y, leftDown || rightDown);
                }
            }
            else if (raw->header.dwType == RIM_TYPEKEYBOARD)
            {
                if (KeyboardKeyPressed != nullptr)
                {
                    bool down = raw->data.keyboard.Message == WM_KEYDOWN || raw->data.keyboard.Message == WM_SYSKEYDOWN;
                    KeyboardKeyPressed(raw->data.keyboard.VKey, down);
                }
            }

            return 0;
        }

        case WM_SIZE:
        {
            SizeU newSize;
            newSize.Width = LOWORD(lParam);
            newSize.Height = HIWORD(lParam);
            m_pGraphicsDevice->Resize(newSize);
            if (Resized != nullptr)
                Resized(newSize);
            return 0;
        }

        case WM_GETMINMAXINFO:
        {
            MINMAXINFO *pMinMaxInfo = reinterpret_cast<MINMAXINFO*>(lParam);
            pMinMaxInfo->ptMinTrackSize.x = WINDOW_MIN_WIDTH;
            pMinMaxInfo->ptMinTrackSize.y = WINDOW_MIN_HEIGHT;
            return 0;
        }

        case WM_MOVE:
        {
            return 0;
        }

        case WM_DISPLAYCHANGE:
            this->DisplayChange();
            InvalidateRect(hWnd, NULL, FALSE);
            return 0;

        case WM_PAINT:
            ValidateRect(hWnd, NULL);
            if (LURCycle != nullptr)
            {
                LURCycle(m_pGraphicsDevice);
            }
            return 0;

        case WM_DESTROY:
            PostQuitMessage(0);
            return 0;
    }
    return DefWindowProc(hWnd, message, wParam, lParam);
}

void Win32Window::DisplayChange()
{
}

void Win32Window::ProcessEvents()
{
    ShowWindow(m_hWnd, SW_SHOWNORMAL);
    UpdateWindow(m_hWnd);

    bool isVisible = true;

    MSG msg = {0};
    while (true)
    {
        if (IsIconic(m_hWnd))
        {
            if (isVisible && VisibilityChanged != nullptr)
            {
                isVisible = false;
                VisibilityChanged(isVisible);
            }
        }
        else
        {
            if (!isVisible && VisibilityChanged != nullptr)
            {
                isVisible = true;
                VisibilityChanged(isVisible);
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
        else if (LURCycle != nullptr)
        {
            LURCycle(m_pGraphicsDevice);
        }
    }

    ShowWindow(m_hWnd, SW_HIDE);
}

void Win32Window::Quit()
{
    PostQuitMessage(0);
}

Win32Window::Win32Window() : m_hWnd(0)
{
    m_pGraphicsDevice = gcnew GraphicsDevice();

    HRESULT hr = m_pGraphicsDevice->Initialize();
    if FAILED(hr)
    {
        Environment::Exit(hr);
    }

    m_wndProc = gcnew WndProcDelegate(this, &Win32Window::WndProc);
    IntPtr fpIntPtr = Marshal::GetFunctionPointerForDelegate(m_wndProc);

    WNDCLASSEX wcex    = { sizeof(wcex) };
    wcex.style         = CS_HREDRAW | CS_VREDRAW;
    wcex.lpfnWndProc   = (WNDPROC)fpIntPtr.ToPointer();
    wcex.cbWndExtra    = sizeof(LONG_PTR);
    wcex.hCursor       = LoadCursor(NULL, IDC_ARROW);
    wcex.hIcon         = LoadIcon(GetThisInstance(), MAKEINTRESOURCE(IDI_ICON1));
    wcex.lpszClassName = L"EMU7800";

    if (!RegisterClassEx(&wcex))
    {
        hr = Marshal::GetHRForLastWin32Error();
        Environment::Exit(hr);
    }

    float dpi = (float)GetDpiForSystem();
    m_dpiX = dpi;
    m_dpiY = dpi;

    int windowWidth = static_cast<UINT>(round(WINDOW_MIN_WIDTH  * m_dpiX / D2D_DPI));
    int windowHeight = static_cast<UINT>(round(WINDOW_MIN_HEIGHT * m_dpiY / D2D_DPI));
    int desktopWidth = GetSystemMetrics(SM_CXFULLSCREEN);
    int desktopHeight = GetSystemMetrics(SM_CYFULLSCREEN);
    int posX = (desktopWidth >> 1) - (windowWidth >> 1);
    int posY = (desktopHeight >> 1) - (windowHeight >> 1);

    m_hWnd = CreateWindow(L"EMU7800", L"EMU7800",
        WS_OVERLAPPEDWINDOW, posX, posY, windowWidth, windowHeight,
        NULL, NULL, NULL, NULL);

    if (!m_hWnd)
    {
        hr = Marshal::GetHRForLastWin32Error();
        Environment::Exit(hr);
    }

    m_pGraphicsDevice->AttachHwnd(m_hWnd);
}

Win32Window::~Win32Window()
{
    this->!Win32Window();
}

Win32Window::!Win32Window()
{
    if (m_pGraphicsDevice != nullptr)
    {
        delete m_pGraphicsDevice;
        m_pGraphicsDevice = nullptr;
    }
    if (m_hWnd)
    {
        CloseWindow(m_hWnd);
        DestroyWindow(m_hWnd);
        m_hWnd = NULL;
    }
    UnregisterClass(L"EMU7800", NULL);
}