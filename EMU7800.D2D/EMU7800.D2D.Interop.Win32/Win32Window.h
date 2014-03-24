// © Mike Murphy

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

namespace EMU7800 { namespace D2D { namespace Interop {

ref class JoystickDevice;
ref class GraphicsDevice;
value struct SizeU;

public ref class Win32Window
{
private:
    HWND m_hWnd;
    float m_dpiX;
    float m_dpiY;
    GraphicsDevice^ m_pGraphicsDevice;

    delegate LRESULT WndProcDelegate(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);
    WndProcDelegate^ m_wndProc;

protected:
    LRESULT WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);

public:
    literal float D2D_DPI           = 96.0f;
    literal int   WINDOW_MIN_WIDTH  = 800,
                  WINDOW_MIN_HEIGHT = 480;
    literal int   MOUSE_WHEEL_DELTA = WHEEL_DELTA;

    property IntPtr Hwnd { IntPtr get() { return IntPtr(m_hWnd); } }

    delegate void KeyboardKeyPressedHandler(USHORT vKey, bool down);
    delegate void MouseMovedHandler(int x, int y, int dx, int dy);
    delegate void MouseButtonChangedHandler(int x, int y, bool down);
    delegate void MouseWheelChangedHandler(int x, int y, int delta);
    KeyboardKeyPressedHandler^ KeyboardKeyPressed;
    MouseMovedHandler^ MouseMoved;
    MouseButtonChangedHandler^ MouseButtonChanged;
    MouseWheelChangedHandler^ MouseWheelChanged;

    delegate void LURCycleHandler(GraphicsDevice^ graphicsDevice);
    delegate void VisibilityChangedHandler(bool isVisible);
    LURCycleHandler^ LURCycle;
    VisibilityChangedHandler^ VisibilityChanged;

    delegate void ResizeHandler(SizeU size);
    ResizeHandler^ Resized;

    void DisplayChange();
    void ProcessEvents();
    void Quit();

    Win32Window();
    ~Win32Window();
    !Win32Window();
};

} } }